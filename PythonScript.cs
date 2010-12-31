using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.IO;
using System.Text.RegularExpressions;

namespace ShootBlues {
    public class PythonScript : ManagedScript {
        private string ScriptText = null;
        private bool ProbablyHasLoadMethod = false;
        private Regex DependencyRegex = new Regex(
            @"(Dependency\(['""](?'dependency'[\w\.]+)['""]\)|OptionalDependency\(['""](?'optionaldependency'[\w\.]+)['""]\))", 
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture
        );

        public PythonScript (ScriptName name)
            : base(name) {
        }

        public override IEnumerator<object> Initialize () {
            yield return Reload();
        }

        public override IEnumerator<object> Reload () {
            ScriptText = null;

            var filename = Program.FindScript(Name);

            if (File.Exists(filename.FullPath)) {
                var fText = Future.RunInThread(
                    () => File.ReadAllText(filename.FullPath)
                );
                yield return fText;

                ScriptText = fText.Result;

                ProbablyHasLoadMethod = ScriptText.Contains("__load__");

                yield return ParseDependencies();
            } else {
                throw new FileNotFoundException("Python script not found", filename.Name);
            }
        }

        public IEnumerator<object> ParseDependencies () {
            ClearDependencies();

            MatchCollection matches = null;
            yield return Future.RunInThread(
                () => DependencyRegex.Matches(ScriptText)
            ).Bind(() => matches);

            if ((matches == null) || (matches.Count == 0))
                yield break;

            foreach (Match m in matches) {
                if (m.Groups["dependency"].Success) {
                    AddDependency(m.Groups["dependency"].Value, false);
                } else {
                    AddDependency(m.Groups["optionaldependency"].Value, true);
                }
            }
        }

        public string ModuleName {
            get {
                return Regex.Replace(
                    Name, @"(\.script\.py|\.py)", 
                    "", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
                );
            }
        }

        public override IEnumerator<object> LoadInto (ProcessInfo process) {
            if (ScriptText == null)
                yield return Reload();

            Program.PythonModuleToScript[ModuleName] = Name;

            yield return Program.LoadPythonScript(
                process, ModuleName, ScriptText
            );

            DisposeFuturesForProcess(process);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            if (ProbablyHasLoadMethod)
                yield return Program.CallFunction(process, ModuleName, "__load__");
        }

        public override IEnumerator<object> UnloadFrom (ProcessInfo process) {
            yield return Program.UnloadPythonScript(
                process, ModuleName
            );

            if (Program.PythonModuleToScript.ContainsKey(ModuleName))
                Program.PythonModuleToScript.Remove(ModuleName);

            DisposeFuturesForProcess(process);
        }
    }
}
