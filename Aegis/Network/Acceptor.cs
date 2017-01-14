using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Aegis;



namespace Aegis.Network
{
    public class Acceptor
    {
        private NetworkChannel _networkChannel;
        private IPEndPoint _listenEndPoint;
        private Socket _listenSocket;
        private SocketAsyncEventArgs _eventAccept;

        public string ListenIpAddress { get; set; }
        public int ListenPortNo { get; set; }





        internal Acceptor(NetworkChannel networkChannel)
        {
            _networkChannel = networkChannel;
            _eventAccept = new SocketAsyncEventArgs();
            _eventAccept.Completed += Accepted;
        }


        public void Listen()
        {
            try
            {
                if (_listenSocket != null)
                    throw new AegisException(AegisResult.AcceptorIsRunning, "Acceptor is already running.");


                if (ListenIpAddress.Length == 0)
                    _listenEndPoint = new IPEndPoint(IPAddress.Any, ListenPortNo);
                else
                    _listenEndPoint = new IPEndPoint(IPAddress.Parse(ListenIpAddress), ListenPortNo);


                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listenSocket.Bind(_listenEndPoint);
                _listenSocket.Listen(100);

                /*
                if (_networkChannel.Name != "")
                    Logger.Info(LogMask.Aegis, "{0} listening on {1}, {2}", _networkChannel.Name, _listenEndPoint.Address, _listenEndPoint.Port);
                else
                    Logger.Info(LogMask.Aegis, "Listening on {0}, {1}", _listenEndPoint.Address, _listenEndPoint.Port);
                */

                _listenSocket.AcceptAsync(_eventAccept);
            }
            catch (AegisException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AegisException(AegisResult.NetworkError, e, e.Message);
            }
        }


        public void Close()
        {
            if (_listenSocket == null)
                return;

            _listenSocket.Close();

            /*
            if (_networkChannel.Name != "")
                Logger.Info(LogMask.Aegis, "{0} listening stopped({1}, {2})", _networkChannel.Name, _listenEndPoint.Address, _listenEndPoint.Port);
            else
                Logger.Info(LogMask.Aegis, "Listening stopped({0}, {1})", _listenEndPoint.Address, _listenEndPoint.Port);
            */


            _listenSocket = null;
            _listenEndPoint = null;
        }


        private void Accepted(object sender, SocketAsyncEventArgs eventArgs)
        {
            try
            {
                Socket acceptedSocket = eventArgs.AcceptSocket;
                if (acceptedSocket.Connected == false)
                    return;


                Session acceptedSession = _networkChannel.PopInactiveSession();
                if (acceptedSession == null)
                {
                    acceptedSocket.Close();
                    Logger.Err(LogMask.Aegis, "Cannot activate any more sessions. Please check MaxSessionPoolSize.");
                    return;
                }


                acceptedSession.AttachSocket(acceptedSocket);
                acceptedSession.OnSocket_Accepted();


                eventArgs.AcceptSocket = null;
                _listenSocket.AcceptAsync(_eventAccept);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.Interrupted)
                    Logger.Err(LogMask.Aegis, e.ToString());
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
            }
        }
    }
}
