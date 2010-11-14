using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.IO;

namespace ShootBlues {
    public class PythonScript : ManagedScript {
        public readonly Filename Script;

        private string ScriptText = null;

        public PythonScript (Filename script) 
            : base (script.Name) {

            Script = script;
        }

        public override IEnumerator<object> Initialize () {
            yield return Reload();
        }

        public override IEnumerator<object> Reload () {
            ScriptText = null;

            var fText = Future.RunInThread(
                () => File.ReadAllText(Script.FullPath)
            );
            yield return fText;

            ScriptText = fText.Result;
        }

        public override IEnumerator<object> LoadInto (ProcessInfo process) {
            if (ScriptText == null)
                yield return Reload();

            yield return Program.LoadPythonScript(
                process, Script.NameWithoutExtension, ScriptText
            );
        }

        public override IEnumerator<object> UnloadFrom (ProcessInfo process) {
            yield return Program.UnloadPythonScript(
                process, Script.NameWithoutExtension
            );
        }
    }
}
