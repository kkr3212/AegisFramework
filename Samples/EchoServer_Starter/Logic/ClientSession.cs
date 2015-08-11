using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Threading;
using Aegis.Network;



namespace EchoServer.Logic
{
    public class ClientSession : AsyncResultSession
    {
        public static IntervalCounter Counter_ReceiveCount = new IntervalCounter(1000);
        public static IntervalCounter Counter_ReceiveBytes = new IntervalCounter(1000);





        public ClientSession()
            : base(1024 * 1024)
        {
        }


        protected override void OnAccept()
        {
            base.OnAccept();
            Logger.Write(LogType.Info, 2, "[{0}] Accepted", SessionId);


            //  Hello packet을 클라이언트에 전달
            Packet packet = new Packet(0x01);
            SendPacket(packet);
        }


        protected override void OnClose()
        {
            Logger.Write(LogType.Info, 2, "[{0}] Closed", SessionId);
        }


        protected override void OnReceive(StreamBuffer buffer)
        {
            Counter_ReceiveCount.Add(1);
            Counter_ReceiveBytes.Add(buffer.WrittenBytes);


            Packet packet = new Packet(buffer);
            AegisTask.Run(() =>
            {
                packet.SkipHeader();
                switch (packet.PID)
                {
                    case 0x02: OnEcho_Req(packet); break;
                }
            });
        }


        private void OnEcho_Req(Packet packet)
        {
            Packet resPacket = new Packet(0x03);
            SendPacket(resPacket);
        }
    }
}
