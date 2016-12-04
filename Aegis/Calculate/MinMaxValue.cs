using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Aegis.Calculate
{
    /// <summary>
    /// 값의 변경내역 가운데서 최대/최소값을 확인할 수 있습니다.
    /// </summary>
    /// <typeparam name="T">값의 형식</typeparam>
    [DebuggerDisplay("Min={Min}, Max={Max}, Value={Value}")]
    public sealed class MinMaxValue<T> where T : struct, IComparable<T>
    {
        private T _value;

        public T Min { get; private set; }
        public T Max { get; private set; }
        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (Min.CompareTo(_value) > 0)
                    Min = _value;
                if (Max.CompareTo(_value) < 0)
                    Max = _value;
            }
        }


        /// <summary>
        /// 초기값을 지정하는 생성자입니다.
        /// </summary>
        /// <param name="initialValue">Min, Max에 대한 초기값을 설정합니다.</param>
        public MinMaxValue(T initialValue)
        {
            Min = initialValue;
            Max = initialValue;
        }


        /// <summary>
        /// Min, Max 값을 재설정합니다.
        /// </summary>
        /// <param name="initialValue">재설정할 Min, Max 값입니다.</param>
        public void Reset(T initialValue)
        {
            Min = initialValue;
            Max = initialValue;
        }
    }
}
