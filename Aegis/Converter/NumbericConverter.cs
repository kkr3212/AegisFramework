using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Converter
{
    public static class NumbericConverter
    {
        public static bool CheckMask(this int src, int mask)
        {
            return ((src & mask) == mask);
        }
    }
}
