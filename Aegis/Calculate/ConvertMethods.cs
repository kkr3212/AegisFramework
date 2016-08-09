using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Calculate
{
    public static class ConvertMethods
    {
        #region string
        public static bool ToBoolean(this string src)
        {
            if (ToInt16(src) == 1 || src.ToLower() == "true")
                return true;

            return false;
        }


        public static short ToInt16(this string src, short defaultValue = 0)
        {
            short val;
            if (short.TryParse(src, out val) == false)
                return defaultValue;

            return val;
        }


        public static ushort ToUInt16(this string src, ushort defaultValue = 0)
        {
            ushort val;
            if (ushort.TryParse(src, out val) == false)
                return defaultValue;

            return val;
        }


        public static int ToInt32(this string src, int defaultValue = 0)
        {
            int val;
            if (int.TryParse(src, out val) == false)
                return defaultValue;

            return val;
        }


        public static uint ToUInt32(this string src, uint defaultValue = 0)
        {
            uint val;
            if (uint.TryParse(src, out val) == false)
                return defaultValue;

            return val;
        }


        public static long ToInt64(this string src, long defaultValue = 0)
        {
            long val;
            if (long.TryParse(src, out val) == false)
                return defaultValue;

            return val;
        }


        public static ulong ToUInt64(this string src, ulong defaultValue = 0)
        {
            ulong val;
            if (ulong.TryParse(src, out val) == false)
                return defaultValue;

            return val;
        }


        public static double ToDouble(this string src, double defaultValue = 0)
        {
            double val;
            if (double.TryParse(src, out val) == false)
                return defaultValue;

            return val;
        }
        #endregion





        #region int
        public static bool CheckBitMask(this int src, int mask)
        {
            return ((src & mask) == mask);
        }
        #endregion





        #region DateTime
        /// <summary>
        /// DateTime을 UnixTimeStamp 값으로 변환합니다.
        /// </summary>
        /// <returns>UnixTimeStamp</returns>
        public static double UnixTimeStamp(this DateTime dt)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1);
            return dt.Subtract(dt1970).TotalSeconds;
        }


        /// <summary>
        /// UnixTimeStamp 값을 DateTime으로 변환합니다.
        /// </summary>
        /// <returns>UnixTimeStamp를 변환한 DateTime값</returns>
        public static DateTime ToDateTime(this double unixTimeStamp)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1);
            return dt1970.AddSeconds(TimeSpan.FromSeconds(unixTimeStamp).TotalSeconds);
        }
        #endregion
    }
}
