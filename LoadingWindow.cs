using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShootBlues {
    public partial class LoadingWindow : Form {
        public LoadingWindow () {
            InitializeComponent();
        }

        private void LoadingWindow_FormClosing (object sender, FormClosingEventArgs e) {
        }

        public void SetProgress (float? progress) {
            if (progress.HasValue) {
                ProgressBar.Style = ProgressBarStyle.Continuous;
                ProgressBar.Value = (int)Math.Floor(progress.Value * ProgressBar.Maximum);
            } else {
                ProgressBar.Style = ProgressBarStyle.Marquee;
            }
        }

        public void SetStatus (string statusText, float? progress) {
            StatusText.Text = statusText;
            SetProgress(progress);

            this.Refresh();
            Application.DoEvents();
        }
    }
}
