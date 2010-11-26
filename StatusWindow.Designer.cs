namespace ShootBlues {
    partial class StatusWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatusWindow));
            this.RunningProcessGroupBox = new System.Windows.Forms.GroupBox();
            this.RunningProcessList = new System.Windows.Forms.ListBox();
            this.ButtonPanel = new System.Windows.Forms.Panel();
            this.UnloadScriptButton = new System.Windows.Forms.Button();
            this.LoadScriptButton = new System.Windows.Forms.Button();
            this.ReloadAllButton = new System.Windows.Forms.Button();
            this.ScriptsList = new System.Windows.Forms.TreeView();
            this.ScriptImageList = new System.Windows.Forms.ImageList(this.components);
            this.ProcessMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.EvalPythonMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.LoadScriptMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.TabList = new System.Windows.Forms.ListBox();
            this.ScriptsPanel = new System.Windows.Forms.Panel();
            this.RunningProcessGroupBox.SuspendLayout();
            this.ButtonPanel.SuspendLayout();
            this.ProcessMenu.SuspendLayout();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.ScriptsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // RunningProcessGroupBox
            // 
            this.RunningProcessGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RunningProcessGroupBox.Controls.Add(this.RunningProcessList);
            this.RunningProcessGroupBox.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.RunningProcessGroupBox.Location = new System.Drawing.Point(2, 2);
            this.RunningProcessGroupBox.Name = "RunningProcessGroupBox";
            this.RunningProcessGroupBox.Size = new System.Drawing.Size(580, 90);
            this.RunningProcessGroupBox.TabIndex = 0;
            this.RunningProcessGroupBox.TabStop = false;
            this.RunningProcessGroupBox.Text = "Running Processes";
            // 
            // RunningProcessList
            // 
            this.RunningProcessList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RunningProcessList.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.RunningProcessList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RunningProcessList.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RunningProcessList.FormattingEnabled = true;
            this.RunningProcessList.ItemHeight = 16;
            this.RunningProcessList.Location = new System.Drawing.Point(6, 19);
            this.RunningProcessList.Name = "RunningProcessList";
            this.RunningProcessList.Size = new System.Drawing.Size(568, 64);
            this.RunningProcessList.TabIndex = 0;
            this.RunningProcessList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RunningProcessList_MouseDown);
            // 
            // ButtonPanel
            // 
            this.ButtonPanel.Controls.Add(this.UnloadScriptButton);
            this.ButtonPanel.Controls.Add(this.LoadScriptButton);
            this.ButtonPanel.Controls.Add(this.ReloadAllButton);
            this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonPanel.Location = new System.Drawing.Point(0, 335);
            this.ButtonPanel.Name = "ButtonPanel";
            this.ButtonPanel.Size = new System.Drawing.Size(383, 27);
            this.ButtonPanel.TabIndex = 5;
            // 
            // UnloadScriptButton
            // 
            this.UnloadScriptButton.Enabled = false;
            this.UnloadScriptButton.Image = ((System.Drawing.Image)(resources.GetObject("UnloadScriptButton.Image")));
            this.UnloadScriptButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UnloadScriptButton.Location = new System.Drawing.Point(131, 0);
            this.UnloadScriptButton.Name = "UnloadScriptButton";
            this.UnloadScriptButton.Size = new System.Drawing.Size(125, 27);
            this.UnloadScriptButton.TabIndex = 1;
            this.UnloadScriptButton.Text = "&Unload Script";
            this.UnloadScriptButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.UnloadScriptButton.UseVisualStyleBackColor = true;
            this.UnloadScriptButton.Click += new System.EventHandler(this.UnloadScriptButton_Click);
            // 
            // LoadScriptButton
            // 
            this.LoadScriptButton.Image = ((System.Drawing.Image)(resources.GetObject("LoadScriptButton.Image")));
            this.LoadScriptButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadScriptButton.Location = new System.Drawing.Point(0, 0);
            this.LoadScriptButton.Name = "LoadScriptButton";
            this.LoadScriptButton.Size = new System.Drawing.Size(125, 27);
            this.LoadScriptButton.TabIndex = 0;
            this.LoadScriptButton.Text = "&Load Script...";
            this.LoadScriptButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.LoadScriptButton.UseVisualStyleBackColor = true;
            this.LoadScriptButton.Click += new System.EventHandler(this.LoadScriptButton_Click);
            // 
            // ReloadAllButton
            // 
            this.ReloadAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ReloadAllButton.Image = ((System.Drawing.Image)(resources.GetObject("ReloadAllButton.Image")));
            this.ReloadAllButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ReloadAllButton.Location = new System.Drawing.Point(283, 0);
            this.ReloadAllButton.Name = "ReloadAllButton";
            this.ReloadAllButton.Size = new System.Drawing.Size(100, 27);
            this.ReloadAllButton.TabIndex = 2;
            this.ReloadAllButton.Text = "&Reload All";
            this.ReloadAllButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ReloadAllButton.UseVisualStyleBackColor = true;
            this.ReloadAllButton.Click += new System.EventHandler(this.ReloadAllButton_Click);
            // 
            // ScriptsList
            // 
            this.ScriptsList.AllowDrop = true;
            this.ScriptsList.Dock = System.Windows.Forms.DockStyle.Top;
            this.ScriptsList.FullRowSelect = true;
            this.ScriptsList.HideSelection = false;
            this.ScriptsList.ImageIndex = 0;
            this.ScriptsList.ImageList = this.ScriptImageList;
            this.ScriptsList.Location = new System.Drawing.Point(0, 0);
            this.ScriptsList.Name = "ScriptsList";
            this.ScriptsList.SelectedImageIndex = 0;
            this.ScriptsList.ShowLines = false;
            this.ScriptsList.ShowNodeToolTips = true;
            this.ScriptsList.Size = new System.Drawing.Size(383, 329);
            this.ScriptsList.TabIndex = 0;
            this.ScriptsList.DragDrop += new System.Windows.Forms.DragEventHandler(this.ScriptsList_DragDrop);
            this.ScriptsList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ScriptsList_AfterSelect);
            this.ScriptsList.DragOver += new System.Windows.Forms.DragEventHandler(this.ScriptsList_DragOver);
            // 
            // ScriptImageList
            // 
            this.ScriptImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ScriptImageList.ImageStream")));
            this.ScriptImageList.TransparentColor = System.Drawing.Color.Fuchsia;
            this.ScriptImageList.Images.SetKeyName(0, "missing");
            this.ScriptImageList.Images.SetKeyName(1, "optional");
            // 
            // ProcessMenu
            // 
            this.ProcessMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EvalPythonMenu});
            this.ProcessMenu.Name = "ProcessMenu";
            this.ProcessMenu.Size = new System.Drawing.Size(146, 26);
            // 
            // EvalPythonMenu
            // 
            this.EvalPythonMenu.Name = "EvalPythonMenu";
            this.EvalPythonMenu.Size = new System.Drawing.Size(145, 22);
            this.EvalPythonMenu.Text = "&Eval Python...";
            this.EvalPythonMenu.Click += new System.EventHandler(this.RunPythonMenu_Click);
            // 
            // LoadScriptMenu
            // 
            this.LoadScriptMenu.Name = "LoadScriptMenu";
            this.LoadScriptMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // SplitContainer
            // 
            this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SplitContainer.Location = new System.Drawing.Point(2, 98);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.TabList);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.ScriptsPanel);
            this.SplitContainer.Size = new System.Drawing.Size(580, 362);
            this.SplitContainer.SplitterDistance = 193;
            this.SplitContainer.TabIndex = 1;
            // 
            // TabList
            // 
            this.TabList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabList.Font = new System.Drawing.Font("MS Reference Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabList.FormattingEnabled = true;
            this.TabList.IntegralHeight = false;
            this.TabList.ItemHeight = 16;
            this.TabList.Location = new System.Drawing.Point(0, 0);
            this.TabList.Name = "TabList";
            this.TabList.Size = new System.Drawing.Size(193, 362);
            this.TabList.TabIndex = 0;
            this.TabList.SelectedIndexChanged += new System.EventHandler(this.TabList_SelectedIndexChanged);
            // 
            // ScriptsPanel
            // 
            this.ScriptsPanel.Controls.Add(this.ScriptsList);
            this.ScriptsPanel.Controls.Add(this.ButtonPanel);
            this.ScriptsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScriptsPanel.Location = new System.Drawing.Point(0, 0);
            this.ScriptsPanel.Name = "ScriptsPanel";
            this.ScriptsPanel.Size = new System.Drawing.Size(383, 362);
            this.ScriptsPanel.TabIndex = 4;
            this.ScriptsPanel.Resize += new System.EventHandler(this.ScriptsPage_SizeChanged);
            // 
            // StatusWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(584, 462);
            this.Controls.Add(this.RunningProcessGroupBox);
            this.Controls.Add(this.SplitContainer);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(401, 300);
            this.Name = "StatusWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shoot Blues $version - $profile";
            this.Shown += new System.EventHandler(this.StatusWindow_Shown);
            this.RunningProcessGroupBox.ResumeLayout(false);
            this.ButtonPanel.ResumeLayout(false);
            this.ProcessMenu.ResumeLayout(false);
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            this.SplitContainer.ResumeLayout(false);
            this.ScriptsPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox RunningProcessGroupBox;
        private System.Windows.Forms.ListBox RunningProcessList;
        private System.Windows.Forms.ContextMenuStrip ProcessMenu;
        private System.Windows.Forms.ToolStripMenuItem EvalPythonMenu;
        private System.Windows.Forms.ContextMenuStrip LoadScriptMenu;
        private System.Windows.Forms.TreeView ScriptsList;
        private System.Windows.Forms.ImageList ScriptImageList;
        private System.Windows.Forms.Panel ButtonPanel;
        private System.Windows.Forms.Button UnloadScriptButton;
        private System.Windows.Forms.Button LoadScriptButton;
        private System.Windows.Forms.Button ReloadAllButton;
        private System.Windows.Forms.SplitContainer SplitContainer;
        private System.Windows.Forms.ListBox TabList;
        private System.Windows.Forms.Panel ScriptsPanel;
    }
}

