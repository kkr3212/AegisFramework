using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Network;



namespace EchoServer.Logic
{
    public class ServerMain
    {
        public static ServerMain Instance { get { return Singleton<ServerMain>.Instance; } }
        private NetworkChannel _networkClient = NetworkChannel.CreateChannel("ClientNetwork");





        private ServerMain()
        {
        }


        public void StartServer(System.Windows.Forms.TextBox ctrl)
        {
            //  Logger 설정
            Logger.AddLogger(new LogTextBox(ctrl));


            try
            {
                Logger.Write(LogType.Info, 2, "EchoServer (Aegis {0})", Aegis.Version.ToString());

                _networkClient.StartNetwork(delegate { return new ClientSession(); }, 1, 100, "192.168.0.100", 10100);
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 2, e.ToString());
            }
        }


        public void StopServer()
        {
            _networkClient.StopNetwork();
            Logger.Release();
        }


        public Int32 GetActiveSessionCount()
        {
            return _networkClient.SessionManager.ActiveSessionCount;
        }
    }
}
