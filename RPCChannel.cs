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
        ReloadModules = 3,
        CallFunction = 4
    }

    public struct RPCMessage {
        public RPCMessageType Type;
        public string ModuleName;
        public string FunctionName;
        public string Text;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TransportRPCMessage {
        public RPCMessageType Type;
        public UInt32 MessageId;
        public UInt32 ModuleName;
        public UInt32 FunctionName;
        public UInt32 Text;
    }

    public class RPCResponseChannel : NativeWindow, IDisposable {
        protected int WM_RPC_MESSAGE;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        protected Process _Process;
        protected Dictionary<UInt32, Future<byte[]>> _AwaitingResponses = new Dictionary<uint, Future<byte[]>>();
        protected BlockingQueue<byte[]> _Messages = new BlockingQueue<byte[]>();
        protected Random _Random = new Random();

        public UInt32 RemoteThreadId = 0;

        public RPCResponseChannel (Process process) 
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

            try {
                if (!Win32.ChangeWindowMessageFilterEx(
                    this.Handle, WM_RPC_MESSAGE, MessageFilterFlag.AllowMessage, IntPtr.Zero
                )) {
                    var error = Win32.GetLastError();
                    throw new Exception(String.Format("Error changing window message filter: {0:x8}", error));
                }
            } catch (EntryPointNotFoundException) {
                try {
                    if (!Win32.ChangeWindowMessageFilter(
                        WM_RPC_MESSAGE, MessageFilterFlag.AllowMessage
                    )) {
                        var error = Win32.GetLastError();
                        throw new Exception(String.Format("Error changing window message filter: {0:x8}", error));
                    }
                } catch (EntryPointNotFoundException) {
                }
            }
        }

        protected unsafe byte[] ReadRemoteData (RemoteMemoryRegion region, out UInt32 messageId) {
            using (var handle = region.OpenHandle(ProcessAccessFlags.VMRead)) {
                messageId = BitConverter.ToUInt32(
                    region.ReadBytes(handle, 0, 4), 0
                );

                return region.ReadBytes(handle, 4, region.Size - 4);
            }
        }

        protected override void WndProc (ref Message m) {
            if (m.Msg == WM_RPC_MESSAGE) {
                byte[] messageData = null;
                UInt32 messageID = 0;
                if ((m.WParam != IntPtr.Zero) && (m.LParam != IntPtr.Zero))
                using (var region = RemoteMemoryRegion.Existing(_Process, m.WParam, (uint)m.LParam.ToInt64()))
                    messageData = ReadRemoteData(region, out messageID);

                Future<byte[]> fResult;
                if (_AwaitingResponses.TryGetValue(messageID, out fResult)) {
                    _AwaitingResponses.Remove(messageID);
                    fResult.SetResult(messageData, null);
                } else {
                    _Messages.Enqueue(messageData);
                }
            } else {
                base.WndProc(ref m);
            }
        }

        public Future<byte[]> Receive () {
            return _Messages.Dequeue();
        }

        public UInt32 GetMessageID () {
            var buf = new byte[4];
            _Random.NextBytes(buf);
            return BitConverter.ToUInt32(buf, 0);
        }

        public Future<byte[]> WaitForMessage (UInt32 messageID) {
            var result = new Future<byte[]>();
            _AwaitingResponses[messageID] = result;
            return result;
        }

        public UInt32 ChannelID {
            get {
                return (UInt32)Handle.ToInt64();
            }
        }

        public void Dispose () {
            foreach (var f in _AwaitingResponses.Values)
                f.Dispose();
            _AwaitingResponses.Clear();

            DestroyHandle();
        }
    }

    public class RPCChannel : RPCResponseChannel {
        public RPCChannel (Process process)
            : base(process) {
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

        public void Send (RPCMessage message) {
            Send(message, false);
        }

        public unsafe Future<byte[]> Send (RPCMessage message, bool wantResponse) {
            if (_Process == null)
                throw new Exception("No remote process");
            if (RemoteThreadId == 0)
                throw new Exception("No remote thread");

            UInt32 messageSize = (UInt32)Marshal.SizeOf(typeof(TransportRPCMessage));
            UInt32 moduleNameSize = TransportStringSize(message.ModuleName);
            UInt32 functionNameSize = TransportStringSize(message.FunctionName);
            UInt32 textSize = TransportStringSize(message.Text);
            Future<byte[]> result = null;
            UInt32 messageID = 0;

            if (wantResponse) {
                messageID = GetMessageID();
                result = new Future<byte[]>();
                _AwaitingResponses[messageID] = result;
            }

            using (var handle = Win32.OpenProcessHandle(ProcessAccessFlags.VMWrite | ProcessAccessFlags.VMOperation, false, _Process.Id)) {
                RemoteMemoryRegion region;
                var regionSize = messageSize + moduleNameSize + textSize + functionNameSize;
                var buffer = new byte[regionSize];

                // leaked on purpose
                region = RemoteMemoryRegion.Allocate(
                    _Process, handle, regionSize
                );

                object transportMessage = new TransportRPCMessage {
                    Type = message.Type,
                    MessageId = messageID,
                    Text = WriteTransportString(message.Text, buffer, messageSize, region.Address),
                    FunctionName = WriteTransportString(message.FunctionName, buffer, messageSize + textSize, region.Address),
                    ModuleName = WriteTransportString(message.ModuleName, buffer, messageSize + textSize + functionNameSize, region.Address)
                };

                fixed (byte* pBuffer = buffer) {
                    Marshal.StructureToPtr(transportMessage, new IntPtr(pBuffer), false);

                    try {
                        region.Write(handle, 0, regionSize, pBuffer);
                    } catch {
                        try {
                            region.Dispose();
                        } catch {
                        }
                        throw;
                    }
                }

                if (!Win32.PostThreadMessage(RemoteThreadId, WM_RPC_MESSAGE, region.Address, region.Size)) {
                    var error = Win32.GetLastError();
                    region.Dispose();
                    throw new Exception(String.Format("Error posting thread message: {0:x8}", error));
                }
            }

            return result;
        }
    }
}
