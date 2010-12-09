using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.Diagnostics;
using System.Windows.Forms;

namespace ShootBlues {
    public class SimpleExecutableProfile : DependencyManager, IProfile {
        public readonly string ExecutableName;
        protected ProcessWatcher Watcher;

        public SimpleExecutableProfile (string executableName) {
            ExecutableName = executableName;

            Name = new Filename(this.GetType().Assembly.Location).Name;

            Watcher = new ProcessWatcher(Program.Scheduler, executableName);
        }

        public virtual string ProfileName {
            get {
                return ExecutableName;
            }
        }

        public virtual IEnumerator<object> Run () {
            while (Watcher != null) {
                var fNewProcess = Watcher.NewProcesses.Dequeue();
                yield return fNewProcess;

                var process = fNewProcess.Result;

                try {
                    if (process.HasExited)
                        continue;
                } catch (Exception ex) {
                    Program.ShowErrorMessage(String.Format("Access denied to process: {0}", ex), process);
                    continue;
                }

                yield return new Start(
                    OnNewProcess(fNewProcess.Result), TaskExecutionPolicy.RunAsBackgroundTask
                );
            }
        }

        public virtual IEnumerator<object> WaitUntilProcessReady (ProcessInfo process) {
            yield break;
        }

        protected virtual IEnumerator<object> OnNewProcess (Process process) {
            yield return Program.NotifyNewProcess(process);
        }

        public virtual void Dispose () {
            if (Watcher != null) {
                Watcher.Dispose();
                Watcher = null;
            }
        }
    }
}
