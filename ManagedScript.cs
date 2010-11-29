using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.Web.Script.Serialization;
using Squared.Task.Data.Mapper;
using Squared.Util.Event;
using Squared.Task.Data;

namespace ShootBlues {
    [Mapper]
    class PrefEntry {
        [Column("prefName")]
        public string Key {
            get; set;
        }
        [Column("value")]
        public object Value {
            get; set;
        }
    }

    public abstract class ManagedScript : IManagedScript {
        protected HashSet<ScriptName> _Dependencies = new HashSet<ScriptName>();
        protected HashSet<ScriptName> _OptionalDependencies = new HashSet<ScriptName>();
        protected EventSubscription _PreferencesChangedEvt;        
        private PreferenceStore _Preferences = null;

        public ScriptName Name {
            get;
            private set;
        }

        protected ConnectionWrapper Database {
            get {
                return Program.Database;
            }
        }

        protected TaskScheduler Scheduler {
            get {
                return Program.Scheduler;
            }
        }

        protected EventBus EventBus {
            get {
                return Program.EventBus;
            }
        }

        protected IProfile Profile {
            get {
                return Program.Profile;
            }
        }

        public PreferenceStore Preferences {
            get {
                if (_Preferences == null) {
                    _Preferences = new PreferenceStore(this, Database, EventBus);
                    _PreferencesChangedEvt = EventBus.Subscribe<string[]>(_Preferences, "Changed", Scheduler, OnPreferencesChanged);
                }
                return _Preferences;
            }
        }

        public ManagedScript (ScriptName name) {
            Name = name;
        }

        public IEnumerable<ScriptName> Dependencies {
            get { return _Dependencies; }
        }

        public IEnumerable<ScriptName> OptionalDependencies {
            get { return _OptionalDependencies; }
        }

        protected void AddDependency (string name, bool optional) {
            var sn = new ScriptName(name, Name.DefaultSearchPath);
            if (optional)
                _OptionalDependencies.Add(sn);
            else
                _Dependencies.Add(sn);
        }

        protected void AddDependency (string name) {
            AddDependency(name, false);
        }

        protected IEnumerator<object> CallFunction (string moduleName, string functionName, params object[] arguments) {
            Console.WriteLine("{0}.{1}", moduleName, functionName);
            foreach (var process in Program.GetProcessesRunningScript(this))
                yield return Program.CallFunction(process, moduleName, functionName, arguments);
        }

        public virtual IEnumerator<object> Reload () {
            yield break;
        }

        public virtual IEnumerator<object> LoadInto (ProcessInfo process) {
            yield break;
        }

        public virtual IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield break;
        }

        public virtual IEnumerator<object> UnloadFrom (ProcessInfo process) {
            yield break;
        }

        public virtual IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            yield break;
        }

        public virtual IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            yield break;
        }

        protected virtual IEnumerator<object> OnPreferencesChanged (EventInfo evt, string[] prefNames) {
            yield break;
        }

        public virtual IEnumerator<object> Initialize () {
            yield break;
        }

        public virtual void Dispose () {
            _PreferencesChangedEvt.Dispose();
        }
    }
}
