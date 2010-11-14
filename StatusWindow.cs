using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;
using System.IO;

namespace ShootBlues {
    public partial class StatusWindow : TaskForm, IStatusWindow {
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

        private TreeNode BuildScriptNode (ScriptName script) {
            var filename = Program.FindScript(script);

            var item = new TreeNode();
            item.Text = filename ?? script.Name;
            item.Tag = filename;

            if ((filename != null) && (!ScriptImageList.Images.ContainsKey(filename))) {
                Icon icon = Squared.Util.IO.ExtractAssociatedIcon(filename, false);
                if (icon != null) {
                    ScriptImageList.Images.Add(filename, icon);
                }
            }
            item.SelectedImageKey = item.ImageKey = filename;

            IManagedScript instance;
            if (Program.LoadedScripts.TryGetValue(script, out instance)) {
                foreach (var dep in instance.Dependencies)
                    item.Nodes.Add(BuildScriptNode(dep));

                if (item.Nodes.Count > 0)
                    item.Expand();
            }

            return item;
        }

        public IEnumerator<object> ShowScriptList () {
            Filename selectedScript;

            while (true) {
                if (ScriptsList.SelectedNode != null)
                    selectedScript = ScriptsList.SelectedNode.Tag as Filename;
                else
                    selectedScript = null;

                ScriptsList.BeginUpdate();
                ScriptsList.Nodes.Clear();
                ScriptImageList.Images.Clear();

                foreach (var script in Program.Scripts) {
                    var item = BuildScriptNode(script.Name);

                    ScriptsList.Nodes.Add(item);
                    if (script == selectedScript)
                        ScriptsList.SelectedNode = item;
                }

                ScriptsList.EndUpdate();

                UnloadScriptButton.Enabled = (ScriptsList.SelectedNode != null) &&
                    (ScriptsList.SelectedNode.Parent == null);

                yield return Program.ScriptsChanged.Wait();
            }
        }

        private void RunPythonMenu_Click (object sender, EventArgs e) {
            var process = (ProcessInfo)ProcessMenu.Tag;

            using (var dialog = new EnterPythonDialog())
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    Start(DoEval(process, dialog.PythonText.Text));
        }

        private IEnumerator<object> DoEval (ProcessInfo process, string pythonText) {
            var f = Program.EvalPython(process, pythonText);
            yield return f;
            byte[] result = f.Result;
            if ((result != null) && (result.Length > 0))
                MessageBox.Show(result.DecodeAsciiZ(), "Result");
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
                dialog.Filter = "All Scripts|*.dll;*.py|Managed Scripts|*.dll|Python Scripts|*.py";
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                AddScripts(new string[] { dialog.FileName });
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

                AddScripts(
                    from file in files where (
                        (Path.GetExtension(file).ToLower() == ".py") ||
                        (Path.GetExtension(file).ToLower() == ".dll")
                    ) select file
                );
            }
        }

        private void AddScripts (IEnumerable<string> filenames) {
            foreach (var filename in filenames)
                Program.Scripts.Add(filename);
            Program.ScriptsChanged.Set();
        }

        private void ReloadAllButton_Click (object sender, EventArgs e) {
            Start(Program.ReloadAllScripts());
        }

        private void UnloadScriptButton_Click (object sender, EventArgs e) {
            var filename = ScriptsList.SelectedNode.Tag as Filename;
            Program.Scripts.Remove(filename);
            Program.ScriptsChanged.Set();
        }

        public TabPage ShowConfigurationPanel (string title, Control panel) {
            TabPage tabPage = panel as TabPage;
            if (tabPage == null) {
                tabPage = new TabPage();
                tabPage.Controls.Add(panel);
                panel.Dock = DockStyle.Fill;
            }

            tabPage.Text = title;
            tabPage.Name = title;
            Tabs.TabPages.Add(tabPage);
            return tabPage;
        }

        public void HideConfigurationPanel (TabPage page) {
            Tabs.TabPages.Remove(page);
        }

        public void HideConfigurationPanel (string title) {
            Tabs.TabPages.RemoveByKey(title);
        }

        private void ScriptsList_AfterSelect (object sender, TreeViewEventArgs e) {
            UnloadScriptButton.Enabled = (ScriptsList.SelectedNode != null) && 
                (ScriptsList.SelectedNode.Parent == null);
        }
    }
}
