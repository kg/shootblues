namespace ShootBlues {
    partial class EnterPythonDialog {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnterPythonDialog));
            this.PythonText = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton_ = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PythonText
            // 
            this.PythonText.AcceptsReturn = true;
            this.PythonText.AcceptsTab = true;
            this.PythonText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PythonText.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PythonText.Location = new System.Drawing.Point(12, 12);
            this.PythonText.Multiline = true;
            this.PythonText.Name = "PythonText";
            this.PythonText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.PythonText.Size = new System.Drawing.Size(423, 170);
            this.PythonText.TabIndex = 0;
            this.PythonText.WordWrap = false;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.OKButton.Location = new System.Drawing.Point(229, 188);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(100, 25);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // CancelButton_
            // 
            this.CancelButton_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton_.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton_.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.CancelButton_.Location = new System.Drawing.Point(335, 188);
            this.CancelButton_.Name = "CancelButton_";
            this.CancelButton_.Size = new System.Drawing.Size(100, 25);
            this.CancelButton_.TabIndex = 2;
            this.CancelButton_.Text = "Cancel";
            this.CancelButton_.UseVisualStyleBackColor = true;
            // 
            // EnterPythonDialog
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(447, 225);
            this.Controls.Add(this.CancelButton_);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.PythonText);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EnterPythonDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter Python";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton_;
        public System.Windows.Forms.TextBox PythonText;
    }
}