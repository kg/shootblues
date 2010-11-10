using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Security;

namespace ShootBlues {
    [Flags]
    public enum AllocationType : uint {
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
    public enum FreeType : uint {
        Decommit = 0x4000,
        Release = 0x8000
    }

    [Flags]
    public enum MemoryProtection : uint {
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
    public enum ProcessAccessFlags : uint {
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

    // Microsoft and their goddamn sealed/abstract classes...
    public class ThreadWaitHandle : WaitHandle {
        public ThreadWaitHandle (SafeWaitHandle handle)
            : base() {
            base.SafeWaitHandle = handle;
        }
    }

    // Why do I even have to do this? sighhhh
    public class SafeProcessHandle : SafeHandle {
        public SafeProcessHandle (IntPtr handle)
            : base(IntPtr.Zero, true) {
            base.SetHandle(handle);
        }

        public override bool IsInvalid {
            get { return (base.handle == IntPtr.Zero) || (base.handle == new IntPtr(-1)); }
        }

        protected override bool ReleaseHandle () {
            return Win32.CloseHandle(base.handle);
        }
    }

    public static class Win32 {
        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle (IntPtr hObject);
        [DllImport("kernel32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr LoadLibrary (string lpFileName);
        [DllImport("kernel32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool FreeLibrary (IntPtr hModule);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern UInt32 GetProcAddress (IntPtr hModule, string procName);
        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr VirtualAllocEx (
            IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, AllocationType flAllocationType,
            MemoryProtection flProtect
        );
        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool VirtualProtectEx (
            IntPtr hProcess, UInt32 lpAddress,
            uint dwSize, MemoryProtection flNewProtect,
            out MemoryProtection flOldProtect
        );
        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr VirtualFreeEx (
            IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, FreeType dwFreeType
        );
        [DllImport("kernel32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool WriteProcessMemory (
            IntPtr hProcess, UInt32 lpBaseAddress,
            IntPtr lpSrc, uint nSize,
            out int lpNumberOfBytesWritten
        );
        [DllImport("kernel32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool ReadProcessMemory (
            IntPtr hProcess, IntPtr lpBaseAddress,
            IntPtr lpDest, uint nSize,
            out int lpNumberOfBytesRead
        );
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread (
            IntPtr hProcess, IntPtr lpThreadAttributes,
            uint dwStackSize, UInt32 lpStartAddress, IntPtr lpParameter,
            uint dwCreationFlags, out UInt32 lpThreadId
        );
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr OpenProcess (
            ProcessAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] 
            bool bInheritHandle,
            int dwProcessId
        );
        [DllImport("kernel32", SetLastError = true)]
        public static extern Int32 GetLastError ();
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool GetExitCodeThread (IntPtr hThread, out Int32 exitCode);
        [DllImport("user32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int RegisterWindowMessage (string lpString);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool PostMessage (IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool PostThreadMessage (UInt32 threadId, int Msg, IntPtr wParam, UInt32 lParam);
    }
}
