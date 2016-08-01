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


        public static short ToInt16(this string src)
        {
            short val;
            if (short.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static short ToUInt16(this string src)
        {
            short val;
            if (short.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static int ToInt32(this string src)
        {
            int val;
            if (int.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static int ToUInt32(this string src)
        {
            int val;
            if (int.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static long ToInt64(this string src)
        {
            long val;
            if (long.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static ulong ToUInt64(this string src)
        {
            ulong val;
            if (ulong.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static double ToDouble(this string src)
        {
            double val;
            if (double.TryParse(src, out val) == false)
                return 0;

            return val;
        }
    }
}
