using System;
using System.Web;

namespace VLP.BS
{
    public class EditPage : BasePage, IHttpHandler, System.Web.SessionState.IReadOnlySessionState
    {
        HttpContext _context;
        string id = "";
        string TableID = "";
        string tablename = "";
        string ids = "";
        string tag = "";

        BaseUser currentUserInfo = null;
        public void ProcessRequest(HttpContext context)
        {
            _context = context;
            //超时直接退出
            if (BasePage.IsOut(context)) return;

            string methodName = _context.Request["method"]; //加载方法
            TableID = _context.Request["TableID"];//主键名称
            id = _context.Request["id"];//主键值


            tablename = _context.Request["tablename"];//
            tag = _context.Request["tag"];//
            ids = _context.Request["ids"];//
            currentUserInfo = GetUserInfo;
            
            if (string.IsNullOrEmpty(methodName))
            {//如果前台没有传递特殊处理的方法，则用系统默认
                methodName = "LoadFormData";
            }

            Type type = this.GetType();
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
        /// 处理启用
        /// </summary>
        public void DoActived()
        {
            System.Data.SqlClient.SqlCommand sqlcom = new System.Data.SqlClient.SqlCommand();
            sqlcom.CommandText = "sp_BaseDataDoAction";
            sqlcom.CommandType = System.Data.CommandType.StoredProcedure;
            sqlcom.Parameters.AddWithValue("@tablename", tablename);
            sqlcom.Parameters.AddWithValue("@tag", tag);
            sqlcom.Parameters.AddWithValue("@ids", ids);
            sqlcom.Parameters.AddWithValue("@userid", currentUserInfo.ID.ToString());
            sqlcom.Parameters.AddWithValue("@empid", currentUserInfo.ID.ToString());
            System.Data.DataTable dt = DB.ExecuteDataTable(sqlcom);
            System.Collections.Hashtable result = new System.Collections.Hashtable();
            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0].ToString().Length > 0)
            {
                result["iswrong"] = dt.Rows[0][0];
                result["errmassage"] = dt.Rows[0][1];
            }
            else
            {
                result["iswrong"] = "1";
                result["errmassage"] = "操作失败，联系管理员。";
            }
            _context.Response.Write(JSON.Encode(result));
        }

