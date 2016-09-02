using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using Aegis.Data;
using Aegis.Calculate;



namespace Aegis.Network
{
    [DebuggerDisplay("Name={Name} ActiveSession={ActiveSessions.Count}")]
    public class NetworkChannel
    {
        /// <summary>
        /// 이 NetworkChannel 객체의 고유한 이름입니다.
        /// </summary>
        public readonly string Name;
        public static NamedObjectIndexer<NetworkChannel> Channels = new NamedObjectIndexer<NetworkChannel>();
        public List<Session> ActiveSessions { get; } = new List<Session>();
        public List<Session> InactiveSessions { get; } = new List<Session>();
        public int MaxSessionCount { get; set; }
        public SessionGenerateHandler SessionGenerator { get; set; }

        public Acceptor Acceptor { get; private set; }
        private TreeNode<string> _configNode;





        /// <summary>
        /// NetworkChannel 객체를 생성합니다.
        /// name은 이전에 생성된 NetworkChannel과 동일한 문자열을 사용할 수 없습니다.
        /// </summary>
        /// <param name="name">생성할 NetworkChannel의 고유한 이름.</param>
        /// <param name="sessionGenerator">Session 객체를 생성할 핸들러. null이면 기본 Session 객체가 생성됩니다.</param>
        /// <returns>생성된 NetworkChannel 객체</returns>
        public static NetworkChannel CreateChannel(string name, SessionGenerateHandler sessionGenerator)
        {
            lock (Channels)
            {
                if (Channels.Exists(name))
                    throw new AegisException(AegisResult.AlreadyExistName, "'{0}' is already exists channel name.", name);


                NetworkChannel channel = new NetworkChannel(name);
                if (sessionGenerator == null)
                    channel.SessionGenerator = delegate { return GenerateSession("Aegis.Network.Session"); };
                else
                    channel.SessionGenerator = sessionGenerator;
                Channels.Add(name, channel);

                return channel;
            }
        }


        /// <summary>
        /// TreeNode에 정의된 데이터를 기준으로 NetworkChannel 객체를 생성합니다.
        /// TreeNode에는 name, sessionClass, maxSessionPoolCount, listenIpAddress, listenPortNo가 정의되어있어야 합니다.
        /// </summary>
        /// <param name="node">생성할 NetworkChannel의 데이터가 정의된 TreeNode</param>
        /// <returns>생성된 NetworkChannel 객체</returns>
        public static NetworkChannel CreateChannelFromNode(TreeNode<string> node)
        {
            lock (Channels)
            {
                string channelName = node.GetValue("name").ToString();
                if (Channels.Exists(channelName))
                    throw new AegisException(AegisResult.AlreadyExistName, "'{0}' is already exists channel name.", node.Name);


                NetworkChannel channel = new NetworkChannel(channelName);
                channel._configNode = node;
                channel.SessionGenerator = delegate { return GenerateSession(node.GetValue("sessionClass")); };
                channel.MaxSessionCount = node.GetValue("maxSessionCount").ToInt32();
                channel.Acceptor.ListenIpAddress = node.GetValue("listenIpAddress");
                channel.Acceptor.ListenPortNo = node.GetValue("listenPortNo").ToInt32();
                Channels.Add(channelName, channel);

                return channel;
            }
        }


        /// <summary>
        /// 생성된 모든 NetworkChannel을 종료하고 사용중인 리소스를 반환합니다.
        /// </summary>
        public static void Release()
        {
            lock (Channels)
            {
                foreach (var channel in Channels.Values)
                {
                    channel.Acceptor.Close();

                    List<Session> sessions = new List<Session>();
                    foreach (var session in channel.ActiveSessions)
                        sessions.Add(session);

                    foreach (var session in sessions)
                        session.Close(AegisResult.Ok);
                }

                Channels.Clear();
            }
        }


        private NetworkChannel(string name)
        {
            Name = name;
            Acceptor = new Acceptor(this);
        }


        public void Close()
        {
            lock (Channels)
            {
                Acceptor.Close();

                //  session.Close 에서 ActiveSessions가 변경되므로 복사본을 사용
                foreach (var session in ActiveSessions.ToList())
                    session.Close(AegisResult.Ok);
            }
        }


        internal Session PopInactiveSession()
        {
            lock (Channels)
            {
                if (MaxSessionCount > 0 &&
                    ActiveSessions.Count + InactiveSessions.Count >= MaxSessionCount)
                    return null;


                if (InactiveSessions.Count == 0)
                {
                    Session session = SessionGenerator();
                    session.Activated += SessionActivated;
                    session.Inactivated += SessionInactivated;

                    InactiveSessions.Add(session);
                }

                return InactiveSessions[0];
            }
        }


        private static Session GenerateSession(string sessionClassName)
        {
            Type type = Framework.ExecutingAssembly.GetType(sessionClassName);
            if (type == null)
                throw new AegisException(AegisResult.InvalidArgument, "'{0}' session class is not exists.", sessionClassName);

            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null);
            if (constructorInfo == null)
                throw new AegisException(AegisResult.InvalidArgument, "'No matches constructor in '{0}'.", sessionClassName);


            Session session = constructorInfo.Invoke(null) as Session;
            return session;
        }


        private void SessionActivated(Session session)
        {
            lock (Channels)
            {
                InactiveSessions.Remove(session);
                ActiveSessions.Add(session);
            }
        }


        private void SessionInactivated(Session session)
        {
            lock (Channels)
            {
                ActiveSessions.Remove(session);
                InactiveSessions.Add(session);
            }
        }
    }
}
