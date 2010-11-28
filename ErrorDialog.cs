using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Squared.Task;

namespace ShootBlues {
    public partial class ErrorDialog : TaskForm {
        public readonly List<ErrorInfo> Errors = new List<ErrorInfo>();
        public int CurrentError = 0;

        public ErrorDialog (TaskScheduler scheduler)
            : base (scheduler) {
            InitializeComponent();
        }

        public void AddError (string errorText, string errorTitle) {
            Errors.Add(new ErrorInfo {
                Text = Regex.Replace(errorText, "(?<!\r)\n", "\r\n"),
                Title = errorTitle
            });
            SetCurrentError(Errors.Count - 1);
        }

        public void SetCurrentError (int index) {
            if (index < 0)
                index = 0;
            if (index >= Errors.Count)
                index = Errors.Count - 1;
            CurrentError = index;

            var error = Errors[index];
            ErrorTitle.Text = error.Title;
            ErrorText.Text = error.Text;

            PreviousError.Enabled = (index > 0);
            NextError.Enabled = (index < Errors.Count - 1);

            Text = String.Format("Shoot Blues - Errors ({0} of {1})", index + 1, Errors.Count);
        }

        private void NextError_Click (object sender, EventArgs e) {
            SetCurrentError(CurrentError + 1);
        }

        private void PreviousError_Click (object sender, EventArgs e) {
            SetCurrentError(CurrentError - 1);
        }

        private void ClearErrors_Click (object sender, EventArgs e) {
            Errors.Clear();
            CurrentError = 0;
            ErrorTitle.Text = "";
            ErrorText.Text = "";
            Text = "Shoot Blues - Errors";

            PreviousError.Enabled = NextError.Enabled = false;
        }
    }

    public struct ErrorInfo {
        public string Title;
        public string Text;
    }
}
