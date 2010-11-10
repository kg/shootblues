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
        public class RemoteMemoryRegion : IDisposable {
            public Process Process;
            public IntPtr Address;
            public UInt32 Size;

            private RemoteMemoryRegion () {
            }

            public static RemoteMemoryRegion Allocate (Process process, UInt32 size) {
                using (var handle = new SafeProcessHandle(
                    Win32.OpenProcess(ProcessAccessFlags.All, false, process.Id)
                )) {
                    return Allocate(process, handle, size);
                }
            }

            public static RemoteMemoryRegion Allocate (Process process, SafeProcessHandle handle, UInt32 size) {
                var result = new RemoteMemoryRegion {
                    Process = process,
                    Size = size
                };
                result.Address = Win32.VirtualAllocEx(
                    handle.DangerousGetHandle(), IntPtr.Zero,
                    size, AllocationType.Commit | AllocationType.Reserve,
                    MemoryProtection.ReadWrite
                );
                if (result.Address == IntPtr.Zero) {
                    var error = Win32.GetLastError();
                    throw new Exception(String.Format("Allocation failed: Error {0:x8}", error));
                }
                return result;
            }

            public static RemoteMemoryRegion Existing (Process process, IntPtr address, UInt32 size) {
                return new RemoteMemoryRegion {
                    Process = process,
                    Address = address,
                    Size = size
                };
            }

            public void Dispose () {
                using (var handle = new SafeProcessHandle(
                    Win32.OpenProcess(ProcessAccessFlags.All, false, Process.Id)
                )) {
                    Win32.VirtualFreeEx(
                        handle.DangerousGetHandle(),
                        Address, Size, FreeType.Release
                    );
                }
            }
        }

        public static unsafe RemoteMemoryRegion Inject (Process process, PortableExecutable executable, IntPtr payloadArgument, Future<Int32> threadResultFuture, Future<UInt32> threadIdFuture) {
            RemoteMemoryRegion region = null;
            using (var handle = new SafeProcessHandle(
                Win32.OpenProcess(ProcessAccessFlags.All, false, process.Id)
            ))
            try {
                region = RemoteMemoryRegion.Allocate(
                    process, handle, executable.OptionalHeader.SizeOfImage
                );
                var baseAddress = (UInt32)region.Address.ToInt64();

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

                UInt32 threadId = 0;
                UInt32 creationFlags = 0x0;
                IntPtr remoteThreadHandle = Win32.CreateRemoteThread(
                    handle.DangerousGetHandle(), IntPtr.Zero, 0,
                    baseAddress + executable.OptionalHeader.AddressOfEntryPoint,
                    payloadArgument,
                    creationFlags, out threadId
                );
                if (remoteThreadHandle == IntPtr.Zero) {
                    var error = Win32.GetLastError();
                    throw new Exception(String.Format("Thread start failed: Error {0:x8}", error));
                }

                threadIdFuture.Complete(threadId);
                var threadHandle = new ThreadWaitHandle(new SafeWaitHandle(remoteThreadHandle, true));
                ThreadPool.RegisterWaitForSingleObject(threadHandle, (s, e) => {
                    Int32 exitCode;
                    Win32.GetExitCodeThread(handle.DangerousGetHandle(), out exitCode);
                    threadResultFuture.Complete(exitCode);
                    threadHandle.Close();
                }, null, -1, true);

                var theResult = region;
                region = null;
                return theResult;
            } finally {
                if (region != null)
                    region.Dispose();
            }
        }
    }
}
