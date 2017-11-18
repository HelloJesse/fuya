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


public partial class data_NavChangeLineCode : BasePage
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
    /// 航线切换
    /// </summary>
    public void updateCheckLine()
    {
        string cLine = Request.Form.Get("cLine");
        string cPoint = Request.Form.Get("cPoint");

        string taskID = Request.Form.Get("TaskID");
        string uavID = Request.Form.Get("UavID");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        try
        {
            //获取当前用户所属部门
            result.departmentID = Convert.ToInt32(currentUserInfo.DepartmentID);

            //说明是测试页面传来的需要查出当前任务 Id信息
            if (string.IsNullOrEmpty(taskID) || string.IsNullOrEmpty(uavID))
            {
                SqlCommand cmmd = new SqlCommand("SELECT TOP 1 BILLID,UavSetID FROM  dbo.D_Uav_MainTask WHERE TaskStatus = 3 AND TrackCheckFlag = 1;");
                DataTable dt = DB.ExecuteDataTable(cmmd);
                if (dt == null || dt.Rows.Count < 0)
                {
                    result.IsOK = false;
                    result.ErrMessage = "没有获取到当前已安排并且已确认的任务,操作已返回！";
                }
                else
                {
                    uavID = dt.Rows[0]["UavSetID"].ToString();
                    taskID = dt.Rows[0]["BILLID"].ToString();
                }
            }

            //查看飞机的航行状态字段 
            //首先检查当前飞机 是否有新目标船舶跟踪,如果有 则返回航行状态 
            //检查是否有跟踪任务 如果有 则返回航行状态 
            //TrackCycleFlag 值为 0 或 1
            SqlCommand cmmdPlane = new SqlCommand("sp_GetPlaneTrackCycleFlag");
            cmmdPlane.CommandType = CommandType.StoredProcedure;
            cmmdPlane.Parameters.Add("@TaskID", SqlDbType.Int);
            cmmdPlane.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);
            DataTable dtPlane = DB.ExecuteDataTable(cmmdPlane);
            if (dtPlane != null && dtPlane.Rows.Count >= 1)
            {
                result.trackCycleFlag = dtPlane.Rows[0]["TrackCycleFlag"].ToString(); 
            }

            //切入航线航点
            NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));
            result.IsOK = nav.Nav_Change_Line(Convert.ToByte(cLine), Convert.ToInt32(cPoint), out msg);

            //保存操作动作
            SqlCommand cmmdOrder = Common.saveControlTypeLog(taskID, uavID, "切入航线[" + cLine + "]航点[" + cPoint + "]", null, result.IsOK, result.ErrMessage, 0, currentUserInfo.ID);
            DB.ExecuteNonQuery(cmmdOrder);
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 目标经纬度盘旋指令 
    /// </summary>
    public void updateControlCycle()
    {
        string lng = Request.Form.Get("lng");
        string lat = Request.Form.Get("lat");
        string num = Request.Form.Get("num");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        try
        {
            //一 查询当前已安排的航线任务
            SqlCommand cmmd = new SqlCommand("SELECT TOP 1 BILLID,UavSetID FROM  dbo.D_Uav_MainTask WHERE TaskStatus = 3 AND TrackCheckFlag = 1;");
            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt == null || dt.Rows.Count < 0)
            {
                result.IsOK = false;
                result.ErrMessage = "没有获取到当前已安排并且已确认的任务,操作已返回！";
            }
            else
            {
                string uavID = dt.Rows[0]["UavSetID"].ToString();
                string taskID = dt.Rows[0]["BILLID"].ToString();
                NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));

                //取出飞机最新经纬度信息
                DataTable dt_YaoCe = nav.Nav_Get_FlySignals(true, out msg);
                if (dt_YaoCe == null || dt_YaoCe.Rows.Count <= 0)
                {
                    result.IsOK = false;
                    result.ErrMessage = "没有获取到最新的飞机遥测信号,目标经纬度盘旋指令失败,操作已返回！";
                }
                else
                {
                    result.flyLng = dt_YaoCe.Rows[0]["Loction_Longitude"].ToString();
                    result.flyLat = dt_YaoCe.Rows[0]["Loction_Latitude"].ToString();

                    NavCycleobj obj = new NavCycleobj();
                    obj.CycleNum = Convert.ToByte(num);
                    obj.Longitude = Convert.ToInt64(Convert.ToDecimal(lng) * 1000000);
                    obj.Latitude = Convert.ToInt64(Convert.ToDecimal(lat) * 1000000);
                    obj.Cycleradius = 200;
                    obj.Cycletype = 0;//盘旋跟踪某一个目标
                    obj.Boatincremental = 0; //度数
                    obj.Boatdistnct = 0;//暂时不知道如何使用
                    if (obj.Longitude == 0 || obj.Latitude == 0)
                    {
                        result.IsOK = false;
                        result.ErrMessage = "请确认盘旋中心点坐标正确.";
                    }
                    #region 3.切换到盘旋航线
                    result.IsOK = nav.Nav_Change_Line(obj, out msg);
                    #endregion
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
        /// 0 飞机飞行状态为目标跟踪  1 飞机飞行状态为目标盘旋
        /// </summary>
        public string trackCycleFlag = null;

        public string flyLat = null;
        public string flyLng = null;
        /// <summary>
        /// DataList 
        /// </summary>
        public DataTable dataTable = null;
        /// <summary>
        /// 当前用户所属部门 
        /// </summary>
        public int departmentID = -1;
        /// <summary>
        /// 其它
        /// </summary>
        public bool IsOK = false;
        public string ErrMessage = string.Empty;
    }
}