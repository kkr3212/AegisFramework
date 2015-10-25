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
    public class ClientSession : Session
    {
        public static IntervalCounter Counter_ReceiveCount = new IntervalCounter(1000);
        public static IntervalCounter Counter_ReceiveBytes = new IntervalCounter(1000);





        public ClientSession()
        {
            base.NetworkEvent_Accepted += OnAccepted;
            base.NetworkEvent_Closed += OnClosed;
            base.NetworkEvent_Received += OnReceived;
            base.PacketValidator += Packet.IsValidPacket;
        }


        private void OnAccepted(Session session)
        {
            Logger.Write(LogType.Info, 2, "[{0}] Accepted", SessionId);


            //  Hello packet을 클라이언트에 전달
            Packet packet = new Packet(0x01);
            SendPacket(packet);
        }


        private void OnClosed(Session session)
        {
            Logger.Write(LogType.Info, 2, "[{0}] Closed", SessionId);
        }


        private void OnReceived(Session session, StreamBuffer buffer)
        {
            Counter_ReceiveCount.Add(1);
            Counter_ReceiveBytes.Add(buffer.WrittenBytes);


            Packet packet = new Packet(buffer);
            packet.SkipHeader();
            switch (packet.PacketId)
            {
                case 0x02: OnEcho_Req(packet); break;
            }
        }


        private void OnEcho_Req(Packet packet)
        {
            Packet resPacket = new Packet(0x03);
            SendPacket(resPacket);
        }
    }
}
