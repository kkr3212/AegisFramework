using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis
{
    /// <summary>
    /// 데이터를 순차적으로 읽거나 쓸 수 있는 버퍼입니다.
    /// 데이터 쓰기의 경우, 버퍼가 부족하면 자동으로 증가시킵니다.
    /// 데이터 읽기의 경우, 쓰기된 크기 이상으로 읽어들일 수 없습니다.
    /// </summary>
    public class StreamBuffer : IDisposable
    {
        private const Int32 AllocBlockSize = 128;

        public Int32 ReadIndex { get; private set; }
        public Int32 WriteIndex { get; private set; }

        public byte[] Buffer { get; private set; }
        public Int32 BufferSize { get { return Buffer.Length; } }





        public StreamBuffer()
        {
            ReadIndex = 0;
            WriteIndex = 0;

            Capacity(256);
        }


        public StreamBuffer(Int32 size)
        {
            ReadIndex = 0;
            WriteIndex = 0;

            Capacity(size);
        }


        public StreamBuffer(StreamBuffer source)
        {
            ReadIndex = 0;
            WriteIndex = 0;

            Capacity(source.BufferSize);
            Write(source.Buffer);
        }


        public StreamBuffer(StreamBuffer source, Int32 index, Int32 size)
        {
            ReadIndex = 0;
            WriteIndex = 0;

            Capacity(size);
            Write(source.Buffer, index, size);
        }


        public void Dispose()
        {
        }


        private Int32 AllocateBlockSize(Int32 size)
        {
            return (size / AllocBlockSize + (size % AllocBlockSize > 0 ? 1 : 0)) * AllocBlockSize;
        }


        public void Capacity(Int32 size)
        {
            Int32 allocSize = AllocateBlockSize(size);
            Buffer = new byte[allocSize];
        }


        public void Resize(Int32 size)
        {
            if (size <= BufferSize)
                return;

            Int32 allocSize = AllocateBlockSize(size);
            byte[] newBuffer = new byte[allocSize];

            Array.Copy(Buffer, newBuffer, Buffer.Length);
            Buffer = newBuffer;
        }


        public virtual void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
            ReadIndex = 0;
            WriteIndex = 0;
        }


        public void ResetReadIndex()
        {
            ReadIndex = 0;
        }


        public void ResetWriteIndex()
        {
            WriteIndex = 0;
        }


        public void Write(byte[] src)
        {
            Int32 srcSize = src.Length;
            if (WriteIndex + srcSize > BufferSize)
                Resize(BufferSize + srcSize);

            Array.Copy(src, 0, Buffer, WriteIndex, srcSize);
            WriteIndex += srcSize;
        }


        public void Write(byte[] source, Int32 index)
        {
            if (index >= source.Length)
                throw new AegisException(ResultCode.InvalidArgument, "The argument index(={0}) is larger then src size(={1}).", index, source.Length);

            Int32 copyBytes = source.Length - index;
            if (WriteIndex + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, index, Buffer, WriteIndex, copyBytes);
            WriteIndex += copyBytes;
        }


        public void Write(byte[] source, Int32 index, Int32 size)
        {
            if (index + size > source.Length)
                throw new AegisException(ResultCode.InvalidArgument, "The argument index(={0}) is larger then src size(={1}).", index, source.Length);

            Int32 copyBytes = size;
            if (WriteIndex + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, index, Buffer, WriteIndex, copyBytes);
            WriteIndex += copyBytes;
        }


        public void Overwrite(byte[] source, Int32 index, Int32 size, Int32 writeIndex)
        {
            if (index + size > source.Length)
                throw new AegisException(ResultCode.InvalidArgument, "The argument index(={0}) is larger then src size(={1}).", index, source.Length);

            Int32 copyBytes = size;
            if (writeIndex + copyBytes >= BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, index, Buffer, writeIndex, copyBytes);

            if (writeIndex + copyBytes > WriteIndex)
                WriteIndex = writeIndex + copyBytes;
        }


        public void Read(byte[] destination)
        {
            if (destination.Length < BufferSize)
                throw new AegisException(ResultCode.NotEnoughBuffer, "Destination buffer size too small.");

            Array.Copy(Buffer, destination, BufferSize);
            ReadIndex += BufferSize;
        }


        public void Read(byte[] destination, Int32 index)
        {
            if (destination.Length - index < BufferSize)
                throw new AegisException(ResultCode.NotEnoughBuffer, "Destination buffer size too small.");

            Array.Copy(Buffer, 0, destination, index, BufferSize);
            ReadIndex += BufferSize;
        }


        public void Read(byte[] destination, Int32 index, Int32 readIndex, Int32 size)
        {
            if (destination.Length - index < size)
                throw new AegisException(ResultCode.NotEnoughBuffer, "Destination buffer size too small.");

            Array.Copy(Buffer, readIndex, destination, index, size);
        }


        public Boolean GetBoolean()
        {
            if (ReadIndex + sizeof(byte) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = Buffer[ReadIndex];
            ReadIndex += sizeof(byte);
            return (val == 1);
        }


        public byte GetByte()
        {
            if (ReadIndex + sizeof(byte) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = Buffer[ReadIndex];
            ReadIndex += sizeof(byte);
            return val;
        }


        public Int16 GetInt16()
        {
            if (ReadIndex + sizeof(Int16) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToInt16(Buffer, ReadIndex);
            ReadIndex += sizeof(Int16);
            return val;
        }


        public UInt16 GetUInt16()
        {
            if (ReadIndex + sizeof(UInt16) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToUInt16(Buffer, ReadIndex);
            ReadIndex += sizeof(UInt16);
            return val;
        }


        public Int32 GetInt32()
        {
            if (ReadIndex + sizeof(Int32) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToInt32(Buffer, ReadIndex);
            ReadIndex += sizeof(Int32);
            return val;
        }


        public UInt32 GetUInt32()
        {
            if (ReadIndex + sizeof(UInt32) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToUInt32(Buffer, ReadIndex);
            ReadIndex += sizeof(UInt32);
            return val;
        }


        public Int64 GetInt64()
        {
            if (ReadIndex + sizeof(Int64) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToInt64(Buffer, ReadIndex);
            ReadIndex += sizeof(Int64);
            return val;
        }


        public UInt64 GetUInt64()
        {
            if (ReadIndex + sizeof(UInt64) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToUInt64(Buffer, ReadIndex);
            ReadIndex += sizeof(UInt64);
            return val;
        }


        public Double GetDouble()
        {
            if (ReadIndex + sizeof(UInt64) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToDouble(Buffer, ReadIndex);
            ReadIndex += sizeof(Double);
            return val;
        }


        public String GetStringFromUtf8()
        {
            Int32 i, stringBytes = 0;
            for (i = ReadIndex; i < BufferSize; ++i)
            {
                if (Buffer[i] == 0)
                    break;

                ++stringBytes;
                if (i > WriteIndex)
                    throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            String val = Encoding.UTF8.GetString(Buffer, ReadIndex, stringBytes);
            ReadIndex += stringBytes + 1;
            return val;
        }


        public String GetStringFromUtf16()
        {
            Int32 i, stringBytes = 0;
            for (i = ReadIndex; i < BufferSize; i += 2)
            {
                if (Buffer[i + 0] == 0
                    && Buffer[i + 1] == 0)
                    break;

                stringBytes += 2;

                if (ReadIndex + stringBytes + 2 > WriteIndex)
                    throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            String val = Encoding.Unicode.GetString(Buffer, ReadIndex, stringBytes);
            ReadIndex += stringBytes + 2;
            return val;
        }


        public Boolean GetBoolean(Int32 readIndex)
        {
            if (readIndex + sizeof(byte) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return (Buffer[readIndex] == 1);
        }


        public byte GetByte(Int32 readIndex)
        {
            if (readIndex + sizeof(byte) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return Buffer[readIndex];
        }


        public Int16 GetInt16(Int32 readIndex)
        {
            if (readIndex + sizeof(Int16) >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToInt16(Buffer, readIndex);
        }


        public UInt16 GetUInt16(Int32 readIndex)
        {
            if (readIndex >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToUInt16(Buffer, readIndex);
        }


        public Int32 GetInt32(Int32 readIndex)
        {
            if (readIndex >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToInt32(Buffer, readIndex);
        }


        public UInt32 GetUInt32(Int32 readIndex)
        {
            if (readIndex >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToUInt32(Buffer, readIndex);
        }


        public Int64 GetInt64(Int32 readIndex)
        {
            if (readIndex >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToInt64(Buffer, readIndex);
        }


        public UInt64 GetUInt64(Int32 readIndex)
        {
            if (readIndex >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToUInt64(Buffer, readIndex);
        }


        public Double GetDouble(Int32 readIndex)
        {
            if (readIndex >= WriteIndex)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToDouble(Buffer, readIndex);
        }


        public String GetStringFromUtf8(Int32 readIndex)
        {
            Int32 i, stringBytes = 0;
            for (i = readIndex; i < BufferSize; ++i)
            {
                if (Buffer[i] == 0)
                    break;

                ++stringBytes;
                if (i > WriteIndex)
                    throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            return Encoding.UTF8.GetString(Buffer, readIndex, stringBytes);
        }


        public String GetStringFromUtf16(Int32 readIndex)
        {
            Int32 i, stringBytes = 0;
            for (i = readIndex; i < BufferSize; i += 2)
            {
                if (Buffer[i + 0] == 0
                    && Buffer[i + 1] == 0)
                    break;

                stringBytes += 2;

                if (readIndex + stringBytes + 2 > WriteIndex)
                    throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            return Encoding.Unicode.GetString(Buffer, readIndex, stringBytes);
        }


        public Int32 PutBoolean(Boolean var)
        {
            Int32 prevIndex = WriteIndex;

            Write(BitConverter.GetBytes(var), 0, 1);
            return prevIndex;
        }


        public Int32 PutByte(Byte var)
        {
            Int32 prevIndex = WriteIndex;

            Write(BitConverter.GetBytes(var), 0, 1);
            return prevIndex;
        }


        public Int32 PutInt16(Int16 var)
        {
            Int32 prevIndex = WriteIndex;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutUInt16(UInt16 var)
        {
            Int32 prevIndex = WriteIndex;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutInt32(Int32 var)
        {
            Int32 prevIndex = WriteIndex;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutUInt32(UInt32 var)
        {
            Int32 prevIndex = WriteIndex;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutInt64(Int64 var)
        {
            Int32 prevIndex = WriteIndex;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutUInt64(UInt64 var)
        {
            Int32 prevIndex = WriteIndex;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutDouble(Double var)
        {
            Int32 prevIndex = WriteIndex;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutStringAsUtf8(String var)
        {
            Int32 prevIndex = WriteIndex;
            byte[] data = Encoding.UTF8.GetBytes(var);

            Write(data);
            PutByte(0);     //  Null terminate
            return prevIndex;
        }


        public Int32 PutStringAsUtf16(String var)
        {
            Int32 prevIndex = WriteIndex;
            byte[] data = Encoding.Unicode.GetBytes(var);

            Write(data);
            PutByte(0);     //  Null terminate
            PutByte(0);     //  Null terminate
            return prevIndex;
        }


        public void OverwriteBoolean(Int32 writeIndex, Boolean var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, 1, writeIndex);
        }


        public void OverwriteByte(Int32 writeIndex, Byte var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, 1, writeIndex);
        }


        public void OverwriteInt16(Int32 writeIndex, Int16 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(Int16), writeIndex);
        }


        public void OverwriteUInt16(Int32 writeIndex, UInt16 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(UInt16), writeIndex);
        }


        public void OverwriteInt32(Int32 writeIndex, Int32 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(Int32), writeIndex);
        }


        public void OverwriteUInt32(Int32 writeIndex, UInt32 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(UInt32), writeIndex);
        }


        public void OverwriteInt64(Int32 writeIndex, Int64 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(Int64), writeIndex);
        }


        public void OverwriteUInt64(Int32 writeIndex, UInt64 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(UInt64), writeIndex);
        }


        public void OverwriteDouble(Int32 writeIndex, Double var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(Double), writeIndex);
        }
    }
}
