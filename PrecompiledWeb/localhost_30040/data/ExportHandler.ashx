<%@ WebHandler Language="C#" Class="ExportHandler" %>

using System;
using System.Web;
using System.Collections;
using System.IO;
using System.Data;

public class ExportHandler : BasePage, IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{
    HttpContext _context;
    public override void ProcessRequest(HttpContext context)
    {
        _context = context;

        string methodName = _context.Request["method"];
        switch (methodName)
        {
            case "CreateExcel":
                //数据来源
                CreateExcel();
                break;
            case "DownLoad":
                DownLoad();
                break;
            case "GoodsBigExport": GetGoodsExcelBig(); break;
            case "ExportPoInvetory": ExportPoInvetory(); break;
            case "GetWinRARList": GetWinRARList(); break;
            default:
                break;
        }
    }

    private void testc()
    {
        string path = _context.Request["path"];
        string rarName = _context.Request["rarName"];
        ToWinRAR winrar = new ToWinRAR();
        string res = winrar.CompressRar(path, rarName, "费用一览");
    }
    /// <summary>
    /// 编辑界面，根据ID获取明细信息
    /// </summary>
    public void CreateExcel()
    {
        string dataStr = _context.Request["data"];
        string Columns = _context.Request["columns"];
        string FileName = _context.Request["FileName"];
        ArrayList columns = (ArrayList)VLP.JSON.Decode(Columns);
        ArrayList datas = (ArrayList)VLP.JSON.Decode(dataStr);
        string Result = ExportTable(datas, columns);

        if (String.IsNullOrEmpty(FileName))
        {
            FileName = "grid";
        }

        _context.Response.Clear();
        _context.Response.Buffer = true;
        _context.Response.Charset = "GB2312";
        //Response.Charset = "UTF-8";
        _context.Response.AppendHeader("Content-Disposition", "attachment;filename=" + FileName + ".xls");
        _context.Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");//设置输出流为简体中文
        _context.Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。
        EnableViewState = false;
        _context.Response.Write(Result);
        _context.Response.End();
    }

    private String ExportTable(ArrayList data, ArrayList columns)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int count = 0;

        sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=gb2312\">");
        sb.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
        //写出列名
        sb.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
        foreach (Hashtable column in columns)
        {
            sb.AppendLine("<td>" + column["header"] + "</td>");
        }
        sb.AppendLine("</tr>");

        //写出数据
        foreach (Hashtable row in data)
        {
            sb.Append("<tr>");
            foreach (Hashtable column in columns)
            {
                if (column["field"] == null) continue;
                Object value = row[column["field"]];
                sb.AppendLine("<td>" + value + "</td>");
            }
            sb.AppendLine("</tr>");
            count++;
        }
        sb.AppendLine("</table>");

        return sb.ToString();
    }
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

    private void DownLoad()
    {
        try
        {
            string path = _context.Request["path"];
            string FileName = _context.Request["name"];

            path = _context.Server.MapPath(path);//

            //以字符流的形式下载文件
            FileStream fs = new FileStream(path, FileMode.Open);
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            _context.Response.Clear();
            _context.Response.ContentType = "application/octet-stream";
            //通知浏览器下载文件而不是打开
            EnableViewState = false;
            _context.Response.AddHeader("Content-Disposition", "attachment;  filename=" + HttpUtility.UrlEncode(FileName, System.Text.Encoding.UTF8));
            _context.Response.BinaryWrite(bytes);
            _context.Response.Flush();
            _context.Response.End();
        }
        catch (Exception ex)
        {

        }
    }

    private void GetWinRARList()
    {
        string date = _context.Request["date"];
        string name = _context.Request["name"];
        string fileType = _context.Request["fileType"];
        string pathUrl = "/WinRAR/" + fileType + "/" + GetUserInfo.Code;
        DateTime dt = DateTime.Now;
        if (DateTime.TryParse(date, out dt))
        {
            pathUrl = pathUrl + "/" + dt.ToString("yyyyMMdd");
        }


        DataTable data = new DataTable();
        data.Columns.Add("UserName");
        data.Columns.Add("FileName");
        data.Columns.Add("Url");
        data.Columns.Add("ID");

        string path = Server.MapPath(pathUrl);
        DirectoryInfo dirpath = new DirectoryInfo(path);
        if (!dirpath.Exists)
        {

        }
        else
        {
            int i = 1;
            foreach (FileInfo file in dirpath.GetFiles("*.rar"))
            {
                if (file.Name == name || string.IsNullOrEmpty(name))
                {
                    data.Rows.Add(GetUserInfo.Name, file.Name, pathUrl + "/" + file.Name, i);
                    i++;
                }
            }
            //JSON
            String json = VLP.JSON.Encode(data);
            json = "{\"total\":" + (i - 1) + ",\"data\":" + json + "}";
            _context.Response.Write(json);
        }
    }


    /// <summary>
    /// 列表页导出合并后大料号
    /// </summary>
    private void GetGoodsExcelBig()
    {
        string billids = _context.Request["BillIDS"];
        string billType = _context.Request["billType"];

        Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "false";
        try
        {
            System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand();
            cmmd.CommandText = "sp_B_GetExportGoodsBig";
            cmmd.CommandType = CommandType.StoredProcedure;
            cmmd.Parameters.Add("@BillIDS", SqlDbType.NVarChar);
            cmmd.Parameters.Add("@TableType", SqlDbType.Bit);

            cmmd.Parameters["@BillIDS"].Value = billids;
            cmmd.Parameters["@TableType"].Value = billType=="0"?"false":"true";

            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt != null )
            {
                string Result = ExportTable(dt);

                _context.Response.Clear();
                _context.Response.Buffer = true;
                _context.Response.Charset = "GB2312";
                //Response.Charset = "UTF-8";
                _context.Response.AppendHeader("Content-Disposition", "attachment;filename=合并后大料号导出.xls");
                _context.Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");//设置输出流为简体中文
                _context.Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。
                EnableViewState = false;
                _context.Response.Write(Result);
                _context.Response.End();
            }
        }
        catch (Exception ex)
        {
            result["msg"] ="false";
        }
    }

    /// <summary>
    /// 导出待确认库存
    /// </summary>
    private void ExportPoInvetory()
    {
        //string billids = _context.Request["BillIDS"];
        Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "false";
        try
        {
            System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand();
            cmmd.CommandText = "sp_GetExportPoInvetoryData";
            cmmd.CommandType = CommandType.StoredProcedure;
            //cmmd.Parameters.Add("@BillIDS", SqlDbType.NVarChar);
            //cmmd.Parameters["@BillIDS"].Value = billids;
            
         
            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt != null)
            {
                string Result = ExportTable(dt);

                _context.Response.Clear();
                _context.Response.Buffer = true;
                _context.Response.Charset = "GB2312";
                //Response.Charset = "UTF-8";
                _context.Response.AppendHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode("待确认库存") + ".xls");
                _context.Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");//设置输出流为简体中文
                _context.Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。
                EnableViewState = false;
                _context.Response.Write(Result);
                _context.Response.End();
            }
        }
        catch (Exception ex)
        {
            result["msg"] = "false";
        }
    }
    
    private String ExportTable(DataTable dt)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int count = 0;

        sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=gb2312\">");
        sb.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
        //写出列名
        sb.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
        foreach (DataColumn column in dt.Columns)
        {
            sb.AppendLine("<td>" + column.ColumnName + "</td>");
        }
        sb.AppendLine("</tr>");

        //写出数据
        foreach (DataRow row in dt.Rows)
        {
            sb.Append("<tr>");
            foreach (DataColumn column in dt.Columns)
            {
                Object value = row[column.ColumnName];
                sb.AppendLine("<td>" + value + "</td>");
            }
            sb.AppendLine("</tr>");
            count++;
        }
        sb.AppendLine("</table>");

        return sb.ToString();
    }
}