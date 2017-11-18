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

public partial class data_Base_EditDSystemCode : BasePage
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
    /// 获取系统代码明细
    /// </summary>
    public void SearchSystemDtl()
    {
        //查询条件
        string ParentID = Request.Form.Get("ParentID");

        if (string.IsNullOrEmpty(ParentID))
        {
            return;
        }
       
        SqlCommand cmd = new SqlCommand();
        string sql = "SELECT * FROM D_SystemCode_Dtl WHERE ParentID=@ParentID ORDER BY OrderBy";
        cmd.Parameters.Add("@ParentID", SqlDbType.SmallInt);
        cmd.Parameters["@ParentID"].Value = ParentID;
        cmd.CommandText = sql;
        DataTable dt = DB.ExecuteDataTable(cmd);

        if (dt == null)
        {
            return;
        }
        string json = VLP.JSON.Encode(VLP.JSON.DataTable2ArrayList(dt));
        Response.Write(json);
    }

    /// <summary>
    /// 默认查询所有系统代码
    /// </summary>
    public void SearchAllSystemData()
    {
        SqlCommand cmd = new SqlCommand();
        string sql = @"SELECT M.*,U.NAME AS CREATE_NAME,X.NAME AS UPDATE_NAME FROM D_SystemCode AS M
LEFT JOIN dbo.Sys_D_USER AS U ON U.ID=M.CREATE_BY
LEFT JOIN dbo.Sys_D_USER AS X ON X.ID=M.UPDATE_BY";
        cmd.CommandText = sql;
        DataTable dt = DB.ExecuteDataTable(cmd);
        if (dt == null)
        {
            return;
        }
        string json = VLP.JSON.Encode(VLP.JSON.DataTable2ArrayList(dt));
        Response.Write(json);
    }

    /// <summary>
    /// 获取系统代码
    /// </summary>
    public void SearchSystemData()
    {
        //查询条件
        string ID = Request.Form.Get("ID");

        if (string.IsNullOrEmpty(ID))
        {
            return;
        }
        //分页
        int pageIndex = Convert.ToInt32(Request.Form.Get("pageIndex"));
        int pageSize = Convert.ToInt32(Request.Form.Get("pageSize"));
        //字段排序
        string sortField = Request.Form.Get("sortField");
        string sortOrder = Request.Form.Get("sortOrder");

        SqlCommand cmd = new SqlCommand();
        string sql = "SELECT  U.NAME AS CREATE_NAME, X.NAME AS UPDATE_NAME,M.* FROM D_SystemCode AS M LEFT JOIN Sys_D_USER AS U ON M.CREATE_BY=U.ID LEFT JOIN Sys_D_USER AS X ON M.UPDATE_BY=X.ID WHERE M.ID=@ID";
        cmd.Parameters.Add("@ID", SqlDbType.SmallInt);
        cmd.Parameters["@ID"].Value = ID;
        cmd.CommandText = sql;
        DataTable dt = DB.ExecuteDataTable(cmd);
        if (dt == null)
        {
            return;
        }
        string json = VLP.JSON.Encode(VLP.JSON.DataTable2ArrayList(dt));
        Response.Write(json.Substring(1, json.Length - 2));
    }


    /// <summary>
    /// 保存系统代码明细
    /// </summary>
    public void SaveSystemDtl()
    {
        System.Collections.Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "true";//保存成功，返回true

        string json = Request.Form.Get("data");
        string parentID = Request.Form.Get("parentID");
        ArrayList rows = (ArrayList)VLP.JSON.Decode(json);
        Collection<SqlCommand> cmmds = new Collection<SqlCommand>();

        try
        {
            foreach (Hashtable row in rows)
            {
                String id = row["ID"] != null ? row["ID"].ToString() : "";
                //根据记录状态，进行不同的增加、修改操作
                String state = row["_state"] != null ? row["_state"].ToString() : "";
                if (state == "added" || id == "")           //新增：id为空，或_state为added
                {
                    cmmds.Add(UpdatSystemDetailInfo(row, parentID, "0"));
                }
                else if (state == "modified" || state == "") //更新：_state为空或modified
                {
                    cmmds.Add(UpdatSystemDetailInfo(row, parentID, "1"));
                }
                else if (state == "removed" || state == "deleted")
                {
                    cmmds.Add(UpdatSystemDetailInfo(row, parentID, "2"));
                }
            }

            string strResult = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
            if (string.IsNullOrEmpty(strResult))
            {
                result["msg"] = "true";
            }
            else
            {
                result["msg"] = strResult;
            }

        }
        catch (Exception ex)
        {
            result["msg"] = ex.Message;
        }
        json = VLP.JSON.Encode(result);
        Response.Write(json);
       
    }

    /// <summary>
    /// 保存系统代码明细
    /// </summary>
    /// <param name="row">行数据 </param>
    /// <param name="parentID">主表ID </param>
    /// <param name="flag">0新增 1修改</param>
    public SqlCommand UpdatSystemDetailInfo(Hashtable row, string parentID, string flag)
    {
      
            SqlCommand cmmd = new SqlCommand("sp_UpdateSystemCode_Dtl");
            cmmd.CommandType = CommandType.StoredProcedure;
            cmmd.Parameters.Add("@ID", SqlDbType.Int);
            

            if (flag == "0")
            {
                cmmd.Parameters["@ID"].Value = 0 ;
            }
            else
            {
                cmmd.Parameters["@ID"].Value = Convert.ToInt32(row["ID"].ToString());
            }

            cmmd.Parameters.Add("@ParentID", SqlDbType.Int);
            cmmd.Parameters["@ParentID"].Value = Convert.ToInt32(parentID);

            cmmd.Parameters.Add("@Code", SqlDbType.Int);
            cmmd.Parameters["@Code"].Value = (row["Code"] == null || row["Code"].ToString() == "") ? 0 : Convert.ToInt32(row["Code"].ToString()); 

            cmmd.Parameters.Add("@Name", SqlDbType.NVarChar, 20);
            cmmd.Parameters["@Name"].Value = (row["Name"] == null || row["Name"].ToString() == "") ? "" : row["Name"].ToString(); 


            cmmd.Parameters.Add("@Name_E", SqlDbType.VarChar, 50);
            cmmd.Parameters["@Name_E"].Value = (row["Name_E"] == null || row["Name_E"].ToString() == "") ? "" : row["Name_E"].ToString(); 

            cmmd.Parameters.Add("@OrderBy", SqlDbType.Int);
            cmmd.Parameters["@OrderBy"].Value = (row["OrderBy"] == null || row["OrderBy"].ToString() == "") ? 0 : Convert.ToInt32(row["OrderBy"].ToString()); 

            cmmd.Parameters.Add("@Activated", SqlDbType.Int);
            cmmd.Parameters["@Activated"].Value = (row["Activated"] == null || row["Activated"].ToString() == "") ? 0 :((row["Activated"].ToString().ToUpper()=="TRUE" || row["Activated"].ToString().ToUpper()=="1")?1:0);

            cmmd.Parameters.Add("@Remark", SqlDbType.VarChar,50);
            cmmd.Parameters["@Remark"].Value = (row["Remark"] == null || row["Remark"].ToString() == "") ? "" : row["Remark"].ToString();

            cmmd.Parameters.Add("@flag", SqlDbType.NVarChar, 2);
            cmmd.Parameters["@flag"].Value = flag; 
            

            return cmmd;
                       
      

        
        
    }

    /// <summary>
    ///  检查CODE和NAME是否已经存在
    /// </summary>
    public void CheckCodeAndName()
    {
        string code = Request.Form.Get("CODE");
        string name = Request.Form.Get("NAME");

        if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(name))
        {
            return;
        }
        bool boolFlag = false;
        string sql = string.Empty;
        Hashtable result = new Hashtable();
        SqlCommand cmmd = new SqlCommand();
        result["msg"] = "true";
        
        try
        {
            if (!string.IsNullOrEmpty(code))
            {
                sql = "select Code,Name from D_SystemCode where Code=@Code ";
                cmmd.Parameters.Clear();
                cmmd.Parameters.Add("@Code", SqlDbType.VarChar, 20);
                cmmd.Parameters["@Code"].Value = code;
                cmmd.CommandText = sql;
                DataTable dt = DB.ExecuteDataTable(cmmd);
                if (dt != null && dt.Rows.Count > 0)
                {
                    boolFlag = true;
                    result["msg"] = "编码已经存在，请重新输入";
                }
            }

            if (!string.IsNullOrEmpty(name) && boolFlag == false)
            {
                sql = "select Code,Name from D_SystemCode where Name=@Name ";
                cmmd.Parameters.Clear();
                cmmd.Parameters.Add("@Name", SqlDbType.NVarChar, 20);
                cmmd.Parameters["@Name"].Value = name;
                cmmd.CommandText = sql;
                DataTable dt2 = DB.ExecuteDataTable(cmmd);
                if (dt2 != null && dt2.Rows.Count > 0)
                {
                    result["msg"] = "中文名称已经存在，请重新输入";
                }
            }
            
        
        }
        catch (Exception ex)
        {
            result["msg"] = ex.Message;
        }
        String json = VLP.JSON.Encode(result);
        Response.Write(json);

    }

    /// <summary>
    /// 删除 系统代码
    /// </summary>
    public void DeleteSystemInfo()
    {
        string Ids = Request.Form.Get("Ids");
        if (string.IsNullOrEmpty(Ids) )
        {
            return;
        }
        SqlCommand cmmd = new SqlCommand();
        string sql = " EXEC sp_DeleteSystemInfo @Ids";
        cmmd.Parameters.Add("@Ids", SqlDbType.VarChar,-1);
        cmmd.Parameters["@Ids"].Value = Ids;

        Hashtable result = new Hashtable();
        try
        {
            cmmd.CommandText = sql;
            DB.ExecuteNonQuery(cmmd);
            result["msg"] = "true";
        }
        catch (Exception ex)
        {
            result["msg"] = ex.Message;
        }
        String json = VLP.JSON.Encode(result);
        Response.Write(json);
    }
}