using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Network
{
    internal class SessionJob : WorkerThreadItem
    {
        public IOType Type { get; set; }
        public Session TargetSession { get; set; }
        public Int32 Value { get; set; }





        private SessionJob()
        {
        }


        public static SessionJob NewJob(IOType type, Session session, Int32 value)
        {
            SessionJob job = ObjectPool<SessionJob>.Pop();
            job.Type = type;
            job.TargetSession = session;
            job.Value = value;

            return job;
        }


        public void DoJob()
        {
            //  이벤트 처리 중 다른 이벤트가 동시에 처리되지 않도록 처리
            lock (TargetSession)
            {
                TargetSession.DoSessionJob(this);
            }

            ObjectPool<SessionJob>.Push(this);
        }
    }
}