        public void LoadFormData()
        {
            string pageid = _context.Request["PageID"];
            string billid = _context.Request["ID"];
            string userid = this.GetUserInfo.ID.ToString();
            string dtlpageid = _context.Request["DTLPageID"];
            LoadFormData2(pageid, billid, userid, dtlpageid);
        }
        public void LoadFormData2(string pageid,string billid,string userid,string dtlpageid)
        {
     

            VLP.BS.LoadFormResult result = new VLP.BS.LoadFormResult();
            try
            {
                System.Data.SqlClient.SqlCommand cmmd = null;
                if (string.IsNullOrEmpty(dtlpageid))
                {
                    cmmd = VLP.BS.EditManager.GetGetBillDataByIDCommand(int.Parse(pageid), int.Parse(billid), userid);
                }
                else
                {
                    cmmd = VLP.BS.EditManager.GetBillAndDTLDataByIDCommand(int.Parse(pageid), int.Parse(billid), userid, dtlpageid);
                }

                System.Data.DataSet ds = DB.ExecuteDataset(cmmd);

                System.Data.DataTable dt = ds.Tables[0];
                System.Data.DataRow row = null;
                if (dt.Rows.Count > 0)
                {
                    row = dt.Rows[0];
                }
                System.Data.DataTable viewbtns = GetEditFormFunction(this.GetUserInfo.ID.ToString(), pageid, row);


                result.IsOK = true;
                result.BillData = dt;

                if (ds.Tables.Count >1 )
                {
                    result.DtlData = ds.Tables[1];

                    if (ds.Tables.Count > 2)
                    {
                        System.Data.DataTable[] tables = new System.Data.DataTable[ds.Tables.Count - 2];

                        for (int i = 2; i < ds.Tables.Count; i++)
                        {
                            tables[i - 2] = ds.Tables[i];
                        }
                        result.OtherData = tables;
                    }
                }
                result.ButtonData = viewbtns;
                //SpecialEditPageManager.ManagerResult(result, pageid, billid, DB, Session);
            }
            catch (Exception err)
            {
                result.ErrMessage = err.Message;
            }
            //_context.Response.Write(JSON.Encode(result));
            BasePage.Write(_context,JSON.Encode(result));
        }
        /// <summary>
        /// 获取新增时按钮
        /// </summary>
        /// <returns></returns>
        public void GetEditFunction()
        {
            string pageid = _context.Request["PageID"];
            System.Data.DataTable dt = GetEditFormFunction(this.GetUserInfo.ID.ToString(), pageid, null);
            VLP.BS.DataResult result = new VLP.BS.DataResult();
            result.IsOK = true;
            result.Data = dt;
            string josn = JSON.Encode(result);
            _context.Response.Write(josn);
        }
        /// <summary>
        /// 获取编辑界面可用的功能按钮
        /// </summary>
        /// <param name="pageid"></param>
        /// <param name="flag"></param>
        public System.Data.DataTable GetEditFormFunction(string userid, string pageid, System.Data.DataRow datarow)
        {
            
            string language = BasePage.GetLanguage(this._context.Request);

            System.Data.DataRow buttonrow = CacheManager.GetPageButtonRow(pageid);
            System.Data.DataSet ds = Role.GetViewPopedomByPageID(userid, pageid, DB);
            System.Data.DataTable dtpopedom = ds.Tables[0];
            System.Data.DataTable dtviewuserid = ds.Tables[1];
            string btnconfig = buttonrow["EditButtons"].ToString();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            SetCheckEditPopedomSQL(btnconfig, this.GetUserInfo.ID.ToString(), sb, datarow, dtpopedom, dtviewuserid);

            System.Data.DataTable popedombilldt = null;
            if (sb.Length > 0)
            {
                string str = string.Format(@"
SELECT FID FROM({0})T WHERE FID IS NOT NULL", sb.ToString().Substring(0, sb.Length - 7));
                System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand(str);
                cmmd.Parameters.Add("@USERID", System.Data.SqlDbType.Int);
                cmmd.Parameters["@USERID"].Value = userid;
                popedombilldt = DB.ExecuteDataTable(cmmd);
            }

            System.Collections.ArrayList tempconfigs = ManagerViewBtnArraylist(btnconfig, this.GetUserInfo.ID.ToString(), popedombilldt);

            return SettingManager.GetButtonTable(tempconfigs, this.Session, language);
        }

        /// <summary>
        /// 设置检查是否有权的SQL
        /// </summary>
        /// <param name="btnconfig"></param>
        /// <param name="userid"></param>
        /// <param name="sb"></param>
        /// <param name="datarow"></param>
        /// <param name="popedomdt"></param>
        private void SetCheckEditPopedomSQL(string btnconfig, string userid, System.Text.StringBuilder sb, System.Data.DataRow datarow, System.Data.DataTable popedomdt,
            System.Data.DataTable dtViewUserid)
        {
            string[] configs = btnconfig.Split('/');
            foreach (string config in configs)
            {
                //判断是否为多级菜单
                string[] mconfig = config.Split(':');
                if (mconfig.Length == 2)
                {
                    //多级菜单
                    string[] cfg = mconfig[1].Split('|');
                    foreach (string mc in cfg)
                    {
                        string[] mcc = mc.Split('.');
                        if (mcc.Length == 2)
                        {
                            SetCheckPopedomSQL(popedomdt, datarow, userid, sb, mcc, dtViewUserid);
                        }
                        else
                            continue;
                    }
                }
                else
                {
                    string[] cfg = config.Split('.');
                    if (cfg.Length == 2)
                    {
                        SetCheckPopedomSQL(popedomdt, datarow, userid, sb, cfg, dtViewUserid);
                    }
                }
            }
        }

        /// <summary>
        /// 处理可见的按钮集
        /// </summary>
        /// <param name="btnconfig"></param>
        /// <param name="userid"></param>
        /// <param name="popedombilldt"></param>
        /// <returns></returns>
        private System.Collections.ArrayList ManagerViewBtnArraylist(string btnconfig, string userid, System.Data.DataTable popedombilldt)
        {
            string[] configs = btnconfig.Split('/');

            System.Collections.ArrayList tempconfigs = new System.Collections.ArrayList();
            foreach (string c in configs)
            {
                System.Text.StringBuilder sbmenu = new System.Text.StringBuilder();
                //判断是否为多级菜单
                string[] mconfig = c.Split(':');
                if (mconfig.Length == 2)
                {
                    //多级菜单
                    string[] config = mconfig[1].Split('|');
                    foreach (string mc in config)
                    {
                        string[] mcc = mc.Split('.');
                        if (mcc.Length == 2)
                        {
                            if (IsHavePopedom(userid, mcc[0], popedombilldt))
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
                        tempconfigs.Add(string.Format("{0}:{1}", mconfig[0], sbmenu.ToString().TrimEnd('|')));
                    }
                }
                else
                {
                    string[] config = c.Split('.');
                    if (config.Length == 2)
                    {
                        if (IsHavePopedom(this.GetUserInfo.ID.ToString(), config[0], popedombilldt))
                        {
                            tempconfigs.Add(c);
                        }
                    }
                }

            }
            return tempconfigs;
        }
        /// <summary>
        /// 检查是否有权限
        /// </summary>
        /// <param name="tempconfigs"></param>
        /// <param name="userid"></param>
        /// <param name="id"></param>
        /// <param name="popedomdt"></param>
        /// <returns></returns>
        private bool IsHavePopedom(string userid, string id, System.Data.DataTable popedomdt)
        {
            //string systemCompany = Common.GetSysCompany(this.Session);

            System.Data.DataRow[] rows = CacheManager.FunctionTable.Select(string.Format("ID={0}", id));
            if (rows.Length != 1)
            {
                throw new ApplicationException(string.Format("未找到ID为[{0}]的功能项.", id));
            }
            if (!(bool)rows[0]["IsCheckPopedom"]        //不需要检查权限
               || CacheManager.IsAdmin(userid) //或者管理员
                || (popedomdt != null && popedomdt.Select(string.Format("FID={0}", id)).Length == 1)
               )
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置检查权限SQL
        /// </summary>
        /// <param name="dtpopedom"></param>
        /// <param name="datarow"></param>
        /// <param name="sb"></param>
        /// <param name="cfg"></param>
        private static void SetCheckPopedomSQL(System.Data.DataTable dtpopedom, System.Data.DataRow datarow, string userid, System.Text.StringBuilder sb, string[] cfg,
            System.Data.DataTable dtViewUserid)
        {
            System.Data.DataRow[] popedomrows = dtpopedom.Select(string.Format("ID={0}", cfg[0]));
            if (popedomrows.Length > 0)
            {
                string opvalue = string.Empty;
                string optype = string.Empty;
                bool isop = false;
                bool fixedflag = false; //是否有固定权限

                foreach (System.Data.DataRow popedomrow in popedomrows)
                {
                    if (datarow == null)
                    {
                        if (popedomrow["OPField"].ToString().Equals("CREATE_BY", StringComparison.CurrentCultureIgnoreCase))
                        {
                            opvalue = userid;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        opvalue = datarow[popedomrow["OPField"].ToString()].ToString();
                    }

                    optype = popedomrow["OPType"].ToString();
                    isop = (bool)popedomrow["IsOP"];
                    if ((bool)popedomrow["FixedFlag"])
                    {
                        if (CheckUserIDIsInViewIDS(opvalue, dtViewUserid))
                        {
                            sb.AppendFormat(@"
SELECT {0} AS FID
UNION
", cfg[0]);
                            continue;
                        }
                    }
                    if (isop)
                    {
                        sb.AppendFormat(@"
SELECT CASE WHEN {0}=1 THEN CASE WHEN @USERID={1} THEN {2} END
            WHEN {0}=2 THEN CASE WHEN {1} IN(SELECT ID FROM Sys_D_USER WHERE DepartmentID=(SELECT DepartmentID FROM Sys_D_USER WHERE ID=@USERID)) THEN {2} END
            WHEN {0}=3 THEN CASE WHEN {1} IN(SELECT ID FROM Sys_D_USER WHERE CompanyID=(SELECT CompanyID FROM Sys_D_USER WHERE ID=@USERID)) THEN {2} END
            WHEN {0}=4 THEN CASE WHEN  {1} IN(SELECT ID FROM Sys_D_USER WHERE CompanyID IN(SELECT CompanyID FROM DBO.GetCompanyAllID(@USERID))) THEN {2} END
            WHEN {0}=5 THEN {2} END AS FID
UNION
"
                        , optype, opvalue, cfg[0]);
                    }
                    else
                    {
                        sb.AppendFormat(@"
SELECT CASE WHEN {0}=1 THEN CASE WHEN (SELECT CompanyID FROM Sys_D_USER WHERE ID=@USERID)={1} THEN {2} END
            WHEN {0}=2 THEN CASE WHEN {1} IN(SELECT CompanyID FROM DBO.GetCompanyAllID(@USERID)) THEN {2} END
END AS FID
UNION
"
                        , optype, opvalue, cfg[0]);
                    }
                }


            }
        }
        /// <summary>
        /// 检查用户ID是否存在于可见用户表中
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="dtViewUserid"></param>
        /// <returns></returns>
        private static bool CheckUserIDIsInViewIDS(string userid, System.Data.DataTable dtViewUserid)
        {
            foreach (System.Data.DataRow row in dtViewUserid.Rows)
            {
                string[] viewuserids = row["ViewID"].ToString().Split(',');
                foreach (string id in viewuserids)
                {
                    if (id == userid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        // /// <summary>
        // /// 系统自动加载表单数据
        // /// </summary>
        //public void LoadFormData()
        //{
        //    VLP.BS.AutoCommand autocmmd = VLP.BS.AutoCommandManager.TableAutoCommands.GetCommand(TableID);

        //    if (autocmmd == null || autocmmd.InitCommand==null)
        //    {
        //        throw new Exception("未获取到表的命令信息,请检查配置信息。");
        //    }
        //    else
        //    {
        //        autocmmd.InitCommand.Parameters["@ID"].Value = id;
        //       System.Data.DataTable dt = DB.ExecuteDataTable(autocmmd.InitCommand);
        //        string josn = JSON.Encode(DBManager.DataTable2ArrayList(dt));
        //        _context.Response.Write(josn.Substring(1, josn.Length - 2));
        //    }

        //    //string sql = @"SELECT InitCommand FROM Sys_Table_Info WHERE ID=@TableID";
        //    //System.Data.SqlClient.SqlCommand sqlcom = new System.Data.SqlClient.SqlCommand();
        //    //sqlcom.CommandText = sql;
        //    //sqlcom.CommandType = System.Data.CommandType.Text;
        //    //sqlcom.Parameters.AddWithValue("@TableID", TableID);
        //    //System.Data.DataTable dt = DB.ExecuteDataTable(sqlcom);
        //    //if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0].ToString().Length>0)
        //    //{
        //    //    sqlcom.CommandText = dt.Rows[0][0].ToString();
        //    //    sqlcom.Parameters.Clear();
        //    //    sqlcom.Parameters.AddWithValue("@ID", id);
        //    //    dt = DB.ExecuteDataTable(sqlcom);
        //    //    string josn = JSON.Encode(DBManager.DataTable2ArrayList(dt));
        //    //    _context.Response.Write(josn.Substring(1, josn.Length - 2));
        //    //}
        //    //else
        //    //{
        //    //    throw new Exception("未获取到加载SQL,请检查配置信息。");
        //    //}
        //}

        //权限管理
        protected void BeforeInvoke(String methodName)
        {

        }

        //日志管理
        protected void AfterInvoke(String methodName)
        {

        }

        /// <summary>
        /// 生成结算单
        /// </summary>
        public void CreateBalance()
        {
            string TypeId = _context.Request.Form.Get("TypeId");
            string BillNo = _context.Request.Form.Get("BillNo");
            System.Collections.Hashtable result = new System.Collections.Hashtable();
            result["msg"] = "";//失败返回错误，成功不返回
            String json = String.Empty;

            if (string.IsNullOrEmpty(TypeId))
            {
                result["msg"] = "未找到账单类型";
                json = JSON.Encode(result);
                Response.Write(json);
                return;
            }
            if (string.IsNullOrEmpty(BillNo))
            {
                result["msg"] = "单号不能为空";
                json = JSON.Encode(result);
                Response.Write(json);
                return;
            }

            System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand();
            try
            {
                cmmd.CommandText = "sp_Create_F_Settle_Bill";
                cmmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmmd.Parameters.Add("@BillNo", System.Data.SqlDbType.VarChar);
                cmmd.Parameters["@BillNo"].Value = BillNo;
                cmmd.Parameters.Add("@IDtype", System.Data.SqlDbType.SmallInt);
                cmmd.Parameters["@IDtype"].Value = TypeId;
                cmmd.Parameters.Add("@UserId", System.Data.SqlDbType.SmallInt);
                cmmd.Parameters["@UserId"].Value = currentUserInfo.ID;
                //cmmd.Parameters.Add("@ErrMessage", System.Data.SqlDbType.NVarChar,200);
                //cmmd.Parameters["@ErrMessage"].Direction = System.Data.ParameterDirection.Output;

                System.Data.SqlClient.SqlParameter outMsg = new System.Data.SqlClient.SqlParameter();

                outMsg.ParameterName = "ErrMessage";
                outMsg.Direction = System.Data.ParameterDirection.Output;
                outMsg.SqlDbType = System.Data.SqlDbType.NVarChar;
                outMsg.Size = 200;
                cmmd.Parameters.Add(outMsg);
                DB.ExecuteNonQuery(cmmd);
                string res = string.Empty;
                if (outMsg.Value != null)
                {
                    res = outMsg.Value.ToString();
                }

                if (!string.IsNullOrEmpty(res))
                {
                    result["msg"] = res;
                }
            }
            catch (Exception ex)
            {
                result["msg"] = "生成失败：" + ex.Message;
            }
            finally
            {
                json = JSON.Encode(result);
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

    }
}
