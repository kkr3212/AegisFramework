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
    public class ClientSession : AsyncEventSession
    {
        public static IntervalCounter Counter_ReceiveCount = new IntervalCounter(1000);
        public static IntervalCounter Counter_ReceiveBytes = new IntervalCounter(1000);





        public ClientSession()
            : base(1024 * 1024)
        {
            base.NetworkEvent_Accepted += OnAcceptd;
            base.NetworkEvent_Closed += OnClosed;
            base.NetworkEvent_Received += OnReceived;
            base.PacketValidator += IsValidPacket;
        }


        private Boolean IsValidPacket(NetworkSession session, StreamBuffer buffer, out int packetSize)
        {
            if (buffer.WrittenBytes < 4)
            {
                packetSize = 0;
                return false;
            }

            //  최초 2바이트를 수신할 패킷의 크기로 처리
            packetSize = buffer.GetUInt16();
            return (packetSize > 0 && buffer.WrittenBytes >= packetSize);
        }


        private void OnAcceptd(NetworkSession session)
        {
            Logger.Write(LogType.Info, 2, "[{0}] Accepted", SessionId);


            //  Hello packet을 클라이언트에 전달
            Packet packet = new Packet(0x01);
            SendPacket(packet);
        }


        private void OnClosed(NetworkSession session)
        {
            Logger.Write(LogType.Info, 2, "[{0}] Closed", SessionId);
        }


        private void OnReceived(NetworkSession session, StreamBuffer buffer)
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
