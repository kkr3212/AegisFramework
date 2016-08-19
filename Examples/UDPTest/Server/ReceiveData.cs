using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Aegis.Network;



namespace UDPTest
{
    public class ReceiveData
    {
        public readonly EndPoint Sender;
        public readonly Packet Packet;


        public ReceiveData(EndPoint sender, Packet packet)
        {
            Sender = sender;
            Packet = packet;
        }
    }
}
