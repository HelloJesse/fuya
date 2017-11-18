using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Data.Sql;
using DataDynamics.ActiveReports;
using System.Configuration;
using Nordasoft.Data.Sql;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public partial class data_ReportPrintLotNo : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            string AR_ID = Request.QueryString["ID"];
            if (string.IsNullOrEmpty(AR_ID))
            {
                return;
            }

            string sql = @"
                SELECT [AR_ID]
                      ,[AR_Code]
                      ,[AR_Name]
                      ,[AR_Content]
                      ,[AR_Sql]
                      ,PageID
                  FROM [dbo].[Sys_AR_LIST] WHERE AR_ID=@AR_ID";

            SqlCommand cmmd = new SqlCommand();
            cmmd.CommandText = sql;
            cmmd.Parameters.Add(new SqlParameter("@AR_ID", System.Data.SqlDbType.Int));
            cmmd.Parameters["@AR_ID"].Value = AR_ID;
            //获取连接
            string cnnstr = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;
            Nordasoft.Data.Sql.DataBase db = new Nordasoft.Data.Sql.DataBase(cnnstr);
            DataTable dt = db.ExecuteDataTable(cmmd);
            if (dt != null && dt.Rows.Count > 0)
            {
                Stream aR_Content = BuilderStream(dt.Rows[0]["AR_Content"]);
                string aR_Sql = dt.Rows[0]["AR_Sql"].ToString();
                //获取打印数据
                string dsql = string.Format(@"
                        {1}
                    ", aR_Content, aR_Sql);
                cmmd.CommandText = dsql;
                DataSet ds = db.ExecuteDataset(cmmd);

                Response.Clear();
                Response.Buffer = true;
                Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");//设置输出流为简体中文
                Response.ContentType = "application/json";//设置输出文件类型为json
                EnableViewState = false;
                //Response.BinaryWrite(GetBinaryFormatData(ds));

                Response.Write(ToJson(ds));

            }
            else
            {
                this.Response.Write("未发现打印模板.");
            }
        }
        catch (Exception ex)
        {
            this.Response.Write(ex.ToString());
            return;
        }

    }

    public static string ToJson(DataSet dataSet)
    {
        string jsonString = "{";
        foreach (DataTable table in dataSet.Tables)
        {
            jsonString += "\"" + table.TableName + "\":" + ToJson(table) + ",";
        }
        jsonString = jsonString.TrimEnd(',');
        return jsonString + "}";
    }

    /// <summary>     
    /// Datatable转换为Json     
    /// </summary>    
    /// <param name="table">Datatable对象</param>     
    /// <returns>Json字符串</returns>     
    public static string ToJson(DataTable dt)
    {
        StringBuilder jsonString = new StringBuilder();
        jsonString.Append("[");
        DataRowCollection drc = dt.Rows;
        for (int i = 0; i < drc.Count; i++)
        {
            jsonString.Append("{");
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                string strKey = dt.Columns[j].ColumnName;
                string strValue = drc[i][j].ToString();
                Type type = dt.Columns[j].DataType;
                jsonString.Append("\"" + strKey + "\":");
                strValue = StringFormat(strValue, type);
                if (j < dt.Columns.Count - 1)
                {
                    jsonString.Append(strValue + ",");
                }
                else
                {
                    jsonString.Append(strValue);
                }
            }
            jsonString.Append("},");
        }
        jsonString.Remove(jsonString.Length - 1, 1);
        jsonString.Append("]");
        return jsonString.ToString();
    }

    /// <summary>
    /// 格式化字符型、日期型、布尔型
    /// </summary>
    /// <param name="str"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private static string StringFormat(string str, Type type)
    {
        if (type == typeof(string))
        {
            str = String2Json(str);
            str = "\"" + str + "\"";
        }
        else if (type == typeof(DateTime))
        {
            str = "\"" + str + "\"";
        }
        else if (type == typeof(bool))
        {
            str = str.ToLower();
        }
        else if (type != typeof(string) && string.IsNullOrEmpty(str))
        {
            str = "\"" + str + "\"";
        }
        return str;
    }

    /// <summary>
    /// 过滤特殊字符
    /// </summary>
    /// <param name="s">字符串</param>
    /// <returns>json字符串</returns>
    private static string String2Json(String s)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            char c = s.ToCharArray()[i];
            switch (c)
            {
                case '\"':
                    sb.Append("\\\""); break;
                case '\\':
                    sb.Append("\\\\"); break;
                case '/':
                    sb.Append("\\/"); break;
                case '\b':
                    sb.Append("\\b"); break;
                case '\f':
                    sb.Append("\\f"); break;
                case '\n':
                    sb.Append("\\n"); break;
                case '\r':
                    sb.Append("\\r"); break;
                case '\t':
                    sb.Append("\\t"); break;
                default:
                    sb.Append(c); break;
            }
        }
        return sb.ToString();
    }

    public static byte[] GetBinaryFormatData(DataSet dsOriginal)
    {
        byte[] binaryDataResult = null;
        MemoryStream memStream = new MemoryStream();
        //以二进制格式将对象或整个连接对象图形序列化和反序列化。
        IFormatter brFormatter = new BinaryFormatter();
        //dsOriginal.RemotingFormat 为远程处理期间使用的DataSet 获取或设置 SerializtionFormat        
        //SerializationFormat.Binary      将字符串比较方法设置为使用严格的二进制排序顺序
        dsOriginal.RemotingFormat = SerializationFormat.Binary;
        //把字符串以二进制放进memStream中
        brFormatter.Serialize(memStream, dsOriginal);
        //转为byte数组
        binaryDataResult = memStream.ToArray();
        memStream.Close();
        memStream.Dispose();
        return binaryDataResult;
    }

    public static ActiveReport3 BuilderReport(object ARS_Content)
    {
        Stream stream = BuilderStream(ARS_Content);
        DataDynamics.ActiveReports.ActiveReport3 MM = null;
        if (stream != null)
        {
            MM = BuilderReport(stream);
        }
        return MM;
    }

    public static Stream BuilderStream(object ARS_Content)
    {
        byte[] memory = ARS_Content as byte[];
        Stream stream = null;
        if (memory != null)
        {
            stream = new MemoryStream(memory);
        }
        return stream;
    }

    public static ActiveReport3 BuilderReport(Stream stream)
    {
        DataDynamics.ActiveReports.ActiveReport3 MM = new ActiveReport3();
        MM.LoadLayout(stream);
        return MM;
    }

}