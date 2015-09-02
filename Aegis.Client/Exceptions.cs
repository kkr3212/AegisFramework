using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Aegis.Client
{
    public class AegisException : Exception
    {
        public int ResultCodeNo { get; private set; }



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
}
