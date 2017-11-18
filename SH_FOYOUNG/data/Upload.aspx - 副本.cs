using ExcelLibrary.SpreadSheet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using VLP.WMS;

public partial class data_Upload : BasePage
{
    UserInfo currentUserInfo = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        currentUserInfo = GetUserInfo;
        if (currentUserInfo == null)
        {
            Response.Write("{\"isOut\":\"1\"}");
            return;
        }

        //上传文件
        HttpPostedFile uploadFile = Request.Files["Fdata"];
        if (uploadFile != null)
        {
            string tempFile = Request.PhysicalApplicationPath;// 获得程序路径
            if (uploadFile.ContentLength > 0)
            {
                string strFileDir = string.Format("{0}{1}{2}", tempFile, "upload\\", Guid.NewGuid() + uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf('.')));
                uploadFile.SaveAs(strFileDir);
                Response.Write(strFileDir);
            }
            return;
        }

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
            String json = JSON.Encode(result);
            Response.Clear();
            Response.Write(json);
        }
        finally
        {
            AfterInvoke(methodName);
        }
    }

    //权限管理
    protected void BeforeInvoke(String methodName){}

    //日志管理
    protected void AfterInvoke(String methodName){}
    
    /// <summary>
    /// 基础数据由模板Excel导入
    /// </summary>
    public void ImportBaseDataModuleExcel()
    {
        string fileDir=Request["FileDir"];
        StringBuilder sbSql = new StringBuilder();
        DataTable dtFile = null;
        using (FileStream fileStream = new FileStream(fileDir, FileMode.Open))
        {
            ExcelLibrary.SpreadSheet.Workbook book = Workbook.Load(fileStream);
            Worksheet worksheet = book.Worksheets[0];
            dtFile = new DataTable();
            dtFile.TableName = book.Worksheets[0].Name;
            if (worksheet.Cells.LastRowIndex < 1)
            {
                //文件不对
            }
            for (int j = 0; j <= worksheet.Cells.LastColIndex; j++)
            {
                string cname = worksheet.Cells[0, j].Value.ToString().Trim();
                if (string.IsNullOrEmpty(cname))
                    break;
                dtFile.Columns.Add(cname);
            }
            for (int i = 1; i <= worksheet.Cells.LastRowIndex; i++)
            {
                DataRow row = dtFile.NewRow();
                for (int j = 0; j <= worksheet.Cells.LastColIndex; j++)
                {
                    string cname = worksheet.Cells[0, j].Value.ToString().Trim();
                    if (string.IsNullOrEmpty(cname))
                        break;
                    row[j] = worksheet.Cells[i, j].Value == null ? DBNull.Value : worksheet.Cells[i, j].Value;
                }
                dtFile.Rows.Add(row);
            }
        }
        
        
        System.Collections.Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "true";
        try
        {
            

            VLP.BS.ImportExcleData importExcel = new VLP.BS.ImportExcleData(DB.GetConnection().ConnectionString,currentUserInfo.ID.ToString());
            //检测是否有MainKey  
            System.Collections.Specialized.NameValueCollection nvs = GetAddOtherKeyValues();
            string strRtn=importExcel.SaveExcleTemplateData(dtFile,false,nvs);
            if(!string.IsNullOrEmpty(strRtn))
            {
                result["msg"] = strRtn;
            }
        }
        catch (Exception ex)
        {
            result["msg"] = "读取导入的文件时出错，请核实文件！"; //+ ex.Message;
        }
        String json = JSON.Encode(result);
        Response.Write(json);
    }
    private string GetBeforCheckSQL(VLP.BS.ImportExcleEvent sender)
    {
        StringBuilder sb = new StringBuilder();
        if (sender.DetailData.TableName.Equals("B_Stock_IN_DTL_Edit"))
        {
            //检查入库通知单是否可以导入
            sb.AppendFormat(@"
    DECLARE @Staus  TINYINT;
    SELECT @Staus=Staus FROM B_Stock_IN WHERE BILLID={0}

    BEGIN
        
    END

", sender.MainKeys["BILLID"]);
        }
        return string.Empty;
    }
    private System.Collections.Specialized.NameValueCollection GetAddOtherKeyValues()
    {
        string mainkey = Request["MainKey"];
        
        if (string.IsNullOrEmpty(mainkey))
        {
            System.Collections.Specialized.NameValueCollection nvs = new System.Collections.Specialized.NameValueCollection();
            string[] keys = mainkey.Split(';');
            foreach (string k in keys)
            {
                if (k.Length == 0)
                    continue;
                string[] v = k.Split(':');
                if (v.Length == 2)
                {
                    nvs.Add(v[0], v[1]);
                }
            }
            return nvs;
        }
        return null;
    }
}