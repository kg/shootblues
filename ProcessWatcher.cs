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
        public BlockingQueue<Process> NewProcesses = new BlockingQueue<Process>();

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

            foreach (var process in Process.GetProcessesByName(
                Path.GetFileNameWithoutExtension(processName).ToLower()
            ))
                NewProcesses.Enqueue(process);
        }

        void OnEventArrived (object sender, EventArrivedEventArgs e) {
            var evt = e.NewEvent;
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
        }
    }
}
