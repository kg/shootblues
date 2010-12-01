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
        IEnumerable<ScriptName> OptionalDependencies {
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

    public interface IConfigurationPanel {
        IEnumerator<object> LoadConfiguration ();
        IEnumerator<object> SaveConfiguration ();
    }

    public interface IStatusWindow : ITaskOwner {
        void ShowConfigurationPanel (string name, IConfigurationPanel panel);
        void HideConfigurationPanel (string name);
    }

    public interface IProfile : IDisposable {
        string ProfileName {
            get;
        }
        IEnumerable<ScriptName> Dependencies {
            get;
        }

        IEnumerator<object> WaitUntilProcessReady (ProcessInfo process);
        IEnumerator<object> Run ();
    }
}
