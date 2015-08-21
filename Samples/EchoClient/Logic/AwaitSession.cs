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
    public class AwaitSession : AsyncResultSession
    {
        private byte[] _tempBuffer = new byte[1024 * 1024];





        public AwaitSession()
        {
            base.NetworkEvent_Connected += OnConnected;
            base.NetworkEvent_Closed += OnClosed;
            base.NetworkEvent_Received += OnReceived;
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


        private void OnConnected(SessionBase session, Boolean connected)
        {
            if (connected == true)
                Logger.Write(LogType.Info, 2, "[{0}] Connected", SessionId);
            else
                Connect("192.168.0.100", 10100);
        }


        private void OnClosed(SessionBase session)
        {
            Logger.Write(LogType.Info, 2, "[{0}] Closed", SessionId);
        }


        private void OnReceived(SessionBase session, StreamBuffer buffer)
        {
            Packet packet = new Packet(buffer);
            AwaitableMethod.ProcessResponseWaitPacket(packet);

            AegisTask.Run(async () =>
            {
                Packet reqPacket = new Packet(0x02);
                reqPacket.Write(_tempBuffer, 0, FormMain.BufferSize);
                await AwaitableMethod.SendAndWaitResponse(reqPacket, 0x03);
            });
        }
    }
}
