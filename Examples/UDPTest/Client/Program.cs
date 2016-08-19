using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Aegis;
using Aegis.IO;
using Aegis.Network;
using Aegis.Calculate;



namespace UDPTest.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            (new Program()).Start();
            System.Threading.Thread.Sleep(-1);
        }





        private UDPClient _udp;
        private MethodSelector<Packet> _packetDispatcher;
        private IntervalCounter _counter;

        private void Start()
        {
            _packetDispatcher = new MethodSelector<Packet>(this, (ref Packet source, out string key) =>
            {
                key = source.PacketId.ToString();
                source.SkipHeader();
            });


            _udp = new UDPClient();
            _udp.EventRead += NetworkEvent_Receive;
            _udp.EventClose += NetworkEvent_Close;
            _udp.Connect("127.0.0.1", 10201);


            _counter = new IntervalCounter(1000);
            _counter.Start();
            (new IntervalTimer(1000, () =>
            {
                DateTime now = DateTime.Now;
                Console.WriteLine(string.Format("[{0}/{1} {2}:{3}:{4}] recv: {5}",
                                        DateTime.Now.Month, DateTime.Now.Day,
                                        DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second,
                                        _counter.Value));
            })).Start();

            {
                Packet reqPacket = new Packet(Protocol.Echo_Req);
                Send(reqPacket);
            }
        }


        public void Send(Packet packet)
        {
            try
            {
                _udp.Send(packet.Buffer);
            }
            catch (Exception e)
            {
                Logger.Err(e.ToString());
            }
        }


        private void NetworkEvent_Receive(IOEventResult result)
        {
            try
            {
                int packetSize;
                if (Packet.IsValidPacket(result.Buffer, 0, result.Buffer.Length, out packetSize) == true)
                {
                    Packet packet = new Packet(result.Buffer);
                    if (_packetDispatcher.Dispatch(packet) == false)
                        Logger.Err("[GameProcess] Invalid packet received(pid=0x{0:X}).", packet.PacketId);
                }
            }
            catch (Exception e)
            {
                Logger.Err(e.ToString());
            }
        }


        private void NetworkEvent_Close(IOEventResult result)
        {
            EndPoint ep = result.Sender as IPEndPoint;
            Logger.Warn("Client closed.");
        }



        [TargetMethod(Protocol.Echo_Res)]
        private void Echo_Res(Packet data)
        {
            _counter.Add(1);

            Packet reqPacket = new Packet(Protocol.Echo_Req);
            Send(reqPacket);
        }
    }
}
