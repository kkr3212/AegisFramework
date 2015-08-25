using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Network;



namespace EchoClient.Logic
{
    public class ClientMain
    {
        public static ClientMain Instance { get { return Singleton<ClientMain>.Instance; } }
        private NetworkChannel _networkServer = NetworkChannel.CreateChannel("ServerNetwork");





        private ClientMain()
        {
        }


        public void StartServer(Int32 clientCount, System.Windows.Forms.TextBox ctrl)
        {
            Logger.AddLogger(new LogTextBox(ctrl));


            try
            {
                Logger.Write(LogType.Info, 2, "EchoClient (Aegis {0})", Aegis.Version.ToString());


                _networkServer.StartNetwork(delegate { return new TestSession(); }, clientCount, clientCount);
                //_networkServer.StartNetwork(delegate { return new AwaitSession(); }, clientCount, clientCount);
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 2, e.ToString());
            }
        }


        public void StopServer()
        {
            _networkServer.StopNetwork();
            Logger.Release();
        }
    }
}
