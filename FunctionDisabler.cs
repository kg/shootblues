using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Util;
using Squared.Task;
using System.Diagnostics;

namespace ShootBlues {
    public class KernelFunctionDisabler : IDisposable {
        // xor eax, eax; ret 4
        public static readonly byte[] ReplacementBytes = new byte[] {
            0x33, 0xC0, 0xC2, 0x04, 0x00, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90
        };

        public readonly Dictionary<Pair<string>, byte[]> DisabledFunctions = new Dictionary<Pair<string>, byte[]>();
        public readonly ProcessInfo Process;

        public KernelFunctionDisabler (ProcessInfo process) {
            Process = process;
        }

        public void Dispose () {
            foreach (var key in DisabledFunctions.Keys.ToArray())
                EnableFunction(key.First, key.Second);
        }

        protected IntPtr GetFunctionAddress (string moduleName, string functionName) {
            var hModule = Win32.LoadLibrary(moduleName);
            if (hModule == IntPtr.Zero)
                throw new Exception(String.Format("Module load failed: {0}", moduleName));

            try {
                var procAddress = new IntPtr(Win32.GetProcAddress(hModule, functionName));
                if (procAddress == IntPtr.Zero)
                    throw new Exception(String.Format("Function {1} not exported by module {0}", moduleName, functionName));

                return procAddress;
            } finally {
                Win32.FreeLibrary(hModule);
            }
        }

        protected RemoteMemoryRegion GetFunctionRegion (string moduleName, string functionName) {
            var address = GetFunctionAddress(moduleName, functionName);
            return RemoteMemoryRegion.Existing(
                Process.Process, address, (uint)ReplacementBytes.Length
            );
        }

        protected Finally SuspendProcess () {
            foreach (ProcessThread thread in Process.Process.Threads) {
                var hThread = Win32.OpenThread(ThreadAccessFlags.SuspendResume, false, thread.Id);
                if (hThread != IntPtr.Zero) {
                    Win32.SuspendThread(hThread);
                    Win32.CloseHandle(hThread);
                } else {
                    Console.WriteLine("Could not open thread {0}", thread.Id);
                }
            }

            return Finally.Do(() => {
                foreach (ProcessThread thread in Process.Process.Threads) {
                    var hThread = Win32.OpenThread(ThreadAccessFlags.SuspendResume, false, thread.Id);
                    if (hThread != IntPtr.Zero) {
                        Win32.ResumeThread(hThread);
                        Win32.CloseHandle(hThread);
                    } else {
                        Console.WriteLine("Could not open thread {0}", thread.Id);
                    }
                }
            });
        }

        public unsafe void DisableFunction (string moduleName, string functionName) {
            var key = new Pair<string>(moduleName, functionName);
            if (DisabledFunctions.ContainsKey(key))
                return;

            if (!Process.IsAlive)
                return;

            var region = GetFunctionRegion(moduleName, functionName);
            using (var suspend = SuspendProcess())
            using (var handle = region.OpenHandle(ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite)) {
                var oldProtect = region.Protect(handle, 0, region.Size, MemoryProtection.ReadWrite);
                try {
                    var oldBytes = region.ReadBytes(handle, 0, region.Size);
                    DisabledFunctions[key] = oldBytes;
                    fixed (byte* pReplacement = ReplacementBytes)
                        region.Write(handle, 0, region.Size, pReplacement);
                } finally {
                    region.Protect(handle, 0, region.Size, oldProtect);
                }
            }
        }

        public unsafe void EnableFunction (string moduleName, string functionName) {
            var key = new Pair<string>(moduleName, functionName);
            if (!DisabledFunctions.ContainsKey(key))
                return;

            if (!Process.IsAlive)
                return;

            var region = GetFunctionRegion(moduleName, functionName);
            using (var suspend = SuspendProcess())
            using (var handle = region.OpenHandle(ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite)) {
                var oldProtect = region.Protect(handle, 0, region.Size, MemoryProtection.ReadWrite);
                try {
                    var oldBytes = DisabledFunctions[key];
                    DisabledFunctions.Remove(key);
                    fixed (byte* pOldBytes = oldBytes)
                        region.Write(handle, 0, region.Size, pOldBytes);
                } finally {
                    region.Protect(handle, 0, region.Size, oldProtect);
                }
            }
        }
    }
}
