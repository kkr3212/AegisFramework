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

        /// <summary>
        /// Session 객체를 생성하는 Delegator를 설정합니다.
        /// SessionManager에서는 내부적으로 Session Pool을 관리하는데, Pool에 객체가 부족할 때 이 Delegator가 호출됩니다.
        /// 그러므로 이 Delegator에서는 ObjectPool 대신 new를 사용해 인스턴스를 생성하는 것이 좋습니다.
        /// </summary>
        public SessionGenerator SessionGenerator
        {
            set { SessionManager.GenerateSession = value; }
        }


        private static List<NetworkChannel> Channels = new List<NetworkChannel>();





        public static NetworkChannel CreateChannel(String name)
        {
            lock (Channels)
            {
                NetworkChannel channel = new NetworkChannel(name);
                Channels.Add(channel);

                return channel;
            }
        }


        public static void Release(NetworkChannel channel)
        {
            lock (Channels)
            {
                Channels.Remove(channel);
                channel.Release();
            }
        }


        public static NetworkChannel GetChannel(String name)
        {
            lock (Channels)
            {
                NetworkChannel channel = Channels.Find(v => v.Name == name);
                if (channel != null)
                    return channel;

                throw new AegisException(ResultCode.NoNetworkChannelName, "Invalid NetworkChannel name({0}).", name);
            }
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
