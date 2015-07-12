using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public class AegisException : Exception
    {
        public AegisException()
        {
        }


        public AegisException(String message)
            : base(message)
        {
        }


        public AegisException(Exception innerException, String message)
            : base(message, innerException)
        {
        }


        public AegisException(String message, params object[] args)
            : base(String.Format(message, args))
        {
        }


        public AegisException(Exception innerException, String message, params object[] args)
            : base(String.Format(message, args), innerException)
        {
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
