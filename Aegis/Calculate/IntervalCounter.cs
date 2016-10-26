using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Aegis.Calculate
{
    [DebuggerDisplay("Name={Name} Value={Value}")]
    public sealed class IntervalCounter : IDisposable
    {
        public static NamedObjectIndexer<IntervalCounter> Counters = new NamedObjectIndexer<IntervalCounter>();

        public string Name { get; private set; }
        public int Interval { get; set; }
        public int ElapsedMilliseconds { get { return _sw.IsRunning ? (int)_sw.ElapsedMilliseconds : 0; } }
        private Stopwatch _sw;
        private int _prevValue, _curValue;


        public int Value
        {
            get
            {
                Check();
                return _prevValue;
            }
        }





        public IntervalCounter(int interval)
        {
            Name = null;
            _sw = new Stopwatch();
            Interval = interval;
            _prevValue = 0;
            _curValue = 0;
        }


        public IntervalCounter(string name, int interval)
        {
            Name = name;
            _sw = new Stopwatch();
            Interval = interval;
            _prevValue = 0;
            _curValue = 0;

            lock (Counters)
                Counters.Add(name, this);
        }


        public void Dispose()
        {
            Stop();

            lock (Counters)
                Counters.Remove(Name);
        }


        public void Start()
        {
            _sw.Start();
        }


        public void Stop()
        {
            _sw.Stop();
            _prevValue = 0;
            _curValue = 0;
        }


        public void Reset()
        {
            _sw.Reset();
            _prevValue = 0;
            _curValue = 0;
        }


        public void Restart()
        {
            _sw.Restart();
            _prevValue = 0;
            _curValue = 0;
        }


        public void Add(int value)
        {
            if (_sw.IsRunning == false)
                return;

            Check();
            Interlocked.Add(ref _curValue, value);
        }


        private void Check()
        {
            if (_sw.ElapsedMilliseconds < Interval)
                return;

            Interlocked.Exchange(ref _prevValue, _curValue);
            Interlocked.Exchange(ref _curValue, 0);
            _sw.Restart();
        }
    }
}
