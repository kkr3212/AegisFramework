using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aegis;



namespace EchoServer.Logic
{
    public class LogTextFile : ILogMedia
    {
        private StreamWriter file;


        public LogTextFile(String prefix)
        {
            String filename;


            if (Directory.Exists(".\\log") == false)
                Directory.CreateDirectory(".\\log");

            filename = String.Format(".\\log\\{0}_{1}_{2:D2}{3:D2}_{4:D2}{5:D2}.log"
                                        , prefix
                                        , DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day
                                        , DateTime.Now.Hour, DateTime.Now.Minute);
            file = new StreamWriter(filename);
        }


        public void Write(LogType type, Int32 lv, String log)
        {
            String text;


            text = String.Format("[{0}/{1} {2}:{3}:{4} {5}] {6}"
                , DateTime.Now.Month, DateTime.Now.Day
                , DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second
                , type, log);

            file.WriteLine(text);
            file.Flush();
        }


        public void Release()
        {
            file.Close();
        }
    }





    public class LogOutputWindow : ILogMedia
    {
        public LogOutputWindow()
        {
        }


        public void Write(LogType type, Int32 lv, String log)
        {
            String text;


            text = String.Format("[{0}/{1} {2}:{3}:{4} {5}] {6}"
                , DateTime.Now.Month, DateTime.Now.Day
                , DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second
                , type, log);

            System.Diagnostics.Debug.WriteLine(text);
        }


        public void Release()
        {
        }
    }





    public class LogListBox : ILogMedia
    {
        private ListBox _control;



        public LogListBox(ListBox ctrl)
        {
            _control = ctrl;
        }


        public void Write(LogType type, Int32 lv, String log)
        {
            if (_control.InvokeRequired)
                _control.BeginInvoke((MethodInvoker)delegate { Write(type, lv, log); });

            else
            {
                String message = String.Format("[{0}, {1}] {2}", type, lv, log);


                _control.Items.Insert(_control.Items.Count, message);
                _control.SelectedIndex = _control.Items.Count - 1;
                _control.SelectedIndex = -1;
            }
        }


        public void Release()
        {
        }
    }





    public class LogTextBox : ILogMedia
    {
        private TextBox _control;



        public LogTextBox(TextBox ctrl)
        {
            _control = ctrl;
        }


        public void Write(LogType type, Int32 lv, String log)
        {
            if (_control.InvokeRequired)
                _control.BeginInvoke((MethodInvoker)delegate { Write(type, lv, log); });

            else
            {
                String message = String.Format("[{0}, {1}] {2}\r\n", type, lv, log);


                _control.Text += message;
                _control.SelectionStart = _control.TextLength;
                _control.ScrollToCaret();
            }
        }


        public void Release()
        {
        }
    }
}
