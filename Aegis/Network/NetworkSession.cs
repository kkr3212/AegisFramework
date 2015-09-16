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
    /// 클라이언트의 연결요청에 의해 Session이 활성화된 경우 호출됩니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    public delegate void EventHandler_Accept(NetworkSession session);
    /// <summary>
    /// 이 Session 객체가 Connect를 사용하여 서버에 연결요청하면 결과가 전달됩니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    /// <param name="connected">tru인 경우 연결에 성공</param>
    public delegate void EventHandler_Connect(NetworkSession session, Boolean connected);
    /// <summary>
    /// 원격지와의 연결이 종료되면 Session 객체를 초기화하기 전에 호출됩니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    public delegate void EventHandler_Close(NetworkSession session);
    /// <summary>
    /// 패킷 전송에 성공하면 전송된 크기를 전달합니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    /// <param name="transBytes">전송된 Bytes</param>
    public delegate void EventHandler_Send(NetworkSession session, Int32 transBytes);
    /// <summary>
    /// 패킷 하나가 완전히 수신되면 이 함수가 호출됩니다.
    /// 전달된 buffer 객체는 현재 Thread에서만 유효하므로, 비동기 작업시에는 새로운 StreamBuffer로 복사해야 합니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    /// <param name="buffer">수신된 패킷이 담긴 StreamBuffer</param>
    public delegate void EventHandler_Receive(NetworkSession session, StreamBuffer buffer);
    /// <summary>
    /// 수신된 데이터가 유효한 패킷인지 여부를 확인합니다.
    /// 유효한 패킷으로 판단되면 packetSize에 이 패킷의 정확한 크기를 입력하고 true를 반환해야 합니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
    /// <param name="packetSize">유효한 패킷의 크기</param>
    /// <returns>true를 반환할 경우 유효한 패킷으로 처리합니다.</returns>
    public delegate Boolean EventHandler_IsValidPacket(NetworkSession session, StreamBuffer buffer, out Int32 packetSize);
    /// <summary>
    /// 수신된 패킷이 지정된 Dispatch를 수행하기에 적합한지 여부를 확인합니다.
    /// 적합할 경우 true를 반환해야 하며, 이 때에는 NetworkEvent_Received에 지정된 핸들러가 호출되지 않습니다.
    /// </summary>
    /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
    /// <returns>true를 반환할 경우 지정된 핸들러가 호출됩니다.</returns>
    public delegate Boolean PacketDeterminator(StreamBuffer buffer);





    public abstract class NetworkSession
    {
        private static Int32 NextSessionId = 0;

        internal SessionManager SessionManager { get; set; }
        /// <summary>
        /// 이 Session 객체의 고유번호입니다.
        /// </summary>
        public Int32 SessionId { get; private set; }
        /// <summary>
        /// 이 Session에서 현재 사용중인 Socket 객체입니다. null일 경우, 네트워킹이 활성화되지 않은 상태입니다.
        /// </summary>
        public Socket Socket { get; private set; }
        /// <summary>
        /// 원격지의 호스트와 통신이 가능한 상태인지 여부를 확인합니다.
        /// </summary>
        public Boolean Connected { get { return (Socket == null ? false : Socket.Connected); } }


        public event EventHandler_Accept NetworkEvent_Accepted;
        public event EventHandler_Connect NetworkEvent_Connected;
        public event EventHandler_Close NetworkEvent_Closed;





        protected NetworkSession()
        {
            Interlocked.Increment(ref NextSessionId);
            SessionId = NextSessionId;
        }


        public abstract void SetReceiveBufferSize(Int32 recvBufferSize);
        internal abstract void WaitForReceive();
        public abstract void SendPacket(byte[] buffer, Int32 offset, Int32 size, Action<StreamBuffer> onSent = null);
        public abstract void SendPacket(StreamBuffer buffer, Action<StreamBuffer> onSent = null);
        public abstract void SendPacket(StreamBuffer buffer, PacketDeterminator determinator, EventHandler_Receive dispatcher, Action<StreamBuffer> onSent = null);


        internal void AttachSocket(Socket socket)
        {
            Socket = socket;
        }


        internal void OnSocket_Accepted()
        {
            try
            {
                if (SessionManager != null)
                    SessionManager.ActivateSession(this);

                lock (this)
                {
                    if (NetworkEvent_Accepted != null)
                        NetworkEvent_Accepted(this);

                    WaitForReceive();
                }
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        /// <summary>
        /// 서버에 연결을 요청합니다. 연결요청의 결과는 OnConnect를 통해 전달됩니다.
        /// 현재 이 Session이 비활성 상태인 경우에만 수행됩니다.
        /// </summary>
        /// <param name="ipAddress">접속할 서버의 Ip Address</param>
        /// <param name="portNo">접속할 서버의 PortNo</param>
        public virtual void Connect(String ipAddress, Int32 portNo)
        {
            lock (this)
            {
                if (Socket != null)
                    throw new AegisException(AegisResult.ActivatedSession, "This session has already been activated.");


                //  연결 시도
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNo);
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.BeginConnect(ipEndPoint, OnSocket_Connect, null);
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

                        if (NetworkEvent_Connected != null)
                            NetworkEvent_Connected(this, true);

                        WaitForReceive();
                    }
                    else
                    {
                        Socket.Close();
                        Socket = null;

                        if (NetworkEvent_Connected != null)
                            NetworkEvent_Connected(this, false);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        /// <summary>
        /// 사용중인 리소스를 반환하고 소켓을 종료하여 네트워크 작업을 종료합니다.
        /// 종료 처리가 진행되기 이전에 OnClose 함수가 호출됩니다.
        /// </summary>
        public virtual void Close()
        {
            try
            {
                //  작업 중 다른 이벤트가 처리되지 못하도록 Clear까지 lock을 걸어야 한다.
                lock (this)
                {
                    if (Socket == null)
                        return;

                    //  #!  Linger Option은 Connection Stress Test 가 필요
                    Socket.LingerState = new LingerOption(true, 3);
                    Socket.Close();
                    Socket = null;

                    if (NetworkEvent_Closed != null)
                        NetworkEvent_Closed(this);
                }
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }


            if (SessionManager != null)
                SessionManager.InactivateSession(this);
        }
    }
}
