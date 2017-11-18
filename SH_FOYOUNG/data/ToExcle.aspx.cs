using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using VLP.BS;

public partial class data_ToExcle : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        String data = Request["data"];
        String group = Request["export"];
        if (data != null && !string.IsNullOrEmpty(data))
        {
            ToExcleTemplate(data);
        }
        else if (group != null && !string.IsNullOrEmpty(group))
        {
            ToExcelByGroup();
        }
        else
        {
            ToExcle();
        }
    }
    private void ToExcleTemplate(string data)
    {
        System.Collections.Hashtable dtas = (System.Collections.Hashtable)VLP.JSON.Decode(data);
        System.Collections.ArrayList clms = (System.Collections.ArrayList)dtas["data"];
        string tableName = dtas["tableName"].ToString();
        DataTable dtRtn = new DataTable();
        foreach (System.Collections.Hashtable row in clms)
        {
            if (row["Checked"].ToString() == "1")
            {
                DataColumn clm = new DataColumn(row["ShowText"].ToString());
                dtRtn.Columns.Add(clm);
            }
        }
        Response.Clear();
        Response.Buffer = true;
        //Response.Charset = "GB2312";
        Response.AppendHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(tableName) + ".xls");
        Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");//设置输出流为简体中文
        Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。
        EnableViewState = false;
        Response.BinaryWrite(ExportExcel.DataExportExcelByte(dtRtn, tableName));
        Response.End();
    }
    /// 将 Stream 转成 byte[]



    public void ToExcle()
    {


        string PageID = Request["PageID"];
        string _filename = Request["filename"];
        if (string.IsNullOrEmpty(_filename))
        {
            _filename = "导出数据";
        }
        _filename = _filename + DateTime.Now.ToString("yyyyMMddHHmm");
        String json = Request["Columns"];
        if (json == null) return;
        System.Collections.Hashtable datas = (System.Collections.Hashtable)VLP.JSON.Decode(json);
        System.Collections.ArrayList columns = (System.Collections.ArrayList)datas["Columns"];
        string ids = datas["IDS"].ToString();

        System.Data.DataTable dt = VLP.BS.SearchManager.GetDataByIDS(PageID, ids, this.GetUserInfo.ID.ToString(), DB);
        //System.Collections.ArrayList dataarraylist = JSON.DataTable2ArrayList(dt);
        Response.Clear();
        Response.Buffer = true;
        //Response.Charset = "GB2312";
        //Response.Charset = "UTF-8";
        Response.AppendHeader("Content-Disposition", "attachment;filename=" + Server.UrlEncode(_filename) + ".xls");
        //Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");//设置输出流为简体中文
        Response.ContentEncoding = System.Text.Encoding.UTF8;
        Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。
        EnableViewState = false;
        BasePage.Write(this, ExportTable(dt, columns));
        //Response.Write(ExportTable(dt, columns));
        //Response.Write(ExportTable(dataarraylist, columns));
        //Response.End();
    }
    public static string ExportTable(System.Data.DataTable dt, System.Collections.ArrayList columns)
    {
        return ExportTable(dt, columns,null);
    }
    public static string ExportTable(System.Data.DataTable dt, System.Collections.ArrayList columns,System.Collections.Specialized.StringDictionary keywidth)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //data = ds.DataSetName + "\n";
        int count = 0;


        //data += tb.TableName + "\n";
        sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; \">");
        sb.AppendLine("<style type=\"text/css\" >");
        sb.Append(".headtd{ ");
        sb.Append(string.Format("font-size:{0}pt;font-family:{1};color:{2};background-color:{3}", 10, "宋体", "WindowText", "Control"));
        sb.AppendLine("}");
        sb.Append("td{ ");
        sb.Append(string.Format("font-size:{0}pt;font-family:{1};vnd.ms-excel.numberformat:@;", 9, "宋体"));
        sb.AppendLine("}");
        sb.Append(".number{vnd.ms-excel.numberformat:#,###.00}");
        sb.Append(".int{vnd.ms-excel.numberformat:#,###}");
        sb.Append(".date{vnd.ms-excel.numberformat:yyyy-MM-dd}");
        sb.Append(".datetime{vnd.ms-excel.numberformat:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("</style>");
        sb.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
        //写出列名
        sb.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
        foreach (System.Collections.Hashtable column in columns)
        {
            string filed = column["field"].ToString();
            if (keywidth != null && keywidth.ContainsKey(filed))
            {
                int width = 0;
                int.TryParse(keywidth[filed], out width);
                if (width == 0)
                {
                    sb.AppendLine(string.Format("<td style='width: {0}px;'>", width) + column["header"] + "</td>");
                }
                else
                {
                    sb.AppendLine("<td>" + column["header"] + "</td>");
                }
            }
            else
            {
                sb.AppendLine("<td>" + column["header"] + "</td>");
            }
        }
        sb.AppendLine("</tr>");

        //写出数据
        foreach (DataRow row in dt.Rows)
        {
            //foreach (DataColumn c in dt.Columns)
            foreach (System.Collections.Hashtable column in columns)
            {
                string filed = column["field"].ToString();
                if (dt.Columns.Contains(filed) == false)
                {
                    sb.Append("<td>未找到数据源<td>");
                    continue;
                }
                DataColumn c = dt.Columns[filed];
                Type t = c.DataType;
                if (t == typeof(string))
                {
                    sb.Append(string.Format("<td>{0}</td>", row[c]));
                }
                else if (t == typeof(decimal) || t == typeof(double))
                {
                    sb.Append(string.Format("<td class=\"number\">{0}</td>", row[c]));
                }
                else if (t == typeof(int) || t == typeof(long) || t == typeof(short))
                {
                    sb.Append(string.Format("<td class=\"int\">{0}</td>", row[c]));
                }
                else if (t == typeof(DateTime))
                {
                    sb.Append(string.Format("<td class=\"date\">{0}</td>", row[c]));
                }
                else
                {
                    sb.Append(string.Format("<td>{0}</td>", row[c]));
                }
            }
            sb.Append("</tr>");
        }
        sb.AppendLine("</table>");
        return sb.ToString();
    }
    public static string ExportTable(System.Collections.ArrayList data, System.Collections.ArrayList columns)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //data = ds.DataSetName + "\n";
        int count = 0;


        //data += tb.TableName + "\n";
        sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=gb2312\">");
        //写出列名
        sb.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
        foreach (System.Collections.Hashtable column in columns)
        {
            sb.AppendLine("<td>" + column["header"] + "</td>");
        }
        sb.AppendLine("</tr>");

        //写出数据
        foreach (System.Collections.Hashtable row in data)
        {
            sb.Append("<tr>");
            foreach (System.Collections.Hashtable column in columns)
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

    public void ToExcelByGroup()
    {
        string PageID = Request["PageID"];
        string _filename = Request["filename"];
        if (string.IsNullOrEmpty(_filename))
        {
            _filename = "导出数据";
        }
        _filename = _filename + DateTime.Now.ToString("yyyyMMddHHmm");
        String json = Request["Columns"];
        if (json == null) return;
        System.Collections.Hashtable datas = (System.Collections.Hashtable)VLP.JSON.Decode(json);
        System.Collections.ArrayList columns = (System.Collections.ArrayList)datas["Columns"];
        string ids = datas["IDS"].ToString();

        System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand("GetFNewAccountByGroup");
        cmmd.CommandType = CommandType.StoredProcedure;

        cmmd.Parameters.Add("@IDS", SqlDbType.NVarChar);
        cmmd.Parameters["@IDS"].Value = ids;
        System.Data.DataTable dtnew = DB.ExecuteDataTable(cmmd);


        System.Collections.ArrayList columnsNew = new System.Collections.ArrayList();
        System.Collections.Hashtable has = new System.Collections.Hashtable();
        has.Add("header", "序号");
        has.Add("field", "ID");
        columnsNew.Add(has);
        System.Collections.Hashtable has1 = new System.Collections.Hashtable();
        has1.Add("header", "结算单位");
        has1.Add("field", "SettleCustomerName");
        columnsNew.Add(has1);
        System.Collections.Hashtable has2 = new System.Collections.Hashtable();
        has2.Add("header", "金额");
        has2.Add("field", "Amount");
        columnsNew.Add(has2);
        System.Collections.Hashtable has3 = new System.Collections.Hashtable();
        has3.Add("header", "币制");
        has3.Add("field", "Curry");
        columnsNew.Add(has3);
        System.Collections.Hashtable has4 = new System.Collections.Hashtable();
        has4.Add("header", "业务员");
        has4.Add("field", "Sale_NAME");
        columnsNew.Add(has4);

        Response.Clear();
        Response.Buffer = true;
        Response.Charset = "GB2312";
        //Response.Charset = "UTF-8";
        Response.AppendHeader("Content-Disposition", "attachment;filename=" + Server.UrlEncode(_filename) + ".xls");
        Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");//设置输出流为简体中文
        Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。
        EnableViewState = false;
        Response.Write(ExportTable(dtnew, columnsNew));
        //Response.Write(ExportTable(dataarraylist, columns));
        Response.End();
    }
}