using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Aegis;



namespace Aegis.Client
{
    internal class Connector
    {
        private AegisClient _aegisClient;
        private Socket _socket;
        private StreamBuffer _receivedBuffer;

        public bool IsConnected { get { return (_socket == null ? false : _socket.Connected); } }





        public Connector(AegisClient parent)
        {
            _aegisClient = parent;
            _receivedBuffer = new StreamBuffer();
        }


        public bool Connect(String ipAddress, int portNo)
        {
            if (_socket != null)
                throw new AegisException("This session has already been activated.");


            //  연결 시도
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNo);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(ipEndPoint);


                if (_socket.Connected == false)
                {
                    _socket.Close();
                    _socket = null;
                    return false;
                }

                WaitForReceive();
                return true;
            }
            catch (Exception)
            {
                Close();
            }

            return false;
        }


        public void Close()
        {
            try
            {
                //  Socket.Close를 호출하면 OnSocket_Read에 이벤트가 발생하는데
                //  OnSocket_Read에서 _socket을 사용하지 않도록 하기위해 _socket을 먼저 null로 바꿔야한다.
                Socket sock = _socket;
                if (sock == null)
                    return;

                _receivedBuffer.Clear();
                _socket = null;

                sock.Close();
                sock = null;
            }
            catch (Exception)
            {
                //  Nothing to do
            }
        }


        private void WaitForReceive()
        {
            if (_receivedBuffer.WritableSize == 0)
                _receivedBuffer.Resize(_receivedBuffer.BufferSize * 2);

            if (_socket != null && _socket.Connected)
                _socket.BeginReceive(_receivedBuffer.Buffer, _receivedBuffer.WrittenBytes, _receivedBuffer.WritableSize, 0, new AsyncCallback(OnSocket_Read), null);
        }


        private void OnSocket_Read(IAsyncResult ar)
        {
            if (_socket == null)
                return;

            try
            {
                //  transBytes가 0이면 원격지 혹은 네트워크에 의해 연결이 끊긴 상태
                int transBytes = _socket.EndReceive(ar);
                if (transBytes == 0)
                {
                    _aegisClient.MQ.Add(MessageType.Disconnect, null, 0);
                    return;
                }


                _receivedBuffer.Write(transBytes);
                while (_receivedBuffer.ReadableSize > 0)
                {
                    //  패킷 하나가 정상적으로 수신되었는지 확인
                    int packetSize = 0;

                    _receivedBuffer.ResetReadIndex();
                    if (_aegisClient.IsValidPacket(_receivedBuffer, out packetSize) == false)
                        break;


                    //  수신처리
                    try
                    {
                        StreamBuffer dispatchBuffer = new StreamBuffer(_receivedBuffer.Buffer, 0, packetSize);
                        _aegisClient.MQ.Add(MessageType.Receive, dispatchBuffer, dispatchBuffer.WrittenBytes);


                        _receivedBuffer.ResetReadIndex();
                        _receivedBuffer.Read(packetSize);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }


                    //  처리된 패킷을 버퍼에서 제거
                    _receivedBuffer.PopReadBuffer();
                }


                //  ReceiveBuffer의 안정적인 처리를 위해 OnReceive 작업이 끝난 후에 다시 수신대기
                WaitForReceive();
            }
            catch (SocketException)
            {
                _aegisClient.MQ.Add(MessageType.Close, null, 0);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }


        public bool SendPacket(StreamBuffer source)
        {
            try
            {
                _socket.Send(source.Buffer, 0, source.WrittenBytes, SocketFlags.None);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            return false;
        }
    }
}
