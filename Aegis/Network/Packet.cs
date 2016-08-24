using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis.IO;



namespace Aegis.Network
{
    /// <summary>
    /// Size(2 bytes), PacketId(2 bytes)로 구성된 4바이트 헤더를 갖는 기본 패킷 구성입니다.
    /// </summary>
    public class Packet : StreamBuffer
    {
        /// <summary>
        /// 현재 패킷의 크기를 가져옵니다. 패킷의 크기값은 임의로 변경할 수 없습니다.
        /// </summary>
        public ushort Size
        {
            get { return GetUInt16(0); }
            private set { OverwriteUInt16(0, value); }
        }
        /// <summary>
        /// 패킷의 고유번호를 지정하거나 가져옵니다.
        /// </summary>
        public ushort PacketId
        {
            get { return GetUInt16(2); }
            set { OverwriteUInt16(2, value); }
        }
        /// <summary>
        /// 패킷의 해더 크기(Byte)
        /// </summary>
        public const int HeaderSize = 4;





        public Packet()
        {
            PutUInt16(0);       //  Size
            PutUInt16(0);       //  PacketId
        }


        /// <summary>
        /// 고유번호를 지정하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="packetId">패킷의 고유번호</param>
        public Packet(ushort packetId)
        {
            PutUInt16(0);           //  Size
            PutUInt16(packetId);    //  PacketId
        }


        /// <summary>
        /// 고유번호와 패킷의 기본 크기를 지정하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="packetId">패킷의 고유번호</param>
        /// <param name="capacity">패킷 버퍼의 크기</param>
        public Packet(ushort packetId, ushort capacity)
        {
            Capacity(capacity);
            PutUInt16(0);           //  Size
            PutUInt16(packetId);    //  PacketId
        }


        /// <summary>
        /// StreamBuffer의 데이터를 복사하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="source">복사할 데이터가 담긴 StreamBuffer 객체</param>
        public Packet(StreamBuffer source)
        {
            Write(source.Buffer, 0, source.WrittenBytes);
        }


        /// <summary>
        /// byte 배열의 데이터를 복사하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="source">복사할 데이터가 담긴 byte 배열</param>
        public Packet(byte[] source)
        {
            Write(source, 0, source.Length);
        }


        /// <summary>
        /// byte 배열의 데이터를 복사하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="source">복사할 데이터가 담긴 byte 배열</param>
        /// <param name="startIndex">source에서 복사할 시작 위치</param>
        /// <param name="size">복사할 크기(Byte)</param>
        public Packet(byte[] source, int startIndex, int size)
        {
            Write(source, startIndex, size);
        }


        /// <summary>
        /// 이 SecurePacket 객체와 동일한 내용의 새로운 객체를 생성합니다.
        /// </summary>
        /// <returns>현재 SecurePacket의 데이터가 복제된 객체</returns>
        public override StreamBuffer Clone()
        {
            Packet packet = new Packet(this);
            packet.Read(ReadBytes);

            return packet;
        }


        /// <summary>
        /// 지정된 버퍼에서 PacketId 값을 가져옵니다.
        /// buffer는 패킷 헤더가 온전히 포함된 데이터로 지정되어야 합니다.
        /// </summary>
        /// <param name="buffer">패킷 데이터가 담긴 버퍼</param>
        /// <returns>패킷의 PacketId를 반환합니다.</returns>
        public static ushort GetPacketId(byte[] buffer)
        {
            if (buffer.Length < 4)
                return 0;

            return BitConverter.ToUInt16(buffer, 2);
        }


        /// <summary>
        /// 지정된 버퍼에서 PacketId 값을 가져옵니다.
        /// buffer는 패킷 헤더가 온전히 포함된 데이터로 지정되어야 합니다.
        /// </summary>
        /// <param name="buffer">패킷 데이터가 담긴 버퍼</param>
        /// <returns>패킷의 PacketId를 반환합니다.</returns>
        public static ushort GetPacketId(StreamBuffer buffer)
        {
            if (buffer.Buffer.Length < 4)
                return 0;

            return BitConverter.ToUInt16(buffer.Buffer, 2);
        }


