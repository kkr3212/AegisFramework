namespace EchoServer
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._tbLog = new System.Windows.Forms.TextBox();
            this._btnStart = new System.Windows.Forms.Button();
            this._btnStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this._lbReceiveCount = new System.Windows.Forms.Label();
            this._lbReceiveBytes = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._lbActiveSession = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this._lbTaskCount = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _tbLog
            // 
            this._tbLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._tbLog.Location = new System.Drawing.Point(14, 133);
            this._tbLog.MaxLength = 1048576;
            this._tbLog.Multiline = true;
            this._tbLog.Name = "_tbLog";
            this._tbLog.ReadOnly = true;
            this._tbLog.Size = new System.Drawing.Size(503, 188);
            this._tbLog.TabIndex = 65;
            this._tbLog.TabStop = false;
            // 
            // _btnStart
            // 
            this._btnStart.Location = new System.Drawing.Point(12, 12);
            this._btnStart.Name = "_btnStart";
            this._btnStart.Size = new System.Drawing.Size(115, 52);
            this._btnStart.TabIndex = 63;
            this._btnStart.Text = "Start";
            this._btnStart.UseVisualStyleBackColor = true;
            this._btnStart.Click += new System.EventHandler(this.OnClick_Start);
            // 
            // _btnStop
            // 
            this._btnStop.Location = new System.Drawing.Point(133, 12);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(115, 52);
            this._btnStop.TabIndex = 64;
            this._btnStop.Text = "Stop";
            this._btnStop.UseVisualStyleBackColor = true;
            this._btnStop.Click += new System.EventHandler(this.OnClick_Stop);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 21);
            this.label1.TabIndex = 66;
            this.label1.Text = "Receive Count";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lbReceiveCount
            // 
            this._lbReceiveCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._lbReceiveCount.Location = new System.Drawing.Point(133, 109);
            this._lbReceiveCount.Name = "_lbReceiveCount";
            this._lbReceiveCount.Size = new System.Drawing.Size(123, 21);
            this._lbReceiveCount.TabIndex = 67;
            this._lbReceiveCount.Text = "0";
            this._lbReceiveCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lbReceiveBytes
            // 
            this._lbReceiveBytes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._lbReceiveBytes.Location = new System.Drawing.Point(383, 109);
            this._lbReceiveBytes.Name = "_lbReceiveBytes";
            this._lbReceiveBytes.Size = new System.Drawing.Size(123, 21);
            this._lbReceiveBytes.TabIndex = 69;
            this._lbReceiveBytes.Text = "0";
            this._lbReceiveBytes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(262, 109);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(115, 21);
            this.label4.TabIndex = 68;
            this.label4.Text = "Receive Bytes";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lbActiveSession
            // 
            this._lbActiveSession.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._lbActiveSession.Location = new System.Drawing.Point(133, 83);
            this._lbActiveSession.Name = "_lbActiveSession";
            this._lbActiveSession.Size = new System.Drawing.Size(123, 21);
            this._lbActiveSession.TabIndex = 71;
            this._lbActiveSession.Text = "0";
            this._lbActiveSession.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(12, 83);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(115, 21);
            this.label6.TabIndex = 70;
            this.label6.Text = "Active Session";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lbTaskCount
            // 
            this._lbTaskCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._lbTaskCount.Location = new System.Drawing.Point(383, 83);
            this._lbTaskCount.Name = "_lbTaskCount";
            this._lbTaskCount.Size = new System.Drawing.Size(123, 21);
            this._lbTaskCount.TabIndex = 73;
            this._lbTaskCount.Text = "0";
            this._lbTaskCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(262, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 21);
            this.label3.TabIndex = 72;
            this.label3.Text = "Task Count";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 344);
            this.Controls.Add(this._lbTaskCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._lbActiveSession);
            this.Controls.Add(this.label6);
            this.Controls.Add(this._lbReceiveBytes);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._lbReceiveCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._tbLog);
            this.Controls.Add(this._btnStart);
            this.Controls.Add(this._btnStop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AegisNetwork EchoServer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _tbLog;
        private System.Windows.Forms.Button _btnStart;
        private System.Windows.Forms.Button _btnStop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label _lbReceiveCount;
        private System.Windows.Forms.Label _lbReceiveBytes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label _lbActiveSession;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label _lbTaskCount;
        private System.Windows.Forms.Label label3;
    }
}

