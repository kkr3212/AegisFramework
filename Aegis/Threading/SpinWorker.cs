using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Threading
{
    public static class SpinWorker
    {
        private static WorkerThread _workerThread = new WorkerThread("WorkerQueue");
        private static WorkerThread _dispatchThread = new WorkerThread("Dispatch");

        public static int QueuedCount { get { return _workerThread.QueuedCount; } }
        public static int WorkerThreadCount
        {
            get { return _workerThread.ThreadCount; }
            set { IncreaseThread(_workerThread, value); }
        }
        public static int DispatchThreadCount
        {
            get { return _dispatchThread.ThreadCount; }
        }





        public static void Initialize()
        {
            IncreaseThread(_dispatchThread, 1);
            IncreaseThread(_workerThread, 4);
        }


        public static void Release()
        {
            _dispatchThread.Stop();
            _workerThread.Stop();
        }


        internal static void IncreaseThread(WorkerThread target, int threadCount)
        {
            int increaseCount = threadCount - target.ThreadCount;
            if (increaseCount < 1)
                return;

            target.Increase(increaseCount);
            target.Start();
        }


        /// <summary>
        /// actionWork 작업을 WorkerThread에서 실행합니다.
        /// </summary>
        /// <param name="actionWork">수행할 작업</param>
        public static void Work(Action actionWork)
        {
            if (WorkerThreadCount == 0)
                AegisTask.Run(actionWork);

            else
                _workerThread.Post(actionWork);
        }


        /// <summary>
        /// 비동기 백그라운드 작업과 결과처리 작업으로 이루어진 기능을 실행합니다.
        /// actionWork는 WorkerThread에 의해 비동기로 실행되고, actionWork 작업이 끝나면
        /// actionDispatch가 DispatchThread에서 실행됩니다.
        /// actionWork에서 exception이 발생되면 actionDispatch 작업은 실행되지 않습니다.
        /// </summary>
        /// <param name="actionWork">비동기로 실행할 작업</param>
        /// <param name="actionDispatch">DispatchThread에서 실행할 작업</param>
        public static void Work(Action actionWork, Action actionDispatch)
        {
            Work(() =>
            {
                actionWork();
                Dispatch(actionDispatch);
            });
        }


        /// <summary>
        /// actionDispatch 작업을 DispatchThread에서 실행합니다.
        /// </summary>
        /// <param name="actionDispatch">수행할 작업</param>
        public static void Dispatch(Action actionDispatch)
        {
            if (DispatchThreadCount == 0)
                AegisTask.Run(actionDispatch);

            else
                _dispatchThread.Post(actionDispatch);
        }
    }
}
