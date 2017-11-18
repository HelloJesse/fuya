using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Data;
/// <summary>
/// Service 的摘要说明
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
// [System.Web.Script.Services.ScriptService]
public class Service : System.Web.Services.WebService {

    public Service () {

        //如果使用设计的组件，请取消注释以下行 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string HelloWorld() {
        return "Hello World";
    }
    [WebMethod]
    public System.Data.DataSet GetReport(string WarehouseID,string reportID)
    {
        string id = string.Empty;

        try
        {
            id = Nordasoft.Common.IO.Cryptography.Decrypt(reportID);
        }
        catch
        {
            return null;
        }
        string cnnstr = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;
        
        SqlCommand cmmd = new SqlCommand("SELECT * FROM Sys_AR_LIST WHERE AR_ID=@AR_ID");
        cmmd.Parameters.Add(new SqlParameter("@AR_ID", id));
        cmmd.Connection = new SqlConnection(cnnstr);
        SqlDataAdapter da = new SqlDataAdapter(cmmd);
        DataSet ds = new DataSet();
        da.Fill(ds);
        return ds;
    }

    [WebMethod]
    public System.Data.DataSet GetLotNoData(string WarehouseID)
    {
        string id = string.Empty;

        try
        {
            id = Nordasoft.Common.IO.Cryptography.Decrypt(WarehouseID);
        }
        catch
        {
            return null;
        }
        string cnnstr = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;

        SqlCommand cmmd = new SqlCommand("SELECT *  FROM [dbo].[V_RPT_B_PropertyNumPrint] WHERE [WarehouseID]=@WarehouseID AND PrintFlag=0");
        cmmd.Parameters.Add(new SqlParameter("@WarehouseID", id));
        cmmd.Connection = new SqlConnection(cnnstr);
        SqlDataAdapter da = new SqlDataAdapter(cmmd);
        DataSet ds = new DataSet();
        da.Fill(ds);
        return ds;
    }
    [WebMethod]
    public string UpdateLotPrintFlag(string WarehouseID,string ID)
    {
        string id = string.Empty;

        try
        {
            id = Nordasoft.Common.IO.Cryptography.Decrypt(ID);
        }
        catch
        {
            return "数据异常.";
        }
        string[] ids = id.Split(',');
        int tempv =0;
        foreach (string s in ids)
        {
            if (int.TryParse(s, out tempv) == false)
            {
                return "数据异常.";
            }
        }
        string cnnstr = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;

        SqlCommand cmmd = new SqlCommand(string.Format(
            "UPDATE B_Property_Print SET PrintFlag=1 WHERE [WarehouseID]=@WarehouseID AND ID IN({0}) AND PrintFlag=0",id));
        cmmd.Parameters.Add(new SqlParameter("@WarehouseID", Nordasoft.Common.IO.Cryptography.Decrypt(WarehouseID)));
        //cmmd.Parameters.Add(new SqlParameter("@WarehouseID", Nordasoft.Common.IO.Cryptography.Decrypt(WarehouseID)));
        cmmd.Connection = new SqlConnection(cnnstr);
        try
        {
            using (cmmd.Connection)
            {
                cmmd.Connection.Open();
                cmmd.ExecuteNonQuery();
            }
        }
        catch (Exception err)
        {
            return Nordasoft.Common.IO.Cryptography.Encrypt(err.Message);
        }
        return Nordasoft.Common.IO.Cryptography.Encrypt("OK");
    }

