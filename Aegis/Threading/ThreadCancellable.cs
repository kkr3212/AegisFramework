using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis.Threading
{
    public class ThreadCancellable
    {
        public Thread Thread { get; private set; }
        private readonly CancellationTokenSource CTS = new CancellationTokenSource();





        public ThreadCancellable()
        {
        }


        public void Start(ParameterizedThreadStart start)
        {
            Thread = new Thread(start);
            Thread.Start(CTS.Token);
        }


        public void CallPeriodically(Int32 periodByMillisecond, Func<Boolean> func)
        {
            Thread = new Thread(() =>
            {
                while (CTS.Token.IsCancellationRequested == false)
                {
                    try
                    {
                        if (CTS.Token.WaitHandle.WaitOne(periodByMillisecond) == true ||
                            func() == false)
                            break;
                    }
                    catch (Exception e)
                    {
                        Logger.Write(LogType.Err, 1, e.ToString());
                    }
                }

                lock (this)
                {
                    CTS.Dispose();
                }
            });
            Thread.Start();
        }


        public async void Cancel()
        {
            await Task.Run(() =>
            {
                lock (this)
                {
                    CTS.Cancel();
                    if (Thread.Join(1000) == false)
                        Thread.Abort();
                }
            });
        }
    }
}
