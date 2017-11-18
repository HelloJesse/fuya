using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Sql;
using System.Data.SqlClient;
using VLP.BS;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Text;

/// <summary>
/// 用户自定义列
/// </summary>
public partial class data_UserColumnsCode : BasePage
{
    UserInfo currentUserInfo = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        currentUserInfo = GetUserInfo;
        if (IsOut(this.Context)) return;

        string methodName = Request["method"];
        Type type = this.GetType();
        MethodInfo method = type.GetMethod(methodName);
        if (method == null) throw new Exception("method is null");
        try
        {
            BeforeInvoke(methodName);
            method.Invoke(this, null);
        }
        catch (Exception ex)
        {
            Hashtable result = new Hashtable();
            result["error"] = -1;
            result["message"] = ex.Message;
            result["stackTrace"] = ex.StackTrace;
            String json = VLP.JSON.Encode(result);
            Response.Clear();
            Response.Write(json);
        }
        finally
        {
            AfterInvoke(methodName);
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

    /// <summary>
    /// 获取用户对应的列信息
    /// </summary>
    public void GetUserColumns()
    {
        //查询条件
        string pageId = Request["PageId"];
        LoadFormResultN result = new LoadFormResultN();
        try
        {
            string language = BasePage.GetLanguageParam(Request);
            result.DataList = UserColumns.GetUserColumnData(pageId, currentUserInfo.ID.ToString(), language, DB); 
            result.IsOK = true;
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }


    /// <summary>
    /// 保存用户自定义列信息
    /// </summary>
    public void UpdateUserColumnsData()
    {
        string pageId = Request["PageId"];
        string jsonData = Request["data"];
        LoadFormResultN result = new LoadFormResultN();
        string language = BasePage.GetLanguageParam(Request);
        try
        {
            //转换成数组资料
            ArrayList rowsD = (ArrayList)VLP.JSON.Decode(jsonData);
            string fieldID = string.Empty;
            StringBuilder sbFileldID = new StringBuilder();
            StringBuilder sbFileldIDWidth = new StringBuilder();
            Hashtable map = new Hashtable();
            double dOrder = 0;
            foreach (Hashtable row in rowsD)
            {
                if (map.ContainsKey(double.Parse(row["ColOrder"].ToString())))
                {
                    dOrder = dOrder + 0.01;
                    map.Add((dOrder + double.Parse(row["ColOrder"].ToString())), row["FieldID"]);
                }
                else
                {
                    map.Add(double.Parse(row["ColOrder"].ToString()), row["FieldID"]);
                }
                if (row["Width"] != null && !string.IsNullOrEmpty(row["Width"].ToString()))
                {
                    sbFileldIDWidth.AppendFormat("{0}:{1};", row["ShowName"], row["Width"]);
                }
            }
            ArrayList list = new ArrayList(map.Keys);
            list.Sort();
            foreach (double str in list)
            {
                sbFileldID.AppendFormat("{0},", map[str]); 
            }

            //添加必填列
            DataTable dtField = UserColumns.GetNecessaryFields(pageId, DB);
            if (dtField != null && dtField.Rows.Count > 0)
            {
                Boolean bl = true;
                foreach (DataRow dr in dtField.Rows)
                {
                    bl = true;
                    foreach (Hashtable item in rowsD)
                    {
                        if (item["FieldID"].ToString().Equals(dr["FieldID"].ToString()))
                        {
                            bl = false;
                            continue; 
                        }
                    }
                    if (bl)//用户未勾选必填列时，添加
                    {
                        sbFileldID.AppendFormat("{0},", dr["FieldID"]);
                        sbFileldIDWidth.AppendFormat("{0}:{1};", dr["ShowName"], dr["Width"]);
                    }
                }
            }
            if (sbFileldID.Length > 0)
            {
                fieldID = sbFileldID.ToString().Substring(0, sbFileldID.Length - 1); 
            }
            UserColumns.UpdateUserColumnsInfo(pageId, fieldID, sbFileldIDWidth.ToString(), currentUserInfo.ID.ToString(), language, DB);
            result.IsOK = true;
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    public partial class LoadFormResultN
    {
        /// <summary>
        /// DataList 用户对应的列信息
        /// </summary>
        public DataTable DataList = null;
        /// <summary>
        /// 其它
        /// </summary>
        public bool IsOK = false;
        public string ErrMessage = string.Empty;
    }
}