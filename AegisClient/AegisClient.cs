using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Diagnostics;



namespace Aegis.Client
{
    public partial class AegisClient
    {
        private String _hostAddress;
        private Boolean _isRunning;
        private Connector _connector;
        private Stopwatch _stampLastAction;


        public event Event_Connect OnConnect;
        public event Event_Disconnect OnDisconnect;
        public event Event_Send OnSend;
        public event Event_Receive OnReceive;
        public ValidPacketHandler ValidPacketHandler;


        public String HostAddress
        {
            get { return _hostAddress; }
            set
            {
                IPAddress[] ipAddrList = Dns.GetHostAddresses(value);
                if (ipAddrList.Count() > 0)
                    _hostAddress = ipAddrList[0].ToString();
            }
        }
        public Int32 HostPortNo { get; set; }
        public ConnectionStatus ConnectionStatus { get; private set; }
        public Boolean EnableSend { get; set; }
        public Boolean IsConnected { get { return _connector.IsConnected; } }
        public Int32 ConnectionAliveTime { get; set; }
        internal MessageQueue MQ;





        public AegisClient()
        {
            MQ = new MessageQueue();
            _connector = new Connector(this);

            ConnectionStatus = ConnectionStatus.Closed;
            ConnectionAliveTime = 0;
            EnableSend = true;
        }


        public void Initialize()
        {
            _isRunning = true;
            (new Thread(Run)).Start();
        }


        public void Release()
        {
            _isRunning = false;
            _connector.Close();
            MQ.Clear();
        }


        public void Connect()
        {
            if (ConnectionStatus == ConnectionStatus.Connecting
                || ConnectionStatus == ConnectionStatus.Connected)
                return;

            ConnectionStatus = ConnectionStatus.Connecting;
            MQ.Add(MessageType.Connect, null, 0);
        }


        public void Close()
        {
            if (ConnectionStatus == ConnectionStatus.Closing
                || ConnectionStatus == ConnectionStatus.Closed)
                return;

            ConnectionStatus = ConnectionStatus.Closing;
            MQ.Add(MessageType.Close, null, 0);
        }


        public void SendPacket(StreamBuffer buffer)
        {
            MQ.Add(MessageType.Send, buffer, buffer.WrittenBytes);
        }


        internal Boolean IsValidPacket(StreamBuffer buffer, out Int32 packetSize)
        {
            packetSize = 0;
            if (ValidPacketHandler == null)
                return false;

            return ValidPacketHandler(buffer, out packetSize);
        }


        private void Run()
        {
            Thread.CurrentThread.Name = "AegisClient Runner";

            while (_isRunning)
            {
                MessageData data = MQ.Pop(100);
                if (data != null)
                    Dispatch(data);

                else if (ConnectionStatus == ConnectionStatus.Connected &&
                         MQ.Count == 0 &&
                         (ConnectionAliveTime > 0 && _stampLastAction.ElapsedMilliseconds > ConnectionAliveTime))
                {
                    Close();
                }
            }
        }


        private void Dispatch(MessageData data)
        {
            switch (data.Type)
            {
                case MessageType.Connect:
                    if (_connector.Connect(HostAddress, HostPortNo) == true)
                        ConnectionStatus = ConnectionStatus.Connected;
                    else
                        ConnectionStatus = ConnectionStatus.Closed;

                    if (OnConnect != null)
                        OnConnect(_connector.IsConnected);
                    break;


                case MessageType.Close:
                    MQ.Clear();
                    _connector.Close();
                    ConnectionStatus = ConnectionStatus.Closed;

                    if (OnDisconnect != null)
                        OnDisconnect();
                    break;


                case MessageType.Disconnect:
                    _connector.Close();
                    ConnectionStatus = ConnectionStatus.Closed;

                    if (OnDisconnect != null)
                        OnDisconnect();
                    break;


                case MessageType.Send:
                    if (ConnectionStatus == ConnectionStatus.Closed)
                    {
                        ConnectionStatus = ConnectionStatus.Connecting;

                        MQ.AddFirst(data.Type, data.Buffer, data.Size);
                        MQ.AddFirst(MessageType.Connect, null, 0);
                    }

                    else if (ConnectionStatus == ConnectionStatus.Connected)
                    {
                        if (EnableSend == false)
                            break;


                        if (_connector.SendPacket(data.Buffer) == true)
                            OnSend(data.Size);
                        else
                            Close();
                    }
                    break;


                case MessageType.Receive:
                    OnReceive(data.Buffer);
                    break;
            }


            if (_stampLastAction == null)
                _stampLastAction = Stopwatch.StartNew();
            else
                _stampLastAction.Restart();
        }
    }
}
