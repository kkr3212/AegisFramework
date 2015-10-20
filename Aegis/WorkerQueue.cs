using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis.Threading;



namespace Aegis
{
    public static class WorkerQueue
    {
        private static WorkerThread _workerThread = new WorkerThread("WorkerQueue");
        public static Int32 ThreadCount { get { return _workerThread.ThreadCount; } }
        public static Int32 QueuedCount { get { return _workerThread.QueuedCount; } }





        internal static void Initialize(Int32 threadCount)
        {
            if (threadCount > 0)
                _workerThread.Start(threadCount);
        }


        internal static void Release()
        {
            _workerThread.Stop();
        }


        public static void Post(Action action)
        {
            if (_workerThread.ThreadCount > 0)
                _workerThread.Post(action);
            else
                action();
        }


        public static void Posts(params Action[] actions)
        {
            if (_workerThread.ThreadCount > 0)
            {
                _workerThread.Post(() =>
                {
                    foreach (Action action in actions)
                        action();
                });
            }
            else
            {
                foreach (Action action in actions)
                    action();
            }
        }
    }
}
