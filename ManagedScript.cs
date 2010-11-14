using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShootBlues {
    public abstract class ManagedScript : IManagedScript {
        protected HashSet<ScriptName> _Dependencies = new HashSet<ScriptName>();

        public ScriptName Name {
            get;
            private set;
        }

        public ManagedScript (ScriptName name) {
            Name = name;
        }

        public IEnumerable<ScriptName> Dependencies {
            get { return _Dependencies; }
        }

        protected void AddDependency (string name) {
            _Dependencies.Add(new ScriptName(name, Name.DefaultSearchPath));
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

        public virtual void Dispose () {
        }
    }
}
