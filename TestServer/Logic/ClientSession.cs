using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Threading;
using Aegis.Network;



namespace TestServer.Logic
{
    public class ClientSession : Session
    {
        public ClientSession()
            : base(4096)
        {
        }


        protected override void OnAccept()
        {
            Logger.Write(LogType.Info, 2, "[{0}] Accepted", SessionId);
        }


        protected override void OnClose()
        {
            Logger.Write(LogType.Info, 2, "[{0}] Closed", SessionId);
        }


        protected override void OnSend(Int32 transBytes)
        {
            Logger.Write(LogType.Info, 2, "[{0}] Sent {0} bytes", SessionId, transBytes);
        }


        protected override bool IsValidPacket(byte[] receiveBuffer, Int32 receiveBytes, out Int32 packetSize)
        {
            //  수신된 모든 패킷을 정상으로 간주
            packetSize = receiveBytes;
            return true;
        }


        protected override void OnReceive(byte[] receiveBuffer, Int32 packetSize)
        {
            StreamBuffer packet = new StreamBuffer(receiveBuffer, 0, packetSize);
            AegisTask.Run(() => OnDispatchPacket(packet));
        }


        private void OnDispatchPacket(StreamBuffer stream)
        {
            String str = "";
            for (Int32 i = 0; i < stream.WriteIndex; ++i)
                str += String.Format("0x{0:X} ", stream.Buffer[i]);

            Logger.Write(LogType.Info, 2, "[{0}] Received {1} bytes [{2}].", SessionId, stream.WriteIndex, str);
        }
    }
}
