using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.IO
{
    /// <summary>
    /// 데이터를 순차적으로 읽거나 쓸 수 있는 버퍼입니다.
    /// 데이터 쓰기의 경우, 버퍼가 부족하면 자동으로 증가시킵니다.
    /// 데이터 읽기의 경우, 쓰기된 크기 이상으로 읽어들일 수 없습니다.
    /// </summary>
    public class StreamBuffer
    {
        public const int AllocBlockSize = 128;

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


        public StreamBuffer(byte[] source)
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(source.Length);
            Write(source, 0, source.Length);
        }


        public StreamBuffer(StreamBuffer source)
        {
            ReadBytes = 0;
            WrittenBytes = 0;

            Capacity(source.WrittenBytes);
            Write(source.Buffer, 0, source.WrittenBytes);
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


        public virtual StreamBuffer Clone()
        {
            StreamBuffer newStream = new StreamBuffer(this);
            newStream.ReadBytes = ReadBytes;
            newStream.WrittenBytes = WrittenBytes;

            return newStream;
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


        public void Write(char source)
        {
            int srcSize = sizeof(char);
            if (WrittenBytes + srcSize > BufferSize)
                Resize(BufferSize + srcSize);

            Buffer[WrittenBytes] = (byte)source;
            WrittenBytes += srcSize;

            OnWritten();
        }


        public void Write(byte source)
        {
            int srcSize = sizeof(byte);
            if (WrittenBytes + srcSize > BufferSize)
                Resize(BufferSize + srcSize);

            Buffer[WrittenBytes] = source;
            WrittenBytes += srcSize;

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


        public void Write(StreamBuffer source)
        {
            int srcSize = source.WrittenBytes;
            if (WrittenBytes + srcSize > BufferSize)
                Resize(BufferSize + srcSize);

            Array.Copy(source.Buffer, 0, Buffer, WrittenBytes, srcSize);
            WrittenBytes += srcSize;

            OnWritten();
        }


        public void Write(byte[] source, int sourceIndex)
        {
            if (sourceIndex >= source.Length)
                throw new AegisException(AegisResult.BufferUnderflow, "The argument index(={0}) is larger then source size(={1}).", sourceIndex, source.Length);

            int copyBytes = source.Length - sourceIndex;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, sourceIndex, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void Write(byte[] source, int sourceIndex, int length)
        {
            if (sourceIndex + length > source.Length)
                throw new AegisException(AegisResult.BufferUnderflow, "The source buffer is small then requested.");

            int copyBytes = length;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source, sourceIndex, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void Write(StreamBuffer source, int sourceIndex)
        {
            if (sourceIndex >= source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "The argument index(={0}) is larger then source size(={1}).", sourceIndex, source.WrittenBytes);

            int copyBytes = source.WrittenBytes - sourceIndex;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source.Buffer, sourceIndex, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void Write(StreamBuffer source, int sourceIndex, int length)
        {
            if (sourceIndex + length > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "The source buffer is small then requested.");

            int copyBytes = length;
            if (WrittenBytes + copyBytes > BufferSize)
                Resize(BufferSize + copyBytes);

            Array.Copy(source.Buffer, sourceIndex, Buffer, WrittenBytes, copyBytes);
            WrittenBytes += copyBytes;

            OnWritten();
        }


        public void WriteWithParams(params object[] args)
        {
            foreach (var v in args)
            {
                switch (v.GetType().Name)
                {
                    case "Bool": PutBoolean((bool)v); break;
                    case "Byte": PutByte((byte)v); break;
                    case "SByte": PutSByte((sbyte)v); break;
                    case "Char": PutChar((char)v); break;
                    case "Int16": PutInt16((short)v); break;
                    case "UInt16": PutUInt16((ushort)v); break;
                    case "Int32": PutInt32((int)v); break;
                    case "UInt32": PutUInt32((uint)v); break;
                    case "Int64": PutInt64((long)v); break;
                    case "UInt64": PutUInt64((ulong)v); break;
                    case "String": PutStringAsUtf16((string)v); break;
                    case "Double": PutDouble((double)v); break;
                }
            }
        }


        public void Overwrite(char source, int writeIndex)
        {
            int copyBytes = sizeof(char);
            if (writeIndex + copyBytes >= BufferSize)
                Resize(BufferSize + copyBytes);

            Buffer[writeIndex] = (byte)source;

            if (writeIndex + copyBytes > WrittenBytes)
            {
                WrittenBytes = writeIndex + copyBytes;
                OnWritten();
            }
        }


        public void Overwrite(byte source, int writeIndex)
        {
            int copyBytes = sizeof(byte);
            if (writeIndex + copyBytes >= BufferSize)
                Resize(BufferSize + copyBytes);

            Buffer[writeIndex] = source;

            if (writeIndex + copyBytes > WrittenBytes)
            {
                WrittenBytes = writeIndex + copyBytes;
                OnWritten();
            }
        }


        public void Overwrite(byte[] source, int index, int size, int writeIndex)
        {
            if (index + size > source.Length)
                throw new AegisException(AegisResult.BufferUnderflow, "The source buffer is small then requested.");

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
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            ReadBytes += size;
        }


        public byte Read()
        {
            if (ReadBytes + sizeof(byte) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var value = Buffer[ReadBytes];
            ReadBytes += sizeof(byte);

            return value;
        }


        public void Read(byte[] destination)
        {
            if (destination.Length < BufferSize)
                throw new AegisException(AegisResult.BufferUnderflow, "Destination buffer size too small.");

            Array.Copy(Buffer, destination, BufferSize);
            ReadBytes = BufferSize;
        }


        public void Read(byte[] destination, int destinationIndex)
        {
            int copyBytes = WrittenBytes - ReadBytes;
            if (destination.Length - destinationIndex < copyBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "Destination buffer size too small.");

            Array.Copy(Buffer, ReadBytes, destination, destinationIndex, copyBytes);
            ReadBytes += copyBytes;
        }


        public void Read(byte[] destination, int destinationIndex, int length)
        {
            if (destination.Length - destinationIndex < length)
                throw new AegisException(AegisResult.BufferUnderflow, "Destination buffer size too small.");

            Array.Copy(Buffer, ReadBytes, destination, destinationIndex, length);
            ReadBytes += length;
        }


        public bool GetBoolean()
        {
            if (ReadBytes + sizeof(bool) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = Buffer[ReadBytes];
            ReadBytes += sizeof(bool);
            return (val == 1);
        }


        public sbyte GetSByte()
        {
            if (ReadBytes + sizeof(sbyte) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = (sbyte)Buffer[ReadBytes];
            ReadBytes += sizeof(sbyte);
            return val;
        }


        public byte GetByte()
        {
            if (ReadBytes + sizeof(byte) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = (byte)Buffer[ReadBytes];
            ReadBytes += sizeof(byte);
            return val;
        }


        public char GetChar()
        {
            if (ReadBytes + sizeof(char) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = BitConverter.ToChar(Buffer, ReadBytes);
            ReadBytes += sizeof(char);
            return val;
        }


        public short GetInt16()
        {
            if (ReadBytes + sizeof(short) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = BitConverter.ToInt16(Buffer, ReadBytes);
            ReadBytes += sizeof(short);
            return val;
        }


        public ushort GetUInt16()
        {
            if (ReadBytes + sizeof(ushort) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = BitConverter.ToUInt16(Buffer, ReadBytes);
            ReadBytes += sizeof(ushort);
            return val;
        }


        public int GetInt32()
        {
            if (ReadBytes + sizeof(int) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = BitConverter.ToInt32(Buffer, ReadBytes);
            ReadBytes += sizeof(int);
            return val;
        }


        public uint GetUInt32()
        {
            if (ReadBytes + sizeof(uint) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = BitConverter.ToUInt32(Buffer, ReadBytes);
            ReadBytes += sizeof(uint);
            return val;
        }


        public long GetInt64()
        {
            if (ReadBytes + sizeof(long) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = BitConverter.ToInt64(Buffer, ReadBytes);
            ReadBytes += sizeof(long);
            return val;
        }


        public ulong GetUInt64()
        {
            if (ReadBytes + sizeof(ulong) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = BitConverter.ToUInt64(Buffer, ReadBytes);
            ReadBytes += sizeof(ulong);
            return val;
        }


        public double GetDouble()
        {
            if (ReadBytes + sizeof(ulong) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            var val = BitConverter.ToDouble(Buffer, ReadBytes);
            ReadBytes += sizeof(double);
            return val;
        }


        public string GetStringAsUtf8()
        {
            int i, stringBytes = 0;
            for (i = ReadBytes; i < BufferSize; ++i)
            {
                if (Buffer[i] == 0)
                    break;

                ++stringBytes;
                if (i > WrittenBytes)
                    throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            string val = Encoding.UTF8.GetString(Buffer, ReadBytes, stringBytes);
            ReadBytes += stringBytes + 1;
            return val;
        }


        public string GetStringAsUtf16()
        {
            int i, stringBytes = 0;
            for (i = ReadBytes; i < BufferSize; i += 2)
            {
                if (Buffer[i + 0] == 0
                    && Buffer[i + 1] == 0)
                    break;

                stringBytes += 2;

                if (ReadBytes + stringBytes + 2 > WrittenBytes)
                    throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            string val = Encoding.Unicode.GetString(Buffer, ReadBytes, stringBytes);
            ReadBytes += stringBytes + 2;
            return val;
        }


        public bool GetBoolean(int readIndex)
        {
            if (readIndex + sizeof(bool) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return (Buffer[readIndex] == 1);
        }


        public sbyte GetSByte(int readIndex)
        {
            if (readIndex + sizeof(sbyte) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return (sbyte)Buffer[readIndex];
        }


        public byte GetByte(int readIndex)
        {
            if (readIndex + sizeof(byte) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return Buffer[readIndex];
        }


        public char GetChar(int readIndex)
        {
            if (readIndex + sizeof(char) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToChar(Buffer, readIndex);
        }


        public short GetInt16(int readIndex)
        {
            if (readIndex + sizeof(short) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToInt16(Buffer, readIndex);
        }


        public ushort GetUInt16(int readIndex)
        {
            if (readIndex + sizeof(ushort) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToUInt16(Buffer, readIndex);
        }


        public int GetInt32(int readIndex)
        {
            if (readIndex + sizeof(int) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToInt32(Buffer, readIndex);
        }


        public uint GetUInt32(int readIndex)
        {
            if (readIndex + sizeof(uint) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToUInt32(Buffer, readIndex);
        }


        public long GetInt64(int readIndex)
        {
            if (readIndex + sizeof(long) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToInt64(Buffer, readIndex);
        }


        public ulong GetUInt64(int readIndex)
        {
            if (readIndex + sizeof(ulong) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToUInt64(Buffer, readIndex);
        }


        public double GetDouble(int readIndex)
        {
            if (readIndex + sizeof(double) > WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToDouble(Buffer, readIndex);
        }


        public string GetStringAsUtf8(int readIndex)
        {
            int i, stringBytes = 0;
            for (i = readIndex; i < BufferSize; ++i)
            {
                if (Buffer[i] == 0)
                    break;

                ++stringBytes;
                if (i > WrittenBytes)
                    throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            return Encoding.UTF8.GetString(Buffer, readIndex, stringBytes);
        }


        public string GetStringAsUtf16(int readIndex)
        {
            int i, stringBytes = 0;
            for (i = readIndex; i < BufferSize; i += 2)
            {
                if (Buffer[i + 0] == 0
                    && Buffer[i + 1] == 0)
                    break;

                stringBytes += 2;

                if (readIndex + stringBytes + 2 > WrittenBytes)
                    throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            return Encoding.Unicode.GetString(Buffer, readIndex, stringBytes);
        }


        public static bool GetBoolean(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(byte) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return (source.Buffer[readIndex] == 1);
        }


        public static sbyte GetSByte(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(sbyte) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return (sbyte)source.Buffer[readIndex];
        }


        public static byte GetByte(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(byte) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return source.Buffer[readIndex];
        }


        public static char GetChar(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(char) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToChar(source.Buffer, readIndex);
        }


        public static short GetInt16(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(short) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToInt16(source.Buffer, readIndex);
        }


        public static ushort GetUInt16(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(ushort) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToUInt16(source.Buffer, readIndex);
        }


        public static int GetInt32(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(int) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToInt32(source.Buffer, readIndex);
        }


        public static uint GetUInt32(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(uint) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToUInt32(source.Buffer, readIndex);
        }


        public static long GetInt64(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(long) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToInt64(source.Buffer, readIndex);
        }


        public static ulong GetUInt64(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(ulong) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToUInt64(source.Buffer, readIndex);
        }


        public static double GetDouble(StreamBuffer source, int readIndex)
        {
            if (readIndex + sizeof(double) > source.WrittenBytes)
                throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");

            return BitConverter.ToDouble(source.Buffer, readIndex);
        }


        public static string GetStringAsUtf8(StreamBuffer source, int readIndex)
        {
            int i, stringBytes = 0;
            for (i = readIndex; i < source.BufferSize; ++i)
            {
                if (source.Buffer[i] == 0)
                    break;

                ++stringBytes;
                if (i > source.WrittenBytes)
                    throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            return Encoding.UTF8.GetString(source.Buffer, readIndex, stringBytes);
        }


        public static string GetStringAsUtf16(StreamBuffer source, int readIndex)
        {
            int i, stringBytes = 0;
            for (i = readIndex; i < source.WrittenBytes; i += 2)
            {
                if (source.Buffer[i + 0] == 0
                    && source.Buffer[i + 1] == 0)
                    break;

                stringBytes += 2;

                if (readIndex + stringBytes + 2 > source.WrittenBytes)
                    throw new AegisException(AegisResult.BufferUnderflow, "No more readable buffer.");
            }


            //  String으로 변환할 때 Null terminate를 포함시켜서는 안된다.
            return Encoding.Unicode.GetString(source.Buffer, readIndex, stringBytes);
        }


        public int PutBoolean(bool var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var), 0, sizeof(bool));
            return prevIndex;
        }


        public int PutSByte(sbyte var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var), 0, sizeof(sbyte));
            return prevIndex;
        }


        public int PutByte(byte var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var), 0, sizeof(byte));
            return prevIndex;
        }


        public int PutChar(char var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var), 0, sizeof(char));
            return prevIndex;
        }


        public int PutInt16(short var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutUInt16(ushort var)
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


        public int PutUInt32(uint var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutInt64(long var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutUInt64(ulong var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutDouble(double var)
        {
            int prevIndex = WrittenBytes;

            Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public int PutStringAsUtf8(string var)
        {
            int prevIndex = WrittenBytes;
            byte[] data = Encoding.UTF8.GetBytes(var);

            Write(data);
            PutByte(0);     //  Null terminate
            return prevIndex;
        }


        public int PutStringAsUtf16(string var)
        {
            int prevIndex = WrittenBytes;
            byte[] data = Encoding.Unicode.GetBytes(var);

            Write(data);
            PutInt16(0);    //  Null terminate (2 byte)
            return prevIndex;
        }


        public static int PutBoolean(StreamBuffer destination, bool var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var), 0, sizeof(bool));
            return prevIndex;
        }


        public static int PutSByte(StreamBuffer destination, sbyte var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var), 0, sizeof(sbyte));
            return prevIndex;
        }


        public static int PutByte(StreamBuffer destination, byte var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var), 0, sizeof(byte));
            return prevIndex;
        }


        public static int PutChar(StreamBuffer destination, char var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var), 0, sizeof(char));
            return prevIndex;
        }


        public static int PutInt16(StreamBuffer destination, short var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public static int PutUInt16(StreamBuffer destination, ushort var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public static int PutInt32(StreamBuffer destination, int var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public static int PutUInt32(StreamBuffer destination, uint var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public static int PutInt64(StreamBuffer destination, long var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public static int PutUInt64(StreamBuffer destination, ulong var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public static int PutDouble(StreamBuffer destination, double var)
        {
            int prevIndex = destination.WrittenBytes;

            destination.Write(BitConverter.GetBytes(var));
            return prevIndex;
        }


        public static int PutStringAsUtf8(StreamBuffer destination, string var)
        {
            int prevIndex = destination.WrittenBytes;
            byte[] data = Encoding.UTF8.GetBytes(var);

            destination.Write(data);
            destination.PutByte(0);     //  Null terminate
            return prevIndex;
        }


        public static int PutStringAsUtf16(StreamBuffer destination, string var)
        {
            int prevIndex = destination.WrittenBytes;
            byte[] data = Encoding.Unicode.GetBytes(var);

            destination.Write(data);
            destination.PutInt16(0);    //  Null terminate (2 byte)
            return prevIndex;
        }


        public void OverwriteBoolean(int writeIndex, bool var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, 1, writeIndex);
        }


        public void OverwriteSByte(int writeIndex, sbyte var)
        {
            Overwrite((byte)var, writeIndex);
        }


        public void OverwriteByte(int writeIndex, byte var)
        {
            Overwrite((byte)var, writeIndex);
        }


        public void OverwriteChar(int writeIndex, char var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(char), writeIndex);
        }


        public void OverwriteInt16(int writeIndex, short var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(short), writeIndex);
        }


        public void OverwriteUInt16(int writeIndex, ushort var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(ushort), writeIndex);
        }


        public void OverwriteInt32(int writeIndex, int var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(int), writeIndex);
        }


        public void OverwriteUInt32(int writeIndex, uint var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(uint), writeIndex);
        }


        public void OverwriteInt64(int writeIndex, long var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(long), writeIndex);
        }


        public void OverwriteUInt64(int writeIndex, ulong var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(ulong), writeIndex);
        }


        public void OverwriteDouble(int writeIndex, double var)
        {
            Overwrite(BitConverter.GetBytes(var), 0, sizeof(double), writeIndex);
        }
    }
}
