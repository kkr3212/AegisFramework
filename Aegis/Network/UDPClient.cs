using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Aegis.IO;
using Aegis.Threading;

namespace Aegis.Network
{
    public class UDPClient
    {
        public event IOEventHandler EventRead, EventClose;
        public Socket Socket { get { return _socket; } }
        public bool Connected { get { return _socket.Connected; } }

        private Socket _socket;
        private EndPoint _endPoint;

        private readonly byte[] _receivedBuffer = new byte[8192];





        public UDPClient()
        {
        }


        public void Connect(string ipAddress, int portNo)
        {
            lock (this)
            {
                if (_socket != null)
                    throw new AegisException(AegisResult.ActivatedSession, "This session has already been activated.");


                Array.Clear(_receivedBuffer, 0, _receivedBuffer.Length);


                _endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNo);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socket.Connect(_endPoint);
            }


            WaitForReceive();
        }


        public void Close()
        {
            lock (this)
            {
                if (_socket == null)
                    return;

                EndPoint ep = _endPoint;
                SpinWorker.Dispatch(() =>
                {
                    EventClose?.Invoke(new IOEventResult(ep, IOEventType.Close, 0));
                });

                _socket.Close();
                _socket = null;
            }
        }


        private void WaitForReceive()
        {
            lock (this)
            {
                var socket = _socket;
                socket?.BeginReceiveFrom(_receivedBuffer, 0, _receivedBuffer.Length, SocketFlags.None,
                    ref _endPoint, SocketEvent_Receive, null);
            }
        }


        private void SocketEvent_Receive(IAsyncResult ar)
        {
            try
            {
                lock (this)
                {
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    var socket = _socket;
                    int transBytes = socket?.EndReceiveFrom(ar, ref remoteEP) ?? -1;
                    if (transBytes == -1)
                        return;

                    byte[] dispatchBuffer = new byte[transBytes];
                    Array.Copy(_receivedBuffer, dispatchBuffer, transBytes);
                    SpinWorker.Dispatch(() =>
                    {
                        EventRead?.Invoke(new IOEventResult(remoteEP, IOEventType.Read, dispatchBuffer, 0, transBytes, 0));
                    });

                    WaitForReceive();
                }
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
                Close();
            }
        }


        public void Send(StreamBuffer buffer)
        {
            lock (this)
            {
                var socket = _socket;
                socket?.BeginSendTo(buffer.Buffer, 0, buffer.WrittenBytes, SocketFlags.None, _endPoint, Socket_Send, null);
            }
        }


        public void Send(byte[] buffer)
        {
            lock (this)
            {
                var socket = _socket;
                socket?.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, _endPoint, Socket_Send, null);
            }
        }


        public void Send(byte[] buffer, int startIndex, int length)
        {
            lock (this)
            {
                var socket = _socket;
                socket?.BeginSendTo(buffer, startIndex, length, SocketFlags.None, _endPoint, Socket_Send, null);
            }
        }


        private void Socket_Send(IAsyncResult ar)
        {
            try
            {
                lock (this)
                {
                    var socket = _socket;
                    socket?.EndSend(ar);
                }
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
            };
        }
    }
}
