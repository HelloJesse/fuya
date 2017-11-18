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
        //if (currentUserInfo == null)
        //{
        //    Response.Write("{\"isOut\":\"1\"}");
        //    return;
        //}

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
            

            VLP.BS.ImportExcleData importExcel = new VLP.BS.ImportExcleData(DB.GetConnection().ConnectionString,currentUserInfo==null?"1":currentUserInfo.ID.ToString());
            //检测是否有MainKey  
            System.Collections.Specialized.NameValueCollection nvs = GetAddOtherKeyValues();
            importExcel.GetBeforCheckSQL = GetBeforCheckSQL;
            importExcel.GetAfterCheckSQL = GetAfterCheckSQL;
            importExcel.GetOtherSaveFields = GetOtherSaveFields;
            importExcel.GetNoSaveFields = GetNoSaveFields;
            importExcel.GetOtherSaveFieldsSQLValue = GetOtherSaveFieldsSQLValue;
            //string strRtn=importExcel.SaveExcleTemplateData(dtFile,false,nvs);
            //if(!string.IsNullOrEmpty(strRtn))
            //{
            //    result["msg"] = strRtn;
            //}
        }
        catch (Exception ex)
        {
            result["msg"] = string.Format("导入文件时出错:{0}", ex.Message); //+ ex.Message;
        }
        String json = VLP.JSON.Encode(result);
        Response.Write(json);
    }
    /// <summary>
    /// 获取其它需要保存的字段
    /// </summary>
    /// <param name="sender"></param>
    /// <returns></returns>
    private string GetOtherSaveFields(VLP.BS.ImportExcleEvent sender)
    {
        if (sender.TemplateTable.TableName.Equals("B_Stock_IN_DTL_Edit"))
        {
            return VLP.WMS.StockIn.GetOtherSaveFields();
        }
        return string.Empty;
    }
        /// <summary>
    /// 获取其它需要保存的字段
    /// </summary>
    /// <param name="sender"></param>
    /// <returns></returns>
    private string GetOtherSaveFieldsSQLValue(VLP.BS.ImportExcleEvent sender,DataRow row)
    {
        if (sender.TemplateTable.TableName.Equals("B_Stock_IN_DTL_Edit"))
        {
            return VLP.WMS.StockIn.GetOtherSaveFieldsSQLValue(sender, row);
        }
        return string.Empty;
    }
    
    /// <summary>
    /// 获取不需要保存的字段
    /// </summary>
    /// <param name="sender"></param>
    /// <returns></returns>
    private string GetNoSaveFields(VLP.BS.ImportExcleEvent sender)
    {
        if (sender.TemplateTable.TableName.Equals("B_Stock_IN_DTL_Edit"))
        {
            return "Property1,Property2,Property3,Property4,Property5,Property6,Property7,Property8,Property9,Property10,Property11,Property12";
        }
        return string.Empty;
    }
    private string GetBeforCheckSQL(VLP.BS.ImportExcleEvent sender)
    {
        StringBuilder sb = new StringBuilder();
        if (sender.TemplateTable.TableName.Equals("B_Stock_IN_DTL_Edit"))
        {
            bool checkcustomergoods = (bool)Session["IsCustomerGoods"];
            return VLP.WMS.StockIn.GetImportDetailBeforSQL(sender,checkcustomergoods);
        }
        return string.Empty;
    }

    private string GetAfterCheckSQL(VLP.BS.ImportExcleEvent sender)
    {
        StringBuilder sb = new StringBuilder();
        if (sender.TemplateTable.TableName.Equals("B_Stock_IN_DTL_Edit"))
        {
            return VLP.WMS.StockIn.GetImportDetailAfterSQL(sender);
        }
        return string.Empty;
    }
    /// <summary>
    /// 获取其它关键字
    /// </summary>
    /// <returns></returns>
    private System.Collections.Specialized.NameValueCollection GetAddOtherKeyValues()
    {
        string mainkey = Request["OtherKey"];
        
        if (string.IsNullOrEmpty(mainkey)==false)
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