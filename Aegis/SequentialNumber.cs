using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Aegis
{
    [DebuggerDisplay("Current={_value}, Max={MaxValue}, Start={StartValue}")]
    public sealed class SequentialNumber
    {
        private Int32 _value = -1;
        public Int32 StartValue { get; private set; }
        public Int32 MaxValue { get; private set; }





        public SequentialNumber()
        {
            StartValue = 0;
            MaxValue = Int32.MaxValue;
        }


        public SequentialNumber(Int32 startValue, Int32 maxValue)
        {
            StartValue = startValue;
            MaxValue = maxValue;

            _value = startValue - 1;
        }


        public Int32 NextNumber()
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
