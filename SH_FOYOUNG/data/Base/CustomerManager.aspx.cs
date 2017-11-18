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


public partial class data_System_CustomerManager : BasePage
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
    /// 锁定和取消锁定
    /// </summary>
    public void UpdateLockStatus()
    {
        string Ids = Request.Form.Get("Ids");
        string FLAG = Request.Form.Get("FLAG");
      
        Hashtable result = new Hashtable();

        SqlCommand cmmd = new SqlCommand("sp_D_CustomerUpdateLockStatus");
        cmmd.CommandType = CommandType.StoredProcedure;

        cmmd.Parameters.Add("@Ids", SqlDbType.VarChar, -1);
        cmmd.Parameters["@Ids"].Value = Ids;

        cmmd.Parameters.Add("@USERID", SqlDbType.Int);
        cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

        cmmd.Parameters.Add("@Flag", SqlDbType.VarChar, 20);
        cmmd.Parameters["@Flag"].Value = FLAG;

        DataTable dt = DB.ExecuteDataTable(cmmd);
        if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0].ToString().Length > 0)
        {
            if (dt.Rows[0][0].ToString() == "0")
            {
                result["msg"] = "true";
            }
            else
            {
                result["msg"] = dt.Rows[0][1];
            }
        }
        else
        {
            result["msg"] = "操作失败，联系管理员。";
        }
        String json = VLP.JSON.Encode(result);
        Response.Write(json);
    }

}