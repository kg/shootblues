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
        protected Dictionary<string, object> _Preferences = new Dictionary<string, object>();
        protected IFuture _PreferencesTask;
        protected EventSubscription _PreferenceChangedEvt;

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

        public ManagedScript (ScriptName name) {
            Name = name;

            _PreferenceChangedEvt = EventBus.Subscribe<string>(this, "PreferenceChanged", Scheduler, OnPreferenceChanged);
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

        public IEnumerator<object> SetPreference<T> (string prefName, T value) {
            using (var query = Database.BuildQuery(
                "replace into prefs (scriptName, prefName, value) values (?, ?, ?)"
            ))
                yield return query.ExecuteNonQuery(Name, prefName, value);

            EventBus.Broadcast(this, "PreferenceChanged", prefName);
        }

        public Future<T> GetPreference<T> (string prefName) {
            using (var query = Database.BuildQuery(
                "select value from prefs where scriptName = ? and prefName = ?"
            ))
                return query.ExecuteScalar<T>(Name, prefName);
        }

        public IEnumerator<object> GetPreferences () {
            var dict = new Dictionary<string, object>();

            using (var query = Database.BuildQuery(
                "select prefName, value from prefs where scriptName = ?"
            ))
            using (var e = query.Execute<PrefEntry>(Name))
            while (!e.Disposed) {
                yield return e.Fetch();

                foreach (var item in e)
                    dict.Add(item.Key, item.Value);
            }

            yield return new Result(dict);
        }

        public IEnumerator<object> GetPreferencesJson () {
            var rtc = new RunToCompletion<Dictionary<string, object>>(GetPreferences());
            yield return rtc;

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(rtc.Result);
            yield return new Result(json);
        }

        public virtual IEnumerator<object> Initialize () {
            yield return Program.CreateDBTable(
                "prefs", @"( scriptName TEXT NOT NULL, prefName TEXT NOT NULL, value VARIANT, PRIMARY KEY ( scriptName, prefName ) )"
            );
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

        protected virtual IEnumerator<object> OnPreferenceChanged (EventInfo evt, string prefName) {
            yield break;
        }

        public virtual void Dispose () {
            _PreferenceChangedEvt.Dispose();
        }
    }
}
