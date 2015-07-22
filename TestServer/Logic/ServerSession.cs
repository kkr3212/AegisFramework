using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Network;



namespace TestServer.Logic
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


        protected override bool IsValidPacket(byte[] receiveBuffer, Int32 receiveBytes, out Int32 packetSize)
        {
            //  수신된 모든 패킷을 정상으로 간주
            packetSize = receiveBytes;
            return true;
        }


        protected override void OnReceive(byte[] receiveBuffer, Int32 packetSize)
        {
        }
    }
}
