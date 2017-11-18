using Nordasoft.Data.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using VLP.WMS;

public partial class data_Main_Help_Demo_Manage : BasePage
{
    UserInfo currentUserInfo = null;
    DataBase _DB_M = null;
    //菜单处理
    protected void Page_Load(object sender, EventArgs e)
    {
        string methodName = Request["method"];
        Type type = this.GetType();
        MethodInfo method = type.GetMethod(methodName);
        if (method == null) throw new Exception("method is null");
        try
        {
            _DB_M = new DataBase(System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString);
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
    /// 需要获取显示已经保存的HelpHtml 内容
    /// </summary>
    public void GetHelp_Demo_Manage()
    {
        string ID = Request.Form.Get("ID");
        string ParentID = Request.Form.Get("ParentID");
        Hashtable result = new Hashtable();
        result["msg"] = "true";
        if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(ParentID))
        {
            result["msg"] = "false";
            Response.Write(VLP.JSON.Encode(result));
            return;
        }
        DataTable dt = Help_Demo_Manage.GetHelp_Demo_ManageHtml(ID, ParentID, _DB_M);
        if (dt == null || dt.Rows.Count <= 0)
        {
            result["msg"] = "false";
            Response.Write(VLP.JSON.Encode(result));
            return;
        }
        result["text"] = dt.Rows[0][0].ToString();
        Response.Write(VLP.JSON.Encode(result)); 
    }

    /// <summary>
    /// 保存HelpHtml内容
    /// </summary>
    public void SaveHelp_Demo_Manage()
    {
        string ID = Request.Form.Get("ID");
        string ParentID = Request.Form.Get("ParentID");
        string Text = Request.Form.Get("Text");

        Text = Server.UrlDecode(Text);
        Hashtable result = new Hashtable();
        result["msg"] = "true";
        if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(ParentID))
        {
            result["msg"] = "false";
            Response.Write(VLP.JSON.Encode(result));
            return;
        }
        try
        {
            Help_Demo_Manage.SaveHelp_Demo_ManageHtml(ID, ParentID, Text, _DB_M);
        }
        catch (Exception ex)
        {
            result["msg"] = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

}