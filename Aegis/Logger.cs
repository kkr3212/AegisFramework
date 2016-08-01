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
        Err = 0x04,
        Debug = 0x08
    }

    public static class LogLevel
    {
        public const int Core = 0;
        public const int Info = 1;
        public const int Debug = 2;
        public const int LowData = 3;
    }





    public delegate void LogWriteHandler(LogType type, int level, string log);
    public static class Logger
    {
        public static int EnabledLogLevel { get; set; } = LogLevel.Debug;
        public static LogType EnabledType { get; set; } = LogType.Info | LogType.Warn | LogType.Err;
        public static event LogWriteHandler Written;
        public static int DefaultLogLevel { get; set; } = LogLevel.Info;





        public static void Write(LogType type, int level, string format, params object[] args)
        {
            if ((EnabledType & type) != type || level > EnabledLogLevel)
                return;

            Written?.Invoke(type, level, string.Format(format, args));
        }


        public static void Info(string format, params object[] args)
        {
            Write(LogType.Info, DefaultLogLevel, format, args);
        }


        public static void Warn(string format, params object[] args)
        {
            Write(LogType.Warn, DefaultLogLevel, format, args);
        }


        public static void Err(string format, params object[] args)
        {
            Write(LogType.Err, DefaultLogLevel, format, args);
        }


        public static void Debug(string format, params object[] args)
        {
            Write(LogType.Debug, DefaultLogLevel, format, args);
        }


        internal static void RemoveAll()
        {
            if (Written != null)
            {
                foreach (Delegate d in Written.GetInvocationList())
                    Written -= (LogWriteHandler)d;

                Written = null;
            }
        }
    }
}
