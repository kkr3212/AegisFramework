using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Network;



namespace EchoServer.Logic
{
    public static class ServerMain
    {
        public static void StartServer(System.Windows.Forms.TextBox ctrl)
        {
            try
            {
                Logger.AddLogger(new LogTextBox(ctrl));
                Logger.Write(LogType.Info, 2, "EchoServer (AegisNetwork {0})", Aegis.Configuration.Environment.AegisVersion);

                Starter.Initialize();
                Starter.CreateNetworkChannel("ClientNetwork")
                       .StartNetwork(delegate { return new ClientSession(); }, 1, 100)
                       .OpenListener("192.168.0.100", 10100);
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 2, e.ToString());
            }
        }


        public static void StopServer()
        {
            Starter.StopNetwork("ClientNetwork");
            Starter.Release();
            Logger.Release();
        }


        public static Int32 GetActiveSessionCount()
        {
            NetworkChannel channel = NetworkChannel.Channels.Find(v => v.Name == "ClientNetwork");
            return channel?.SessionManager.ActiveSessionCount ?? 0;
        }
    }
}
