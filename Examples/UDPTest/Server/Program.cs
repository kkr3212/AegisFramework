using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Aegis;
using Aegis.IO;
using Aegis.Network;
using Aegis.Calculate;



namespace UDPTest.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            (new Program()).Start();
            System.Threading.Thread.Sleep(-1);
        }





        private UDPServer _udp;
        private MethodSelector<ReceiveData> _packetDispatcher;
        private IntervalCounter _counter;

        private void Start()
        {
            _packetDispatcher = new MethodSelector<ReceiveData>(this, (ref ReceiveData source, out string key) =>
            {
                key = source.Packet.PacketId.ToString();
                source.Packet.SkipHeader();
            });


            _udp = new UDPServer();
            _udp.EventRead += NetworkEvent_Receive;
            _udp.EventClose += NetworkEvent_Close;
            _udp.Bind("127.0.0.1", 10201);


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
        }


        public void Send(EndPoint ep, Packet packet)
        {
            try
            {
                _udp.Send(ep, packet.Buffer);
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
                    if (_packetDispatcher.Dispatch(new ReceiveData(result.Sender as EndPoint, packet)) == false)
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



        [TargetMethod(Protocol.Echo_Req)]
        private void Echo_Req(ReceiveData data)
        {
            _counter.Add(1);

            Packet resPacket = new Packet(Protocol.Echo_Res);
            Send(data.Sender as EndPoint, resPacket);
        }
    }
}
