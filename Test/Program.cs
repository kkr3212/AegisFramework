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
                nc.StartNetwork("192.168.0.100", 10100);

                Thread.Sleep(100000);
            }
            catch (AegisException e)
            {
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
