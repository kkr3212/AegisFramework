using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public class Definitions
    {
        public const Int32 BuildNo = 2;
    }



    internal enum IOType
    {
        Accept = 0,
        Connect,
        Close,
        Send,
        Receive
    }
}
