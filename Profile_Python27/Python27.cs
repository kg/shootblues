using System;
using ShootBlues;

namespace ShootBlues.Profile {
    public class Python27 : SimpleExecutableProfile {
        public Python27 ()
            : base("python27.exe") {
        }

        public override string Name {
            get {
                return "Python 2.7";
            }
        }
    }
}
