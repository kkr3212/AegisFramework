using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            Logic.ServerMain.Instance.StartServer(_tbLog);
        }


        private void OnClick_Stop(object sender, EventArgs e)
        {
            _btnStart.Enabled = true;
            _btnStop.Enabled = false;

            Logic.ServerMain.Instance.StopServer();
        }


        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Logic.ServerMain.Instance.StopServer();
        }
    }
}
