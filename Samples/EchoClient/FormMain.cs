using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace EchoClient
{
    public partial class FormMain : Form
    {
        public static Int32 BufferSize { get; set; }





        public FormMain()
        {
            InitializeComponent();

            _btnStart.Enabled = true;
            _btnStop.Enabled = false;
        }


        private void OnClick_Start(object sender, EventArgs e)
        {
            Int32 count, bufferSize;
            if (Int32.TryParse(_tbClientCount.Text, out count) == false ||
                count < 1 || count > 50)
            {
                MessageBox.Show("클라이언트 개수는 1 - 50 사이의 숫자로 입력해야 합니다.");
                return;
            }
            if (Int32.TryParse(_tbBufferSize.Text, out bufferSize) == false ||
                count < 1)
            {
                MessageBox.Show("버퍼 크기는 1 이상의 숫자로 입력해야 합니다.");
                return;
            }


            _btnStart.Enabled = false;
            _btnStop.Enabled = true;
            _tbClientCount.Enabled = false;

            _tbLog.Text = "";
            BufferSize = bufferSize;
            Logic.ClientMain.Start(count, _tbLog);
        }


        private void OnClick_Stop(object sender, EventArgs e)
        {
            _btnStart.Enabled = true;
            _btnStop.Enabled = false;
            _tbClientCount.Enabled = true;

            Logic.ClientMain.Stop();
        }


        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Logic.ClientMain.Stop();
        }
    }
}
