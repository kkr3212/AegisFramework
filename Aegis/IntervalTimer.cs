using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Aegis
{
    public static class IntervalTimer
    {
        private class TimeElapsedData
        {
            public String Name { get; }
            public Stopwatch Stopwatch { get; }



            public TimeElapsedData(String name)
            {
                Name = name;
                Stopwatch = new Stopwatch();
            }
        }

        private static Dictionary<String, TimeElapsedData> _timeElapseds = new Dictionary<String, TimeElapsedData>();





        public static void StartElapsedChecker(String name)
        {
            TimeElapsedData data = new TimeElapsedData(name);
            data.Stopwatch.Start();


            lock (_timeElapseds)
            {
                _timeElapseds[name] = data;
            }
        }


        public static Int64 StopElapsedChecker(String name)
        {
            TimeElapsedData data;
            lock (_timeElapseds)
            {
                if (_timeElapseds.TryGetValue(name, out data) == false)
                    return 0;

                _timeElapseds.Remove(name);
            }

            return data.Stopwatch.ElapsedMilliseconds;
        }


        public static void SetIntervalCall(Int64 millisecondsInterval, Action action)
        {
        }
    }
}
