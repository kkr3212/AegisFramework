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
        private byte[] _tempBuffer = new byte[1024 * 1024];





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
