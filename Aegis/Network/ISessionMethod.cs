using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis.IO;



namespace Aegis.Network
{
    internal interface ISessionMethod
    {
        void Clear();
        void WaitForReceive();
        void SendPacket(byte[] buffer, int offset, int size, Action<StreamBuffer> onSent = null);
        void SendPacket(StreamBuffer buffer, Action<StreamBuffer> onSent = null);
        void SendPacket(StreamBuffer buffer, PacketPredicate predicate, IOEventHandler dispatcher, Action<StreamBuffer> onSent = null);
    }
}
