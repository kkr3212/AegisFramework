using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Aegis.Calculate;



namespace Aegis.IO
{
    public class SerialPortWatcher
    {
        private List<Tuple<SerialPort, object>> _pluggedPorts, _unpluggedPorts;
        public event Action<SerialPort, object> Plugged, Unplugged;
        private IntervalTimer _timer;





        public SerialPortWatcher()
        {
            _pluggedPorts = new List<Tuple<SerialPort, object>>();
            _unpluggedPorts = new List<Tuple<SerialPort, object>>();

            _timer = new IntervalTimer(500, () => { Check(); });
            _timer.Start();
        }


        private void Check()
        {
            //  사용 가능한 포트목록
            var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
            List<string> currentPortNames = new List<string>();


            foreach (ManagementObject queryObj in searcher.Get())
            {
                object obj = queryObj["Caption"];
                if (obj != null && obj.ToString().Contains("(COM"))
                {
                    string name = obj.ToString().Substring(obj.ToString().LastIndexOf("(COM"))
                                                .Replace("(", string.Empty)
                                                .Replace(")", string.Empty);

                    currentPortNames.Add(name);
                }
            }



            lock (this)
            {
                //  새로운 포트가 인식되었는지 확인
                var pluggedPort = (from monitorPort in _unpluggedPorts
                                   join availablePortName in currentPortNames
                                   on monitorPort.Item1.PortName equals availablePortName
                                   select monitorPort).ToList();

                foreach (var port in pluggedPort)
                {
                    _unpluggedPorts.Remove(port);
                    _pluggedPorts.Add(port);
                    Plugged?.Invoke(port.Item1, port.Item2);
                }



                //  기존포트가 차단되었는지 확인
                var unpluggedPortNames = _pluggedPorts.Select(v => v.Item1.PortName)
                                                      .Except(currentPortNames);
                var unpluggedPorts = (from monitorPort in _pluggedPorts
                                      join unpluggedPortName in unpluggedPortNames
                                      on monitorPort.Item1.PortName equals unpluggedPortName
                                      select monitorPort).ToList();

                foreach (var port in unpluggedPorts)
                {
                    _pluggedPorts.Remove(port);
                    _unpluggedPorts.Add(port);
                    Unplugged?.Invoke(port.Item1, port.Item2);
                }
            }
        }


        public void Close()
        {
            _timer.Stop();


            lock (this)
            {
                _pluggedPorts.Clear();
                _unpluggedPorts.Clear();
            }
        }


        public void Add(SerialPort port, object tag)
        {
            if (port == null)
                return;


            lock (this)
            {
                if (port.IsOpen)
                    _pluggedPorts.Add(new Tuple<SerialPort, object>(port, tag));
                else
                    _unpluggedPorts.Add(new Tuple<SerialPort, object>(port, tag));
            }
        }
    }
}
