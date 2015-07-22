using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Aegis;



namespace Aegis.Network
{
    public class Session
    {
        private static Int32 NextSessionId = 0;

        public Int32 SessionId { get; private set; }
        public Socket Socket { get; internal set; }
        public Boolean IsConnected { get { return (Socket == null ? false : Socket.Connected); } }

        internal Action<Session> OnSessionClosed;
        private AsyncCallback _acConnect, _acReceive;
        private byte[] _receivedBuffer;
        private Int32 _receivedBytes;





        public Session(Int32 recvBufferSize)
        {
            Interlocked.Increment(ref NextSessionId);

            SessionId = NextSessionId;
            _acConnect = new AsyncCallback(OnSocket_Connect);
            _acReceive = new AsyncCallback(OnSocket_Read);
            _receivedBuffer = new byte[recvBufferSize];

            Clear();
        }


        internal void Clear()
        {
            if (Socket == null)
                return;

            Socket.Dispose();
            Socket = null;

            _receivedBytes = 0;
            Array.Clear(_receivedBuffer, 0, _receivedBuffer.Length);
        }


        private void WaitForReceive()
        {
            Int32 remainBufferSize = _receivedBuffer.Length - _receivedBytes;
            Socket.BeginReceive(_receivedBuffer, _receivedBytes, remainBufferSize, 0, _acReceive, null);
        }


        public void Connect(String ipAddress, Int32 portNo)
        {
            if (Socket != null)
                throw new AegisException(ResultCode.ActivatedSession, "Sessions is already active.");


            //  연결 시도
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNo);

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.BeginConnect(ipEndPoint, _acConnect, null);
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


        internal void OnSocket_Closed()
        {
            try
            {
                //  Close 작업 중 다른 이벤트가 처리되지 못하도록 Clear까지 lock을 걸어야 한다.
                lock (this)
                {
                    OnClose();
                    Clear();
                }
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }


            OnSessionClosed(this);
        }


        private void OnSocket_Connect(IAsyncResult ar)
        {
            Socket.EndConnect(ar);

            if (Socket.Connected == true)
            {
                lock (this)
                    OnConnect(true);

                WaitForReceive();
            }
            else
            {
                Socket = null;

                lock (this)
                    OnConnect(false);
            }
        }


        private void OnSocket_Read(IAsyncResult ar)
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

                _receivedBytes += transBytes;


                //  패킷 하나가 정상적으로 수신되었는지 확인
                Int32 packetSize;
                if (IsValidPacket(_receivedBuffer, _receivedBytes, out packetSize) == true)
                {
                    try
                    {
                        //  수신 이벤트
                        lock (this)
                        {
                            if (Socket != null)
                                OnReceive(_receivedBuffer, packetSize);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Write(LogType.Err, 1, e.ToString());
                    }


                    //  패킷을 버퍼에서 제거
                    Array.Copy(_receivedBuffer, _receivedBytes, _receivedBuffer, 0, _receivedBuffer.Length - packetSize);
                    _receivedBytes -= packetSize;
                }

                //  ReceiveBuffer의 안정적인 처리를 위해 OnReceive 작업이 끝난 후에 다시 수신대기
                WaitForReceive();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        protected virtual Boolean IsValidPacket(byte[] receiveBuffer, Int32 receiveBytes, out Int32 packetSize)
        {
            //  최초 2바이트를 수신할 패킷의 크기로 처리
            packetSize = BitConverter.ToInt16(receiveBuffer, 0);
            return (packetSize > 0 && receiveBytes >= packetSize);
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


        protected virtual void OnReceive(byte[] receiveBuffer, Int32 packetSize)
        {
        }
    }
}
