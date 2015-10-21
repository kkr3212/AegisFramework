using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Aegis
{
    public sealed class IntervalCounter
    {
        private Stopwatch _sw;
        private Int32 _interval;
        private Int32 _prevValue, _curValue;


        public Int32 Intervel { get { return _interval; } set { _interval = value; } }
        public Int32 Value
        {
            get
            {
                Check();
                return _prevValue;
            }
        }





        public IntervalCounter(Int32 interval)
        {
            _sw = Stopwatch.StartNew();
            _interval = interval;
            _prevValue = 0;
            _curValue = 0;
        }


        public void Reset()
        {
            _sw = Stopwatch.StartNew();
            _prevValue = 0;
            _curValue = 0;
        }


        public void Add(Int32 value)
        {
            Check();
            Interlocked.Add(ref _curValue, value);
        }


        private void Check()
        {
            if (_sw.ElapsedMilliseconds < _interval)
                return;

            Interlocked.Exchange(ref _prevValue, _curValue);
            Interlocked.Exchange(ref _curValue, 0);
            _sw.Restart();
        }
    }
}
