using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aegis.IO;



namespace Aegis.Network
{
    public class AwaitableMethod
    {
        private struct TCSData
        {
            public ushort packetId;
            public Func<Packet, bool> predicate;
            public TaskCompletionSource<Packet> tcs;
        }
        private List<TCSData> _listTCS = new List<TCSData>();
        private TaskCompletionSource<bool> _tcsConnect;
        private Session _session;





        internal AwaitableMethod(Session session)
        {
            _session = session;
            _session.EventConnect += NetworkConnected;
            _session.EventClose += NetworkClosed;
        }


        private void NetworkConnected(IOEventResult result)
        {
            if (_tcsConnect != null)
            {
                _tcsConnect.SetResult(result.Result == AegisResult.Ok);
                _tcsConnect = null;
            }
        }


        private void NetworkClosed(IOEventResult result)
        {
            lock (_listTCS)
            {
                foreach (TCSData data in _listTCS)
                    data.tcs.SetCanceled();

                _listTCS.Clear();
            }


            if (_tcsConnect != null)
            {
                _tcsConnect.SetException(new AegisException("Connection closed when trying ConnectAndWait()"));
                _tcsConnect = null;
            }
        }


        public async Task<bool> Connect(string ipAddress, int portNo)
        {
            bool ret = false;

            _tcsConnect = new TaskCompletionSource<bool>();
            _session.Connect(ipAddress, portNo);
            await Task.Run(() => ret = _tcsConnect.Task.Result);

            return ret;
        }


        public bool ProcessResponseWaitPacket(Packet packet)
        {
            lock (_listTCS)
            {
                foreach (TCSData data in _listTCS)
                {
                    if (data.packetId == packet.PacketId
                        && (data.predicate == null || data.predicate(packet) == true))
                    {
                        data.tcs.SetResult(new Packet(packet));
                        _listTCS.Remove(data);

                        return true;
                    }
                }
            }

            return false;
        }


        public virtual async Task<Packet> SendAndWaitResponse(Packet packet, ushort responsePacketId)
        {
            TaskCompletionSource<Packet> tcs = new TaskCompletionSource<Packet>();
            TCSData data = new TCSData() { packetId = responsePacketId, tcs = tcs, predicate = null };
            Packet response = null;


            lock (_listTCS)
            {
                _listTCS.Add(data);
            }


            await Task.Run(() =>
            {
                try
                {
                    _session.SendPacket(packet);
                    response = tcs.Task.Result;
                }
                catch (Exception)
                {
                    //  Nothing to do.
                }
            });

            return response;
        }


        public virtual async Task<Packet> SendAndWaitResponse(Packet packet, ushort responsePacketId, int timeout)
        {
            TaskCompletionSource<Packet> tcs = new TaskCompletionSource<Packet>();
            CancellationTokenSource cancel = new CancellationTokenSource();
            TCSData data = new TCSData() { packetId = responsePacketId, tcs = tcs, predicate = null };
            Packet response = null;


            lock (_listTCS)
            {
                _listTCS.Add(data);
            }


            //  Task.Result의 Timeout 처리
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(timeout, cancel.Token);
                    tcs.SetCanceled();
                }
                catch (Exception)
                {
                }
            });

            //  Packet Send & Response 작업
            await Task.Run(() =>
            {
                try
                {
                    _session.SendPacket(packet);
                    response = tcs.Task.Result;
                    cancel.Cancel();
                }
                catch (Exception)
                {
                    //  Task가 Cancel된 경우 추가된 작업(data)을 삭제한다.
                    _listTCS.Remove(data);
                }
            });

            cancel.Dispose();


            if (response == null)
                throw new WaitResponseTimeoutException("The waiting time of ResponsePacketId(0x{0:X}) has expired.", responsePacketId);


            return response;
        }


        public virtual async Task<Packet> SendAndWaitResponse(Packet packet, ushort responsePacketId, Func<Packet, bool> predicate)
        {
            TaskCompletionSource<Packet> tcs = new TaskCompletionSource<Packet>();
            TCSData data = new TCSData() { packetId = responsePacketId, tcs = tcs, predicate = predicate };
            Packet response = null;


            lock (_listTCS)
            {
                _listTCS.Add(data);
            }


            await Task.Run(() =>
            {
                try
                {
                    _session.SendPacket(packet);
                    response = tcs.Task.Result;
                }
                catch (Exception)
                {
                    //  Nothing to do.
                }
            });

            return response;
        }


        public virtual async Task<Packet> SendAndWaitResponse(Packet packet, ushort responsePacketId, Func<Packet, bool> predicate, int timeout)
        {
            TaskCompletionSource<Packet> tcs = new TaskCompletionSource<Packet>();
            CancellationTokenSource cancel = new CancellationTokenSource();
            TCSData data = new TCSData() { packetId = responsePacketId, tcs = tcs, predicate = predicate };
            Packet response = null;


            lock (_listTCS)
            {
                _listTCS.Add(data);
            }


            //  Task.Result의 Timeout 처리
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(timeout, cancel.Token);
                    tcs.SetCanceled();
                }
                catch (Exception)
                {
                }
            });

            //  Packet Send & Response 작업
            await Task.Run(() =>
            {
                try
                {
                    _session.SendPacket(packet);
                    response = tcs.Task.Result;
                    cancel.Cancel();
                }
                catch (Exception)
                {
                    //  Task가 Cancel된 경우 추가된 작업(data)을 삭제한다.
                    _listTCS.Remove(data);
                }
            });

            cancel.Dispose();


            if (response == null)
                throw new WaitResponseTimeoutException("The waiting time of ResponsePacketId(0x{0:X}) has expired.", responsePacketId);

            return response;
        }
    }
}
