using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Network
{
    internal class ResponseSelector
    {
        private struct Data
        {
            public PacketCriterion Criterion;
            public EventHandler_Receive Dispatcher;


            public Data(PacketCriterion criterion, EventHandler_Receive dispatcher)
            {
                Criterion = criterion;
                Dispatcher = dispatcher;
            }
        }





        private readonly Session _session;
        private readonly List<Data> _listResponseAction;


        public ResponseSelector(Session session)
        {
            _session = session;
            _listResponseAction = new List<Data>();
        }


        public void Add(PacketCriterion criterion, EventHandler_Receive dispatcher)
        {
            _listResponseAction.Add(new Data(criterion, dispatcher));
        }


        public Boolean Dispatch(StreamBuffer buffer)
        {
            foreach (var data in _listResponseAction)
            {
                if (data.Criterion(buffer) == true)
                {
                    try
                    {
                        _listResponseAction.Remove(data);
                        data.Dispatcher(_session, buffer);
                    }
                    catch (Exception e)
                    {
                        Logger.Write(LogType.Err, 1, e.ToString());
                    }
                    return true;
                }
            }

            return false;
        }
    }
}
