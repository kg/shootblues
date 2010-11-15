using System;
using ShootBlues;

namespace ShootBlues.Profile {
    public class Python : SimpleExecutableProfile {
        public Python ()
            : base("python.exe") {
        }

        public override string Name {
            get {
                return "Python";
            }
        }
    }
}
