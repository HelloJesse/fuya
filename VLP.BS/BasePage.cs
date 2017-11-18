using System;
using System.Collections.Generic;
using System.Web;
using Nordasoft.Data.Sql;

namespace VLP.BS
{
    public class BasePage : System.Web.UI.Page
    {
        /// <summary>
        /// 获取语言版本
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetLanguage(HttpRequest request)
        {
            string language = string.Empty;
            string requesturl = request.Headers["Referer"].ToString();
            if (requesturl.IndexOf("/en/") > 0)
            {
                language = "E";
            }
            return language;
        }

        /// <summary>
        /// 获取语言版本参数 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetLanguageParam(HttpRequest request)
        {
            string language = string.Empty;
            string requesturl = request.Headers["Referer"].ToString();
            if (requesturl.IndexOf("/en/") > 0)
            {
                language = "EN";
            }
            else
            {
                language = "CN";
            }
            return language;
        }
        public BaseUser GetUserInfo
        {
            get
            {
                if (Session["CurrentUserInfo"] == null) return null;
                else
                    return (BaseUser)Session["CurrentUserInfo"];
            }
        }
        public static DataBase GetDB(System.Web.SessionState.HttpSessionState session)
        {
            string cnnstr = session["SystemConnection"].ToString();
            Nordasoft.Data.Sql.DataBase db = new DataBase(cnnstr);
            return db;
        }
        /// <summary>
        /// 设置系统连接字符串
        /// </summary>
        /// <param name="session"></param>
        /// <param name="cnnstr"></param>
        public static void SetSystemConnectionString(System.Web.SessionState.HttpSessionState session,string cnnstr)
        {
            session["SystemConnection"] = cnnstr; 
        }
        public static string GetConnectionString(System.Web.SessionState.HttpSessionState session)
        {
            return session["SystemConnection"].ToString();
        }
        public static BaseUser GetCurrentUser(System.Web.SessionState.HttpSessionState session)
        {
            if (session["CurrentUserInfo"] == null) return null;
            else
                return (BaseUser)session["CurrentUserInfo"];
        }
        public System.Data.DataTable BaseDataPopedom
        {
            get
            {
                return GetBaseDataViewIDTable(this.Session);
            }
        }
        public static System.Data.DataTable GetBaseDataViewIDTable(System.Web.SessionState.HttpSessionState session)
        {
            if (session["BaseDataPopedom"] == null)
            {
                string sql = @"
SELECT [BaseTable]
      ,[IDS]
  FROM [dbo].[Sys_Role_View_BaseDataID] INNER JOIN dbo.Sys_User_Role ON dbo.Sys_Role_View_BaseDataID.RoleID = dbo.Sys_User_Role.RoleID
  WHERE UserID=@UserID AND IDS>''
";
                System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand(sql);
                cmmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
                cmmd.Parameters["@UserID"].Value = GetCurrentUser(session).ID;
                System.Data.DataTable dt = GetDB(session).ExecuteDataTable(cmmd);
                session["BaseDataPopedom"] = dt;
                session["BaseDataPopedom_Time"] = DateTime.Now;
            }
            else if ((DateTime.Now - (DateTime)session["BaseDataPopedom_Time"]).TotalMinutes > 10)
            {
                RefreshBasePopedom(session);
            }
            return (System.Data.DataTable)session["BaseDataPopedom"];
        }

