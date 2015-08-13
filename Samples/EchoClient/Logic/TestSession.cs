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
    public class TestSession : AsyncEventSession
    {
        private byte[] _tempBuffer = new byte[1024 * 1024];





        public TestSession()
            : base(4096)
        {
            base.OnConnect += OnEvent_Connect;
            base.OnClose += OnEvent_Close;
            base.OnReceive += OnEvent_Receive;
            base.PacketValidator += IsValidPacket;


            Connect("192.168.0.100", 10100);
        }


        private Boolean IsValidPacket(SessionBase session, StreamBuffer buffer, out int packetSize)
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


        private void OnEvent_Connect(SessionBase session, Boolean connected)
        {
            if (connected == true)
                Logger.Write(LogType.Info, 2, "[{0}] Connected", SessionId);
            else
                Connect("192.168.0.100", 10100);
        }


        private void OnEvent_Close(SessionBase session)
        {
            Logger.Write(LogType.Info, 2, "[{0}] Closed", SessionId);
        }


        private void OnEvent_Receive(SessionBase session, StreamBuffer buffer)
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
            reqPacket.Write(_tempBuffer, 0, FormMain.BufferSize);
            SendPacket(reqPacket);
        }


        private void OnEcho_Res(Packet packet)
        {
            Packet reqPacket = new Packet(0x02);
            reqPacket.Write(_tempBuffer, 0, FormMain.BufferSize);
            SendPacket(reqPacket);
        }
    }
}
