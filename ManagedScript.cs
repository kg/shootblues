using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.Web.Script.Serialization;
using Squared.Task.Data.Mapper;
using Squared.Util.Event;
using Squared.Task.Data;
using System.Diagnostics;
using System.IO;

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

    public class DependencyManager {
        protected HashSet<ScriptName> _Dependencies = new HashSet<ScriptName>();
        protected HashSet<ScriptName> _OptionalDependencies = new HashSet<ScriptName>();

        public ScriptName Name {
            get;
            protected set;
        }

        public IEnumerable<ScriptName> Dependencies {
            get { return _Dependencies; }
        }

        public IEnumerable<ScriptName> OptionalDependencies {
            get { return _OptionalDependencies; }
        }

        protected void ClearDependencies () {
            _Dependencies.Clear();
            _OptionalDependencies.Clear();
        }

        protected void AddDependency (string name, bool optional) {
            string searchPath = Name.DefaultSearchPath;

            if (Debugger.IsAttached) {
                var myAssembly = this.GetType().Assembly;
                var myAssemblyPath = Path.GetFullPath(Path.GetDirectoryName(myAssembly.Location)).ToLowerInvariant();
                var mySourcePath = Path.GetFullPath(myAssemblyPath.Replace(
                    @"\shootblues\bin", String.Format(
                        @"\shootbluesscripts\{0}", Name.NameWithoutExtension.ToLowerInvariant()
                        .Replace(".script", "")
                    )
                ));
                if (File.Exists(Path.Combine(mySourcePath, name)))
                    searchPath = mySourcePath;
            }

            var sn = new ScriptName(name, searchPath);
            if (optional)
                _OptionalDependencies.Add(sn);
            else
                _Dependencies.Add(sn);
        }

        protected void AddDependency (string name) {
            AddDependency(name, false);
        }
    }

    public abstract class ManagedScript : DependencyManager, IManagedScript {
        protected EventSubscription _PreferencesChangedEvt;
        protected readonly Dictionary<ProcessInfo, OwnedFutureSet> _OwnedFutures = new Dictionary<ProcessInfo, OwnedFutureSet>();
        private PreferenceStore _Preferences = null;

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

        public IFuture Start (ProcessInfo process, ISchedulable task) {
            OwnedFutureSet of = null;
            if (!_OwnedFutures.TryGetValue(process, out of)) {
                of = new OwnedFutureSet();
                _OwnedFutures[process] = of;
            }

            var f = process.Start(task);
            of.Add(f);
            return f;
        }

        public IFuture Start (ProcessInfo process, IEnumerator<object> task) {
            return this.Start(process, new SchedulableGeneratorThunk(task));
        }

        protected void DisposeFuturesForProcess (ProcessInfo process) {
            OwnedFutureSet of = null;
            if (_OwnedFutures.TryGetValue(process, out of)) {
                _OwnedFutures.Remove(process);
                of.Dispose();
            }
        }

        protected IEnumerator<object> CallFunction (string moduleName, string functionName, params object[] arguments) {
            foreach (var process in Program.GetProcessesRunningScript(this))
                yield return Program.CallFunction(process, moduleName, functionName, arguments);
        }

        public virtual IEnumerator<object> Reload () {
            yield break;
        }

        public virtual IEnumerator<object> LoadInto (ProcessInfo process) {
            DisposeFuturesForProcess(process);

            yield break;
        }

        public virtual IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield break;
        }

        public virtual IEnumerator<object> UnloadFrom (ProcessInfo process) {
            DisposeFuturesForProcess(process);

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
