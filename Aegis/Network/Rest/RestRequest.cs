using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;



namespace Aegis.Network.Rest
{
    public sealed class RestRequest
    {
        public readonly HttpMethodType MethodType;
        public readonly string RawUrl, Path, MessageBody;

        public Dictionary<string, string> Arguments { get; } = new Dictionary<string, string>();
        public string this[string key] { get { return Arguments[key]; } }





        internal RestRequest(HttpMethodType methodType, string url, string path, string messageBody)
        {
            MethodType = methodType;
            RawUrl = url;
            Path = path;
            MessageBody = messageBody;
        }


        public void ParseArguments()
        {
            Arguments.Clear();

            foreach (string arg in MessageBody.Split('&'))
            {
                if (arg.Length == 0)
                    continue;


                string[] keyValue = arg.Split('=');
                if (keyValue.Length != 2)
                    throw new AegisException(AegisResult.InvalidArgument);


                Arguments[keyValue[0]] = keyValue[1];
            }
        }
    }
}
