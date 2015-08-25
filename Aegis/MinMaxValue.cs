using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Aegis
{
    /// <summary>
    /// 값의 변경내역 가운데서 최대/최소값을 확인할 수 있습니다.
    /// </summary>
    /// <typeparam name="T">값의 형식</typeparam>
    [DebuggerDisplay("Min={Min}, Max={Max}, Value={Value}")]
    public class MinMaxValue<T> where T : struct, IComparable<T>
    {
        private T? _value;

        public T Min { get; private set; }
        public T Max { get; private set; }
        public T Value
        {
            get { return (_value == null ? default(T) : _value.Value); }
            set
            {
                if (_value == null)
                {
                    _value = value;
                    Min = _value.Value;
                    Max = _value.Value;
                }
                else
                {
                    _value = value;
                    if (Min.CompareTo(_value.Value) > 0)
                        Min = _value.Value;
                    if (Max.CompareTo(_value.Value) < 0)
                        Max = _value.Value;
                }
            }
        }
    }
}
