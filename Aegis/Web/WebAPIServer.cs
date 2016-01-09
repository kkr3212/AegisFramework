using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using Aegis.Threading;



namespace Aegis.Web
{
    public delegate void RequestHandler(WebAPIRequest request, HttpListenerResponse response);

    public enum WebMethodType
    {
        Get,
        Post
    }





    public class WebAPIServer
    {
        private Thread _thread;
        private HttpListener _listener = new HttpListener();
        private Dictionary<String, RequestHandler> _routes = new Dictionary<String, RequestHandler>();
        private RWLock _lock = new RWLock();





        public WebAPIServer()
        {
        }


        /// <summary>
        /// WebAPIServer가 사용할 URL 및 포트정보를 지정합니다.
        /// http:// 혹은 https:// 로 시작되어야 하며, /로 끝나는 URI로 지정해야 합니다.
        /// (ex. http://*:8080/)
        /// Start를 호출하기 전에 먼저 호출되어야 합니다.
        /// </summary>
        /// <param name="prefix"></param>
        public void AddPrefix(String prefix)
        {
            _listener.Prefixes.Add(prefix);
        }


        public void Start()
        {
            if (_thread != null)
                throw new AegisException(AegisResult.AlreadyInitialized);


            _listener.Start();

            _thread = new Thread(Run);
            _thread.Start();
        }


        public void Join()
        {
            if (_thread == null)
                throw new AegisException(AegisResult.NotInitialized);

            _thread.Join();
        }


        public void Stop()
        {
            if (_thread == null)
                throw new AegisException(AegisResult.NotInitialized);

            _listener.Stop();
            _thread = null;
        }


        public void Route(String path, RequestHandler handler)
        {
            if (path[0] != '/')
                throw new AegisException(AegisResult.InvalidArgument, "The path must be a string that starts with '/'.");

            if (path.Length > 1 && path[path.Length - 1] == '/')
                path = path.Remove(path.Length - 1);


            using (_lock.WriterLock)
            {
                if (_routes.ContainsKey(path.ToLower()) == true)
                {
                    //  #! throw exception
                    return;
                }

                _routes.Add(path.ToLower(), handler);
            }
        }


        private void Run()
        {
            while (_listener.IsListening)
            {
                try
                {
                    ProcessContext(_listener.GetContext());
                }
                catch (Exception e) when ((UInt32)e.HResult == 0x80004005)
                {
                    break;
                }
                catch (Exception e) when ((UInt32)e.HResult == 0x80131509)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                    break;
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
            }
        }


        private void ProcessContext(HttpListenerContext context)
        {
            String path, rawUrl = context.Request.RawUrl;
            if (rawUrl == "")
                return;


            String[] splitUrl = rawUrl.Split('?');
            String rawMessage;
            WebAPIRequest request = null;


            //  Path 가져오기
            path = splitUrl[0].ToLower();
            if (path.Length == 0)
                return;

            if (path.Length > 1 && path[path.Length - 1] == '/')
                path = path.Remove(path.Length - 1);


            //  Query / Message Body 가져오기
            if (context.Request.HttpMethod == "GET")
            {
                if (splitUrl.Length > 1)
                {
                    rawMessage = splitUrl[1];
                    request = new WebAPIRequest(WebMethodType.Get, rawUrl, path, rawMessage);
                }
                else
                    request = new WebAPIRequest(WebMethodType.Get, rawUrl, path, "");
            }
            if (context.Request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    rawMessage = reader.ReadToEnd();
                    request = new WebAPIRequest(WebMethodType.Post, rawUrl, path, rawMessage);
                }
            }


            //  Routing
            RequestHandler handler;
            if (request == null)
                return;


            using (_lock.ReaderLock)
            {
                if (_routes.TryGetValue(path, out handler) == false)
                    return;
            }


            SpinWorker.Work(() =>
            {
                handler(request, context.Response);
            });
        }
    }
}
