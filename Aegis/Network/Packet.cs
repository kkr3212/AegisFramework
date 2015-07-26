using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Aegis.Network
{
    public class Packet : StreamBuffer
    {
        public UInt16 Size
        {
            get { return GetUInt16(0); }
            set { OverwriteUInt16(0, value); }
        }
        public UInt16 PID
        {
            get { return GetUInt16(2); }
            set { OverwriteUInt16(2, value); }
        }





        public Packet()
        {
            PutUInt16(0);       //  Size
            PutUInt16(0);       //  PID
        }


        public Packet(UInt16 pid)
        {
            PutUInt16(0);       //  Size
            PutUInt16(pid);     //  PID
        }


        public Packet(UInt16 pid, UInt16 capacity)
        {
            Capacity(capacity);
            PutUInt16(0);       //  Size
            PutUInt16(pid);     //  PID
        }


        public Packet(StreamBuffer src)
        {
            Write(src.Buffer, 0, src.WrittenBytes);
        }


        public Packet(byte[] source, Int32 startIndex, Int32 size)
        {
            Write(source, startIndex, size);
        }


        public override void Clear()
        {
            UInt16 pid = PID;


            base.Clear();

            PutUInt16(0);       //  Size
            PutUInt16(pid);     //  PID
        }


        protected override void OnSizeChanged()
        {
            Size = (UInt16)WrittenBytes;
        }


        public virtual void SkipHeader()
        {
            ResetReadIndex();

            GetUInt16();        //  Size
            GetUInt16();        //  PID
        }
    }
}
