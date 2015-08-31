using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Network
{
    internal class ResponseAlternator
    {
        private struct Data
        {
            public PacketDeterminator Determinator;
            public EventHandler_Receive Dispatcher;


            public Data(PacketDeterminator determinator, EventHandler_Receive dispatcher)
            {
                Determinator = determinator;
                Dispatcher = dispatcher;
            }
        }





        private readonly NetworkSession _session;
        private readonly List<Data> _listResponseAction;


        public ResponseAlternator(NetworkSession session)
        {
            _session = session;
            _listResponseAction = new List<Data>();
        }


        public void Add(PacketDeterminator determinator, EventHandler_Receive dispatcher)
        {
            _listResponseAction.Add(new Data(determinator, dispatcher));
        }


        public Boolean Dispatch(StreamBuffer buffer)
        {
            foreach (var data in _listResponseAction)
            {
                if (data.Determinator(buffer) == true)
                {
                    _listResponseAction.Remove(data);
                    data.Dispatcher(_session, buffer);
                    return true;
                }
            }

            return false;
        }
    }
}
