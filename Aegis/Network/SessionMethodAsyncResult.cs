using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Aegis.IO;
using Aegis.Threading;



namespace Aegis.Network
{
    internal class SessionMethodAsyncResult : ISessionMethod
    {
        private Session _session;
        private StreamBuffer _receivedBuffer;
        private ResponseSelector _responseSelector;





        public SessionMethodAsyncResult(Session session)
        {
            _session = session;
            _receivedBuffer = new StreamBuffer(2048);
            _responseSelector = new ResponseSelector(_session);
        }


        public void Clear()
        {
            _receivedBuffer.Clear();
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
                        _session.Socket.BeginReceive(_receivedBuffer.Buffer, _receivedBuffer.WrittenBytes, _receivedBuffer.WritableSize, 0, Socket_Read, null);
                    else
                        _session.Close(AegisResult.UnknownError);
                }
            }
            catch (Exception)
            {
                _session.Close(AegisResult.UnknownError);
            }
        }


        private void Socket_Read(IAsyncResult ar)
        {
            try
            {
                lock (_session)
                {
                    if (_session.Socket == null)
                        return;

                    //  transBytes가 0이면 원격지 혹은 네트워크에 의해 연결이 끊긴 상태
                    int transBytes = _session.Socket.EndReceive(ar);
                    if (transBytes == 0)
                    {
                        _session.Close(AegisResult.ClosedByRemote);
                        return;
                    }


                    _receivedBuffer.Write(transBytes);
                    while (_receivedBuffer.ReadableSize > 0)
                    {
                        //  패킷 하나가 정상적으로 수신되었는지 확인
                        int packetSize;
                        StreamBuffer tmpBuffer = new StreamBuffer(_receivedBuffer, _receivedBuffer.ReadBytes, _receivedBuffer.ReadableSize);
                        if (_session.PacketValidator == null ||
                            _session.PacketValidator(tmpBuffer, out packetSize) == false)
                            break;

                        try
                        {
                            //  수신 이벤트 처리 중 종료 이벤트가 발생한 경우
                            if (_session.Socket == null)
                                return;


                            //  수신버퍼에서 제거
                            _receivedBuffer.Read(packetSize);


                            //  수신처리(Dispatch)
                            StreamBuffer dispatchBuffer = new StreamBuffer(tmpBuffer, 0, packetSize);
                            SpinWorker.Dispatch(() =>
                            {
                                if (_responseSelector.Dispatch(dispatchBuffer) == false)
                                    _session.OnReceived(dispatchBuffer);
                            });
                        }
                        catch (Exception e)
                        {
                            Logger.Err(LogMask.Aegis, e.ToString());
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
                _session.Close(AegisResult.ClosedByRemote);
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
            }
        }


        public void SendPacket(byte[] buffer, int offset, int size, Action<StreamBuffer> onSent = null)
        {
            try
            {
                lock (_session)
                {
                    if (_session.Socket != null)
                    {
                        if (onSent == null)
                            _session.Socket.BeginSend(buffer, offset, size, SocketFlags.None, Socket_Send, null);
                        else
                            _session.Socket.BeginSend(buffer, offset, size, SocketFlags.None, Socket_Send,
                                             new NetworkSendToken(new StreamBuffer(buffer, offset, size), onSent));
                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
            }
        }


        public void SendPacket(StreamBuffer buffer, Action<StreamBuffer> onSent = null)
        {
            try
            {
                lock (_session)
                {
                    if (_session.Socket != null)
                    {
                        //  ReadIndex가 OnSocket_Send에서 사용되므로 ReadIndex를 초기화해야 한다.
                        buffer.ResetReadIndex();

                        if (onSent == null)
                            _session.Socket.BeginSend(buffer.Buffer, 0, buffer.WrittenBytes, SocketFlags.None, Socket_Send, null);
                        else
                            _session.Socket.BeginSend(buffer.Buffer, 0, buffer.WrittenBytes, SocketFlags.None, Socket_Send,
                                             new NetworkSendToken(buffer, onSent));
                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
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
                    _responseSelector.Add(predicate, dispatcher);
                    if (_session.Socket != null)
                    {
                        //  ReadIndex가 OnSocket_Send에서 사용되므로 ReadIndex를 초기화해야 한다.
                        buffer.ResetReadIndex();

                        if (onSent == null)
                            _session.Socket.BeginSend(buffer.Buffer, 0, buffer.WrittenBytes, SocketFlags.None, Socket_Send, null);
                        else
                            _session.Socket.BeginSend(buffer.Buffer, 0, buffer.WrittenBytes, SocketFlags.None, Socket_Send,
                                             new NetworkSendToken(buffer, onSent));
                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
            }
        }


        private void Socket_Send(IAsyncResult ar)
        {
            try
            {
                lock (_session)
                {
                    if (_session.Socket == null)
                        return;


                    int transBytes = _session.Socket.EndSend(ar);
                    NetworkSendToken token = (NetworkSendToken)ar.AsyncState;

                    if (token != null)
                    {
                        token.Buffer.Read(transBytes);
                        if (token.Buffer.ReadableSize == 0)
                            token.CompletionAction();
                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (Exception e)
            {
                Logger.Err(LogMask.Aegis, e.ToString());
            }
        }
    }
}
