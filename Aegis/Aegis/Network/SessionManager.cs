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
        private Queue<Session> _inactiveSessions;
        private Dictionary<Int32, Session> _activeSessions;
        private Int32 _sessionId = 0, _activeSessionCount;

        public Int32 ActiveSessionCount { get { return _activeSessionCount; } }


        public SessionGenerator GenerateSession = delegate { return new Session(1024); };





        internal SessionManager(NetworkChannel networkChannel)
        {
            NetworkChannel = networkChannel;
            _inactiveSessions = new Queue<Session>();
            _activeSessions = new Dictionary<Int32, Session>();
        }


        public Session ActivateSession(Socket socket)
        {
            Session session;


            //  Inactive queue에서 Session 객체를 가져온다.
            //  잔여 Session이 없을 경우, 새로 생성한다.
            lock (this)
            {
                if (_inactiveSessions.Count == 0)
                {
                    Interlocked.Increment(ref _sessionId);
                    session = GenerateSession();
                }
                else
                    session = _inactiveSessions.Dequeue();

                _activeSessions.Add(session.SessionId, session);
                _activeSessionCount = _activeSessions.Count();
            }


            session.Clear();
            session.OnSessionClosed = InactivateSession;
            session.Socket = socket;

            return session;
        }


        private void InactivateSession(Session session)
        {
            lock (this)
            {
                //  Inactive queue로 반환
                _activeSessions.Remove(session.SessionId);
                _inactiveSessions.Enqueue(session);


                _activeSessionCount = _activeSessions.Count();
            }
        }


        public void Clear()
        {
            //  #!  모든 Active Session을 종료시키는 기능이 있으면...
        }
    }
}
