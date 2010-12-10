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
    public class RemoteMemoryRegion : IDisposable {
        public Process Process;
        public IntPtr Address;
        public UInt32 Size;

        private RemoteMemoryRegion () {
        }

        public static RemoteMemoryRegion Allocate (Process process, UInt32 size) {
            using (var handle = Win32.OpenProcessHandle(
                ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite, 
                false, process.Id
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

        public unsafe int Write (SafeProcessHandle handle, uint offset, uint size, byte* data) {
            if (Address == IntPtr.Zero)
                throw new ObjectDisposedException("RemoteMemoryRegion");
            if ((offset + size) > Size)
                throw new ArgumentException("Size too large for region");

            int bytesWritten = 0;
            int result = Win32.WriteProcessMemory(
                handle.DangerousGetHandle(),
                (uint)(Address.ToInt64() + offset),
                new IntPtr(data), size, out bytesWritten
            );

            if (result == 0 || bytesWritten != size) {
                var error = Win32.GetLastError();
                throw new Exception(String.Format("Write failed: Error {0:x8}", error));
            }

            return bytesWritten;
        }

        private unsafe int Read (SafeProcessHandle handle, uint offset, uint size, byte* pBuffer) {
            if (Address == IntPtr.Zero)
                throw new ObjectDisposedException("RemoteMemoryRegion");
            if ((offset + size) > Size)
                throw new ArgumentException("Size too large for region");

            int bytesRead = 0, result;
            result = Win32.ReadProcessMemory(
                handle.DangerousGetHandle(),
                (uint)(Address.ToInt64() + offset),
                new IntPtr(pBuffer), size, out bytesRead
            );

            if (result == 0 || bytesRead != size) {
                var error = Win32.GetLastError();
                throw new Exception(String.Format("Read failed: Error {0:x8}", error));
            }

            return bytesRead;
        }

        public unsafe int Read (SafeProcessHandle handle, uint offset, uint size, byte[] buffer) {
            if ((buffer == null) || (size != buffer.Length))
                throw new ArgumentException("Invalid buffer to read into");

            fixed (byte* pBuffer = buffer)
                return Read(handle, offset, size, pBuffer);
        }

        public byte[] ReadBytes (SafeProcessHandle handle, uint offset, uint size) {
            if (size == 0)
                return null;

            byte[] buffer = new byte[size];
            Read(handle, offset, (uint)size, buffer);
            return buffer;
        }

        public MemoryProtection Protect (SafeProcessHandle handle, uint offset, uint size, MemoryProtection newProtect) {
            if (Address == IntPtr.Zero)
                throw new ObjectDisposedException("RemoteMemoryRegion");
            if ((offset + size) > Size)
                throw new ArgumentException("Size too large for region");

            MemoryProtection oldProtect;
            int result = Win32.VirtualProtectEx(
                handle.DangerousGetHandle(),
                (uint)(Address.ToInt64() + offset), 
                size, newProtect, out oldProtect
            );

            if (result == 0) {
                var error = Win32.GetLastError();
                throw new Exception(String.Format("Protect failed: Error {0:x8}", error));
            }

            return oldProtect;
        }

        public SafeProcessHandle OpenHandle (ProcessAccessFlags flags) {
            return Win32.OpenProcessHandle(flags, false, Process.Id);
        }

        public void Dispose () {
            if (Address == IntPtr.Zero)
                return;

            using (var handle = OpenHandle(ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite)) {
                int result = Win32.VirtualFreeEx(
                    handle.DangerousGetHandle(),
                    Address, 0, FreeType.Release
                );

                if (result == 0) {
                    var error = Win32.GetLastError();

                    throw new Exception(String.Format(
                        "Failed to free region: Error {0:x8}", error
                    ));
                } else {
                    Address = IntPtr.Zero;
                    Size = 0;
                }
            }
        }
    }

    public static class ProcessInjector {
        public static unsafe RemoteMemoryRegion Inject (Process process, PortableExecutable executable, IntPtr payloadArgument, Future<Int32> threadResultFuture, Future<UInt32> threadIdFuture) {
            RemoteMemoryRegion region = null;
            using (var handle = Win32.OpenProcessHandle(
                ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite |
                ProcessAccessFlags.VMOperation | ProcessAccessFlags.CreateThread | 
                ProcessAccessFlags.QueryInformation, 
                false, process.Id
            )) 
            try {
                region = RemoteMemoryRegion.Allocate(
                    process, handle, executable.OptionalHeader.SizeOfImage
                );
                region.Protect(handle, 0, region.Size, MemoryProtection.ReadWrite);

                var baseAddress = (UInt32)region.Address.ToInt64();

                executable.Rebase(baseAddress);
                executable.ResolveImports();

                foreach (var section in executable.Sections.Values) {
                    fixed (byte* data = section.RawData) {
                        region.Write(
                            handle, section.VirtualAddress, section.Size, data
                        );

                        // Why the fuck isn't this a flags-style enumeration? Sigh, classic windows.
                        MemoryProtection protection = MemoryProtection.ReadOnly;
                        if ((section.Characteristics & PortableExecutable.SectionCharacteristics.MemExecute) == PortableExecutable.SectionCharacteristics.MemExecute)
                            protection = MemoryProtection.ExecuteRead;
                        else if ((section.Characteristics & PortableExecutable.SectionCharacteristics.MemWrite) == PortableExecutable.SectionCharacteristics.MemWrite)
                            protection = MemoryProtection.ReadWrite;

                        region.Protect(
                            handle, section.VirtualAddress, section.Size, protection
                        );
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
                if (!process.HasExited && (region != null))
                    region.Dispose();
            }
        }
    }
}
