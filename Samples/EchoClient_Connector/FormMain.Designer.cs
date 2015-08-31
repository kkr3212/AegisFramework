namespace EchoClient
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
            this.SuspendLayout();
            // 
            // _tbLog
            // 
            this._tbLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._tbLog.Location = new System.Drawing.Point(10, 76);
            this._tbLog.MaxLength = 1048576;
            this._tbLog.Multiline = true;
            this._tbLog.Name = "_tbLog";
            this._tbLog.ReadOnly = true;
            this._tbLog.Size = new System.Drawing.Size(431, 204);
            this._tbLog.TabIndex = 76;
            this._tbLog.TabStop = false;
            // 
            // _btnStart
            // 
            this._btnStart.Location = new System.Drawing.Point(10, 13);
            this._btnStart.Name = "_btnStart";
            this._btnStart.Size = new System.Drawing.Size(99, 56);
            this._btnStart.TabIndex = 74;
            this._btnStart.Text = "Start";
            this._btnStart.UseVisualStyleBackColor = true;
            this._btnStart.Click += new System.EventHandler(this.OnClick_Start);
            // 
            // _btnStop
            // 
            this._btnStop.Location = new System.Drawing.Point(114, 13);
            this._btnStop.Name = "_btnStop";
            this._btnStop.Size = new System.Drawing.Size(99, 56);
            this._btnStop.TabIndex = 75;
            this._btnStop.Text = "Stop";
            this._btnStop.UseVisualStyleBackColor = true;
            this._btnStop.Click += new System.EventHandler(this.OnClick_Stop);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 301);
            this.Controls.Add(this._tbLog);
            this.Controls.Add(this._btnStart);
            this.Controls.Add(this._btnStop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EchoClient (AegisClient)";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _tbLog;
        private System.Windows.Forms.Button _btnStart;
        private System.Windows.Forms.Button _btnStop;
    }
}

