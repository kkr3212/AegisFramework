using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Aegis.Calculate
{
    [DebuggerDisplay("Current={_value}, Max={MaxValue}, Start={StartValue}")]
    public sealed class SequentialNumber
    {
        private int _value = -1;
        public int StartValue { get; private set; }
        public int MaxValue { get; private set; }





        public SequentialNumber()
        {
            StartValue = 0;
            MaxValue = int.MaxValue;
        }


        public SequentialNumber(int startValue, int maxValue)
        {
            StartValue = startValue;
            MaxValue = maxValue;

            _value = startValue - 1;
        }


        public int NextNumber()
        {
            lock (this)
            {
                if (++_value > MaxValue)
                    _value = StartValue;

                return _value;
            }
        }
    }
}
