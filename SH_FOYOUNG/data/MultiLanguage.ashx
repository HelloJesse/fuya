<%@ WebHandler Language="C#" Class="MultiLanguage" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Data;

public class MultiLanguage : BasePage, IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{
    HttpContext _context;
    public void ProcessRequest(HttpContext context)
    {
        _context = context;
        string methodName = _context.Request["method"]; //加载方法
        if (BasePage.IsOut(context)) return;
        if (methodName == "GetEnValue")
        {
            GetEnValue();
        }
    }
    
    /// <summary>
    /// 获取中文对应的英文名称
    /// </summary>
    /// <param name="context"></param>
    private void GetEnValue()
    {
        string cnvalue = _context.Request["ML_CN"];
        string envalue = cnvalue;
        string sql = @"select ML_CN,ML_EN,ML_HK from dbo.D_MultiLanguage where ML_CN =@ML_CN";

        System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand(sql);
        cmmd.Parameters.Add("@ML_CN", System.Data.SqlDbType.NVarChar,200);
        cmmd.Parameters["@ML_CN"].Value = cnvalue;
        
        System.Data.DataTable dt = DB.ExecuteDataTable(cmmd);

        if (dt != null && dt.Rows.Count > 0)
        {
            envalue = dt.Rows[0]["ML_EN"].ToString();
        }
        System.Collections.Hashtable result = new System.Collections.Hashtable();
        result["ML_EN"] = envalue;
        _context.Response.Write(VLP.JSON.Encode(result));
        
        
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