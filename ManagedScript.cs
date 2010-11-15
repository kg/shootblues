using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.Web.Script.Serialization;

namespace ShootBlues {
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

        public void SetPreference (string name, object value) {
            _Preferences[name] = value;
            _PreferencesChanged.Set();
        }

        public bool GetPreference (string name, out object value) {
            return _Preferences.TryGetValue(name, out value);
        }

        public string GetPreferencesJson () {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(_Preferences);
        }

        public virtual IEnumerator<object> Initialize () {
            yield break;
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
