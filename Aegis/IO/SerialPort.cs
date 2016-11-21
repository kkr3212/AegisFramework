using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Aegis.IO
{
    public class SerialPort
    {
        public event IOEventHandler EventClose, EventRead, EventWrite;


        private Thread _receiveThread;

        public System.IO.Ports.SerialPort Handle { get; private set; }
        public string PortName { get; set; }
        public bool IsOpen { get { return Handle?.IsOpen ?? false; } }
        public int BaudRate { get; set; } = 9600;
        public int DataBit { get; set; } = 8;
        public System.IO.Ports.Parity Parity { get; set; } = System.IO.Ports.Parity.None;
        public System.IO.Ports.StopBits StopBits { get; set; } = System.IO.Ports.StopBits.One;
        public System.IO.Ports.Handshake Handshake { get; set; } = System.IO.Ports.Handshake.None;
        public int ReadTimeout { get; set; } = System.IO.Ports.SerialPort.InfiniteTimeout;
        public int WriteTimeout { get; set; } = System.IO.Ports.SerialPort.InfiniteTimeout;
        public Action<Exception> ErrorHandler { get; set; }





        public SerialPort()
        {
        }


        public void Open()
        {
            lock (this)
            {
                if (Handle != null && Handle.IsOpen == true)
                    throw new AegisException(AegisResult.AlreadyInitialized, "{0} port already opened.", Handle.PortName);


                try
                {
                    Handle = new System.IO.Ports.SerialPort();
                    Handle.PortName = PortName;
                    Handle.BaudRate = BaudRate;
                    Handle.DataBits = DataBit;
                    Handle.Parity = Parity;
                    Handle.StopBits = StopBits;
                    Handle.ReadTimeout = ReadTimeout;
                    Handle.WriteTimeout = WriteTimeout;
                    Handle.Open();
                }
                catch (Exception e)
                {
                    Handle.Dispose();
                    Handle = null;
                    throw e;
                }

                _receiveThread = new Thread(ReceiveThread);
                _receiveThread.Start();
            }
        }


        public void Close()
        {
            lock (this)
            {
                try
                {
                    EventClose?.Invoke(new IOEventResult(this, IOEventType.Close, AegisResult.Ok));
                    Handle?.Close();
                    Handle?.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Err(e.Message);
                }


                Handle = null;
                _receiveThread = null;
            }
        }


        private void ReceiveThread()
        {
            byte[] buffer = new byte[BaudRate * 2];


            while (_receiveThread != null && Handle != null)
            {
                try
                {
                    int readBytes = Handle.Read(buffer, 0, BaudRate);
                    if (readBytes == 0)
                    {
                        lock (this)
                        {
                            Handle?.Close();
                            Handle = null;
                            _receiveThread = null;
                        }

                        EventClose?.Invoke(new IOEventResult(this, IOEventType.Close, AegisResult.ClosedByRemote));
                        break;
                    }

                    EventRead?.Invoke(new IOEventResult(this, IOEventType.Read, buffer, 0, readBytes, AegisResult.Ok));
                }
                catch (System.IO.IOException)
                {
                    lock (this)
                    {
                        Handle?.Close();
                        Handle = null;
                        _receiveThread = null;

                        EventClose?.Invoke(new IOEventResult(this, IOEventType.Close, AegisResult.ClosedByRemote));
                    }
                    break;
                }
                catch (Exception e)
                {
                    ErrorHandler?.Invoke(e);
                    Logger.Err(LogMask.Aegis, e.Message);
                }
            }
        }


        public bool Write(byte[] buffer, int offset, int count)
        {
            try
            {
                lock (this)
                {
                    Handle?.Write(buffer, offset, count);
                    EventWrite?.Invoke(new IOEventResult(this, IOEventType.Write, AegisResult.Ok));
                }

                return true;
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(e);
                Logger.Err(LogMask.Aegis, e.Message);

                return false;
            }
        }
    }
}
