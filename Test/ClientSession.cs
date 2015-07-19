using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Network;



namespace Test
{
    public class ClientSession : Session
    {
        public ClientSession()
            : base(4096)
        {
        }


        protected override void OnAccept()
        {
            Logger.Write(LogType.Info, 2, "Accepted");
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


        protected override void OnReceive(int transBytes)
        {
            Logger.Write(LogType.Info, 2, "Received {0} bytes", transBytes);
        }


        protected override bool IsValidPacket(int recvBytes, int headerIndex, out int realPacketSize)
        {
            realPacketSize = recvBytes;
            return true;
        }
    }
}
