using System;
using System.Reflection;



namespace Aegis
{
    public delegate Network.NetworkSession SessionGenerateDelegator();


    public class NetworkSendToken
    {
        public StreamBuffer Buffer { get; private set; }
        public Action<StreamBuffer> ActionOnCompletion { get; private set; }


        public NetworkSendToken(StreamBuffer buffer, Action<StreamBuffer> onCompletion)
        {
            Buffer = buffer;
            ActionOnCompletion = onCompletion;
        }
    }
}
