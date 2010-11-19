using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.Web.Script.Serialization;
using Squared.Task.Data.Mapper;

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
        protected Dictionary<string, object> _Preferences = new Dictionary<string, object>();
        protected Signal _PreferencesChanged = new Signal();
        protected IFuture _PreferencesTask;

        public ScriptName Name {
            get;
            private set;
        }

        public ManagedScript (ScriptName name) {
            Name = name;

            _PreferencesTask = Program.Scheduler.Start(PreferencesTask(), TaskExecutionPolicy.RunAsBackgroundTask);
        }

        public IEnumerable<ScriptName> Dependencies {
            get { return _Dependencies; }
        }

        protected void AddDependency (string name) {
            _Dependencies.Add(new ScriptName(name, Name.DefaultSearchPath));
        }

        public IEnumerator<object> SetPreference<T> (string prefName, T value) {
            using (var query = Program.Database.BuildQuery(
                "replace into prefs (scriptName, prefName, value) values (?, ?, ?)"
            ))
                yield return query.ExecuteNonQuery(Name, prefName, value);

            _PreferencesChanged.Set();
        }

        public Future<T> GetPreference<T> (string prefName) {
            using (var query = Program.Database.BuildQuery(
                "select value from prefs where scriptName = ? and prefName = ?"
            ))
                return query.ExecuteScalar<T>(Name, prefName);
        }

        public IEnumerator<object> GetPreferences () {
            var dict = new Dictionary<string, object>();

            using (var query = Program.Database.BuildQuery(
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

        protected IEnumerator<object> PreferencesTask () {
            while (true) {
                yield return _PreferencesChanged.Wait();

                yield return new Start(
                    OnPreferencesChanged(), TaskExecutionPolicy.RunAsBackgroundTask
                );
            }
        }

        protected virtual IEnumerator<object> OnPreferencesChanged () {
            yield break;
        }

        public virtual void Dispose () {
            if (_PreferencesTask != null) {
                _PreferencesTask.Dispose();
                _PreferencesTask = null;
            }
        }
    }
}
