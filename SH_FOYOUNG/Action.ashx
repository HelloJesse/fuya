<%@ WebHandler Language="C#" Class="Action" %>

using System;
using System.Web;
using System.Data.SqlClient;
using VLP.BS;
using VLP.Search;
using System.Web.SessionState;

public class Action : BasePage, IHttpHandler, IReadOnlySessionState 
{

    public void ProcessRequest(HttpContext context)
    {
        //context.Response.ContentType = "text/plain";
        Result result = new Result();
        UserInfo userinfo = base.GetUserInfo;
        //if (userinfo == null)
        //{
        //    //context.Server.TransferRequest("login.html");
        //    context.Response.Write("{\"isOut\":\"1\"}"); 
        //    return;
        //}
        //超时直接退出
        if (BasePage.IsOut(context)) return;
        
        string userid = userinfo.ID.ToString();
        string empid = userinfo.ID.ToString();
        string method = context.Request.Params["method"];
        string tableid = context.Request.Params["id"];
        String submitJSON = context.Request["submitData"];
        
        
        if (method == "s")
        {
            int pageid_int = 0;
            if (int.TryParse(tableid, out pageid_int) == false)
            {
                result.ErrMessage = "系统参数不正确.";
            }
            else
            {
                System.Collections.Hashtable data = (System.Collections.Hashtable)VLP.JSON.Decode(submitJSON);
                result = AutoCommandManager.DoCommand(tableid.ToString(), data, userid, empid,DB.GetConnection().ConnectionString);
            }
            context.Response.Write(VLP.JSON.Encode(result));     
        }
        
    }

   

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
    
}

#region//自动保存命令相关方法
///// <summary>
///// 自动命令
///// </summary>
//public class AutoCommand
//{
//    public string Key = string.Empty;
//    public string PKName = string.Empty;
//    public SqlCommand InsertCommand = null;
//    public SqlCommand UpdateCommand = null;
//}

//public class AutoCommands : System.Collections.ObjectModel.Collection<AutoCommand>
//{
//    public AutoCommand GetCommand(string key)
//    {
//        foreach (AutoCommand t in this.Items)
//        {
//            if (t.Key.Equals(key))
//                return t;
//        }
//        return null;
//    }
//}
//public class Result
//{
//    public string PKName = string.Empty;
//    public string PKValue = string.Empty;
//    public string NoValue = string.Empty;
//    public bool IsOK = false;
//    public string ErrMessage = string.Empty;
//}

//public class AutoCommandManager
//{
//    public static string _CNN = DB.GetConnection().ConnectionString;// string.Empty;
//    public static AutoCommands TableAutoCommands = null;
//    public AutoCommandManager(string connstring)
//    {
//        _CNN = connstring;
//        TableAutoCommands = new AutoCommands();
//    }

//    public static Result DoCommand(string cmmdkey, System.Collections.Hashtable data, string userid, string empid)
//    {
//        Result result = new Result();
//        result.IsOK = false;
//        if (TableAutoCommands == null)
//        {
//            InitCommand();
//        }
//        AutoCommand cmmd = TableAutoCommands.GetCommand(cmmdkey);

//        if (cmmd == null)
//        {
//            result.ErrMessage = string.Format("未找到[{0}]对应的命令", cmmdkey);
//            return result;
//        }
//        //判断是否为新增
//        if (!data.ContainsKey(cmmd.PKName))
//        {
//            result.ErrMessage = "数据与命令不一致";
//            return result;
//        }
//        string pkvalue = data[cmmd.PKName].ToString();
//        SqlCommand dbcommd = cmmd.UpdateCommand;
//        if (string.IsNullOrEmpty(pkvalue) || pkvalue == "0")
//        {
//            dbcommd = cmmd.InsertCommand;
//        }
//        //邦定参数
//        foreach (SqlParameter sp in dbcommd.Parameters)
//        {
//            string key = sp.ParameterName.Substring(1);
//            if (data.ContainsKey(key))
//            {
//                if (data[key] == null || data[key].ToString().Length == 0)
//                {
//                    sp.Value = DBNull.Value; 
//                }
//                else
//                {
//                    sp.Value = data[key];
//                }
//            }
            
//        }
//        //设置当前登录人参数值
//        dbcommd.Parameters["@USERID"].Value=userid;
//        dbcommd.Parameters["@EMPID"].Value = empid;
        
//        try
//        {
//            using (SqlConnection cnn = new SqlConnection(_CNN))
//            {
//                dbcommd.Connection = cnn;
//                SqlDataAdapter da = new SqlDataAdapter(dbcommd);
//                System.Data.DataTable dt = new System.Data.DataTable();
//                da.Fill(dt);
//                if (dt.Rows.Count == 1)
//                {
//                    result.IsOK = true;
//                    result.PKValue = dt.Rows[0][0].ToString();
//                }
//            }
//        }
//        catch (Exception err)
//        {
//            result.ErrMessage = err.Message;
//        }
//        return result;
//    }

