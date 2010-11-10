using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using Squared.Task;
using System.Diagnostics;
using System.Text;

namespace ShootBlues {
    public class ProcessInfo {
        public Process Process;
        public RPCChannel Channel;
        public string Status;

        public override string ToString () {
            return String.Format("{0} - {1}", Process.Id, Status);
        }
    }

    static class Program {
        public static Signal RunningProcessesChanged = new Signal();
        public static HashSet<ProcessInfo> RunningProcesses = new HashSet<ProcessInfo>();
        public static TaskScheduler Scheduler;
        public static int ExitCode = 0;

        [STAThread]
        static void Main () {
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

        public static bool OnTaskError (Exception error) {
            Console.WriteLine("Error in background task: {0}", error.ToString());

            return true;
        }

        public static void AddItem (this ContextMenuStrip menu, string text, EventHandler onClick) {
            var newItem = menu.Items.Add(text);
            newItem.Click += onClick;
        }

        public static IEnumerator<object> MainTask () {
            var trayMenu = new ContextMenuStrip();
            trayMenu.AddItem("&Status", (s, e) => Scheduler.Start(ShowStatusWindow(), TaskExecutionPolicy.RunAsBackgroundTask));
            trayMenu.Items.Add("-");
            trayMenu.AddItem("E&xit", (s, e) => Application.Exit());

            using (trayMenu)
            using (var trayIcon = new NotifyIcon {
                Text = "Shoot Blues v" + Application.ProductVersion,
                Icon = Properties.Resources.icon,
                Visible = true,
                ContextMenuStrip = trayMenu
            })
            using (var pw = new ProcessWatcher("python.exe")) {
                trayIcon.DoubleClick += (s, e) => Scheduler.Start(ShowStatusWindow(), TaskExecutionPolicy.RunAsBackgroundTask);

                ProcessEventArgs evt = null;
                while (true) {
                    yield return pw.Events.Dequeue().Bind(() => evt);

                    yield return new Start(
                        ProcessTask(evt.Process), TaskExecutionPolicy.RunAsBackgroundTask
                    );
                }
            }
        }

        public static IEnumerator<object> ShowStatusWindow () {
            var f = new SignalFuture();
            using (var window = new StatusWindow()) {
                window.FormClosed += (s, e) => f.Complete();

                window.Show();
                using (Scheduler.Start(window.ShowProcessList(), TaskExecutionPolicy.RunWhileFutureLives))
                    yield return f;
            }
        }

        public static IEnumerator<object> ProcessTask (Process process) {
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

        public static IEnumerator<object> RPCTask (ProcessInfo pi) {
            yield return pi.Channel.Receive();
            pi.Status = "RPC channel open";
            RunningProcessesChanged.Set();

            yield return Future.RunInThread(
                () => pi.Channel.Send(Encoding.ASCII.GetBytes("print 'Hello from Shoot Blues!'\0"))
            );

            while (true) {
                var fMessage = pi.Channel.Receive();
                yield return fMessage;

                pi.Status = Encoding.ASCII.GetString(fMessage.Result);
                RunningProcessesChanged.Set();
            }
        }
    }
}
