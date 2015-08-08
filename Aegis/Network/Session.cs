using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Aegis;
using Aegis.Threading;



namespace Aegis.Network
{
    /// <summary>
    /// 원격지의 호스트와 네트워킹을 할 수 있는 기능을 제공합니다.
    /// </summary>
    public class Session : SessionBase
    {
        private StreamBuffer _receivedBuffer, _dispatchBuffer;





        /// <summary>
        /// 수신버퍼의 크기는 StreamBuffer의 기본할당크기로 초기화됩니다.
        /// </summary>
        protected Session()
        {
            _receivedBuffer = new StreamBuffer();
            _dispatchBuffer = new StreamBuffer();
        }


        /// <summary>
        /// 수신버퍼의 크기를 지정하여 Session 객체를 생성합니다. 수신버퍼의 크기는 패킷 하나의 크기 이상으로 설정하는 것이 좋습니다.
        /// </summary>
        /// <param name="recvBufferSize">수신버퍼의 크기(Byte)</param>
        protected Session(Int32 recvBufferSize)
        {
            _receivedBuffer = new StreamBuffer(recvBufferSize);
            _dispatchBuffer = new StreamBuffer();
        }


        /// <summary>
        /// 수신버퍼의 크기를 변경합니다.
        /// 새로운 버퍼의 크기는 기존 버퍼의 크기보다 커야합니다.
        /// 버퍼 크기가 변경되더라도 기존의 데이터는 유지됩니다.
        /// </summary>
        /// <param name="recvBufferSize">변경할 수신버퍼의 크기(Byte)</param>
        public void SetReceiveBufferSize(Int32 recvBufferSize)
        {
            if (recvBufferSize <= _receivedBuffer.BufferSize)
                return;

            StreamBuffer oldBuffer = new StreamBuffer(_receivedBuffer, 0, _receivedBuffer.WrittenBytes);

            _receivedBuffer = new StreamBuffer(recvBufferSize);
            _receivedBuffer.Write(oldBuffer.Buffer, 0, oldBuffer.WrittenBytes);
        }


        public override void Close()
        {
            base.Close();
            _receivedBuffer.Clear();
            _dispatchBuffer.Clear();
        }


        private void WaitForReceive()
        {
            AegisTask.Run(() =>
            {
                try
                {
                    lock (this)
                    {
                        if (_receivedBuffer.WritableSize == 0)
                            throw new AegisException(ResultCode.NotEnoughBuffer, "There is no remaining capacity of the receive buffer.");

                        if (Socket != null && Socket.Connected)
                            Socket.BeginReceive(_receivedBuffer.Buffer, _receivedBuffer.WrittenBytes, _receivedBuffer.WritableSize, 0, OnSocket_Read, null);
                    }
                }
                catch (Exception)
                {
                }
            });
        }


        private void OnSocket_Read(IAsyncResult ar)
        {
            try
            {
                lock (this)
                {
                    if (Socket == null)
                        return;

                    //  transBytes가 0이면 원격지 혹은 네트워크에 의해 연결이 끊긴 상태
                    Int32 transBytes = Socket.EndReceive(ar);
                    if (transBytes == 0)
                    {
                        Close();
                        return;
                    }


                    _receivedBuffer.Write(transBytes);
                    _dispatchBuffer.Clear();
                    _dispatchBuffer.Write(_receivedBuffer.Buffer, 0, _receivedBuffer.WrittenBytes);
                    while (_dispatchBuffer.ReadableSize > 0)
                    {
                        //  패킷 하나가 정상적으로 수신되었는지 확인
                        Int32 packetSize;
                        if (IsValidPacket(_dispatchBuffer, out packetSize) == false)
                            break;

                        try
                        {
                            //  수신 이벤트 처리 중 종료 이벤트가 발생한 경우
                            if (Socket == null)
                                return;


                            //  수신처리(Dispatch)
                            _dispatchBuffer.ResetReadIndex();
                            OnReceive(_dispatchBuffer);

                            _dispatchBuffer.Read(packetSize);
                            _receivedBuffer.Read(packetSize);
                        }
                        catch (Exception e)
                        {
                            Logger.Write(LogType.Err, 1, e.ToString());
                        }
                    }


                    //  처리된 패킷을 버퍼에서 제거
                    _receivedBuffer.PopReadBuffer();

                    //  ReceiveBuffer의 안정적인 처리를 위해 OnReceive 작업이 끝난 후에 다시 수신대기
                    WaitForReceive();
                }
            }
            catch (SocketException)
            {
                Close();
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        /// <summary>
        /// 패킷을 전송합니다. 패킷이 전송되면 OnSend함수가 호출됩니다.
        /// </summary>
        /// <param name="source">보낼 데이터가 담긴 버퍼</param>
        /// <param name="offset">source에서 전송할 시작 위치</param>
        /// <param name="size">source에서 전송할 크기(Byte)</param>
        public virtual void SendPacket(byte[] source, Int32 offset, Int32 size)
        {
            try
            {
                lock (this)
                {
                    if (Socket != null)
                        Socket.BeginSend(source, offset, size, SocketFlags.None, OnSocket_Send, null);
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        /// <summary>
        /// 패킷을 전송합니다. 패킷이 전송되면 OnSend함수가 호출됩니다.
        /// </summary>
        /// <param name="source">전송할 데이터가 담긴 StreamBuffer</param>
        public virtual void SendPacket(StreamBuffer source)
        {
            try
            {
                lock (this)
                {
                    if (Socket != null)
                        Socket.BeginSend(source.Buffer, 0, source.WrittenBytes, SocketFlags.None, OnSocket_Send, null);
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        private void OnSocket_Send(IAsyncResult ar)
        {
            try
            {
                lock (this)
                {
                    if (Socket == null)
                        return;

                    Int32 transBytes = Socket.EndSend(ar);
                    OnSend(transBytes);
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        /// <summary>
        /// 클라이언트의 연결요청에 의해 Session이 활성화된 경우 이 함수가 호출됩니다.
        /// </summary>
        protected override void OnAccept()
        {
            WaitForReceive();
        }


        /// <summary>
        /// 이 Session 객체가 Connect를 사용하여 서버에 연결요청하면 결과가 이 함수로 전달됩니다.
        /// </summary>
        /// <param name="connected">true인 경우 연결에 성공한 상태입니다.</param>
        protected override void OnConnect(bool connected)
        {
            if (connected)
                WaitForReceive();
        }


        /// <summary>
        /// 수신된 데이터가 유효한 패킷인지 여부를 확인합니다.
        /// 유효한 패킷으로 판단되면 packetSize에 이 패킷의 정확한 크기를 입력하고 true를 반환해야 합니다.
        /// </summary>
        /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
        /// <param name="packetSize">유효한 패킷의 크기</param>
        /// <returns>true를 반환하면 OnReceive함수를 통해 수신된 데이터가 전달됩니다.</returns>
        protected virtual Boolean IsValidPacket(StreamBuffer buffer, out Int32 packetSize)
        {
            if (buffer.WrittenBytes < 4)
            {
                packetSize = 0;
                return false;
            }

            //  최초 2바이트를 수신할 패킷의 크기로 처리
            packetSize = buffer.GetUInt16();
            return (packetSize > 0 && buffer.WrittenBytes >= packetSize);
        }
    }
}
