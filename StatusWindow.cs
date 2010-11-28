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
        Control _ActivePanel = null;
        Dictionary<string, Control> _Panels = new Dictionary<string, Control>();

        public StatusWindow (TaskScheduler scheduler)
            : base(scheduler) {
            InitializeComponent();

            Text = Text.Replace(
                "$version", String.Format("v{0}", Application.ProductVersion)
            ).Replace("$profile", Program.Profile.Name);

            SplitContainer.SplitterDistance = 130;
            SplitContainer.Panel1MinSize = 100;
            SplitContainer.Panel2MinSize = 200;

            _ActivePanel = _Panels["Scripts"] = ScriptsPanel;
            RefreshTabList();

            SubscribeTo(Program.EventBus, Program.Profile, "RunningProcessAdded", (e) => RefreshProcessList());
            SubscribeTo(Program.EventBus, Program.Profile, "RunningProcessRemoved", (e) => RefreshProcessList());
            SubscribeTo(Program.EventBus, Program.Profile, "RunningProcessChanged", (e) => RefreshProcessList());

            SubscribeTo(Program.EventBus, Program.Profile, "ScriptsChanged", (e) => RefreshScriptsList());

            RefreshProcessList();
            RefreshScriptsList();
        }

        protected void RefreshTabList () {
            string oldSelection = (TabList.SelectedItem as string) ?? "Scripts";

            TabList.BeginUpdate();
            TabList.Items.Clear();
            TabList.Items.Add("Scripts");
            TabList.Items.AddRange((
                from k in _Panels.Keys 
                where k != "Scripts" orderby k select k
            ).ToArray());
            try {
                TabList.SelectedItem = oldSelection;
            } catch {
            }
            TabList.EndUpdate();
        }

        public void RefreshProcessList () {
            RunningProcessList.BeginUpdate();
            RunningProcessList.Items.Clear();
            foreach (var pi in Program.RunningProcesses)
                RunningProcessList.Items.Add(pi);
            RunningProcessList.EndUpdate();
        }

        private TreeNode BuildScriptNode (ScriptName script, bool optional, out bool shouldExpand) {
            var filename = Program.FindScript(script);

            var item = new TreeNode();
            string text = filename;
            if (text != null) {
                var appUri = new Uri(Path.GetDirectoryName(Application.ExecutablePath) + "\\");
                var uri = new Uri(filename);
                var relUri = appUri.MakeRelativeUri(uri);
                text = relUri.ToString().Replace("/", "\\");
            } else {
                text = script.Name;
            }
            item.Text = text;
            item.Tag = filename;

            if ((filename != null) && (!ScriptImageList.Images.ContainsKey(filename))) {
                Icon icon = Squared.Util.IO.ExtractAssociatedIcon(filename, false);
                if (icon != null) {
                    ScriptImageList.Images.Add(filename, icon);
                }
            }
            item.SelectedImageKey = item.ImageKey = filename;

            shouldExpand = false;
            bool se = false;

            IManagedScript instance = Program.GetScriptInstance(script);
            if (instance != null) {
                foreach (var dep in instance.Dependencies)
                    item.Nodes.Add(BuildScriptNode(dep, false, out se));

                foreach (var dep in instance.OptionalDependencies)
                    item.Nodes.Add(BuildScriptNode(dep, true, out se));

                if (se) {
                    item.Expand();
                    shouldExpand = true;
                }
            } else {
                if (optional) {
                    item.SelectedImageKey = item.ImageKey = "optional";
                    item.ToolTipText = "Optional script not loaded.";
                } else {
                    item.SelectedImageKey = item.ImageKey = "missing";
                    item.ToolTipText = "Script missing or failed to load!";
                }

                shouldExpand = true;
            }

            return item;
        }

        public void RefreshScriptsList () {
            Filename selectedScript;

            if (ScriptsList.SelectedNode != null)
                selectedScript = ScriptsList.SelectedNode.Tag as Filename;
            else
                selectedScript = null;

            ScriptsList.BeginUpdate();
            ScriptsList.Nodes.Clear();
            while (ScriptImageList.Images.Count > 2)
                ScriptImageList.Images.RemoveAt(2);

            bool temp = false;
            foreach (var script in Program.Scripts) {
                var item = BuildScriptNode(script.Name, false, out temp);

                ScriptsList.Nodes.Add(item);
                if (script == selectedScript)
                    ScriptsList.SelectedNode = item;
            }

            ScriptsList.EndUpdate();

            UnloadScriptButton.Enabled = (ScriptsList.SelectedNode != null) &&
                (ScriptsList.SelectedNode.Parent == null);
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
                MessageBox.Show(this, result.DecodeAsciiZ(), String.Format("Result from process {0}", process.Process.Id));
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
                dialog.Filter = "All Scripts|*.script.dll;*.py|Managed Scripts|*.script.dll|Python Scripts|*.py";
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                AddScripts(new string[] { dialog.FileName });
            }
        }

        private void StatusWindow_Shown (object sender, EventArgs e) {
            ScriptsPage_SizeChanged(null, EventArgs.Empty);
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
            foreach (var filename in filenames) {
                bool alreadyInList = false;
                foreach (var script in Program.Scripts) {
                    if (script.Name.Equals((new Filename(filename)).Name)) {
                        alreadyInList = true;
                        break;
                    }
                }

                if (!alreadyInList)
                    Program.Scripts.Add(filename);
            }

            Program.EventBus.Broadcast(Program.Profile, "OnScriptsAdded", filenames);
        }

        private void ReloadAllButton_Click (object sender, EventArgs e) {
            Start(Program.ReloadAllScripts());
        }

        private void UnloadScriptButton_Click (object sender, EventArgs e) {
            var filename = ScriptsList.SelectedNode.Tag as Filename;
            Program.Scripts.Remove(filename);

            Program.EventBus.Broadcast(Program.Profile, "OnScriptRemoved", filename);
        }

        public void ShowConfigurationPanel (string title, IConfigurationPanel panel) {
            title = String.Intern(title);
            _Panels[title] = (Control)panel;
            RefreshTabList();
        }

        public void HideConfigurationPanel (string title) {
            title = String.Intern(title);
            _Panels.Remove(title);
            RefreshTabList();
        }

        private void ScriptsList_AfterSelect (object sender, TreeViewEventArgs e) {
            UnloadScriptButton.Enabled = (ScriptsList.SelectedNode != null) &&
                Program.Scripts.Contains(ScriptsList.SelectedNode.Tag as Filename);
        }

        private void ScriptsPage_SizeChanged (object sender, EventArgs e) {
            ScriptsList.Height = ButtonPanel.Top - 4;
        }

        public void SelectTab (string name) {
            name = String.Intern(name);
            TabList.SelectedItem = name;
        }

        private void TabList_SelectedIndexChanged (object sender, EventArgs e) {
            var tabName = TabList.SelectedItem as string;

            if (tabName == null)
                return;

            using (new ControlWaitCursor(this)) {
                var panel = _Panels[tabName];
                SplitContainer.Panel2.SuspendLayout();

                if (_ActivePanel != null) {
                    var iface = _ActivePanel as IConfigurationPanel;
                    if (iface != null)
                        Scheduler.WaitFor(iface.SaveConfiguration());

                    SplitContainer.Panel2.Controls.Remove(_ActivePanel);
                    _ActivePanel.Dock = DockStyle.None;
                }

                _ActivePanel = panel;
                {
                    var iface = _ActivePanel as IConfigurationPanel;
                    if (iface != null)
                        Scheduler.WaitFor(iface.LoadConfiguration());
                }

                panel.Dock = DockStyle.Fill;
                SplitContainer.Panel2.Controls.Add(panel);
                SplitContainer.Panel2.ResumeLayout(true);
            }
        }
    }
}
