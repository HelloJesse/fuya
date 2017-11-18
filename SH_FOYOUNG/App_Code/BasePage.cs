using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nordasoft.Data.Sql;

/// <summary>
/// BasePage 的摘要说明
/// </summary>
public class BasePage : VLP.BS.BasePage
{

	public BasePage()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
	}
    public UserInfo GetUserInfo
    {
        get { return (UserInfo)base.GetUserInfo; }
    }

//    public UserInfo GetUserInfo
//    {
//        get
//        {
//            if (Session["CurrentUserInfo"] == null) return null;
//            else
//                return (UserInfo)Session["CurrentUserInfo"];
//        }
//    }
//    public static UserInfo GetCurrentUser(System.Web.SessionState.HttpSessionState session)
//    {
//        if (session["CurrentUserInfo"] == null) return null;
//        else
//            return (UserInfo)session["CurrentUserInfo"];
//    }
//    public System.Data.DataTable BaseDataPopedom
//    {
//        get
//        {
//            return GetBaseDataViewIDTable(this.Session);
//        }
//    }
//    public static System.Data.DataTable GetBaseDataViewIDTable(System.Web.SessionState.HttpSessionState session)
//    {
//        if (session["BaseDataPopedom"] == null)
//        {
//            string sql = @"
//SELECT [BaseTable]
//      ,[IDS]
//  FROM [dbo].[Sys_Role_View_BaseDataID] INNER JOIN dbo.Sys_User_Role ON dbo.Sys_Role_View_BaseDataID.RoleID = dbo.Sys_User_Role.RoleID
//  WHERE UserID=@UserID AND IDS>''
//";
//            System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand(sql);
//            cmmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
//            cmmd.Parameters["@UserID"].Value = GetCurrentUser(session).ID;
//            System.Data.DataTable dt =Common.GetDB(session).ExecuteDataTable(cmmd);
//            session["BaseDataPopedom"] = dt;
//            session["BaseDataPopedom_Time"] = DateTime.Now;
//        }
//        else if ((DateTime.Now - (DateTime)session["BaseDataPopedom_Time"]).TotalMinutes > 10)
//        {
//            RefreshBasePopedom(session);
//        }
//        return (System.Data.DataTable)session["BaseDataPopedom"];
//    }

//    public static string ViewBaseID(System.Web.SessionState.HttpSessionState session, string tableName)
//    {
//        System.Data.DataRow[] rows = GetBaseDataViewIDTable(session).Select(string.Format("BaseTable='{0}'", tableName));
//        System.Text.StringBuilder sb = new System.Text.StringBuilder();
//        for (int i = 0; i < rows.Length - 1; i++)
//        {
//            sb.Append(rows[i]["IDS"].ToString());
//            sb.Append(",");
//        }
//        if (rows.Length > 0)
//        {
//            sb.Append(rows[rows.Length - 1]["IDS"].ToString());
//        }
//        return sb.ToString();
//    }

//    /// <summary>
//    /// 获取当前用户可见基础表ID
//    /// </summary>
//    /// <param name="tableName"></param>
//    /// <returns></returns>
//    public string ViewBaseID(string tableName)
//    {
//        return ViewBaseID(this.Session, tableName);
//    }
//    /// <summary>
//    /// 刷新当前用户可见基础数据
//    /// </summary>
//    private static void RefreshBasePopedom(System.Web.SessionState.HttpSessionState session)
//    {
//        string sql = @"
//SELECT [BaseTable]
//      ,[IDS]
//  FROM [dbo].[Sys_Role_View_BaseDataID] INNER JOIN dbo.Sys_User_Role ON dbo.Sys_Role_View_BaseDataID.RoleID = dbo.Sys_User_Role.RoleID
//  WHERE UserID=@UserID AND IDS>''
//";
//        System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand(sql);
//        cmmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
//        cmmd.Parameters["@UserID"].Value = GetCurrentUser(session).ID;
//        System.Data.DataTable dt =GetDB(session).ExecuteDataTable(cmmd);
//        session["BaseDataPopedom"] = dt;
//        session["BaseDataPopedom_Time"] = DateTime.Now;
//    }
//    public DataBase DB
//    {
//        get
//        {
//            return Common.GetDB(this.Session);
//        }
//    }
//    public string ConnectionString
//    {
//        get
//        {
//            return Common.GetConnectionString(this.Session);
//        }
//    }
//    public static DataBase GetDB(System.Web.SessionState.HttpSessionState session)
//    {
//        return Common.GetDB(session);
//    }
//    /// <summary>
//    /// 检查是否登录超时
//    /// </summary>
//    /// <param name="context"></param>
//    /// <returns></returns>
//    public static bool IsOut(System.Web.HttpContext context)
//    {
//        VLP.BS.Result result = new VLP.BS.Result();
//        if( context.Session["CurrentUserInfo"]==null)
//        {
//            //context.Server.TransferRequest("login.html");
//            context.Response.Write("{\"isOut\":\"1\"}");
//            return true;
//        }
//        return false;
//    }
}


public class UserInfo:VLP.BS.BaseUser
{
    //public int? ID
    //{
    //    get;
    //    set;
    //}
    //public string Code
    //{
    //    get;
    //    set;
    //}
    //public string Name
    //{
    //    get;
    //    set;
    //}
    //public string Name_E
    //{
    //    get;
    //    set;
    //}
    public int? USER_TYPE
    {
        get;
        set;
    }
    public int? CompanyID
    {
        get;
        set;
    }
    public int? DepartmentID
    {
        get;
        set;
    }
    public string TELE
    {
        get;
        set;
    }
    public string FAX
    {
        get;
        set;
    }
    public string MOBILE
    {
        get;
        set;
    }
    public string QQ
    {
        get;
        set;
    }
    public string WinXin
    {
        get;
        set;
    }
    public string EMAIL
    {
        get;
        set;
    }
    public string ADDRESS
    {
        get;
        set;
    }
    public string UserLan
    {
        get;
        set;
    }
    /// <summary>
    /// 系统级的公司名称
    /// </summary>
    public string SystemCompanyName     
    {
        get;
        set;
    }
    /// <summary>
    /// 此系统是否进行客户与货物的关联限制
    /// </summary>
    public bool IsCustomerGoods
    {
        get;
        set;
    }
    /// <summary>
    /// 用户所属客户
    /// </summary>
    public string CustomerName
    {
        get;
        set; 
    }
}