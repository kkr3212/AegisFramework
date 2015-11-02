using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Xml;
using Aegis.Network;
using Aegis.Converter;
using Aegis.Configuration;
using Aegis.Threading;



namespace Aegis
{
    public static class Starter
    {
        private static Mutex _mutex;
        private static List<ConfigNetworkChannel> _listNetworkConfig;

        public static CustomData CustomData { get; private set; }
        public static Int32 ServerID { get; private set; }





        /// <summary>
        /// AegisNetwork 모듈을 초기화합니다.
        /// workerThreadCount와 dispatchThreadCount의 의미는 다음과 같습니다.
        /// -1 : 해당 작업을 AegisTask에서 실행됩니다.
        /// 0 : 해당 작업을 호출하는 쓰레드에서 실행됩니다.
        /// >0 : 정해진 ThreadPool에서 해당 작업이 실행됩니다.
        /// </summary>
        /// <param name="workerThreadCount">백그라운드에서 작업을 처리할 Thread 개수</param>
        /// <param name="dispatchThreadCount">작업결과를 전달할 Thread 개수</param>
        public static void Initialize(Int32 workerThreadCount = -1, Int32 dispatchThreadCount = 1)
        {
            _listNetworkConfig = new List<ConfigNetworkChannel>();
            CustomData = new CustomData("CustomData");

            SpinWorker.Initialize(workerThreadCount, dispatchThreadCount);
        }


        /// <summary>
        /// 구성정보 파일(XML)을 사용하여 AegisNetwork 모듈을 초기화합니다.
        /// workerThreadCount와 dispatchThreadCount의 의미는 다음과 같습니다.
        /// -1 : 해당 작업을 AegisTask에서 실행됩니다.
        /// 0 : 해당 작업을 호출하는 쓰레드에서 실행됩니다.
        /// >0 : 정해진 ThreadPool에서 해당 작업이 실행됩니다.
        /// </summary>
        /// <param name="configFilename">XML 파일명</param>
        /// <param name="workerThreadCount">백그라운드에서 작업을 처리할 Thread 개수</param>
        /// <param name="dispatchThreadCount">작업결과를 전달할 Thread 개수</param>
        public static void Initialize(String configFilename, Int32 workerThreadCount = -1, Int32 dispatchThreadCount = 1)
        {
            _listNetworkConfig = new List<ConfigNetworkChannel>();
            CustomData = new CustomData("CustomData");

            LoadConfigFile(configFilename);
            SpinWorker.Initialize(workerThreadCount, dispatchThreadCount);
        }


        /// <summary>
        /// 사용중인 모든 리소스를 반환하고, AegisNetwork을 종료합니다.
        /// </summary>
        public static void Release()
        {
            StopNetwork();
            SpinWorker.Release();

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
                if (config.ListenPortNo == 0)
                {
                    channel.StartNetwork(
                        delegate { return GenerateSession(config.SessionClassName); },
                        config.InitSessionPoolCount, config.MaxSessionPoolCount);
                }
                else
                {
                    channel.StartNetwork(
                            delegate { return GenerateSession(config.SessionClassName); },
                            config.InitSessionPoolCount, config.MaxSessionPoolCount)
                        .OpenListener(config.ListenIpAddress, config.ListenPortNo);
                }
            }
        }


        /// <summary>
        /// 새로운 NetworkChannel을 생성합니다.
        /// 이 Method는 NetworkChannel.CreateChannel와 동일합니다.
        /// </summary>
        /// <param name="channelName">생성할 NetworkChannel의 고유이름</param>
        /// <returns>생성된 NetworkChannel 객체</returns>
        public static NetworkChannel CreateNetworkChannel(String channelName)
        {
            return NetworkChannel.CreateChannel(channelName);
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
                    delegate { return GenerateSession(config.SessionClassName); },
                    config.InitSessionPoolCount, config.MaxSessionPoolCount);
            }
            else
            {
                channel.StartNetwork(
                        delegate { return GenerateSession(config.SessionClassName); },
                        config.InitSessionPoolCount, config.MaxSessionPoolCount)
                    .OpenListener(config.ListenIpAddress, config.ListenPortNo);
            }

            return channel;
        }


        /// <summary>
        /// 지정된 NetworkChannel만을 생성하고 네트워킹을 시작합니다.
        /// sessionGenerator가 지정되기 때문에 config file에 정의된 sessionClass, receiveBufferSize는 무시됩니다.
        /// </summary>
        /// <param name="networkChannelName">시작할 NetworkChannel 이름</param>
        /// <param name="sessionGenerator">Session 객체를 생성하는 Delegator</param>
        /// <returns>해당 NetworkChannel 객체</returns>
        public static NetworkChannel StartNetwork(String networkChannelName, SessionGenerateDelegator sessionGenerator)
        {
            ConfigNetworkChannel config = _listNetworkConfig.Find(v => v.NetworkChannelName == networkChannelName);
            if (config == null)
                throw new AegisException(AegisResult.InvalidArgument, "Invalid NetworkChannel name({0}).", networkChannelName);


            NetworkChannel channel = NetworkChannel.CreateChannel(config.NetworkChannelName);
            if (config.ListenIpAddress.Length == 0 || config.ListenPortNo == 0)
            {
                channel.StartNetwork(sessionGenerator, config.InitSessionPoolCount, config.MaxSessionPoolCount);
            }
            else
            {
                channel.StartNetwork(sessionGenerator, config.InitSessionPoolCount, config.MaxSessionPoolCount)
                       .OpenListener(config.ListenIpAddress, config.ListenPortNo);
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
            channel?.StopNetwork();

            return channel;
        }


        private static Session GenerateSession(String sessionClassName)
        {
            Type type = Configuration.Environment.ExecutingAssembly.GetType(sessionClassName);
            if (type == null)
                throw new AegisException(AegisResult.InvalidArgument, "'{0}' session class is not exists.", sessionClassName);

            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null);
            if (constructorInfo == null)
                throw new AegisException(AegisResult.InvalidArgument, "'No matches constructor on '{0}'.", sessionClassName);


            Session session = constructorInfo.Invoke(null) as Session;
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
