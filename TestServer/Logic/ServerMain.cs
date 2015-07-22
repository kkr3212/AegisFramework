using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using Aegis;
using Aegis.Network;



namespace TestServer.Logic
{
    public class ServerMain : ServiceBase
    {
        private NetworkChannel _channelClient;

        public static ServerMain Instance { get { return Singleton<ServerMain>.Instance; } }





        private ServerMain()
        {
        }


        override protected void OnStart(string[] args)
        {
            StartServer(null);
        }


        override protected void OnStop()
        {
            StopServer();
        }


        public void StartServer(System.Windows.Forms.TextBox ctrl)
        {
            //  Logger 설정
            if (Environment.UserInteractive)
            {
                Logger.AddLogger(new LogTextFile("TestServer"));
                Logger.AddLogger(new LogTextBox(ctrl));
            }
            else
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                Logger.AddLogger(new LogTextFile("TestServer"));
            }


            try
            {
                Logger.Write(LogType.Info, 2, "TestServer (Build {0})", Aegis.Definitions.BuildNo);


                _channelClient = NetworkChannel.CreateChannel("ClientNetwork");
                _channelClient.SessionGenerator = delegate { return new ClientSession(); };
                _channelClient.InitSessionPoolSize = 1;
                _channelClient.MaxSessionPoolSize = 1;
                _channelClient.ListenIpAddress = "192.168.0.100";
                _channelClient.ListenPortNo = 10100;
                _channelClient.StartNetwork();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 2, e.ToString());
            }
        }


        public void StopServer()
        {
            _channelClient.StopNetwork();
            Logger.Release();
        }
    }
}
