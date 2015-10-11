using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis.Threading
{
    public delegate void ThreadStartWithCancel(CancellationToken token);
    public static class ThreadFactory
    {
        private class ThreadInfo
        {
            public readonly Thread Thread;
            public readonly ThreadStartWithCancel Start;
            public readonly CancellationTokenSource CTS;

            public ThreadInfo(ThreadStartWithCancel start)
            {
                Thread = new Thread(Runner);
                Start = start;
                CTS = new CancellationTokenSource();
            }
        }
        private static List<ThreadInfo> _threads = new List<ThreadInfo>();
        public static Int32 Count { get { return _threads.Count(); } }






        public static Thread NewThread(ThreadStartWithCancel start)
        {
            lock (_threads)
            {
                ThreadInfo newThread = new ThreadInfo(start);
                _threads.Add(newThread);

                return newThread.Thread;
            }
        }


        private static void Runner()
        {
            ThreadInfo myThreadInfo;


            lock (_threads)
            {
                myThreadInfo = _threads.Find(v => v.Thread == Thread.CurrentThread);
            }


            if (myThreadInfo != null)
                myThreadInfo.Start(myThreadInfo.CTS.Token);
        }


        public static Thread CallPeriodically(Int32 periodByMillisecond, Func<Boolean> func)
        {
            Thread thread = NewThread((cancelToken) =>
            {
                while (cancelToken.IsCancellationRequested == false)
                {
                    try
                    {
                        if (cancelToken.WaitHandle.WaitOne(periodByMillisecond) == true ||
                            func() == false)
                            break;
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.Write(LogType.Err, 1, e.ToString());
                    }
                }
            });
            thread.Start();

            return thread;
        }


        public static void Stop(Thread thread)
        {
            lock (_threads)
            {
                ThreadInfo myThreadInfo = _threads.Find(v => v.Thread == thread);
                if (myThreadInfo == null)
                    return;


                myThreadInfo.CTS.Cancel();
                if (myThreadInfo.Thread.Join(1000) == false)
                    myThreadInfo.Thread.Abort();
                myThreadInfo.CTS.Dispose();

                _threads.Remove(myThreadInfo);
            }
        }


        public static void StopAllThreads()
        {
            lock (_threads)
            {
                foreach (ThreadInfo threadInfo in _threads)
                {
                    threadInfo.CTS.Cancel();
                    if (threadInfo.Thread.Join(1000) == false)
                        threadInfo.Thread.Abort();
                    threadInfo.CTS.Dispose();
                }

                _threads.Clear();
            }
        }
    }
}
