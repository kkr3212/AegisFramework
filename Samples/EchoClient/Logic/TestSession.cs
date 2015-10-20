using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Threading;
using Aegis.Network;



namespace EchoClient.Logic
{
    public class TestSession : Session
    {
        private byte[] _tempBuffer = new byte[1024 * 1024];





        public TestSession()
        {
            base.NetworkEvent_Connected += OnConnected;
            base.NetworkEvent_Closed += OnClosed;
            base.NetworkEvent_Received += OnReceived;
            base.PacketValidator += Packet.IsValidPacket;


            Connect("192.168.0.100", 10100);
        }


        private void OnConnected(Session session, Boolean connected)
        {
            if (connected == true)
                Logger.Write(LogType.Info, 2, "[{0}] Connected", SessionId);
            else
                Connect("192.168.0.100", 10100);
        }


        private void OnClosed(Session session)
        {
            Logger.Write(LogType.Info, 2, "[{0}] Closed", SessionId);
        }


        private void OnReceived(Session session, StreamBuffer buffer)
        {
            Packet packet = new Packet(buffer);
            packet.SkipHeader();
            switch (packet.PacketId)
            {
                case 0x01: OnHello(packet); break;
                case 0x03: OnEcho_Res(packet); break;
            }
        }


        private void OnHello(Packet packet)
        {
            Packet reqPacket = new Packet(0x02);
            reqPacket.Write(_tempBuffer, 0, FormMain.BufferSize);
            SendPacket(reqPacket);
        }


        private void OnEcho_Res(Packet packet)
        {
            Packet reqPacket = new Packet(0x02);
            reqPacket.Write(_tempBuffer, 0, FormMain.BufferSize);


            SendPacket(reqPacket,
                        (buffer) => { return Packet.GetPacketId(buffer.Buffer) == 0x03; },
                        (session, buffer) =>
                        {
                            packet.SkipHeader();
                            OnEcho_Res(new Packet(buffer));
                        }
                );
        }
    }
}
