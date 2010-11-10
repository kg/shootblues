using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using Squared.Task;
using System.Diagnostics;

namespace ShootBlues {
    static class Program {
        public static TaskScheduler Scheduler;
        public static int ExitCode = 0;

        [STAThread]
        static void Main () {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            using (Scheduler = new TaskScheduler(JobQueue.WindowsMessageBased)) {
                Scheduler.ErrorHandler = OnTaskError;

                Scheduler.Start(MainTask(), TaskExecutionPolicy.RunAsBackgroundTask)
                    .RegisterOnComplete((_) => {
                        if (_.Failed)
                            ExitCode = 1;
                    
                        Application.Exit(); 
                    });

                Application.Run();
            }

            Environment.Exit(ExitCode);
        }

        public static bool OnTaskError (Exception error) {
            Console.WriteLine("Error in background task: {0}", error.ToString());

            return true;
        }

        public static IEnumerator<object> MainTask () {
            using (var trayIcon = new NotifyIcon {
                Text = "Shoot Blues v" + Application.ProductVersion,
                Icon = Properties.Resources.icon,
                Visible = true
            })
            using (var pw = new ProcessWatcher("python.exe")) {
                ProcessEventArgs evt = null;
                while (true) {
                    yield return pw.Events.Dequeue().Bind(() => evt);

                    yield return new Start(InjectProcess(evt.Process), TaskExecutionPolicy.RunAsBackgroundTask);
                }
            }
        }

        public static IEnumerator<object> InjectProcess (Process process) {
            var payload = Future.RunInThread(() => {
                using (var payloadStream = Assembly.GetExecutingAssembly().
                    GetManifestResourceStream("ShootBlues.payload.dll")) {
                    return new PortableExecutable(payloadStream);
                }
            });

            yield return payload;
            
            Console.WriteLine("Injecting payload into process {0}...", process.Id);
            var f = new Future<Int32>();
            yield return Future.RunInThread(() => {
                ProcessInjector.Inject(process, payload.Result, f);
            });
            Console.WriteLine("Payload injected.");

            yield return f;
            Console.WriteLine("Payload terminated with exit code {0}.", f.Result);
        }
    }
}
