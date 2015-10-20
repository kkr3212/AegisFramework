using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Network
{
    internal interface ISessionMethod
    {
        void Clear();
        void WaitForReceive();
        void SendPacket(byte[] buffer, Int32 offset, Int32 size, Action<StreamBuffer> onSent = null);
        void SendPacket(StreamBuffer buffer, Action<StreamBuffer> onSent = null);
        void SendPacket(StreamBuffer buffer, PacketCriterion criterion, EventHandler_Receive dispatcher, Action<StreamBuffer> onSent = null);
    }
}
