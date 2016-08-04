using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Converter
{
    public static class StringConverter
    {
        public static bool ToBoolean(this string src)
        {
            if (ToInt16(src) == 0)
                return false;

            return true;
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
    }
}
