using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Aegis;



namespace Aegis.Network
{
    public class Session
    {
        public Int32 SessionId { get; private set; }
        public Socket Socket { get; internal set; }
        public byte[] ReceivedBuffer { get; private set; }
        public Int32 ReceivedBytes { get; private set; }
        public Boolean IsConnected { get { return (Socket == null ? false : Socket.Connected); } }

        private SessionManager _sessionManager;
        private AsyncCallback _acReceive;





        internal Session(SessionManager parent, Int32 sessionId, Int32 recvBufferSize)
        {
            _sessionManager = parent;
            SessionId = sessionId;

            _acReceive = new AsyncCallback(OnRead);
            ReceivedBuffer = new byte[recvBufferSize];

            Clear();
        }


        internal void Clear()
        {
            Socket = null;

            ReceivedBytes = 0;
            Array.Clear(ReceivedBuffer, 0, ReceivedBuffer.Length);
        }


        internal void OnSocket_Accepted()
        {
            //  OnReceive보다 OnAccept가 먼저 호출되기 위해서는
            //  BeginReceive보다 먼저 Post해야 한다.
            {
                SessionJob job = SessionJob.NewJob(IOType.Accept, this, 0);
                _sessionManager.NetworkChannel.IoWorker.Post(job);
            }
            BeginReceive();
        }


        private void OnSocket_Closed()
        {
            Clear();

            SessionJob job = SessionJob.NewJob(IOType.Close, this, 0);
            _sessionManager.NetworkChannel.IoWorker.Post(job);
        }


        private void BeginReceive()
        {
            Int32 remainBufferSize = ReceivedBuffer.Length - ReceivedBytes;
            Socket.BeginReceive(ReceivedBuffer, ReceivedBytes, remainBufferSize, 0, _acReceive, null);
        }


        private void OnRead(IAsyncResult ar)
        {
            try
            {
                Int32 transBytes = Socket.EndReceive(ar);


                //  transBytes가 0이면 원격지 혹은 네트워크에 의해 연결이 끊긴 상태
                if (transBytes == 0)
                {
                    OnSocket_Closed();
                    return;
                }

                ReceivedBytes += transBytes;


                //  패킷 하나가 정상적으로 수신되었는지 확인
                Int32 realPacketSize;
                if (IsValidPacket(ReceivedBytes - transBytes, out realPacketSize) == true)
                {
                    //  수신 이벤트 (#! 수신된 패킷을 전달해야 함)
                    SessionJob job = SessionJob.NewJob(IOType.Receive, this, transBytes);
                    _sessionManager.NetworkChannel.IoWorker.Post(job);


                    //  패킷을 버퍼에서 제거
                    Array.Copy(ReceivedBuffer, ReceivedBytes, ReceivedBuffer, 0, ReceivedBuffer.Length - realPacketSize);
                    ReceivedBytes -= realPacketSize;
                }

                BeginReceive();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        internal void DoSessionJob(SessionJob job)
        {
            switch (job.Type)
            {
                case IOType.Accept: OnAccept(); break;
                case IOType.Connect: OnConnect(job.Value == 1); break;
                case IOType.Close: OnClose(); break;
                case IOType.Send: OnSend(job.Value); break;
                case IOType.Receive: OnReceive(job.Value); break;
            }
        }


        protected virtual Boolean IsValidPacket(Int32 headerIndex, out Int32 realPacketSize)
        {
            realPacketSize = BitConverter.ToInt16(ReceivedBuffer, headerIndex);
            return (realPacketSize > 0 && ReceivedBytes >= realPacketSize);
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
