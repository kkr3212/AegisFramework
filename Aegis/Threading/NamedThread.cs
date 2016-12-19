using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis.Threading
{
    public class NamedThread : IDisposable
    {
        public static NamedObjectIndexer<NamedThread> Threads = new NamedObjectIndexer<NamedThread>();
        private readonly string Name;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Thread _thread;





        private NamedThread(string name, Action<CancellationToken> action)
        {
            Name = name;
            lock (Threads)
                Threads.Add(name, this);

            _thread = new Thread(() =>
            {
                action(_cts.Token);
                _cts.Dispose();
                _thread = null;

                lock (Threads)
                    Threads.Remove(name);
            });
            _thread.Start();
        }


        public void Dispose()
        {
            if (_thread?.Join(1000) == false)
                _thread?.Abort();

            _cts?.Dispose();

            lock (Threads)
                Threads.Remove(Name);
        }


        internal static void DisposeAll()
        {
            lock (Threads)
            {
                foreach (var item in Threads.Items)
                    item.Item2.Dispose();
            }
        }


        public void Abort(int millisecondsTimeout = 1000)
        {
            try
            {
                lock (Threads)
                    Threads.Remove(Name);

                _cts.Cancel();
                if (_thread.Join(millisecondsTimeout) == false)
                    _thread.Abort();
            }
            catch (Exception)
            {
            }

            _cts.Dispose();
            _cts = null;
            _thread = null;
        }


        public async void AbortAsync(int millisecondsTimeout = 1000)
        {
            await Task.Run(() =>
            {
                try
                {
                    lock (Threads)
                        Threads.Remove(Name);

                    _cts.Cancel();
                    if (_thread.Join(millisecondsTimeout) == false)
                        _thread.Abort();
                }
                catch (Exception)
                {
                }

                _cts.Dispose();
                _cts = null;
                _thread = null;
            });
        }


        public static NamedThread Run(string name, Action<CancellationToken> action)
        {
            lock (Threads)
            {
                NamedThread namedThread = Threads[name];
                if (namedThread == null)
                    namedThread = new NamedThread(name, action);

                return namedThread;
            }
        }


        public static void Abort(string name, int millisecondsTimeout = 1000)
        {
            lock (Threads)
            {
                NamedThread namedThread = Threads[name];
                if (namedThread == null)
                    return;

                namedThread.Abort(millisecondsTimeout);
            }
        }
    }
}
