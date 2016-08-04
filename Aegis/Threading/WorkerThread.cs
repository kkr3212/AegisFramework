using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis.Threading
{
    public sealed class WorkerThread
    {
        private BlockingQueue<Action> _works = new BlockingQueue<Action>();
        private bool _running;
        private Thread[] _threads;

        public string Name { get; private set; }
        public int QueuedCount { get { return _works.Count; } }
        public int ThreadCount { get { return _threads?.Count() ?? 0; } }





        public WorkerThread(string name)
        {
            Name = name;
        }


        public void Increase(int threadCount)
        {
            if (threadCount < 1)
                return;


            lock (this)
            {
                int index = 0;
                if (_threads == null)
                {
                    index = 0;
                    _threads = new Thread[threadCount];
                }
                else
                {
                    index = _threads.Count();

                    //  기존 Thread 객체를 새 배열로 복사
                    Thread[] temp = new Thread[_threads.Count() + threadCount];
                    for (int i = 0; i < _threads.Count(); ++i)
                        temp[i] = _threads[i];

                    _threads = temp;
                }

                for (int i = index; i < index + threadCount; ++i)
                {
                    _threads[i] = new Thread(Run);
                    _threads[i].Name = string.Format("{0} {1}", Name, i);
                }
            }
        }


        public void Start()
        {
            lock (this)
            {
                _running = true;
                foreach (var thread in _threads)
                {
                    if (thread.ThreadState != ThreadState.Running)
                        thread.Start();
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

                _works.Clear();
                _threads = null;
            }
        }


        public void Post(Action item)
        {
            _works.Enqueue(item);
        }


        private void Run()
        {
            while (_running)
            {
                try
                {
                    Action item = _works.Dequeue();
                    if (item == null)
                        break;

                    item();
                }
                catch (JobCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Logger.Err(LogMask.Aegis, e.ToString());
                }
            }
        }
    }
}
