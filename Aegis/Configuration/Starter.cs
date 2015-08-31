using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using Aegis.Threading;
using Aegis.Network;
using Aegis.Converter;



namespace Aegis.Configuration
{
    /// <summary>
    /// XML 파일에서 네트워크 구성정보를 읽어들여 실행합니다.
    /// </summary>
    public static class Starter
    {
        private static Assembly _assembly;
        private static Mutex _mutex;
        private static List<ConfigNetworkChannel> _listNetworkConfig;

        public static CustomData CustomData { get; private set; }
        public static Int32 ServerID { get; private set; }





        /// <summary>
        /// XML 파일로 부터 구성정보를 읽어들입니다.
        /// assembly에 값이 지정되어야 XML의 NetworkChannel/sessionClass에 정의한 클래스를 생성할 수 있습니다.
        /// assembly는 sessionClass가 정의된 Assembly에서 System.Reflection.Assembly.GetExecutingAssembly()를 호출하여 얻을 수 있습니다.
        /// </summary>
        /// <param name="assembly">NetworkChannel/sessionClass가 정의된 Assembly</param>
        /// <param name="configFilename">XML 파일명</param>
        public static void Initialize(Assembly assembly, String configFilename)
        {
            _assembly = assembly;
            _listNetworkConfig = new List<ConfigNetworkChannel>();
            CustomData = new CustomData("CustomData");

            LoadConfigFile(configFilename);
        }


        public static void Release()
        {
            StopNetwork();

            if (_mutex != null)
            {
                _mutex.Close();
                _mutex = null;
            }
        }


        /// <summary>
        /// NetworkChannel을 생성하고 네트워킹을 시작합니다.
        /// </summary>
        public static void StartNetwork()
        {
            foreach (ConfigNetworkChannel config in _listNetworkConfig)
            {
                NetworkChannel channel = NetworkChannel.CreateChannel(config.NetworkChannelName);
                if (config.ListenIpAddress.Length == 0 || config.ListenPortNo == 0)
                {
                    channel.StartNetwork(
                        delegate { return GenerateSession(config.SessionClassName, config.ReceiveBufferSize); }
                        , config.InitSessionPoolCount, config.MaxSessionPoolCount);
                }
                else
                {
                    channel.StartNetwork(
                        delegate { return GenerateSession(config.SessionClassName, config.ReceiveBufferSize); }
                        , config.InitSessionPoolCount, config.MaxSessionPoolCount
                        , config.ListenIpAddress, config.ListenPortNo);
                }
            }
        }


        /// <summary>
        /// 지정된 NetworkChannel만을 생성하고 네트워킹을 시작합니다.
        /// </summary>
        /// <param name="networkChannelName">시작할 NetworkChannel 이름</param>
        /// <returns>해당 NetworkChannel 객체</returns>
        public static NetworkChannel StartNetwork(String networkChannelName)
        {
            ConfigNetworkChannel config = _listNetworkConfig.Find(v => v.NetworkChannelName == networkChannelName);
            if (config == null)
                throw new AegisException(AegisResult.InvalidArgument, "Invalid NetworkChannel name({0}).", networkChannelName);


            NetworkChannel channel = NetworkChannel.CreateChannel(config.NetworkChannelName);
            if (config.ListenIpAddress.Length == 0 || config.ListenPortNo == 0)
            {
                channel.StartNetwork(
                    delegate { return GenerateSession(config.SessionClassName, config.ReceiveBufferSize); }
                    , config.InitSessionPoolCount, config.MaxSessionPoolCount);
            }
            else
            {
                channel.StartNetwork(
                    delegate { return GenerateSession(config.SessionClassName, config.ReceiveBufferSize); }
                    , config.InitSessionPoolCount, config.MaxSessionPoolCount
                    , config.ListenIpAddress, config.ListenPortNo);
            }

            return channel;
        }


        /// <summary>
        /// 실행중인 모든 NetworkChannel을 중지합니다.
        /// </summary>
        public static void StopNetwork()
        {
            foreach (NetworkChannel channel in NetworkChannel.Channels)
                channel.StopNetwork();

            NetworkChannel.Release();
        }


        /// <summary>
        /// 지정된 NetworkChannel만 중지합니다.
        /// </summary>
        /// <param name="networkChannelName">중지할 NetworkChannel 이름</param>
        /// <returns>해당 NetworkChannel 객체</returns>
        public static NetworkChannel StopNetwork(String networkChannelName)
        {
            NetworkChannel channel = NetworkChannel.Channels.Find(v => v.Name == networkChannelName);
            if (channel != null)
                channel.StopNetwork();

            return channel;
        }


