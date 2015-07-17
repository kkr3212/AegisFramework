using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis;



namespace Aegis.Network
{
    public class NetworkChannel
    {
        public String Name { get; private set; }
        internal SessionManager SessionManager { get; private set; }
        internal Acceptor Acceptor { get; private set; }





        public static NetworkChannel CreateChannel(String name)
        {
            NetworkChannel channel = new NetworkChannel(name);
            return channel;
        }


        public static void Release(NetworkChannel channel)
        {
            channel.Release();
        }


        private NetworkChannel(String name)
        {
            Name = name;
            SessionManager = new SessionManager(this);
            Acceptor = new Acceptor(this);
        }


        private void Release()
        {
            Acceptor.Close();
            SessionManager.Clear();

            Acceptor = null;
            SessionManager = null;
        }


        public void StartNetwork(String ipAddress, Int32 portNo)
        {
            Acceptor.Listen(ipAddress, portNo);
        }
    }
}
