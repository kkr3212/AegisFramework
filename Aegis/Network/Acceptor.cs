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
        private Boolean _isRunning = false;





        internal Acceptor(NetworkChannel networkChannel)
        {
            _networkChannel = networkChannel;
        }


        internal void Listen(String ipAddress, Int32 portNo)
        {
            if (_isRunning == true)
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

                _isRunning = true;
                (new Thread(Run)).Start();
            }
            catch (Exception e)
            {
                throw new AegisException(ResultCode.NetworkError, e, e.Message);
            }
        }


        internal void Close()
        {
            if (_isRunning == false)
                return;

            _isRunning = false;
            _listenSocket.Close();
            _listenSocket.Dispose();


            Logger.Write(LogType.Info, 1, "Listening stopped({0}, {1})", _listenEndPoint.Address, _listenEndPoint.Port);


            _listenSocket = null;
            _listenEndPoint = null;
        }


        private void Run()
        {
            try
            {
                while (_isRunning == true)
                {
                    Socket acceptedSocket = _listenSocket.Accept();
                    Session acceptedSession = _networkChannel.SessionManager.AttackSocket(acceptedSocket);

                    if (acceptedSession == null)
                    {
                        acceptedSocket.Close();
                        Logger.Write(LogType.Warn, 1, "Cannot activate any more sessions. Please check MaxSessionPoolSize.");
                        continue;
                    }

                    acceptedSession.OnSocket_Accepted();
                }
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


            Close();
        }
    }
}
