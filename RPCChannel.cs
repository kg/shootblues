using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ShootBlues {
    public enum RPCMessageType : uint {
        Run = 0,
        AddModule = 1,
        RemoveModule = 2,
        ReloadModules = 3
    }

    public struct RPCMessage {
        public RPCMessageType Type;
        public string ModuleName;
        public string Text;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TransportRPCMessage {
        public RPCMessageType Type;
        public UInt32 ModuleName;
        public UInt32 Text;
    }

    public class RPCChannel : NativeWindow, IDisposable {
        private int WM_RPC_MESSAGE;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private Process _Process;
        private BlockingQueue<byte[]> _Messages = new BlockingQueue<byte[]>();

        public UInt32 RemoteThreadId = 0;

        public RPCChannel (Process process) 
            : base() {
            _Process = process;

            WM_RPC_MESSAGE = Win32.RegisterWindowMessage("ShootBlues.RPCMessage");
            var cp = new CreateParams {
                Caption = "ShootBlues.RPCChannel",
                X = 0,
                Y = 0,
                Width = 0,
                Height = 0,
                Style = 0,
                ExStyle = WS_EX_NOACTIVATE,
                Parent = new IntPtr(-3)
            };
            CreateHandle(cp);
        }

        protected unsafe byte[] ReadRemoteData (IntPtr address, UInt32 size) {
            int bytesRead = 0;
            byte[] result = new byte[size];
            using (var handle = new SafeProcessHandle(
                Win32.OpenProcess(ProcessAccessFlags.All, false, _Process.Id)
            ))
            fixed (byte* pResult = result)
                Win32.ReadProcessMemory(
                    handle.DangerousGetHandle(), address,
                    new IntPtr(pResult), size,
                    out bytesRead
                );

            if (bytesRead != 0)
                return result;
            else
                return null;
        }

        protected override void WndProc (ref Message m) {
            if (m.Msg == WM_RPC_MESSAGE) {
                byte[] messageData = null;
                if ((m.WParam != IntPtr.Zero) && (m.LParam != IntPtr.Zero))
                    messageData = ReadRemoteData(m.WParam, (uint)m.LParam.ToInt64());

                _Messages.Enqueue(messageData);
            } else {
                base.WndProc(ref m);
            }
        }

        public Future<byte[]> Receive () {
            return _Messages.Dequeue();
        }

        private UInt32 TransportStringSize (string text) {
            if (text == null)
                return 0;

            return (UInt32)Encoding.ASCII.GetByteCount(text) + 1;
        }

        private UInt32 WriteTransportString (string text, byte[] buffer, UInt32 offset, IntPtr baseAddress) {
            if (text == null)
                return 0;

            int chars = Encoding.ASCII.GetBytes(text, 0, text.Length, buffer, (int)offset);
            buffer[offset + chars] = 0;
            return (UInt32)(baseAddress.ToInt64() + offset);
        }

        public unsafe void Send (RPCMessage message) {
            if (_Process == null)
                throw new Exception("No remote process");
            if (RemoteThreadId == 0)
                throw new Exception("No remote thread");

            UInt32 messageSize = (UInt32)Marshal.SizeOf(typeof(TransportRPCMessage));
            UInt32 moduleNameSize = TransportStringSize(message.ModuleName);
            UInt32 textSize = TransportStringSize(message.Text);

            using (var handle = new SafeProcessHandle(
                Win32.OpenProcess(ProcessAccessFlags.All, false, _Process.Id)
            )) {
                int result;
                ProcessInjector.RemoteMemoryRegion region;
                var regionSize = messageSize + moduleNameSize + textSize;
                var buffer = new byte[regionSize];

                // leaked on purpose
                region = ProcessInjector.RemoteMemoryRegion.Allocate(
                    _Process, handle, regionSize
                );

                object transportMessage = new TransportRPCMessage {
                    Type = message.Type,
                    Text = WriteTransportString(message.Text, buffer, messageSize, region.Address),
                    ModuleName = WriteTransportString(message.ModuleName, buffer, messageSize + textSize, region.Address)
                };

                fixed (byte* pBuffer = buffer) {
                    Marshal.StructureToPtr(transportMessage, new IntPtr(pBuffer), false);

                    Win32.WriteProcessMemory(
                        _Process.Handle, (uint)region.Address.ToInt64(),
                        new IntPtr(pBuffer), region.Size,
                        out result
                    );
                }

                if (result != regionSize) {
                    var error = Win32.GetLastError();
                    region.Dispose();
                    throw new Exception(String.Format("Remote write failed: error {0:x8}", error));
                }

                Win32.PostThreadMessage(RemoteThreadId, WM_RPC_MESSAGE, region.Address, region.Size);
            }
        }

        public void Dispose () {
            DestroyHandle();
        }
    }
}
