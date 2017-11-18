using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using Nordasoft.Data.Sql;
using System.Collections.ObjectModel;
using VLP.NAV;


public partial class data_NavGetgpsCode : BasePage
{
    UserInfo currentUserInfo = null;
    private const string _cnnstr = "server=116.62.103.98,1438;database=uav_foyoung;user id=foyoung;password=vlp@foyoung;";

    protected void Page_Load(object sender, EventArgs e)
    {
        currentUserInfo = GetUserInfo;
        //if (IsOut(this.Context)) return;

        string methodName = Request["method"];
        Type type = this.GetType();
        MethodInfo method = type.GetMethod(methodName);
        if (method == null) throw new Exception("method is null");
        try
        {
            BeforeInvoke(methodName);
            method.Invoke(this, null);
        }
        catch (Exception ex)
        {
            Hashtable result = new Hashtable();
            result["error"] = -1;
            result["message"] = ex.Message;
            result["stackTrace"] = ex.StackTrace;
            String json = VLP.JSON.Encode(result);
            Response.Clear();
            Response.Write(json);
        }
        finally
        {
            AfterInvoke(methodName);
        }
    }

    //权限管理
    protected void BeforeInvoke(String methodName)
    {

    }

    //日志管理
    protected void AfterInvoke(String methodName)
    {

    }

    /// <summary>
    /// 读取 gps 经纬度
    /// </summary>
    public void getGps()
    {
        string lng = Request.Params["lng"];
        string lat = Request.Params["lat"];

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            using (SqlConnection cnn = new SqlConnection(_cnnstr))
            {
                SqlCommand cmmd = new SqlCommand("SELECT TOP 1 BILLID FROM  dbo.D_Uav_MainTask WHERE TaskStatus = 3 AND TrackCheckFlag = 1;");
                cmmd.Connection = cnn;
                SqlDataAdapter da = new SqlDataAdapter(cmmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count < 0)
                {
                    result.IsOK = false;
                    result.ErrMessage = "没有获取到当前已安排并且已确认的任务,操作已返回！";
                }
                else if (string.IsNullOrEmpty(lng) || string.IsNullOrEmpty(lat) )
                {
                    result.IsOK = false;
                    result.ErrMessage = "没有读取到GPS信息,操作已返回！";
                }
                else
                {
                    string taskID = dt.Rows[0]["BILLID"].ToString();
                    SqlCommand sqlCmmd = GetShipInfoCommd(lng, lat, taskID, cnn); 
                    SqlDataAdapter sqlDa = new SqlDataAdapter(sqlCmmd);
                    DataTable sqlDt = new DataTable();
                    sqlDa.Fill(sqlDt);

                    result.GpsLng = lng;
                    result.GpsLat = lat;

                    result.ErrMessage = "读取录入成功";
                    result.TimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 模拟船舶gps 保存当前经纬度信息
    /// </summary>
    /// <param name="shipRow"></param>
    /// <returns></returns>
    private SqlCommand GetShipInfoCommd(string lng, string lat, string taskId, SqlConnection cnn)
    {
        SqlCommand cmmd = new SqlCommand("sp_SaveShipTrack", cnn);
        cmmd.CommandType = CommandType.StoredProcedure;

        cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskId);

        cmmd.Parameters.Add("@ShipID", SqlDbType.NVarChar, 20);
        cmmd.Parameters["@ShipID"].Value = "111111111";

        cmmd.Parameters.Add("@mmsi", SqlDbType.NVarChar, 20);
        cmmd.Parameters["@mmsi"].Value = "-1";

        cmmd.Parameters.Add("@shiptype", SqlDbType.NVarChar, 20);
        cmmd.Parameters["@shiptype"].Value = "90";

        cmmd.Parameters.Add("@imo", SqlDbType.NVarChar, 20);
        cmmd.Parameters["@imo"].Value = "111111111";

        cmmd.Parameters.Add("@name", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@name"].Value = "虚拟测试船";

        cmmd.Parameters.Add("@lat", SqlDbType.Decimal);
        cmmd.Parameters["@lat"].Value = Convert.ToDecimal(lat);

        cmmd.Parameters.Add("@lon", SqlDbType.Decimal);
        cmmd.Parameters["@lon"].Value = Convert.ToDecimal(lng);

        cmmd.Parameters.Add("@lasttime", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@lasttime"].Value = "1483686471";

        //新增记录字段 状态 船迹向 航速
        cmmd.Parameters.Add("@status", SqlDbType.Int);
        cmmd.Parameters["@status"].Value = 0;

        cmmd.Parameters.Add("@course", SqlDbType.Float);
        cmmd.Parameters["@course"].Value = 1;

        cmmd.Parameters.Add("@heading", SqlDbType.Float);
        cmmd.Parameters["@heading"].Value = 1;

        cmmd.Parameters.Add("@speed", SqlDbType.Float);
        cmmd.Parameters["@speed"].Value = 5;

        cmmd.Parameters.Add("@GUID", SqlDbType.NVarChar, 200);
        cmmd.Parameters["@GUID"].Value = Guid.NewGuid().ToString();

        return cmmd;
    }
   
    public partial class LoadFormResultN
    {
        public string GpsLng = null;
        public string GpsLat = null;
        /// <summary>
        /// 其它
        /// </summary>
        public bool IsOK = false;
        public string ErrMessage = string.Empty;
        public string TimeNow = null;
    }
}