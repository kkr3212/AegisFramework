using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using Aegis.Threading;



namespace Aegis.Network.Rest
{
    public delegate void RequestHandler(RestRequest request, HttpListenerResponse response);

    public enum HttpMethodType
    {
        Get,
        Post
    }





    public class RestAPIServer
    {
        private Thread _thread;
        public HttpListener HttpListener { get; private set; } = new HttpListener();
        private Dictionary<string, RequestHandler> _routes = new Dictionary<string, RequestHandler>();
        private RWLock _lock = new RWLock();

        public delegate void InvalidRouteDelegator(RestRequest request, HttpListenerResponse response);
        public InvalidRouteDelegator InvalidRouteHandler { get; set; }





        public RestAPIServer()
        {
        }


        /// <summary>
        /// 사용할 URL 및 포트정보를 지정합니다.
        /// http:// 혹은 https:// 로 시작되어야 하며, /로 끝나는 URI로 지정해야 합니다.
        /// (ex. http://*:8080/)
        /// Start를 호출하기 전에 먼저 호출되어야 합니다.
        /// </summary>
        /// <param name="prefix"></param>
        public void AddPrefix(string prefix)
        {
            HttpListener.Prefixes.Add(prefix);
        }


        public void Start()
        {
            if (_thread != null)
                throw new AegisException(AegisResult.AlreadyInitialized);


            HttpListener.Start();

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

            HttpListener.Stop();
            _thread = null;
        }


        public void Route(string path, RequestHandler handler)
        {
            if (path[0] != '/')
                throw new AegisException(AegisResult.InvalidArgument, "The path must be a string that starts with '/'.");

            if (path.Length > 1 && path[path.Length - 1] == '/')
                path = path.Remove(path.Length - 1);


            using (_lock.WriterLock)
            {
                if (_routes.ContainsKey(path.ToLower()) == true)
                    throw new AegisException(AegisResult.InvalidArgument, "'{0}' is already exists route path.", path);

                _routes.Add(path.ToLower(), handler);
            }
        }


        private void Run()
        {
            while (HttpListener.IsListening)
            {
                try
                {
                    ProcessContext(HttpListener.GetContext());
                }
                catch (Exception e) when ((uint)e.HResult == 0x80004005)
                {
                    break;
                }
                catch (Exception e) when ((uint)e.HResult == 0x80131509)
                {
                    Logger.Err(LogMask.Aegis, e.ToString());
                    break;
                }
                catch (Exception e)
                {
                    Logger.Err(LogMask.Aegis, e.ToString());
                }
            }
        }


        private void ProcessContext(HttpListenerContext context)
        {
            string path, rawUrl = context.Request.RawUrl;
            if (rawUrl == "")
                return;


            string[] splitUrl = rawUrl.Split('?');
            string rawMessage;
            RestRequest request = null;


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
                    request = new RestRequest(HttpMethodType.Get, rawUrl, path, rawMessage);
                }
                else
                    request = new RestRequest(HttpMethodType.Get, rawUrl, path, "");
            }
            if (context.Request.HttpMethod == "POST")
            {
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    rawMessage = reader.ReadToEnd();
                    request = new RestRequest(HttpMethodType.Post, rawUrl, path, rawMessage);
                }
            }
            if (request == null)
                return;


            //  Routing
            RequestHandler handler;
            using (_lock.ReaderLock)
            {
                if (_routes.TryGetValue(path, out handler) == false)
                {
                    InvalidRouteHandler?.Invoke(request, context.Response);
                    return;
                }
            }


            SpinWorker.Work(() =>
            {
                handler(request, context.Response);
            });
        }
    }
}
