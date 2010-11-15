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
            this.Tabs = new System.Windows.Forms.TabControl();
            this.ScriptsPage = new System.Windows.Forms.TabPage();
            this.ScriptsList = new System.Windows.Forms.TreeView();
            this.ScriptImageList = new System.Windows.Forms.ImageList(this.components);
            this.ReloadAllButton = new System.Windows.Forms.Button();
            this.UnloadScriptButton = new System.Windows.Forms.Button();
            this.LoadScriptButton = new System.Windows.Forms.Button();
            this.ImageList = new System.Windows.Forms.ImageList(this.components);
            this.ProcessMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.EvalPythonMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.LoadScriptMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RunningProcessGroupBox.SuspendLayout();
            this.Tabs.SuspendLayout();
            this.ScriptsPage.SuspendLayout();
            this.ProcessMenu.SuspendLayout();
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
            this.RunningProcessGroupBox.Size = new System.Drawing.Size(480, 90);
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
            this.RunningProcessList.Size = new System.Drawing.Size(468, 64);
            this.RunningProcessList.TabIndex = 0;
            this.RunningProcessList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RunningProcessList_MouseDown);
            // 
            // Tabs
            // 
            this.Tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Tabs.Controls.Add(this.ScriptsPage);
            this.Tabs.ImageList = this.ImageList;
            this.Tabs.Location = new System.Drawing.Point(2, 98);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(482, 313);
            this.Tabs.TabIndex = 1;
            // 
            // ScriptsPage
            // 
            this.ScriptsPage.Controls.Add(this.ScriptsList);
            this.ScriptsPage.Controls.Add(this.ReloadAllButton);
            this.ScriptsPage.Controls.Add(this.UnloadScriptButton);
            this.ScriptsPage.Controls.Add(this.LoadScriptButton);
            this.ScriptsPage.ImageKey = "script.png";
            this.ScriptsPage.Location = new System.Drawing.Point(4, 25);
            this.ScriptsPage.Name = "ScriptsPage";
            this.ScriptsPage.Padding = new System.Windows.Forms.Padding(3);
            this.ScriptsPage.Size = new System.Drawing.Size(474, 284);
            this.ScriptsPage.TabIndex = 0;
            this.ScriptsPage.Text = "Scripts";
            this.ScriptsPage.UseVisualStyleBackColor = true;
            // 
            // ScriptsList
            // 
            this.ScriptsList.AllowDrop = true;
            this.ScriptsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ScriptsList.FullRowSelect = true;
            this.ScriptsList.HideSelection = false;
            this.ScriptsList.ImageIndex = 0;
            this.ScriptsList.ImageList = this.ScriptImageList;
            this.ScriptsList.Location = new System.Drawing.Point(6, 6);
            this.ScriptsList.Name = "ScriptsList";
            this.ScriptsList.SelectedImageIndex = 0;
            this.ScriptsList.ShowNodeToolTips = true;
            this.ScriptsList.ShowPlusMinus = false;
            this.ScriptsList.ShowRootLines = false;
            this.ScriptsList.Size = new System.Drawing.Size(462, 237);
            this.ScriptsList.TabIndex = 4;
            this.ScriptsList.DragDrop += new System.Windows.Forms.DragEventHandler(this.ScriptsList_DragDrop);
            this.ScriptsList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ScriptsList_AfterSelect);
            this.ScriptsList.DragOver += new System.Windows.Forms.DragEventHandler(this.ScriptsList_DragOver);
            // 
            // ScriptImageList
            // 
            this.ScriptImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.ScriptImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.ScriptImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ReloadAllButton
            // 
            this.ReloadAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ReloadAllButton.Image = ((System.Drawing.Image)(resources.GetObject("ReloadAllButton.Image")));
            this.ReloadAllButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ReloadAllButton.Location = new System.Drawing.Point(366, 249);
            this.ReloadAllButton.Name = "ReloadAllButton";
            this.ReloadAllButton.Size = new System.Drawing.Size(100, 27);
            this.ReloadAllButton.TabIndex = 3;
            this.ReloadAllButton.Text = "&Reload All";
            this.ReloadAllButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ReloadAllButton.UseVisualStyleBackColor = true;
            this.ReloadAllButton.Click += new System.EventHandler(this.ReloadAllButton_Click);
            // 
            // UnloadScriptButton
            // 
            this.UnloadScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.UnloadScriptButton.Enabled = false;
            this.UnloadScriptButton.Image = ((System.Drawing.Image)(resources.GetObject("UnloadScriptButton.Image")));
            this.UnloadScriptButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UnloadScriptButton.Location = new System.Drawing.Point(137, 248);
            this.UnloadScriptButton.Name = "UnloadScriptButton";
            this.UnloadScriptButton.Size = new System.Drawing.Size(125, 27);
            this.UnloadScriptButton.TabIndex = 2;
            this.UnloadScriptButton.Text = "&Unload Script";
            this.UnloadScriptButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.UnloadScriptButton.UseVisualStyleBackColor = true;
            this.UnloadScriptButton.Click += new System.EventHandler(this.UnloadScriptButton_Click);
            // 
            // LoadScriptButton
            // 
            this.LoadScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LoadScriptButton.Image = ((System.Drawing.Image)(resources.GetObject("LoadScriptButton.Image")));
            this.LoadScriptButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadScriptButton.Location = new System.Drawing.Point(6, 249);
            this.LoadScriptButton.Name = "LoadScriptButton";
            this.LoadScriptButton.Size = new System.Drawing.Size(125, 27);
            this.LoadScriptButton.TabIndex = 1;
            this.LoadScriptButton.Text = "&Load Script...";
            this.LoadScriptButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.LoadScriptButton.UseVisualStyleBackColor = true;
            this.LoadScriptButton.Click += new System.EventHandler(this.LoadScriptButton_Click);
            // 
            // ImageList
            // 
            this.ImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList.ImageStream")));
            this.ImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList.Images.SetKeyName(0, "script.png");
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
            // StatusWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(484, 412);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.RunningProcessGroupBox);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(401, 300);
            this.Name = "StatusWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shoot Blues $version - $profile";
            this.Shown += new System.EventHandler(this.StatusWindow_Shown);
            this.RunningProcessGroupBox.ResumeLayout(false);
            this.Tabs.ResumeLayout(false);
            this.ScriptsPage.ResumeLayout(false);
            this.ProcessMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox RunningProcessGroupBox;
        private System.Windows.Forms.ListBox RunningProcessList;
        private System.Windows.Forms.TabPage ScriptsPage;
        private System.Windows.Forms.Button LoadScriptButton;
        private System.Windows.Forms.Button ReloadAllButton;
        private System.Windows.Forms.Button UnloadScriptButton;
        private System.Windows.Forms.ImageList ImageList;
        private System.Windows.Forms.ContextMenuStrip ProcessMenu;
        private System.Windows.Forms.ToolStripMenuItem EvalPythonMenu;
        private System.Windows.Forms.ContextMenuStrip LoadScriptMenu;
        public System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TreeView ScriptsList;
        private System.Windows.Forms.ImageList ScriptImageList;
    }
}

