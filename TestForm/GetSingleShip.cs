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
using System.Text;
using System.Windows.Forms;

namespace TestForm
{
    public partial class GetSingleShip : Form
    {
        private const string _Url = "http://api.shipxy.com/apicall/GetSingleShip";//请求的地址
        private const string _Param = "v=2&k=1ee14d6d7788461380589643ce44bcb7&enc=1&idtype=0";

        public GetSingleShip()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 实时更新船舶轨迹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBeginSearch_Click(object sender, EventArgs e)
        {
            this.rtbResult.AppendText("==========开始更新船舶轨迹==========\r\n");
            try
            {
                this.rtbResult.AppendText(string.Format("\r\n[{0}]开始查询已安排的船舶跟踪任务............\r\n" , DateTime.Now)) ;

                string cnnstr = System.Configuration.ConfigurationManager.ConnectionStrings["VLP"].ConnectionString;
                using (SqlConnection cnn = new SqlConnection(cnnstr))
                {
                    SqlCommand cmmd = new SqlCommand("SELECT BILLID,shipId FROM dbo.V_GetShipMainTask");
                    cmmd.Connection = cnn;
                    SqlDataAdapter da = new SqlDataAdapter(cmmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    //存在已安排的船舶跟踪任务/记录唯一最新的船舶经纬度
                    if (dt.Rows.Count > 0)
                    {
                        string shipId = "", taskId = "",shipInfo = "";
                        Hashtable shipRow = null;
                        SqlCommand sqlCmmd = null;
                        SqlDataAdapter sqlDa = null;
                        foreach (DataRow dr in dt.Rows)
                        {
                            shipId = dr["shipId"].ToString();
                            taskId = dr["BILLID"].ToString();
                            shipInfo = HttpPost(shipId);
                            if (string.IsNullOrEmpty(shipInfo))
                            {
                               this.rtbResult.AppendText(string.Format("\r\n[{0}]根据此船舶shipId没有获取到船舶信息............\r\n", shipId));
                               continue;
                            }
                            //转换 JOSN 保存返回结果
                            shipRow = new Hashtable();
                            shipRow = (Hashtable)VLP.JSON.Decode(shipInfo);
                            //验证是否返回了正确的数据
                            if (shipRow.Count <= 0 || !"0".Equals(shipRow["status"].ToString()))
                            {
                               this.rtbResult.AppendText(string.Format("\r\n[{0}]根据此船舶shipId没有正确返回船舶信息............\r\n", shipId));
                               continue;
                            }
                            //保存船舶信息
                            sqlCmmd = GetShipInfoCommd(shipRow, taskId, cnn);
                            sqlDa = new SqlDataAdapter(sqlCmmd);
                            sqlDa.Fill(dt);
                        }

                        //1:获取已跟踪的船舶 shipId (dbo.D_Ship_MainTask)
                        //2:调用船讯网单船查询方法，获取船舶最新经纬度，插入dbo.Ship_Track  唯一标识： Guid.NewGuid()
                        //3:调用补点存储过程，插入新的船舶跟踪 飞行航线
                    }
                    else
                    {
                        this.rtbResult.AppendText(string.Format("\r\n[{0}]没有查询到已安排的船舶跟踪任务............\r\n", DateTime.Now));
                        this.rtbResult.AppendText(string.Format("\r\n[{0}]分钟后重新开始更新船舶轨迹............\r\n",5));
                    }
                }
            }
            catch (Exception ex)
            {
                this.rtbResult.AppendText("更新船舶轨迹失败：" + ex.ToString() + "\r\n\r\n");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shipRow"></param>
        /// <returns></returns>
        private SqlCommand GetShipInfoCommd(Hashtable shipRow, string taskId, SqlConnection cnn)
        {
            SqlCommand cmmd = new SqlCommand("sp_SaveShipTrack", cnn);
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
            cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskId);

            cmmd.Parameters.Add("@ShipID", SqlDbType.NVarChar, 20);
            cmmd.Parameters["@ShipID"].Value = (shipRow["ShipID"] == null || shipRow["ShipID"].ToString() == "") ? "" : shipRow["ShipID"];

            cmmd.Parameters.Add("@mmsi", SqlDbType.NVarChar, 20);
            cmmd.Parameters["@mmsi"].Value = (shipRow["mmsi"] == null || shipRow["mmsi"].ToString() == "") ? "" : shipRow["mmsi"];

            cmmd.Parameters.Add("@shiptype", SqlDbType.NVarChar, 20);
            cmmd.Parameters["@shiptype"].Value = (shipRow["shiptype"] == null || shipRow["shiptype"].ToString() == "") ? "" : shipRow["shiptype"];

            cmmd.Parameters.Add("@imo", SqlDbType.NVarChar, 20);
            cmmd.Parameters["@imo"].Value = (shipRow["imo"] == null || shipRow["imo"].ToString() == "") ? "" : shipRow["imo"];

            cmmd.Parameters.Add("@name", SqlDbType.NVarChar, 50);
            cmmd.Parameters["@name"].Value = (shipRow["name"] == null || shipRow["name"].ToString() == "") ? "" : shipRow["name"];

            cmmd.Parameters.Add("@lat", SqlDbType.Decimal);
            cmmd.Parameters["@lat"].Value = (shipRow["lat"] == null || shipRow["lat"].ToString() == "") ? DBNull.Value : shipRow["lat"];

            cmmd.Parameters.Add("@lon", SqlDbType.Decimal);
            cmmd.Parameters["@lon"].Value = (shipRow["lon"] == null || shipRow["lon"].ToString() == "") ? DBNull.Value : shipRow["lon"];

            cmmd.Parameters.Add("@lasttime", SqlDbType.NVarChar, 50);
            cmmd.Parameters["@lasttime"].Value = (shipRow["lasttime"] == null || shipRow["lasttime"].ToString() == "") ? "" : shipRow["lasttime"];

            cmmd.Parameters.Add("@GUID", SqlDbType.NVarChar, 200);
            cmmd.Parameters["@GUID"].Value = Guid.NewGuid();

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

        }
    }
}
