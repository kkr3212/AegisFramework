using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis.Client;



namespace EchoClient
{
    public class Session
    {
        private AegisClient _aegisClient = new AegisClient();
        private byte[] _tempBuffer = new byte[1024 * 1024];





        public Session()
        {
            _aegisClient.OnConnect += OnConnect;
            _aegisClient.OnDisconnect += OnDisconnect;
            _aegisClient.OnSend += OnSend;
            _aegisClient.OnReceive += OnReceive;
            _aegisClient.ValidPacketHandler = IsValidPacket;
            _aegisClient.Initialize();
        }


        public void Connect()
        {
            _aegisClient.HostAddress = "192.168.0.100";
            _aegisClient.HostPortNo = 10100;
            _aegisClient.Connect();
        }


        public void Close()
        {
            _aegisClient.Close();
        }


        public void Release()
        {
            _aegisClient.Release();
        }


        private Boolean IsValidPacket(StreamBuffer buffer, out Int32 packetSize)
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


        private void OnConnect(bool connected)
        {
            if (connected == true)
                FormMain.Log("Connected.");

            else
                FormMain.Log("Connection failed.");
        }


        private void OnDisconnect()
        {
            FormMain.Log("Connection closed.");
        }


        private void OnSend(Int32 transBytes)
        {
        }


        private void OnReceive(StreamBuffer buffer)
        {
            Packet packet = new Packet(buffer);
            Task.Run(() =>
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
            reqPacket.Write(_tempBuffer, 0, 127);
            _aegisClient.SendPacket(reqPacket);
        }


        private void OnEcho_Res(Packet packet)
        {
            Packet reqPacket = new Packet(0x02);
            reqPacket.Write(_tempBuffer, 0, 127);
            _aegisClient.SendPacket(reqPacket);
        }
    }
}