//    public static void InitCommand()
//    {
//        try
//        {
//            TableAutoCommands = new AutoCommands();
//            SqlConnection CNN = new SqlConnection(_CNN);
//            SqlCommand cmmd = new SqlCommand(
//                @"SELECT ID,PKName,TableName,InsertCommand,InsertCommandP,UpdateCommand,UpdateCommandP FROM [dbo].[Sys_Table_Info] 
//            WHERE InsertCommand IS NOT NULL AND UpdateCommand IS NOT NULL;
//SELECT PageID,SearchKey,DBSQL FROM Sys_Search_Page_WhereConfig
//");
//            cmmd.Connection = CNN;
//            SqlDataAdapter da = new SqlDataAdapter(cmmd);
//            System.Data.DataSet ds = new System.Data.DataSet();
//            da.Fill(ds);
//            System.Data.DataTable dt = new System.Data.DataTable();
//            dt = ds.Tables[0];
//            foreach (System.Data.DataRow row in dt.Rows)
//            {
//                AutoCommand autocmmd = new AutoCommand();
//                autocmmd.Key = row["ID"].ToString();
//                autocmmd.PKName = row["PKName"].ToString();
//                autocmmd.InsertCommand = GetCommand(row["InsertCommand"].ToString(), row["InsertCommandP"].ToString());
//                autocmmd.UpdateCommand = GetCommand(row["UpdateCommand"].ToString(), row["UpdateCommandP"].ToString());
//                //添加USERID参数
//                AddUserIDParameter(autocmmd.InsertCommand);
//                AddUserIDParameter(autocmmd.UpdateCommand);
//                TableAutoCommands.Add(autocmmd);
//            }
            
//        }
//        catch (Exception ERR)
//        {
//            Console.WriteLine(ERR.ToString());
//        }
//    }
    
    
    
//    private static SqlCommand GetCommand(string cmmdtext, string paras)
//    {
//        SqlCommand cmmd = new SqlCommand(cmmdtext);
//        //解析参数
//        string[] ps = paras.TrimEnd(';').Split(';');
//        foreach (string p in ps)
//        {

//            string[] pconfig = p.Split(',');
//            if (pconfig.Length > 1)
//            {
//                string pname = pconfig[0];
//                System.Data.SqlDbType dbtype = GetSqlDbType(pconfig[1]);

//                SqlParameter sp = new SqlParameter(pname, dbtype);
//                if (pconfig[1].IndexOf("char") >= 0)
//                {
//                    sp.Size = int.Parse(pconfig[2]);
//                }
//                sp.Precision = byte.Parse(pconfig[3]);
//                if (pconfig.Length == 5 && pconfig[4].Length > 0)
//                {
//                    //说明有值
//                    sp.Value = ManagerDBDefaultValue(pconfig[4]);
//                }
//                cmmd.Parameters.Add(sp);
//            }
//        }
//        ////插入主键ID与操作人信息
//        //SqlParameter spID = new SqlParameter("@ID", System.Data.SqlDbType.Int);
//        //SqlParameter spUSERID = new SqlParameter("@USERID", System.Data.SqlDbType.Int);
//        //SqlParameter spEMPID = new SqlParameter("@EMPID", System.Data.SqlDbType.Int);
//        //cmmd.Parameters.Add(spID);
//        //cmmd.Parameters.Add(spUSERID);
//        //cmmd.Parameters.Add(spEMPID); ;
//        return cmmd;
//    }

//    private static void AddUserIDParameter(SqlCommand cmmd)
//    {
//        cmmd.Parameters.Add(new SqlParameter("@USERID", System.Data.SqlDbType.Int));
//        cmmd.Parameters.Add(new SqlParameter("@EMPID", System.Data.SqlDbType.Int));
//    }
        

//    private static System.Data.SqlDbType GetSqlDbType(string dbtype)
//    {
//        string[] names = Enum.GetNames(typeof(System.Data.SqlDbType));
//        foreach (string n in names)
//        {
//            if (n.Equals(dbtype, StringComparison.CurrentCultureIgnoreCase))
//            {
//                return (System.Data.SqlDbType)Enum.Parse(typeof(System.Data.SqlDbType), n);
//            }
//        }
//        return System.Data.SqlDbType.Variant;
//    }

//    private static string ManagerDBDefaultValue(string defaultvalue)
//    {
//        if (defaultvalue.Length > 2)
//        {

//            if (defaultvalue.Substring(0, 1) == "(" && defaultvalue.Substring(defaultvalue.Length - 1, 1) == ")")
//            {
//                defaultvalue = defaultvalue.Substring(1, defaultvalue.Length - 2);
//            }
//            else if (defaultvalue.Substring(0, 1) == "'" && defaultvalue.Substring(defaultvalue.Length - 1, 1) == "'")
//            {
//                defaultvalue = defaultvalue.Substring(1, defaultvalue.Length - 2);
//            }
//            else
//            {
//                if (defaultvalue.Equals("getdate()", StringComparison.CurrentCultureIgnoreCase))
//                    return string.Empty;
//                else
//                    return defaultvalue;
//            }
//            return ManagerDBDefaultValue(defaultvalue);
//        }
//        return defaultvalue;
//    }
//}
#endregion
