using System;
using Aegis.IO;
using Aegis.Network;
using Aegis.Threading;



namespace Aegis
{
    public delegate Session SessionGenerateHandler();
    /// <summary>
    /// 수신된 데이터가 유효한 패킷인지 여부를 확인합니다.
    /// 유효한 패킷으로 판단되면 packetSize에 이 패킷의 정확한 크기를 입력하고 true를 반환해야 합니다.
    /// </summary>
    /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
    /// <param name="packetSize">유효한 패킷의 크기</param>
    /// <returns>true를 반환할 경우 유효한 패킷으로 처리합니다.</returns>
    public delegate bool EventHandler_IsValidPacket(StreamBuffer buffer, out int packetSize);
    /// <summary>
    /// 수신된 패킷이 지정된 Dispatch를 수행하기에 적합한지 여부를 확인합니다.
    /// 적합할 경우 true를 반환해야 하며, 이 때에는 EventReceive에 지정된 핸들러가 호출되지 않습니다.
    /// </summary>
    /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
    /// <returns>true를 반환할 경우 지정된 핸들러가 호출됩니다.</returns>
    public delegate bool PacketPredicate(StreamBuffer buffer);





    public enum NetworkMethodType
    {
        /// <summary>
        /// Begin 계열의 Socket API를 사용하여 원격지의 호스트와 네트워킹을 할 수 있는 기능을 제공합니다.
        /// 성능상 AsyncEvent에 비해 조금 더 유리합니다.
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





        public NetworkSendToken(StreamBuffer buffer, Action<StreamBuffer> completion)
        {
            Buffer = buffer;
            _actionOnCompletion = completion;
        }


        public void CompletionAction()
        {
            SpinWorker.Dispatch(() =>
            {
                _actionOnCompletion?.Invoke(Buffer);
            });
        }
    }
}
