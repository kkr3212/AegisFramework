using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;



namespace Aegis.Network
{
    public class AwaitableMethod
    {
        private struct TCSData
        {
            public UInt16 packetId;
            public Func<Packet, Boolean> predicate;
            public TaskCompletionSource<Packet> tcs;
        }
        private List<TCSData> _listTCS = new List<TCSData>();
        private TaskCompletionSource<Boolean> _tcsConnect;
        private NetworkSession _session;





        internal AwaitableMethod(NetworkSession session)
        {
            _session = session;
            _session.NetworkEvent_Connected += OnConnected;
            _session.NetworkEvent_Closed += OnClosed;
        }


        private void OnConnected(NetworkSession session, bool connected)
        {
            if (_tcsConnect != null)
            {
                _tcsConnect.SetResult(connected);
                _tcsConnect = null;
            }
        }


        private void OnClosed(NetworkSession session)
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


        public async Task<Boolean> Connect(String ipAddress, Int32 portNo)
        {
            Boolean ret = false;

            _tcsConnect = new TaskCompletionSource<Boolean>();
            _session.Connect(ipAddress, portNo);
            await Task.Run(() => ret = _tcsConnect.Task.Result);

            return ret;
        }


        public Boolean ProcessResponseWaitPacket(Packet packet)
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


        public virtual async Task<Packet> SendAndWaitResponse(Packet packet, UInt16 responsePacketId)
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


        public virtual async Task<Packet> SendAndWaitResponse(Packet packet, UInt16 responsePacketId, Int32 timeout)
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


        public virtual async Task<Packet> SendAndWaitResponse(Packet packet, UInt16 responsePacketId, Func<Packet, Boolean> predicate)
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


        public virtual async Task<Packet> SendAndWaitResponse(Packet packet, UInt16 responsePacketId, Func<Packet, Boolean> predicate, Int32 timeout)
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
