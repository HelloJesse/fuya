<%@ WebHandler Language="C#" Class="RoleManager" %>

using System;
using System.Web;
using System.Web.SessionState;
using VLP.BS;
using VLP.Search;

public class RoleManager : BasePage, IHttpHandler, IReadOnlySessionState, IRequiresSessionState
{
    UserInfo currentUserInfo = null;
    
    public new void ProcessRequest (HttpContext context) {
        //超时直接退出
        if (BasePage.IsOut(context)) return;
        
        string methodName = context.Request.Params["method"];
        string tableid = context.Request.Params["id"];
        String submitJSON = context.Request["submitData"];
        if (methodName == "getbasedata")
        {
            string tableName=context.Request["tablekey"];
            string roleid=context.Request["roleid"];
            context.Response.Write(VLP.JSON.Encode(VLP.BS.Role.GetBaseTableInfo(tableName, roleid, DB)));
            
        }
        else if (methodName == "getbasetree")
        {
            string roleid = context.Request["roleid"];
            context.Response.Write(VLP.JSON.Encode(VLP.BS.Role.GetBaseTableInfo(roleid, DB)));
        }
        else
        {
            Type type = this.GetType();
            if (string.IsNullOrEmpty(methodName))
            {//如果前台没有传递特殊处理的方法，则用系统默认
                //methodName = "SearchGridList";
            }
            System.Reflection.MethodInfo method = type.GetMethod(methodName);
            if (methodName == null) throw new Exception("method is null");
            currentUserInfo = GetUserInfo;
            try
            {
                //BeforeInvoke(method);
                method.Invoke(this,new object[]{context});
            }
            catch (Exception ex)
            {
                System.Collections.Hashtable result = new System.Collections.Hashtable();
                result["error"] = -1;
                result["message"] = ex.Message;
                result["stackTrace"] = ex.StackTrace;
                String json = VLP.JSON.Encode(result);
                context.Response.Clear();
                context.Response.Write(json);
            }
            finally
            {
                //AfterInvoke(method);
            }
        }
        
    }
    
