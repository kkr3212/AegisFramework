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
        public static void Start(System.Windows.Forms.TextBox ctrl)
        {
            try
            {
                Logger.AddLogger(new LogTextBox(ctrl));
                Logger.Write(LogType.Info, 2, "EchoServer (AegisNetwork {0})", Aegis.Configuration.Environment.AegisVersion);

                Starter.Initialize("./Config.xml");
                Starter.StartNetwork();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 2, e.ToString());
            }
        }


        public static void Stop()
        {
            Starter.StopNetwork();
            Starter.Release();
            Logger.Release();
        }


        public static Int32 GetActiveSessionCount()
        {
            lock (NetworkChannel.Channels)
            {
                NetworkChannel channel = NetworkChannel.Channels.Find(v => v.Name == "NetworkClient");
                return channel?.SessionManager.ActiveSessionCount ?? 0;
            }
        }
    }
}
