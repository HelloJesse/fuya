using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace GetSingleShipInfo
{
    using GetSingleShipInfo.Data;
    using GetSingleShipInfo.Models;

    public partial class FromTest : Form
    {
        public FromTest()
        {
            InitializeComponent();
        }
        private const string _Url = "http://api.shipxy.com/apicall/GetSingleShip";//请求的地址
        private const string _Param = "v=2&k=1ee14d6d7788461380589643ce44bcb7&enc=1&idtype=0";
        private TrackInfoRepository _trackInfoRepository;
        System.Timers.Timer timer;
        private bool _isRunning = false;

        private void btnBegin_Click(object sender, EventArgs e)
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;

            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            SendPost();
            _isRunning = false;
            
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            //终止
            timer.Enabled = false;
            timer.Stop();
        }

        Hashtable shipRow = null;
        string shipInfo = string.Empty;
        private void SendPost()
        {

            
            shipInfo = HttpPost("413831556");
            if (!string.IsNullOrEmpty(shipInfo))
            {
                //转换 JOSN 保存返回结果
                shipRow = new Hashtable();
                shipRow = (Hashtable)VLP.JSON.Decode(shipInfo);
                if (shipRow.Count > 0)
                {
                    ArrayList rows = (ArrayList)VLP.JSON.Decode(VLP.JSON.Encode(shipRow["data"]));
                    foreach (Hashtable row in rows)
                    {
                        var trackInfo = new TrackInfo(row);
                        if (_trackInfoRepository.ExistsByShipIdAndLasttime(trackInfo.ShipId, trackInfo.Lasttime))
                        {
                            DateTime dt = DateTime.Now;
                            DateTime time = SpecialFunctions.ConvertIntDateTime(trackInfo.Lasttime);
                            //计算出相差的秒数
                            int sec = (int)(dt - time).TotalSeconds;
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


                                trackInfo.Lasttime = SpecialFunctions.ConvertDateTimeInt(dt);
                                //  LogUtilities.WriteLine($"dt:{dt.ToString()},time:{time},latNew：{(int)latNew},lonNew{(int)lonNew},own:{ JsonConvert.SerializeObject(trackInfo)}");
                                trackInfo.Latitude = (int)latNew;
                                trackInfo.Longitude = (int)lonNew;
                                trackInfo.Course = 0;

                                _trackInfoRepository.Insert(trackInfo);
                            }
                            continue;
                        }
                        //  LogUtilities.WriteLine($"nowtime:{SpecialFunctions.ConvertDateTimeInt(DateTime.Now)},now:{DateTime.Now.ToString()},lasttime:{SpecialFunctions.ConvertIntDateTime(trackInfo.Lasttime)},new:{JsonConvert.SerializeObject(trackInfo)}");
                        _trackInfoRepository.Insert(trackInfo);
                    }
                }
            }
            // NLogHelper.Info(HttpPost("413776252"));
        }

        /// <summary>
        /// 调用船讯网单船查询方法 
        /// </summary>
        /// <returns></returns>
        private string HttpPost(string shipId)
        {
            string retString = string.Empty;
            try
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
                retString = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // NLogHelper.Error(ex);
            }
            return retString;
        }

        private void FromTest_Load(object sender, EventArgs e)
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
