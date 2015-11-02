using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aegis.Client.Network;



namespace Aegis.Client
{
    public enum NetworkStatus
    {
        Connected = 1,
        ConnectionFailed,
        Disconnected,
        SessionForceClosed
    }
    public delegate void NetworkStatusHandler(NetworkStatus status);
    public delegate Boolean PacketHandler(SecurePacket packet);





    public partial class Request
    {
        private AegisClient _aegisClient = new AegisClient();
        private Queue<SecurePacket> _queueSendPacket = new Queue<SecurePacket>();
        private Queue<SecurePacket> _queueReceivedPacket = new Queue<SecurePacket>();
        private CallbackQueue _callbackQueue = new CallbackQueue();

        private Int32 _nextSeqNo;

        public event NetworkStatusHandler NetworkStatusChanged;
        public event PacketHandler PacketPreprocessing, PacketSending;


        public String HostAddress
        {
            get { return _aegisClient.HostAddress; }
            set { _aegisClient.HostAddress = value; }
        }
        public Int32 HostPortNo
        {
            get { return _aegisClient.HostPortNo; }
            set { _aegisClient.HostPortNo = value; }
        }
        public String AESIV { get; set; }
        public String AESKey { get; set; }
        public Boolean EnableSend
        {
            get { return _aegisClient.EnableSend; }
            set { _aegisClient.EnableSend = value; }
        }
        public Int32 ConnectionAliveTime
        {
            get { return _aegisClient.ConnectionAliveTime; }
            set { _aegisClient.ConnectionAliveTime = value; }
        }
        public ConnectionStatus ConnectionStatus { get { return _aegisClient.ConnectionStatus; } }





        public void Initialize()
        {
            _aegisClient.NetworkEvent_Connected += OnConnect;
            _aegisClient.NetworkEvent_Disconnected += OnDisconnect;
            _aegisClient.NetworkEvent_Received += OnReceive;
            _aegisClient.PacketValidator = IsValidPacket;
            _aegisClient.Initialize();

            _nextSeqNo = 1;
        }


        public void Release()
        {
            lock (_aegisClient)
            {
                _aegisClient.EnableSend = false;
                _aegisClient.NetworkEvent_Connected += OnConnect;
                _aegisClient.NetworkEvent_Disconnected += OnDisconnect;
                _aegisClient.NetworkEvent_Received += OnReceive;

                _aegisClient.Release();
                _callbackQueue.Clear();
                _queueSendPacket.Clear();

                PacketPreprocessing = null;
                PacketSending = null;
            }
        }


        public void Disconnect(Action actionOnClosed = null)
        {
            _aegisClient.Close(actionOnClosed);
        }


        public void Update()
        {
            ProcessSendQueue();
            _callbackQueue.DoCallback();
        }


        private void ProcessSendQueue()
        {
            lock (_aegisClient)
            {
                if (_queueSendPacket.Count() == 0)
                    return;

                else if (_aegisClient.ConnectionStatus == ConnectionStatus.Closed)
                    _aegisClient.Connect();

                else if (_aegisClient.ConnectionStatus == ConnectionStatus.Connected &&
                         _aegisClient.EnableSend == true)
                {
                    SecurePacket packet = _queueSendPacket.Peek();
                    if (PacketSending == null ||
                        PacketSending(packet) == true)
                    {
                        _queueSendPacket.Dequeue();
                        packet.Encrypt(AESIV, AESKey);
                        _aegisClient.SendPacket(packet);
                    }
                }
            }
        }


        private void OnNetworkStatusChanged(NetworkStatus status)
        {
            if (NetworkStatusChanged != null)
                NetworkStatusChanged(status);
        }


        private bool IsValidPacket(AegisClient ac, StreamBuffer buffer, out Int32 packetSize)
        {
            if (buffer.WrittenBytes < 8)
            {
                packetSize = 0;
                return false;
            }

            packetSize = buffer.GetUInt16();
            return (packetSize > 0 && buffer.WrittenBytes >= packetSize);
        }


        private void OnConnect(AegisClient ac, bool connected)
        {
            if (connected == true)
                OnNetworkStatusChanged(NetworkStatus.Connected);
            else
                OnNetworkStatusChanged(NetworkStatus.ConnectionFailed);
        }


        private void OnDisconnect(AegisClient ac)
        {
            _aegisClient.EnableSend = false;
            OnNetworkStatusChanged(NetworkStatus.Disconnected);
        }


        private void OnReceive(AegisClient ac, StreamBuffer buffer)
        {
            SecurePacket packet = new SecurePacket(buffer);
            packet.Decrypt(AESIV, AESKey);
            packet.SkipHeader();

            if (PacketPreprocessing != null &&
                PacketPreprocessing(packet) == false)
            {
                _callbackQueue.AddPacket(packet);
            }
        }


        public void SendPacket(SecurePacket packet, Action<SecurePacket> responseAction)
        {
            lock (_aegisClient)
            {
                packet.SeqNo = _nextSeqNo++;
                if (_nextSeqNo == Int32.MaxValue)
                    _nextSeqNo = 0;

                _queueSendPacket.Enqueue(packet);
                _callbackQueue.AddCallback(packet.SeqNo, responseAction);
            }
        }
    }
}
