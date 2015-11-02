using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public enum LogType
    {
        Info = 0x01,
        Warn = 0x02,
        Err = 0x04
    }





    public delegate void LogWriteHandler(LogType type, Int32 level, String log);
    public static class Logger
    {
        public static Int32 EnabledLevel { get; set; } = 0xFFFF;
        public static LogType EnabledType { get; set; } = LogType.Info | LogType.Warn | LogType.Err;
        public static event LogWriteHandler Written;





        public static void Write(LogType type, Int32 level, String format, params object[] args)
        {
            if ((EnabledType & type) != type || level > EnabledLevel)
                return;

            if (Written != null)
                Written(type, level, String.Format(format, args));
        }
    }
}
