using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;

namespace ShootBlues {
    public interface IManagedScript : IDisposable {
        IEnumerator<object> LoadInto (ProcessInfo process);
        IEnumerator<object> UnloadFrom (ProcessInfo process);
    }
}
