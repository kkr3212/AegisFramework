using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis.Threading
{
    public class ThreadCancellable : IDisposable
    {
        public Thread Thread { get; private set; }
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();





        public ThreadCancellable(ParameterizedThreadStart start)
        {
            Thread = new Thread(start);
        }


        public void Start()
        {
            Thread.Start(_cts.Token);
        }


        public void Dispose()
        {
            _cts.Dispose();
        }


        public static ThreadCancellable CallInterval(Int32 millisecondsInterval, Func<Boolean> func)
        {
            ThreadCancellable thread = new ThreadCancellable((obj) =>
            {
                CancellationToken cancelToken = (CancellationToken)obj;
                while (cancelToken.IsCancellationRequested == false)
                {
                    try
                    {
                        if (cancelToken.WaitHandle.WaitOne(millisecondsInterval) == true ||
                            func() == false)
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


        public void Cancel(Int32 millisecondsTimeout = 1000)
        {
            try
            {
                _cts.Cancel();
                if (Thread.Join(millisecondsTimeout) == false)
                    Thread.Abort();
            }
            catch (Exception)
            {
            }

            Dispose();
        }


        public async void CancelAsync(Int32 millisecondsTimeout = 1000)
        {
            await Task.Run(() =>
            {
                try
                {
                    _cts.Cancel();
                    if (Thread.Join(millisecondsTimeout) == false)
                        Thread.Abort();
                }
                catch (Exception)
                {
                }

                Dispose();
            });
        }
    }
}
