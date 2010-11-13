using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using Squared.Task;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace ShootBlues {
    public class Filename {
        public readonly string FullPath;

        private Filename (string filename) {
            FullPath = filename;
        }

        public string Extension {
            get {
                return Path.GetExtension(FullPath);
            }
        }

        public string Name {
            get {
                return Path.GetFileName(FullPath);
            }
        }

        public string NameWithoutExtension {
            get {
                return Path.GetFileNameWithoutExtension(FullPath);
            }
        }

        public string Directory {
            get {
                return Path.GetDirectoryName(FullPath);
            }
        }

        public static implicit operator Filename (string filename) {
            return new Filename(Path.GetFullPath(filename));
        }

        public static implicit operator string (Filename filename) {
            return filename.FullPath;
        }

        public override int GetHashCode () {
            return FullPath.GetHashCode();
        }

        public override string ToString () {
            return FullPath;
        }

        public override bool Equals (object obj) {
            var fn = obj as Filename;
            if (fn != null)
                return FullPath.Equals(fn.FullPath);
            else {
                var str = obj as String;
                if (str != null)
                    return FullPath.Equals(str);
                else
                    return base.Equals(obj);
            }
        }
    }

    public class ProcessInfo : IDisposable {
        public Process Process = null;
        public RPCChannel Channel = null;
        public string Status = "Unknown";

        public HashSet<Filename> LoadedScripts = new HashSet<Filename>();

        private Dictionary<string, RPCResponseChannel> NamedChannels = new Dictionary<string, RPCResponseChannel>();
        private HashSet<IFuture> OwnedFutures = new HashSet<IFuture>();

        public ProcessInfo (Process process) {
            Process = process;
            Channel = new RPCChannel(process);
        }

        public RPCResponseChannel GetNamedChannel (string name) {
            RPCResponseChannel result;
            if (!NamedChannels.TryGetValue(name, out result))
                NamedChannels[name] = result = new RPCResponseChannel(Process);

            return result;
        }

        public IFuture Start (ISchedulable schedulable) {
            var f = Program.Scheduler.Start(schedulable, TaskExecutionPolicy.RunWhileFutureLives);
            OwnedFutures.Add(f);
            return f;
        }

        public IFuture Start (IEnumerator<object> task) {
            var f = Program.Scheduler.Start(task, TaskExecutionPolicy.RunWhileFutureLives);
            OwnedFutures.Add(f);
            return f;
        }

        public void Dispose () {
            foreach (var f in OwnedFutures)
                f.Dispose();
            OwnedFutures.Clear();

            foreach (var nc in NamedChannels.Values)
                nc.Dispose();
            NamedChannels.Clear();

            Channel.Dispose();
        }

        public override string ToString () {
            return String.Format("{0} - {1}", Process.Id, Status);
        }
    }

    public static class Program {
        private static StatusWindow StatusWindowInstance = null;
        private static ContextMenuStrip TrayMenu = null;
        private static Dictionary<string, SignalFuture> LoadingScripts = new Dictionary<string, SignalFuture>();
        private static int ExitCode = 0;

        public static readonly Signal RunningProcessesChanged = new Signal();
        public static readonly Signal ScriptsChanged = new Signal();
        public static readonly HashSet<ProcessInfo> RunningProcesses = new HashSet<ProcessInfo>();
        public static readonly HashSet<Filename> Scripts = new HashSet<Filename>();
        public static readonly Dictionary<Filename, IManagedScript> ManagedScripts = new Dictionary<Filename, IManagedScript>();
        public static TaskScheduler Scheduler;

        [STAThread]
        private static void Main () {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (Scheduler = new TaskScheduler(JobQueue.WindowsMessageBased)) {
                Scheduler.ErrorHandler = OnTaskError;

                using (var fMainTask = Scheduler.Start(MainTask(), TaskExecutionPolicy.RunAsBackgroundTask)) {
                    fMainTask.RegisterOnComplete((_) => {
                        if (_.Failed) {
                            ExitCode = 1;
                            Application.Exit();
                        }
                    });

                    Application.Run();
                    fMainTask.Dispose();
                }
            }

            Environment.Exit(ExitCode);
        }

        private static bool OnTaskError (Exception error) {
            MessageBox.Show(error.ToString(), "Error in background task");

            return true;
        }

        private static void AddItem (this ContextMenuStrip menu, string text, EventHandler onClick) {
            var newItem = menu.Items.Add(text);
            newItem.Click += onClick;
        }

        private static IEnumerator<object> MainTask () {
            TrayMenu = new ContextMenuStrip();
            TrayMenu.AddItem("&Status", (s, e) => Scheduler.Start(ShowStatusWindow(), TaskExecutionPolicy.RunAsBackgroundTask));
            TrayMenu.Items.Add("-");
            TrayMenu.AddItem("E&xit", (s, e) => Application.Exit());

            using (TrayMenu)
            using (var trayIcon = new NotifyIcon {
                Text = "Shoot Blues v" + Application.ProductVersion,
                Icon = Properties.Resources.icon,
                Visible = true,
                ContextMenuStrip = TrayMenu
            })
            using (var pw = new ProcessWatcher("python.exe")) {
                trayIcon.DoubleClick += (s, e) => Scheduler.Start(ShowStatusWindow(), TaskExecutionPolicy.RunAsBackgroundTask);

                yield return new Start(
                    ManagedScriptLoaderTask(), 
                    TaskExecutionPolicy.RunAsBackgroundTask
                );

                Process newProcess = null;
                while (true) {
                    yield return pw.NewProcesses.Dequeue().Bind(() => newProcess);

                    yield return new Start(
                        ProcessTask(newProcess), TaskExecutionPolicy.RunAsBackgroundTask
                    );
                }
            }
        }

        public static void AddCustomMenu (ToolStripMenuItem menu) {
            if (TrayMenu.Items.Count == 3)
                TrayMenu.Items.Insert(1, new ToolStripSeparator());
            TrayMenu.Items.Insert(TrayMenu.Items.Count - 2, menu);
        }

        public static void RemoveCustomMenu (ToolStripMenuItem menu) {
            TrayMenu.Items.Remove(menu);
            if (TrayMenu.Items.Count == 4)
                TrayMenu.Items.RemoveAt(1);
        }

        public static IEnumerator<object> ShowStatusWindow () {
            return ShowStatusWindow(null);
        }

        public static IEnumerator<object> ShowStatusWindow (string initialPage) {
            if (StatusWindowInstance != null) {
                StatusWindowInstance.Activate();
                StatusWindowInstance.Focus();

                if (initialPage != null)
                    try {
                        StatusWindowInstance.Tabs.SelectedTab = StatusWindowInstance.Tabs.TabPages[initialPage];
                    } catch {
                    }

                yield break;
            }

            using (StatusWindowInstance = new StatusWindow(Scheduler)) {
                foreach (var instance in ManagedScripts.Values)
                    yield return instance.OnStatusWindowShown(StatusWindowInstance);

                if (initialPage != null)
                    try {
                        StatusWindowInstance.Tabs.SelectedTab = StatusWindowInstance.Tabs.TabPages[initialPage];
                    } catch {
                    }

                yield return StatusWindowInstance.Show();
            }

            StatusWindowInstance = null;
        }

        private static IEnumerator<object> ManagedScriptLoaderTask () {
            while (true) {
                yield return ScriptsChanged.Wait();

                foreach (var script in Scripts)
                    if ((script.Extension == ".dll") && !ManagedScripts.ContainsKey(script))
                        yield return LoadManagedScript(script);

                yield return UnloadDeadManagedScripts();
            }
        }

        private static IEnumerator<object> ProcessTask (Process process) {
            var payload = Future.RunInThread(() => {
                using (var payloadStream = Assembly.GetExecutingAssembly().
                    GetManifestResourceStream("ShootBlues.payload.dll")) {
                    return new PortableExecutable(payloadStream);
                }
            });

            yield return payload;

            Console.WriteLine("Injecting payload into process {0}...", process.Id);

            var processExit = new SignalFuture();
            process.Exited += (s, e) => {
                processExit.Complete();
                process.EnableRaisingEvents = false;
            };
            process.EnableRaisingEvents = true;

            using (var pi = new ProcessInfo(process)) {
                var payloadResult = new Future<Int32>();
                var threadId = new Future<UInt32>();

                RunningProcesses.Add(pi);
                RunningProcessesChanged.Set();

                var fCodeRegion = Future.RunInThread(() =>
                    ProcessInjector.Inject(process, payload.Result, pi.Channel.Handle, payloadResult, threadId)
                );
                yield return fCodeRegion;

                pi.Channel.RemoteThreadId = threadId.Result;

                using (fCodeRegion.Result) {
                    pi.Status = "Payload injected";
                    RunningProcessesChanged.Set();

                    yield return pi.Channel.Receive();
                    pi.Status = "Loading scripts...";
                    RunningProcessesChanged.Set();

                    foreach (var script in Scripts)
                        yield return LoadScriptFromFilename(pi, script);

                    yield return ReloadModules(pi);

                    pi.Status = "Scripts loaded";
                    RunningProcessesChanged.Set();

                    var fRpcTask = Scheduler.Start(RPCTask(pi), TaskExecutionPolicy.RunWhileFutureLives);

                    using (fRpcTask)
                        yield return payloadResult;

                    pi.Status = String.Format("Payload terminated with exit code {0}.", payloadResult.Result);
                    RunningProcessesChanged.Set();
                }

                yield return processExit;
                RunningProcesses.Remove(pi);
            }

            RunningProcessesChanged.Set();
        }

        public static IEnumerator<object> LoadManagedScript (Filename script) {
            IManagedScript instance;
            SignalFuture loadFuture;
            if (LoadingScripts.TryGetValue(script, out loadFuture)) {
                yield return loadFuture;
                yield break;
            } else if (ManagedScripts.TryGetValue(script, out instance)) {
                yield break;
            } else {
                LoadingScripts[script] = loadFuture = new SignalFuture();
            }

            var fAssembly = Future.RunInThread(() =>
                Assembly.LoadFile(script)
            );
            yield return fAssembly;

            var fTypes = Future.RunInThread(() => fAssembly.Result.GetTypes());
            yield return fTypes;

            var managedScript = typeof(IManagedScript);
            foreach (var type in fTypes.Result) {
                if (!managedScript.IsAssignableFrom(type))
                    continue;

                var constructor = type.GetConstructor(new Type[0]);
                instance = constructor.Invoke(null) as IManagedScript;

                ManagedScripts[script] = instance;
                LoadingScripts.Remove(script);
                loadFuture.Complete();

                if (StatusWindowInstance != null)
                    yield return instance.OnStatusWindowShown(StatusWindowInstance);

                break;
            }
        }

        public static IEnumerator<object> LoadScriptFromString (ProcessInfo pi, string moduleName, string scriptText) {
            yield return Future.RunInThread(() =>
                pi.Channel.Send(new RPCMessage {
                    Type = RPCMessageType.AddModule,
                    ModuleName = moduleName,
                    Text = scriptText
                })
            );
        }

        public static IEnumerator<object> LoadScriptFromFilename (ProcessInfo pi, Filename script) {
            if (pi.LoadedScripts.Contains(script))
                yield break;

            if (script.Extension == ".py") {
                var fScript = Future.RunInThread(() =>
                    File.ReadAllText(script)
                );
                yield return fScript;

                var moduleName = script.NameWithoutExtension;
                pi.LoadedScripts.Add(script);
                yield return LoadScriptFromString(pi, moduleName, fScript.Result);

            } else {
                IManagedScript instance;
                if (!ManagedScripts.TryGetValue(script, out instance)) {
                    yield return LoadManagedScript(script);
                    instance = ManagedScripts[script];
                }

                pi.LoadedScripts.Add(script);
                yield return instance.LoadInto(pi);
            }
        }

        public static IEnumerator<object> UnloadScriptFromModuleName (ProcessInfo pi, string moduleName) {
            yield return Future.RunInThread(() =>
                pi.Channel.Send(new RPCMessage {
                    Type = RPCMessageType.RemoveModule,
                    ModuleName = moduleName
                })
            );
        }

        public static IEnumerator<object> UnloadScriptFromFilename (ProcessInfo pi, Filename script) {
            if (script.Extension == ".py") {
                yield return UnloadScriptFromModuleName(
                    pi, 
                    script.NameWithoutExtension
                );
            } else {
                if (pi.LoadedScripts.Contains(script)) {
                    var instance = ManagedScripts[script];

                    pi.LoadedScripts.Remove(script);
                    yield return instance.UnloadFrom(pi);
                }
            }
        }

        public static IEnumerator<object> UnloadDeadManagedScripts () {
            var keys = new Filename[ManagedScripts.Count];
            ManagedScripts.Keys.CopyTo(keys, 0);

            foreach (var key in keys) {
                bool inUse = Scripts.Contains(key);

                if (!inUse)
                    foreach (var pi in RunningProcesses)
                        if (pi.LoadedScripts.Contains(key))
                            inUse = true;

                if (!inUse) {
                    if (StatusWindowInstance != null)
                        yield return ManagedScripts[key].OnStatusWindowHidden(StatusWindowInstance);

                    ManagedScripts[key].Dispose();
                    ManagedScripts.Remove(key);
                }
            }
        }

        public static Future<byte[]> EvalPython (ProcessInfo process, string pythonText) {
            var messageID = process.Channel.GetMessageID();
            var fResult = process.Channel.WaitForMessage(messageID);

            if (pythonText.Contains("\n") || pythonText.Contains("return "))
                pythonText = "  " + pythonText.Replace("\t", "  ").Replace("\n", "\n  ");
            else
                pythonText = "  return " + pythonText;

            pythonText = String.Format(
                @"def __eval__():
{0}
result = __eval__()
if result:
  result = repr(result)
from shootblues import rpcSend
rpcSend(result, id={1}L)", pythonText, messageID
            );

            Future.RunInThread(() =>
                process.Channel.Send(new RPCMessage {
                    Type = RPCMessageType.Run,
                    Text = pythonText
                })
            );

            return fResult;
        }

        public static Future<byte[]> CallFunction (ProcessInfo process, string moduleName, string functionName, string arguments) {
            return process.Channel.Send(new RPCMessage {
                Type = RPCMessageType.CallFunction,
                ModuleName = moduleName,
                FunctionName = functionName,
                Text = arguments
            }, true);
        }

        public static IEnumerator<object> ReloadModules (ProcessInfo pi) {
            yield return UnloadDeadManagedScripts();

            yield return Future.RunInThread(() =>
                pi.Channel.Send(new RPCMessage {
                    Type = RPCMessageType.ReloadModules
                }));

            foreach (var instance in ManagedScripts.Values)
                yield return instance.LoadedInto(pi);
        }

        private static IEnumerator<object> RPCTask (ProcessInfo pi) {
            while (true) {
                var fMessage = pi.Channel.Receive();
                yield return fMessage;

                var errorText = Encoding.ASCII.GetString(fMessage.Result);
                MessageBox.Show(errorText, String.Format("Error in process {0}", pi.Process.Id));
            }
        }
    }
}
