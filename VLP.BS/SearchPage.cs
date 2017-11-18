using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace VLP.BS
{

    public class SearchPage : BasePage, IHttpHandler, System.Web.SessionState.IReadOnlySessionState
    {
        HttpContext _context;
        string PageID = "";
        BaseUser currentUserInfo = null;
        string Language = "CN";
        public void ProcessRequest(HttpContext context)
        {
            _context = context;

            string methodName = _context.Request["method"];
            PageID = _context.Request["PageID"];
            if (string.IsNullOrEmpty(PageID))
            {//页的ID不能为空
                System.Collections.Hashtable result = new System.Collections.Hashtable();
                result["error"] = -1;
                result["message"] = "PageID is null";
                return;
            }
            currentUserInfo = GetUserInfo;
            //超时直接退出
            if (BasePage.IsOut(context)) return;
            Language = BasePage.GetLanguage(context.Request);

            Type type = this.GetType();
            if (string.IsNullOrEmpty(methodName))
            {//如果前台没有传递特殊处理的方法，则用系统默认
                methodName = "SearchGridList";
            }
            System.Reflection.MethodInfo method = type.GetMethod(methodName);
            if (method == null) throw new Exception("method is null");
            try
            {
                BeforeInvoke(methodName);
                method.Invoke(this, null);
            }
            catch (Exception ex)
            {
                System.Collections.Hashtable result = new System.Collections.Hashtable();
                result["error"] = -1;
                result["message"] = ex.Message;
                result["stackTrace"] = ex.StackTrace;
                String json = JSON.Encode(result);
                _context.Response.Clear();
                _context.Response.Write(json);
            }
            finally
            {
                AfterInvoke(methodName);
            }
        }

        /// <summary>
        /// 编辑界面，根据ID获取明细信息
        /// </summary>
        public void SearchEditGridList()
        {
            if (string.IsNullOrEmpty(PageID) == false)
            {
                string MainID = _context.Request["MainID"];
                if (string.IsNullOrEmpty(MainID))
                {
                    MainID = "0";
                }
                string Orderby = _context.Request["Orderby"];
                if (Orderby == null) Orderby = string.Empty;
                try
                {
                    System.Data.SqlClient.SqlCommand sqlcom = VLP.BS.SearchManager.GetGridData_EditDtlCommand(
                        int.Parse(PageID), int.Parse(MainID), Orderby);

                    System.Data.DataTable dt = DB.ExecuteDataTable(sqlcom);
                    if (dt != null)
                    {

                        //BasePage.Write(_context,JSON.Encode(JSON.DataTable2ArrayList(dt)));
                        BasePage.Write(_context, JSON.Encode(dt));
                    }
                }
                catch (Exception err)
                {

                    System.Collections.Hashtable result = new System.Collections.Hashtable();
                    result["errMessage"] = err.Message;
                    BasePage.Write(_context, JSON.Encode(result));
                }

            }
            else
            {
                System.Collections.Hashtable result = new System.Collections.Hashtable();
                result["error"] = -1;
                result["message"] = "未获取到对应的PageID,请检查PageID配置信息.";
                String json = JSON.Encode(result);
                BasePage.Write(_context, json);
                //_context.Response.Clear();
                //_context.Response.Write(json);
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

        private string GetOrderBySQL(string pageID, string sortfields, out string subtables)
        {
            subtables = string.Empty;
            System.Collections.Specialized.StringCollection subtable = new System.Collections.Specialized.StringCollection();
            if (string.IsNullOrEmpty(sortfields) == false)
            {
                System.Text.StringBuilder sborderby = new System.Text.StringBuilder();
                System.Collections.ArrayList sortfieldhs = (System.Collections.ArrayList)JSON.Decode(sortfields);
                foreach (object o in sortfieldhs)
                {
                    System.Collections.Hashtable hs = (System.Collections.Hashtable)o;
                    if (hs.Count == 2)
                    {
                        string field = hs["field"].ToString();
                        System.Data.DataRow[] rows = VLP.BS.AutoCommandManager.ShowFieldTable.Select(string.Format("PageID={0} AND ShowName='{1}'", pageID, field));
                        if (rows.Length == 1)
                        {
                            System.Data.DataRow row = rows[0];

                            string shortname = row["ShortName"].ToString();

                            if(shortname.Equals("M", StringComparison.CurrentCultureIgnoreCase)==false){
                                if (subtable.Contains(shortname) == false)
                                {
                                    subtable.Add(shortname);
                                }
                            }
                            field = string.Format("{0}.{1}",shortname, row["FieldName"].ToString());
                            if (row["Format"].ToString().Equals("D", StringComparison.CurrentCultureIgnoreCase))
                            {
                                field = string.Format("CONVERT(VARCHAR(10),{0},120)", field);
                            }

                            //if (shortname.Equals("M", StringComparison.CurrentCultureIgnoreCase))
                            //{
                            //    field = row["FieldName"].ToString();
                            //    if (row["Format"].ToString().Equals("D", StringComparison.CurrentCultureIgnoreCase))
                            //    {
                            //        field = string.Format("CONVERT(VARCHAR(10),{0},120)", field);
                            //    }
                            //}
                            //else
                            //{
                            //    field = VLP.BS.AutoCommandManager.Search_Page_Dtl.Select(string.Format("PageID={0} AND ShortName='{1}'", pageID, shortname))[0]["MainFiled"].ToString();
                            //}
                            
                            if (sborderby.Length == 0)
                            {
                                sborderby.AppendFormat(" ORDER BY {0} {1}", field, hs["dir"]);
                            }
                            else
                            {
                                sborderby.AppendFormat(",{0} {1}", field, hs["dir"]);
                            }
                        }
                        else
                        {
                            throw new ApplicationException(string.Format("转换排序字段出错:PageID={0}&Field={1}", pageID, field));
                        }
                    }
                }
                string[] subs = new string[subtable.Count];
                subtable.CopyTo(subs, 0);
                subtables = string.Join(",", subs);
                return sborderby.ToString();
            }
            else
                return string.Empty;
        }

        //系统默认查询方法
        public void SearchGridList()
        {
            if (string.IsNullOrEmpty(PageID) == false)
            {
                System.Data.DataTable dt_toal = new System.Data.DataTable();//总数
                System.Data.DataTable dt_data = new System.Data.DataTable();//记录


                string PageIndx = _context.Request.Form["PageIndex"].ToString(); //取页码数据
                string PageSize = _context.Request.Form["PageSize"].ToString(); //取每页记录数
                //string sortField = _context.Request.Form["sortField"].ToString(); //排序字段
                //string sortOrder = _context.Request.Form["sortOrder"].ToString(); //排序类型
                string sortFlag = _context.Request["SortFlag"].ToString();



                short PageSize_short = 0;
                short pageindx_short = 0;
                if (short.TryParse(PageSize, out PageSize_short) == false)
                {
                    PageSize_short = 20;
                }
                short.TryParse(PageIndx, out pageindx_short);

                System.Collections.Hashtable result = new System.Collections.Hashtable();
                string ids = _context.Request.Form["IDS"].ToString();

                string orderbysql = string.Empty;
                if (string.IsNullOrEmpty(ids))
                {//执行查询
                    string wheresql = VLP.Search.SearchConfig.GetWhereSQL(PageID, _context.Request);
                    orderbysql = VLP.Search.SearchConfig.GetOrderBySQL(PageID);
                    System.Diagnostics.Debug.WriteLine(wheresql);
                    System.Data.SqlClient.SqlCommand sqlcom = VLP.BS.SearchManager.GetSearchDataBySQLCommand(int.Parse(PageID), currentUserInfo.ID.ToString(), pageindx_short
                        , PageSize_short, wheresql, orderbysql);
                    try
                    {
                        System.Data.DataSet ds = DB.ExecuteDataset(sqlcom);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            dt_toal = ds.Tables[0];
                            dt_data = ds.Tables[1];
                            result["data"] = JSON.DataTable2ArrayList(dt_data);
                            result["total"] = dt_toal != null && dt_toal.Rows.Count > 0 ? dt_toal.Rows[0]["TOTAL"] : 0;
                            result["Ids"] = dt_toal != null && dt_toal.Rows.Count > 0 ? dt_toal.Rows[0]["IDS"] : "";
                        }
                    }
                    catch (Exception err)
                    {
                        result["errMessage"] = err.Message;
                    }
                }
                else
                {
                    System.Data.SqlClient.SqlCommand sqlcom = null;
                    try
                    {
                        if (sortFlag == "1")
                        {
                            string sortfields = _context.Request["SortFields"];
                            string subtables = string.Empty;
                            orderbysql = GetOrderBySQL(PageID, sortfields, out subtables);
                            sqlcom = VLP.BS.SearchManager.GetGridDataByOrderBySQLCommand(int.Parse(PageID), currentUserInfo.ID.ToString()
                            , pageindx_short, PageSize_short, ids, orderbysql, subtables);
                            System.Data.DataSet ds = DB.ExecuteDataset(sqlcom);
                            if (ds != null && ds.Tables.Count > 0)
                            {
                                ids = ds.Tables[0].Rows[0]["IDS"].ToString();
                                dt_data = ds.Tables[1];
                                result["data"] = JSON.DataTable2ArrayList(dt_data);
                                result["total"] = ids.Split(',').Length;
                                result["Ids"] = ids;
                            }
                        }
                        else
                        {
                            sqlcom = VLP.BS.SearchManager.GetSearchDataByIDCommand(int.Parse(PageID), currentUserInfo.ID.ToString()
                                   , pageindx_short, PageSize_short, ids);
                            System.Data.DataSet ds = DB.ExecuteDataset(sqlcom);
                            if (ds != null && ds.Tables.Count > 0)
                            {
                                dt_data = ds.Tables[0];
                                result["data"] = JSON.DataTable2ArrayList(dt_data);
                                result["total"] = ids.Split(',').Length;
                                result["Ids"] = ids;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        result["errMessage"] = err.Message;
                    }

                }
                String json = JSON.Encode(result);
                BasePage.Write(_context, json);
            }
            else
            {
                System.Collections.Hashtable result = new System.Collections.Hashtable();
                result["errMessage"] = "未获取到对应的PageID,请检查PageID配置信息.";
                String json = JSON.Encode(result);
                _context.Response.Clear();
                _context.Response.Write(json);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 获取功能按钮
        /// </summary>
        /// <param name="pageid"></param>
        /// <param name="flag"></param>
        public void GetSearchFunction()
        {
            String pageid = PageID;// context.Request["PageID"];
            string language = Language;// Common.GetLanguage(context.Request);

            System.Data.DataSet ds = VLP.BS.Role.GetSearchFunctionPopedom(this.GetUserInfo.ID.ToString(), pageid, DB);
            System.Data.DataTable dt = ds.Tables[1];
            System.Data.DataTable dtallbtn = ds.Tables[0];
            System.Data.DataTable dtviewbtn = new System.Data.DataTable();
            dtviewbtn.Columns.Add("Text", typeof(string));
            dtviewbtn.Columns.Add("Ico", typeof(string));
            dtviewbtn.Columns.Add("Click", typeof(string));

            string userid = this.GetUserInfo.ID.ToString();

            string[] btnconfigs = dt.Rows[0]["SearchButton"].ToString().Split('/');
            VLP.BS.SearchPageResult result = new SearchPageResult();

            //string systemCompany = Common.GetSysCompany(this.Session);

            System.Collections.ArrayList temparray = new System.Collections.ArrayList();

            foreach (string c in btnconfigs)
            {
                string[] config = c.Split(':');
                if (config.Length == 2)
                {
                    System.Text.StringBuilder sbmenu = new System.Text.StringBuilder();
                    //多级菜单情况
                    string[] configs = config[1].Split('|');
                    foreach (string mc in configs)
                    {
                        string[] mcc = mc.Split('.');
                        if (mcc.Length == 2)
                        {
                            if (IsHavePopedom(userid, mcc[0], dtallbtn))
                            {
                                sbmenu.AppendFormat("{0}|", mc);
                            }
                        }
                        else
                            continue;
                    }
                    if (sbmenu.Length > 0)
                    {
                        //说明有菜单情况
                        temparray.Add(string.Format("{0}:{1}", config[0], sbmenu.ToString().TrimEnd('|')));
                    }
                }
                else
                {
                    config = c.Split('.');
                    if (config.Length != 2)
                        continue;
                    if (IsHavePopedom(userid, config[0], dtallbtn))
                    {
                        temparray.Add(c);
                    }
                }

            }
            dtviewbtn = SettingManager.GetButtonTable(temparray, _context.Session, language);
            result.ButtonData = dtviewbtn;
            //获取列表
            VLP.BS.GridManager gridmanager = new VLP.BS.GridManager(PageID, currentUserInfo.ID.ToString(), Language);
            gridmanager.Init(DB.GetConnection().ConnectionString);
            result.ColumnsData = gridmanager;
            //获取下拉数据
            result.DropdownData = GetDropdownData();
            result.IsOK = true;
            //_context.Response.Write(JSON.Encode(result));
            BasePage.Write(_context, JSON.Encode(result));

        }
        public System.Data.DataSet GetDropdownData()
        {
            string dropdowndata = _context.Request["Dropdown"];
            if (string.IsNullOrEmpty(dropdowndata) || dropdowndata=="null") return null;
             System.Collections.ArrayList arrlist=((System.Collections.ArrayList)JSON.Decode(dropdowndata));

            System.Collections.Specialized.NameValueCollection ns = new System.Collections.Specialized.NameValueCollection();

            foreach (object k in arrlist)
            {
                System.Collections.ArrayList al = (System.Collections.ArrayList)k;
                ns.Add(al[0].ToString(),al[1].ToString());
            }
          return  ComboBoxManager.GetDropdownData(ns, Language,_context.Session);
        }
        public void GetSearchGridConfig()
        {

            VLP.BS.SearchPageResult result = new SearchPageResult();
            //获取列表
            VLP.BS.GridManager gridmanager = new VLP.BS.GridManager(PageID, currentUserInfo.ID.ToString(), Language);
            //检查是否需要保存列布局
            string fieldwidth = _context.Request["FieldWidth"];
            string cnnstr = DB.GetConnection().ConnectionString;
            if (string.IsNullOrEmpty(fieldwidth) == false)
            {
                gridmanager.SaveColumnsWidth(cnnstr, Language, fieldwidth);
            }
            gridmanager.Init(cnnstr);
            result.ColumnsData = gridmanager;
            result.IsOK = true;
            _context.Response.Write(JSON.Encode(result));
        }
        private bool IsHavePopedom(string userid, string functionid, System.Data.DataTable popedomdt)
        {
            System.Data.DataRow[] rows = CacheManager.FunctionTable.Select(string.Format("ID={0}", functionid));
            if (rows.Length != 1)
            {
                throw new ApplicationException(string.Format("未找到ID为[{0}]的功能项.", functionid));
            }
            if (!(bool)rows[0]["IsCheckPopedom"]        //不需要检查权限
               || CacheManager.IsAdmin(this.GetUserInfo.ID.ToString()) //或者管理员
               || popedomdt.Select(string.Format("ID={0}", functionid)).Length == 1  //有权限
               )
            {
                return true;
            }
            return false;
        }
        public void GetGridColumns()
        {
            VLP.BS.GridManager gridmanager = new VLP.BS.GridManager(PageID, currentUserInfo.ID.ToString(), Language);
            gridmanager.Init(DB.GetConnection().ConnectionString);
            String json = JSON.Encode(gridmanager);
            _context.Response.Write(json);

        }
    }
}
