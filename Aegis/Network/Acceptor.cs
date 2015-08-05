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
    internal class Acceptor
    {
        private NetworkChannel _networkChannel;
        private IPEndPoint _listenEndPoint;
        private Socket _listenSocket;
        private SocketAsyncEventArgs _eventAccept;





        internal Acceptor(NetworkChannel networkChannel)
        {
            _networkChannel = networkChannel;
            _eventAccept = new SocketAsyncEventArgs();
            _eventAccept.Completed += OnAccepted;
        }


        internal void Listen(String ipAddress, Int32 portNo)
        {
            if (_listenSocket != null)
                throw new AegisException(ResultCode.AcceptorIsRunning, "Acceptor is already running.");

            try
            {
                if (ipAddress.Length == 0)
                    _listenEndPoint = new IPEndPoint(IPAddress.Any, portNo);
                else
                    _listenEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNo);


                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listenSocket.Bind(_listenEndPoint);
                _listenSocket.Listen(100);

                Logger.Write(LogType.Info, 1, "Listening on {0}, {1}", _listenEndPoint.Address, _listenEndPoint.Port);
                _listenSocket.AcceptAsync(_eventAccept);
            }
            catch (Exception e)
            {
                throw new AegisException(ResultCode.NetworkError, e, e.Message);
            }
        }


        internal void Close()
        {
            if (_listenSocket == null)
                return;

            _listenSocket.Close();
            Logger.Write(LogType.Info, 1, "Listening stopped({0}, {1})", _listenEndPoint.Address, _listenEndPoint.Port);


            _listenSocket = null;
            _listenEndPoint = null;
        }


        private void OnAccepted(object sender, SocketAsyncEventArgs eventArgs)
        {
            try
            {
                Socket acceptedSocket = eventArgs.AcceptSocket;
                if (acceptedSocket.Connected == false)
                    return;


                SessionBase acceptedSession = _networkChannel.SessionManager.AttackSocket(acceptedSocket);
                if (acceptedSession == null)
                {
                    acceptedSocket.Close();
                    Logger.Write(LogType.Warn, 1, "Cannot activate any more sessions. Please check MaxSessionPoolSize.");
                    return;
                }


                acceptedSession.OnSocket_Accepted();


                eventArgs.AcceptSocket = null;
                _listenSocket.AcceptAsync(_eventAccept);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.Interrupted)
                    Logger.Write(LogType.Err, 1, e.ToString());
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }
    }
}
