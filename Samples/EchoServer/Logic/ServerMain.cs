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
            if (Environment.UserInteractive)
            {
                Logger.AddLogger(new LogTextFile("EchoServer"));
                Logger.AddLogger(new LogTextBox(ctrl));
            }
            else
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                Logger.AddLogger(new LogTextFile("EchoServer"));
            }


            try
            {
                Logger.Write(LogType.Info, 2, "EchoServer (Build {0})", Aegis.Definitions.BuildNo);

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
