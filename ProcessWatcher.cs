using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Diagnostics;
using Squared.Task;
using System.IO;

namespace ShootBlues {
    public class ProcessWatcher : IDisposable {
        public readonly bool WatcherEnabled = false;
        public readonly TaskScheduler Scheduler;
        public BlockingQueue<Process> NewProcesses = new BlockingQueue<Process>();

        ManagementEventWatcher Watcher = null;
        IFuture TimerTask = null;
        readonly HashSet<int> RunningProcessIds = new HashSet<int>();
        readonly HashSet<string> ProcessNames = new HashSet<string>();

        public ProcessWatcher (TaskScheduler scheduler, params string[] processNames) {
            Scheduler = scheduler;

            foreach (var pname in processNames) {
                var invariant = Path.GetFileNameWithoutExtension(pname).ToLowerInvariant();
                ProcessNames.Add(invariant);

                foreach (var process in Process.GetProcessesByName(invariant)) {
                    RunningProcessIds.Add(process.Id);
                    NewProcesses.Enqueue(process);
                }
            }

            try {
                var query = new WqlEventQuery(@"SELECT * FROM Win32_ProcessStartTrace");
                Watcher = new ManagementEventWatcher(query);
                Watcher.Options.BlockSize = 1;
                Watcher.EventArrived += new EventArrivedEventHandler(OnEventArrived);
                Watcher.Start();
                WatcherEnabled = true;
            } catch {
                Watcher = null;
                WatcherEnabled = false;

                TimerTask = Scheduler.Start(InitTimer(), TaskExecutionPolicy.RunAsBackgroundTask);
            }
        }

        protected IEnumerator<object> InitTimer () {
            var sleep = new Sleep(5.0);

            while (true) {
                yield return sleep;

                foreach (var pname in ProcessNames) {
                    foreach (var process in Process.GetProcessesByName(pname)) {
                        if (!RunningProcessIds.Contains(process.Id)) {
                            RunningProcessIds.Add(process.Id);
                            NewProcesses.Enqueue(process);
                        }
                    }
                }
            }
        }

        void OnEventArrived (object sender, EventArrivedEventArgs e) {
            var evt = e.NewEvent;

            var pname = evt.GetPropertyValue("ProcessName") as string;
            if (pname == null)
                return;

            pname = Path.GetFileNameWithoutExtension(pname).ToLowerInvariant();
            if (!ProcessNames.Contains(pname))
                return;

            int pid = Convert.ToInt32(evt.GetPropertyValue("ProcessID"));

            Process process = null;
            process = Process.GetProcessById(pid);

            NewProcesses.Enqueue(process);
        }

        public void Dispose () {
            if (Watcher != null) {
                Watcher.Stop();
                Watcher.Dispose();
                Watcher = null;
            }

            if (TimerTask != null) {
                TimerTask.Dispose();
                TimerTask = null;
            }
        }
    }
}
