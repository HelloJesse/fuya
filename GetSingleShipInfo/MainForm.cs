using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using GetSingleShipInfo.Data;
using GetSingleShipInfo.Models;


namespace GetSingleShipInfo
{
    public partial class MainForm : Form
    {
        private const string _Url = "http://api.shipxy.com/apicall/GetSingleShip";//请求的地址
        private const string _Param = "v=2&k=1ee14d6d7788461380589643ce44bcb7&enc=1&idtype=0";
        private const string _cnnstr = "server=116.62.103.98,1438;database=uav_foyoung;user id=foyoung;password=vlp@foyoung;";
        private const int _Minutes = 10;
        private TrackInfoRepository _trackInfoRepository;
        private delegate void StringArgReturningVoidDelegate(string text);
        private delegate void OperationDelegate();
        private bool _isRunning = false;

        System.Timers.Timer timer = null;

        public MainForm()
        {
            InitializeComponent();
            //timer = new System.Timers.Timer(60000);   //实例化Timer类，设置间隔时间为60000毫秒；
            timer = new System.Timers.Timer(2000);   //实例化Timer类，设置间隔时间为10000毫秒；
            this.btnEnd.Enabled = false;
        }

        /// <summary>
        /// 开始定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBeginSearch_Click(object sender, EventArgs e)
        {
            this.rtbResult.Text = "";
            AppendMessage("==========开启更新船舶轨迹==========\r\n");
            AppendMessage(string.Format("当前时间[{0}] :[{1}]秒后开始更新船舶轨迹...\r\n", DateTime.Now, _Minutes));

            timer.Elapsed += new System.Timers.ElapsedEventHandler(DoSearchShipInfo); //到达时间的时候执行事件；   

            timer.AutoReset = true;   //设置是执行一次（false）还是一直执行(true)；   
            timer.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件；   
            timer.Start();

            //开始后，设置按钮不可用，防止在次点击
            this.btnBeginSearch.Enabled = false;
            this.btnEnd.Enabled = true;
        }

        #region 内存回收
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion


        private void DoSearchShipInfo(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                SearchShipInfo();
                _isRunning = false;
            }
        }

