using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public static class ResultCode
    {
        public const Int32 Ok = 0;

        public const Int32 NetworkError = 0;            //  네트워크 관련 에러가 발생했습니다. (InnerException 참고)
        public const Int32 AcceptorIsRunning = 0;       //  Acceptor가 이미 실행중입니다.
    }
}
