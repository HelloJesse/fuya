<%@ Application Language="C#" %>

<script runat="server">

    System.Threading.Thread _BackThread;    //后台线程
    //Nordasoft.Data.Sql.DataBase _ConfigDB;
    void Application_Start(object sender, EventArgs e) 
    {
        // 在应用程序启动时运行的代码
        string cnn=System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;
        //类型名影射表名，若没有则不指定
        VLP.BS.CacheManager.GetTableNameByTypeName = Common.GetTableNameByTypeName;
        //处理下拉框中特殊处理，若没有则不指定
        VLP.BS.ComboBoxManager.HandlerTableNameDelegate = Common.ComboBoxTableNameHander;
        //设置特殊下拉，若没有则不指定
        VLP.BS.CacheManager.InitSpecialComboBoxSqlSetting = Common.InitSpecialComboBoxSqlSetting;
        //初始相关缓存命令
        VLP.BS.AutoCommandManager._CNN = cnn;
        VLP.BS.AutoCommandManager.InitCommand();
        VLP.BS.CacheManager.InitCache(cnn);
        VLP.BS.AutoCommandManager.Key = System.Configuration.ConfigurationManager.AppSettings["key"];
        _BackThread = new System.Threading.Thread(BackThreadManager);
        _BackThread.Start();    
        
    }

    void BackThreadManager(object sender)
    {
        VLP.BS.VLPHandler.HandChangeFile();
        
    }

    

    void CloseBackThreadManager()
    {
        VLP.BS.VLPHandler.HandChangeFileDispose();
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  在应用程序关闭时运行的代码
        CloseBackThreadManager();
    }
        
    void Application_Error(object sender, EventArgs e) 
    { 
        // 在出现未处理的错误时运行的代码
        try{
        Nordasoft.Common.IO.FileLog log = new Nordasoft.Common.IO.FileLog(AppDomain.CurrentDomain.BaseDirectory, "errlog.txt");
        Exception err=Server.GetLastError();
        log.AddLogInfo(string.Format("日期：{0}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
            string.Format("{0}{1}{2}",err.Message,Environment.NewLine,err.ToString()));
        }
        catch{
            //忽略记录时发生的错误
        }
        
    }

    void Session_Start(object sender, EventArgs e) 
    {
        // 在新会话启动时运行的代码
        //string id = this.Session.SessionID;
        //string cnn= System.Configuration.ConfigurationManager.ConnectionStrings["VLP"].ConnectionString;
        //LoginManager.AddSession(this.Session.SessionID, this.Request.UserHostAddress, this.Request.UserHostName,
        //new Nordasoft.Data.Sql.DataBase(cnn));
        
    }

    void Session_End(object sender, EventArgs e) 
    {
        //string cnn = System.Configuration.ConfigurationManager.ConnectionStrings["VLP"].ConnectionString;
        try
        {
            string cnnstr = Session["SystemConnection"].ToString();
            //LoginManager.DropSession(this.Session.SessionID, new Nordasoft.Data.Sql.DataBase(cnnstr));
        }
        catch
        {
        }
        // 在会话结束时运行的代码。 
        // 注意: 只有在 Web.config 文件中的 sessionstate 模式设置为
        // InProc 时，才会引发 Session_End 事件。如果会话模式设置为 StateServer
        // 或 SQLServer，则不引发该事件。

    }
       
</script>
