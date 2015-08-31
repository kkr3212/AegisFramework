using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Aegis.Client
{
    /// <summary>
    /// 데이터를 순차적으로 읽거나 쓸 수 있는 버퍼입니다.
    /// 데이터 쓰기의 경우, 버퍼가 부족하면 자동으로 증가시킵니다.
    /// 데이터 읽기의 경우, 쓰기된 크기 이상으로 읽어들일 수 없습니다.
    /// </summary>
    public class StreamBuffer
    {
        public static int AllocBlockSize = 128;

        public int ReadBytes { get; private set; }
        public int WrittenBytes { get; private set; }

        public byte[] Buffer { get; private set; }
        public int BufferSize { get { return Buffer.Length; } }
        public int ReadableSize { get { return WrittenBytes - ReadBytes; } }
        public int WritableSize { get { return Buffer.Length - WrittenBytes; } }





        public StreamBuffer()
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(256);
        }


        public StreamBuffer(int size)
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


        public StreamBuffer(byte[] source, int index, int size)
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(size);
            Write(source, index, size);
        }


        public StreamBuffer(StreamBuffer source, int index, int size)
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(size);
            Write(source.Buffer, index, size);
        }


        private int AllocateBlockSize(int size)
        {
            return (size / AllocBlockSize + (size % AllocBlockSize > 0 ? 1 : 0)) * AllocBlockSize;
        }


        public void Capacity(int size)
        {
            int allocSize = AllocateBlockSize(size);
            Buffer = new byte[allocSize];
        }


        public void Resize(int size)
        {
            if (size <= BufferSize)
                return;

            int allocSize = AllocateBlockSize(size);
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


        public void Write(int size)
        {
            if (WrittenBytes + size > BufferSize)
                Resize(BufferSize + size);

            WrittenBytes += size;
            OnWritten();
        }


        public void Write(byte[] source)
        {
            int srcSize = source.Length;
            if (WrittenBytes + srcSize > BufferSize)
                Resize(BufferSize + srcSize);

            Array.Copy(source, 0, Buffer, WrittenBytes, srcSize);
            WrittenBytes += srcSize;

            OnWritten();
        }


        public void Write(byte[] source, int index)
        {
            if (index >= source.Length)
                throw new AegisException("The argument index(={0}) is larger then source size(={1}).", index, source.Length);

            int copyBytes = source.Length - index;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, index, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void Write(byte[] source, int index, int size)
        {
            if (index + size > source.Length)
                throw new AegisException("The source buffer is small then requested.");

            int copyBytes = size;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, index, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void Overwrite(byte[] source, int index, int size, int writeIndex)
        {
            if (index + size > source.Length)
                throw new AegisException("The source buffer is small then requested.");

            int copyBytes = size;
            if (writeIndex + copyBytes >= BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, index, Buffer, writeIndex, copyBytes);

            if (writeIndex + copyBytes > WrittenBytes)
            {
                WrittenBytes = writeIndex + copyBytes;
                OnWritten();
            }
        }


        public void Read(int size)
        {
            if (ReadBytes + size > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            ReadBytes += size;
        }


        public void Read(byte[] destination)
        {
            if (destination.Length < BufferSize)
                throw new AegisException("Destination buffer size too small.");

            Array.Copy(Buffer, destination, BufferSize);
            ReadBytes = BufferSize;
        }


        public void Read(byte[] destination, int index)
        {
            if (destination.Length - index < BufferSize)
                throw new AegisException("Destination buffer size too small.");

            Array.Copy(Buffer, 0, destination, index, BufferSize);
            ReadBytes += BufferSize;
        }


        public void Read(byte[] destination, int index, int readIndex, int size)
        {
            if (destination.Length - index < size)
                throw new AegisException("Destination buffer size too small.");

            Array.Copy(Buffer, readIndex, destination, index, size);
        }


        public bool GetBoolean()
        {
            if (ReadBytes + sizeof(byte) > WrittenBytes)
                throw new AegisException( "No more readable buffer.");

            var val = Buffer[ReadBytes];
            ReadBytes += sizeof(byte);
            return (val == 1);
        }


        public byte GetByte()
        {
            if (ReadBytes + sizeof(byte) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            var val = Buffer[ReadBytes];
            ReadBytes += sizeof(byte);
            return val;
        }


        public Int16 GetInt16()
        {
            if (ReadBytes + sizeof(Int16) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            var val = BitConverter.ToInt16(Buffer, ReadBytes);
            ReadBytes += sizeof(Int16);
            return val;
        }


        public UInt16 GetUInt16()
        {
            if (ReadBytes + sizeof(UInt16) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            var val = BitConverter.ToUInt16(Buffer, ReadBytes);
            ReadBytes += sizeof(UInt16);
            return val;
        }


        public int GetInt32()
        {
            if (ReadBytes + sizeof(int) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            var val = BitConverter.ToInt32(Buffer, ReadBytes);
            ReadBytes += sizeof(int);
            return val;
        }


        public UInt32 GetUInt32()
        {
            if (ReadBytes + sizeof(UInt32) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            var val = BitConverter.ToUInt32(Buffer, ReadBytes);
            ReadBytes += sizeof(UInt32);
            return val;
        }


        public Int64 GetInt64()
        {
            if (ReadBytes + sizeof(Int64) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            var val = BitConverter.ToInt64(Buffer, ReadBytes);
            ReadBytes += sizeof(Int64);
            return val;
        }


        public UInt64 GetUInt64()
        {
            if (ReadBytes + sizeof(UInt64) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            var val = BitConverter.ToUInt64(Buffer, ReadBytes);
            ReadBytes += sizeof(UInt64);
            return val;
        }


        public Double GetDouble()
        {
            if (ReadBytes + sizeof(UInt64) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            var val = BitConverter.ToDouble(Buffer, ReadBytes);
            ReadBytes += sizeof(Double);
            return val;
        }


        public String GetStringFromUtf8()
        {
            int i, stringBytes = 0;
            for (i = ReadBytes; i < BufferSize; ++i)
            {
                if (Buffer[i] == 0)
                    break;

                ++stringBytes;
                if (i > WrittenBytes)
                    throw new AegisException("No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            String val = Encoding.UTF8.GetString(Buffer, ReadBytes, stringBytes);
            ReadBytes += stringBytes + 1;
            return val;
        }


        public String GetStringFromUtf16()
        {
            int i, stringBytes = 0;
            for (i = ReadBytes; i < BufferSize; i += 2)
            {
                if (Buffer[i + 0] == 0
                    && Buffer[i + 1] == 0)
                    break;

                stringBytes += 2;

                if (ReadBytes + stringBytes + 2 > WrittenBytes)
                    throw new AegisException("No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            String val = Encoding.Unicode.GetString(Buffer, ReadBytes, stringBytes);
            ReadBytes += stringBytes + 2;
            return val;
        }


        public bool GetBoolean(int readIndex)
        {
            if (readIndex + sizeof(byte) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            return (Buffer[readIndex] == 1);
        }


        public byte GetByte(int readIndex)
        {
            if (readIndex + sizeof(byte) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            return Buffer[readIndex];
        }


        public Int16 GetInt16(int readIndex)
        {
            if (readIndex + sizeof(Int16) > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            return BitConverter.ToInt16(Buffer, readIndex);
        }


        public UInt16 GetUInt16(int readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            return BitConverter.ToUInt16(Buffer, readIndex);
        }


        public int GetInt32(int readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            return BitConverter.ToInt32(Buffer, readIndex);
        }


        public UInt32 GetUInt32(int readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            return BitConverter.ToUInt32(Buffer, readIndex);
        }


        public Int64 GetInt64(int readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            return BitConverter.ToInt64(Buffer, readIndex);
        }


        public UInt64 GetUInt64(int readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            return BitConverter.ToUInt64(Buffer, readIndex);
        }


        public Double GetDouble(int readIndex)
        {
            if (readIndex > WrittenBytes)
                throw new AegisException("No more readable buffer.");

            return BitConverter.ToDouble(Buffer, readIndex);
        }


        public String GetStringFromUtf8(int readIndex)
        {
            int i, stringBytes = 0;
            for (i = readIndex; i < BufferSize; ++i)
            {
                if (Buffer[i] == 0)
                    break;

                ++stringBytes;
                if (i > WrittenBytes)
                    throw new AegisException("No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            return Encoding.UTF8.GetString(Buffer, readIndex, stringBytes);
        }


        public String GetStringFromUtf16(int readIndex)
        {
            int i, stringBytes = 0;
            for (i = readIndex; i < BufferSize; i += 2)
            {
                if (Buffer[i + 0] == 0
                    && Buffer[i + 1] == 0)
                    break;

                stringBytes += 2;

                if (readIndex + stringBytes + 2 > WrittenBytes)
                    throw new AegisException("No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            return Encoding.Unicode.GetString(Buffer, readIndex, stringBytes);
        }


        public int PutBoolean(bool var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var), 0, 1);
            return prevIndex;
        }


        public int PutByte(Byte var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var), 0, 1);
            return prevIndex;
        }


        public int PutInt16(Int16 var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutUInt16(UInt16 var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutInt32(int var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutUInt32(UInt32 var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutInt64(Int64 var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutUInt64(UInt64 var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutDouble(Double var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutStringAsUtf8(String var)
        {
            int prevIndex = WrittenBytes;
            byte[] data = Encoding.UTF8.GetBytes(var);

            Write(data);
            PutByte(0);     //  Null terminate
            return prevIndex;
        }


        public int PutStringAsUtf16(String var)
        {
            int prevIndex = WrittenBytes;
            byte[] data = Encoding.Unicode.GetBytes(var);

            Write(data);
            PutInt16(0);    //  Null terminate (2 byte)
            return prevIndex;
        }


        public void OverwriteBoolean(int writeIndex, bool var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, 1, writeIndex);
        }


        public void OverwriteByte(int writeIndex, Byte var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, 1, writeIndex);
        }


        public void OverwriteInt16(int writeIndex, Int16 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(Int16), writeIndex);
        }


        public void OverwriteUInt16(int writeIndex, UInt16 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(UInt16), writeIndex);
        }


        public void OverwriteInt32(int writeIndex, int var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(int), writeIndex);
        }


        public void OverwriteUInt32(int writeIndex, UInt32 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(UInt32), writeIndex);
        }


        public void OverwriteInt64(int writeIndex, Int64 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(Int64), writeIndex);
        }


        public void OverwriteUInt64(int writeIndex, UInt64 var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(UInt64), writeIndex);
        }


        public void OverwriteDouble(int writeIndex, Double var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(Double), writeIndex);
        }
    }
}
