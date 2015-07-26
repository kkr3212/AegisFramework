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


        public const Int32 UnknownError = 1;            //  정의되지 않은 오류입니다.
        public const Int32 InvalidArgument = 2;         //  잘못된 인자 값이 지정되었습니다.
        public const Int32 ActivatedSession = 3;        //  이미 활성화된 세션입니다.
        public const Int32 NotEnoughBuffer = 4;         //  버퍼 용량이 부족합니다.
        public const Int32 AlreadyExistName = 5;        //  동일한 이름이 존재합니다.
        public const Int32 NoNetworkChannelName = 6;    //  존재하지 않는 NetworkChannel 이름입니다.
        public const Int32 NetworkError = 7;            //  네트워크 관련 에러가 발생했습니다. (InnerException 참고)
        public const Int32 AcceptorIsRunning = 8;       //  Acceptor가 이미 실행중입니다.
        public const Int32 JobCanceled = 9;             //  진행중인 작업이 취소되었습니다.
    }
}
