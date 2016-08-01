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


        public void Cancel(int millisecondsTimeout = 1000)
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


        public async void CancelAsync(int millisecondsTimeout = 1000)
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
