using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public static class AegisResult
    {
        public const Int32 Ok = 0;


        public const Int32 UnknownError = 1;            //  정의되지 않은 오류입니다.
        public const Int32 InvalidArgument = 2;         //  잘못된 인자 값이 지정되었습니다.
        public const Int32 ActivatedSession = 3;        //  이미 활성화된 세션입니다.
        public const Int32 BufferUnderflow = 4;         //  대상버퍼의 크기가 부족합니다.
        public const Int32 BufferOverflow = 5;          //  대상버퍼의 크기가 부족합니다.
        public const Int32 AlreadyExistName = 6;        //  동일한 이름이 존재합니다.
        public const Int32 NoNetworkChannelName = 7;    //  존재하지 않는 NetworkChannel 이름입니다.
        public const Int32 NetworkError = 8;            //  네트워크 관련 에러가 발생했습니다. (InnerException 참고)
        public const Int32 AcceptorIsRunning = 9;       //  Acceptor가 이미 실행중입니다.
        public const Int32 JobCanceled = 10;            //  진행중인 작업이 취소되었습니다.
        public const Int32 NotInitialized = 11;         //  초기화되지 않은 객체입니다.
        public const Int32 AlreadyInitialized = 12;     //  이미 초기화된 객체입니다.

        public const Int32 MySqlConnectionFailed = 100; //  MySql Database에 접속할 수 없습니다.
        public const Int32 DataReaderNotClosed = 101;   //  DataReader가 사용중입니다. 먼저 진행중인 쿼리를 종료해야 합니다.
    }
}
