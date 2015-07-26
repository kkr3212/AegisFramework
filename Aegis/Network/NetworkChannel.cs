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
        public SessionManager SessionManager { get; private set; }
        public Acceptor Acceptor { get; private set; }
        private static List<NetworkChannel> _channels = new List<NetworkChannel>();





        /// <summary>
        /// NetworkChannel 객체를 생성합니다.
        /// name은 이전에 생성된 NetworkChannel과 동일한 문자열을 사용할 수 없습니다.
        /// </summary>
        /// <param name="name">생성할 NetworkChannel의 고유한 이름.</param>
        /// <returns>생성된 NetworkChannel 객체</returns>
        public static NetworkChannel CreateChannel(String name)
        {
            lock (_channels)
            {
                NetworkChannel channel = _channels.Find(v => v.Name == name);
                if (channel != null)
                    throw new AegisException(ResultCode.AlreadyExistName, "Already exists same name.");


                channel = new NetworkChannel(name);
                _channels.Add(channel);

                return channel;
            }
        }


        /// <summary>
        /// 생성된 모든 NetworkChannel을 종료하고 사용중인 리소스를 반환합니다.
        /// 활성화된 Acceptor, Session 등 모든 네트워크 작업이 종료됩니다.
        /// </summary>
        public static void Release()
        {
            lock (_channels)
            {
                foreach (NetworkChannel networkChannel in _channels)
                    networkChannel.StopNetwork();

                _channels.Clear();
            }
        }


        /// <summary>
        /// name을 사용하여 NetworkChannel을 가져옵니다.
        /// </summary>
        /// <param name="name">검색할 NetworkChannel의 이름</param>
        /// <returns>검색된 NetworkChannel 객체</returns>
        public static NetworkChannel FindChannel(String name)
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

            SessionManager = new SessionManager(this);
            Acceptor = new Acceptor(this);
        }


        /// <summary>
        /// 네트워크 작업을 시작합니다.
        /// </summary>
        public void StartNetwork(SessionGenerateDelegator generator, Int32 initPoolSize, Int32 maxPoolSize)
        {
            SessionManager.SessionGenerator = generator;
            SessionManager.MaxSessionPoolSize = maxPoolSize;
            SessionManager.CreatePool(initPoolSize);
        }


        /// <summary>
        /// 네트워크 작업을 시작합니다.
        /// 지정된 ipAddress를 사용해 Acceptor를 실행합니다.
        /// </summary>
        public void StartNetwork(SessionGenerateDelegator generator, Int32 initPoolSize, Int32 maxPoolSize, String ipAddress, Int32 portNo)
        {
            SessionManager.SessionGenerator = generator;
            SessionManager.MaxSessionPoolSize = maxPoolSize;
            SessionManager.CreatePool(initPoolSize);

            Acceptor.Listen(ipAddress, portNo);
        }


        /// <summary>
        /// 네트워크 작업을 종료하고 사용중인 리소스를 반환합니다.
        /// Acceptor와 활성화된 Session의 네트워크 작업이 중단됩니다.
        /// </summary>
        public void StopNetwork()
        {
            Acceptor.Close();
            SessionManager.Release();
        }
    }
}
