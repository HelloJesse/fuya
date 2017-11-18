using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using VLP.Search;
using System.Data;

namespace VLP.BS
{

    public class AutoCommandManager
    {
        public static string _CNN = string.Empty;// DBManager.db.GetConnection().ConnectionString;// string.Empty;
        public static AutoCommands TableAutoCommands = null;
        /// <summary>
        /// 列表显示配置表
        /// </summary>
        public static DataTable ShowFieldTable
        {
            get;
            set;
        }
        public static DataTable Search_Page_Dtl
        {
            get;
            set;
        }
        public static Nordasoft.Common.IO.FileLog FileLog = new Nordasoft.Common.IO.FileLog(AppDomain.CurrentDomain.BaseDirectory, "errlog.txt");
        
        public static string Key = string.Empty;
        private static System.Collections.Specialized.StringDictionary nvs;
        private static bool IsLoadKey = false;
        private static string CheckKey(string cnnstr)
        {
            
            if (string.IsNullOrEmpty(Key))
            {
                return Resource1.KeyValid;
            }
            try
            {
                nvs = new System.Collections.Specialized.StringDictionary();
                string v = MD5.Decrypt(Key);

                if (!string.IsNullOrEmpty(v))
                {
                    string[] vs = v.Split(',');
                    foreach (string k in vs)
                    {
                        string[] ks = k.Split(':');
                        nvs.Add(ks[0], ks[1]);
                    }
                }
                IsLoadKey = true;
                return string.Empty;
            }
            catch
            {
                return Resource1.KeyValid;
            }

        }
        private static string CheckLimit(string pageid, string value,string cnnstr)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(cnnstr))
                {
                    string sql = @"DECLARE @SQL NVARCHAR(1000);
SET @SQL='';
SELECT @SQL='SELECT COUNT(1) FROM '+ TableName +'' FROM dbo.Sys_Table_Info WHERE ID=@ID
EXEC SP_EXECUTESQL @SQL";
                    SqlCommand cmmd = new SqlCommand(sql);
                    cmmd.Parameters.AddWithValue("@ID", pageid);
                    cmmd.Connection = cnn;
                    cnn.Open();
                    int t = int.Parse(cmmd.ExecuteScalar().ToString());
                    if (t >= int.Parse(value))
                    {
                        return string.Format(Resource1.KeyLimit, value);
                    }
                    return string.Empty;
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="cmmdkey"></param>
        /// <param name="data"></param>
        /// <param name="userid"></param>
        /// <param name="empid"></param>
        /// <param name="cnnstr"></param>
        /// <returns></returns>
        public static Result DoCommand(string cmmdkey, System.Collections.Hashtable data, string userid, string empid,string cnnstr)
        {
            Result result = new Result();
            result.IsOK = false;
            if (IsLoadKey == false)
            {
                result.ErrMessage = CheckKey(Key);
                if (string.IsNullOrEmpty(result.ErrMessage) == false)
                    return result;
            }
           
            
#if(DEBUG)
            string [] fields= new string[data.Keys.Count];
            data.Keys.CopyTo(fields,0);
            string tempsql = string.Format(@"
DECLARE @TNAME	VARCHAR(100);
SELECT @TNAME=TableName FROM dbo.Sys_Table_Info WHERE ID={0}
DECLARE @F	VARCHAR(max);
SET @f='';
SELECT @f=@f+name+','
 FROM sys.columns WHERE object_id= object_id(@TNAME) AND name NOT IN('{1}')
 IF @F>'' SET @F=SUBSTRING(@F,1,LEN(@F)-1)
 
 PRINT @F

 UPDATE dbo.Sys_Table_Info SET NoInsertFields=@F,NoUpdateFields=@F WHERE ID={0}
", cmmdkey,string.Join("','", fields));

            System.Diagnostics.Debug.WriteLine(string.Format("设置NoUPdateSQL:{0}",tempsql));
#endif
            SqlCommand dbcommd = null;
            try
            {
                
                dbcommd = GetSqlCommand(cmmdkey, data, userid, empid);
                if (nvs.ContainsKey(cmmdkey) && dbcommd.CommandText.IndexOf("INSERT") >= 0)
                {
                    result.ErrMessage = CheckLimit(cmmdkey, nvs[cmmdkey], cnnstr);
                    if (string.IsNullOrEmpty(result.ErrMessage) == false)
                        return result;
                }
            }
            catch (Exception err)
            {
                result.ErrMessage = err.Message;
                return result;
            }
            try
            {
                using (SqlConnection cnn = new SqlConnection(cnnstr))
                {
                    dbcommd.Connection = cnn;
                    SqlDataAdapter da = new SqlDataAdapter(dbcommd);
                    System.Data.DataTable dt = new System.Data.DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count == 1)
                    {
                        result.IsOK = true;
                        result.PKValue = dt.Rows[0][0].ToString();
                    }
                }
            }
            catch (Exception err)
            {
                result.ErrMessage = err.Message;
            }
            return result;
        }
        /// <summary>
        /// 保存列表数据
        /// </summary>
        /// <param name="cmmdkey"></param>
        /// <param name="jsondata"></param>
        /// <param name="userid"></param>
        /// <param name="empid"></param>
        /// <param name="cnnstr"></param>
        /// <returns></returns>
        public static Result SaveGridData(string cmmdkey, string jsondata, string userid, string empid, string cnnstr)
        {
            Result result = new Result();
            result.IsOK = false;
            
             System.Collections.ArrayList array = (System.Collections.ArrayList)VLP.JSON.Decode(jsondata);
             SqlCommand[] cmmds= new SqlCommand[array.Count];

             for (int i = 0; i < array.Count; i++)
             {
                 System.Collections.Hashtable hs = (System.Collections.Hashtable)array[i];
                 cmmds[i] = GetSqlCommand(cmmdkey, hs, userid, empid,true);
             }
             string[] ids = new string[cmmds.Length];
             try
             {
                 using (SqlConnection cnn = new SqlConnection(cnnstr))
                 {
                     cnn.Open();
                     using (SqlTransaction trans = cnn.BeginTransaction())
                     {
                         for (int i = 0; i < cmmds.Length; i++)
                         {
                             cmmds[i].Connection = cnn;
                             cmmds[i].Transaction = trans;
                             try
                             {
                                 if (i == 0)
                                 {
                                     using (SqlDataReader dr = cmmds[i].ExecuteReader(CommandBehavior.SingleResult))
                                     {
                                         if (dr.Read())
                                         {
                                             result.PKName = dr.GetName(i);
                                             ids[i] = dr[i].ToString();
                                         }
                                     }
                                 }
                                 else
                                 {
                                     ids[i] = cmmds[i].ExecuteScalar().ToString();
                                 }
                             }
                             catch (SqlException sqlerr)
                             {
                                 trans.Rollback();
                                 result.ErrMessage = sqlerr.Message;
                                 return result;
                             }
                         }
                         trans.Commit();
                         result.IsOK = true;
                         result.PKValue = string.Join(",", ids);
                     }
                 }
             }
             catch (Exception err)
             {
                 result.ErrMessage = err.Message;
             }
            return result;
        }
        /// <summary>
        /// 根据Key获取命令
        /// </summary>
        /// <param name="cmmdkey"></param>
        /// <returns></returns>
        public static AutoCommand GetAutoCommand(string cmmdkey)
        {
            if (TableAutoCommands == null)
            {
                InitCommand();
            }
            AutoCommand cmmd = TableAutoCommands.GetCommand(cmmdkey);
            return cmmd;
        }
        /// <summary>
        /// 获取主表保存命令
        /// </summary>
        /// <param name="cmmdkey"></param>
        /// <param name="data"></param>
        /// <param name="userid"></param>
        /// <param name="empid"></param>
        /// <returns></returns>
        public static SqlCommand GetSqlCommand(string cmmdkey, System.Collections.Hashtable data, string userid, string empid)
        {
            return GetSqlCommand(cmmdkey, data, userid, empid, false);
        }
        /// <summary>
        /// 获取保存命令
        /// </summary>
        /// <param name="cmmdkey"></param>
        /// <param name="data"></param>
        /// <param name="userid"></param>
        /// <param name="empid"></param>
        /// <param name="isGrid">是否为列表保存</param>
        /// <returns></returns>
        public static SqlCommand GetSqlCommand(string cmmdkey, System.Collections.Hashtable data, string userid, string empid,bool isGrid)
        {
            AutoCommand cmmd = GetAutoCommand(cmmdkey);// TableAutoCommands.GetCommand(cmmdkey);

            if (cmmd == null)
            {
                throw new ApplicationException( string.Format("未找到[{0}]对应的命令", cmmdkey));
            }
            bool isdel = false;
            if (isGrid)
            {
                String state = data["_state"] != null ? data["_state"].ToString() : "";
                if (state == "removed" || state == "deleted")
                {
                    //说明为删除
                    isdel = true;
                }
            }
            //判断是否为新增
            if (!data.ContainsKey(cmmd.PKName))
            {
                if (isGrid && data["_state"] != null && data["_state"].ToString() == "added")
                {
                    data.Add(cmmd.PKName, 0);
                }
                else
                {
                    //仅只有列表并且为新增时，才会忽略
                    throw new ApplicationException("数据与命令不一致");
                }
                
            }
            string pkvalue = data[cmmd.PKName].ToString();
            SqlCommand dbcommd = null;
            
            if (isdel)
            {
                dbcommd = cmmd.DeleteCommand.Clone();
            }
            else if (string.IsNullOrEmpty(pkvalue) || pkvalue == "0")
            {
                dbcommd = cmmd.InsertCommand.Clone();
            }
            else
            {
                dbcommd = cmmd.UpdateCommand.Clone();
            }
            //邦定参数
            return BindCommandParameters(dbcommd, data, userid, empid);
        }

        /// <summary>
        /// //邦定参数
        /// </summary>
        /// <param name="dbcommd"></param>
        /// <param name="data"></param>
        /// <param name="userid"></param>
        /// <param name="empid"></param>
        /// <returns></returns>
        public static SqlCommand BindCommandParameters(SqlCommand dbcommd, System.Collections.Hashtable data, string userid, string empid)
        {
            foreach (SqlParameter sp in dbcommd.Parameters)
            {
                string key = sp.ParameterName.Substring(1);
                
                

                if (data.ContainsKey(key))
                {
                    object v = data[key];
#if (DEBUG)
                    if (key.Equals("HBL_MODE"))
                    {
                    }
                    if (sp.SqlDbType.Equals(System.Data.SqlDbType.Decimal)
                        || sp.SqlDbType.Equals(System.Data.SqlDbType.Float))
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format(string.Format("{0}:{1}", key, v)));
                    }
#endif
                    if (v == null || v.ToString().Trim().Length == 0)
                    {
                        sp.Value = DBNull.Value;
                    }
                    else
                    {
                        if (sp.SqlDbType == SqlDbType.TinyInt ||
                            sp.SqlDbType == SqlDbType.Bit)
                        {
                            string tempv=v.ToString().ToLower();
                            
                            if (tempv.Equals("false"))
                            {
                                if (sp.SqlDbType == SqlDbType.Bit)
                                    v = false;
                                else
                                    v = "0";
                            }
                            else if (tempv.Equals("true"))
                            {
                                if (sp.SqlDbType == SqlDbType.Bit)
                                    v = true;
                                else
                                    v = "1";
                            }
                            else if (sp.SqlDbType == SqlDbType.Bit)
                            {
                                v = tempv != "0";
                            }
                        }
                        sp.Value = v;
                    }
                }

            }
            //设置当前登录人参数值
            dbcommd.Parameters["@USERID"].Value = userid;
            dbcommd.Parameters["@EMPID"].Value = empid;
            return dbcommd;
        }

        /// <summary>
        /// 初始保存命令及查询配置
        /// </summary>
        public static void InitCommand()
        {
            try
            {
                TableAutoCommands = new AutoCommands();
                SqlConnection CNN = new SqlConnection(_CNN);
                SqlCommand cmmd = new SqlCommand(@"
SELECT ID,PKName,TableName,InsertCommand,InsertCommandP,UpdateCommand,UpdateCommandP,InitCommand,UniqueFields,DeleteCommand,DeleteCommandP FROM [dbo].[Sys_Table_Info] 
SELECT PageID,SearchKey,DBSQL FROM Sys_Search_Page_WhereConfig ORDER BY PageID
SELECT PageID,FilterSQL,SortSQL FROM dbo.Sys_Search_Page WHERE FilterSQL>'' OR SortSQL>'';
SELECT PageID,ShowName,FieldName,Format,ShortName FROM dbo.Sys_Search_Page_ShowField
SELECT PageID,ShortName,MainFiled FROM dbo.Sys_Search_Page_Dtl
");
                cmmd.Connection = CNN;
                SqlDataAdapter da = new SqlDataAdapter(cmmd);
                System.Data.DataSet ds = new System.Data.DataSet();
                da.Fill(ds);
                System.Data.DataTable dt = new System.Data.DataTable();
                dt = ds.Tables[0];
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    AutoCommand autocmmd = new AutoCommand();
                    autocmmd.Key = row["ID"].ToString();
                    autocmmd.PKName = row["PKName"].ToString();
                    string uniquefields = row["UniqueFields"].ToString();
                    if (autocmmd.Key == "91")
                    {
                    }
                    autocmmd.InsertCommand = GetCommand(row["InsertCommand"].ToString(), row["InsertCommandP"].ToString());
                    autocmmd.UpdateCommand = GetCommand(row["UpdateCommand"].ToString(), row["UpdateCommandP"].ToString());
                    autocmmd.DeleteCommand = GetCommand(row["DeleteCommand"].ToString(), row["DeleteCommandP"].ToString());

                    if (uniquefields.Length > 0)
                    {
                        try
                        {
                            AddCheckUniqueField(uniquefields, row["TableName"].ToString(), autocmmd, true);
                        }
                        catch (Exception err)
                        {
                            FileLog.AddLogInfo(string.Format("AddCheckUniqueField【{0}】", row["TableName"]), err.ToString());
                        }
                    }
                    autocmmd.InitCommand = GetCommand(row["InitCommand"].ToString(), "@ID,int,4,10");
                    //添加USERID参数
                    AddUserIDParameter(autocmmd.InsertCommand);
                    AddUserIDParameter(autocmmd.UpdateCommand);
                    TableAutoCommands.Add(autocmmd);
                }
                //初始查询配置
                InitSearchConfig(ds.Tables[1], ds.Tables[2]);
                //设置列表显示配置表
                ShowFieldTable = ds.Tables[3];
                Search_Page_Dtl = ds.Tables[4];

                VLP.BS.SearchManager.InitCommand();
            }
            catch (Exception ERR)
            {
                FileLog.AddLogInfo("InitCommand", ERR.ToString());
                Console.WriteLine(ERR.ToString());
            }
        }
        private static void AddCheckUniqueField(string uniquefields,string tableName, AutoCommand cmmd,bool isCN)
        {
            if (uniquefields.Length == 0) return;
            if (cmmd.UpdateCommand == null || cmmd.InsertCommand == null) return;
            string[] fields = uniquefields.Split(',');
            StringBuilder sbinsertcheck = new StringBuilder();
            StringBuilder sbupdatecheck = new StringBuilder();
            string declarestr = @"
DECLARE @UniqueCheckTip NVARCHAR(1000);
DECLARE @UniqueShowText NVARCHAR(50);
SET @UniqueCheckTip='';
";
            sbinsertcheck.Append(declarestr);
            sbupdatecheck.Append(declarestr);

            foreach (string f in fields)
            {
                string checksql = GetCheckUniqueSQL(tableName, cmmd.PKName,f, isCN, true);
                sbinsertcheck.Append(checksql);
                checksql = GetCheckUniqueSQL(tableName, cmmd.PKName,f, isCN, false);
                sbupdatecheck.Append(checksql);
            }

            cmmd.InsertCommand.CommandText = string.Format("{0}{1}", sbinsertcheck.ToString(), cmmd.InsertCommand.CommandText);
            cmmd.UpdateCommand.CommandText = string.Format("{0}{1}", sbupdatecheck.ToString(), cmmd.UpdateCommand.CommandText);
        }
        private static string GetCheckUniqueSQL(string tableName, string pkName,string field, bool isCN,bool isInsert)
        { string insertstr = string.Format("AND {0}!=@{0}",pkName);
            string checksql=string.Format(@"
IF EXISTS(SELECT 1 FROM {0} WHERE {1}=@{1} {3})
BEGIN
    SELECT @UniqueShowText={2} FROM dbo.Sys_Search_Page M INNER JOIN Sys_Search_Page_ShowField S
    ON M.PageID=S.PageID AND S.ShortName='M'
    WHERE MainTable='{0}' AND S.FieldName='{1}'
    IF @@ROWCOUNT!=1
    BEGIN
        SET @UniqueShowText='{1}';
    END
    SET @UniqueCheckTip=@UniqueShowText+':【'+@{1}+'】已经存在,请使用其它值.'
    RAISERROR(@UniqueCheckTip,16,1);RETURN;
END
", tableName, field,  isCN ? "ShowText" : "ISNULL(ShowText_EN,ShowText)", isInsert ? string.Empty : insertstr);
            return checksql;
        }
        /// <summary>
        /// 将查询回传信息Key信息插入到配置表中,为性能考虑，此方法仅用于测试版本
        /// </summary>
        /// <param name="pageID"></param>
        /// <param name="request"></param>
        public static void AddSearchConfigToDB(string pageID, System.Web.HttpRequest request)
        {
#if(DEBUG)
            StringBuilder sb = new StringBuilder();
            foreach (string key in request.Form)
            {
                if (key.Equals("IDS") || key.Equals("pageIndex") || key.Equals("pageSize") || key.Equals("sortField") || key.Equals("sortOrder") || key.Equals("SortFlag"))
                {
                    continue;
                }
                if (key.Length > 4 && key.EndsWith("NAME", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                sb.AppendFormat(@"
IF NOT EXISTS(SELECT 1 FROM Sys_Search_Page_WhereConfig WHERE PageID='{0}' AND SearchKey='{1}') 
BEGIN
    INSERT INTO Sys_Search_Page_WhereConfig(PageID, SearchKey, DBSQL)VALUES('{0}','{1}','')
END", pageID, key);
            }
            if (sb.Length == 0) return;//没有直接跳出
            using (SqlConnection cnn = new SqlConnection(_CNN))
            {
                SqlCommand cmmd = new SqlCommand(sb.ToString());
                cnn.Open();
                cmmd.Connection = cnn;
                cmmd.ExecuteNonQuery();
            }
            InitCommand();
            System.Diagnostics.Debug.WriteLine(sb.ToString());
#endif

        }
        /// <summary>
        /// //初始查询配置
        /// </summary>
        /// <param name="dt"></param>
        private static void InitSearchConfig(System.Data.DataTable dt,System.Data.DataTable defaultFilter)
        {
            ConfigCollection.SearchConfigs = new SearchConfig();
            ConfigCollection cs = new ConfigCollection();
            foreach (DataRow row in dt.Rows)
            {
                string pid=row["PageID"].ToString();
                if (cs.PageID != pid)
                {
                    cs = new ConfigCollection();
                    cs.PageID = pid;
                    //检查是否有默认条件
                    cs.DefaultFilter = GetDefaultFilter(pid, defaultFilter);
                    //获取默认排序SQL
                    cs.DefaultSortSQL = GetPageConfigFieldValue(pid, defaultFilter, "SortSQL");
                    ConfigCollection.SearchConfigs.Add(cs);
                    Config c = new Config();
                    c.Key = row["SearchKey"].ToString();
                    c.DBSQL = row["DBSQL"].ToString();
                    cs.Add(c);
                }
                else
                {
                    Config c = new Config();
                    c.Key = row["SearchKey"].ToString();
                    c.DBSQL = row["DBSQL"].ToString();
                    cs.Add(c);
                }
            }
        }
        /// <summary>
        /// 获取默认过滤条件
        /// </summary>
        /// <param name="pageid"></param>
        /// <param name="defaultFilter"></param>
        /// <returns></returns>
        private static string GetDefaultFilter(string pageid, System.Data.DataTable defaultFilter)
        {
            return GetPageConfigFieldValue(pageid, defaultFilter, "FilterSQL");
        }
        /// <summary>
        /// 获取默认过滤条件
        /// </summary>
        /// <param name="pageid"></param>
        /// <param name="defaultFilter"></param>
        /// <returns></returns>
        private static string GetPageConfigFieldValue(string pageid, System.Data.DataTable defaultFilter,string fieldName)
        {
            DataRow[] rows = defaultFilter.Select(string.Format("PageID={0}", pageid));
            if (rows.Length > 0)
            {
                return rows[0][fieldName].ToString();
            }
            return string.Empty;
        }
        /// <summary>
        /// 根据命令和参数配置，解析命令
        /// </summary>
        /// <param name="cmmdtext"></param>
        /// <param name="paras">参数配置</param>
        /// <returns></returns>
        public static SqlCommand GetCommand(string cmmdtext, string paras)
        {
            if (string.IsNullOrEmpty(cmmdtext)) return null;

            SqlCommand cmmd = new SqlCommand(cmmdtext);
            //解析参数
            string[] ps = paras.TrimEnd(';').Split(';');
            foreach (string p in ps)
            {
                
                string[] pconfig = p.Split(',');
                if (pconfig.Length > 1)
                {
                    string pname = pconfig[0];
#if(DEBUG)
                    
#endif
                    System.Data.SqlDbType dbtype = GetSqlDbType(pconfig[1]);
                    SqlParameter sp = new SqlParameter(pname, dbtype);
                    //if (dbtype == SqlDbType.Bit||dbtype== SqlDbType.TinyInt)
                    //{
                    //    sp = new SqlParameter();
                    //    sp.ParameterName = pname;
                    //}
                    if (pconfig[1].IndexOf("char") >= 0)
                    {
                        sp.Size = int.Parse(pconfig[2]);
                    }
                    if (pconfig.Length > 3)
                    {
                        sp.Precision = byte.Parse(pconfig[3]);
                    }
                    if (sp.SqlDbType == SqlDbType.Decimal && (pconfig.Length > 4))
                    {
                        
                        //sp.Scale = byte.Parse(pconfig[3]);
                    }
                    if (pconfig.Length == 5 && pconfig[4].Length > 0)
                    {
                        //说明有值
                        sp.Value = ManagerDBDefaultValue(pconfig[4]);
                        if (sp.SqlDbType == SqlDbType.Bit)
                        {
                            sp.SqlValue = sp.Value.ToString() == "0" ? false : true;
                        }
                        else if (sp.SqlDbType == SqlDbType.TinyInt)
                        {
                            sp.SqlValue = byte.Parse(sp.Value.ToString());
                        }
                    }
                    else
                    {
                        sp.Value = DBNull.Value;
                    }
                    cmmd.Parameters.Add(sp);
                }
            }
            ////插入主键ID与操作人信息
            //SqlParameter spID = new SqlParameter("@ID", System.Data.SqlDbType.Int);
            //SqlParameter spUSERID = new SqlParameter("@USERID", System.Data.SqlDbType.Int);
            //SqlParameter spEMPID = new SqlParameter("@EMPID", System.Data.SqlDbType.Int);
            //cmmd.Parameters.Add(spID);
            //cmmd.Parameters.Add(spUSERID);
            //cmmd.Parameters.Add(spEMPID); ;
            return cmmd;
        }
        
        /// <summary>
        /// 添加用户参数
        /// </summary>
        /// <param name="cmmd"></param>
        private static void AddUserIDParameter(SqlCommand cmmd)
        {
            if (cmmd == null) return;
            cmmd.Parameters.Add(new SqlParameter("@USERID", System.Data.SqlDbType.Int));
            cmmd.Parameters.Add(new SqlParameter("@EMPID", System.Data.SqlDbType.Int));
        }

        /// <summary>
        /// 根据字符串类型，转换为数据库类型，若找不到相应的，则转换为Variant
        /// </summary>
        /// <param name="dbtype"></param>
        /// <returns></returns>
        private static System.Data.SqlDbType GetSqlDbType(string dbtype)
        {
            string[] names = Enum.GetNames(typeof(System.Data.SqlDbType));
            foreach (string n in names)
            {
                if (n.Equals(dbtype, StringComparison.CurrentCultureIgnoreCase))
                {
                    return (System.Data.SqlDbType)Enum.Parse(typeof(System.Data.SqlDbType), n);
                }
            }
            if (dbtype.Equals("numeric", StringComparison.CurrentCultureIgnoreCase))
            {
                return System.Data.SqlDbType.Decimal;
            }
            return System.Data.SqlDbType.Variant;
        }
        /// <summary>
        /// 设置命令默认值
        /// </summary>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        private static string ManagerDBDefaultValue(string defaultvalue)
        {
            if (defaultvalue.Length > 2)
            {

                if (defaultvalue.Substring(0, 1) == "(" && defaultvalue.Substring(defaultvalue.Length - 1, 1) == ")")
                {
                    defaultvalue = defaultvalue.Substring(1, defaultvalue.Length - 2);
                }
                else if (defaultvalue.Substring(0, 1) == "'" && defaultvalue.Substring(defaultvalue.Length - 1, 1) == "'")
                {
                    defaultvalue = defaultvalue.Substring(1, defaultvalue.Length - 2);
                }
                else
                {
                    if (defaultvalue.Equals("getdate()", StringComparison.CurrentCultureIgnoreCase))
                        return string.Empty;
                    else
                        return defaultvalue;
                }
                return ManagerDBDefaultValue(defaultvalue);
            }
            return defaultvalue;
        }
        /// <summary>
        /// 检查命令是否有回传，将未回传信息通过System.Diagnostics.Debug.WriteLine输出
        /// </summary>
        /// <param name="cmmd"></param>
        /// <param name="data"></param>
        public static void CheckData(SqlCommand cmmd, System.Collections.Hashtable data)
        {
#if(DEBUG)
            foreach (SqlParameter p in cmmd.Parameters)
            {
                if (data[p.ParameterName.Substring(1)] == null)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Key没有传回值:{0}", p.ParameterName.Substring(1)));
                }
            }
#endif
        }

    }
}
