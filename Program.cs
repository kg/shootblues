using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using Squared.Task;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace ShootBlues {
    public class ProcessInfo {
        public Process Process;
        public RPCChannel Channel;
        public string Status;

        public override string ToString () {
            return String.Format("{0} - {1}", Process.Id, Status);
        }
    }

    public static class Program {
        private static StatusWindow StatusWindowInstance = null;
        private static ContextMenuStrip TrayMenu = null;
        private static int ExitCode = 0;

        public static readonly Signal RunningProcessesChanged = new Signal();
        public static readonly Signal ScriptsChanged = new Signal();
        public static readonly HashSet<ProcessInfo> RunningProcesses = new HashSet<ProcessInfo>();
        public static readonly HashSet<string> Scripts = new HashSet<string>();
        public static readonly Dictionary<string, IManagedScript> ManagedScripts = new Dictionary<string, IManagedScript>();
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
            if (StatusWindowInstance != null) {
                StatusWindowInstance.Activate();
                StatusWindowInstance.Focus();
                yield break;
            }

            using (StatusWindowInstance = new StatusWindow(Scheduler)) {
                foreach (var instance in ManagedScripts.Values)
                    yield return instance.OnStatusWindowShown(StatusWindowInstance);

                yield return StatusWindowInstance.Show();
            }

            StatusWindowInstance = null;
        }

        private static IEnumerator<object> ManagedScriptLoaderTask () {
            while (true) {
                yield return ScriptsChanged.Wait();

                foreach (var script in Scripts)
                    if (!ManagedScripts.ContainsKey(script))
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

            var pi = new ProcessInfo {
                Process = process,
                Status = "Injecting payload"
            };
            var processExit = new SignalFuture();
            process.Exited += (s, e) => {
                processExit.Complete();
                process.EnableRaisingEvents = false;
            };
            process.EnableRaisingEvents = true;

            using (pi.Channel = new RPCChannel(process)) {
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
                        yield return SendScriptFile(pi, script);

                    yield return ReloadModules(pi);

                    pi.Status = "Scripts loaded";
                    RunningProcessesChanged.Set();

                    var fRpcTask = Scheduler.Start(RPCTask(pi), TaskExecutionPolicy.RunWhileFutureLives);

                    using (fRpcTask)
                        yield return payloadResult;

                    pi.Status = String.Format("Payload terminated with exit code {0}.", payloadResult.Result);
                    RunningProcessesChanged.Set();
                }
            }

            yield return processExit;

            RunningProcesses.Remove(pi);
            RunningProcessesChanged.Set();
        }

        public static IEnumerator<object> LoadManagedScript (string scriptFilename) {
            var fAssembly = Future.RunInThread(() =>
                Assembly.LoadFile(scriptFilename)
            );
            yield return fAssembly;

            var fTypes = Future.RunInThread(() => fAssembly.Result.GetTypes());
            yield return fTypes;

            var managedScript = typeof(IManagedScript);
            foreach (var type in fTypes.Result) {
                if (!managedScript.IsAssignableFrom(type))
                    continue;

                var constructor = type.GetConstructor(new Type[0]);
                var instance = constructor.Invoke(null) as IManagedScript;

                ManagedScripts[scriptFilename] = instance;

                if (StatusWindowInstance != null)
                    yield return instance.OnStatusWindowShown(StatusWindowInstance);

                break;
            }
        }

        public static IEnumerator<object> SendScriptText (ProcessInfo pi, string moduleName, string scriptText) {
            yield return Future.RunInThread(() =>
                pi.Channel.Send(new RPCMessage {
                    Type = RPCMessageType.AddModule,
                    ModuleName = moduleName,
                    Text = scriptText
                })
            );
        }

        public static IEnumerator<object> SendScriptFile (ProcessInfo pi, string scriptFilename) {
            if (Path.GetExtension(scriptFilename).ToLower() == ".py") {
                var fScript = Future.RunInThread(() =>
                    File.ReadAllText(scriptFilename)
                );
                yield return fScript;

                var moduleName = Path.GetFileNameWithoutExtension(scriptFilename);
                yield return SendScriptText(pi, moduleName, fScript.Result);

            } else {
                IManagedScript instance;
                if (!ManagedScripts.TryGetValue(scriptFilename, out instance)) {
                    yield return LoadManagedScript(scriptFilename);
                    instance = ManagedScripts[scriptFilename];
                }

                yield return instance.LoadInto(pi);
            }
        }

        public static IEnumerator<object> UnloadScriptByModuleName (ProcessInfo pi, string moduleName) {
            yield return Future.RunInThread(() =>
                pi.Channel.Send(new RPCMessage {
                    Type = RPCMessageType.RemoveModule,
                    ModuleName = moduleName
                })
            );
        }

        public static IEnumerator<object> UnloadScriptByFilename (ProcessInfo pi, string scriptFilename) {
            if (Path.GetExtension(scriptFilename).ToLower() == ".py") {
                yield return UnloadScriptByModuleName(
                    pi, 
                    Path.GetFileNameWithoutExtension(scriptFilename)
                );
            } else {
                var instance = ManagedScripts[scriptFilename];

                yield return instance.UnloadFrom(pi);
            }
        }

        public static IEnumerator<object> UnloadDeadManagedScripts () {
            var keys = new string[ManagedScripts.Count];
            ManagedScripts.Keys.CopyTo(keys, 0);

            foreach (var key in keys) {
                if (!Scripts.Contains(key)) {
                    if (StatusWindowInstance != null)
                        yield return ManagedScripts[key].OnStatusWindowHidden(StatusWindowInstance);

                    ManagedScripts[key].Dispose();
                    ManagedScripts.Remove(key);
                }
            }
        }

        public static IEnumerator<object> ReloadModules (ProcessInfo pi) {
            yield return UnloadDeadManagedScripts();

            yield return Future.RunInThread(() =>
                pi.Channel.Send(new RPCMessage {
                    Type = RPCMessageType.ReloadModules
                }));
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
