using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aegis.Client.Network;



namespace Aegis.Client
{
    public class Response
    {
        public readonly SecurePacket Packet;
        public readonly Int32 ResultCodeNo;





        public Response(SecurePacket packet)
        {
            Packet = packet;
            Packet.SkipHeader();
            ResultCodeNo = packet.GetInt32();
        }


        public Response(Int32 resultCodeNo)
        {
            ResultCodeNo = resultCodeNo;
        }
    }
}
