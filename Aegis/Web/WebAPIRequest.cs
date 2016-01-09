using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;



namespace Aegis.Web
{
    public sealed class WebAPIRequest
    {
        public readonly WebMethodType MethodType;
        public readonly String RawUrl, Path, MessageBody;

        public Dictionary<String, String> Arguments { get; } = new Dictionary<String, String>();
        public String this[String key] { get { return Arguments[key]; } }





        internal WebAPIRequest(WebMethodType methodType, String url, String path, String messageBody)
        {
            MethodType = methodType;
            RawUrl = url;
            Path = path;
            MessageBody = messageBody;
        }


        public void ParseArguments()
        {
            Arguments.Clear();

            foreach (String arg in MessageBody.Split('&'))
            {
                if (arg.Length == 0)
                    continue;


                String[] keyValue = arg.Split('=');
                if (keyValue.Length != 2)
                    throw new AegisException(AegisResult.InvalidArgument);


                Arguments[keyValue[0]] = keyValue[1];
            }
        }
    }
}
