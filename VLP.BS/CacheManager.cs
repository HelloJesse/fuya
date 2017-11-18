using System;
using System.Collections.Generic;
using System.Web;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace VLP.BS
{
    /// <summary>
    /// 类型名转换为表名
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public delegate string TypeNameToTableNameDelegate(string typeName);
    /// <summary>
    /// 缓存管理类
    /// </summary>
    public class CacheManager
    {
        public CacheManager()
        {
            
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        /// <summary>
        /// 根据类型名转换为表名
        /// </summary>
        public static TypeNameToTableNameDelegate GetTableNameByTypeName;
        /// <summary>
        /// 系统功能表
        /// </summary>
        public static System.Data.DataTable FunctionTable;
        /// <summary>
        /// 页面按钮表
        /// </summary>
        public static System.Data.DataTable PageButtonTable;
        /// <summary>
        /// 是否为管理员
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static bool IsAdmin(string userid)
        {
            if (userid == "1")
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取指定页的按钮信息
        /// </summary>
        /// <param name="pageid"></param>
        /// <returns></returns>
        public static DataRow GetPageButtonRow(string pageid)//, string systemCompany)
        {
            DataRow[] rows = PageButtonTable.Select(string.Format("PageID={0}", pageid));
            if (rows.Length == 0)
            {
                throw new ApplicationException(string.Format("未找到PageID[{0}]对应的按钮信息.", pageid));
            }
            return rows[0];
        }
        /// <summary>
        /// 初始系统相关的缓存
        /// </summary>
        public static void InitCache(string cnnstr)
        {
           string sql = @"
--获取系统按钮信息
SELECT ID,Code, Name,Name_E, ICO,IsCheckPopedom FROM Sys_Function
--获取页面按钮信息
SELECT PageID, dbo.Sys_Module.ModuleID,SearchButtons,EditButtons,FunctionID FROM dbo.Sys_Search_Page
INNER JOIN dbo.Sys_Module ON dbo.Sys_Search_Page.ModuleID = dbo.Sys_Module.ModuleID
";
            Nordasoft.Data.Sql.DataBase db = new Nordasoft.Data.Sql.DataBase(cnnstr);
            DataSet ds = db.ExecuteDataset(new SqlCommand(sql));
            FunctionTable = ds.Tables[0];
            PageButtonTable = ds.Tables[1];
        }


        static DataTable _ComboBoxSQL;

        private static void InitComboBoxSQL()
        {
            _ComboBoxSQL = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter("SELECT ObjectName AS Name,Selectsql AS SQL,OrderBySQL AS OrderBy,TableName AS RelTableName FROM dbo.Sys_ComboBox"
                , System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString);
            da.Fill(_ComboBoxSQL);
        }
        public static void ComboBoxSQLAdd(string tableName, string sql)
        {
            ComboBoxSQLAdd(tableName, sql, string.Empty);
        }
        public static void ComboBoxSQLAdd(string tableName, string sql, string orderBy)
        {
            if (_ComboBoxSQL == null)
            {
                InitComboBoxSQL();
            }
            System.Data.DataRow row = _ComboBoxSQL.NewRow();
            row["Name"] = tableName;
            row["SQL"] = sql;
            row["OrderBy"] = orderBy;
            _ComboBoxSQL.Rows.Add(row);
        }
        private static System.Data.DataRow GetComboBoxSQLRow(string tableName, string reltableName, string language, string orderBy)
        {
            System.Data.DataRow[] rows = _ComboBoxSQL.Select(string.Format("Name='{0}'", tableName));
            if (rows.Length == 0)
            {
                string sql = string.Format(@"
                 SET ROWCOUNT 15;
                 SELECT ID,CODE,NAME{0} AS NAME FROM {1} WHERE Activated=1 AND (CODE LIKE @V OR NAME LIKE @V)
                 ", language, reltableName);

                ComboBoxSQLAdd(tableName, sql, orderBy);
                rows = _ComboBoxSQL.Select(string.Format("Name='{0}'", tableName));
            }
            return rows[0];
        }
        public static EventHandler InitSpecialComboBoxSqlSetting;
        /// <summary>
        /// 初始化特殊下拉查询
        /// </summary>
        private static void InitializeComboxSqlSetting()
        {
            InitComboBoxSQL();
            if (InitSpecialComboBoxSqlSetting != null)
            {
                InitSpecialComboBoxSqlSetting(_ComboBoxSQL,new EventArgs());
            }
        }
        /// <summary>
        /// 获取内存下拉SQL
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="language"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string GetComboBoxSQLSetting(string tablename, string language, System.Web.SessionState.HttpSessionState session)
        {
            if (_ComboBoxSQL == null)
            {
                InitializeComboxSqlSetting();
            }
            if (language == "" || language.Equals("cn"))
            {
                language = "";
            }
            else
            {
                language = string.Format("_{0}", language);
            }

            DataRow[] rows = _ComboBoxSQL.Select(string.Format("Name='{0}'", tablename));
            if (rows.Length == 0)
            {
                throw new ApplicationException(string.Format("未找到[{0}]配置项。", tablename));
            }
            string reltableName = rows[0]["RelTableName"].ToString();
            if (string.IsNullOrEmpty(reltableName))
            {
                reltableName = tablename;
            }

            string viewid = BasePage.ViewBaseID(session, reltableName);
            if (!string.IsNullOrEmpty(viewid))
            {
                viewid = string.Format(" AND ID IN({0})", viewid);
            }
            string setting = string.Format("{0} {1} {2}", string.Format(rows[0]["SQL"].ToString(), language), viewid, rows[0]["OrderBY"]);
            return setting;
        }
        /// <summary>
        /// 获取内存下拉框配置语句
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="language">语言,默认为空、英文为e,根据当前用户语言类型来传入</param>
        /// <returns></returns>
        public static SqlCommand GetComboBoxSQLSetting(string tablename, string language, bool isOderby, System.Web.SessionState.HttpSessionState session)
        {
            return GetComboBoxSQLSetting(tablename, language, isOderby, session, string.Empty);
        }
        /// <summary>
        /// 获取内存下拉框配置语句
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="language">语言,默认为空、英文为e,根据当前用户语言类型来传入</param>
        /// <returns></returns>
        public static SqlCommand GetComboBoxSQLSetting(string tablename, string language, bool isOderby, System.Web.SessionState.HttpSessionState session, string value)
        {
            if (_ComboBoxSQL == null)
            {
                InitializeComboxSqlSetting();
            }
            if (language == "" || language.Equals("cn"))
            {
                language = "";
            }
            else
            {
                language = string.Format("_{0}", language);
            }



            tablename = string.Format("{0}{1}", tablename, language);
            string reltableName = tablename;
            if (GetTableNameByTypeName != null)
            {
                reltableName = GetTableNameByTypeName(tablename);
            }
            
            string viewid = BasePage.ViewBaseID(session, reltableName);
            if (!string.IsNullOrEmpty(viewid))
            {
                viewid = string.Format(" AND ID IN({0})", viewid);
            }
            DataRow row = GetComboBoxSQLRow(tablename, reltableName, language, isOderby ? "ORDER BY OrderBy ASC" : "");

            string setting = string.Format("SET ROWCOUNT 15; {0} {1} {2}", row["SQL"], row["OrderBY"], viewid);

            SqlCommand cmmd = new SqlCommand(setting);
            if (setting.Contains(" @V "))
            {
                cmmd.Parameters.AddWithValue("@V", string.Format("%{0}%", value));//加入参数信息
            }

            return cmmd;
        }


        /// <summary>
        /// 获取内存下拉框配置语句
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="language">语言,默认为空、英文为e,根据当前用户语言类型来传入</param>
        /// <param name="Params">参数集合</param>
        /// <returns></returns>
        public static SqlCommand GetComboBoxSQLSetting(string tablename, string language, string ParamsID, string value)
        {
            if (_ComboBoxSQL == null)
            {
                InitializeComboxSqlSetting();
            }
            DataRow[] rows = _ComboBoxSQL.Select(string.Format("Name='{0}'", tablename));
            if (rows.Length == 0)
            {
                return null;
            }
            string setting = string.Format("{0} {1}", rows[0]["SQL"], rows[0]["OrderBy"]);

            //string setting = _ComboBoxSQLSetting[tablename];
            //if (string.IsNullOrEmpty(setting))
            //{//如果没有配置信息，则返回null
            //    return null;
            //}
            SqlCommand cmmd = new SqlCommand(setting);
            if (value != null)
            {
                cmmd.Parameters.AddWithValue("@V", string.Format("%{0}%", value.Trim().Replace("'", "''")));
            }
            //加入参数信息
            cmmd.Parameters.AddWithValue("@Param", ParamsID);
            return cmmd;
        }



        /// <summary>
        /// 获取实时下拉框配置语句
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="language">语言,默认为空、英文为e,根据当前用户语言类型来传入</param>
        /// <returns></returns>
        public static SqlCommand GetComboBoxSQLSettingTimes(string tablename, string language, string value, System.Web.SessionState.HttpSessionState session)
        {
            if (_ComboBoxSQL == null)
            {
                InitializeComboxSqlSetting();
            }
            if (language == "" || language.Equals("cn"))
            {
                language = "";
            }
            else
            {
                language = string.Format("_{0}", language);
            }
            string reltableName = tablename;
            if (GetTableNameByTypeName != null)
            {
                reltableName = GetTableNameByTypeName(tablename);
            }
            string viewid = BasePage.ViewBaseID(session, reltableName);
            if (!string.IsNullOrEmpty(viewid))
            {
                viewid = string.Format(" AND ID IN({0})", viewid);
            }
            tablename = string.Format("{0}{1}", tablename, language);

            DataRow row = GetComboBoxSQLRow(tablename, reltableName, language, "ORDER BY CODE");

            string setting = string.Format("SET ROWCOUNT 15;{0} {1} {2}", row["SQL"], viewid, row["OrderBY"]);

            //string setting = _ComboBoxSQLSetting[tablename];
            //if (setting == null)
            //{

            //    setting = string.Format(
            //        "SET ROWCOUNT 15; SELECT ID,CODE,NAME{0} FROM {1} WHERE (CODE LIKE @V OR NAME LIKE @V) AND Activated=1 {0}"
            //        , language, reltableName, viewid);
            //    _ComboBoxSQLSetting.Add(tablename, setting);
            //}
            SqlCommand cmmd = new SqlCommand(setting);
            cmmd.Parameters.AddWithValue("@V", string.Format("%{0}%", value));
            return cmmd;
        }
        

    }
}
