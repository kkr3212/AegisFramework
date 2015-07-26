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
        public IntervalCounter Counter_ReceiveCount { get; private set; }
        public IntervalCounter Counter_ReceiveBytes { get; private set; }





        public ClientSession()
        {
            Counter_ReceiveCount = new IntervalCounter(1000);
            Counter_ReceiveBytes = new IntervalCounter(1000);
        }


        protected override void OnAccept()
        {
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
            Int32 var1 = packet.GetInt32();
            String var2 = packet.GetStringFromUtf16();
            Int16 var3 = packet.GetInt16();
            String var4 = packet.GetStringFromUtf8();
            Double var5 = packet.GetDouble();

            Packet resPacket = new Packet(0x03);
            resPacket.PutInt32(var1);
            resPacket.PutStringAsUtf16(var2);
            resPacket.PutInt16(var3);
            resPacket.PutStringAsUtf8(var4);
            resPacket.PutDouble(var5);
            SendPacket(resPacket);
        }


        /*
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

            SendPacket(stream);
        }
        */
    }
}
