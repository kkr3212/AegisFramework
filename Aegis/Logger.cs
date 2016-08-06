using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public static class LogMask
    {
        public const int Info = 0x0001;
        public const int Warn = 0x0002;
        public const int Err = 0x0004;
        public const int Debug = 0x0008;

        public const int Aegis = 0x0010;
    }





    public delegate void LogWriteHandler(int mask, string log);
    public static class Logger
    {
        public static int EnableMask { get; set; }
            = LogMask.Info | LogMask.Warn | LogMask.Err | LogMask.Debug | LogMask.Aegis;
        public static int DefaultMask { get; set; } = LogMask.Info | LogMask.Aegis;
        public static event LogWriteHandler Written;





        public static void Write(int mask, string format, params object[] args)
        {
            if ((EnableMask & mask) != mask)
                return;

            Written?.Invoke(mask, string.Format(format, args));
        }


        public static void Info(string format, params object[] args)
        {
            Write(DefaultMask, format, args);
        }


        public static void Warn(string format, params object[] args)
        {
            Write(DefaultMask, format, args);
        }


        public static void Err(string format, params object[] args)
        {
            Write(DefaultMask, format, args);
        }


        public static void Debug(string format, params object[] args)
        {
            Write(DefaultMask, format, args);
        }


        public static void Info(int mask, string format, params object[] args)
        {
            Write(LogMask.Info | mask, format, args);
        }


        public static void Warn(int mask, string format, params object[] args)
        {
            Write(LogMask.Warn | mask, format, args);
        }


        public static void Err(int mask, string format, params object[] args)
        {
            Write(LogMask.Err | mask, format, args);
        }


        public static void Debug(int mask, string format, params object[] args)
        {
            Write(LogMask.Debug | mask, format, args);
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