        /// <summary>
        /// 实时更新船舶轨迹
        /// </summary>
        private void SearchShipInfo()
        {
            //内存回收
            ClearMemory();
            try
            {
                //每小时清除一次文本框内容
                if (DateTime.Now.ToString("mm").Equals("59"))
                {
                    this.rtbResult.Text = "";
                    
                }

                AppendMessage(string.Format("\r\n[{0}]开始查询已安排的船舶跟踪任务...\r\n", DateTime.Now));
                setrichTextBoxScroll();

                using (SqlConnection cnn = new SqlConnection(_cnnstr))
                {
                    SqlCommand cmmd = new SqlCommand("SELECT BILLID,BILLNO,shipId FROM dbo.V_GetShipMainTask");
                    cmmd.Connection = cnn;
                    SqlDataAdapter da = new SqlDataAdapter(cmmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    //存在已安排的船舶跟踪任务/记录唯一最新的船舶经纬度
                    if (dt.Rows.Count > 0)
                    {
                        string shipId = "", taskId = "", shipInfo = "";
                        Hashtable shipRow = null;
                        SqlCommand sqlCmmd = null;
                        SqlDataAdapter sqlDa = null;
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (dr["shipId"] == null || string.IsNullOrEmpty(dr["shipId"].ToString()))
                            {
                                AppendMessage(string.Format("\r\n[{0}]任务单号[{1}]没有需要跟踪的船舶目标...\r\n", DateTime.Now, dr["BILLNO"].ToString()));
                                setrichTextBoxScroll();
                                continue;
                            }
                            shipId = dr["shipId"].ToString();
                            taskId = dr["BILLID"].ToString();
                            if (shipId.Equals("111111111"))//测试船舶不需要自动更新
                            {
                                continue;
                            }

                            shipInfo = HttpPost(shipId);
                            if (string.IsNullOrEmpty(shipInfo))
                            {
                                AppendMessage(string.Format("\r\n[{0}]根据此船舶shipId没有获取到船舶信息...\r\n", shipId));
                                setrichTextBoxScroll();
                                continue;
                            }
                            //转换 JOSN 保存返回结果
                            shipRow = new Hashtable();
                            shipRow = (Hashtable)VLP.JSON.Decode(shipInfo);
                            //验证是否返回了正确的数据
                            if (shipRow.Count <= 0 || !"0".Equals(shipRow["status"].ToString()))
                            {
                                AppendMessage(string.Format("\r\n[{0}]根据此船舶shipId没有正确返回船舶信息,错误编码[{1}]...\r\n", shipId, shipRow["status"].ToString()));
                                setrichTextBoxScroll();
                                continue;
                            }
                            ArrayList rows = (ArrayList)VLP.JSON.Decode(VLP.JSON.Encode(shipRow["data"]));
                            foreach (Hashtable row in rows)
                            {
                                var trackInfo = new TrackInfo(row);
                                if (_trackInfoRepository.ExistsByShipIdAndLasttime(trackInfo.ShipId, trackInfo.Lasttime)) {
                                    DateTime dttime = DateTime.Now;
                                    DateTime time = SpecialFunctions.ConvertIntDateTime(trackInfo.Lasttime);
                                    //计算出相差的秒数
                                    int sec = (int)(dttime - time).TotalSeconds;
                                    if (sec < 120)
                                    {
                                        //不考虑 为null的情况  拿到角度（度）
                                        double angle = ((trackInfo.Heading > 35990 || trackInfo.Heading <= 0) ? trackInfo.Course * 1.0 : trackInfo.Heading * 1.0) / 100;
                                        //米/秒
                                        double speed = trackInfo.Speed * 1.0 / 1000;
                                        //距离（海里 ） 按照中国标准算（1海里=1.852公里=1852米）
                                        double distance = sec * speed / 1852;
                                        ////纬度 换度数
                                        //string lat = SpecialFunctions.ConvertDigitalToDegrees(trackInfo.Latitude * 1.0 / 1000000);
                                        ////经度 换度数
                                        //string lon = SpecialFunctions.ConvertDigitalToDegrees(trackInfo.Longitude * 1.0 / 1000000);
                                        //根据角度不同 cos 会自动正负  无需单独处理
                                        double deviationLat = Math.Cos(angle) * distance * 10000;
                                        //产生的新经度
                                        double latNew = deviationLat + trackInfo.Latitude;

                                        double m = SpecialFunctions.ConvertDigitalToDegrees((latNew + deviationLat) * 1.0 / 2000000);

                                        double depm = Math.Sin(angle) * distance * (1 / Math.Cos(m)) * 10000;

                                        double lonNew = depm + trackInfo.Longitude;


                                        trackInfo.Lasttime = SpecialFunctions.ConvertDateTimeInt(dttime);
                                        //  LogUtilities.WriteLine($"dt:{dt.ToString()},time:{time},latNew：{(int)latNew},lonNew{(int)lonNew},own:{ JsonConvert.SerializeObject(trackInfo)}");
                                        trackInfo.Latitude = (int)latNew;
                                        trackInfo.Longitude = (int)lonNew;
                                        trackInfo.Course = 0;
                                        _trackInfoRepository.Insert(trackInfo);
                                    }
                                        continue;
                                }
                                  
                                
                                //保存船舶信息
                                sqlCmmd = GetShipInfoCommd(row, taskId, cnn);
                                sqlDa = new SqlDataAdapter(sqlCmmd);
                                sqlDa.Fill(dt);
                                AppendMessage(string.Format("\r\n[{0}]船舶信息保存成功...\r\n", shipId));
                                setrichTextBoxScroll();

                                _trackInfoRepository.Insert(trackInfo);
                            }
                        }

                        AppendMessage(string.Format("\r\n本次查询结束,[{0}]秒后重新开始更新船舶轨迹...\r\n", _Minutes));
                        setrichTextBoxScroll();

                        /**后期需求
                        1:调用补点存储过程，插入新的船舶跟踪 飞行航线
                        */
                    }
                    else
                    {
                        AppendMessage(string.Format("\r\n[{0}]没有查询到已安排的船舶跟踪任务...\r\n", DateTime.Now));
                        AppendMessage(string.Format("\r\n[{0}]秒后重新开始更新船舶轨迹...\r\n", _Minutes));
                        setrichTextBoxScroll();
                    }
                }


            }
            catch (Exception ex)
            {
                AppendMessage("更新船舶轨迹失败：" + ex.ToString() + "\r\n\r\n");
                setrichTextBoxScroll();
            }
        }

        
        private void AppendMessage(string message)
        {

            if (this.rtbResult.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AppendMessage);
                this.Invoke(d, new object[] { message });
            }
            else
            {
                this.rtbResult.AppendText(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shipRow"></param>
        /// <returns></returns>
        private SqlCommand GetShipInfoCommd(Hashtable row, string taskId, SqlConnection cnn)
        {
            SqlCommand cmmd = new SqlCommand("sp_SaveShipTrack", cnn);
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
            cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskId);

            cmmd.Parameters.Add("@ShipID", SqlDbType.NVarChar, 20);
            cmmd.Parameters["@ShipID"].Value = (row["ShipID"] == null || row["ShipID"].ToString() == "") ? "" : row["ShipID"];

            cmmd.Parameters.Add("@mmsi", SqlDbType.NVarChar, 20);
            cmmd.Parameters["@mmsi"].Value = (row["mmsi"] == null || row["mmsi"].ToString() == "") ? "" : row["mmsi"];

            cmmd.Parameters.Add("@shiptype", SqlDbType.NVarChar, 20);
            cmmd.Parameters["@shiptype"].Value = (row["shiptype"] == null || row["shiptype"].ToString() == "") ? "" : row["shiptype"];

            cmmd.Parameters.Add("@imo", SqlDbType.NVarChar, 20);
            cmmd.Parameters["@imo"].Value = (row["imo"] == null || row["imo"].ToString() == "") ? "" : row["imo"];

            cmmd.Parameters.Add("@name", SqlDbType.NVarChar, 50);
            cmmd.Parameters["@name"].Value = (row["name"] == null || row["name"].ToString() == "") ? "" : row["name"];

            cmmd.Parameters.Add("@lat", SqlDbType.Decimal);
            cmmd.Parameters["@lat"].Value = (row["lat"] == null || row["lat"].ToString() == "") ? 0 : Convert.ToDecimal(row["lat"]) * Convert.ToDecimal(0.000001);

            cmmd.Parameters.Add("@lon", SqlDbType.Decimal);
            cmmd.Parameters["@lon"].Value = (row["lon"] == null || row["lon"].ToString() == "") ? 0 : Convert.ToDecimal(row["lon"]) * Convert.ToDecimal(0.000001);

            cmmd.Parameters.Add("@lasttime", SqlDbType.NVarChar, 50);
            cmmd.Parameters["@lasttime"].Value = (row["lasttime"] == null || row["lasttime"].ToString() == "") ? "" : row["lasttime"];

            //新增记录字段 状态 船迹向 航速
            cmmd.Parameters.Add("@status", SqlDbType.Int);
            cmmd.Parameters["@status"].Value = (row["navistat"] == null || row["navistat"].ToString() == "") ? DBNull.Value : row["navistat"];

            cmmd.Parameters.Add("@course", SqlDbType.Float);
            cmmd.Parameters["@course"].Value = (row["cog"] == null || row["cog"].ToString() == "") ? 0 : Convert.ToDecimal(row["cog"].ToString()) / 100;

            cmmd.Parameters.Add("@heading", SqlDbType.Float);
            cmmd.Parameters["@heading"].Value = (row["hdg"] == null || row["hdg"].ToString() == "") ? 0 : Convert.ToDecimal(row["hdg"].ToString()) / 100;

            cmmd.Parameters.Add("@speed", SqlDbType.Float);
            cmmd.Parameters["@speed"].Value = (row["sog"] == null || row["sog"].ToString() == "") ? DBNull.Value : row["sog"];

            cmmd.Parameters.Add("@GUID", SqlDbType.NVarChar, 200);
            cmmd.Parameters["@GUID"].Value = Guid.NewGuid().ToString();

            return cmmd;
        }

        /// <summary>
        /// 调用船讯网单船查询方法 
        /// </summary>
        /// <returns></returns>
        private string HttpPost(string shipId)
        {
            string param = _Param + "&id=" + shipId;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = param.Length;
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            writer.Write(param);
            writer.Flush();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码  
            }
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retString = reader.ReadToEnd();
            return retString;
        }


        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnd_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
            timer.Stop();

            this.btnBeginSearch.Enabled = true;
            this.btnEnd.Enabled = false;
        }

        /// <summary>
        /// 跟随数据向下移动
        /// </summary>
        private void setrichTextBoxScroll()
        {
            rtbResult.Invoke(new OperationDelegate(() => {
                rtbResult.SelectionStart = rtbResult.TextLength;
                rtbResult.ScrollToCaret();
            }));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _trackInfoRepository = new TrackInfoRepository();
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动MongoDB失败，请检查! " + ex.Message);
            }
            
        }
    }
}
