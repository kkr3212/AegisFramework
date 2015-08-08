using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Converter
{
    public static class DateTimeConverter
    {
        /// <summary>
        /// DateTime을 UnixTimeStamp 값으로 변환합니다.
        /// </summary>
        /// <returns>UnixTimeStamp</returns>
        public static Double UnixTimeStamp(this DateTime dt)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1);
            return dt.Subtract(dt1970).TotalSeconds;
        }


        /// <summary>
        /// UnixTimeStamp 값을 DateTime으로 변환합니다.
        /// </summary>
        /// <returns>UnixTimeStamp를 변환한 DateTime값</returns>
        public static DateTime ToDateTime(this Double unixTimeStamp)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1);
            return dt1970.AddSeconds(TimeSpan.FromSeconds(unixTimeStamp).TotalSeconds);
        }
    }
}
