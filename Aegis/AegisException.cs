using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public class AegisException : Exception
    {
        public int ResultCodeNo { get; private set; }



        public AegisException()
        {
        }


        public AegisException(string message)
            : base(message)
        {
        }


        public AegisException(int resultCode)
        {
            ResultCodeNo = resultCode;
        }


        public AegisException(int resultCode, string message)
            : base(message)
        {
            ResultCodeNo = resultCode;
        }


        public AegisException(Exception innerException, string message)
            : base(message, innerException)
        {
        }


        public AegisException(Exception innerException, int resultCode)
            : base("", innerException)
        {
            ResultCodeNo = resultCode;
        }


        public AegisException(int resultCode, Exception innerException, string message)
            : base(message, innerException)
        {
            ResultCodeNo = resultCode;
        }


        public AegisException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }


        public AegisException(int resultCode, string message, params object[] args)
            : base(string.Format(message, args))
        {
            ResultCodeNo = resultCode;
        }


        public AegisException(Exception innerException, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }


        public AegisException(int resultCode, Exception innerException, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            ResultCodeNo = resultCode;
        }


        public override string ToString()
        {
            string msg = string.Format("{0}\r\nResultCodeNo={1}\r\n{2}",
                base.Message, ResultCodeNo, StackTrace);

            return msg;
        }
    }


    public class WaitResponseTimeoutException : AegisException
    {
        public WaitResponseTimeoutException(string message, params object[] args)
            : base(message, args)
        {
        }
    }


    public class JobCanceledException : AegisException
    {
        public JobCanceledException(string message, params object[] args)
            : base(message, args)
        {
        }
    }
}