        public static string ViewBaseID(System.Web.SessionState.HttpSessionState session, string tableName)
        {
            System.Data.DataRow[] rows = GetBaseDataViewIDTable(session).Select(string.Format("BaseTable='{0}'", tableName));
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < rows.Length - 1; i++)
            {
                sb.Append(rows[i]["IDS"].ToString());
                sb.Append(",");
            }
            if (rows.Length > 0)
            {
                sb.Append(rows[rows.Length - 1]["IDS"].ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取当前用户可见基础表ID
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string ViewBaseID(string tableName)
        {
            return ViewBaseID(this.Session, tableName);
        }
        /// <summary>
        /// 刷新当前用户可见基础数据
        /// </summary>
        private static void RefreshBasePopedom(System.Web.SessionState.HttpSessionState session)
        {
            string sql = @"
SELECT [BaseTable]
      ,[IDS]
  FROM [dbo].[Sys_Role_View_BaseDataID] INNER JOIN dbo.Sys_User_Role ON dbo.Sys_Role_View_BaseDataID.RoleID = dbo.Sys_User_Role.RoleID
  WHERE UserID=@UserID AND IDS>''
";
            System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand(sql);
            cmmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
            cmmd.Parameters["@UserID"].Value = GetCurrentUser(session).ID;
            System.Data.DataTable dt = GetDB(session).ExecuteDataTable(cmmd);
            session["BaseDataPopedom"] = dt;
            session["BaseDataPopedom_Time"] = DateTime.Now;
        }
        public DataBase DB
        {
            get
            {
                return GetDB(this.Session);
            }
        }
        public string ConnectionString
        {
            get
            {
                return GetConnectionString(this.Session);
            }
        }
        //public static DataBase GetDB(System.Web.SessionState.HttpSessionState session)
        //{
        //    return Common.GetDB(session);
        //}
        /// <summary>
        /// 检查是否登录超时
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsOut(System.Web.HttpContext context)
        {
            VLP.BS.Result result = new VLP.BS.Result();
            if (context.Session["CurrentUserInfo"] == null)
            {
                //context.Server.TransferRequest("login.html");
                context.Response.Write("{\"isOut\":\"1\"}");
                return true;
            }
            return false;
        }
        /// <summary>
        /// 启用GZip的字符长度
        /// </summary>
        public static int GZipLength = 1024;
        /// <summary>
        /// 输出内容,输出内容，系统会自动判断并处理压缩
        /// </summary>
        /// <param name="page">输出的页</param>
        /// <param name="value">待输出的内容</param>
        public static void Write(System.Web.UI.Page page ,string value)
        {
            Write(page.Request, page.Response, value,System.Text.Encoding.UTF8);
            try
            {
                page.Response.End();
            }
            catch
            {
            }
        }
        /// <summary>
        /// 输出内容,输出内容，系统会自动判断并处理压缩
        /// </summary>
        /// <param name="page">输出的页</param>
        /// <param name="value">待输出的内容</param>
        /// <param name="encoding">编码格式</param>
        public static void Write(System.Web.UI.Page page, string value, System.Text.Encoding encoding)
        {
            Write(page.Request, page.Response, value, encoding);
            try
            {
                page.Response.End();
            }
            catch
            {
            }
        }
        /// <summary>
        /// 输出内容,输出内容，系统会自动判断并处理压缩
        /// </summary>
        /// <param name="page">输出的页</param>
        /// <param name="value">待输出的内容</param>
        public static void Write(System.Web.UI.Page page, byte[] value)
        {
            Write(page.Request, page.Response, value);
            try
            {
                page.Response.End();
            }
            catch
            {
            }
        }
        /// <summary>
        /// 输出内容,输出内容，系统会自动判断并处理压缩,默认采用UTF-8编码
        /// </summary>
        /// <param name="page">输出的页</param>
        /// <param name="value">待输出的内容</param>
        public static void Write(System.Web.HttpContext context, string value)
        {
            Write(context.Request, context.Response, value, System.Text.Encoding.UTF8);
        }
        /// <summary>
        /// 输出内容,输出内容，系统会自动判断并处理压缩
        /// </summary>
        /// <param name="context">输出的页</param>
        /// <param name="value">待输出的内容</param>
        public static void Write(System.Web.HttpContext context, byte[] value)
        {
            Write(context.Request, context.Response, value);
        }
        /// <summary>
        /// 输出内容,输出内容，系统会自动判断并处理压缩
        /// </summary>
        /// <param name="page">输出的页</param>
        /// <param name="value">待输出的内容</param>
        /// <param name="encoding">编码格式</param>
        public static void Write(System.Web.HttpContext context, string value, System.Text.Encoding encoding)
        {
            Write(context.Request, context.Response, value,encoding);
        }
        private static void Write(HttpRequest request, HttpResponse response, string value,System.Text.Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(value);
            Write(request, response, bytes);
            //int contentlength = value.Length;
            //if (contentlength >= GZipLength)
            //{
            //    string acceptcode = request.Headers["Accept-Encoding"];
            //    if (acceptcode.IndexOf("gzip") >= 0)
            //    {
            //        byte[] bytes = Zip.GZipCompress(value);
            //        response.Headers["Content-Encoding"] = "gzip";
            //        response.BinaryWrite(bytes);
            //    }
            //}
            //else
            //{
            //    response.Write(value);
            //}
        }
        private static void Write(HttpRequest request, HttpResponse response, byte[] value)
        {
            int contentlength = value.Length;
            if (contentlength >= GZipLength)
            {
                string acceptcode = request.Headers["Accept-Encoding"];
                if (acceptcode.IndexOf("gzip") >= 0)
                {
                    byte[] bytes = Zip.GZipCompress(value);
                    response.Headers["Content-Encoding"] = "gzip";
                    //response.Headers["Cache-Control"] = "no-cache"; //指定不需要缓存
                    response.BinaryWrite(bytes);
                }
            }
            else
            {
                response.BinaryWrite(value);
            }
        }
    }
}
