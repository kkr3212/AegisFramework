using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Threading;
using Aegis.Network;



namespace TestClient.Logic
{
    public class ServerSession : Session
    {
        public ServerSession()
            : base(4096)
        {
            Connect("192.168.0.100", 10100);
        }


        protected override void OnConnect(bool connected)
        {
            if (connected == true)
                Logger.Write(LogType.Info, 2, "[{0}] Connected", SessionId);
            else
                Connect("192.168.0.100", 10100);
        }


        protected override void OnClose()
        {
            Logger.Write(LogType.Info, 2, "[{0}] Closed", SessionId);
        }


        protected override void OnReceive(StreamBuffer buffer)
        {
            Packet packet = new Packet(buffer);
            AegisTask.Run(() =>
            {
                packet.SkipHeader();
                switch (packet.PID)
                {
                    case 0x01: OnHello(packet); break;
                    case 0x03: OnEcho_Res(packet); break;
                }
            });
        }


        private void OnHello(Packet packet)
        {
            Packet reqPacket = new Packet(0x02);
            reqPacket.PutInt32(1234);
            reqPacket.PutStringAsUtf16("UTF16 String 이지스 네트워크 !@#$◎");
            reqPacket.PutInt16(5678);
            reqPacket.PutStringAsUtf8("UTF8 String 이지스 네트워크 !@#$◎");
            reqPacket.PutDouble(1234.5678);

            SendPacket(reqPacket);
        }


        private void OnEcho_Res(Packet packet)
        {
            Int32 var1 = packet.GetInt32();
            String var2 = packet.GetStringFromUtf16();
            Int16 var3 = packet.GetInt16();
            String var4 = packet.GetStringFromUtf8();
            Double var5 = packet.GetDouble();


            Packet reqPacket = new Packet(0x02);
            reqPacket.PutInt32(1234);
            reqPacket.PutStringAsUtf16("UTF16 String 이지스 네트워크 !@#$◎");
            reqPacket.PutInt16(5678);
            reqPacket.PutStringAsUtf8("UTF8 String 이지스 네트워크 !@#$◎");
            reqPacket.PutDouble(1234.5678);

            SendPacket(reqPacket);
        }
    }
}
