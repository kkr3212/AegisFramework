using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Network;



namespace Test
{
    public class ServerSession : Session
    {
        public ServerSession()
            : base(4096)
        {
            Connect("192.168.0.100", 20100);
        }


        protected override void OnConnect(bool connected)
        {
            if (connected == true)
                Logger.Write(LogType.Info, 2, "Connected");
        }


        protected override void OnClose()
        {
            Logger.Write(LogType.Info, 2, "Closed");
        }


        protected override void OnSend(int transBytes)
        {
            Logger.Write(LogType.Info, 2, "Sent {0} bytes", transBytes);
        }


        protected override void OnReceive(Int32 receivedPacketSize)
        {
            String str = "";
            for (Int32 i = 0; i < receivedPacketSize; ++i)
                str += String.Format("0x{0:X} ", ReceivedBuffer[i]);

            Logger.Write(LogType.Info, 2, "Received {0} bytes [{1}].", receivedPacketSize, str);
        }


        protected override bool IsValidPacket(int recvBytes, int headerIndex, out int realPacketSize)
        {
            realPacketSize = recvBytes;
            return true;
        }
    }
}
