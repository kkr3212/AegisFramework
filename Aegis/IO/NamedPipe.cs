using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using Aegis.Threading;



namespace Aegis.IO
{
    public enum NamedPipeType
    {
        Server,
        Client
    }





    public class NamedPipe
    {
        public string PipeName { get; private set; }
        public NamedPipeType PipeType { get; private set; }
        public PipeStream PipeStream { get; private set; }
        public event IOEventHandler EventAccept, EventClose, EventRead, EventWrite;


        private byte[] _receivedBuffer = new byte[1024 * 1024];





        public NamedPipe(NamedPipeType type, string pipeName)
        {
            PipeType = type;
            PipeName = pipeName;
            Reset();
        }


        private void Reset()
        {
            PipeStream?.Close();


            if (PipeType == NamedPipeType.Server)
            {
                PipeStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            }
            if (PipeType == NamedPipeType.Client)
            {
                PipeStream = new NamedPipeClientStream(
                    ".", PipeName, PipeDirection.InOut, PipeOptions.None,
                    System.Security.Principal.TokenImpersonationLevel.Impersonation);
            }
        }


        public void Close()
        {
            EventClose?.Invoke(new IOEventResult(this, IOEventType.Close, AegisResult.Ok));

            AegisTask.SafeAction(() =>
            {
                PipeStream?.Close();
                PipeStream = null;
            });
        }


        public void Listen()
        {
            Reset();


            var stream = PipeStream as NamedPipeServerStream;
            if (stream == null)
                return;


            stream.BeginWaitForConnection((asyncResult) =>
            {
                AegisTask.SafeAction(() =>
                {
                    stream.EndWaitForConnection(asyncResult);

                    EventAccept?.Invoke(new IOEventResult(this, IOEventType.Accept, AegisResult.Ok));
                    WaitForReceive();
                });
            }, null);
        }


        public bool Connect(int timeout)
        {
            Reset();


            var stream = PipeStream as NamedPipeClientStream;
            if (stream == null)
                return false;


            try
            {
                if (timeout != 0)
                    stream.Connect(timeout);
                else
                    stream.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            WaitForReceive();

            return true;
        }


        private void WaitForReceive()
        {
            PipeStream.BeginRead(_receivedBuffer, 0, _receivedBuffer.Length, OnRead, null);
        }


        private void OnRead(IAsyncResult ar)
        {
            int transBytes = PipeStream.EndRead(ar);
            if (transBytes == 0)
            {
                PipeStream.Close();
                PipeStream = null;

                EventClose?.Invoke(new IOEventResult(this, IOEventType.Close, AegisResult.ClosedByRemote));
                return;
            }


            EventRead?.Invoke(new IOEventResult(this, IOEventType.Read, _receivedBuffer, 0, transBytes, AegisResult.Ok));
            WaitForReceive();
        }


        public void Write(byte[] buffer, int offset, int count)
        {
            if (PipeStream == null || PipeStream.IsConnected == false)
                return;

            PipeStream.BeginWrite(buffer, offset, count, (asyncResult) =>
            {
                PipeStream.EndWrite(asyncResult);

                EventWrite?.Invoke(new IOEventResult(this, IOEventType.Write, AegisResult.Ok));
            }, null);
        }
    }
}
