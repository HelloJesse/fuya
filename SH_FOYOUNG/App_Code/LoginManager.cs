using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nordasoft.Data.Sql;
using System.Data.SqlClient;

/// <summary>
/// LoginManager 的摘要说明
/// </summary>
public class LoginManager
{
	public LoginManager()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
	}

    public static void AddSession(string sessionid,string address,string hostname,DataBase db)
    {
        string sql = @"
IF NOT EXISTS(SELECT 1 FROM dbo.Sys_LOGIN_LOG WHERE SessionID=@SessionID)
BEGIN
	INSERT INTO dbo.Sys_LOGIN_LOG
	        ( SessionID ,
	          Login_Date ,
	          UserHostAddress ,
	          UserHostName
	        )
	VALUES  ( @SessionID , -- SessionID - varchar(50)
	          GETDATE() , -- Login_Date - datetime
	          @UserHostAddress , -- UserHostAddress - varchar(50)
	          @UserHostName  -- UserHostName - nvarchar(50)
	        )
END
";
        SqlCommand cmmd = new SqlCommand(sql);
        cmmd.Parameters.Add(new SqlParameter("@SessionID", System.Data.SqlDbType.VarChar, 50));
        cmmd.Parameters["@SessionID"].Value = sessionid;
        cmmd.Parameters.Add(new SqlParameter("@UserHostAddress", System.Data.SqlDbType.VarChar, 50));
        cmmd.Parameters["@UserHostAddress"].Value = address;
        cmmd.Parameters.Add(new SqlParameter("@UserHostName", System.Data.SqlDbType.NVarChar, 50));
        cmmd.Parameters["@UserHostName"].Value = hostname;
        db.ExecuteNonQuery(cmmd);
    }
    public static void ClearAll(DataBase db)
    {
        SqlCommand cmmd = new SqlCommand("TRUNCATE TABLE Sys_LOGIN_LOG");
        db.ExecuteNonQuery(cmmd);
    }
    public static void DropSession(string sessionID, DataBase db)
    {
        SqlCommand cmmd = new SqlCommand("DELETE Sys_LOGIN_LOG WHERE SessionID=@SessionID");
        cmmd.Parameters.Add(new SqlParameter("@SessionID", System.Data.SqlDbType.VarChar, 50));
        cmmd.Parameters["@SessionID"].Value = sessionID;
        db.ExecuteNonQuery(cmmd);
    }
    public static bool UpdateSessionUserInfo(string sessionID,string userid,string companyName , DataBase db)
    {
        string sql = @"
IF EXISTS(SELECT 1 FROM dbo.Sys_LOGIN_LOG WHERE SessionID=@SessionID)
BEGIN
    UPDATE dbo.Sys_LOGIN_LOG SET
	    USERID=@USERID,
	    CompanyName=@CompanyName,
	    Login_Date=GETDATE()
    WHERE SessionID=@SessionID
    SELECT 1
END
ELSE
BEGIN
    SELECT 0
END
";
        SqlCommand cmmd = new SqlCommand(sql);
        cmmd.Parameters.Add(new SqlParameter("@SessionID", System.Data.SqlDbType.VarChar, 50));
        cmmd.Parameters["@SessionID"].Value = sessionID;
        cmmd.Parameters.Add(new SqlParameter("@CompanyName", System.Data.SqlDbType.VarChar, 50));
        cmmd.Parameters["@CompanyName"].Value = companyName;
        cmmd.Parameters.Add(new SqlParameter("@USERID", System.Data.SqlDbType.Int));
        cmmd.Parameters["@USERID"].Value = userid;
       System.Data.DataTable dt= db.ExecuteDataTable(cmmd);
       return dt.Rows[0][0].ToString() == "1";
    }
}