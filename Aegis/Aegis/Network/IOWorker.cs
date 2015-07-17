using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Network
{
    internal enum IOType
    {
        Accept = 0,
        Connect,
        Close,
        Send,
        Receive
    }


    internal class SessionJob
    {
        public IOType Type { get; set; }
        public Session TargetSession { get; set; }
    }





    internal class IOWorker
    {
    }
}