        /// <summary>
        /// 수신된 데이터가 유효한 패킷인지 여부를 확인합니다.
        /// 유효한 패킷으로 판단되면 packetSize에 이 패킷의 정확한 크기를 입력하고 true를 반환해야 합니다.
        /// </summary>
        /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
        /// <param name="packetSize">유효한 패킷의 크기</param>
        /// <returns>true를 반환하면 OnReceive를 통해 수신된 데이터가 전달됩니다.</returns>
        public static bool IsValidPacket(StreamBuffer buffer, out int packetSize)
        {
            if (buffer.WrittenBytes < HeaderSize)
            {
                packetSize = 0;
                return false;
            }

            //  최초 2바이트를 수신할 패킷의 크기로 처리
            packetSize = buffer.GetUInt16(0);
            return (packetSize > 0 && buffer.WrittenBytes >= packetSize);
        }


        /// <summary>
        /// 수신된 데이터가 유효한 패킷인지 여부를 확인합니다.
        /// 유효한 패킷으로 판단되면 packetSize에 이 패킷의 정확한 크기를 입력하고 true를 반환해야 합니다.
        /// </summary>
        /// <param name="buffer">수신된 데이터가 담긴 버퍼</param>
        /// <param name="startIndex">버퍼에서 데이터의 시작지점</param>
        /// <param name="length">데이터의 길이</param>
        /// <param name="packetSize">유효한 패킷의 크기</param>
        /// <returns>true를 반환하면 OnReceive를 통해 수신된 데이터가 전달됩니다.</returns>
        public static bool IsValidPacket(byte[] buffer, int startIndex, int length, out int packetSize)
        {
            if (startIndex + length > buffer.Length ||
                length < HeaderSize)
            {
                packetSize = 0;
                return false;
            }

            //  최초 2바이트를 수신할 패킷의 크기로 처리
            packetSize = BitConverter.ToUInt16(buffer, startIndex);
            return (packetSize > 0 && length >= packetSize);
        }


        /// <summary>
        /// 패킷 버퍼를 초기화합니다. 기존의 PacketId 값은 유지됩니다.
        /// </summary>
        public override void Clear()
        {
            ushort packetId = PacketId;


            base.Clear();
            PutUInt16(0);           //  Size
            PutUInt16(packetId);    //  PacketId
        }


        /// <summary>
        /// 패킷 버퍼를 초기화하고 source 데이터를 저장합니다. Packet Header의 Size는 source 버퍼의 헤더값이 사용됩니다.
        /// </summary>
        /// <param name="source">저장할 데이터</param>
        public virtual void Clear(StreamBuffer source)
        {
            if (source.BufferSize < HeaderSize)
                throw new AegisException(AegisResult.InvalidArgument, "The source size must be at lest {0} bytes.", HeaderSize);

            base.Clear();
            Write(source.Buffer, 0, source.WrittenBytes);
            Size = GetUInt16(0);
        }


        /// <summary>
        /// 패킷 버퍼를 초기화하고 source 데이터를 저장합니다. Packet Header의 Size는 source 버퍼의 헤더값이 사용됩니다.
        /// </summary>
        /// <param name="source">저장할 데이터</param>
        /// <param name="index">저장할 데이터의 시작위치</param>
        /// <param name="size">저장할 데이터 크기(Byte)</param>
        public virtual void Clear(byte[] source, int index, int size)
        {
            if (size < 4)
                throw new AegisException(AegisResult.InvalidArgument, "The source size must be at lest 4 bytes.");

            Clear();
            Write(source, index, size);
            Size = GetUInt16(0);
        }


        /// <summary>
        /// 패킷의 크기가 변경되었을 때 호출됩니다.
        /// 이 함수가 호출되어야 패킷의 Size값이 변경됩니다.
        /// </summary>
        protected override void OnWritten()
        {
            Size = (ushort)WrittenBytes;
        }


        /// <summary>
        /// 패킷의 헤더 위치를 건너띄어 본문 데이터를 읽을 수 있도록 읽기위치를 조절합니다.
        /// 이 함수가 호출되면 ReadIndex는 4에 위치하지만, WriteIndex는 변하지 않습니다.
        /// </summary>
        public virtual void SkipHeader()
        {
            ResetReadIndex();

            GetUInt16();        //  Size
            GetUInt16();        //  PacketId
        }
    }
}
