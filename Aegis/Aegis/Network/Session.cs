using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using Aegis;



namespace Aegis.Network
{
    public class Session
    {
        private static Int32 NextSessionId = 0;

        public Int32 SessionId { get; private set; }
        public Socket Socket { get; internal set; }
        public byte[] ReceivedBuffer { get; private set; }
        public Int32 ReceivedBytes { get; private set; }
        public Boolean IsConnected { get { return (Socket == null ? false : Socket.Connected); } }

        internal Action<Session> OnSessionClosed;
        private AsyncCallback _acReceive;





        public Session(Int32 recvBufferSize)
        {
            Interlocked.Increment(ref NextSessionId);

            SessionId = NextSessionId;
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
            try
            {
                lock (this)
                    OnAccept();

                WaitForReceive();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        private void OnSocket_Closed()
        {
            Clear();

            try
            {
                lock (this)
                    OnClose();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }


            OnSessionClosed(this);
        }


        private void WaitForReceive()
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
                if (IsValidPacket(transBytes, ReceivedBytes - transBytes, out realPacketSize) == true)
                {
                    try
                    {
                        //  수신 이벤트 (#! 수신된 패킷을 전달해야 함)
                        lock (this)
                            OnReceive(transBytes);

                    }
                    catch (Exception e)
                    {
                        Logger.Write(LogType.Err, 1, e.ToString());
                    }


                    //  패킷을 버퍼에서 제거
                    Array.Copy(ReceivedBuffer, ReceivedBytes, ReceivedBuffer, 0, ReceivedBuffer.Length - realPacketSize);
                    ReceivedBytes -= realPacketSize;
                }

                WaitForReceive();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        protected virtual Boolean IsValidPacket(Int32 recvBytes, Int32 headerIndex, out Int32 realPacketSize)
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
