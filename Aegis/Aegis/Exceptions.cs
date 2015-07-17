using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public class AegisException : Exception
    {
        public Int32 ResultCode { get; private set; }



        public AegisException()
        {
        }


        public AegisException(String message)
            : base(message)
        {
        }


        public AegisException(Int32 resultCode, String message)
            : base(message)
        {
            ResultCode = resultCode;
        }


        public AegisException(Exception innerException, String message)
            : base(message, innerException)
        {
        }


        public AegisException(Int32 resultCode, Exception innerException, String message)
            : base(message, innerException)
        {
            ResultCode = resultCode;
        }


        public AegisException(String message, params object[] args)
            : base(String.Format(message, args))
        {
        }


        public AegisException(Int32 resultCode, String message, params object[] args)
            : base(String.Format(message, args))
        {
            ResultCode = resultCode;
        }


        public AegisException(Exception innerException, String message, params object[] args)
            : base(String.Format(message, args), innerException)
        {
        }


        public AegisException(Int32 resultCode, Exception innerException, String message, params object[] args)
            : base(String.Format(message, args), innerException)
        {
            ResultCode = resultCode;
        }
    }


    public class WaitResponseTimeoutException : AegisException
    {
        public WaitResponseTimeoutException(String message, params object[] args)
            : base(message, args)
        {
        }
    }
}
