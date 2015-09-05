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


        public AegisException(string message)
            : base(message)
        {
        }


        public AegisException(Exception innerException, string message)
            : base(message, innerException)
        {
        }


        public AegisException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }


        public AegisException(Exception innerException, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }
    }
}
