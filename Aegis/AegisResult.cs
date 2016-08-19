using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public static class AegisResult
    {
        public const int Ok = 0;


        public const int UnknownError = 1;            //  정의되지 않은 오류입니다.
        public const int InvalidArgument = 2;         //  잘못된 인자 값이 지정되었습니다.
        public const int ActivatedSession = 3;        //  이미 활성화된 세션입니다.
        public const int BufferUnderflow = 4;         //  대상버퍼의 크기가 부족합니다.
        public const int BufferOverflow = 5;          //  대상버퍼의 크기가 부족합니다.
        public const int AlreadyExistName = 6;        //  동일한 이름이 존재합니다.
        public const int NotExistName = 7;            //  존재하지 않는 이름입니다.
        public const int NetworkError = 9;            //  네트워크 관련 에러가 발생했습니다. (InnerException 참고)
        public const int AcceptorIsRunning = 10;      //  Acceptor가 이미 실행중입니다.
        public const int JobCanceled = 11;            //  진행중인 작업이 취소되었습니다.
        public const int NotInitialized = 12;         //  초기화되지 않은 객체입니다.
        public const int AlreadyInitialized = 13;     //  이미 초기화된 객체입니다.
        public const int ClosedByRemote = 15;         //  핸들이 원격지에 의해 종료되었습니다.
        public const int ClosedByUser = 16;           //  핸들이 사용자에 의해 종료되었습니다.
        public const int TimerIsRunning = 17;         //  타이머가 이미 실행중입니다.
        public const int ConnectionFailed = 18;       //  대상 호스트에 접속할 수 없습니다.

        public const int MySqlConnectionFailed = 100; //  MySql Database에 접속할 수 없습니다.
        public const int DataReaderNotClosed = 101;   //  DataReader가 사용중입니다. 먼저 진행중인 쿼리를 종료해야 합니다.
    }
}
