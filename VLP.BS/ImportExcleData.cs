using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;

namespace VLP.BS
{
    public delegate string ImportExcleHandler(ImportExcleEvent sender);
    public class ImportExcleEvent
    {
        public  string TableName;
        public DataTable DetailData;
        public DataTable TemplateTable;
        public System.Collections.Specialized.NameValueCollection MainKeys;

    }
    public delegate string ImportExcleDataRowOtherFieldHandler(ImportExcleEvent sender,DataRow row);

    public class ImportExcleData
    {
        string _CNN = string.Empty;
        string _UserID = string.Empty;
        string _pagePID = string.Empty;
        string _treePID = "0";


        ImportExcleHandler _GetBeforCheckSQL;
        ImportExcleHandler _GetAfterCheckSQL;
        ImportExcleHandler _GetOtherSaveFields;
        ImportExcleHandler _GetNoSaveFields;
        ImportExcleDataRowOtherFieldHandler _GetOtherSaveFieldsSQLValue;
        System.Collections.Specialized.NameValueCollection _MainKeys;
        public ImportExcleHandler GetBeforCheckSQL
        {
            get { return _GetBeforCheckSQL; }
            set { _GetBeforCheckSQL = value; }
        }
        public ImportExcleHandler GetAfterCheckSQL
        {
            get { return _GetAfterCheckSQL; }
            set { _GetAfterCheckSQL = value; }
        }
        /// <summary>
        /// 获取需要额外保存的字段，多个字段用半角逗号分隔.如:Field1,Field2
        /// </summary>
        public ImportExcleHandler GetOtherSaveFields
        {
            get { return _GetOtherSaveFields; }
            set { _GetOtherSaveFields = value; }
        }
        /// <summary>
        /// 获取不需要保存的字段，多个字段用半角逗号分隔.如:Field1,Field2
        /// </summary>
        public ImportExcleHandler GetNoSaveFields
        {
            get { return _GetNoSaveFields; }
            set { _GetNoSaveFields = value; }
        }
        /// <summary>
        /// 获取保存其它字段的SQL或Value
        /// </summary>
        public ImportExcleDataRowOtherFieldHandler GetOtherSaveFieldsSQLValue
        {
            get { return _GetOtherSaveFieldsSQLValue; }
            set { _GetOtherSaveFieldsSQLValue = value; }
        }
        //private StringCollection _NoSaveFields;
        ///// <summary>
        ///// 不需要保存的字段
        ///// </summary>
        //public StringCollection NoSaveFields
        //{
        //    get
        //    {
        //        if (_NoSaveFields == null)
        //            _NoSaveFields = new System.Collections.Specialized.StringCollection();
        //        return _NoSaveFields;
        //    }
        //    set
        //    {
        //        _NoSaveFields = value;
        //    }
        //}
        //private StringCollection _OtherSaveFields;
        ///// <summary>
        ///// 其它需要保存的字段
        ///// </summary>
        //public StringCollection OtherSaveFields
        //{
        //    get
        //    {
        //        if (_OtherSaveFields == null)
        //            _OtherSaveFields = new System.Collections.Specialized.StringCollection();
        //        return _OtherSaveFields;
        //    }
        //    set
        //    {
        //        _OtherSaveFields = value;
        //    }
        //}

        public ImportExcleData(string cnnstr,string userID)
        {
            _CNN = cnnstr;
            _UserID = userID;
        }

        /// <summary>
        /// </summary>
        /// <param name="pageIDorName"></param>
        /// <param name="pagePID">0 导入基础数据 -999更新基础数据</param>
        /// <param name="allData"></param>
        /// <returns></returns>
        public DataTable GetExcleTemplate(string pageIDorName,string pagePID, bool allData)
        {
            _pagePID = pagePID;
            return GetExcleTemplate(pageIDorName, allData);
        }

