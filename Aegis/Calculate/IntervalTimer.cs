using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Aegis.Threading;



namespace Aegis.Calculate
{
    public class IntervalTimer : IDisposable
    {
        public readonly string Name;
        public readonly int Interval;
        private long _lastCallTime, _remainCount = -1;
        private Action _action;

        public static NamedObjectIndexer<IntervalTimer> Timers = new NamedObjectIndexer<IntervalTimer>();
        private static Thread _timerThread;
        private static EventWaitHandle _threadWait = new EventWaitHandle(false, EventResetMode.AutoReset);
        private static List<IntervalTimer> _queue = new List<IntervalTimer>();
        private static Stopwatch _stopwatch = Stopwatch.StartNew();
        private static RWLock _lock = new RWLock();





        public IntervalTimer(int interval, Action action)
        {
            Name = "";
            Interval = interval;
            _action = action;
        }


        public IntervalTimer(string name, int interval, Action action)
        {
            lock (Timers)
                Timers.Add(name, this);

            Name = name;
            Interval = interval;
            _action = action;
        }


        /// <summary>
        /// 호출 횟수를 지정합니다.
        /// </summary>
        /// <param name="count">-1을 설정하면 무한호출이 되며, 1 이상의 값은 횟수를 지정합니다. 0은 허용되지 않습니다.</param>
        /// <returns></returns>
        public IntervalTimer SetCallCount(int count)
        {
            if (count == 0)
                throw new AegisException(AegisResult.InvalidArgument);

            _remainCount = count;
            return this;
        }


        public void Start()
        {
            using (_lock.WriterLock)
            {
                if (_queue.Find(v => v == this) != null)
                    throw new AegisException(AegisResult.TimerIsRunning);


                _lastCallTime = _stopwatch.ElapsedMilliseconds - Interval;
                _queue.Add(this);


                //  동작중인 쓰레드가 없으면 생성
                if (_timerThread == null)
                {
                    _timerThread = new Thread(TimerThreadRunner);
                    _timerThread.Start();
                }
            }
        }


        public void StartAfter(int delay)
        {
            using (_lock.WriterLock)
            {
                if (_queue.Find(v => v == this) != null)
                    throw new AegisException(AegisResult.TimerIsRunning);


                _lastCallTime = _stopwatch.ElapsedMilliseconds + delay;
                _queue.Add(this);


                //  동작중인 쓰레드가 없으면 생성
                if (_timerThread == null)
                {
                    _timerThread = new Thread(TimerThreadRunner);
                    _timerThread.Start();
                }
            }
        }


        public void Stop()
        {
            using (_lock.WriterLock)
            {
                _queue.Remove(this);


                //  대기중인 작업이 없으면 쓰레드 종료
                if (_queue.Count == 0)
                {
                    _timerThread = null;
                    _threadWait.Set();
                }
            }
        }


        public void Dispose()
        {
            Stop();

            _lastCallTime = 0;
            _action = null;

            lock (Timers)
                Timers.Remove(Name);
        }


        internal static void DisposeAll()
        {
            lock (Timers)
            {
                List<IntervalTimer> timers = new List<IntervalTimer>();
                foreach (var timer in Timers.Values)
                    timers.Add(timer);
                foreach (var timer in _queue)
                    timers.Add(timer);


                foreach (var timer in timers)
                    timer.Dispose();
                _queue.Clear();
            }
        }


        private static void TimerThreadRunner()
        {
            MinMaxValue<long> sleepTime = new MinMaxValue<long>(0);
            List<IntervalTimer> deprecated = new List<IntervalTimer>();


            while (true)
            {
                if (_threadWait.WaitOne((int)sleepTime.Min) == true)
                    break;


                using (_lock.ReaderLock)
                {
                    deprecated.Clear();
                    sleepTime.Reset(100);

                    foreach (var timer in _queue)
                    {
                        long remainTime = timer.Interval - (_stopwatch.ElapsedMilliseconds - timer._lastCallTime);
                        if (remainTime > 0)
                            sleepTime.Value = remainTime;
                        else
                        {
                            timer._lastCallTime = _stopwatch.ElapsedMilliseconds;
                            SpinWorker.Dispatch(timer._action);

                            //  호출횟수 만료
                            if (timer._remainCount != -1 &&
                                --timer._remainCount == 0)
                                deprecated.Add(timer);
                        }
                    }
                }

                foreach (var timer in deprecated)
                    timer.Dispose();
            }
            _timerThread = null;
        }
    }
}
