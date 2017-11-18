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


public partial class data_DUavImageCode : BasePage
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
    /// 获取登陆的配置信息 和已安排并且最终确认的当前任务ID
    /// </summary>
    public void getMonitorConfig()
    {
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            SqlCommand cmmd = new SqlCommand("SELECT * FROM V_GetMonitorConfig");
            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                result.TaskID = dr["BILLID"].ToString();
                result.UavSetID = dr["UavSetID"].ToString();
                result.MonitorIP = dr["MonitorIP"].ToString();
                result.MonitorPort = dr["MonitorPort"].ToString();
                result.MonitorAc = dr["MonitorAc"].ToString();
                result.MonitorPass = dr["MonitorPass"].ToString();
                result.dataTable = setFileLocalCfg();//设置文件保存地址
            }
            else 
            {
                result.IsOK = false;
                result.ErrMessage = "已检测,当前没有需要监控的任务!";
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
    ///设置本地参数 
    /// </summary>
    private DataTable setFileLocalCfg()
    {
        SqlCommand cmmd = new SqlCommand("SELECT Name,Name_E FROM V_FileLocalCfg");
        return DB.ExecuteDataTable(cmmd);
    }


    public partial class LoadFormResultN
    {
        //监控配置信息
        public string TaskID;
        public string UavSetID;
        public string MonitorIP;
        public string MonitorPort;
        public string MonitorAc;
        public string MonitorPass;
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