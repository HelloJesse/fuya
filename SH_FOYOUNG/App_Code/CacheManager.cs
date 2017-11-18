using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

public delegate string GetCurrentCompany(System.Web.HttpContext context);

/// <summary>
/// CacheManager 的摘要说明
/// </summary>
public class CacheManager
{
    public CacheManager()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }
    //public static GetCurrentCompany GetCompany;
    /// <summary>
    /// 不需要检查权限的按钮
    /// </summary>
    //static System.Collections.Specialized.StringDictionary _NoCheckPopedomButtons;
    //static TableCaches _NoCheckPopedomButtons;
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
    public static DataRow GetPageButtonRow(string pageid, string systemCompany)
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
        //        string systemCompany = Common.GetSysCompany(session);

        //        //检查是否初始化
        //        if (_FunctionTables == null || _FunctionTables[systemCompany] == null)
        //        {
        //            string sql = @"
        //--获取系统按钮信息
        //SELECT ID,Code, Name, ICO,IsCheckPopedom FROM Sys_Function
        //--获取页面按钮信息
        //SELECT PageID, dbo.Sys_Module.ModuleID,SearchButtons,EditButtons,FunctionID FROM dbo.Sys_Search_Page
        //INNER JOIN dbo.Sys_Module ON dbo.Sys_Search_Page.ModuleID = dbo.Sys_Module.ModuleID
        //";
        //            DataSet ds = Common.GetDB(session).ExecuteDataset(new SqlCommand(sql));
        //            if (_FunctionTables == null)
        //            {
        //                _FunctionTables = new TableCaches();
        //                _PageButtonTable = new TableCaches();
        //            }
        //            _FunctionTables.Add(new TableCache(systemCompany,ds.Tables[0]));
        //            _PageButtonTable.Add(new TableCache(systemCompany, ds.Tables[1]));
        //            //FunctionTable = ds.Tables[0];
        //            //PageButtonTable = ds.Tables[1];
        //        }
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

    //static NameValueCollection _ComboBoxSQLSetting;
    static DataTable _ComboBoxSQL;

    private static void InitComboBoxSQL()
    {
        _ComboBoxSQL = new DataTable();
        SqlDataAdapter da = new SqlDataAdapter("SELECT ObjectName AS Name,Selectsql AS SQL,OrderBySQL AS OrderBy,TableName AS RelTableName FROM dbo.Sys_ComboBox", System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString);
        da.Fill(_ComboBoxSQL);
        //_ComboBoxSQL.Columns.Add("Name", typeof(string));
        //_ComboBoxSQL.Columns.Add("SQL", typeof(string));
        //_ComboBoxSQL.Columns.Add("OrderBy", typeof(string));
    }
    private static void ComboBoxSQLAdd(string tableName, string sql)
    {
        ComboBoxSQLAdd(tableName, sql, string.Empty);
    }
    private static void ComboBoxSQLAdd(string tableName, string sql, string orderBy)
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
//            string sql = string.Format(@"
//                 SET ROWCOUNT 15;
//                 SELECT ID,CODE,NAME{0} AS NAME FROM {1} WHERE Activated=1 AND (CODE LIKE @V OR NAME LIKE @V)
//                 SET ROWCOUNT 0;", language, reltableName);

            string sql = string.Format(@"
                 SET ROWCOUNT 15;
                 SELECT ID,CODE,NAME{0} AS NAME FROM {1} WHERE Activated=1 AND (CODE LIKE @V OR NAME LIKE @V)
                 ", language, reltableName);

            ComboBoxSQLAdd(tableName, sql, orderBy);
            rows = _ComboBoxSQL.Select(string.Format("Name='{0}'", tableName));
        }
        return rows[0];
    }
    /// <summary>
    /// 初始化特殊下拉查询
    /// </summary>
    private static void InitializeComboxSqlSetting()
    {
        InitComboBoxSQL();

        ComboBoxSQLAdd("DeptByCompanyID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.Sys_D_Department WHERE Activated=1 AND CompanyID = @Param");
        ComboBoxSQLAdd("YesOrNo", @"SELECT 0 AS ID,0 AS CODE,'否' AS NAME UNION SELECT 1 AS ID,1 AS CODE,'是' AS NAME ");
        ComboBoxSQLAdd("CityByProviceID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_City WHERE Activated=1 AND ProvinceID=@Param");
        ComboBoxSQLAdd("PackageByGoodsID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.V_D_GoodPackage WHERE GoodsID=@Param");
        //改用客户货物关系表中取数据
        ComboBoxSQLAdd("GoodsIDByCustomer", "SELECT TOP 15 T2.ID,T2.CODE,T2.NAME,T2.NAME_E FROM D_Customer_Goods AS T1 INNER JOIN dbo.D_GoodsInfo AS T2 ON T1.GoodsID=T2.ID AND T2.Activated=1 WHERE T1.CustomerID=@Param AND (T2.CODE LIKE @V OR T2.NAME LIKE @V OR T2.NAME_E LIKE @V)");
        //下拉全部的货物信息
        ComboBoxSQLAdd("GoodsIDByCustomer_all", "SELECT TOP 15 T2.ID,T2.CODE,T2.NAME,T2.NAME_E FROM  dbo.D_GoodsInfo AS T2  WHERE T2.Activated=1  AND (T2.CODE LIKE @V OR T2.NAME LIKE @V OR T2.NAME_E LIKE @V)");


        //ComboBoxSQLAdd("GoodsIDByCustomer", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_GoodsInfo WHERE  Activated=1 and CustomerID= @Param");
        ComboBoxSQLAdd("WHouse", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='2' and Activated=1 and WhareHouseID= @Param");

        //根据库区查询库位
        ComboBoxSQLAdd("LoctionByAreaID", "SELECT  ID,CODE,NAME,NAME_E FROM dbo.D_Location WHERE  Activated=1 and AreaID=@Param AND (CODE LIKE @V OR NAME LIKE @V OR NAME_E LIKE @V)");

        //根据库区查询库位
        ComboBoxSQLAdd("AreaIDByWhouse", "SELECT  ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE  Activated=1 AND WhareHouseID=@Param AND (CODE LIKE @V OR NAME LIKE @V OR NAME_E LIKE @V)");




        //库区类型收货区
        ComboBoxSQLAdd("AreaByWHouseID_AT2", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='2' and Activated=1 and WhareHouseID= @Param");
        //库区类型存储区
        ComboBoxSQLAdd("AreaByWHouseID_AT1", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='1' and Activated=1 and WhareHouseID= @Param AND (CODE LIKE @V OR NAME LIKE @V)");
        //如果加上@V条件则联动报错，不知道为何要增加这个，如果有其他用的地方需要增加的，则单独摘除，此处我先修改为原方式，有问题修改到这的 联系我 joey 2016-5-5
        ComboBoxSQLAdd("AreaByWHouseID_AT12", "SELECT TOP 15 ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='1' and Activated=1 and WhareHouseID= @Param AND (CODE LIKE @V OR NAME LIKE @V OR NAME_E LIKE @V)");

        //库区类型发货区
        ComboBoxSQLAdd("AreaByWHouseID_AT3", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='3' and Activated=1 and WhareHouseID= @Param");

        //库区类型备货区
        ComboBoxSQLAdd("AreaByWHouseID_AT4", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='4' and Activated=1 and WhareHouseID= @Param");

        //发货人
        ComboBoxSQLAdd("Customer_Address", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Customer_Address WHERE Activated=1 AND SHIPPER = 1 AND Customer_Id= @Param");

        //收货人
        ComboBoxSQLAdd("Customer_Address_Consignee", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Customer_Address WHERE Activated=1 AND CONSIGNEE = 1 AND Customer_Id= @Param");

        //_ComboBoxSQLSetting = new NameValueCollection();
        ////根据公司ID获取对应的部门信息--参数请用@Param 进行配置
        //_ComboBoxSQLSetting.Add("DeptByCompanyID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.Sys_D_Department WHERE Activated=1 AND CompanyID = @Param");
        //_ComboBoxSQLSetting.Add("YesOrNo", @"SELECT 0 AS ID,0 AS CODE,'否' AS NAME UNION SELECT 1 AS ID,1 AS CODE,'是' AS NAME ");
        //_ComboBoxSQLSetting.Add("CityByProviceID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_City WHERE Activated=1 AND ProvinceID=@Param");
        //_ComboBoxSQLSetting.Add("PackageByGoodsID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.V_D_GoodPackage WHERE GoodsID=@Param");
        //_ComboBoxSQLSetting.Add("GoodsIDByCustomer", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_GoodsInfo WHERE  Activated=1 and CustomerID= @Param");
        //_ComboBoxSQLSetting.Add("WHouse", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='2' and Activated=1 and WhareHouseID= @Param");



        ////库区类型收货区
        //_ComboBoxSQLSetting.Add("AreaByWHouseID_AT2", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='2' and Activated=1 and WhareHouseID= @Param");
        ////库区类型存储区
        //_ComboBoxSQLSetting.Add("AreaByWHouseID_AT1", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='1' and Activated=1 and WhareHouseID= @Param");

        ////库区类型发货区
        //_ComboBoxSQLSetting.Add("AreaByWHouseID_AT3", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='3' and Activated=1 and WhareHouseID= @Param");

        ////库区类型备货区
        //_ComboBoxSQLSetting.Add("AreaByWHouseID_AT4", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='4' and Activated=1 and WhareHouseID= @Param");

        ////发货人
        //_ComboBoxSQLSetting.Add("Customer_Address", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Customer_Address WHERE Activated=1 AND Customer_Id= @Param");
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
    public static SqlCommand GetComboBoxSQLSetting(string tablename, string language, bool isOderby, System.Web.SessionState.HttpSessionState session,string value)
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
        string reltableName = GetTableNameByTypeName(tablename);
        string viewid = BasePage.ViewBaseID(session, reltableName);
        if (!string.IsNullOrEmpty(viewid))
        {
            viewid = string.Format(" AND ID IN({0})", viewid);
        }
        DataRow row = GetComboBoxSQLRow(tablename, reltableName, language, isOderby ? "ORDER BY OrderBy ASC" : "");

        string setting = string.Format("{0} {1} {2}", row["SQL"], row["OrderBY"], viewid);

        SqlCommand cmmd = new SqlCommand(setting);
        if (setting.Contains(" @V "))
        {
            cmmd.Parameters.AddWithValue("@V", string.Format("%{0}%",value));//加入参数信息
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
    public static SqlCommand GetComboBoxSQLSetting(string tablename, string language, string ParamsID,string value)
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
    public static SqlCommand GetComboBoxSQLSettingTimes(string tablename, string language, string value,System.Web.SessionState.HttpSessionState session)
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
        string reltableName=GetTableNameByTypeName(tablename);
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
    /// <summary>
    /// 根据类型名，获取表或视图名
    /// </summary>
    /// <param name="typename"></param>
    /// <returns></returns>
    private static string GetTableNameByTypeName(string typename)
    {
        string tablename = typename;
        switch (typename)
        {
            case "Customer":
                tablename = "D_Customer";
                break;
            case "Customer_Address":
                tablename = "D_Customer_Address";
                break;
            case "Customer_Address_Consignee":
                tablename = "D_Customer_Address";
                break;
            case "MRG_COP_G_NO":
                tablename = "V_MRG_COP_G_NO";
                break;
            default:
                break;
        }
        return tablename;
    }

}