        /// <summary>
        /// 获取Excle导入模板数据返回的表名为PageName
        /// </summary>
        /// <param name="pageIDorName">PageID或PageName</param>
        /// <param name="allData">是否获取所有配置信息,false只获取ShowText,ImportFlag信息</param>
        /// ImportFlag:导入基础数据标识 
        /// UpdateFlag:更新基础数据标识 
        /// <returns></returns>
        public DataTable GetExcleTemplate(string pageIDorName,bool allData)
        {
            DataTable dt = new DataTable();
            
            SqlCommand cmmd = new SqlCommand(string.Format(@"
            DECLARE @PageID	INT
            IF ISNUMERIC(@PageIDorName)=0
            BEGIN
                SELECT @PName=PageName,@PageID=PageID FROM dbo.Sys_Search_Page 
                    WHERE (PageName=@PageIDorName OR MainTable=(SELECT [TableName]  FROM [dbo].[Sys_Table_Info] WHERE Remark=@PageIDorName ))
                     AND PageName= (SELECT [TableName]  FROM [dbo].[Sys_Table_Info] WHERE Remark=@PageIDorName );
            END
            ELSE
            BEGIN
                SELECT @PName=PageName,@PageID=PageID FROM dbo.Sys_Search_Page WHERE PageID=@PageIDorName;
            END
            IF @ALLFLAG=0
            BEGIN
                SELECT ShowText,{0},CASE {0} WHEN 2 THEN 0 WHEN 0 THEN 0 ELSE 1 END AS Checked,ShowName 
                FROM dbo.Sys_Search_Page_ShowField S WHERE PageID=@PageID AND ({0}!=2) ORDER BY ColOrder
            END
            ELSE
            BEGIN
                SELECT ShowText,{0},S.ShortName,FieldName,SubTable,SubField,MainFiled,MainTable
                    ,CASE {0} WHEN 2 THEN 0 WHEN 0 THEN 0 ELSE 1 END AS Checked,ShowName 
                FROM dbo.Sys_Search_Page_ShowField S 
                INNER JOIN dbo.Sys_Search_Page P ON  S.PageID = P.PageID
                LEFT JOIN dbo.Sys_Search_Page_Dtl dtl ON S.PageID = dtl.PageID AND S.ShortName = dtl.ShortName
                WHERE S.PageID=@PageID AND ({0}!=2)  ORDER BY ColOrder
            END ", (string.IsNullOrEmpty(_pagePID) || _treePID.Equals(_pagePID)) ? "ImportFlag" : "UpdateFlag"));

            cmmd.Parameters.Add(new SqlParameter("@PageIDorName", SqlDbType.VarChar,50));
            cmmd.Parameters["@PageIDorName"].Value = pageIDorName;
            cmmd.Parameters.Add(new SqlParameter("@ALLFLAG", SqlDbType.Bit));
            cmmd.Parameters["@ALLFLAG"].Value = allData;
            cmmd.Parameters.Add(new SqlParameter("@PName", SqlDbType.NVarChar, 50));
            cmmd.Parameters["@PName"].Direction = ParameterDirection.Output;
            cmmd.Connection = new SqlConnection(_CNN);
            SqlDataAdapter da = new SqlDataAdapter(cmmd);
            da.Fill(dt);
            dt.TableName = cmmd.Parameters["@PName"].Value.ToString();
            return dt;
        }
                /// <summary>
        /// 保存模板数据,TableName为sheet表名
        /// </summary>
        /// <param name="dt">数据表,注意表名应为PageName</param>
        public string SaveExcleTemplateData(DataTable dt)
        {
            return SaveExcleTemplateData(dt, false,null,null,null);
        }
        /// <summary>
        /// 保存模板数据,TableName为sheet表名
        /// </summary>
        /// <param name="dt">数据表,注意表名应为PageName</param>
        /// <param name="ignoreExist">是否忽略存在的 true--存在时不做处理,flase--存在时，提示用户已经存在</param>
        /// <param name="tableType">表类型 0--系统表1--基础表 2--业务主表 3--业务子表 255--未知</param> 
        public string SaveExcleTemplateData(DataTable dt, bool ignoreExist, System.Collections.Specialized.NameValueCollection nvs, string tableType, string treePid)
        {
            _MainKeys = nvs;
            if (dt == null || dt.Rows.Count == 0)
            {
                return "未发现需要导入的数据.";
            }
            if (string.IsNullOrEmpty(dt.TableName))
            {
                return "PageName不能为空.";
            }
            //首先检查列是不是正确.
            DataTable dttemplate = GetExcleTemplate(dt.TableName, treePid, true);
            if(dttemplate.Rows.Count==0){
                return string.Format("未找到【{0}】的配置信息.",dt.TableName);
            }
            StringBuilder sb = new StringBuilder();
            
            foreach (DataColumn c in dt.Columns)
            {
                DataRow[] rows = dttemplate.Select(string.Format("ShowText='{0}'", c.ColumnName.Replace("'", "''")));
                if (rows.Length == 0)
                {
                    sb.AppendFormat("列【{0}】无效;",c.ColumnName);
                }
            }
            System.Collections.Specialized.NameValueCollection checknosql = new System.Collections.Specialized.NameValueCollection();

            //foreach (DataRow row in dttemplate.Select("ImportFlag=1 OR ImportFlag=10 OR ImportFlag=11"))
            foreach (DataRow row in dttemplate.Select(string.Format("{0}=1 OR {0}=10 OR {0}=11", (string.IsNullOrEmpty(treePid) || _treePID.Equals(treePid)) ? "ImportFlag" : "UpdateFlag")))
            {
                //验证必须存在的列是否存在
                //string flag = row["ImportFlag"].ToString();
                string flag = (string.IsNullOrEmpty(treePid) || _treePID.Equals(treePid)) ? row["ImportFlag"].ToString() :row["UpdateFlag"].ToString(); 
                string name = row["ShowText"].ToString();
                string maintable=row["MainTable"].ToString();
                string fieldname = row["FieldName"].ToString();
                if (dt.Columns.Contains(name) == false)
                {
                    sb.AppendFormat("列【{0}】未存在导入模板中.", name);
                }
                if (flag == "10")
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        if (string.IsNullOrEmpty(r[name].ToString()))
                    {
                        sb.AppendFormat("列【{0}】存为为空的数据.", name);
                            break;
                    }
                }
                    //if (dt.Select(string.Format("{0}=''", name)).Length > 0)//修改查询为空的数据长度大于0，才说明字段列有空值
                    //{
                    //    sb.AppendFormat("列【{0}】存为为空的数据.", name);
                    //}
                }
                if (flag == "11")
                {
                    try
                    {
                        dt.PrimaryKey = new DataColumn[] { dt.Columns[name] };
                        checknosql.Add(name, "IF EXISTS(SELECT 1 FROM " + maintable + " WHERE " + fieldname + @"='{0}')
BEGIN
    SELECT '【" + name + @"】列中的值:【{0}】在系统中已经存在.','"+name+@"' AS CName,'{0}' AS CVALUE
END
");
                    }
                    catch
                    {
                        StringBuilder sbValue = new StringBuilder();//重复的值
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (dt.Select(string.Format("{0}='{1}'", name, dr[name].ToString().Replace("'", "''"))).Length > 1)
                            {
                                sbValue.AppendFormat("{0}; ", dr[name].ToString());
                            }
                        }
                        sb.AppendFormat("列【{0}】必须唯一.重复值【{1}】", name, sbValue);
                    }
                }
            }
            if (sb.Length > 0) return sb.ToString();

            //检查数据唯一性
            StringBuilder sbchecknosql = new StringBuilder();

            foreach (string name in checknosql.AllKeys)
            {
                foreach (DataRow row in dt.Rows)
                {
                    sbchecknosql.AppendFormat(checknosql[name], row[name].ToString().Replace("'", "''"));
                }
            }
            DataSet ds_Exists = new DataSet();
            if (sbchecknosql.Length > 0)
            {
                SqlDataAdapter da = new SqlDataAdapter(sbchecknosql.ToString(), _CNN);
                //DataSet ds = new DataSet();
                da.Fill(ds_Exists);
                if (ignoreExist == false)
                {
                    foreach (DataTable t in ds_Exists.Tables)
                    {
                        sb.AppendLine(t.Rows[0][0].ToString());
                    }
                }
            }
            if (sb.Length > 0) return sb.ToString();
            //生成导入SQL
            StringBuilder sbsql = new StringBuilder();
            if (ignoreExist && ds_Exists.Tables.Count>0)
            {
                //移除忽略的数据
                foreach (DataTable t in ds_Exists.Tables)
                {
                    DataRow[] exitsrows = dt.Select(string.Format("{0}='{1}'", t.Rows[0]["CName"], t.Rows[0]["CValue"]));
                    foreach (DataRow r in exitsrows)
                    {
                        dt.Rows.Remove(r);
                    }
                }
            }
            if (dt.Rows.Count == 0)
            {
                return "未发现需要导入的数据.";
            }
            string result = CreateInsertSQL(dt, dttemplate, ref sbsql, tableType);
            if (string.IsNullOrEmpty(result) == false) return result;
            string beforchecksql = string.Empty;
            string afterchecksql = string.Empty;
            if (_GetBeforCheckSQL != null)
            {
                ImportExcleEvent importsender = new ImportExcleEvent();
                importsender.DetailData=dt;
                importsender.TemplateTable = dttemplate;
                importsender.MainKeys=nvs;
            
                beforchecksql = _GetBeforCheckSQL(importsender);
            }
            if (_GetAfterCheckSQL != null)
            {
                ImportExcleEvent importsender = new ImportExcleEvent();
                importsender.DetailData = dt;
                importsender.TemplateTable = dttemplate;
                importsender.MainKeys = nvs;

                afterchecksql = _GetAfterCheckSQL(importsender);
            }
            SqlCommand cmmd = new SqlCommand();
            cmmd.CommandText = string.Format(@"
BEGIN TRAN;
{0}
{1}
IF @@ERROR<>0
BEGIN
    ROLLBACK TRAN;
    SELECT 0
END
ELSE
BEGIN
    {2}
    COMMIT TRAN;
    SELECT 1
END",beforchecksql, sbsql.ToString(),afterchecksql);
            cmmd.Connection = new SqlConnection(_CNN);
            cmmd.Parameters.Add("@UserID", SqlDbType.Int);
            cmmd.Parameters["@UserID"].Value = _UserID;
            SqlDataAdapter resultda = new SqlDataAdapter(cmmd);
            try
            {
                DataTable resultdt = new DataTable();
                resultda.Fill(resultdt);
                if (resultdt.Rows[0][0].ToString() == "1")
                {
                    return string.Empty;
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
            return "导入失败.";
        }
        private bool CheckExist(string[] values, string checkvalue)
        {
            foreach (string v in values)
                if (v.Equals(checkvalue, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            return false;
        }
        /// <summary>
        /// 获取插入SQL
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="templateTable"></param>
        /// <param name="sbsql"></param>
        /// <returns></returns>
        private string CreateInsertSQL(DataTable dt, DataTable templateTable, ref StringBuilder sbsql, string tableType)
        {
            //System.Collections.Specialized.NameValueCollection nvs
            string maintable = templateTable.Rows[0]["MainTable"].ToString();
            sbsql = new StringBuilder();
            StringBuilder sbfield = new StringBuilder();
            System.Collections.Specialized.NameValueCollection mainfields = new System.Collections.Specialized.NameValueCollection();
            System.Collections.Specialized.NameValueCollection subfields = new System.Collections.Specialized.NameValueCollection();
              ImportExcleEvent importsender = new ImportExcleEvent();
                importsender.DetailData = dt;
                importsender.TemplateTable = templateTable;
                importsender.MainKeys = _MainKeys;
                string[] _NoSaveFields = GetNoSaveFields(importsender).Split(','); ;
                
                string[] othersavefields = new string[] { };
                string othersavefieldstr = GetOtherSaveFields(importsender).Trim();
                if (othersavefieldstr.Length > 0)
                {
                    othersavefields = othersavefieldstr.Split(',');
                }

            foreach (DataColumn c in dt.Columns)
            {
                DataRow row = GetTemplateRow(templateTable, c.ColumnName);
                string showtext=row["ShowText"].ToString();
                string showname = row["ShowName"].ToString();
                if (_NoSaveFields != null && _NoSaveFields.Length>0 && (CheckExist(_NoSaveFields, showname)))
                    continue;
                if (row["ShortName"].ToString().Equals("M"))
                {
                    mainfields.Add(row["FieldName"].ToString(), showtext);
                }
                else
                {
                    subfields.Add(row["MainFiled"].ToString(), showtext);
                }
            }
            DataSet subTextDataSet = GetSubTextDataSet(dt, templateTable, subfields);

            string cellvalue = string.Empty;
            string otherfields = "";
            if (othersavefields.Length > 0)
            {
                otherfields = string.Format(",{0}", string.Join(",", othersavefields));
                if (GetOtherSaveFieldsSQLValue == null)
                {
                    throw new ApplicationException("请指定GetOtherSaveFieldsSQLValue属性。");
                }
            }

            //根据 _pagePID 判定生成指定命令 0：Insert 其它 Update
            if (string.IsNullOrEmpty(_pagePID) || _treePID.Equals(_pagePID))
            {
                #region 生成 Insert 命令
                if (subTextDataSet != null)
                {
                    string checkresult = CheckSuTextDataSet(subTextDataSet);
                    if (string.IsNullOrEmpty(checkresult) == false) return checkresult;

                    string[] fields = null;
                    //业务表导入数据，需要流水号
                    if (null != tableType && "2".Equals(tableType))
                    {
                        fields = new string[mainfields.Count + subfields.Count + 4];
                        fields[fields.Length - 4] = "BILLNO";
                    }
                    else
                    {
                        fields = new string[mainfields.Count + subfields.Count + 3];
                    }

                    mainfields.AllKeys.CopyTo(fields, 0);
                    subfields.AllKeys.CopyTo(fields, mainfields.Count);

                    fields[fields.Length - 3] = "CREATE_BY";
                    fields[fields.Length - 2] = "EMP_ID_CREATE_BY";
                    fields[fields.Length - 1] = "CREATE_DATE";

                    sbsql.Append(@" DECLARE @EMPID  INT;
                                SET @EMPID=DBO.GETEMPID(@UserID);
                                DECLARE @BILLNO NVARCHAR(20);");
                    sbsql.AppendLine();
                    int tcount = 0;

                    foreach (DataRow r in dt.Rows)
                    {
                        //业务表导入数据，需要生成流水号
                        if (null != tableType && "2".Equals(tableType))
                        {
                            sbsql.AppendFormat("EXEC [dbo].[Sys_GetFlowNo] @UserID,'{0}','','',@BILLNO OUT ", maintable);
                        }
                        sbsql.AppendFormat(" INSERT INTO {0}({1}{2})", maintable, string.Join(",", fields), otherfields);

                        sbsql.Append("SELECT ");
                        for (int i = 0; i < mainfields.Count; i++)
                        {
                            cellvalue = r[mainfields[i]].ToString().Replace("'", "''");
                            //特殊判断
                            if (mainfields[i].ToString().Equals("是否启用"))
                            {
                                if (cellvalue.Length == 0)
                                {
                                    sbsql.Append("1,");
                                }
                                else if (cellvalue.Equals("是") || cellvalue.Equals("1") || cellvalue.ToUpper().Equals("TRUE"))
                                {
                                    sbsql.AppendFormat("'{0}',", 1);
                                }
                                else
                                {
                                    sbsql.AppendFormat("'{0}',", 0);
                                }
                            }
                            else
                            {
                                if (cellvalue.Length == 0)
                                {
                                    sbsql.Append("NULL,");
                                }
                                else if (cellvalue.Equals("是") || cellvalue.Equals("1") || cellvalue.ToUpper().Equals("TRUE"))
                                {
                                    sbsql.AppendFormat("'{0}',", 1);
                                }
                                else if (cellvalue.Equals("否") || cellvalue.Equals("0") || cellvalue.ToUpper().Equals("FALSE"))
                                {
                                    sbsql.AppendFormat("'{0}',", 0);
                                }
                                else
                                {
                                    sbsql.AppendFormat("'{0}',", cellvalue);
                                }
                            }
                        }
                        //处理子表
                        for (int i = 0; i < subfields.Count; i++)
                        {
                            string v = r[subfields[i]].ToString();
                            if (string.IsNullOrEmpty(v))
                            {
                                sbsql.AppendFormat("NULL,");
                            }
                            else
                            {
                                sbsql.AppendFormat("{0},", GetSubTextIDValue(subTextDataSet, subfields[i], v));
                            }
                        }
                        //业务表导入数据，需要插入流水号
                        if (null != tableType && "2".Equals(tableType))
                        {
                            sbsql.Append("@BILLNO,@UserID,@EMPID,GETDATE()");
                        }
                        else
                        {
                            sbsql.Append("@UserID,@EMPID,GETDATE()");
                        }
                        //添加其它特殊处理字段
                        if (othersavefields.Length > 0)
                        {
                            sbsql.AppendFormat(",{0}", GetOtherSaveFieldsSQLValue(importsender, r));
                        }
                        tcount++;
                        if (tcount != dt.Rows.Count)
                        {
                            sbsql.AppendLine();
                            //sbsql.Append("UNION");
                            //sbsql.AppendLine();
                        }
                    }
                }
                else
                {
                    //仅主表情况
                    string[] fields = null;
                    //业务表导入数据，需要流水号
                    if (null != tableType && "2".Equals(tableType))
                    {
                        fields = new string[mainfields.Count + 4];
                        fields[fields.Length - 4] = "BILLNO";
                    }
                    else
                    {
                        fields = new string[mainfields.Count + 3];
                    }
                    mainfields.AllKeys.CopyTo(fields, 0);
                    fields[fields.Length - 3] = "CREATE_BY";
                    fields[fields.Length - 2] = "EMP_ID_CREATE_BY";
                    fields[fields.Length - 1] = "CREATE_DATE";

                    sbsql.Append(@" DECLARE @EMPID  INT;
                                SET @EMPID=DBO.GETEMPID(@UserID);
                                DECLARE @BILLNO NVARCHAR(20);");
                    sbsql.AppendLine();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow r = dt.Rows[i];
                        //业务表导入数据，需要生成流水号
                        if (null != tableType && "2".Equals(tableType))
                        {
                            sbsql.AppendFormat("EXEC [dbo].[Sys_GetFlowNo] @UserID,'{0}','','',@BILLNO OUT ", maintable);
                        }
                        sbsql.AppendFormat(" INSERT INTO {0}({1}{2})", maintable, string.Join(",", fields), otherfields);

                        sbsql.Append("SELECT ");
                        for (int j = 0; j < mainfields.Count; j++)
                        {
                            cellvalue = r[mainfields[j]].ToString().Replace("'", "''");
                            //特殊判断
                            if (mainfields[j].ToString().Equals("是否启用"))
                            {
                                if (cellvalue.Length == 0)
                                {
                                    sbsql.Append("1,");
                                }
                                else if (cellvalue.Equals("是") || cellvalue.Equals("1") || cellvalue.ToUpper().Equals("TRUE"))
                                {
                                    sbsql.AppendFormat("'{0}',", 1);
                                }
                                else
                                {
                                    sbsql.AppendFormat("'{0}',", 0);
                                }
                            }
                            else
                            {
                                if (cellvalue.Length == 0)
                                {
                                    sbsql.Append("NULL,");
                                }
                                else if (cellvalue.Equals("是") || cellvalue.Equals("1") || cellvalue.ToUpper().Equals("TRUE"))
                                {
                                    sbsql.AppendFormat("'{0}',", 1);
                                }
                                else if (cellvalue.Equals("否") || cellvalue.Equals("0") || cellvalue.ToUpper().Equals("FALSE"))
                                {
                                    sbsql.AppendFormat("'{0}',", 0);
                                }
                                else
                                {
                                    sbsql.AppendFormat("'{0}',", cellvalue);
                                }
                            }

                        }
                        //业务表导入数据，需要插入流水号
                        if (null != tableType && "2".Equals(tableType))
                        {
                            sbsql.Append("@BILLNO,@UserID,@EMPID,GETDATE()");
                        }
                        else
                        {
                            sbsql.Append("@UserID,@EMPID,GETDATE()");
                        }

                        //添加其它特殊处理字段
                        if (othersavefields.Length > 0)
                        {
                            sbsql.AppendFormat(",{0}", GetOtherSaveFieldsSQLValue(importsender, r));
                        }
                        if (i != dt.Rows.Count - 1)
                        {
                            sbsql.AppendLine();
                            //sbsql.Append("UNION");
                            //sbsql.AppendLine();
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region 生成 Update 命令
                if (subTextDataSet != null)
                {
                    string checkresult = CheckSuTextDataSet(subTextDataSet);
                    if (string.IsNullOrEmpty(checkresult) == false) return checkresult;

                    string[] fields = new string[3];

                    fields[fields.Length - 3] = "UPDATE_BY = @UserID";
                    fields[fields.Length - 2] = "EMP_ID_UPDATE_BY = @EMPID";
                    fields[fields.Length - 1] = "UPDATE_DATE = GETDATE()";

                    sbsql.Append(@" DECLARE @EMPID  INT;
                                SET @EMPID=DBO.GETEMPID(@UserID); ");

                    sbsql.AppendLine();
                    int tcount = 0;
                    string othersql = "";
                    foreach (DataRow r in dt.Rows)
                    {
                        //sbsql.AppendFormat(" INSERT INTO {0}({1}{2})", maintable, string.Join(",", fields), otherfields);
                        sbsql.AppendFormat(" UPDATE {0} SET {1}", maintable, string.Join(",", fields));
                        othersql = " WHERE 1=1 ";

                        for (int i = 0; i < mainfields.Count; i++)
                        {
                            cellvalue = r[mainfields[i]].ToString().Replace("'", "''");
                            //特殊判断
                            if (mainfields[i].ToString().Equals("是否启用"))
                            {
                                if (cellvalue.Length == 0)
                                {
                                    //sbsql.Append("1,");
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[i], 1);
                                }
                                else if (cellvalue.Equals("是") || cellvalue.Equals("1") || cellvalue.ToUpper().Equals("TRUE"))
                                {
                                    //sbsql.AppendFormat("'{0}',", 1);
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[i], 1);
                                }
                                else
                                {
                                    //sbsql.AppendFormat("'{0}',", 0);
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[i], 0);
                                }
                            }
                            else if (mainfields.Keys[i].ToUpper().Equals("CODE"))//保存条件语句
                            {
                                othersql += string.Format(" AND {0}='{1}'", mainfields.Keys[i], cellvalue);
                            }
                            else if (mainfields.Keys[i].ToUpper().Equals("BILLNO"))//保存条件语句
                            {
                                othersql += string.Format(" AND {0}='{1}'", mainfields.Keys[i], cellvalue);
                            }
                            else
                            {
                                if (cellvalue.Length == 0)
                                {
                                    //sbsql.Append("NULL,");
                                    sbsql.AppendFormat(",{0}=NULL", mainfields.Keys[i]);
                                }
                                else if (cellvalue.Equals("是") || cellvalue.Equals("1") || cellvalue.ToUpper().Equals("TRUE"))
                                {
                                    //sbsql.AppendFormat("'{0}',", 1);
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[i], 1);
                                }
                                else if (cellvalue.Equals("否") || cellvalue.Equals("0") || cellvalue.ToUpper().Equals("FALSE"))
                                {
                                    //sbsql.AppendFormat("'{0}',", 0);
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[i], 0);
                                }
                                else
                                {
                                    //sbsql.AppendFormat("'{0}',", cellvalue);
                                    sbsql.AppendFormat(",{0}='{1}'", mainfields.Keys[i], cellvalue);
                                }
                            }
                        }
                        //处理关联的子表
                        for (int i = 0; i < subfields.Count; i++)
                        {
                            string v = r[subfields[i]].ToString();
                            if (string.IsNullOrEmpty(v))
                            {
                                //sbsql.AppendFormat("NULL,");
                                sbsql.AppendFormat(",{0}=NULL", subfields.Keys[i]);
                            }
                            else
                            {
                                //sbsql.AppendFormat("{0},", GetSubTextIDValue(subTextDataSet, subfields[i], v));
                                sbsql.AppendFormat(",{0}={1}", subfields.Keys[i], GetSubTextIDValue(subTextDataSet, subfields[i], v));
                            }
                        }

                        //添加 where 条件语句
                        sbsql.Append(othersql);
                        
                        tcount++;
                        if (tcount != dt.Rows.Count)
                        {
                            sbsql.AppendLine();
                        }
                    }
                }
                else
                {
                    //仅主表情况
                    string[] fields = new string[3];

                    fields[fields.Length - 3] = "UPDATE_BY = @UserID";
                    fields[fields.Length - 2] = "EMP_ID_UPDATE_BY = @EMPID";
                    fields[fields.Length - 1] = "UPDATE_DATE = GETDATE()";

                    sbsql.Append(@" DECLARE @EMPID  INT;
                                SET @EMPID=DBO.GETEMPID(@UserID); ");

                    sbsql.AppendLine();
                    string othersql = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow r = dt.Rows[i];
                        sbsql.AppendFormat(" UPDATE {0} SET {1}", maintable, string.Join(",", fields));
                        othersql = " WHERE 1=1 ";

                        for (int j = 0; j < mainfields.Count; j++)
                        {
                            cellvalue = r[mainfields[j]].ToString().Replace("'", "''");
                            //特殊判断
                            if (mainfields[j].ToString().Equals("是否启用"))
                            {
                                if (cellvalue.Length == 0)
                                {
                                    //sbsql.Append("1,");
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[j], 1);
                                }
                                else if (cellvalue.Equals("是") || cellvalue.Equals("1") || cellvalue.ToUpper().Equals("TRUE"))
                                {
                                    //sbsql.AppendFormat("'{0}',", 1);
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[j], 1);
                                }
                                else
                                {
                                    //sbsql.AppendFormat("'{0}',", 0);
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[j], 0);
                                }
                            }
                            else if (mainfields.Keys[j].ToUpper().Equals("CODE"))//保存条件语句
                            {
                                othersql += string.Format(" AND {0}='{1}'", mainfields.Keys[j], cellvalue);
                            }
                            else if (mainfields.Keys[j].ToUpper().Equals("BILLNO"))//保存条件语句
                            {
                                othersql += string.Format(" AND {0}='{1}'", mainfields.Keys[j], cellvalue);
                            }
                            else
                            {
                                if (cellvalue.Length == 0)
                                {
                                    //sbsql.Append("NULL,");
                                    sbsql.AppendFormat(",{0}=NULL", mainfields.Keys[j]);
                                }
                                else if (cellvalue.Equals("是") || cellvalue.Equals("1") || cellvalue.ToUpper().Equals("TRUE"))
                                {
                                    //sbsql.AppendFormat("'{0}',", 1);
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[j], 1);
                                }
                                else if (cellvalue.Equals("否") || cellvalue.Equals("0") || cellvalue.ToUpper().Equals("FALSE"))
                                {
                                    //sbsql.AppendFormat("'{0}',", 0);
                                    sbsql.AppendFormat(",{0}={1}", mainfields.Keys[j], 0);
                                }
                                else
                                {
                                    //sbsql.AppendFormat("'{0}',", cellvalue);
                                    sbsql.AppendFormat(",{0}='{1}'", mainfields.Keys[j], cellvalue);
                                }
                            }
                        }
                        //添加 where 条件语句
                        sbsql.Append(othersql);
                       
                        if (i != dt.Rows.Count - 1)
                        {
                            sbsql.AppendLine();
                        }
                    }
                }
                #endregion
            }

            return string.Empty;
        }
        /// <summary>
        /// 获取子表ID
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="showText"></param>
        /// <param name="textValue"></param>
        /// <returns></returns>
        private string GetSubTextIDValue(DataSet ds,string showText, string textValue)
        {
            foreach (DataTable dt in ds.Tables)
            {
                if (dt.TableName.Equals(showText))
                {
                    DataRow[] rows = dt.Select(string.Format("V='{0}'", textValue.Replace("'", "''")));
                    if (rows.Length == 1)
                    {
                        return rows[0]["ID"].ToString();
                    }
                    else
                    {
                        throw new ApplicationException(string.Format("列【{0}】值【{1}】无效.",showText,textValue));
                    }
                }
            }
            throw new ApplicationException(string.Format("未找到列【{0}】对应数据表.", showText));
        }
        private DataRow GetTemplateRow(DataTable templateTable, string showText)
        {
            DataRow[] rows = templateTable.Select(string.Format("ShowText='{0}'", showText));
            return rows[0];
        }
        /// <summary>
        /// 获取子表数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="templateTable"></param>
        /// <param name="subFields"></param>
        /// <returns></returns>
        private DataSet GetSubTextDataSet(DataTable dt, DataTable templateTable,System.Collections.Specialized.NameValueCollection subFields)
        {
            if (subFields.Count == 0) return null;
            StringBuilder sb = new StringBuilder();
            System.Collections.Specialized.NameValueCollection checksql = new System.Collections.Specialized.NameValueCollection();
            foreach (string field in subFields)
            {
                DataRow row = templateTable.Select(string.Format("MainFiled='{0}'", field))[0];
                string showtext = row["ShowText"].ToString();
                string subtable = row["SubTable"].ToString();
                string subfield = row["SubField"].ToString();
                string fieldname = row["FieldName"].ToString();
                System.Collections.Specialized.StringCollection values = new System.Collections.Specialized.StringCollection();
                foreach (DataRow r in dt.Rows)
                {
                    string value=r[showtext].ToString();
                    if (string.IsNullOrEmpty(value)) continue;

                    if (values.Contains(value) == false)
                    {
                        sb.AppendFormat(" SELECT '{0}' AS V,(SELECT TOP 1 {1} FROM {2} WHERE {3}='{0}' AND Activated=1) AS ID", value.Replace("'", "''"), subfield, subtable, fieldname);
                        sb.AppendFormat("{0}UNION", Environment.NewLine);
                        values.Add(value);
                    }
                }
                if (sb.Length > 0)
                {
                    checksql.Add(field, sb.ToString().TrimEnd("UNION".ToCharArray()));
                    sb = new StringBuilder();
                }
            }
            if (checksql.Count > 0)
            {
                string sql = "";
                foreach (string s in checksql.AllKeys)
                {
                    sql += checksql[s] + ";";
                }
                SqlDataAdapter da = new SqlDataAdapter(sql,_CNN);
                DataSet ds = new DataSet();
                da.Fill(ds);
                for (int i = 0; i < ds.Tables.Count; i++)
                {

                    ds.Tables[i].TableName = templateTable.Select(string.Format("MainFiled='{0}'", checksql.AllKeys[i]))[0]["ShowText"].ToString();
                }
                return ds;
            }
            return null;
        }
        /// <summary>
        /// 检查子表内容是否有效
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private string CheckSuTextDataSet(DataSet ds)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DataTable dt in ds.Tables)
            {
                DataRow[] rows = dt.Select("ID IS NULL");
                foreach (DataRow row in rows)
                {
                    sb.AppendFormat("列【{0}】值【{1}】无效.{2}", dt.TableName, row["V"], Environment.NewLine);
                }
            }
            return sb.ToString();
        }
    }
}
