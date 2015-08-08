using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Converter
{
    public static class StringConverter
    {
        public static Boolean ToBoolean(this String src)
        {
            if (ToInt16(src) == 0)
                return false;

            return true;
        }


        public static Int16 ToInt16(this String src)
        {
            Int16 val;
            if (Int16.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static Int16 ToUInt16(this String src)
        {
            Int16 val;
            if (Int16.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static Int32 ToInt32(this String src)
        {
            Int32 val;
            if (Int32.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static Int32 ToUInt32(this String src)
        {
            Int32 val;
            if (Int32.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static Int64 ToInt64(this String src)
        {
            Int64 val;
            if (Int64.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static UInt64 ToUInt64(this String src)
        {
            UInt64 val;
            if (UInt64.TryParse(src, out val) == false)
                return 0;

            return val;
        }


        public static Double ToDouble(this String src)
        {
            Double val;
            if (Double.TryParse(src, out val) == false)
                return 0;

            return val;
        }
    }
}
