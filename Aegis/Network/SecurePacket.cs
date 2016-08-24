using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Aegis.IO;



namespace Aegis.Network
{
    public class SecurePacket : StreamBuffer
    {
        private static byte[] _tempBuffer = new byte[32];
        private static uint[] _crcTable =
        {
            0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
            0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
            0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
            0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
            0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
            0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
            0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
            0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
            0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
            0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
            0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
            0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
            0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
            0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
            0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
            0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
            0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
            0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
            0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
            0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
            0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
            0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
            0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
            0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
            0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
            0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
            0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
            0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
            0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
            0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
            0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
            0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
            0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
            0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
            0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
            0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
            0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
            0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
            0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
            0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
            0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
            0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
            0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
            0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
            0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
            0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
            0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
            0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
            0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
            0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
            0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
            0x2d02ef8d
        };
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
        /// 패킷의 Sequence number를 가져옵니다.
        /// </summary>
        public int SeqNo
        {
            get { return base.GetInt32(4); }
            set { base.OverwriteInt32(4, value); }
        }
        /// <summary>
        /// 패킷의 해더 크기(Byte)
        /// </summary>
        public const int HeaderSize = 8;





        public SecurePacket()
        {
            PutUInt16(0);       //  Size
            PutUInt16(0);       //  PacketId
            PutInt32(0);        //  SeqNo
        }


        /// <summary>
        /// 고유번호를 지정하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="packetId">패킷의 고유번호</param>
        public SecurePacket(ushort packetId)
            : base(packetId)
        {
            PutUInt16(0);           //  Size
            PutUInt16(packetId);    //  PacketId
            PutInt32(0);            //  SeqNo
        }


        /// <summary>
        /// 고유번호와 패킷의 기본 크기를 지정하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="packetId">패킷의 고유번호</param>
        /// <param name="capacity">패킷 버퍼의 크기</param>
        public SecurePacket(ushort packetId, ushort capacity)
        {
            Capacity(capacity);
            PutUInt16(0);           //  Size
            PutUInt16(packetId);    //  PacketId
            PutInt32(0);            //  SeqNo
        }


        /// <summary>
        /// StreamBuffer의 데이터를 복사하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="source">복사할 데이터가 담긴 StreamBuffer 객체</param>
        public SecurePacket(StreamBuffer source)
        {
            Write(source.Buffer, 0, source.WrittenBytes);
        }


        /// <summary>
        /// byte 배열의 데이터를 복사하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="source">복사할 데이터가 담긴 byte 배열</param>
        public SecurePacket(byte[] source)
        {
            Write(source, 0, source.Length);
        }


        /// <summary>
        /// byte 배열의 데이터를 복사하여 패킷을 생성합니다.
        /// </summary>
        /// <param name="source">복사할 데이터가 담긴 byte 배열</param>
        /// <param name="startIndex">source에서 복사할 시작 위치</param>
        /// <param name="size">복사할 크기(Byte)</param>
        public SecurePacket(byte[] source, int startIndex, int size)
        {
            Write(source, startIndex, size);
        }


        /// <summary>
        /// 이 SecurePacket 객체와 동일한 내용의 새로운 객체를 생성합니다.
        /// </summary>
        /// <returns>현재 SecurePacket의 데이터가 복제된 객체</returns>
        public override StreamBuffer Clone()
        {
            SecurePacket packet = new SecurePacket(this);
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
        /// <returns>true를 반환하면 EventReceive 통해 수신된 데이터가 전달됩니다.</returns>
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
        /// 패킷 버퍼를 초기화합니다. 기존의 PacketId 값은 유지됩니다.
        /// </summary>
        public override void Clear()
        {
            ushort packetId = PacketId;


            base.Clear();
            PutUInt16(0);           //  Size
            PutUInt16(packetId);    //  PacketId
            PutInt32(0);            //  SeqNo
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
            GetInt32();         //  SeqNo
        }


        protected static uint GetCRC32(byte[] data, int startOffset, int length)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < length; i++)
                crc = _crcTable[(crc ^ data[startOffset + i]) & 0xff] ^ (crc >> 8);

            return crc ^ 0xffffffff;
        }


        public virtual void Encrypt(string iv, string key)
        {
            //  Padding
            {
                int paddingByte, blockSizeInByte = 16;


                //  (Size - 2) = Encrypt data size
                //  -4 = CRC data
                paddingByte = blockSizeInByte - (Size - 2) % blockSizeInByte - 4;
                if (paddingByte < 0)
                    paddingByte = blockSizeInByte + paddingByte;

                if (paddingByte > 0)
                    Write(_tempBuffer, 0, paddingByte);
            }


            //  CRC
            {
                uint crc = GetCRC32(Buffer, 2, Size - 2);
                PutUInt32(crc);
            }


            //  Encrypt
            using (Aes aes = Aes.Create())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Key = Encoding.ASCII.GetBytes(key);
                aes.IV = Encoding.ASCII.GetBytes(iv);


                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    ushort packetSize = Size;

                    cryptoStream.Write(Buffer, 2, packetSize - 2);
                    byte[] encrypted = memoryStream.ToArray();
                    Overwrite(encrypted, 0, encrypted.Length, 2);

                    cryptoStream.Close();
                }

                memoryStream.Close();
            }
        }



        public virtual bool Decrypt(string iv, string key)
        {
            ushort packetSize = Size;


            //  Block Size가 일치하지 않으면 복호화를 할 수 없다.
            if ((packetSize - 2) % 16 != 0)
            {
                Logger.Err(LogMask.Aegis, "BlockSize is not match(packetsize={0}).", packetSize);
                return false;
            }


            //  Decrypt AES
            {
                using (Aes aes = Aes.Create())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.None;
                    aes.KeySize = 128;
                    aes.BlockSize = 128;
                    aes.Key = Encoding.ASCII.GetBytes(key);
                    aes.IV = Encoding.ASCII.GetBytes(iv);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(Buffer, 2, packetSize - 2);
                        byte[] decrypted = memoryStream.ToArray();
                        Overwrite(decrypted, 0, decrypted.Length, 2);

                        cryptoStream.Close();
                    }

                    memoryStream.Close();
                }
            }


            //  Check CRC
            {
                uint crc = GetCRC32(Buffer, 2, packetSize - 4 - 2);
                uint packetCRC = BitConverter.ToUInt32(Buffer, packetSize - 4);


                if (crc != packetCRC)
                {
                    Logger.Err(LogMask.Aegis, "Invalid CRC.");
                    return false;
                }
            }

            return true;
        }
    }
}
