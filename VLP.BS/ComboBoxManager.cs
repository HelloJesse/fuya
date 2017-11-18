using System;
using System.Web;
using System.Web.SessionState;
using System.Data;
using System.Collections.Specialized;

namespace VLP.BS
{
    public class ComboBoxManager : BasePage, IHttpHandler, IReadOnlySessionState
    {
        public delegate void ComboBoxTableHanlder(HttpContext context, ref string tableName, ref string ParamsID, object sender);
        public static ComboBoxTableHanlder HandlerTableNameDelegate;
        public void ProcessRequest(HttpContext context)
        {
            if (BasePage.IsOut(context)) return;

            string method = context.Request["method"];
            if (method == "GetDropDownData")
            {
                GetDropDownData(context);
            }
            else
            {
                CommonManager(context);
            }

        }
        /// <summary>
        /// 获取多个下拉数据
        /// </summary>
        /// <param name="context"></param>
        private void GetDropDownData(HttpContext context)
        {
            NameValueCollection ns = new NameValueCollection();
            //string data = context.Request["data"];

            System.Collections.ArrayList arr = (System.Collections.ArrayList)JSON.Decode(context.Request["data"]);
            

            foreach (object o in arr)
            {
                System.Collections.ArrayList k = (System.Collections.ArrayList)o;
                string key = k[0].ToString();
                if (string.IsNullOrEmpty(key))
                    key = k[1].ToString();
                ns.Add(key, k[1].ToString());
            }

            string language = BasePage.GetLanguage(context.Request);
            DataSet ds = GetDropdownData(ns, language, context.Session);
            String json = JSON.Encode(ds);
            
            BasePage.Write(context, json);
        }
        /// <summary>
        /// 获取下拉数据
        /// </summary>
        /// <param name="dropdowns"></param>
        /// <param name="language"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static System.Data.DataSet GetDropdownData(NameValueCollection dropdowns, string language,HttpSessionState session)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //排查是否有重复项
            System.Collections.Specialized.StringCollection datakeys = new System.Collections.Specialized.StringCollection();
            foreach (string name in dropdowns)
            {
                string key = dropdowns[name];
                if (datakeys.Contains(key) == false)
                {
                    datakeys.Add(key);
                    sb.Append(CacheManager.GetComboBoxSQLSetting(key, language, session));
                    sb.AppendLine();
                }
            }
            System.Data.DataSet ds = BasePage.GetDB(session).ExecuteDataset(new System.Data.SqlClient.SqlCommand(sb.ToString()));
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                ds.Tables[i].TableName = datakeys[i];
            }
            ds.DataSetName = "Data";
            return ds;
        }

        private void CommonManager(HttpContext context)
        {
            
            string ParamsID = context.Request["ParamsID"]; // 特殊处理时的参数值
            string value = context.Request["key"]; //实时下拉时的value值
            string tablename = context.Request["tablename"]; //table值
            string DataLoad = context.Request["DataLoad"]; //实时下拉or内存加载  0：内存加载 1：实时下拉
            string isorderby = context.Request["isOrderby"];
            if (string.IsNullOrEmpty(tablename))
            {//如果前台没有传递特殊处理的方法，则用系统默认
                throw new Exception("tablename is null");
            }
            if (string.IsNullOrEmpty(value) == false)
            {
                value = value.Trim();
            }
            System.Collections.Hashtable result = new System.Collections.Hashtable();
            try
            {
                if (HandlerTableNameDelegate != null)
                {
                    HandlerTableNameDelegate(context, ref tablename, ref ParamsID, this);
                }
                

                string language = BasePage.GetLanguage(context.Request);
                System.Data.SqlClient.SqlCommand comm = new System.Data.SqlClient.SqlCommand();
                if (DataLoad == "1")
                {
                    if (string.IsNullOrEmpty(ParamsID))
                    {
                        comm = CacheManager.GetComboBoxSQLSettingTimes(tablename, language, value, context.Session);
                    }
                    else
                    {
                        comm = CacheManager.GetComboBoxSQLSetting(tablename, language, ParamsID, null);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(ParamsID))
                    {
                        comm = CacheManager.GetComboBoxSQLSetting(tablename, language, string.IsNullOrEmpty(isorderby) == false ? (isorderby == "1" ? true : false)
                            : false, context.Session, value);
                    }
                    else
                    {
                        comm = CacheManager.GetComboBoxSQLSetting(tablename, language, ParamsID, value);
                    }
                    if (comm == null)
                    {
                        throw new Exception(string.Format("未获取到{0}的配置信息，请检查.", tablename));
                    }
                }

                System.Data.DataTable dt = BasePage.GetDB(context.Session).ExecuteDataTable(comm);
                String json = "";
                if (dt != null && dt.Rows.Count > 0)
                {
                    json = JSON.Encode(JSON.DataTable2ArrayList(dt));
                }
                if (string.IsNullOrEmpty(json))
                {
                    json = "[]";
                }
                context.Response.Write(json);
            }
            catch (Exception ex)
            {
                result["error"] = -1;
                result["message"] = ex.Message;
                result["stackTrace"] = ex.StackTrace;
                String json = JSON.Encode(result);
                context.Response.Clear();
                context.Response.Write(json);
            }
            finally
            {
                AfterInvoke(tablename);//写入日志
            }
        }



        //日志管理
        protected void AfterInvoke(string _tablename)
        {

        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
