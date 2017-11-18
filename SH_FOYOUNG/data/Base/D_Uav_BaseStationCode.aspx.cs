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
using Nordasoft.Data.Sql;

public partial class data_D_Uav_BaseStation : BasePage
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
    /// 操作 基站与客户关系列表信息
    /// </summary>
    public void UpdateD_Uav_BaseForCustomer() 
    {
        string baseStationID = Request["BaseStationID"];
        string json = Request.Form.Get("data");

        ArrayList rows = (ArrayList)VLP.JSON.Decode(json);
        if (rows == null || rows.Count <= 0)
        {
            return;
        }
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            Collection<SqlCommand> cmmds = new Collection<SqlCommand>();

            foreach (Hashtable row in rows)
            {
                String id = row["DTLID"] != null ? row["DTLID"].ToString() : "";
                //根据记录状态，进行不同的增加、修改操作
                String state = row["_state"] != null ? row["_state"].ToString() : "";
                if (state == "added" || id == "" || id == "0")           //新增：id为空，或_state为added
                {
                    cmmds.Add(UpdateD_Uav_BaseForCustomerCmmd(row, baseStationID, 0));
                }
                else if (state == "modified" || state == "") //更新：_state为空或modified
                {
                    cmmds.Add(UpdateD_Uav_BaseForCustomerCmmd(row, baseStationID, 1));
                }
                else if (state == "removed" || state == "deleted")
                {
                    cmmds.Add(UpdateD_Uav_BaseForCustomerCmmd(row, baseStationID, 2));
                }
            }
            string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
            if (!string.IsNullOrEmpty(res))
            {
                result.IsOK = false;
                result.ErrMessage = res;
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    private SqlCommand UpdateD_Uav_BaseForCustomerCmmd(Hashtable row, string baseStationID, int Flag)
    {
        SqlCommand cmmd = new SqlCommand("sp_UpdateD_Uav_BaseForCustomer");
        cmmd.CommandType = CommandType.StoredProcedure;
        cmmd.Parameters.Add("@DTLID", SqlDbType.Int);
        if (Flag == 0)
        {
            cmmd.Parameters["@DTLID"].Value = "0";
        }
        else
        {
            cmmd.Parameters["@DTLID"].Value = row["DTLID"].ToString();
        }
        cmmd.Parameters.Add("@BaseStationID", SqlDbType.Int);
        cmmd.Parameters["@BaseStationID"].Value = baseStationID;

        cmmd.Parameters.Add("@CustomerID", SqlDbType.Int);
        cmmd.Parameters["@CustomerID"].Value = row["CustomerID"];

        cmmd.Parameters.Add("@USERID", SqlDbType.Int);
        cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

        cmmd.Parameters.Add("@Flag", SqlDbType.Int);
        cmmd.Parameters["@Flag"].Value = Flag;

        return cmmd;
    }


    /// <summary>
    /// 操作 游标列表信息
    /// </summary>
    public void UpdateD_Uav_BaseForPloc()
    {
        string baseStationID = Request["BaseStationID"];
        string json = Request.Form.Get("data");

        ArrayList rows = (ArrayList)VLP.JSON.Decode(json);
        if (rows == null || rows.Count <= 0)
        {
            return;
        }
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            Collection<SqlCommand> cmmds = new Collection<SqlCommand>();

            foreach (Hashtable row in rows)
            {
                String id = row["ID"] != null ? row["ID"].ToString() : "";
                //根据记录状态，进行不同的增加、修改操作
                String state = row["_state"] != null ? row["_state"].ToString() : "";
                if (state == "added" || id == "" || id == "0")           //新增：id为空，或_state为added
                {
                    cmmds.Add(UpdateD_Uav_BaseForPlocCmmd(row, baseStationID, 0));
                }
                else if (state == "modified" || state == "") //更新：_state为空或modified
                {
                    cmmds.Add(UpdateD_Uav_BaseForPlocCmmd(row, baseStationID, 1));
                }
                else if (state == "removed" || state == "deleted")
                {
                    cmmds.Add(UpdateD_Uav_BaseForPlocCmmd(row, baseStationID, 2));
                }
            }
            string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
            if (!string.IsNullOrEmpty(res))
            {
                result.IsOK = false;
                result.ErrMessage = res;
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    private SqlCommand UpdateD_Uav_BaseForPlocCmmd(Hashtable row, string baseStationID, int Flag)
    {
        SqlCommand cmmd = new SqlCommand("sp_UpdateD_Uav_BaseForPloc");
        cmmd.CommandType = CommandType.StoredProcedure;
        cmmd.Parameters.Add("@ID", SqlDbType.Int);
        if (Flag == 0)
        {
            cmmd.Parameters["@ID"].Value = "0";
        }
        else
        {
            cmmd.Parameters["@ID"].Value = row["ID"];
        }
        cmmd.Parameters.Add("@BaseStationID", SqlDbType.Int);
        cmmd.Parameters["@BaseStationID"].Value = baseStationID;

        cmmd.Parameters.Add("@LineType", SqlDbType.Int);
        cmmd.Parameters["@LineType"].Value = row["LineType"];

        cmmd.Parameters.Add("@Name", SqlDbType.NVarChar, 20);
        cmmd.Parameters["@Name"].Value = row["Name"];

        cmmd.Parameters.Add("@Sortid", SqlDbType.Int);
        cmmd.Parameters["@Sortid"].Value = row["Sortid"];

        cmmd.Parameters.Add("@Longitude", SqlDbType.Decimal);
        cmmd.Parameters["@Longitude"].Value = row["Longitude"];

        cmmd.Parameters.Add("@Latitude", SqlDbType.Decimal);
        cmmd.Parameters["@Latitude"].Value = row["Latitude"];

        cmmd.Parameters.Add("@USERID", SqlDbType.Int);
        cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

        cmmd.Parameters.Add("@Flag", SqlDbType.Int);
        cmmd.Parameters["@Flag"].Value = Flag;

        return cmmd;
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