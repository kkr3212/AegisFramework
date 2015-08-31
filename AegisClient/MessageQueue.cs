using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Aegis.Client
{
    internal class MessageData
    {
        public MessageType Type;
        public StreamBuffer Buffer;
        public int Size;
    }





    internal class MessageQueue
    {
        private List<MessageData> _queue;
        public int Count { get { return _queue.Count(); } }





        public MessageQueue()
        {
            _queue = new List<MessageData>();
        }


        public void Clear()
        {
            lock (_queue)
            {
                _queue.Clear();
                Monitor.PulseAll(_queue);
            }
        }


        public void AddFirst(MessageType type, StreamBuffer buffer, int size)
        {
            lock (_queue)
            {
                MessageData data = new MessageData();
                data.Type = type;
                data.Buffer = buffer;
                data.Size = size;


                _queue.Insert(0, data);
                Monitor.Pulse(_queue);
            }
        }


        public void Add(MessageType type, StreamBuffer buffer, int size)
        {
            lock (_queue)
            {
                MessageData data = new MessageData();
                data.Type = type;
                data.Buffer = buffer;
                data.Size = size;


                _queue.Add(data);
                Monitor.Pulse(_queue);
            }
        }


        public MessageData Pop(int timeout)
        {
            lock (_queue)
            {
                while (_queue.Count == 0)
                {
                    if (Monitor.Wait(_queue, timeout) == false)
                        return null;
                }


                MessageData data = _queue.First();
                _queue.Remove(data);
                return data;
            }
        }
    }
}