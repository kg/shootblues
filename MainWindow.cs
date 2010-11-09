using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShootBlues {
    public partial class MainWindow : Form {
        public MainWindow () {
            InitializeComponent();

            Text = Text.Replace("$version", String.Format("v{0}", Application.ProductVersion));
        }
    }
}
