using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Aegis.IO;
using Aegis.Threading;



namespace Aegis.Network
{
    public class Session
    {
        private static int NextSessionId = 0;

        /// <summary>
        /// 이 Session 객체의 고유번호입니다.
        /// </summary>
        public int SessionId { get; private set; }
        /// <summary>
        /// 이 Session에서 현재 사용중인 Socket 객체입니다. null일 경우, 네트워킹이 활성화되지 않은 상태입니다.
        /// </summary>
        public Socket Socket { get; private set; }
        /// <summary>
        /// 원격지의 호스트와 통신이 가능한 상태인지 여부를 확인합니다.
        /// </summary>
        public bool Connected { get { return (Socket == null ? false : Socket.Connected); } }


        private ISessionMethod _method;
        public NetworkMethodType MethodType { get; private set; }
        public AwaitableMethod AwaitableMethod { get; private set; }


        public event IOEventHandler EventAccept, EventConnect, EventClose, EventReceive;
        public EventHandler_IsValidPacket PacketValidator;

        internal event Action<Session> Activated, Inactivated;

        private MethodSelector<StreamBuffer> _packetDispatcher;





        public Session()
        {
            Interlocked.Increment(ref NextSessionId);
            SessionId = NextSessionId;


            AwaitableMethod = new AwaitableMethod(this);
            MethodType = NetworkMethodType.AsyncResult;
            _method = new SessionMethodAsyncResult(this);
        }


        public Session(NetworkMethodType methodType)
        {
            Interlocked.Increment(ref NextSessionId);
            SessionId = NextSessionId;


            AwaitableMethod = new AwaitableMethod(this);
            MethodType = methodType;

            if (MethodType == NetworkMethodType.AsyncResult)
                _method = new SessionMethodAsyncResult(this);

            if (MethodType == NetworkMethodType.AsyncEvent)
                _method = new SessionMethodAsyncEvent(this);
        }


        internal void AttachSocket(Socket socket)
        {
            Socket = socket;
        }


        internal void OnSocket_Accepted()
        {
            try
            {
                Activated?.Invoke(this);

                SpinWorker.Dispatch(() =>
                {
                    lock (this)
                    {
                        EventAccept?.Invoke(new IOEventResult(this, IOEventType.Accept, AegisResult.Ok));

                        _method.WaitForReceive();
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
            }
        }


        public void CreatePacketDispatcher(object targetInstance, MethodSelector<StreamBuffer>.MethodSelectHandler handler)
        {
            if (handler == null)
                return;

            _packetDispatcher = new MethodSelector<StreamBuffer>(targetInstance, handler);
        }


        /// <summary>
        /// 서버에 연결을 요청합니다. 연결요청의 결과는 OnConnect를 통해 전달됩니다.
        /// 현재 이 Session이 비활성 상태인 경우에만 수행됩니다.
        /// </summary>
        /// <param name="ipAddress">접속할 서버의 Ip Address</param>
        /// <param name="portNo">접속할 서버의 PortNo</param>
        public virtual void Connect(string ipAddress, int portNo)
        {
            lock (this)
            {
                if (Socket != null)
                    throw new AegisException(AegisResult.ActivatedSession, "This session has already been activated.");


                //  연결 시도
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNo);
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.BeginConnect(ipEndPoint, Socket_Connect, null);
            }
        }


        private void Socket_Connect(IAsyncResult ar)
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
                        Activated?.Invoke(this);


                        SpinWorker.Dispatch(() =>
                        {
                            EventConnect?.Invoke(new IOEventResult(this, IOEventType.Connect, AegisResult.Ok));
                        });

                        _method.WaitForReceive();
                    }
                    else
                    {
                        Socket.Close();
                        Socket = null;

                        SpinWorker.Dispatch(() =>
                        {
                            EventConnect?.Invoke(new IOEventResult(this, IOEventType.Accept, AegisResult.ConnectionFailed));
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
            }
        }


        /// <summary>
        /// 사용중인 리소스를 반환하고 소켓을 종료하여 네트워크 작업을 종료합니다.
        /// 종료 처리가 진행되기 이전에 OnClose 함수가 호출됩니다.
        /// </summary>
        public virtual void Close(int reason = AegisResult.Ok)
        {
            try
            {
                //  작업 중 다른 이벤트가 처리되지 못하도록 Clear까지 lock을 걸어야 한다.
                lock (this)
                {
                    if (Socket == null)
                        return;

                    Socket.LingerState = new LingerOption(true, 3);
                    Socket.Close();
                    Socket = null;


                    Inactivated?.Invoke(this);


                    SpinWorker.Dispatch(() =>
                    {
                        EventClose?.Invoke(new IOEventResult(this, IOEventType.Close, reason));
                    });


                    _method.Clear();
                }
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
            }
        }


        /// <summary>
        /// 패킷을 전송합니다.
        /// </summary>
        /// <param name="buffer">보낼 데이터가 담긴 버퍼</param>
        /// <param name="offset">source에서 전송할 시작 위치</param>
        /// <param name="size">source에서 전송할 크기(Byte)</param>
        /// <param name="onSent">패킷 전송이 완료된 후 호출할 Action</param>
        public virtual void SendPacket(byte[] buffer, int offset, int size, Action<StreamBuffer> onSent = null)
        {
            _method.SendPacket(buffer, offset, size, onSent);
        }


        /// <summary>
        /// 패킷을 전송합니다.
        /// </summary>
        /// <param name="buffer">전송할 데이터가 담긴 StreamBuffer</param>
        /// <param name="onSent">패킷 전송이 완료된 후 호출할 Action</param>
        public virtual void SendPacket(StreamBuffer buffer, Action<StreamBuffer> onSent = null)
        {
            _method.SendPacket(buffer, onSent);
        }


        /// <summary>
        /// 패킷을 전송하고, 특정 패킷이 수신될 경우 dispatcher에 지정된 핸들러를 실행합니다.
        /// 이 기능은 AwaitableMethod보다는 빠르지만, 동시에 많이 호출될 경우 성능이 저하될 수 있습니다.
        /// </summary>
        /// <param name="buffer">전송할 데이터가 담긴 StreamBuffer</param>
        /// <param name="predicate">dispatcher에 지정된 핸들러를 호출할 것인지 여부를 판단하는 함수를 지정합니다.</param>
        /// <param name="dispatcher">실행될 함수를 지정합니다.</param>
        /// <param name="onSent">패킷 전송이 완료된 후 호출할 Action</param>
        public virtual void SendPacket(StreamBuffer buffer, PacketPredicate predicate, IOEventHandler dispatcher, Action<StreamBuffer> onSent = null)
        {
            _method.SendPacket(buffer, predicate, dispatcher, onSent);
        }


        internal void OnReceived(StreamBuffer buffer)
        {
            StreamBuffer dispatchBuffer = new StreamBuffer(buffer);
            SpinWorker.Dispatch(() =>
            {
                if (_packetDispatcher?.Dispatch(dispatchBuffer) == false)
                    EventReceive?.Invoke(new IOEventResult(this, IOEventType.Read, dispatchBuffer.Buffer, AegisResult.Ok));
            });
        }
    }
}
