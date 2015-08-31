using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Aegis.Configuration
{
    internal class ConfigNetworkChannel
    {
        public String NetworkChannelName { get; internal set; }
        public String SessionClassName { get; internal set; }
        public Int32 ReceiveBufferSize { get; internal set; }
        public Int32 InitSessionPoolCount { get; internal set; }
        public Int32 MaxSessionPoolCount { get; internal set; }


        public String ListenIpAddress { get; internal set; }
        public Int32 ListenPortNo { get; internal set; }
        public Object Tag { get; set; }
    }


    [DebuggerDisplay("Name={Name} Value={Value} Childs={Childs.Count()}")]
    public class CustomData
    {
        public String Name { get; private set; }
        public String Value { get; internal set; }
        public List<CustomData> Childs { get; private set; }



        internal CustomData(String name)
        {
            Name = name;
            Childs = new List<CustomData>();
        }


        internal CustomData(String name, String value)
        {
            Name = name;
            Value = value;
            Childs = new List<CustomData>();
        }


        internal CustomData GetChild(String name)
        {
            return Childs.Where(v => v.Name == name).FirstOrDefault();
        }


        /// <summary>
        /// 지정된 Path에서 값을 가져옵니다.
        /// 지정한 Path가 XmlAttribute가 아닌 경우, null을 반환할 수 있습니다.
        /// </summary>
        /// <param name="path">구분자는 \ 혹은 / 를 사용할 수 있습니다.</param>
        /// <returns>지정된 Path에 정의된 값</returns>
        public String GetValue(String path)
        {
            String[] names = path.Split(new char[] { '\\', '/' });
            CustomData data = this;


            foreach (String name in names)
            {
                data = data.GetChild(name);
                if (data == null)
                    throw new AegisException(AegisResult.InvalidArgument, "Invalid node name({0}).", name);
            }

            return data.Value;
        }
    }
}
