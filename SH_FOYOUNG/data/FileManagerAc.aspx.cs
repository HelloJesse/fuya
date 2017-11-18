using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class data_FileManagerAc : BasePage
{
    //文件管理的处理
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
            result["msg"] = ex.Message;
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
    protected void BeforeInvoke(String methodName) { }

    //日志管理
    protected void AfterInvoke(String methodName) { }


    /// <summary>
    /// 获取文档主表的MainID
    /// </summary>
    public void FileCreateIndex()
    {
        string PkValue = Request.Form.Get("PkValue");
        string TypeID = Request.Form.Get("TypeID");
        string DBsID = Request.Form.Get("DBsID");
        System.Collections.Hashtable result = new System.Collections.Hashtable();
        if (string.IsNullOrEmpty(DBsID))
        {
            DBsID = "1";
        }
        if (string.IsNullOrEmpty(PkValue))
        {
            result["error"] = "-1";
            result["msg"] = "操作失败:未传入主键值.";
            Response.Clear();
            Response.Write(VLP.JSON.Encode(result));
            return;
        }
        if (string.IsNullOrEmpty(TypeID))
        {
            result["error"] = "-1";
            result["msg"] = "操作失败:未传入操作类型ID.";
            Response.Clear();
            Response.Write(VLP.JSON.Encode(result));
            return;
        }



        System.Data.SqlClient.SqlCommand sqlcom = new System.Data.SqlClient.SqlCommand();
        sqlcom.CommandText = "sp_File_CreateDataIndex";
        sqlcom.CommandType = System.Data.CommandType.StoredProcedure;
        sqlcom.Parameters.AddWithValue("@PkValue", PkValue);
        sqlcom.Parameters.AddWithValue("@TypeID", TypeID);
        sqlcom.Parameters.AddWithValue("@DBsID", DBsID);
        sqlcom.Parameters.AddWithValue("@UserID", currentUserInfo.ID.ToString());
        System.Data.DataTable dt = DB.ExecuteDataTable(sqlcom);
       
        if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0].ToString().Length > 0)
        {
            if (dt.Rows[0][0].ToString() == "-1")
            {
                result["error"] = "-1";
                result["msg"] = dt.Rows[0][1].ToString();
            }
            else
            {
                dt.Rows[0]["DBConnection"] = VLP.WMS.Common.Encrypt(dt.Rows[0]["DBConnection"].ToString()).Replace("=",":");//对数据库连接字符串进行加密
                string json = VLP.JSON.Encode(VLP.JSON.DataTable2ArrayList(dt));
                Response.Clear();
                Response.Write(json);
                return;
            }
        }
        else
        {
            result["error"] = "-1";
            result["msg"] = "操作失败:未获取到主MainID请联系管理员.";
        }
        Response.Clear();
        Response.Write(VLP.JSON.Encode(result));
    }
}