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
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.TabImageList = new System.Windows.Forms.ImageList(this.components);
            this.RunningProcessGroupBox.SuspendLayout();
            this.Tabs.SuspendLayout();
            this.ScriptsPage.SuspendLayout();
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
            this.RunningProcessGroupBox.Size = new System.Drawing.Size(433, 90);
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
            this.RunningProcessList.Size = new System.Drawing.Size(421, 64);
            this.RunningProcessList.TabIndex = 0;
            // 
            // Tabs
            // 
            this.Tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Tabs.Controls.Add(this.ScriptsPage);
            this.Tabs.ImageList = this.TabImageList;
            this.Tabs.Location = new System.Drawing.Point(2, 98);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(435, 315);
            this.Tabs.TabIndex = 1;
            // 
            // ScriptsPage
            // 
            this.ScriptsPage.Controls.Add(this.button3);
            this.ScriptsPage.Controls.Add(this.button2);
            this.ScriptsPage.Controls.Add(this.button1);
            this.ScriptsPage.Controls.Add(this.listView1);
            this.ScriptsPage.ImageKey = "script.png";
            this.ScriptsPage.Location = new System.Drawing.Point(4, 25);
            this.ScriptsPage.Name = "ScriptsPage";
            this.ScriptsPage.Padding = new System.Windows.Forms.Padding(3);
            this.ScriptsPage.Size = new System.Drawing.Size(427, 286);
            this.ScriptsPage.TabIndex = 0;
            this.ScriptsPage.Text = "Scripts";
            this.ScriptsPage.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Enabled = false;
            this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
            this.button3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button3.Location = new System.Drawing.Point(320, 252);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 27);
            this.button3.TabIndex = 3;
            this.button3.Text = "&Reload All";
            this.button3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.Location = new System.Drawing.Point(137, 252);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(125, 27);
            this.button2.TabIndex = 2;
            this.button2.Text = "&Unload Script";
            this.button2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(6, 252);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(125, 27);
            this.button1.TabIndex = 1;
            this.button1.Text = "&Load Script...";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Location = new System.Drawing.Point(6, 6);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(414, 240);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // TabImageList
            // 
            this.TabImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TabImageList.ImageStream")));
            this.TabImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.TabImageList.Images.SetKeyName(0, "script.png");
            // 
            // StatusWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(437, 414);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.RunningProcessGroupBox);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "StatusWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shoot Blues $version";
            this.RunningProcessGroupBox.ResumeLayout(false);
            this.Tabs.ResumeLayout(false);
            this.ScriptsPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox RunningProcessGroupBox;
        private System.Windows.Forms.ListBox RunningProcessList;
        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage ScriptsPage;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ImageList TabImageList;
    }
}

