using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;
using Aegis.Network;



namespace EchoClient.Logic
{
    public static class ClientMain
    {
        public static void Start(Int32 clientCount, System.Windows.Forms.TextBox ctrl)
        {
            try
            {
                Logger.AddLogger(new LogTextBox(ctrl));
                Logger.Write(LogType.Info, 2, "EchoClient (AegisNetwork {0})", Aegis.Configuration.Environment.AegisVersion);


                Starter.Initialize();
                Starter.CreateNetworkChannel("ServerNetwork")
                       .StartNetwork(delegate { return new TestSession(); }, clientCount, clientCount);
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 2, e.ToString());
            }
        }


        public static void Stop()
        {
            Starter.StopNetwork("ServerNetwork");
            Starter.Release();
            Logger.Release();
        }
    }
}