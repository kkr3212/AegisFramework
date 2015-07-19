using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis
{
    public interface WorkerThreadItem
    {
        void DoJob();
    }





    public class WorkerThread
    {
        private SafeQueue<WorkerThreadItem> _works = new SafeQueue<WorkerThreadItem>();
        private Boolean _running;
        private Thread[] _threads;

        public String Name { get; private set; }
        public Int32 QueuedCount { get { return _works.QueuedCount; } }
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

                _running = true;
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
                if (_running == false || _threads == null)
                    return;


                _running = false;
                _works.Cancel();

                foreach (Thread th in _threads)
                    th.Join();
                _threads = null;
            }
        }


        public void Post(WorkerThreadItem item)
        {
            _works.Post(item);
        }


        private void Run()
        {
            while (_running)
            {
                try
                {
                    WorkerThreadItem item = _works.Pop();
                    if (item == null)
                        break;

                    item.DoJob();
                }
                catch (JobCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
            }
        }
    }
}