        private static NetworkSession GenerateSession(String sessionClassName, Int32 receiveBufferSize)
        {
            Type type = _assembly.GetType(sessionClassName);
            if (type == null)
                throw new AegisException(AegisResult.InvalidArgument, "'{0}' session class is not exists.", sessionClassName);

            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null);
            if (constructorInfo == null)
                throw new AegisException(AegisResult.InvalidArgument, "'No matches constructor on '{0}'.", sessionClassName);


            NetworkSession session = constructorInfo.Invoke(null) as NetworkSession;
            session.SetReceiveBufferSize(receiveBufferSize);

            return session;
        }


        private static void LoadConfigFile(String filename)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(filename);


            //  Aegis
            {
                XmlNode node = xml.SelectSingleNode("Aegis");

                ServerID = GetAttributeValue(node.Attributes, "id").ToInt32();
                Logger.EnabledLevel = GetAttributeValue(node.Attributes, "logLevel", "0").ToInt32();


                String mutexName = GetAttributeValue(node.Attributes, "mutexName", "");
                if (mutexName.Length > 0)
                {
                    Boolean isNew;
                    _mutex = new Mutex(true, mutexName, out isNew);

                    if (isNew == false)
                        throw new AegisException(AegisResult.InvalidArgument, "The mutex name({0}) is already in use.", mutexName);
                }
            }


            //  Aegis/NetworkChannel
            foreach (XmlNode node in xml.SelectNodes("Aegis/NetworkChannel"))
            {
                ConfigNetworkChannel channelConfig = new ConfigNetworkChannel();
                channelConfig.NetworkChannelName = GetAttributeValue(node.Attributes, "name");
                channelConfig.SessionClassName = GetAttributeValue(node.Attributes, "sessionClass");

                channelConfig.ReceiveBufferSize = GetAttributeValue(node.Attributes, "receiveBufferSize", "0").ToInt32();
                channelConfig.InitSessionPoolCount = GetAttributeValue(node.Attributes, "initSessionPoolCount", "0").ToInt32();
                channelConfig.MaxSessionPoolCount = GetAttributeValue(node.Attributes, "maxSessionPoolCount", "0").ToInt32();
                channelConfig.ListenIpAddress = GetAttributeValue(node.Attributes, "listenIpAddress", "");
                channelConfig.ListenPortNo = GetAttributeValue(node.Attributes, "listenPortNo", "0").ToInt32();


                if (_listNetworkConfig.Where(v => v.NetworkChannelName == channelConfig.NetworkChannelName).Count() > 0)
                    throw new AegisException(AegisResult.InvalidArgument, "The NetworkChannel name({0}) is already in use.", channelConfig.NetworkChannelName);

                _listNetworkConfig.Add(channelConfig);
            }


            //  Aegis/CustomData
            XmlNode nodeCustomData = xml.SelectSingleNode("Aegis/CustomData");
            if (nodeCustomData != null)
                LoadCustomData(nodeCustomData, CustomData);
        }


        private static void LoadCustomData(XmlNode node, CustomData customData)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (customData.GetChild(childNode.Name) != null)
                    throw new AegisException(AegisResult.InvalidArgument, "Duplicate key({0}) in CustomData.");


                CustomData data = new CustomData(childNode.Name);
                customData.Childs.Add(data);


                foreach (XmlAttribute attr in childNode.Attributes)
                {
                    CustomData childData = new CustomData(attr.Name, attr.Value);
                    data.Childs.Add(childData);
                }

                LoadCustomData(childNode, data);
            }
        }


        private static String GetAttributeValue(XmlAttributeCollection xac, String name)
        {
            try
            {
                XmlAttribute attr = xac[name];
                if (attr == null)
                    throw new AegisException("Invalid attribute name({0})", name);

                return xac[name].Value;
            }
            catch (Exception)
            {
                throw new AegisException("Invalid attribute name({0})", name);
            }
        }


        private static String GetAttributeValue(XmlAttributeCollection xac, String name, String defaultValue)
        {
            try
            {
                XmlAttribute attr = xac[name];
                if (attr != null)
                    return xac[name].Value;
            }
            catch (Exception)
            {
            }

            return defaultValue;
        }
    }
}
