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
        IConfigurationPanel _ActivePanel = null;

        public StatusWindow (TaskScheduler scheduler)
            : base(scheduler) {
            InitializeComponent();

            Text = Text.Replace(
                "$version", String.Format("v{0}", Application.ProductVersion)
            ).Replace("$profile", Program.Profile.Name);
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

        public IEnumerator<object> ShowScriptList () {
            Filename selectedScript;

            while (true) {
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
                dialog.Filter = "All Scripts|*.script.dll;*.py|Managed Scripts|*.script.dll|Python Scripts|*.py";
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                AddScripts(new string[] { dialog.FileName });
            }
        }

        private void StatusWindow_Shown (object sender, EventArgs e) {
            ScriptsPage_SizeChanged(null, EventArgs.Empty);

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

        public TabPage ShowConfigurationPanel (string title, IConfigurationPanel panel) {
            TabPage tabPage = panel as TabPage;

            if (tabPage == null) {
                var ctl = (Control)panel;
                tabPage = new TabPage();
                tabPage.Controls.Add(ctl);
                ctl.Dock = DockStyle.Fill;
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
                Program.Scripts.Contains(ScriptsList.SelectedNode.Tag as Filename);
        }

        private void ScriptsPage_SizeChanged (object sender, EventArgs e) {
            ScriptsList.Height = ButtonPanel.Top - 4;
        }

        private void Tabs_SelectedIndexChanged (object sender, EventArgs e) {
            var selectedTab = Tabs.SelectedTab;

            IConfigurationPanel configPanel = selectedTab as IConfigurationPanel;
            if ((configPanel == null) && (selectedTab.Controls.Count > 0))
                configPanel = selectedTab.Controls[0] as IConfigurationPanel;

            if (_ActivePanel != null)
                Start(_ActivePanel.SaveConfiguration());
            _ActivePanel = configPanel;
            if (configPanel != null)
                Start(configPanel.LoadConfiguration());
        }
    }
}
