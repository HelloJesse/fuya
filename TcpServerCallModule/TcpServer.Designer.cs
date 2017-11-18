namespace TcpServerCallModule
{
    partial class TcpServer
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnReceive = new System.Windows.Forms.Button();
            this.rtbResult = new System.Windows.Forms.RichTextBox();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.lblIp = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDK = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(129, 29);
            this.btnStart.TabIndex = 14;
            this.btnStart.Tag = "begin";
            this.btnStart.Text = "开启Tcp服务";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(12, 47);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(129, 29);
            this.btnSend.TabIndex = 16;
            this.btnSend.Tag = "begin";
            this.btnSend.Text = "讲话发射";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(172, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(129, 29);
            this.btnStop.TabIndex = 17;
            this.btnStop.Tag = "begin";
            this.btnStop.Text = "关闭Tcp服务";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnReceive
            // 
            this.btnReceive.Location = new System.Drawing.Point(172, 47);
            this.btnReceive.Name = "btnReceive";
            this.btnReceive.Size = new System.Drawing.Size(129, 29);
            this.btnReceive.TabIndex = 18;
            this.btnReceive.Tag = "begin";
            this.btnReceive.Text = "接收接听";
            this.btnReceive.UseVisualStyleBackColor = true;
            this.btnReceive.Click += new System.EventHandler(this.btnReceive_Click);
            // 
            // rtbResult
            // 
            this.rtbResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbResult.Location = new System.Drawing.Point(10, 82);
            this.rtbResult.Name = "rtbResult";
            this.rtbResult.Size = new System.Drawing.Size(476, 194);
            this.rtbResult.TabIndex = 19;
            this.rtbResult.Text = "";
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(333, 52);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(90, 21);
            this.txtIp.TabIndex = 20;
            this.txtIp.Text = "116.62.103.98";
            // 
            // lblIp
            // 
            this.lblIp.AutoSize = true;
            this.lblIp.Location = new System.Drawing.Point(331, 29);
            this.lblIp.Name = "lblIp";
            this.lblIp.Size = new System.Drawing.Size(89, 12);
            this.lblIp.TabIndex = 21;
            this.lblIp.Text = "监听的IP地址：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(431, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 22;
            this.label1.Text = "端口：";
            // 
            // txtDK
            // 
            this.txtDK.Location = new System.Drawing.Point(432, 52);
            this.txtDK.Name = "txtDK";
            this.txtDK.Size = new System.Drawing.Size(40, 21);
            this.txtDK.TabIndex = 23;
            this.txtDK.Text = "8005";
            // 
            // TcpServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 288);
            this.Controls.Add(this.txtDK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblIp);
            this.Controls.Add(this.txtIp);
            this.Controls.Add(this.rtbResult);
            this.Controls.Add(this.btnReceive);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "TcpServer";
            this.Text = "TcpServer服务器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TcpServer_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnReceive;
        private System.Windows.Forms.RichTextBox rtbResult;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Label lblIp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDK;
    }
}

