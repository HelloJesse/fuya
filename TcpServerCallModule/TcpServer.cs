using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TcpServerCallModule
{
    public partial class TcpServer : Form
    {
        private IPAddress localAddress;// IP地址
        private TcpListener tcpListener;//监听套接字
        private TcpClient tcpClient;//服务端与客户端建立连接 

        private NetworkStream newworkStream;//利用NetworkStream对象与远程主机发送数据或接收数据\ 
        private BinaryReader binaryReader;//读取 BinaryReader与BinaryWriter类在命名空间using System.IO下 
        private BinaryWriter binaryWrite;//写入 

        private int port = 8005;//端口 
        private string _ipaddress = "116.62.103.98";//接口命令地址的IP
        private Thread _thread;

        public TcpServer()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 开启服务器的按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            this.rtbResult.Text = "";
            this.rtbResult.AppendText("==========开启TCP服务器==========\r\n");
            if (!string.IsNullOrEmpty(txtIp.Text))
            {
                _ipaddress = txtIp.Text; 
            }
            if (!string.IsNullOrEmpty(txtDK.Text))
            {
                port = Convert.ToInt32(txtDK.Text); 
            }

            this.rtbResult.AppendText(string.Format("\r\n[{0}:{1}]监听的IP地址...\r\n", _ipaddress, port));

            try
            {
                IPAddress[] listenerIp = Dns.GetHostAddresses(_ipaddress);//返回主机指定的IP地址 
                localAddress = listenerIp[0]; //初始化IP地址为本地地址 
                tcpListener = new TcpListener(localAddress, port);
                //tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();//开始监听 

                _thread = new Thread(onListenClientConnect);//启动一个线程接收请求 
                _thread.Start();//启动线程 

                this.btnStart.Enabled = false;//按钮不可用
            }
            catch (Exception ex)
            {
                this.rtbResult.AppendText(string.Format("\r\nTCP服务开启失败:[{0}]请确保本地IP是否正确...\r\n", ex.Message));
            }
        }

        /// <summary>
        /// 监听连接
        /// </summary>
        private void onListenClientConnect()
        {
            while (true)
            {
                try
                {
                    tcpClient = tcpListener.AcceptTcpClient();//从端口接收一个连接，并赋予它TcpClient对象 
                    if (tcpClient != null)
                    {
                        newworkStream = tcpClient.GetStream();
                        binaryReader = new BinaryReader(newworkStream);
                        binaryWrite = new BinaryWriter(newworkStream);
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 关闭服务器的按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            this.rtbResult.Text = "";
            this.rtbResult.AppendText("==========关闭TCP服务器==========\r\n");

            this.btnStart.Enabled = true;

            //关闭
            CloseResource();
        }

        /// <summary>
        /// 关闭资源
        /// </summary>
        private void CloseResource()
        {
            if (null != binaryReader)
            {
                binaryReader.Close();
            }
            if (null != binaryWrite)
            {
                binaryWrite.Close();
            }
            if (null != tcpClient)
            {
                tcpClient.Close();
            }
            if (null != tcpListener)
            {
                tcpListener.Stop();
            }
            if (null != _thread)
            {
                _thread.Abort();
            }
        }

        /// <summary>
        /// 通话模块 讲话发射
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] mybyte = Nav_onCallModuleSend();

            //发送命令
            sendCommand(mybyte);
            this.rtbResult.AppendText(string.Format("\r\n[{0}]讲话发射命令发送成功...\r\n", DateTime.Now));
        }

        /// <summary>
        /// 通话模块 接听接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceive_Click(object sender, EventArgs e)
        {
            byte[] mybyte = Nav_onCallModuleReceive();

            //发送命令
            sendCommand(mybyte);
            this.rtbResult.AppendText(string.Format("\r\n[{0}]接听接收命令发送成功...\r\n", DateTime.Now));
        }

        /// <summary>
        /// 发送命令 
        /// </summary>
        private void sendCommand(byte[] mybyte)
        {
            MemoryStream memory = new MemoryStream();
            BinaryFormatter format = new BinaryFormatter();
            format.Serialize(memory, mybyte);
            byte[] bytes = memory.ToArray();
            binaryWrite.Write(bytes);
            binaryWrite.Flush();
        }

        /// <summary>
        /// 通话模块 讲话发射命令
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public byte[] Nav_onCallModuleSend()
        {
            byte[] mybtye = new byte[5];//目前是5个字节
            try
            {
                mybtye[0] = 0xBB;
                mybtye[1] = 0x01;
                mybtye[2] = 0x01;
                mybtye[3] = 0xBD;
                mybtye[4] = 0x0E;
            }
            catch (Exception)
            {
                return null;
            }
            return mybtye;
        }

        /// <summary>
        /// 通话模块 接听接收命令
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public byte[] Nav_onCallModuleReceive()
        {
            byte[] mybtye = new byte[5];//目前是5个字节
            try
            {
                mybtye[0] = 0xBB;
                mybtye[1] = 0x01;
                mybtye[2] = 0x02;
                mybtye[3] = 0xBE;
                mybtye[4] = 0x0E;
            }
            catch (Exception)
            {
                return null;
            }
            return mybtye;
        }

        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpServer_FormClosing(object sender, FormClosingEventArgs e)
        {

            DialogResult result = MessageBox.Show("你确定要关闭吗！", "提示信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {

                //关闭
                CloseResource();

                e.Cancel = false;  //点击OK
            }
            else
            {
                e.Cancel = true;  
            }
        }

    }
}
