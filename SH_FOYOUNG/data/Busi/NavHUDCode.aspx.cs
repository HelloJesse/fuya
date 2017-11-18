using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Data.SqlClient;
using Nordasoft.Data.Sql;
using System.Collections.ObjectModel;
using VLP.NAV;
using System.IO;
using System.Drawing;
using System.Globalization;


public partial class data_NavHUDCode : BasePage
{
    UserInfo currentUserInfo = null;
    

    protected void Page_Load(object sender, EventArgs e)
    {
        currentUserInfo = GetUserInfo;
        if (IsOut(this.Context)) return;

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
    /// 根据飞机遥测信息，绘制HUD图
    /// </summary>
    public void onGetNavFlyPanel()
    {
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        try
        {
            SqlCommand cmmd = new SqlCommand("SELECT TOP 1 BILLID,UavSetID FROM  dbo.D_Uav_MainTask WHERE TaskStatus = 3 AND TrackCheckFlag = 1;");
            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt == null || dt.Rows.Count < 0)
            {
                result.IsOK = false;
                result.ErrMessage = "没有获取到当前已安排并且已确认的任务,绘制飞机HUD失败";
            }
            else
            {
                string uavID = dt.Rows[0]["UavSetID"].ToString();
                string taskID = dt.Rows[0]["BILLID"].ToString();
                //获取飞机遥测新型号信息 

                NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));
                DataTable dt_YaoCe = nav.Nav_Get_FlySignals(true, out msg);//获取最新的遥测信号
                if (dt_YaoCe == null || dt_YaoCe.Rows.Count <= 0)
                {
                    result.IsOK = false;
                    result.ErrMessage = "没有获取到最新的飞机遥测信号,绘制飞机HUD失败";
                }
                else
                {
                    if (null != dt_YaoCe.Rows[0]["Loction_Longitude"])
                    {
                        result.lng = dt_YaoCe.Rows[0]["Loction_Longitude"].ToString();//经度
                    }
                    if (null != dt_YaoCe.Rows[0]["Loction_Latitude"])
                    {
                        result.lat = dt_YaoCe.Rows[0]["Loction_Latitude"].ToString();//纬度
                    }
                    if (null != dt_YaoCe.Rows[0]["GroundHeight"])
                    {
                        result.high = Convert.ToDecimal(dt_YaoCe.Rows[0]["GroundHeight"]);//当前对地高度
                    }
                    if (null != dt_YaoCe.Rows[0]["GroundSpeed"])
                    {
                        result.speed = Convert.ToDecimal(dt_YaoCe.Rows[0]["GroundSpeed"]);//当前地速
                    }
                    if (null != dt_YaoCe.Rows[0]["RollAngle"])
                    {
                        result.balance = Convert.ToDecimal(dt_YaoCe.Rows[0]["RollAngle"]);//当前滚转角
                    }
                    if (null != dt_YaoCe.Rows[0]["PitchingAngle"])
                    {
                        result.angle = Convert.ToDecimal(dt_YaoCe.Rows[0]["PitchingAngle"]);//当前俯仰角
                    }
                    if (null != dt_YaoCe.Rows[0]["NoseAzimuth"])
                    {
                        result.direction = Convert.ToDecimal(dt_YaoCe.Rows[0]["NoseAzimuth"]);//当前机头方位角
                    }
                    
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

    public partial class LoadFormResultN
    {
        /// <summary>
        /// 经度
        /// </summary>
        public string lng = "0";
        /// <summary>
        /// 纬度
        /// </summary>
        public string lat = "0";
        /// <summary>
        /// 高度High
        /// </summary>
        public decimal high = 0;
        /// <summary>
        /// 速度Speed
        /// </summary>
        public decimal speed = 0;
        /// <summary>
        /// 左倾或右倾角度Balance
        /// </summary>
        public decimal balance = 0;
        /// <summary>
        ///前倾或后倾角度Angle 
        /// </summary>
        public decimal angle = 0;
        /// <summary>
        ///方向Direction 
        /// </summary>
        public decimal direction = 0;
        /// <summary>
        /// 
        /// </summary>
        public string navHUDUrl = null;
        /// <summary>
        /// DataList 
        /// </summary>
        public DataTable dataTable = null;
        /// <summary>
        /// 其它
        /// </summary>
        public bool IsOK = false;
        public string ErrMessage = string.Empty;
    }
}