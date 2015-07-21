using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    public enum LogType
    {
        Info = 0x01,
        Warn = 0x02,
        Err = 0x04
    }



    public interface ILogMedia
    {
        void Write(LogType type, Int32 level, String log);
        void Release();
    }





    public static class Logger
    {
        public static Int32 EnabledLevel { get; set; }
        public static LogType EnabledType { get; set; }
        private static List<ILogMedia> _listMedia = new List<ILogMedia>();



        static Logger()
        {
            EnabledLevel = 0xFFFF;
            EnabledType = LogType.Info | LogType.Warn | LogType.Err;
        }


        /// <summary>
        /// 사용할 LogMedia를 추가합니다.
        /// 이미 추가된 인스턴스인 경우 무시됩니다.
        /// </summary>
        /// <param name="media">추가할 LogMedia 인스턴스</param>
        public static void AddLogger(ILogMedia media)
        {
            lock (_listMedia)
            {
                if (_listMedia.Contains(media) == false)
                    _listMedia.Add(media);
            }
        }


        /// <summary>
        /// LogMedia 목록에서 지정한 LogMedia를 제거합니다.
        /// 정상적으로 제거된 경우 해당 LogMedia의 Release가 호출됩니다.
        /// </summary>
        /// <param name="media">제거할 LogMedia 인스턴스</param>
        public static void RemoveLogger(ILogMedia media)
        {
            lock (_listMedia)
            {
                if (_listMedia.Remove(media) == true)
                    media.Release();
            }
        }


        /// <summary>
        /// 전체 LogMedia를 제거하고 사용중인 리소스를 반환합니다.
        /// 사용중인 LogMedia의 Release가 호출됩니다.
        /// </summary>
        public static void Release()
        {
            lock (_listMedia)
            {
                _listMedia.ForEach(v => v.Release());
                _listMedia.Clear();
            }
        }


        /// <summary>
        /// 추가된 ILogMedia 전체에 로그 문자열을 전달합니다.
        /// </summary>
        /// <param name="type">로그의 종류. EnabledType에 정의되지 않으면 무시됩니다.</param>
        /// <param name="level">로그의 레벨. EnabledLevel보다 큰 경우 무시됩니다.</param>
        /// <param name="format">로그 문자열</param>
        /// <param name="args">로그 문자열의 인자</param>
        public static void Write(LogType type, Int32 level, String format, params object[] args)
        {
            if ((EnabledType & type) != type || level > EnabledLevel)
                return;


            lock (_listMedia)
            {
                String log = String.Format(format, args);
                _listMedia.ForEach(v => v.Write(type, level, log));
            }
        }
    }
}
