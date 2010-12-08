using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Security;
using Squared.Task;
using System.Security.Principal;

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

    [Flags]
    public enum ThreadAccessFlags : uint {
        Terminate = 0x0001,
        SuspendResume = 0x0002,
        GetContext = 0x0008,
        SetContext = 0x0010,
        SetInformation = 0x0020,
        QueryInformation = 0x0040,
        SetThreadToken = 0x0080,
        Impersonate = 0x0100,
        DirectImpersonation = 0x0200
    }
    
    [Flags]
    public enum PrivilegeAttributes : uint {
        Disabled = 0x0,
        EnabledByDefault = 0x00000001,
        Enabled = 0x00000002,
        Removed = 0x00000004,
        UsedForAccess = 0x80000000
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct TokenPrivileges {
        public UInt32 PrivilegeCount;
        public UInt64 Luid;
        public PrivilegeAttributes Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect {
        public int Left, Top, Right, Bottom;
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

    [SuppressUnmanagedCodeSecurity]
    public delegate bool EnumWindowsProc (IntPtr hWnd, IntPtr lParam);

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
        public static extern int VirtualProtectEx (
            IntPtr hProcess, UInt32 lpAddress,
            uint dwSize, MemoryProtection flNewProtect,
            out MemoryProtection flOldProtect
        );
        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int VirtualFreeEx (
            IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, FreeType dwFreeType
        );
        [DllImport("kernel32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int WriteProcessMemory (
            IntPtr hProcess, UInt32 lpBaseAddress,
            IntPtr lpSrc, uint nSize,
            out int lpNumberOfBytesWritten
        );
        [DllImport("kernel32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int ReadProcessMemory (
            IntPtr hProcess, UInt32 lpBaseAddress,
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
        public static extern bool PostMessage (
            IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam
        );
        [DllImport("user32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern UInt32 SendMessage (
            IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam
        );
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool PostThreadMessage (
            UInt32 threadId, int Msg, IntPtr wParam, UInt32 lParam
        );
        [DllImport("user32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr FindWindow (string lpClassName, string lpWindowName);
        [DllImport("user32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern UInt32 GetWindowThreadProcessId (IntPtr hWnd, out int lpdwProcessId);
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool EnumWindows (EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int GetWindowText (
            IntPtr hWnd, StringBuilder lpString, int nMaxCount
        );
        [DllImport("user32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int GetWindowTextLength (IntPtr hWnd);
        [DllImport("user32", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int GetClassName (
            IntPtr hWnd, StringBuilder lpString, int nMaxCount
        );
        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Auto)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LookupPrivilegeValue (
            string lpSystemName, string lpName, out UInt64 lpLuid
        );
        [DllImport("advapi32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AdjustTokenPrivileges (
            IntPtr tokenHandle,
            [MarshalAs(UnmanagedType.Bool)]bool disableAllPrivileges,
            ref TokenPrivileges newState,
            UInt32 pOldStateMaxLength,
            IntPtr pOldState,
            IntPtr pOldStateLength
        );
        [DllImport("advapi32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken (
            IntPtr processHandle, 
            [MarshalAs(UnmanagedType.U4)]
            TokenAccessLevels desiredAccess, 
            out IntPtr tokenHandle
        );
        [DllImport("user32", SetLastError = true)]
        public static extern bool GetClientRect (IntPtr hWnd, out Rect lpRect);
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow (IntPtr hwnd, IntPtr hDC, uint nFlags);
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr OpenThread (ThreadAccessFlags dwDesiredAccess, bool bInheritHandle, int dwThreadId);
        [DllImport("kernel32", SetLastError = true)]
        public static extern uint SuspendThread (IntPtr hThread);
        [DllImport("kernel32", SetLastError = true)]
        public static extern int ResumeThread (IntPtr hThread);
        [DllImport("user32", SetLastError = true, CharSet=CharSet.Unicode)]
        public static extern short VkKeyScan (short wchar);

        public static string GetWindowTextString (IntPtr hWnd) {
            int length = GetWindowTextLength(hWnd);
            var sb = new StringBuilder(length + 1);
            int numChars = GetWindowText(hWnd, sb, length + 1);
            if (numChars > 0)
                return sb.ToString(0, numChars);
            else
                return null;
        }

        public static string GetWindowClassString (IntPtr hWnd) {
            var sb = new StringBuilder(256);
            int numChars = GetClassName(hWnd, sb, 256);
            if (numChars > 0)
                return sb.ToString(0, numChars);
            else
                return null;
        }

        public static IntPtr FindProcessWindow (int processId, string className, string windowName) {
            var result = new Future<IntPtr>();
            EnumWindowsProc callback = (hWnd, lParam) => {
                int windowProcessId;
                GetWindowThreadProcessId(hWnd, out windowProcessId);
                if (windowProcessId != processId)
                    return true;
                if ((windowName != null) && (GetWindowTextString(hWnd) != windowName))
                    return true;
                if ((className != null) && (GetWindowClassString(hWnd) != className))
                    return true;

                result.SetResult(hWnd, null);
                return false;
            };

            EnumWindows(callback, IntPtr.Zero);
            if (result.Completed)
                return result.Result;
            else
                return IntPtr.Zero;
        }

        public static SafeProcessHandle OpenProcessHandle (ProcessAccessFlags desiredAccess, bool inheritHandle, int processId) {
            var handle = OpenProcess(desiredAccess, inheritHandle, processId);
            var error = GetLastError();

            if (handle == IntPtr.Zero)
                throw new Exception(String.Format(
                    "Failed to open process: Error {0:x8}", error
                ));

            return new SafeProcessHandle(handle);
        }

        public static void AdjustProcessPrivilege (int processId, string privilegeName, bool privilegeStatus) {
            UInt64 luid;
            if (!LookupPrivilegeValue(null, privilegeName, out luid)) {
                var error = GetLastError();
                throw new Exception(String.Format("LookupPrivilegeValue failed: Error {0:x8}", error));
            }

            using (var handle = OpenProcessHandle(ProcessAccessFlags.QueryInformation, false, processId)) {
                IntPtr token;
                if (!OpenProcessToken(handle.DangerousGetHandle(), TokenAccessLevels.AdjustPrivileges | TokenAccessLevels.Query, out token)) {
                    var error = GetLastError();
                    throw new Exception(String.Format("OpenProcessToken failed: Error {0:x8}", error));
                }
                try {
                    var newState = new TokenPrivileges {
                        PrivilegeCount = 1,
                        Luid = luid,
                        Attributes = privilegeStatus ? PrivilegeAttributes.Enabled : PrivilegeAttributes.Removed
                    };
                    if (!AdjustTokenPrivileges(token, false, ref newState, 0, IntPtr.Zero, IntPtr.Zero)) {
                        var error = GetLastError();
                        throw new Exception(String.Format("AdjustTokenPrivileges failed: Error {0:x8}", error));
                    }
                } finally {
                    CloseHandle(token);
                }                
            }
        }
    }
}
