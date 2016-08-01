using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.IO
{
    public enum IOEventType
    {
        Accept,
        Connect,
        Write,
        Read,
        Close
    }


    public delegate void IOEventHandler(IOEventResult result);





    public class IOEventResult
    {
        public readonly object Sender;
        public readonly IOEventType EventType;
        public readonly byte[] Buffer;
        public readonly int Result;



        public IOEventResult(object sender, IOEventType type, int result)
        {
            Sender = sender;
            EventType = type;
            Result = result;
        }


        public IOEventResult(object sender, IOEventType type, byte[] buffer, int result)
        {
            Sender = sender;
            EventType = type;
            Result = result;
            if (buffer != null)
            {
                Buffer = new byte[buffer.Length];
                Array.Copy(buffer, Buffer, Buffer.Length);
            }
        }


        public IOEventResult(object sender, IOEventType type, byte[] buffer, int startIndex, int length, int result)
        {
            Sender = sender;
            EventType = type;
            Result = result;
            if (buffer != null)
            {
                Buffer = new byte[length];
                Array.Copy(buffer, startIndex, Buffer, 0, Buffer.Length);
            }
        }
    }
}
