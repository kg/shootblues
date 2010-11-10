using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Diagnostics;
using Squared.Task;

namespace ShootBlues {
    public class ProcessEventArgs : EventArgs {
        public int ProcessId;
        public Process Process;
    }

    public class ProcessWatcher : IDisposable {
        public BlockingQueue<ProcessEventArgs> Events = new BlockingQueue<ProcessEventArgs>();

        ManagementEventWatcher Watcher;

        public ProcessWatcher (string processName) {
            var query = new WqlEventQuery(String.Format(
                @"SELECT * FROM Win32_ProcessStartTrace WHERE ProcessName = '{0}'",
                processName.Replace("\\", "\\\\").Replace("'", "\\'")
            ));
            Watcher = new ManagementEventWatcher(query);
            Watcher.Options.BlockSize = 1;
            Watcher.EventArrived += new EventArrivedEventHandler(OnEventArrived);
            Watcher.Start();
        }

        void OnEventArrived (object sender, EventArrivedEventArgs e) {
            var evt = e.NewEvent;
            int pid = Convert.ToInt32(evt.GetPropertyValue("ProcessID"));

            Process process = null;
            process = Process.GetProcessById(pid);

            Events.Enqueue(new ProcessEventArgs { Process = process, ProcessId = pid });
        }

        public void Dispose () {
            if (Watcher != null) {
                Watcher.Stop();
                Watcher.Dispose();
                Watcher = null;
            }
        }
    }
}
