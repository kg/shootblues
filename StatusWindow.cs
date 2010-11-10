using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShootBlues {
    public partial class StatusWindow : Form {
        public StatusWindow () {
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

        private void RunPythonMenu_Click (object sender, EventArgs e) {
            var process = (ProcessInfo)ProcessMenu.Tag;

            using (var dialog = new EnterPythonDialog())
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    process.Channel.Send(Encoding.ASCII.GetBytes(dialog.PythonText.Text));
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
    }
}
