using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Aegis;
using Aegis.Network;



namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Logger.AddLogger(new LogConsole());


                NetworkChannel nc = NetworkChannel.CreateChannel("test");
                nc.StartNetwork("", 10100);
            }
            catch (AegisException e)
            {
                Logger.Write(LogType.Err, 2, e.ToString());
            }
        }
    }





    public class LogConsole : ILogMedia
    {
        public void Write(LogType type, Int32 level, String log)
        {
            Console.WriteLine(log);
        }


        public void Release()
        {
        }
    }
}
