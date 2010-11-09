using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.IO;
using Squared.Task;

namespace ShootBlues {
    [Flags]
    public enum AllocationType {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Reset = 0x80000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        LargePages = 0x20000000
    }

    [Flags]
    public enum MemoryProtection {
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
        GuardModifierflag = 0x100,
        NoCacheModifierflag = 0x200,
        WriteCombineModifierflag = 0x400
    }

    [Flags]
    enum ProcessAccessFlags : uint {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VMOperation = 0x00000008,
        VMRead = 0x00000010,
        VMWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        Synchronize = 0x00100000
    }

    public class SafeProcessHandle : SafeHandle {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle (IntPtr hObject);

        public SafeProcessHandle (IntPtr handle)
            : base (IntPtr.Zero, true) {
            base.SetHandle(handle);
        }

        public override bool IsInvalid {
            get { return (base.handle == IntPtr.Zero) || (base.handle == new IntPtr(-1)); }
        }

        protected override bool ReleaseHandle () {
            return CloseHandle(base.handle);
        }
    }

    public class ThreadWaitHandle : WaitHandle {
        public ThreadWaitHandle (SafeWaitHandle handle)
            : base() {
            base.SafeWaitHandle = handle;
        }
    }

    public static class ProcessInjector {
        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr LoadLibrary (string lpFileName);
        [DllImport("kernel32", SetLastError = true)]
        static extern bool FreeLibrary (IntPtr hModule);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern UInt32 GetProcAddress (IntPtr hModule, string procName);
        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx (
            IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, AllocationType flAllocationType, 
            MemoryProtection flProtect
        );
        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualProtectEx (
            IntPtr hProcess, UInt32 lpAddress,
            uint dwSize, MemoryProtection flNewProtect,
            out MemoryProtection flOldProtect
        );
        [DllImport("kernel32", SetLastError = true)]
        static extern bool WriteProcessMemory (
            IntPtr hProcess, UInt32 lpBaseAddress, 
            IntPtr lpSrc, uint nSize, 
            out int lpNumberOfBytesWritten
        );
        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr CreateRemoteThread (
            IntPtr hProcess, IntPtr lpThreadAttributes, 
            uint dwStackSize, UInt32 lpStartAddress, IntPtr lpParameter, 
            uint dwCreationFlags, out Int32 lpThreadId
        );
        [DllImport("kernel32", SetLastError=true)]
        static extern IntPtr OpenProcess (
            ProcessAccessFlags dwDesiredAccess, 
            [MarshalAs(UnmanagedType.Bool)] 
            bool bInheritHandle, 
            int dwProcessId
        );
        [DllImport("kernel32", SetLastError = true)]
        static extern Int32 GetLastError ();
        [DllImport("kernel32", SetLastError = true)]
        static extern bool GetExitCodeThread (IntPtr hThread, out Int32 exitCode);

        public static unsafe void Inject (Process process, PortableExecutable executable, Future<Int32> threadResultFuture) {
            using (var handle = new SafeProcessHandle(OpenProcess(ProcessAccessFlags.All, false, process.Id))) {
                var allocPtr = VirtualAllocEx(
                    handle.DangerousGetHandle(), IntPtr.Zero,
                    executable.OptionalHeader.SizeOfImage,
                    AllocationType.Commit | AllocationType.Reserve,
                    MemoryProtection.ReadWrite
                );
                if (allocPtr == IntPtr.Zero) {
                    var error = GetLastError();
                    throw new Exception(String.Format("Allocation failed: Error {0:x8}", error));
                }

                var baseAddress = (UInt32)allocPtr.ToInt64();

                executable.Rebase(baseAddress);

                // Resolve imports (note: this only works for OS libraries)
                foreach (var import in executable.Imports) {
                    var hModule = LoadLibrary(import.ModuleName);
                    if (hModule == IntPtr.Zero)
                        throw new Exception(String.Format("Module load failed: {0}", import.ModuleName));

                    try {
                        var procAddress = GetProcAddress(hModule, import.FunctionName);
                        if (procAddress == 0)
                            throw new Exception(String.Format("Unresolved import: {0}:{1}", import.ModuleName, import.FunctionName));

                        var section = executable.SectionFromVirtualAddress(import.FunctionAddressDestination);
                        var offset = import.FunctionAddressDestination - section.VirtualAddress;
                        var bytes = BitConverter.GetBytes(procAddress);
                        Array.Copy(bytes, 0, section.RawData, offset, bytes.Length);
                    } finally {
                        FreeLibrary(hModule);
                    }
                }

                foreach (var section in executable.Sections.Values) {
                    fixed (byte* data = section.RawData) {
                        int result = 0;
                        bool success = WriteProcessMemory(
                            handle.DangerousGetHandle(),
                            baseAddress + section.VirtualAddress,
                            new IntPtr(data), section.Size, out result
                        );

                        if (!success || result != section.Size) {
                            var error = GetLastError();
                            throw new Exception(String.Format("Write failed: Error {0:x8}", error));
                        }

                        MemoryProtection protection = MemoryProtection.ReadOnly;
                        if ((section.Characteristics & PortableExecutable.SectionCharacteristics.MemExecute) == PortableExecutable.SectionCharacteristics.MemExecute)
                            protection = MemoryProtection.ExecuteRead;
                        else if ((section.Characteristics & PortableExecutable.SectionCharacteristics.MemWrite) == PortableExecutable.SectionCharacteristics.MemWrite)
                            protection = MemoryProtection.ReadWrite;

                        MemoryProtection temp;
                        success = VirtualProtectEx(
                            handle.DangerousGetHandle(),
                            baseAddress + section.VirtualAddress,
                            section.Size, protection,
                            out temp
                        );

                        if (!success) {
                            var error = GetLastError();
                            throw new Exception(String.Format("Protect failed: Error {0:x8}", error));
                        }
                    }
                }

                Int32 threadId = 0;
                UInt32 creationFlags = 0x0;
                var threadHandle = new ThreadWaitHandle(new SafeWaitHandle(CreateRemoteThread(
                    handle.DangerousGetHandle(), IntPtr.Zero, 0,
                    baseAddress + executable.OptionalHeader.AddressOfEntryPoint,
                    IntPtr.Zero, creationFlags, out threadId
                ), true));

                ThreadPool.RegisterWaitForSingleObject(threadHandle, (s, e) => {
                    Int32 exitCode;
                    GetExitCodeThread(handle.DangerousGetHandle(), out exitCode);
                    threadResultFuture.Complete(exitCode);
                }, null, -1, true);
            }
        }
    }
}
