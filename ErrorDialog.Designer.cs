namespace ShootBlues {
    partial class ErrorDialog {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorDialog));
            this.ErrorText = new System.Windows.Forms.TextBox();
            this.NextError = new System.Windows.Forms.Button();
            this.PreviousError = new System.Windows.Forms.Button();
            this.ClearErrors = new System.Windows.Forms.Button();
            this.ToolTips = new System.Windows.Forms.ToolTip(this.components);
            this.ErrorTitle = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ErrorText
            // 
            this.ErrorText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ErrorText.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorText.Location = new System.Drawing.Point(13, 47);
            this.ErrorText.Margin = new System.Windows.Forms.Padding(4);
            this.ErrorText.Multiline = true;
            this.ErrorText.Name = "ErrorText";
            this.ErrorText.ReadOnly = true;
            this.ErrorText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ErrorText.Size = new System.Drawing.Size(469, 230);
            this.ErrorText.TabIndex = 1;
            this.ToolTips.SetToolTip(this.ErrorText, "Error Text");
            this.ErrorText.WordWrap = false;
            // 
            // NextError
            // 
            this.NextError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextError.Enabled = false;
            this.NextError.Image = ((System.Drawing.Image)(resources.GetObject("NextError.Image")));
            this.NextError.Location = new System.Drawing.Point(456, 284);
            this.NextError.Name = "NextError";
            this.NextError.Size = new System.Drawing.Size(26, 26);
            this.NextError.TabIndex = 4;
            this.ToolTips.SetToolTip(this.NextError, "Next Error");
            this.NextError.UseVisualStyleBackColor = true;
            this.NextError.Click += new System.EventHandler(this.NextError_Click);
            // 
            // PreviousError
            // 
            this.PreviousError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviousError.Enabled = false;
            this.PreviousError.Image = ((System.Drawing.Image)(resources.GetObject("PreviousError.Image")));
            this.PreviousError.Location = new System.Drawing.Point(424, 284);
            this.PreviousError.Name = "PreviousError";
            this.PreviousError.Size = new System.Drawing.Size(26, 26);
            this.PreviousError.TabIndex = 3;
            this.ToolTips.SetToolTip(this.PreviousError, "Previous Error");
            this.PreviousError.UseVisualStyleBackColor = true;
            this.PreviousError.Click += new System.EventHandler(this.PreviousError_Click);
            // 
            // ClearErrors
            // 
            this.ClearErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ClearErrors.Image = ((System.Drawing.Image)(resources.GetObject("ClearErrors.Image")));
            this.ClearErrors.Location = new System.Drawing.Point(12, 284);
            this.ClearErrors.Name = "ClearErrors";
            this.ClearErrors.Size = new System.Drawing.Size(26, 26);
            this.ClearErrors.TabIndex = 2;
            this.ToolTips.SetToolTip(this.ClearErrors, "Clear All Errors");
            this.ClearErrors.UseVisualStyleBackColor = true;
            this.ClearErrors.Click += new System.EventHandler(this.ClearErrors_Click);
            // 
            // ErrorTitle
            // 
            this.ErrorTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ErrorTitle.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorTitle.Location = new System.Drawing.Point(13, 13);
            this.ErrorTitle.Margin = new System.Windows.Forms.Padding(4);
            this.ErrorTitle.Name = "ErrorTitle";
            this.ErrorTitle.ReadOnly = true;
            this.ErrorTitle.Size = new System.Drawing.Size(469, 26);
            this.ErrorTitle.TabIndex = 0;
            this.ToolTips.SetToolTip(this.ErrorTitle, "Error Title");
            this.ErrorTitle.WordWrap = false;
            // 
            // ErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 322);
            this.Controls.Add(this.ErrorTitle);
            this.Controls.Add(this.ClearErrors);
            this.Controls.Add(this.PreviousError);
            this.Controls.Add(this.NextError);
            this.Controls.Add(this.ErrorText);
            this.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(250, 200);
            this.Name = "ErrorDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shoot Blues - Errors";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ErrorText;
        private System.Windows.Forms.Button NextError;
        private System.Windows.Forms.Button PreviousError;
        private System.Windows.Forms.Button ClearErrors;
        private System.Windows.Forms.ToolTip ToolTips;
        private System.Windows.Forms.TextBox ErrorTitle;
    }
}