    [WebMethod]
    public string InsertOrUpdatePrinter(string WarehouseID, string PrinterName, string Desc)
    {
        string cnnstr = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;

        //如果DESC 为'',删除数据库中的记录
        string desc = Nordasoft.Common.IO.Cryptography.Decrypt(Desc);
        if (string.IsNullOrEmpty(desc))
        {

            SqlCommand delCmmd = new SqlCommand("DELETE FROM dbo.D_Warehouse_Printer WHERE WarehouseID = @WarehouseID AND PrinterName = @PrinterName");
            delCmmd.Parameters.Add(new SqlParameter("@WarehouseID", Nordasoft.Common.IO.Cryptography.Decrypt(WarehouseID)));
            delCmmd.Parameters.Add(new SqlParameter("@PrinterName", Nordasoft.Common.IO.Cryptography.Decrypt(PrinterName)));
            delCmmd.Connection = new SqlConnection(cnnstr);
            using (delCmmd.Connection)
            {
                delCmmd.Connection.Open();
                delCmmd.ExecuteNonQuery();
            }
        }
        else
        {
            SqlCommand cmmd = new SqlCommand("SELECT * FROM D_Warehouse_Printer WHERE WarehouseID = @WarehouseID AND PrinterName = @PrinterName");
            cmmd.Parameters.Add(new SqlParameter("@WarehouseID", Nordasoft.Common.IO.Cryptography.Decrypt(WarehouseID)));
            cmmd.Parameters.Add(new SqlParameter("@PrinterName", Nordasoft.Common.IO.Cryptography.Decrypt(PrinterName)));
            cmmd.Connection = new SqlConnection(cnnstr);
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(cmmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt == null || dt.Rows.Count <= 0)//保存新的打印机设备
                {
                    cmmd = new SqlCommand("INSERT INTO dbo.D_Warehouse_Printer(WarehouseID,PrinterName,Description) VALUES(@WarehouseID,@PrinterName,@Description)");
                    cmmd.Parameters.Add(new SqlParameter("@WarehouseID", Nordasoft.Common.IO.Cryptography.Decrypt(WarehouseID)));
                    cmmd.Parameters.Add(new SqlParameter("@PrinterName", Nordasoft.Common.IO.Cryptography.Decrypt(PrinterName)));
                    cmmd.Parameters.Add(new SqlParameter("@Description", desc));
                    cmmd.Connection = new SqlConnection(cnnstr);
                    using (cmmd.Connection)
                    {
                        cmmd.Connection.Open();
                        cmmd.ExecuteNonQuery();
                    }
                }
                else//更新打印机说明
                {
                    cmmd = new SqlCommand("UPDATE D_Warehouse_Printer SET DESCRIPTION = @Description WHERE WarehouseID = @WarehouseID AND PrinterName = @PrinterName");
                    cmmd.Parameters.Add(new SqlParameter("@Description", desc));
                    cmmd.Parameters.Add(new SqlParameter("@WarehouseID", Nordasoft.Common.IO.Cryptography.Decrypt(WarehouseID)));
                    cmmd.Parameters.Add(new SqlParameter("@PrinterName", Nordasoft.Common.IO.Cryptography.Decrypt(PrinterName)));
                    cmmd.Connection = new SqlConnection(cnnstr);
                    using (cmmd.Connection)
                    {
                        cmmd.Connection.Open();
                        cmmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception err)
            {
                return Nordasoft.Common.IO.Cryptography.Encrypt(err.Message);
            }
        }
        return Nordasoft.Common.IO.Cryptography.Encrypt("OK");
    }

    [WebMethod]
    public System.Data.DataSet GetPrinterData(string WarehouseID)
    {
        string id = string.Empty;

        try
        {
            id = Nordasoft.Common.IO.Cryptography.Decrypt(WarehouseID);
        }
        catch
        {
            return null;
        }
        string cnnstr = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;

        SqlCommand cmmd = new SqlCommand("SELECT * FROM [dbo].[D_Warehouse_Printer] WHERE [WarehouseID]=@WarehouseID");
        cmmd.Parameters.Add(new SqlParameter("@WarehouseID", id));
        cmmd.Connection = new SqlConnection(cnnstr);
        SqlDataAdapter da = new SqlDataAdapter(cmmd);
        DataSet ds = new DataSet();
        da.Fill(ds);
        return ds;
    }

    [WebMethod]
    public string DelPrinterData(string WarehouseID, string oldPrinterNames)
    {

        string printerNames = string.Empty;
        string cnnstr = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;
        
        try
        {
            printerNames = Nordasoft.Common.IO.Cryptography.Decrypt(oldPrinterNames);

            SqlCommand cmmd = new SqlCommand(string.Format("DELETE FROM dbo.D_Warehouse_Printer WHERE WarehouseID = @WarehouseID AND PrinterName IN ({0})", printerNames));
            cmmd.Parameters.Add(new SqlParameter("@WarehouseID", Nordasoft.Common.IO.Cryptography.Decrypt(WarehouseID)));
            cmmd.Connection = new SqlConnection(cnnstr);
            using (cmmd.Connection)
            {
                cmmd.Connection.Open();
                cmmd.ExecuteNonQuery();
            }
        }
        catch (Exception err)
        {
            return Nordasoft.Common.IO.Cryptography.Encrypt(err.Message);
        }
        return Nordasoft.Common.IO.Cryptography.Encrypt("OK"); 
    }
}
