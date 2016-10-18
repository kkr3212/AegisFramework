using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis.SystemDll;



namespace Aegis.Calculate
{
    public class HighResolutionTimer : IDisposable
    {
        private long _frequency, _startTime;
        public long Frequency { get { return _frequency; } }
        public long StartTime { get { return _startTime; } }
        public bool IsHighResolution { get; private set; }
        public double ElapsedSeconds
        {
            get
            {
                if (_startTime == 0)
                    return 0;

                if (IsHighResolution == false)
                    return System.Environment.TickCount - _startTime;


                long now;
                if (Kernel32.QueryPerformanceCounter(out now) == false)
                    throw new AegisException(AegisResult.UnknownError, "QueryPerformanceCounter error.");

                return (double)(now - _startTime) / (double)_frequency;
            }
        }
        public List<double> Laps { get; private set; } = new List<double>();


        public static NamedObjectIndexer<HighResolutionTimer> Items = new NamedObjectIndexer<HighResolutionTimer>();
        public readonly string Name = "";





        public HighResolutionTimer()
        {
            _startTime = 0;
            if (Kernel32.QueryPerformanceFrequency(out _frequency) == true && _frequency != 1000)
                IsHighResolution = true;
            else
                IsHighResolution = false;
        }


        public HighResolutionTimer(string name)
        {
            _startTime = 0;
            if (Kernel32.QueryPerformanceFrequency(out _frequency) == true && _frequency != 1000)
                IsHighResolution = true;
            else
                IsHighResolution = false;


            lock (Items)
            {
                Name = name;
                Items.Add(name, this);
            }
        }


        public void Start()
        {
            if (IsHighResolution)
            {
                if (Kernel32.QueryPerformanceCounter(out _startTime) == false)
                    throw new AegisException(AegisResult.UnknownError, "QueryPerformanceCounter error.");
            }
            else
                _startTime = System.Environment.TickCount;
        }


        public void Stop()
        {
            _startTime = 0;
            Laps.Clear();
        }


        public void Restart()
        {
            Stop();
            Start();
        }


        public void Lap()
        {
            var elapsed = ElapsedSeconds;
            Laps.Add(elapsed);
        }


        public void Dispose()
        {
            Stop();
            lock (Items)
                Items.Remove(Name);
        }


        public static HighResolutionTimer StartNew()
        {
            HighResolutionTimer item = new HighResolutionTimer();
            item.Start();

            return item;
        }


        public static double GetElapsedSeconds(long frequency, long startTime)
        {
            if (startTime == 0)
                return 0;

            long now;
            if (Kernel32.QueryPerformanceCounter(out now) == false)
                throw new AegisException(AegisResult.UnknownError, "QueryPerformanceCounter error.");

            return (double)(now - startTime) / (double)frequency;
        }
    }
}
