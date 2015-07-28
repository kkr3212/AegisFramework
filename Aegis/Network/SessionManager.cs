﻿using System;
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


        internal SessionGenerateDelegator SessionGenerator = delegate { return new Session(1024); };
        /// <summary>
        /// 최대 값으로 지정된 Session의 개수를 가져옵니다.
        /// 이 값은 NetworkChannel을 Start할 때 지정한 값입니다.
        /// </summary>
        public Int32 MaxSessionPoolSize { get; internal set; }
        /// <summary>
        /// 현재 활성화 상태인 Session 개수를 가져옵니다.
        /// </summary>
        public Int32 ActiveSessionCount { get { return _activeSessionCount; } }
        /// <summary>
        /// 현재 활성화 상태인 Session 목록을 가져옵니다.
        /// </summary>
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