using System;
using System.Reflection;
using Aegis.Network;



namespace Aegis
{
    public delegate Network.Session SessionGenerateDelegator();
    /// <summary>
    /// 클라이언트의 연결요청에 의해 Session이 활성화된 경우 호출됩니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    public delegate void EventHandler_Accept(Session session);
    /// <summary>
    /// 이 Session 객체가 Connect를 사용하여 서버에 연결요청하면 결과가 전달됩니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    /// <param name="connected">tru인 경우 연결에 성공</param>
    public delegate void EventHandler_Connect(Session session, Boolean connected);
    /// <summary>
    /// 원격지와의 연결이 종료되면 Session 객체를 초기화하기 전에 호출됩니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    public delegate void EventHandler_Close(Session session);
    /// <summary>
    /// 패킷 하나가 완전히 수신되면 이 함수가 호출됩니다.
    /// 전달된 buffer 객체는 현재 Thread에서만 유효하므로, 비동기 작업시에는 새로운 StreamBuffer로 복사해야 합니다.
    /// </summary>
    /// <param name="session">이벤트가 발생된 SessionBase 객체</param>
    /// <param name="buffer">수신된 패킷이 담긴 StreamBuffer</param>
    public delegate void EventHandler_Receive(Session session, StreamBuffer buffer);
    /// <summary>
    /// 수신된 데이터가 유효한 패킷인지 여부를 확인합니다.
    /// 유효한 패킷으로 판단되면 packetSize에 이 패킷의 정확한 크기를 입력하고 true를 반환해야 합니다.
    /// </summary>
    /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
    /// <param name="packetSize">유효한 패킷의 크기</param>
    /// <returns>true를 반환할 경우 유효한 패킷으로 처리합니다.</returns>
    public delegate Boolean EventHandler_IsValidPacket(StreamBuffer buffer, out Int32 packetSize);
    /// <summary>
    /// 수신된 패킷이 지정된 Dispatch를 수행하기에 적합한지 여부를 확인합니다.
    /// 적합할 경우 true를 반환해야 하며, 이 때에는 NetworkEvent_Received에 지정된 핸들러가 호출되지 않습니다.
    /// </summary>
    /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
    /// <returns>true를 반환할 경우 지정된 핸들러가 호출됩니다.</returns>
    public delegate Boolean PacketCriterion(StreamBuffer buffer);


    public enum NetworkMethodType
    {
        /// <summary>
        /// Begin 계열의 Socket API를 사용하여 원격지의 호스트와 네트워킹을 할 수 있는 기능을 제공합니다.
        /// </summary>
        AsyncResult,

        /// <summary>
        /// Async 계열의 Socket API를 사용하여 원격지의 호스트와 네트워킹을 할 수 있는 기능을 제공합니다.
        /// </summary>
        AsyncEvent
    }


    internal class NetworkSendToken
    {
        public StreamBuffer Buffer { get; }
        private Action<StreamBuffer> _actionOnCompletion;





        public NetworkSendToken(StreamBuffer buffer, Action<StreamBuffer> onCompletion)
        {
            Buffer = buffer;
            _actionOnCompletion = onCompletion;
        }


        public void CompletionAction()
        {
            SpinWorker.Dispatch(() =>
            {
                if (_actionOnCompletion != null)
                    _actionOnCompletion(Buffer);
            });
        }
    }
}
