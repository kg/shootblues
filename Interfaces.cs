using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.Windows.Forms;

namespace ShootBlues {
    public delegate IEnumerator<object> TableConverterTask (string oldTableName, string newTableName, string oldTableSql, string newTableSql);

    public interface IManagedScript : IDisposable {
        ScriptName Name {
            get;
        }
        IEnumerable<ScriptName> Dependencies {
            get;
        }

        IEnumerator<object> Initialize ();
        IEnumerator<object> Reload ();
        IEnumerator<object> LoadInto (ProcessInfo process);
        IEnumerator<object> LoadedInto (ProcessInfo process);
        IEnumerator<object> UnloadFrom (ProcessInfo process);
        IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow);
        IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow);
    }

    public interface IStatusWindow : ITaskForm {
        TabPage ShowConfigurationPanel (string name, Control panel);
        void HideConfigurationPanel (TabPage page);
        void HideConfigurationPanel (string name);
    }

    public interface IProfile : IDisposable {
        string Name {
            get;
        }

        IEnumerator<object> Run ();
    }
}
