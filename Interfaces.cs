using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.Windows.Forms;

namespace ShootBlues {
    public interface IManagedScript : IDisposable {
        IEnumerator<object> LoadInto (ProcessInfo process);
        IEnumerator<object> UnloadFrom (ProcessInfo process);
        IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow);
        IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow);
    }

    public interface IStatusWindow : ITaskForm {
        TabPage ShowConfigurationPanel (string name, Control panel);
        void HideConfigurationPanel (TabPage page);
        void HideConfigurationPanel (string name);
    }
}
