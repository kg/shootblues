using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task.Data;
using Squared.Util.Event;
using Squared.Task;
using System.Web.Script.Serialization;

namespace ShootBlues {
    public class PreferenceStore : IDisposable {
        public readonly ConnectionWrapper Database;
        public readonly EventBus EventBus;
        public readonly IManagedScript Script;

        protected readonly HashSet<string> DirtyPrefs = new HashSet<string>();
        protected readonly Signal Dirty = new Signal();
        protected bool _Initialized = false;
        protected IFuture _DirtyTask = null;

        public PreferenceStore (IManagedScript script, ConnectionWrapper database, EventBus eventBus) {
            Script = script;
            Database = database;
            EventBus = eventBus;
            _DirtyTask = Database.Scheduler.Start(
                DirtyTask(), TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        public void Dispose () {
            if (_DirtyTask != null)
                _DirtyTask.Dispose();
        }

        protected IEnumerator<object> DirtyTask () {
            while (true) {
                yield return Dirty.Wait();

                var prefNames = DirtyPrefs.ToArray();
                DirtyPrefs.Clear();

                EventBus.Broadcast(this, "Changed", prefNames);
            }
        }

        public void Flush () {
            Dirty.Set();
        }

        public IEnumerator<object> Set<T> (string prefName, T value) {
            if (!_Initialized)
                yield return Initialize();

            using (var query = Database.BuildQuery(
                "replace into prefs (scriptName, prefName, value) values (?, ?, ?)"
            ))
                yield return query.ExecuteNonQuery(Script.Name, prefName, value);

            DirtyPrefs.Add(prefName);
            Flush();
        }

        public IEnumerator<object> SetMultiple (Dictionary<string, object> toUpdate) {
            if (!_Initialized)
                yield return Initialize();

            using (var xact = Database.CreateTransaction()) {
                yield return xact;

                using (var query = Database.BuildQuery(
                    "replace into prefs (scriptName, prefName, value) values (?, ?, ?)"
                ))
                foreach (var kvp in toUpdate) {
                    yield return query.ExecuteNonQuery(Script.Name, kvp.Key, kvp.Value);
                    DirtyPrefs.Add(kvp.Key);
                }

                yield return xact.Commit();
            }

            Flush();
        }

        public IEnumerator<object> Get<T> (string prefName) {
            if (!_Initialized)
                yield return Initialize();

            using (var query = Database.BuildQuery(
                "select value from prefs where scriptName = ? and prefName = ?"
            )) {
                var fResult = query.ExecuteScalar<T>(Script.Name, prefName);
                yield return fResult;
                yield return new Result(fResult.Result);
            }
        }

        public IEnumerator<object> GetAll () {
            if (!_Initialized)
                yield return Initialize();

            var dict = new Dictionary<string, object>();

            using (var query = Database.BuildQuery(
                "select prefName, value from prefs where scriptName = ?"
            ))
            using (var e = query.Execute<PrefEntry>(Script.Name))
                while (!e.Disposed) {
                    yield return e.Fetch();

                    foreach (var item in e)
                        dict.Add(item.Key, item.Value);
                }

            yield return new Result(dict);
        }

        public IEnumerator<object> GetAllJson () {
            var rtc = new RunToCompletion<Dictionary<string, object>>(GetAll());
            yield return rtc;

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(rtc.Result);
            yield return new Result(json);
        }

        public virtual IEnumerator<object> Initialize () {
            yield return Program.CreateDBTable(
                "prefs", @"( scriptName TEXT NOT NULL, prefName TEXT NOT NULL, value VARIANT, PRIMARY KEY ( scriptName, prefName ) )"
            );

            _Initialized = true;
        }
    }
}
