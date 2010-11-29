using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShootBlues;
using Squared.Util.Bind;
using System.IO;
using Squared.Task;

namespace ShootBlues.Script {
    public partial class SimpleConfigPanel<T> : TaskUserControl, IConfigurationPanel
        where T : ManagedScript {

        public IBoundMember[] Prefs;
        public T Script;

        public SimpleConfigPanel (T script)
            : base(Program.Scheduler) {
            Script = script;
            Prefs = new IBoundMember[0];
        }

        public string GetMemberName (IBoundMember member) {
            return ((Control)member.Target).Name;
        }

        public IEnumerator<object> LoadConfiguration () {
            var rtc = new RunToCompletion<Dictionary<string, object>>(Script.Preferences.GetAll());
            yield return rtc;

            var dict = rtc.Result;
            object value;

            foreach (var bm in Prefs)
                if (dict.TryGetValue(GetMemberName(bm), out value))
                    bm.Value = value;
        }

        public IEnumerator<object> SaveConfiguration () {
            var prefsDict = new Dictionary<string, object>();

            foreach (var bm in Prefs)
                prefsDict[GetMemberName(bm)] = bm.Value;

            yield return Script.Preferences.SetMultiple(prefsDict);
        }

        public void ValuesChanged (object sender, EventArgs args) {
            Start(SaveConfiguration());
        }
    }
}
