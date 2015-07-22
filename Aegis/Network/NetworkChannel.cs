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
        public Int32 InitSessionPoolSize { get; set; }
        public Int32 MaxSessionPoolSize { get; set; }
        public String ListenIpAddress { get; set; }
        public Int32 ListenPortNo { get; set; }

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


        private static List<NetworkChannel> _channels = new List<NetworkChannel>();





        public static NetworkChannel CreateChannel(String name)
        {
            lock (_channels)
            {
                NetworkChannel channel = new NetworkChannel(name);
                _channels.Add(channel);

                return channel;
            }
        }


        public static void Release()
        {
            lock (_channels)
            {
                foreach (NetworkChannel networkChannel in _channels)
                    networkChannel.StopNetwork();

                _channels.Clear();
            }
        }


        public static NetworkChannel GetChannel(String name)
        {
            lock (_channels)
            {
                NetworkChannel channel = _channels.Find(v => v.Name == name);
                if (channel != null)
                    return channel;

                throw new AegisException(ResultCode.NoNetworkChannelName, "Invalid NetworkChannel name({0}).", name);
            }
        }


        private NetworkChannel(String name)
        {
            Name = name;

            InitSessionPoolSize = 0;
            ListenIpAddress = "";
            ListenPortNo = 0;

            SessionManager = new SessionManager(this);
            Acceptor = new Acceptor(this);
        }


        public void StartNetwork()
        {
            SessionManager.CreatePool(InitSessionPoolSize);
            SessionManager.MaxSessionPoolSize = MaxSessionPoolSize;

            if (ListenIpAddress.Length > 0  && ListenPortNo > 0)
                Acceptor.Listen(ListenIpAddress, ListenPortNo);
        }


        public void StopNetwork()
        {
            Acceptor.Close();
            SessionManager.Release();
        }
    }
}
