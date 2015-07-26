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

        internal SessionManager SessionManager;
        private StreamBuffer _receivedBuffer, _dispatchBuffer;





        public Session()
        {
            Interlocked.Increment(ref NextSessionId);

            SessionId = NextSessionId;
            _receivedBuffer = new StreamBuffer();
            _dispatchBuffer = new StreamBuffer();

            Clear();
        }


        public Session(Int32 recvBufferSize)
        {
            Interlocked.Increment(ref NextSessionId);

            SessionId = NextSessionId;
            _receivedBuffer = new StreamBuffer(recvBufferSize);
            _dispatchBuffer = new StreamBuffer();

            Clear();
        }


        internal Session(SessionManager sessionManager)
        {
            SessionManager = sessionManager;
        }


        internal void Clear()
        {
            if (Socket == null)
                return;

            if (Socket.Connected == true)
                Socket.Shutdown(SocketShutdown.Both);

            Socket.Close();
            Socket = null;

            _receivedBuffer.Clear();
        }


        private void WaitForReceive()
        {
            if (_receivedBuffer.WritableSize == 0)
                throw new AegisException(ResultCode.NotEnoughBuffer, "There is no remaining capacity of the receive buffer.");

            Socket.BeginReceive(_receivedBuffer.Buffer, _receivedBuffer.WrittenBytes, _receivedBuffer.WritableSize, 0, OnSocket_Read, null);
        }


        public void Connect(String ipAddress, Int32 portNo)
        {
            if (Socket != null)
                throw new AegisException(ResultCode.ActivatedSession, "Sessions is already active.");


            //  연결 시도
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNo);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.BeginConnect(ipEndPoint, OnSocket_Connect, null);
        }


        public void CloseSocket()
        {
            try
            {
                //  작업 중 다른 이벤트가 처리되지 못하도록 Clear까지 lock을 걸어야 한다.
                lock (this)
                {
                    if (Socket == null)
                        return;

                    OnClose();
                    Clear();
                }
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }


            if (SessionManager != null)
                SessionManager.InactivateSession(this);
        }


        internal void OnSocket_Accepted()
        {
            try
            {
                if (SessionManager != null)
                    SessionManager.ActivateSession(this);

                lock (this)
                {
                    OnAccept();
                    WaitForReceive();
                }
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        private void OnSocket_Connect(IAsyncResult ar)
        {
            try
            {
                lock (this)
                {
                    try
                    {
                        if (Socket == null)
                            return;

                        Socket.EndConnect(ar);
                    }
                    catch (Exception)
                    {
                        //  Nothing to do.
                    }



                    if (Socket.Connected == true)
                    {
                        if (SessionManager != null)
                            SessionManager.ActivateSession(this);

                        OnConnect(true);
                        WaitForReceive();
                    }
                    else
                    {
                        Clear();
                        if (SessionManager != null)
                            SessionManager.InactivateSession(this);

                        OnConnect(false);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        private void OnSocket_Read(IAsyncResult ar)
        {
            try
            {
                lock (this)
                {
                    if (Socket == null)
                        return;

                    //  transBytes가 0이면 원격지 혹은 네트워크에 의해 연결이 끊긴 상태
                    Int32 transBytes = Socket.EndReceive(ar);
                    if (transBytes == 0)
                    {
                        CloseSocket();
                        return;
                    }


                    _receivedBuffer.Write(transBytes);
                    _dispatchBuffer.Clear();
                    _dispatchBuffer.Write(_receivedBuffer.Buffer, 0, _receivedBuffer.WrittenBytes);
                    while (_dispatchBuffer.ReadableSize > 0)
                    {
                        //  패킷 하나가 정상적으로 수신되었는지 확인
                        Int32 packetSize;
                        if (IsValidPacket(_dispatchBuffer, out packetSize) == false)
                            break;

                        try
                        {
                            //  수신 이벤트 처리 중 종료 이벤트가 발생한 경우
                            if (Socket == null)
                                return;


                            //  수신처리(Dispatch)
                            _dispatchBuffer.ResetReadIndex();
                            OnReceive(_dispatchBuffer);

                            _dispatchBuffer.Read(packetSize);
                            _receivedBuffer.Read(packetSize);
                        }
                        catch (Exception e)
                        {
                            Logger.Write(LogType.Err, 1, e.ToString());
                        }
                    }


                    //  처리된 패킷을 버퍼에서 제거
                    _receivedBuffer.PopReadBuffer();

                    //  ReceiveBuffer의 안정적인 처리를 위해 OnReceive 작업이 끝난 후에 다시 수신대기
                    WaitForReceive();
                }
            }
            catch (SocketException)
            {
                CloseSocket();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        private void OnSocket_Send(IAsyncResult ar)
        {
            try
            {
                lock (this)
                {
                    if (Socket == null)
                        return;

                    Int32 transBytes = Socket.EndSend(ar);
                    OnSend(transBytes);
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        public virtual void SendPacket(byte[] source, Int32 offset, Int32 size)
        {
            try
            {
                Socket.BeginSend(source, offset, size, SocketFlags.None, OnSocket_Send, null);
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        public virtual void SendPacket(StreamBuffer source)
        {
            try
            {
                Socket.BeginSend(source.Buffer, 0, source.WrittenBytes, SocketFlags.None, OnSocket_Send, null);
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        protected virtual Boolean IsValidPacket(StreamBuffer buffer, out Int32 packetSize)
        {
            if (buffer.WrittenBytes < 4)
            {
                packetSize = 0;
                return false;
            }

            //  최초 2바이트를 수신할 패킷의 크기로 처리
            packetSize = buffer.GetUInt16();
            return (packetSize > 0 && buffer.WrittenBytes >= packetSize);
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


        protected virtual void OnReceive(StreamBuffer buffer)
        {
        }
    }
}
