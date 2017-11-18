<%@ WebHandler Language="C#" Class="PropertyAction" %>

using System;
using System.Web;

public class PropertyAction : BasePage, IHttpHandler, System.Web.SessionState.IReadOnlySessionState{
    
    
    public void ProcessRequest (HttpContext context) {
        string customid = context.Request["id"]; //加载cunstomid值
        string Optype = context.Request["type"]; //
        string isReadyOnly = context.Request["isReadyonly"]; //
        try
        {
            System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand("sp_D_GetCustomerPropertyGridColumns");
            cmmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmmd.Parameters.Add("@Customid", System.Data.SqlDbType.Int);
            cmmd.Parameters["@Customid"].Value = customid;
            cmmd.Parameters.Add("@Optype", System.Data.SqlDbType.TinyInt);
            cmmd.Parameters["@Optype"].Value = Optype;
            System.Data.DataSet ds = DB.ExecuteDataset(cmmd);
            System.Collections.Hashtable result = new System.Collections.Hashtable();
            if (ds != null && ds.Tables.Count > 0)
            {
                System.Data.DataTable dtCol = ds.Tables[0];
                System.Data.DataTable dtDrop = ds.Tables[1];
                result["iswrong"] = 0;
                result["newcols"] = PropertyTable2Array(dtCol, dtDrop, isReadyOnly); 
            }
            else
            {
                result["iswrong"] = 1;
                result["newcols"] = ""; 
            }

            context.Response.Write(VLP.JSON.Encode(result));
        }
        catch(Exception ex)
        {
            System.Collections.Hashtable result = new System.Collections.Hashtable();
            result["iswrong"] = 1;
            result["newcols"] = ex.Message;
            context.Response.Write(VLP.JSON.Encode(result));
        }
        
    }

    /// <summary>
    /// Table转为ArrayList
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static System.Collections.ArrayList PropertyTable2Array(System.Data.DataTable dtCol, System.Data.DataTable dtDrop,string isReadonly)
    {
        System.Collections.ArrayList array = new System.Collections.ArrayList();
        for (int i = 0; i < dtCol.Rows.Count; i++)
        {
            System.Data.DataRow row = dtCol.Rows[i];
            System.Collections.Hashtable record = new System.Collections.Hashtable();
            record["field"] = "Property" + row["PropertyNum"].ToString();
            record["width"] = 100;
            record["headerAlign"] = "center";
            if ((bool)row["IsMustDo"] == true)
            {
                record["vtype"] = "required";
            }
            //allowinput="true"
            record["header"] = row["PropertyName"].ToString();
            if (row["PropertyDataType"].ToString() == "2")
            {
                record["dateFormat"] = "yyyy-MM-dd";
            }
            if (isReadonly == "0" && (bool)row["IsEdit"] == true)
            {
                record["editor"] = PropertyTable2ArrayEdit(row, dtDrop)[0];
            }
            array.Add(record);
        }
        return array;
    }

    private static System.Collections.ArrayList PropertyTable2ArrayEdit(System.Data.DataRow row, System.Data.DataTable dtDrop)
    {
        System.Collections.ArrayList array = new System.Collections.ArrayList();
        System.Collections.Hashtable record = new System.Collections.Hashtable();
        if ((bool)row["IsDropDown"] == false)
        {//不是下拉
            if (row["PropertyDataType"].ToString() == "0")
            {//文本
                record["type"] = "textbox";
                record["minValue"] = 0;
                record["maxValue"] = 20;
            }
            else if (row["PropertyDataType"].ToString() == "1")
            { //数值
                record["type"] = "spinner";
            }
            else if (row["PropertyDataType"].ToString() == "2")
            {//日期
                record["type"] = "datepicker";
                record["valueFormat"] = "yyyy-MM-dd";
            }
            record["allowinput"] = "true";
        }
        else
        {
            record["type"] = "combobox";
            record["allowinput"] = "true";
            record["data"] = PropertyTable2ArrayDropDownData(row["ID"].ToString(), dtDrop);
        }
        
        array.Add(record);

        return array;
    }

    private static System.Collections.ArrayList PropertyTable2ArrayDropDownData(string ids, System.Data.DataTable dtDrop)
    {
        System.Collections.ArrayList array = new System.Collections.ArrayList();
        if (dtDrop != null && dtDrop.Select(string.Format("ProID={0}", ids)).Length > 0)
        {
            foreach (System.Data.DataRow rows in dtDrop.Select(string.Format("ProID={0}", ids)))
            {
                System.Collections.Hashtable record = new System.Collections.Hashtable();
                record["id"] = rows["ValueName"];
                record["text"] = rows["ValueName"];
                array.Add(record);
            }
        }

        return array;
    }
    
    
    
    public bool IsReusable {
        get {
            return false;
        }
    }

}