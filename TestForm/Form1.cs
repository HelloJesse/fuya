using Nordasoft.Data.Sql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using VLP.NAV;

namespace TestForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        DataBase db = new DataBase("server=116.62.103.98,1438;database=uav_foyoung;user id=foyoung;password=vlp@foyoung;");
        string msg = "";

        public static void SendData(byte[] bytes)
        {
            //设置服务IP，设置TCP端口号
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("192.168.10.13"), 8001);

            //定义网络类型，数据连接类型和网络协议UDP
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.SendTo(bytes, bytes.Length, SocketFlags.None, ip);
        }

        private void btn_Click(object sender, EventArgs e)
        {
            // 用新的方法测试一下
            NavWebaction actions = new NavWebaction(db, 1, 44);
            int nu = (int)numericUpDown4.Value;
            actions.Nav_Control_Uav(nu, nu > 0 ? ControlType.升高 : ControlType.下降, 0, out msg); // 0 为目标距离 这里暂且这样给
            if (string.IsNullOrEmpty(msg))
            {
                richTextBox1.Text = richTextBox1.Text + "\r\n" + "高度变化成功.";
            }
            else
            {
                richTextBox1.Text = richTextBox1.Text + "\r\n" + "高度变化失败.";
            }
            return;


            /*
            NavSendMessage sendmsg = new NavSendMessage(1, db);
            
            byte[] ss = sendmsg.Nav_ChangeHigh(nu, out msg);
            if (string.IsNullOrEmpty(msg))
            {
                //发送
                SendData(ss);
                string s = "";
                string s1 = "";
                for (int i = 0; i < ss.Length; i++)
                {
                    s1 = Convert.ToString(ss[i],16);
                    if (s1.Length == 1)
                        s1 = "0" + s1;
                    s = s + s1.ToUpper()+ " ";
                }
                richTextBox1.Text = richTextBox1.Text + "\r\n" + s;
            }
            else
            {
                richTextBox1.Text = msg;
            }
            */
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = richTextBox1.Text = string.Empty;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AirlineModel obj = new AirlineModel();
            obj.Airlinepoint = new List<Airlinepoint>();
            obj.Airlinecode = (int)numericUpDown5.Value;
            obj.Aircycle = ckAircycle.Checked;
            try
            {
                //for循环的时候的，如果i设置类型为byte 但是最终循环到的值达到255以上，则直接造成内存泄漏，异常无法捕获
                for (int i = 1; i <= numericUpDown6.Value; i++)
                {
                    Airlinepoint pointobj = new Airlinepoint();
                    pointobj.Airpointcode = i;
                    pointobj.Lineswitch = 0;
                    pointobj.Heightcontrol = 0;
                    pointobj.Longitude = 113101990 + i;
                    pointobj.Latitude = 29489470 + i;
                    pointobj.height = 50 * i;
                    pointobj.NextAreaSpeed = (byte)(21 * i);
                    pointobj.Isok_vedio = true;// Convert.ToBoolean(i % 2);
                    pointobj.Isok_pictrue = true;// Convert.ToBoolean(i % 2);
                    pointobj.Isaction_vedio = true;// Convert.ToBoolean(i % 2);
                    pointobj.Isaction_pictrue = true;//Convert.ToBoolean(i % 2);
                    pointobj.CycleNum = (byte)(12 * i);
                    pointobj.PicDist = 10 * i;
                    obj.Airlinepoint.Add(pointobj);
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message);
                return;
            }
            NavSendMessage sendmsg = new NavSendMessage(1, db);
            List<byte[]> ssD = sendmsg.Nav_WaypointSet(obj, out msg);
            if (string.IsNullOrEmpty(msg))
            {

                foreach (byte[] ss in ssD)
                {
                    SendData(ss);
                    string s = "";
                    string s1 = "";
                    for (int i = 0; i < ss.Length; i++)
                    {
                        s1 = Convert.ToString(ss[i], 16);
                        if (s1.Length == 1)
                            s1 = "0" + s1;
                        s = s + s1.ToUpper() + " ";
                    }

                    richTextBox1.Text = richTextBox1.Text + "\r\n" + s;
                }
            }
            else
            {
                richTextBox1.Text = msg;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                ////Nav_Get_FlySignals
                //NavWebaction actions = new NavWebaction(db, 1, 44);
                //DataTable dt = actions.Nav_Get_FlySignals(false, out msg);
                //if (string.IsNullOrEmpty(msg))
                //{
                //    foreach (DataRow dr in dt.Rows)
                //    {
                //        foreach (DataColumn dc in dt.Columns)
                //        {
                //            richTextBox2.Text = richTextBox2.Text + dc.ColumnName + "：" + dr[dc.ColumnName].ToString() + "\r\n"; ;
                //        }
                //        richTextBox2.Text = richTextBox2.Text + "\r\n";
                //    }
                //}
                
                string getmsg = richTextBox1.Text;
                if (string.IsNullOrEmpty(getmsg) == false)
                {
                    string[] sd = getmsg.Split(' ');
                    byte[] mybyte = new byte[136];
                    for (int i = 0; i < 136; i++)
                    {
                        mybyte[i] = Convert.ToByte(sd[i], 16);
                    }

                    NavMessageObj obj = NavStateMessage.ParsingTelemetrySignals(mybyte, out msg);

                    NavStateMessage.SaveTelemetrySignals(db, mybyte, out msg);

                    if (string.IsNullOrEmpty(msg))
                    {
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "长度："+obj.FrameLength;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "目的地址：" + obj.TargetAddress;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "源地址：" + obj.SourceAddress;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "数据类型：" + obj.FrameDataType;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "当前俯仰角：" + obj.PitchingAngle;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "目标俯仰角：" + obj.TargetPitchingAngle;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "当前滚转角：" + obj.RollAngle;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "目标滚转角：" + obj.TargetRollAngle;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "当前机头方位：" + obj.NoseAzimuth;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "目标机头方位：" + obj.TargetNoseAzimuth;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "GPS航迹向：" + obj.GPSTrack;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "当前对地高度：" + obj.GroundHeight;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "目标对地高度：" + obj.TargetGroundHeight;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "当前地速：" + obj.GroundSpeed;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "当前空速：" + obj.AirSpeed;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "目标速度：" + obj.TargetSpeed;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "升降速率：" + obj.LiftingRate;



                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "报警状态-主电压低 0：正常 1：报警：" + obj.Alarm_MainV_Low;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "报警状态-电流低 0：正常 1：报警：" + obj.Alarm_ServoV_Low;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "报警状态-状态监控 1 0：正常 1：报警：" + obj.Alarm_Status1;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "报警状态-转速异常 0：正常 1：报警：" + obj.Alarm_AbnormalSpeed;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "报警状态-空速异常 0：正常 1：报警：" + obj.Alarm_SpaceVelocityAnomaly;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "遥控解锁状态--0 解锁 1 未解锁：" + obj.ControlUnlocked;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "报警状态-GPS 定位精度低 0：正常 1：报警：" + obj.Alarm_GPSLoction_Low;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "报警状态-航姿传感器状态 0：正常 1：报警：" + obj.Alarm_PositionSensor;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "报警状态-高度报警 0：正常 1：报警：" + obj.Alarm_GrandHigh;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "开关--盘旋 0 无效 1 盘旋：" + obj.Switch_Circle;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "开关--归航 0 无效 1 归航： " + obj.Switch_Homing;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "当前盘旋圈数：" + obj.CurrentCircles;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "目标盘旋圈数：" + obj.TargetCircles;





                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "控制状态：" + obj.ControlMode;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "飞型器模态：" + obj.VehicleModal;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "系统状态 0正常 1校准 2保护：" + obj.SystemStatus;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "飞行阶段标志 0：巡航段 1：起飞段 2：降落段 3：地面等待：" + obj.FlightPhase;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "GPS_H：" + obj.GPS_H;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "GPS_M：" + obj.GPS_M;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "GPS_S：" + obj.GPS_S;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "当前航线 1~10：普通用户航线  0：返航航线 11:盘旋航线：" + obj.CurrentLine;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "目标航点： " + obj.CurrentTargetPoint;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "原点距离--m 米：" + obj.DistanceOrigin;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "目标距离--m 米：" + obj.DistanceTarget;
                        richTextBox2.Text = richTextBox2.Text + "\r\n" + "高度 单位：m 米 非对地：" + obj.Height;
                    }
                    else
                    {
                        richTextBox2.Text = msg;
                    }
                }
                 
            }
            catch(Exception ex) {
                richTextBox1.Text = ex.Message; 
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > 255)
            {
                MessageBox.Show("角度处理范围在0~255之间.");
            }
            string tag = ((System.Windows.Forms.Button)sender).Tag.ToString();
            NavWebaction actions = new NavWebaction(db, 1, 44);

            if (tag == "上")
            {
                actions.Nav_Control_Uav((byte)numericUpDown1.Value, ControlType.升高, 0, out msg);
            }
            else if (tag == "下")
            {
                actions.Nav_Control_Uav((byte)numericUpDown1.Value, ControlType.下降, 0, out msg);
            }
            else if (tag == "左")
            {
                actions.Nav_Control_Uav((byte)numericUpDown1.Value, ControlType.左飞, 0, out msg);
            }
            else if (tag == "右")
            {
                actions.Nav_Control_Uav((byte)numericUpDown1.Value, ControlType.右飞, 0, out msg);
            }

            if (string.IsNullOrEmpty(msg))
            {
                richTextBox1.Text = richTextBox1.Text + "\r\n" + "高度变化成功.";
            }
            else
            {
                richTextBox1.Text = richTextBox1.Text + "\r\n" + "高度变化失败." + msg;
            }
            return;
            /*
            //numericUpDown1.Value
            if (numericUpDown1.Value > 255)
            {
                MessageBox.Show("角度处理范围在0~255之间.");
            }
            string tag = ((System.Windows.Forms.Button)sender).Tag.ToString();
            NavSendMessage sendmsg = new NavSendMessage(1, db);
            byte[] ss = null;
            if (tag == "上")
            {
                ss = sendmsg.Nav_ControlNav((byte)numericUpDown1.Value, ControlType.升高, out msg);
            }
            else if (tag == "下")
            {
                ss = sendmsg.Nav_ControlNav((byte)numericUpDown1.Value, ControlType.下降, out msg);
            }
            else if (tag == "左")
            {
                ss = sendmsg.Nav_ControlNav((byte)numericUpDown1.Value, ControlType.左飞, out msg);
            }
            else if (tag == "右")
            {
                ss = sendmsg.Nav_ControlNav((byte)numericUpDown1.Value, ControlType.右飞, out msg);
            }
          



           
            if (string.IsNullOrEmpty(msg))
            {
                SendData(ss);
                string s = "";
                string s1 = "";
                for (int i = 0; i < ss.Length; i++)
                {
                    s1 = Convert.ToString(ss[i], 16);
                    if (s1.Length == 1)
                        s1 = "0" + s1;
                    s = s + s1.ToUpper() + " ";
                }
                richTextBox1.Text = richTextBox1.Text + "\r\n" + s;
            }
            else
            {
                richTextBox1.Text = msg;
            }
            */
        }

        private void button8_Click(object sender, EventArgs e)
        {
            NavSendMessage sendmsg = new NavSendMessage(1, db);
             
            byte[] ss = sendmsg.Nav_AirpointSwith((byte)numericUpDown2.Value, (byte)numericUpDown3.Value, out msg);
            if (string.IsNullOrEmpty(msg))
            {
                SendData(ss);
                string s = "";
                string s1 = "";
                for (int i = 0; i < ss.Length; i++)
                {
                    s1 = Convert.ToString(ss[i], 16);
                    if (s1.Length == 1)
                        s1 = "0" + s1;
                    s = s + s1.ToUpper() + " ";
                }
                richTextBox1.Text = richTextBox1.Text + "\r\n" + s;
            }
            else
            {
                richTextBox1.Text = msg;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            NavSendMessage sendmsg = new NavSendMessage(1, db);
            NavCycleobj obj = new NavCycleobj();
            obj.CycleNum = (byte)px_CycleNum.Value;
            obj.Longitude = (long)px_Longitude.Value;
            obj.Latitude = (long)px_Latitude.Value;
            obj.Cycleradius = (Int16)px_Cycleradius.Value;
            obj.Boatincremental = (Int16)px_Boatincremental.Value;
            obj.Boatdistnct = (Int16)px_Boatdistnct.Value;
            if (radioButton1.Checked)
            {
                obj.Cycletype = 0;
            }
            else if (radioButton2.Checked)
            {
                obj.Cycletype = 1;
            }
            else if (radioButton3.Checked)
            {
                obj.Cycletype = 2;
            }
            byte[] ss = sendmsg.Nav_CycleSwith(obj,out msg);
            if (string.IsNullOrEmpty(msg))
            {
                SendData(ss);
                string s = "";
                string s1 = "";
                for (int i = 0; i < ss.Length; i++)
                {
                    s1 = Convert.ToString(ss[i], 16);
                    if (s1.Length == 1)
                        s1 = "0" + s1;
                    s = s + s1.ToUpper() + " ";
                }
                richTextBox1.Text = richTextBox1.Text + "\r\n" + s;
            }
            else
            {
                richTextBox1.Text = msg;
            }
        }

        
        private void btnBeginListen_Click(object sender, EventArgs e)
        {
            System.Threading.Thread t = new System.Threading.Thread(Listen);
            t.Start();
        }
        Socket _Socket;
        EndPoint _Remote = new IPEndPoint(IPAddress.Any, 0);
        private void Listen()
        {
            
            //得到本机IP，设置TCP端口号         
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, int.Parse(numericPort.Value.ToString()));
            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //绑定网络地址
            _Socket.Bind(ip);

            //得到客户机IP
            
            byte[] data = new byte[1024];
            while (true)
            {
                try
                {
                    int recv = _Socket.ReceiveFrom(data, ref _Remote);
                    byte[] tempdata = new byte[recv];
                    //tempdata.CopyTo(data, recv);
                    Array.Copy(data, tempdata, recv);
                    Application.DoEvents();
                    PrintLog(string.Format("{0}:{2}{1}{2}", _Remote, byteToHexStr(tempdata), Environment.NewLine));
                    Send();
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);
                    //newsock.SendTo(data, recv, SocketFlags.None, Remote); //发送消息
                }
                catch (Exception err)
                {
                    PrintLog(err.Message);
                }
            }
        }
        private void Send()
        {
            try
            {
                if (ckIsJianhua.Checked)
                {
                    //发送讲话指令
                    _Socket.SendTo(new byte[] { 0xBB, 0x01, 0x01, 0xBD, 0x0E }, _Remote);

                }
                else
                {
                    //发送接听指令
                    _Socket.SendTo(new byte[] { 0xBB, 0x01, 0x02, 0xBE, 0x0E }, _Remote);
                }
                PrintLog(string.Format("成功发送:{0}指令.", ckIsJianhua.Checked ? "讲话指令" : "接收指令"));
            }catch(Exception err)
            {
                PrintLog(err.Message);
            }
        }
        private delegate void PrintLogDelegate(string message); //代理
        private void PrintLog(string message)
        {

            if (this.richTextBox1.InvokeRequired)//等待异步
            {
                PrintLogDelegate fc = new PrintLogDelegate(PrintLog);
                this.Invoke(fc,new object[] { message}); //通过代理调用刷新方法
            }
            else
            {
                richTextBox1.AppendText(message);
            }
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(_Socket != null)
            {
                Send();
            }
        }
    }
}
