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
    public static class ProcessInjector {
        public static unsafe void Inject (Process process, PortableExecutable executable, Future<Int32> threadResultFuture) {
            using (var handle = new SafeProcessHandle(
                Win32.OpenProcess(ProcessAccessFlags.All, false, process.Id)
            )) {
                var allocPtr = Win32.VirtualAllocEx(
                    handle.DangerousGetHandle(), IntPtr.Zero,
                    executable.OptionalHeader.SizeOfImage,
                    AllocationType.Commit | AllocationType.Reserve,
                    MemoryProtection.ReadWrite
                );
                if (allocPtr == IntPtr.Zero) {
                    var error = Win32.GetLastError();
                    throw new Exception(String.Format("Allocation failed: Error {0:x8}", error));
                }

                var baseAddress = (UInt32)allocPtr.ToInt64();

                executable.Rebase(baseAddress);
                executable.ResolveImports();

                foreach (var section in executable.Sections.Values) {
                    fixed (byte* data = section.RawData) {
                        int result = 0;
                        bool success = Win32.WriteProcessMemory(
                            handle.DangerousGetHandle(),
                            baseAddress + section.VirtualAddress,
                            new IntPtr(data), section.Size, out result
                        );

                        if (!success || result != section.Size) {
                            var error = Win32.GetLastError();
                            throw new Exception(String.Format("Write failed: Error {0:x8}", error));
                        }

                        // Why the fuck isn't this a flags-style enumeration? Sigh, classic windows.
                        MemoryProtection protection = MemoryProtection.ReadOnly;
                        if ((section.Characteristics & PortableExecutable.SectionCharacteristics.MemExecute) == PortableExecutable.SectionCharacteristics.MemExecute)
                            protection = MemoryProtection.ExecuteRead;
                        else if ((section.Characteristics & PortableExecutable.SectionCharacteristics.MemWrite) == PortableExecutable.SectionCharacteristics.MemWrite)
                            protection = MemoryProtection.ReadWrite;

                        MemoryProtection temp;
                        success = Win32.VirtualProtectEx(
                            handle.DangerousGetHandle(),
                            baseAddress + section.VirtualAddress,
                            section.Size, protection,
                            out temp
                        );

                        if (!success) {
                            var error = Win32.GetLastError();
                            throw new Exception(String.Format("Protect failed: Error {0:x8}", error));
                        }
                    }
                }

                Int32 threadId = 0;
                UInt32 creationFlags = 0x0;
                IntPtr remoteThreadHandle = Win32.CreateRemoteThread(
                    handle.DangerousGetHandle(), IntPtr.Zero, 0,
                    baseAddress + executable.OptionalHeader.AddressOfEntryPoint,
                    baseAddress,
                    creationFlags, out threadId
                );
                if (remoteThreadHandle == IntPtr.Zero) {
                    var error = Win32.GetLastError();
                    throw new Exception(String.Format("Thread start failed: Error {0:x8}", error));
                }
                var threadHandle = new ThreadWaitHandle(new SafeWaitHandle(remoteThreadHandle, true));
                ThreadPool.RegisterWaitForSingleObject(threadHandle, (s, e) => {
                    Int32 exitCode;
                    Win32.GetExitCodeThread(handle.DangerousGetHandle(), out exitCode);
                    threadResultFuture.Complete(exitCode);
                    threadHandle.Close();
                }, null, -1, true);
            }
        }
    }
}
