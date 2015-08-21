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
    public class StreamBuffer
    {
        public static Int32 AllocBlockSize = 128;

        public Int32 ReadBytes { get; private set; }
        public Int32 WrittenBytes { get; private set; }

        public byte[] Buffer { get; private set; }
        public Int32 BufferSize { get { return Buffer.Length; } }
        public Int32 ReadableSize { get { return WrittenBytes - ReadBytes; } }
        public Int32 WritableSize { get { return Buffer.Length - WrittenBytes; } }





        public StreamBuffer()
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(256);
        }


        public StreamBuffer(Int32 size)
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(size);
        }


        public StreamBuffer(StreamBuffer source)
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(source.BufferSize);
            Write(source.Buffer);
        }


        public StreamBuffer(byte[] source, Int32 index, Int32 size)
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(size);
            Write(source, index, size);
        }


        public StreamBuffer(StreamBuffer source, Int32 index, Int32 size)
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(size);
            Write(source.Buffer, index, size);
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
            ReadBytes = 0;
            WrittenBytes = 0;
        }


        public void ResetReadIndex()
        {
            ReadBytes = 0;
        }


        public void ResetWriteIndex()
        {
            WrittenBytes = 0;
            OnWritten();
        }


        protected virtual void OnWritten()
        {
        }


        public void PopReadBuffer()
        {
            if (ReadBytes == 0)
                return;

            Array.Copy(Buffer, ReadBytes, Buffer, 0, WrittenBytes - ReadBytes);
            WrittenBytes -= ReadBytes;
            ReadBytes = 0;

            OnWritten();
        }


        public void Write(Int32 size)
        {
            if (WrittenBytes + size > BufferSize)
                Resize(BufferSize + size);

            WrittenBytes += size;
            OnWritten();
        }


        public void Write(Byte source)
        {
            Int32 srcSize = sizeof(Byte);
            if (WrittenBytes + srcSize > BufferSize)
                Resize(BufferSize + srcSize);

            Buffer[WrittenBytes] = source;
            WrittenBytes += srcSize;

            OnWritten();
        }


        public void Write(Byte[] source)
        {
            Int32 srcSize = source.Length;
            if (WrittenBytes + srcSize > BufferSize)
                Resize(BufferSize + srcSize);

            Array.Copy(source, 0, Buffer, WrittenBytes, srcSize);
            WrittenBytes += srcSize;

            OnWritten();
        }


        public void Write(StreamBuffer source)
        {
            Int32 srcSize = source.WrittenBytes;
            if (WrittenBytes + srcSize > BufferSize)
                Resize(BufferSize + srcSize);

            Array.Copy(source.Buffer, 0, Buffer, WrittenBytes, srcSize);
            WrittenBytes += srcSize;

            OnWritten();
        }


        public void Write(Byte[] source, Int32 index)
        {
            if (index >= source.Length)
                throw new AegisException(ResultCode.InvalidArgument, "The argument index(={0}) is larger then source size(={1}).", index, source.Length);

            Int32 copyBytes = source.Length - index;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, index, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void Write(Byte[] source, Int32 index, Int32 size)
        {
            if (index + size > source.Length)
                throw new AegisException(ResultCode.InvalidArgument, "The source buffer is small then requested.");

            Int32 copyBytes = size;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, index, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void Write(StreamBuffer source, Int32 index)
        {
            if (index >= source.WrittenBytes)
                throw new AegisException(ResultCode.InvalidArgument, "The argument index(={0}) is larger then source size(={1}).", index, source.WrittenBytes);

            Int32 copyBytes = source.WrittenBytes - index;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source.Buffer, index, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void Write(StreamBuffer source, Int32 index, Int32 size)
        {
            if (index + size > source.WrittenBytes)
                throw new AegisException(ResultCode.InvalidArgument, "The source buffer is small then requested.");

            Int32 copyBytes = size;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source.Buffer, index, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void Overwrite(Byte source, Int32 writeIndex)
        {
            Int32 copyBytes = sizeof(Byte);
            if (writeIndex + copyBytes >= BufferSize)
                Resize(BufferSize + copyBytes);

            Buffer[writeIndex] = source;

            if (writeIndex + copyBytes > WrittenBytes)
            {
                WrittenBytes = writeIndex + copyBytes;
                OnWritten();
            }
        }


        public void Overwrite(Byte[] source, Int32 index, Int32 size, Int32 writeIndex)
        {
            if (index + size > source.Length)
                throw new AegisException(ResultCode.InvalidArgument, "The source buffer is small then requested.");

            Int32 copyBytes = size;
            if (writeIndex + copyBytes >= BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, index, Buffer, writeIndex, copyBytes);

            if (writeIndex + copyBytes > WrittenBytes)
            {
                WrittenBytes = writeIndex + copyBytes;
                OnWritten();
            }
        }


        public void Read(Int32 size)
        {
            if (ReadBytes + size > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            ReadBytes += size;
        }


        public Byte Read()
        {
            if (ReadBytes + sizeof(Byte) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var value = Buffer[ReadBytes];
            ReadBytes += sizeof(Byte);

            return value;
        }


        public void Read(Byte[] destination)
        {
            if (destination.Length < BufferSize)
                throw new AegisException(ResultCode.NotEnoughBuffer, "Destination buffer size too small.");

            Array.Copy(Buffer, destination, BufferSize);
            ReadBytes = BufferSize;
        }


        public void Read(Byte[] destination, Int32 index)
        {
            if (destination.Length - index < BufferSize)
                throw new AegisException(ResultCode.NotEnoughBuffer, "Destination buffer size too small.");

            Array.Copy(Buffer, 0, destination, index, BufferSize);
            ReadBytes += BufferSize;
        }


        public void Read(Byte[] destination, Int32 index, Int32 readIndex, Int32 size)
        {
            if (destination.Length - index < size)
                throw new AegisException(ResultCode.NotEnoughBuffer, "Destination buffer size too small.");

            Array.Copy(Buffer, readIndex, destination, index, size);
        }


        public Boolean GetBoolean()
        {
            if (ReadBytes + sizeof(Boolean) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = Buffer[ReadBytes];
            ReadBytes += sizeof(Boolean);
            return (val == 1);
        }


        public SByte GetSByte()
        {
            if (ReadBytes + sizeof(SByte) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = (SByte)Buffer[ReadBytes];
            ReadBytes += sizeof(SByte);
            return val;
        }


        public Byte GetByte()
        {
            if (ReadBytes + sizeof(Byte) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = (Byte)Buffer[ReadBytes];
            ReadBytes += sizeof(Byte);
            return val;
        }


        public char GetChar()
        {
            if (ReadBytes + sizeof(char) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToChar(Buffer, ReadBytes);
            ReadBytes += sizeof(char);
            return val;
        }


        public Int16 GetInt16()
        {
            if (ReadBytes + sizeof(Int16) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToInt16(Buffer, ReadBytes);
            ReadBytes += sizeof(Int16);
            return val;
        }


        public UInt16 GetUInt16()
        {
            if (ReadBytes + sizeof(UInt16) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToUInt16(Buffer, ReadBytes);
            ReadBytes += sizeof(UInt16);
            return val;
        }


        public Int32 GetInt32()
        {
            if (ReadBytes + sizeof(Int32) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToInt32(Buffer, ReadBytes);
            ReadBytes += sizeof(Int32);
            return val;
        }


        public UInt32 GetUInt32()
        {
            if (ReadBytes + sizeof(UInt32) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToUInt32(Buffer, ReadBytes);
            ReadBytes += sizeof(UInt32);
            return val;
        }


        public Int64 GetInt64()
        {
            if (ReadBytes + sizeof(Int64) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToInt64(Buffer, ReadBytes);
            ReadBytes += sizeof(Int64);
            return val;
        }


        public UInt64 GetUInt64()
        {
            if (ReadBytes + sizeof(UInt64) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToUInt64(Buffer, ReadBytes);
            ReadBytes += sizeof(UInt64);
            return val;
        }


        public Double GetDouble()
        {
            if (ReadBytes + sizeof(UInt64) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            var val = BitConverter.ToDouble(Buffer, ReadBytes);
            ReadBytes += sizeof(Double);
            return val;
        }


        public String GetStringFromUtf8()
        {
            Int32 i, stringBytes = 0;
            for (i = ReadBytes; i < BufferSize; ++i)
            {
                if (Buffer[i] == 0)
                    break;

                ++stringBytes;
                if (i > WrittenBytes)
                    throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            String val = Encoding.UTF8.GetString(Buffer, ReadBytes, stringBytes);
            ReadBytes += stringBytes + 1;
            return val;
        }


        public String GetStringFromUtf16()
        {
            Int32 i, stringBytes = 0;
            for (i = ReadBytes; i < BufferSize; i += 2)
            {
                if (Buffer[i + 0] == 0
                    && Buffer[i + 1] == 0)
                    break;

                stringBytes += 2;

                if (ReadBytes + stringBytes + 2 > WrittenBytes)
                    throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            String val = Encoding.Unicode.GetString(Buffer, ReadBytes, stringBytes);
            ReadBytes += stringBytes + 2;
            return val;
        }


        public Boolean GetBoolean(Int32 readIndex)
        {
            if (readIndex + sizeof(byte) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return (Buffer[readIndex] == 1);
        }


        public SByte GetSByte(Int32 readIndex)
        {
            if (readIndex + sizeof(SByte) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return (SByte)Buffer[readIndex];
        }


        public Byte GetByte(Int32 readIndex)
        {
            if (readIndex + sizeof(Byte) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return Buffer[readIndex];
        }


        public Char GetChar(Int32 readIndex)
        {
            if (readIndex + sizeof(Char) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToChar(Buffer, readIndex);
        }


        public Int16 GetInt16(Int32 readIndex)
        {
            if (readIndex + sizeof(Int16) > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToInt16(Buffer, readIndex);
        }


        public UInt16 GetUInt16(Int32 readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToUInt16(Buffer, readIndex);
        }


        public Int32 GetInt32(Int32 readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToInt32(Buffer, readIndex);
        }


        public UInt32 GetUInt32(Int32 readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToUInt32(Buffer, readIndex);
        }


        public Int64 GetInt64(Int32 readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToInt64(Buffer, readIndex);
        }


        public UInt64 GetUInt64(Int32 readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");

            return BitConverter.ToUInt64(Buffer, readIndex);
        }


        public Double GetDouble(Int32 readIndex)
        {
            if (readIndex > WrittenBytes)
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
                if (i > WrittenBytes)
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

                if (readIndex + stringBytes + 2 > WrittenBytes)
                    throw new AegisException(ResultCode.NotEnoughBuffer, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            return Encoding.Unicode.GetString(Buffer, readIndex, stringBytes);
        }


        public Int32 PutBoolean(Boolean var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var), 0, 1);
            return prevIndex;
        }


        public Int32 PutSByte(SByte var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(var);
            return prevIndex;
        }


        public Int32 PutByte(Byte var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(var);
            return prevIndex;
        }


        public Int32 PutChar(Char var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutInt16(Int16 var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutUInt16(UInt16 var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutInt32(Int32 var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutUInt32(UInt32 var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutInt64(Int64 var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutUInt64(UInt64 var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutDouble(Double var)
        {
            Int32 prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public Int32 PutStringAsUtf8(String var)
        {
            Int32 prevIndex = WrittenBytes;
            byte[] data = Encoding.UTF8.GetBytes(var);

            Write(data);
            PutByte(0);     //  Null terminate
            return prevIndex;
        }


        public Int32 PutStringAsUtf16(String var)
        {
            Int32 prevIndex = WrittenBytes;
            byte[] data = Encoding.Unicode.GetBytes(var);

            Write(data);
            PutInt16(0);    //  Null terminate (2 byte)
            return prevIndex;
        }


        public void OverwriteBoolean(Int32 writeIndex, Boolean var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, 1, writeIndex);
        }


        public void OverwriteSByte(Int32 writeIndex, SByte var)
        {
            Overwrite((Byte)var, writeIndex);
        }


        public void OverwriteByte(Int32 writeIndex, Byte var)
        {
            Overwrite((Byte)var, writeIndex);
        }


        public void OverwriteChar(Int32 writeIndex, Char var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(Char), writeIndex);
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
