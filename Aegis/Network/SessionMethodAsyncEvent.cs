using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Aegis.IO;



namespace Aegis.Network
{
    internal class SessionMethodAsyncEvent : ISessionMethod
    {
        private Session _session;
        private StreamBuffer _receivedBuffer, _dispatchBuffer;
        private SocketAsyncEventArgs _saeaRecv;
        private ResponseSelector _responseSelector;





        public SessionMethodAsyncEvent(Session session)
        {
            _session = session;
            _receivedBuffer = new StreamBuffer(2048);
            _dispatchBuffer = new StreamBuffer(2048);

            _saeaRecv = new SocketAsyncEventArgs();
            _saeaRecv.Completed += OnComplete_Receive;
            _responseSelector = new ResponseSelector(_session);
        }


        public void Clear()
        {
            _receivedBuffer.Clear();
            _dispatchBuffer.Clear();
        }


        public void WaitForReceive()
        {
            try
            {
                lock (_session)
                {
                    if (_session.Socket == null)
                        return;


                    if (_receivedBuffer.WritableSize == 0)
                        _receivedBuffer.Resize(_receivedBuffer.BufferSize * 2);

                    if (_session.Socket.Connected)
                    {
                        _saeaRecv.SetBuffer(_receivedBuffer.Buffer, _receivedBuffer.WrittenBytes, _receivedBuffer.WritableSize);
                        if (_session.Socket.ReceiveAsync(_saeaRecv) == false)
                            OnComplete_Receive(null, _saeaRecv);
                    }
                    else
                        _session.Close();
                }
            }
            catch (Exception)
            {
                _session.Close();
            }
        }


        private void OnComplete_Receive(object sender, SocketAsyncEventArgs saea)
        {
            try
            {
                lock (_session)
                {
                    //  transBytes가 0이면 원격지 혹은 네트워크에 의해 연결이 끊긴 상태
                    int transBytes = saea.BytesTransferred;
                    if (transBytes == 0)
                    {
                        _session.Close();
                        return;
                    }


                    _receivedBuffer.Write(transBytes);
                    while (_receivedBuffer.ReadableSize > 0)
                    {
                        _dispatchBuffer.Clear();
                        _dispatchBuffer.Write(_receivedBuffer.Buffer, _receivedBuffer.ReadBytes, _receivedBuffer.ReadableSize);


                        //  패킷 하나가 정상적으로 수신되었는지 확인
                        int packetSize;

                        _dispatchBuffer.ResetReadIndex();
                        if (_session.PacketValidator == null ||
                            _session.PacketValidator(_dispatchBuffer, out packetSize) == false)
                            break;

                        try
                        {
                            //  수신처리(Dispatch)
                            _receivedBuffer.Read(packetSize);
                            _dispatchBuffer.ResetReadIndex();


                            if (_responseSelector.Dispatch(_dispatchBuffer) == false)
                                _session.OnReceived(_dispatchBuffer);
                        }
                        catch (Exception e)
                        {
                            Logger.Write(LogType.Err, LogLevel.Core, e.ToString());
                        }
                    }


                    //  처리된 패킷을 버퍼에서 제거
                    _receivedBuffer.PopReadBuffer();

                    //  ReceiveBuffer의 안정적인 처리를 위해 작업이 끝난 후에 다시 수신대기
                    WaitForReceive();
                }
            }
            catch (SocketException)
            {
                _session.Close();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, LogLevel.Core, e.ToString());
            }
        }


        public void SendPacket(byte[] buffer, int offset, int size, Action<StreamBuffer> onSent = null)
        {
            try
            {
                lock (_session)
                {
                    if (_session.Socket == null)
                        return;


                    SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                    saea.Completed += OnComplete_Send;
                    saea.SetBuffer(buffer, offset, size);
                    if (onSent != null)
                        saea.UserToken = new NetworkSendToken(new StreamBuffer(buffer, offset, size), onSent);

                    if (_session.Socket.SendAsync(saea) == false)
                        OnComplete_Receive(null, saea);
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, LogLevel.Core, e.ToString());
            }
        }


        public void SendPacket(StreamBuffer buffer, Action<StreamBuffer> onSent = null)
        {
            try
            {
                lock (_session)
                {
                    if (_session.Socket == null)
                        return;


                    //  ReadIndex가 OnSocket_Send에서 사용되므로 ReadIndex를 초기화해야 한다.
                    buffer.ResetReadIndex();


                    SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                    saea.Completed += OnComplete_Send;
                    saea.SetBuffer(buffer.Buffer, 0, buffer.WrittenBytes);
                    if (onSent != null)
                        saea.UserToken = new NetworkSendToken(buffer, onSent);

                    if (_session.Socket.SendAsync(saea) == false)
                        OnComplete_Receive(null, saea);
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, LogLevel.Core, e.ToString());
            }
        }


        public void SendPacket(StreamBuffer buffer, PacketPredicate predicate, IOEventHandler dispatcher, Action<StreamBuffer> onSent = null)
        {
            if (predicate == null || dispatcher == null)
                throw new AegisException(AegisResult.InvalidArgument, "The argument predicate and dispatcher cannot be null.");


            try
            {
                lock (_session)
                {
                    if (_session.Socket == null)
                        return;

                    //  ReadIndex가 OnSocket_Send에서 사용되므로 ReadIndex를 초기화해야 한다.
                    buffer.ResetReadIndex();


                    SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                    saea.Completed += OnComplete_Send;
                    saea.SetBuffer(buffer.Buffer, 0, buffer.WrittenBytes);
                    if (onSent != null)
                        saea.UserToken = new NetworkSendToken(buffer, onSent);

                    _responseSelector.Add(predicate, dispatcher);

                    if (_session.Socket.SendAsync(saea) == false)
                        OnComplete_Receive(null, saea);
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, LogLevel.Core, e.ToString());
            }
        }


        private void OnComplete_Send(object sender, SocketAsyncEventArgs saea)
        {
            try
            {
                NetworkSendToken token = (NetworkSendToken)saea.UserToken;
                if (token != null)
                {
                    token.Buffer.Read(saea.BytesTransferred);
                    if (token.Buffer.ReadableSize == 0)
                        token.CompletionAction();
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, LogLevel.Core, e.ToString());
            }
        }
    }
}
