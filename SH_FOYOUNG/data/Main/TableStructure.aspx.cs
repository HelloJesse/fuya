using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class data_Main_TableStructure : BasePage
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

    public void GetBaseTableByMenu()
    {
        Hashtable result = new Hashtable();
        string code = Request.Form.Get("Code");
        try
        {
            if (string.IsNullOrEmpty(code))
            {
                code = "";
            }
            string sql = @" SELECT S.PageID as id,S.ModuleID,S.PageName,S.MainTable+'('+CASE WHEN ISNULL(T.Remark,'')='' THEN S.MainTable ELSE T.Remark END+')' as text FROM dbo.Sys_Search_Page AS S
LEFT JOIN dbo.Sys_Table_Info AS T ON S.MainTable=T.TableName
WHERE ModuleID IN (SELECT ModuleID FROM dbo.Sys_SystemMenu_Dtl WHERE Code=@Code)";
            SqlCommand cmmd = new SqlCommand();
            cmmd.CommandType = CommandType.Text;
            cmmd.CommandText = sql;
            cmmd.Parameters.Add("@Code",SqlDbType.VarChar,100);
            cmmd.Parameters["@Code"].Value = code;
            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt.Rows.Count > 0)
            {
                result["error"] = 0;
                result["msg"] = VLP.JSON.Encode(dt);
            }
            else
            {
                result["error"] = 1;
                result["msg"] = "未找到该节点信息";
            }
        }
        catch (Exception ex)
        {
            result["error"] = 1;
            result["msg"] = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    public void GetBaseTableInfo()
    {
        Hashtable result = new Hashtable();
        string PageId = Request.Form.Get("PageId");
        try
        {
            if (string.IsNullOrEmpty(PageId))
            {
                PageId = "0";
            }
            string sql = @"SELECT 'M' as id,P.MainTable+'([主表]'+CASE WHEN ISNULL(T.Remark,'')='' THEN P.MainTable ELSE T.Remark END +')' as text FROM dbo.Sys_Search_Page AS P 
LEFT JOIN dbo.Sys_Table_Info AS T ON P.MainTable=T.TableName WHERE PageID=@PageID 
UNION ALL
SELECT ShortName ,SubTable+'('+CASE WHEN ISNULL(T.Remark,'')='' THEN s.SubTable ELSE T.Remark END +')'  FROM dbo.Sys_Search_Page_Dtl AS S LEFT JOIN
dbo.Sys_Table_Info AS T ON S.SubTable=T.TableName
 WHERE PageID=@PageID";
            SqlCommand cmmd = new SqlCommand();
            cmmd.CommandType = CommandType.Text;
            cmmd.CommandText = sql;
            cmmd.Parameters.Add("@PageID",SqlDbType.Int);
            cmmd.Parameters["@PageID"].Value = PageId;
            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt!=null&&dt.Rows.Count > 0)
            {
                result["error"] = 0;
                result["SubTable"] = VLP.JSON.Encode(dt);
            }
            else
            {
                result["error"] = 1;
                result["msg"] = "未找到有效数据";
            }
        }
        catch (Exception ex)
        {
            result["error"] = 1;
            result["msg"] = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    public void GetSubTableInfo()
    {
        var pageid = Request.Form.Get("PageID");
        var shortname = Request.Form.Get("ShortName");
        Hashtable result = new Hashtable();
        try
        {
            if (string.IsNullOrEmpty(pageid)){
                pageid = "0";
            }
            if(string.IsNullOrEmpty(shortname)){
                shortname = "";
            }
            string sql = @"SELECT JOINSQL FROM dbo.Sys_Search_Page_Dtl WHERE PageID=@PageID AND ShortName=@ShortName;
SELECT M.TypeName,M.name,M.length,ISNULL(S.ShowText,M.name) AS ShowText FROM (select st.name as 'TypeName',sc.name,sc.length,sc.id from syscolumns sc,systypes st 
where sc.xtype=st.xtype and st.status=0 AND sc.id in(select id from sysobjects where (xtype='U' or xtype='V') and 
name IN (SELECT SubTable FROM dbo.Sys_Search_Page_Dtl WHERE PageID=@PageID AND ShortName=@ShortName))
 ) AS M  LEFT JOIN dbo.Sys_Search_Page_ShowField AS S ON M.name=S.FieldName AND S.PageID=@PageID AND S.ShortName=@ShortName ORDER BY M.id";
            if (shortname=="M")
            {
                sql = @"SELECT '' as JOINSQL;
SELECT M.TypeName,M.name,M.length,ISNULL(S.ShowText,M.name) AS ShowText FROM (select st.name as 'TypeName',sc.name,sc.length,sc.id from syscolumns sc,systypes st 
where sc.xtype=st.xtype and st.status=0 AND sc.id in(select id from sysobjects where (xtype='U' or xtype='V') and 
name IN (SELECT MainTable FROM dbo.Sys_Search_Page WHERE PageID=@PageID )) 
 ) AS M  LEFT JOIN dbo.Sys_Search_Page_ShowField AS S ON M.name=S.FieldName AND S.PageID=@PageID AND S.ShortName=@ShortName ORDER BY M.id";
            }
            SqlCommand cmmd = new SqlCommand();
            cmmd.CommandType = CommandType.Text;
            cmmd.CommandText = sql;
            cmmd.Parameters.Add("@PageID",SqlDbType.Int);
            cmmd.Parameters["@PageID"].Value = pageid;
            cmmd.Parameters.Add("@ShortName",SqlDbType.VarChar,20);
            cmmd.Parameters["@ShortName"].Value = shortname;
            DataSet ds = DB.ExecuteDataset(cmmd);
            if (ds!=null&&ds.Tables.Count>0)
            {
                result["error"] = 0;
                if (ds.Tables[0].Rows.Count > 0)
                {
                    result["relation"] = ds.Tables[0].Rows[0]["JOINSQL"].ToString();
                }
                if (ds.Tables.Count > 1)
                {
                    result["msg"] = VLP.JSON.Encode(ds.Tables[1]);
                }
            }
            else
            {
                result["error"] = 1;
                result["msg"] = "未找到有效字段";
            }
        }
        catch (Exception ex)
        {
            result["error"] = 1;
            result["msg"] = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }
}