using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using Aegis;



namespace Aegis.Network
{
    public class SessionManager
    {
        internal NetworkChannel NetworkChannel { get; private set; }
        private List<Session> _inactiveSessions;
        private Dictionary<Int32, Session> _activeSessions;
        private Int32 _sessionId = 0, _activeSessionCount;

        /// <summary>
        /// Session 객체를 생성하는 Delegator를 설정합니다.
        /// SessionManager에서는 내부적으로 Session Pool을 관리하는데, Pool에 객체가 부족할 때 이 Delegator가 호출됩니다.
        /// 그러므로 이 Delegator에서는 ObjectPool 대신 new를 사용해 인스턴스를 생성하는 것이 좋습니다.
        /// </summary>
        internal SessionGenerateDelegator SessionGenerator = delegate { return new Session(1024); };
        public Int32 MaxSessionPoolSize { get; internal set; }
        public Int32 ActiveSessionCount { get { return _activeSessionCount; } }
        public List<Session> ActiveSessions
        {
            get
            {
                lock (this)
                    return _activeSessions.Select(v => v.Value).ToList();
            }
        }





        internal SessionManager(NetworkChannel networkChannel)
        {
            NetworkChannel = networkChannel;
            _inactiveSessions = new List<Session>();
            _activeSessions = new Dictionary<Int32, Session>();
        }


        internal void CreatePool(Int32 sessionCount)
        {
            lock (this)
            {
                for (Int32 i = 0 ; i < sessionCount ; ++i)
                {
                    if (_inactiveSessions.Count() + _activeSessions.Count() >= MaxSessionPoolSize)
                        break;


                    Session session = SessionGenerator();
                    session.SessionManager = this;
                    _inactiveSessions.Add(session);
                }
            }
        }


        internal void Release()
        {
            lock (this)
            {
                foreach (Session session in _activeSessions.Select(v => v.Value))
                {
                    session.SessionManager = null;
                    session.CloseSocket();
                }


                _activeSessions.Clear();
                _inactiveSessions.Clear();
                _activeSessionCount = 0;
            }
        }


        internal Session AttackSocket(Socket socket)
        {
            Session session;


            //  Inactive queue에서 Session 객체를 가져온다.
            //  잔여 Session이 없을 경우, 새로 생성한다.
            lock (this)
            {
                if (_inactiveSessions.Count == 0)
                {
                    if (MaxSessionPoolSize > 0 && _activeSessions.Count() >= MaxSessionPoolSize)
                        return null;


                    Interlocked.Increment(ref _sessionId);
                    session = SessionGenerator();
                    session.SessionManager = this;
                }
                else
                {
                    session = _inactiveSessions.First();
                    _inactiveSessions.Remove(session);
                }
            }


            session.Clear();
            session.Socket = socket;

            return session;
        }


        internal void ActivateSession(Session session)
        {
            lock (this)
            {
                _inactiveSessions.Remove(session);
                _activeSessions.Add(session.SessionId, session);

                _activeSessionCount = _activeSessions.Count();
            }
        }


        internal void InactivateSession(Session session)
        {
            lock (this)
            {
                _activeSessions.Remove(session.SessionId);
                _inactiveSessions.Add(session);

                _activeSessionCount = _activeSessions.Count();
            }
        }
    }
}
