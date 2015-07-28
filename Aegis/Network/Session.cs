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
    /// <summary>
    /// 원격지의 호스트와 네트워킹을 할 수 있는 기능을 제공합니다.
    /// </summary>
    public class Session
    {
        private static Int32 NextSessionId = 0;

        /// <summary>
        /// 이 Session 객체의 고유번호입니다.
        /// </summary>
        public Int32 SessionId { get; private set; }
        /// <summary>
        /// 이 Session에서 현재 사용중인 Socket 객체입니다. null일 경우, 네트워킹이 활성화되지 않은 상태입니다.
        /// </summary>
        public Socket Socket { get; internal set; }
        /// <summary>
        /// 원격지의 호스트와 통신이 가능한 상태인지 여부를 확인합니다.
        /// </summary>
        public Boolean IsConnected { get { return (Socket == null ? false : Socket.Connected); } }

        internal SessionManager SessionManager { get; set; }
        private StreamBuffer _receivedBuffer, _dispatchBuffer;





        /// <summary>
        /// 수신버퍼의 크기는 StreamBuffer의 기본할당크기로 초기화됩니다.
        /// </summary>
        public Session()
        {
            Interlocked.Increment(ref NextSessionId);

            SessionId = NextSessionId;
            _receivedBuffer = new StreamBuffer();
            _dispatchBuffer = new StreamBuffer();

            Clear();
        }


        /// <summary>
        /// 수신버퍼의 크기를 지정하여 Session 객체를 생성합니다. 수신버퍼의 크기는 패킷 하나의 크기 이상으로 설정하는 것이 좋습니다.
        /// </summary>
        /// <param name="recvBufferSize">수신버퍼의 크기(Byte)</param>
        public Session(Int32 recvBufferSize)
        {
            Interlocked.Increment(ref NextSessionId);

            SessionId = NextSessionId;
            _receivedBuffer = new StreamBuffer(recvBufferSize);
            _dispatchBuffer = new StreamBuffer();

            Clear();
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


        /// <summary>
        /// 서버에 연결을 요청합니다. 연결요청의 결과는 OnConnect 함수를 통해 전달됩니다.
        /// 현재 이 Session이 비활성 상태인 경우에만 수행됩니다.
        /// </summary>
        /// <param name="ipAddress">접속할 서버의 Ip Address</param>
        /// <param name="portNo">접속할 서버의 PortNo</param>
        public void Connect(String ipAddress, Int32 portNo)
        {
            lock (this)
            {
                if (Socket != null)
                    throw new AegisException(ResultCode.ActivatedSession, "Sessions is already active.");


                //  연결 시도
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNo);
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.BeginConnect(ipEndPoint, OnSocket_Connect, null);
            }
        }


        /// <summary>
        /// 사용중인 리소스를 반환하고 소켓을 종료하여 네트워크 작업을 종료합니다.
        /// 종료 처리가 진행되기 이전에 OnClose 함수가 호출됩니다.
        /// </summary>
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


        /// <summary>
        /// 패킷을 전송합니다. 패킷이 전송되면 OnSend함수가 호출됩니다.
        /// </summary>
        /// <param name="source">보낼 데이터가 담긴 버퍼</param>
        /// <param name="offset">source에서 전송할 시작 위치</param>
        /// <param name="size">source에서 전송할 크기(Byte)</param>
        public virtual void SendPacket(byte[] source, Int32 offset, Int32 size)
        {
            try
            {
                lock (this)
                {
                    if (Socket != null)
                        Socket.BeginSend(source, offset, size, SocketFlags.None, OnSocket_Send, null);
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


        /// <summary>
        /// 패킷을 전송합니다. 패킷이 전송되면 OnSend함수가 호출됩니다.
        /// </summary>
        /// <param name="source">전송할 데이터가 담긴 StreamBuffer</param>
        public virtual void SendPacket(StreamBuffer source)
        {
            try
            {
                lock (this)
                {
                    if (Socket != null)
                        Socket.BeginSend(source.Buffer, 0, source.WrittenBytes, SocketFlags.None, OnSocket_Send, null);
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


        /// <summary>
        /// 수신된 데이터가 유효한 패킷인지 여부를 확인합니다.
        /// 유효한 패킷으로 판단되면 packetSize에 이 패킷의 정확한 크기를 입력하고 true를 반환해야 합니다.
        /// </summary>
        /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
        /// <param name="packetSize">유효한 패킷의 크기</param>
        /// <returns>true를 반환하면 OnReceive함수를 통해 수신된 데이터가 전달됩니다.</returns>
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


        /// <summary>
        /// 클라이언트의 연결요청에 의해 Session이 활성화된 경우 이 함수가 호출됩니다.
        /// </summary>
        protected virtual void OnAccept()
        {
        }


        /// <summary>
        /// 이 Session 객체가 Connect를 사용하여 서버에 연결요청하면 결과가 이 함수로 전달됩니다.
        /// </summary>
        /// <param name="connected">true인 경우 연결에 성공한 상태입니다.</param>
        protected virtual void OnConnect(Boolean connected)
        {
        }


        /// <summary>
        /// 원격지와의 연결이 종료되면 Session 객체를 초기화하기 전에 이 함수가 호출됩니다.
        /// </summary>
        protected virtual void OnClose()
        {
        }


        /// <summary>
        /// 패킷 전송에 성공하면 전송된 크기를 전달합니다.
        /// </summary>
        /// <param name="transBytes">전송된 Bytes</param>
        protected virtual void OnSend(Int32 transBytes)
        {
        }


        /// <summary>
        /// 패킷 하나가 완전히 수신되면 이 함수가 호출됩니다.
        /// 전달된 buffer 객체는 현재 Thread에서만 유효합니다.
        /// </summary>
        /// <param name="buffer">수신된 패킷이 담긴 StreamBuffer</param>
        protected virtual void OnReceive(StreamBuffer buffer)
        {
        }
    }
}
