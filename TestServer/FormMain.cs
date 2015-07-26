using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestServer.Logic;



namespace TestServer
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            _btnStart.Enabled = true;
            _btnStop.Enabled = false;
        }


        private void OnClick_Start(object sender, EventArgs e)
        {
            _btnStart.Enabled = false;
            _btnStop.Enabled = true;

            _tbLog.Text = "";

            ServerMain.Instance.StartServer(_tbLog);
            (new Thread(Run)).Start();
        }


        private void OnClick_Stop(object sender, EventArgs e)
        {
            _btnStart.Enabled = true;
            _btnStop.Enabled = false;

            ServerMain.Instance.StopServer();
        }


        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            ServerMain.Instance.StopServer();
        }


        private void Run()
        {
            while (_btnStart.Enabled == false)
            {
                if (InvokeRequired)
                    Invoke((MethodInvoker)delegate { UpdateStatistics(); });
                else
                    UpdateStatistics();


                Thread.Sleep(100);
            }
        }


        private void UpdateStatistics()
        {
            Int32 sessionCount = ServerMain.Instance.GetActiveSessionCount();
            Int32 receiveCount = ServerMain.Instance.GetReceiveCount();
            Int32 receiveBytes = ServerMain.Instance.GetReceiveBytes();


            _lbActiveSession.Text = String.Format("{0:N0}", sessionCount);
            _lbReceiveCount.Text = String.Format("{0:N0}", receiveCount);
            _lbReceiveBytes.Text = String.Format("{0:N0}", receiveBytes);
            _lbTaskCount.Text = Aegis.Threading.AegisTask.TaskCount.ToString();
        }
    }
}
