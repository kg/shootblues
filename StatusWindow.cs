using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;
using System.IO;

namespace ShootBlues {
    public partial class StatusWindow : TaskForm {
        public StatusWindow (TaskScheduler scheduler)
            : base(scheduler) {
            InitializeComponent();

            Text = Text.Replace("$version", String.Format("v{0}", Application.ProductVersion));
        }

        public IEnumerator<object> ShowProcessList () {
            while (true) {
                RunningProcessList.BeginUpdate();
                RunningProcessList.Items.Clear();
                foreach (var pi in Program.RunningProcesses)
                    RunningProcessList.Items.Add(pi);
                RunningProcessList.EndUpdate();

                yield return Program.RunningProcessesChanged.Wait();
            }
        }

        public IEnumerator<object> ShowScriptList () {
            while (true) {
                ScriptsList.BeginUpdate();
                ScriptsList.Items.Clear();
                foreach (var script in Program.Scripts)
                    ScriptsList.Items.Add(script);
                ScriptsList.EndUpdate();

                yield return Program.ScriptsChanged.Wait();
            }
        }

        private void RunPythonMenu_Click (object sender, EventArgs e) {
            var process = (ProcessInfo)ProcessMenu.Tag;

            using (var dialog = new EnterPythonDialog())
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    process.Channel.Send(new RPCMessage {
                        Type = RPCMessageType.Run,
                        Text = dialog.PythonText.Text
                    });
        }

        private void RunningProcessList_MouseDown (object sender, MouseEventArgs e) {
            var index = RunningProcessList.IndexFromPoint(e.X, e.Y);
            if (index == ListBox.NoMatches)
                return;

            RunningProcessList.SelectedIndex = index;

            if (e.Button == MouseButtons.Right) {
                ProcessMenu.Tag = RunningProcessList.Items[index];
                ProcessMenu.Show(RunningProcessList, new Point(e.X, e.Y));
            }
        }

        private void LoadScriptButton_Click (object sender, EventArgs e) {
            using (var dialog = new OpenFileDialog()) {
                dialog.Title = "Load Script";
                dialog.Filter = "Python Scripts|*.py";
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                Program.Scripts.Add(dialog.FileName);
                Program.ScriptsChanged.Set();
            }
        }

        private void StatusWindow_Shown (object sender, EventArgs e) {
            Start(ShowProcessList());
            Start(ShowScriptList());
        }

        private void ScriptsList_DragOver (object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void ScriptsList_DragDrop (object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop)) {
                string[] files = e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop) as string[];
                if (files == null)
                    return;

                foreach (var file in files) {
                    if (Path.GetExtension(file).ToLower() != ".py")
                        continue;

                    Program.Scripts.Add(file);
                }

                Program.ScriptsChanged.Set();
            }
        }
    }
}
