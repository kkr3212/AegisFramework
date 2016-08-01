using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Aegis;
using System.Management;



namespace Aegis.IO
{
    public class SerialPort : IDisposable
    {
        public event IOEventHandler EventClose, EventRead, EventWrite;


        private System.IO.Ports.SerialPort _serialPort;
        private Thread _receiveThread;
        private ManagementEventWatcher _watcherPortOpen, _watcherPortClose;

        public string PortName { get; set; }
        public int BaudRate { get; set; } = 9600;
        public int DataBit { get; set; } = 8;
        public System.IO.Ports.Parity Parity { get; set; } = System.IO.Ports.Parity.None;
        public System.IO.Ports.StopBits StopBits { get; set; } = System.IO.Ports.StopBits.One;
        public System.IO.Ports.Handshake Handshake { get; set; } = System.IO.Ports.Handshake.None;
        public int ReadTimeout { get; set; } = System.IO.Ports.SerialPort.InfiniteTimeout;
        public int WriteTimeout { get; set; } = System.IO.Ports.SerialPort.InfiniteTimeout;





        public SerialPort()
        {
        }


        public void Dispose()
        {
            _serialPort?.Dispose();

            if (_watcherPortOpen != null)
            {
                _watcherPortOpen.Stop();
                _watcherPortOpen.Dispose();
                _watcherPortOpen = null;
            }

            if (_watcherPortClose != null)
            {
                _watcherPortClose.Stop();
                _watcherPortClose.Dispose();
                _watcherPortClose = null;
            }
        }


        public bool Open()
        {
            if (_serialPort != null && _serialPort.IsOpen == true)
                return false;


            bool ret = true;

            try
            {
                _serialPort = new System.IO.Ports.SerialPort();
                _serialPort.PortName = PortName;
                _serialPort.BaudRate = BaudRate;
                _serialPort.DataBits = DataBit;
                _serialPort.Parity = Parity;
                _serialPort.StopBits = StopBits;
                _serialPort.ReadTimeout = ReadTimeout;
                _serialPort.WriteTimeout = WriteTimeout;
                _serialPort.Open();

                _receiveThread = new Thread(ReceiveThread);
                _receiveThread.Start();

                Logger.Write(LogType.Info, LogLevel.Core, "{0} port opened.", PortName);
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, LogLevel.Core, e.Message);
                ret = false;
            }



            {
                WqlEventQuery query;

                if (_watcherPortOpen == null)
                {
                    query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
                    _watcherPortOpen = new ManagementEventWatcher(query);
                    _watcherPortOpen.EventArrived += (sender, e) =>
                    {
                        if (IsAvailablePort() == true)
                            Open();
                    };
                    _watcherPortOpen.Start();
                }


                if (_watcherPortClose == null)
                {
                    query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
                    _watcherPortClose = new ManagementEventWatcher(query);
                    _watcherPortClose.EventArrived += (sender, e) =>
                    {
                        Close(AegisResult.ClosedByRemote);
                    };
                    _watcherPortClose.Start();
                }
            }

            return ret;
        }


        public void Close()
        {
            Close(AegisResult.Ok);
        }


        private void Close(int reason)
        {
            try
            {
                EventClose?.Invoke(new IOEventResult(this, IOEventType.Close, reason));
                _serialPort?.Close();
            }
            catch (Exception)
            {
            }


            _serialPort = null;
            _receiveThread = null;
            Logger.Write(LogType.Info, LogLevel.Core, "SerialPort({0}) closed.", PortName);
        }


        private bool IsAvailablePort()
        {
            return System.IO.Ports.SerialPort.GetPortNames()
                    .Where(v => v == PortName).Count() != 0;
        }


        private void ReceiveThread()
        {
            byte[] buffer = new byte[BaudRate * 2];


            while (_receiveThread != null)
            {
                try
                {
                    int readBytes = _serialPort.Read(buffer, 0, BaudRate);
                    if (readBytes == 0)
                    {
                        _serialPort.Close();
                        _serialPort = null;
                        _receiveThread = null;

                        EventClose?.Invoke(new IOEventResult(this, IOEventType.Close, AegisResult.ClosedByRemote));
                        break;
                    }

                    EventRead?.Invoke(new IOEventResult(this, IOEventType.Read, buffer, 0, readBytes, AegisResult.Ok));
                }
                catch (System.IO.IOException)
                {
                    //  Close 호출로 인한 예외
                    _receiveThread = null;
                    break;
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, LogLevel.Core, e.Message);
                }
            }
        }


        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort?.Write(buffer, offset, count);
            EventWrite?.Invoke(new IOEventResult(this, IOEventType.Write, AegisResult.Ok));
        }
    }
}
