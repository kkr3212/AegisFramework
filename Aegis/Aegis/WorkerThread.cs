using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis
{
    public class WorkerThread
    {
        private class WorkItemQueue
        {
            private Queue<Action> _queue = new Queue<Action>();


            public void Post(Action item)
            {
                lock (_queue)
                {
                    _queue.Enqueue(item);
                    Monitor.PulseAll(_queue);
                }

            }


            public Action Pop()
            {
                lock (_queue)
                {
                    while (_queue.Count == 0)
                        Monitor.Wait(_queue);

                    return _queue.Dequeue();
                }
            }


            public void Clear()
            {
                lock (_queue)
                {
                    _queue.Clear();
                    Monitor.PulseAll(_queue);
                }
            }


            public Int32 Count()
            {
                lock (_queue)
                {
                    return _queue.Count();
                }
            }
        }





        private WorkItemQueue _works = new WorkItemQueue();
        private Boolean _isRun;
        private Thread[] _threads;

        public String Name { get; private set; }
        public Int32 QueuedCount { get { return _works.Count(); } }
        public Int32 ThreadCount { get { return (_threads == null ? 0 : _threads.Count()); } }





        public WorkerThread(String name)
        {
            Name = name;
        }


        public void Start(Int32 threadCount)
        {
            lock (this)
            {
                _works.Clear();

                _isRun = true;
                _threads = new Thread[threadCount];
                for (Int32 i = 0; i < threadCount; ++i)
                {
                    _threads[i] = new Thread(Run);
                    _threads[i].Name = String.Format("{0} {1}", Name, i);
                    _threads[i].Start();
                }
            }
        }


        public void Stop()
        {
            lock (this)
            {
                if (_isRun == false || _threads == null)
                    return;


                //  Thread를 즉시 종료시키기 위해 포스팅된 작업을 모두 삭제
                _works.Clear();
                _isRun = false;

                foreach (Thread th in _threads)
                    Post(null);

                _threads = null;
            }
        }


        public void Post(Action action)
        {
            _works.Post(action);
        }


        private void Run()
        {
            while (_isRun)
            {
                try
                {
                    Action action = _works.Pop();
                    if (action == null)
                        break;

                    action();
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
            }
        }
    }
}