    public void GetRoleTree(HttpContext context)
    {
        context.Response.Write(VLP.JSON.Encode(VLP.BS.Role.GetRoleTable(DB)));   
    }
    #region//菜单设置相关方法
    public void GetMenuSetting(HttpContext context)
    {

        string roleid = context.Request["RoleID"].ToString();
        string menuid = context.Request["MenuID"].ToString();

        context.Response.Write(VLP.JSON.Encode(VLP.BS.Role.GetMenuSetting(roleid, menuid, DB)));

    }
    /// <summary>
    /// 获取菜单对应的操作项
    /// </summary>
    /// <param name="context"></param>
    public void GetMenuOpFields(HttpContext context)
    {
        String submitJSON = context.Request["submitData"];

        System.Collections.Hashtable data = (System.Collections.Hashtable)VLP.JSON.Decode(submitJSON);

        string menuid = data["MenuID"].ToString();
        VLP.BS.DataResult result = new DataResult();
        try
        {

            result.Data = VLP.BS.Role.GetMenuOpFields(menuid, DB);
            result.IsOK = true;

        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result)); 
        
    }
    #endregion
    
    public void SaveUserViewRole(HttpContext context)
    {
        String submitJSON = context.Request["submitData"];

        System.Collections.Hashtable data = (System.Collections.Hashtable)VLP.JSON.Decode(submitJSON);

        string userid = data["userid"].ToString();
        string ids = data["ids"].ToString();
        VLP.BS.Result result = new Result();
        try
        {

            VLP.BS.Role.SaveUserViewRole(userid, ids, DB);
            result.IsOK = true;
            
        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result));    
    }
    /// <summary>
    /// 保存模块功能项
    /// </summary>
    /// <param name="context"></param>
    public void SaveFunctionSetting(HttpContext context)
    {
        String submitJSON = context.Request["submitData"];

        System.Collections.Hashtable data = (System.Collections.Hashtable)VLP.JSON.Decode(submitJSON);

        string RoleID = data["RoleID"].ToString();
        string MenuID = data["MenuID"].ToString();
        string configs = data["Config"].ToString();
        VLP.BS.Result result = new Result();
        try
        {

            VLP.BS.Role.SaveFunctionSetting(MenuID, RoleID, configs,DB);
            result.IsOK = true;

        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
     /// <summary>
    /// 保存查询模块功能项
    /// </summary>
    /// <param name="context"></param>
    public void SaveSearchFunctionSetting(HttpContext context)
    {
        string RoleID = context.Request.Form["RoleID"].ToString();
        string MenuID = context.Request.Form["MenuID"].ToString();
        string configs = context.Request.Form["Config"].ToString();
        VLP.BS.Result result = new Result();
        try
        {

            VLP.BS.Role.SaveSearchFunctionSetting(MenuID, RoleID, configs, DB);
            result.IsOK = true;

        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
    
    /// <summary>
    /// 保存可见菜单
    /// </summary>
    /// <param name="context"></param>
    public void SaveViewMenu(HttpContext context)
    {
        String submitJSON = context.Request["submitData"];

        System.Collections.Hashtable data = (System.Collections.Hashtable)VLP.JSON.Decode(submitJSON);

        string RoleID = data["RoleID"].ToString();
        string viewMenuID = data["ViewMenu"].ToString();
        VLP.BS.Result result = new Result();
        try
        {

            VLP.BS.Role.SaveViewMenu(RoleID, viewMenuID, DB);
            result.IsOK = true;

        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
    
    public void GetUserRoleTree(HttpContext context)
    {
        string userid = context.Request["userid"];
        context.Response.Write(VLP.JSON.Encode(VLP.BS.Role.GetUserRoleTree(userid, DB)));
    }
    public void GetMenuTree(HttpContext context)
    {
        string roleid = context.Request["roleid"];
        string json = VLP.JSON.Encode(VLP.BS.Role.GetMenuData(roleid, DB));
        context.Response.Write(json);
    }
    /// <summary>
    /// 保存可见基础数据
    /// </summary>
    /// <param name="context"></param>
    public void SaveViewBaseData(HttpContext context)
    {
        String submitJSON = context.Request["submitData"];
        System.Collections.Hashtable data = (System.Collections.Hashtable)VLP.JSON.Decode(submitJSON);

        string roleid = data["RoleID"].ToString();
        string basetable = data["BaseTable"].ToString();
        string ids = data["IDS"].ToString();
        
        VLP.BS.Result result = new Result();
        if (string.IsNullOrEmpty(basetable) || string.IsNullOrEmpty(roleid))
        {
            result.ErrMessage = "参数不正确.";
        }
        else
        {
            System.Data.DataTable dt = VLP.BS.Role.SaveViewBaseData(roleid, basetable, ids, DB);
            if (dt.Rows[0][0].ToString().Length > 0)
            {
                result.ErrMessage = dt.Rows[0][0].ToString();
            }
            else
            {
                result.IsOK = true;
            }
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
    /// <summary>
    /// 获取功能按钮
    /// </summary>
    /// <param name="pageid"></param>
    /// <param name="flag"></param>
    public void GetSearchFunction(HttpContext context)
    {

        String pageid = context.Request["PageID"];
        string language = BasePage.GetLanguage(context.Request);
        //System.Collections.Hashtable data = (System.Collections.Hashtable)JSON.Decode(submitJSON);
        //string pageid = data["PageID"].ToString();


        System.Data.DataSet ds = VLP.BS.Role.GetSearchFunctionPopedom(this.GetUserInfo.ID.ToString(), pageid, DB);
        System.Data.DataTable dt = ds.Tables[1];
        System.Data.DataTable dtallbtn = ds.Tables[0];
        System.Data.DataTable dtviewbtn = new System.Data.DataTable();
        dtviewbtn.Columns.Add("Text", typeof(string));
        dtviewbtn.Columns.Add("Ico", typeof(string));
        dtviewbtn.Columns.Add("Click", typeof(string));

        string userid = this.GetUserInfo.ID.ToString();

        string[] btnconfigs = dt.Rows[0]["SearchButton"].ToString().Split('/');
        VLP.BS.DataResult result = new DataResult();
        ;
        string systemCompany = Common.GetSysCompany(this.Session);

        System.Collections.ArrayList temparray = new System.Collections.ArrayList();
        int i = 0;
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
        //foreach (string c in btnconfigs)
        //{
        //    string[] config = c.Split('.');
        //    if (config.Length != 2)
        //        continue;
        //    System.Data.DataRow[] rows = CacheManager.FunctionTable.Select(string.Format("ID={0}", config[0]));
        //    if (rows.Length != 1)
        //    {
        //        throw new ApplicationException(string.Format("未找到ID为[{0}]的功能项.", config[0]));
        //    }
        //    if (!(bool)rows[0]["IsCheckPopedom"]        //不需要检查权限
        //       || CacheManager.IsAdmin(this.GetUserInfo.ID.ToString()) //或者管理员
        //       || dtallbtn.Select(string.Format("ID={0}", config[0])).Length == 1  //有权限
        //       )
        //    {
        //        temparray.Add(c);
        //        //rconfigs[i] = c;
        //        //i++;
        //    }
        //}
        //string[] rconfigs = new string[temparray.Count];
        //temparray.CopyTo(rconfigs);
        dtviewbtn = VLP.BS.SettingManager.GetButtonTable(temparray, this.Session, language);
        //foreach (string c in btnconfigs)
        //{
        //    string[] config=c.Split(',');
        //    if(config.Length != 2)
        //        continue;

        //    System.Data.DataRow[] rows= CacheManager.FunctionTable.Select(string.Format("ID={0}", config[0]));
        //    if (rows.Length != 1)
        //    {
        //        throw new ApplicationException(string.Format("未找到ID为[{0}]的功能项.",config[0]));
        //    }
        //    string name = rows[0]["Name"].ToString();
        //    string ico = rows[0]["ICO"].ToString();

        //    if (!(bool)rows[0]["IsCheckPopedom"]        //不需要检查权限
        //        || CacheManager.IsAdmin(this.GetUserInfo.ID.ToString()) //或者管理员
        //        || dtallbtn.Select(string.Format("ID={0}", config[0])).Length == 1  //有权限
        //        )   
        //    {
        //        System.Data.DataRow row = dtviewbtn.NewRow();
        //        row["Text"] = name;
        //        row["Ico"] = ico;
        //        row["click"] = config[1];
        //        dtviewbtn.Rows.Add(row);
        //    }
        //}
        result.Data = dtviewbtn;
        result.IsOK = true;
        context.Response.Write(VLP.JSON.Encode(result));


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
    /// <summary>
    /// 获取功能按钮
    /// </summary>
    /// <param name="pageid"></param>
    /// <param name="flag"></param>
    public void GetUserList(HttpContext context)
    {
        VLP.BS.DataResult result = new DataResult();


        string roleid = context.Request.Form ["RoleID"].ToString();
        string pageid = context.Request.Form["PageID"].ToString();
        if (string.IsNullOrEmpty(pageid) || string.IsNullOrEmpty(roleid))
        {
            result.ErrMessage = "参数不正确.";
        }
        else
        {
            System.Data.DataTable dt = VLP.BS.Role.GetUserList(pageid, roleid, this.GetUserInfo.ID.ToString(), DB);
            result.Data = dt;
            result.IsOK = true;
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
    /// <summary>
    /// 获取功能按钮
    /// </summary>
    /// <param name="pageid"></param>
    /// <param name="flag"></param>
    public void GetUserTree(HttpContext context)
    {
        string roleid = context.Request.Form["RoleID"];
        System.Data.DataTable dt = VLP.BS.Role.GetUserTreeData(roleid, DB);

        context.Response.Write(VLP.JSON.Encode(dt));
    }
    /// <summary>
    /// 获取功能按钮
    /// </summary>
    /// <param name="pageid"></param>
    /// <param name="flag"></param>
    public void RemoveUser(HttpContext context)
    {
        VLP.BS.DataResult result = new DataResult();


        string roleid = context.Request.Form["RoleID"].ToString();
        string userids = context.Request.Form["UserIDS"].ToString();
        try
        {
            VLP.BS.Role.RemoveUser(roleid, userids, DB);
            result.IsOK = true;
        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
    /// <summary>
    /// 获取功能按钮
    /// </summary>
    /// <param name="pageid"></param>
    /// <param name="flag"></param>
    public void AddUserToRole(HttpContext context)
    {
        VLP.BS.DataResult result = new DataResult();

        try
        {
            string roleid = context.Request.Form["RoleID"].ToString();
            string userids = context.Request.Form["UserIDS"].ToString().Replace("U", "");
            VLP.BS.Role.AddUser(roleid, userids, DB);
            result.IsOK = true;
        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
    /// <summary>
    /// 添加可查看的指定用户ID    
    /// </summary>
    /// <param name="pageid"></param>
    /// <param name="flag"></param>
    public void AddOPViewUserID(HttpContext context)
    {
        VLP.BS.DataResult result = new DataResult();

        try
        {
            string roleid = context.Request.Form["RoleID"].ToString();
            string opfield = context.Request.Form["OPField"].ToString();
            string menuid = context.Request.Form["MenuID"].ToString();
            string userids = context.Request.Form["UserIDS"].ToString().Replace("U", "");
            VLP.BS.Role.AddOPViewUser(roleid, opfield, menuid, userids, DB);
            result.IsOK = true;
        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
    /// <summary>
    /// 获取指定操作、菜单、角色可查看的用户   
    /// </summary>
    public void GetOPUserList(HttpContext context)
    {
        VLP.BS.DataResult result = new DataResult();

        try
        {
            string roleid = context.Request.Form["RoleID"].ToString();
            string opfield = context.Request.Form["OPField"].ToString();
            string menuid = context.Request.Form["MenuID"].ToString();
            result.Data = VLP.BS.Role.GetOPUserList(roleid, opfield, menuid, DB);
            result.IsOK = true;
        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
    /// <summary>
    /// 移除指定操作、菜单、角色可查看的用户   
    /// </summary>
    public void RemoveViewUserID(HttpContext context)
    {
        VLP.BS.DataResult result = new DataResult();

        try
        {
            string roleid = context.Request.Form["RoleID"].ToString();
            string opfield = context.Request.Form["OPField"].ToString();
            string menuid = context.Request.Form["MenuID"].ToString();
            string userid = context.Request.Form["UserID"].ToString();
            result.Data = VLP.BS.Role.RemoveViewUserID(roleid, opfield, menuid, userid, DB);
            result.IsOK = true;
        }
        catch (Exception err)
        {
            result.ErrMessage = err.Message;
        }
        context.Response.Write(VLP.JSON.Encode(result));
    }
    
    public bool IsReusable {
        get {
            return false;
        }
    }
    /// <summary>
    /// 读取导出模板配置
    /// </summary>
    /// <param name="context"></param>
    public void GetBaseDataMenuTree(HttpContext context)
    {
        string userID = context.Request["userID"];
        //采用统一的配置库
        string cnn = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;

        string json = VLP.JSON.Encode(VLP.BS.Role.GetBaseDataMenuData(userID, new Nordasoft.Data.Sql.DataBase(cnn)));
        context.Response.Write(json);
    }
    public void GetBaseDataFields(HttpContext context)
    {
        string pageID = context.Request["ID"];
        string cnn = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;
        string json = VLP.JSON.Encode(new ImportExcleData(cnn, currentUserInfo.ID.ToString()).GetExcleTemplate(pageID, false));
        context.Response.Write(json);
    }

}

