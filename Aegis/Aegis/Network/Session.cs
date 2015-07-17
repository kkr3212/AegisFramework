using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Aegis;



namespace Aegis.Network
{
    public enum SessionStatus
    {
        Closed = 0,
        Accepted,
        Connecting,
        Connected,
        WaitForReceive,
        PacketReceived
    }





    public class Session
    {
        public Int32 SessionId { get; private set; }
        public Socket Socket { get; internal set; }
        public SessionStatus Status { get; private set; }

        private AsyncCallback _acReceive;
        private byte[] _recvBuffer;
        private Int32 _receivedBytes;





        internal Session(Int32 sessionId)
        {
            SessionId = sessionId;

            _acReceive = new AsyncCallback(OnRead);
            _recvBuffer = new byte[1024];

            Clear();
        }


        internal void Clear()
        {
            Socket = null;
            Status = SessionStatus.Closed;

            _receivedBytes = 0;
        }


        internal void Accepted()
        {
            Clear();
            BeginReceive();
        }


        private void BeginReceive()
        {
            Int32 remainBufferSize = _recvBuffer.Length - _receivedBytes;
            Socket.BeginReceive(_recvBuffer, _receivedBytes, remainBufferSize, 0, _acReceive, null);
        }


        private void OnRead(IAsyncResult ar)
        {
            try
            {
                Int32 transBytes = Socket.EndReceive(ar);
                if (transBytes == 0)
                {
                    OnClose();
                    return;
                }

                _receivedBytes += transBytes;
                BeginReceive();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        protected virtual void OnAccept()
        {
        }


        protected virtual void OnConnect(Boolean connected)
        {
        }


        protected virtual void OnClose()
        {
        }


        protected virtual void OnSend(Int32 transBytes)
        {
        }


        protected virtual void OnReceive(Int32 transBytes)
        {
        }
    }
}
