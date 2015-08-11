using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aegis.Client;



namespace EchoClient
{
    public partial class FormMain : Form
    {
        private static FormMain _instance;
        private Session _session;




        public FormMain()
        {
            InitializeComponent();

            _instance = this;
            _session = new Session();
        }


        public static void Log(String log, params object[] args)
        {
            if (_instance._tbLog.InvokeRequired)
                _instance._tbLog.BeginInvoke((MethodInvoker)delegate { Log(log, args); });

            else
            {
                _instance._tbLog.Text += String.Format(log, args) + "\r\n";
                _instance._tbLog.SelectionStart = _instance._tbLog.TextLength;
                _instance._tbLog.ScrollToCaret();
            }
        }


        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            _session.Release();
        }


        private void OnClick_Start(object sender, EventArgs e)
        {
            _session.Connect();
        }


        private void OnClick_Stop(object sender, EventArgs e)
        {
            _session.Close();
        }
    }
}
