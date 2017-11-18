using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using Nordasoft.Data.Sql;
using System.Configuration;
using VLP.WMS;

public partial class data_Main_ChangePwd : BasePage
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
    /// 保存属性设置
    /// </summary>
    public void UpdatePwd()
    {
        string oldPwd = Request.Form.Get("oldPwd"); //主表信息 
        string newPwd = Request.Form.Get("newPwd"); //主表信息 
        
        System.Collections.Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "true";
        String json = string.Empty;
        try
        {
            //更新密码
            SqlCommand cmmd = new SqlCommand();
            cmmd.CommandType = CommandType.Text;
            cmmd.CommandText = "select * from Sys_D_USER where ID=@USER_ID AND USER_PWD=@OLD_USER_PWD";
            cmmd.Parameters.Add("@USER_ID", SqlDbType.Int);
            cmmd.Parameters["@USER_ID"].Value = currentUserInfo.ID;

            cmmd.Parameters.Add("@OLD_USER_PWD", SqlDbType.NVarChar, 250);
            cmmd.Parameters["@OLD_USER_PWD"].Value = VLP.WMS.Common.Encrypt(oldPwd);
            

            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt != null && dt.Rows.Count > 0)
            {
                cmmd = new SqlCommand();
                cmmd.CommandText = "update Sys_D_USER set USER_PWD=@USER_PWD,UPDATE_BY=@USER_ID,UPDATE_DATE=GETDATE(),Emp_ID_UPDATE_BY=dbo.GetEmpID(@USER_ID) WHERE ID=@USER_ID";
                cmmd.Parameters.Add("@USER_ID", SqlDbType.Int);
                cmmd.Parameters["@USER_ID"].Value = currentUserInfo.ID;

                cmmd.Parameters.Add("@USER_PWD", SqlDbType.NVarChar, 250);
                cmmd.Parameters["@USER_PWD"].Value = VLP.WMS.Common.Encrypt(newPwd);
                
                DB.ExecuteNonQuery(cmmd);
                result["msg"] = "true";
            }
            else
            {
                result["msg"] = "原始密码不正确.";
            }
                         

        }
        catch (Exception ex)
        {
            result["msg"] = ex.Message;//保存成功，返回true
        }
        json = VLP.JSON.Encode(result);
        Response.Write(json);

    }

    /// <summary>
    /// 获取加密密码
    /// </summary>
    public void GetEncryptPwd()
    {
        string pwd = Request.Form.Get("pwd"); //主表信息 
       
        System.Collections.Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "true";
        String json = string.Empty;
        try
        {
            result["msg"] = "true";
            result["pwd"] = VLP.WMS.Common.Encrypt(pwd);
        }
        catch (Exception ex)
        {
            result["msg"] = ex.Message;//保存成功，返回true
            result["pwd"] = ex.Message;

        }
        json = VLP.JSON.Encode(result);
        Response.Write(json);

    }

    /// <summary>
    /// 获取解密密码
    /// </summary>
    public void GetDecryptPwd()
    {
        string pwd = Request.Form.Get("pwd"); //主表信息 

        System.Collections.Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "true";
        String json = string.Empty;
        try
        {
            result["msg"] = "true";
            result["pwd"] = VLP.WMS.Common.Decrypt(pwd);
        }
        catch (Exception ex)
        {
            result["msg"] = ex.Message;//保存成功，返回true
            result["pwd"] = ex.Message;//保存成功，返回true
        }
        json = VLP.JSON.Encode(result);
        Response.Write(json);

    }
}