using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis.Threading
{
    public static class ThreadExtend
    {
        private class ThreadCancel
        {
            public readonly Thread Thread;
            public readonly CancellationTokenSource CTS;

            public ThreadCancel(Thread thread)
            {
                Thread = thread;
                CTS = new CancellationTokenSource();
            }
        }
        private static List<ThreadCancel> _threads = new List<ThreadCancel>();
        public static Int32 Count { get { return _threads.Count(); } }





        public static void StartCancellable(this Thread thread)
        {
            ThreadCancel threadInfo;

            lock (_threads)
            {
                threadInfo = new ThreadCancel(thread);
                _threads.Add(threadInfo);
            }

            thread.Start(threadInfo.CTS.Token);
        }


        public static Thread CallPeriodically(Int32 periodByMillisecond, Func<Boolean> func)
        {
            Thread thread = new Thread((obj) =>
            {
                CancellationToken cancelToken = (CancellationToken)obj;
                while (cancelToken.IsCancellationRequested == false)
                {
                    try
                    {
                        if (cancelToken.WaitHandle.WaitOne(periodByMillisecond) == true ||
                            func() == false)
                            break;
                    }
                    catch (Exception e)
                    {
                        Logger.Write(LogType.Err, 1, e.ToString());
                    }
                }
            });
            thread.StartCancellable();

            return thread;
        }


        public static async void Cancel(this Thread thread)
        {
            await Task.Run(() =>
            {
                lock (_threads)
                {
                    ThreadCancel myThreadInfo = _threads.Find(v => v.Thread == thread);
                    if (myThreadInfo == null)
                        return;


                    myThreadInfo.CTS.Cancel();
                    if (myThreadInfo.Thread.Join(1000) == false)
                        myThreadInfo.Thread.Abort();
                    myThreadInfo.CTS.Dispose();

                    _threads.Remove(myThreadInfo);
                }
            });
        }


        public static async Task CancelAll()
        {
            await Task.Run(() =>
            {
                lock (_threads)
                {
                    foreach (ThreadCancel threadInfo in _threads)
                    {
                        threadInfo.CTS.Cancel();
                        if (threadInfo.Thread.Join(1000) == false)
                            threadInfo.Thread.Abort();
                        threadInfo.CTS.Dispose();
                    }

                    _threads.Clear();
                }
            });
        }
    }
}
