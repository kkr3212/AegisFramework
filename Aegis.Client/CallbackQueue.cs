using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aegis.Client.Network;



namespace Aegis.Client
{
    public delegate void APICallbackHandler<T>(T response) where T : Response;





    internal class CallbackQueue
    {
        private Dictionary<Int32, Action<SecurePacket>> _callbacks = new Dictionary<Int32, Action<SecurePacket>>();
        private Queue<SecurePacket> _receivedPackets = new Queue<SecurePacket>();





        public CallbackQueue()
        {
        }


        public void Clear()
        {
            lock (this)
            {
                _callbacks.Clear();
                _receivedPackets.Clear();
            }
        }


        public void AddCallback(Int32 key, Action<SecurePacket> callback)
        {
            lock (this)
            {
                if (_callbacks.ContainsKey(key) == true)
                    _callbacks[key] = callback;
                else
                    _callbacks.Add(key, callback);
            }
        }


        public void AddPacket(SecurePacket packet)
        {
            lock (this)
            {
                _receivedPackets.Enqueue(packet);
            }
        }


        public void DoCallback()
        {
            lock (this)
            {
                while (_receivedPackets.Count() > 0)
                {
                    SecurePacket packet = _receivedPackets.Dequeue();
                    Int32 key = packet.SeqNo;
                    Action<SecurePacket> callback;


                    packet.SkipHeader();
                    if (_callbacks.TryGetValue(key, out callback) == true)
                    {
                        if (callback != null)
                            callback(packet);
                        _callbacks.Remove(key);
                    }
                }
            }
        }
    }
}
