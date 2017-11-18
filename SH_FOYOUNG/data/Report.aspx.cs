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
//using VLP.CMSDBTool;
//using FineUI;
//using Popedom.SqlPopedom;
using System.Configuration;
using Nordasoft.Data.Sql;
using System.Collections;

public partial class AS_Report : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (IsOut(this.Context))
            {
                this.Response.Clear();
                this.Response.Write("登录超时,请重新登录."); return;
            }
            string whereSql = Request.QueryString["SQL"];
            string AR_ID = Request.QueryString["ID"];
            string IsRecPrint = Request.QueryString["IsRecPrint"];//判断是否是退货快递面单打印
            if (whereSql == null)
            {
                whereSql = "";
            }
            //if (whereSql.Length >0 && whereSql.TrimStart(' ').ToUpper().StartsWith("AND") == false)
            //{
            //    whereSql = string.Format(" AND {0}", whereSql);
            //}
            if (string.IsNullOrEmpty(IsRecPrint))
            {
                IsRecPrint = "0";
            }
            if (string.IsNullOrEmpty(AR_ID))
            {
                return;
            }

            string strSql = @"
SELECT [AR_ID]
      ,[AR_Code]
      ,[AR_Name]
      ,[AR_Content]
      ,[AR_Sql]
      ,PageID
  FROM [dbo].[Sys_AR_LIST] WHERE AR_ID=@AR_ID
";
            SqlCommand sqlcom = new SqlCommand("sp_Sys_GetReportData");
            sqlcom.CommandType = CommandType.StoredProcedure;
            sqlcom.Parameters.Add("@AR_ID", SqlDbType.Int);
            sqlcom.Parameters["@AR_ID"].Value = AR_ID;
            sqlcom.Parameters.Add("@UserID", SqlDbType.Int);
            sqlcom.Parameters["@UserID"].Value = GetUserInfo.ID;
            sqlcom.Parameters.Add("@WhereSQL", SqlDbType.NVarChar, -1);
            sqlcom.Parameters["@WhereSQL"].Value = whereSql;

            DataSet ds = DB.ExecuteDataset(sqlcom);
            DataTable dt = ds.Tables[1];

            if (dt != null && dt.Rows.Count > 0)
            {
                DataDynamics.ActiveReports.ActiveReport3 report = BuilderReport(dt.Rows[0]["AR_Content"] as byte[]);
                report.Document.Printer.PrinterName = "";
                //sqlcom.CommandText = dt.Rows[0]["AR_Sql"].ToString() + whereSql;

                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    //如果AR_ID = 12,13,16 备案清单打印,则数据条数必须是5的倍数，不足5条补充空白数据进去
                    if ((AR_ID == "12" || AR_ID == "13" || AR_ID == "16" || AR_ID == "17" || AR_ID == "18" || AR_ID == "19" || AR_ID == "20" || AR_ID == "33" || AR_ID == "34" || AR_ID == "35" || AR_ID == "36" || AR_ID == "48") && (dt.Rows.Count % 8) > 0)
                    {
                        dt=SetPagingDT(dt);
                    }
                    //快递面单打印
                    if (AR_ID == "52" && IsRecPrint == "1")
                    {
                        dt = SetRecPrintTable(dt,whereSql);
                    }

                    SetPrintValue(dt);
                    report.DataSource = dt;//orderM.GetTable(dt.Rows[0]["AR_Sql"].ToString() + whereSql, this.GetBusiDB());
                    report.Run();
                    arvWebMain.Report = report;

                    this.arvWebMain.ViewerType = DataDynamics.ActiveReports.Web.ViewerType.AcrobatReader;
                }
                else
                {
                    this.Response.Write("未发现打印数据,请确认是否有权限打印此数据.");
                }
            }
            else
            {
                this.Response.Write("未发现打印模板.");
            }

        }
        catch (Exception ex)
        {
            this.Response.Write(ex.ToString());
            //ShowMessage(ex.Message);
            return;
        }

    }
    /// <summary>
    /// 设置退货快递面单打印
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private DataTable SetRecPrintTable(DataTable dt,string sqlwhere)
    {
        string sql = @"UPDATE dbo.B_Stock_Out SET Print2Flag=1 WHERE ISDISABLED=0 AND " + sqlwhere + 
            " ;SELECT recAddress2,recHabdset2,recPerson2,paintMarker2,BILLID FROM dbo.B_Stock_Out WHERE " + sqlwhere + " AND ISDISABLED=0;";
        SqlCommand cmmd2 = new SqlCommand();
        cmmd2.CommandText = sql;
        cmmd2.CommandType = CommandType.Text;
        DataTable dt2 = DB.ExecuteDataTable(cmmd2);
        foreach (DataRow dr in dt2.Rows)
        {
            foreach (DataRow dr2 in dt.Rows)
            {
                if (dr["BILLID"].ToString() == dr2["BILLID"].ToString())
                {
                    dr2["收货地址"] = dr["recAddress2"].ToString();
                    dr2["收货人姓名"] = dr["recPerson2"].ToString();
                    dr2["收货人电话"] = dr["recHabdset2"].ToString();
                    dr2["快递大头笔"] = dr["paintMarker2"].ToString();
                }
            }
        }
        return dt;
    }

    //备案清单打印作业，补充空白行
    private DataTable SetPagingDT(DataTable dt)
    {
        //如果AR_ID = 12,13,16 备案清单打印,则数据条数必须是5的倍数，不足5条补充空白数据进去
        string strBillNo = dt.Rows[0]["报关建议书号"].ToString();
        DataRow rowT = null;
        ArrayList list = new ArrayList();
        list.Add(dt.Rows[0]["报关建议书号"].ToString());
        foreach (DataRow rowt in dt.Rows)
        {
            if (!list.Contains(rowt["报关建议书号"].ToString()))
            {
                list.Add(rowt["报关建议书号"].ToString());
            }
        }

        //补空行
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                strBillNo = list[i].ToString();
                DataRow[] rowBillNo = dt.Select("报关建议书号='" + list[i].ToString() + "'");
                //取模后的数量 用8在减去后就是要加的行数
                int lessCount = 8 - (rowBillNo.Length % 8);
                if (lessCount > 0)
                {

                    //有不足的行数
                    int rowindx = dt.Rows.IndexOf(rowBillNo[rowBillNo.Length - 1]);
                    for (int m = 0; m < lessCount; m++)
                    {
                        rowT = dt.NewRow();
                        rowT["报关建议书号"] = strBillNo;
                        dt.Rows.InsertAt(rowT, rowindx+1);                      

                    }
                }
            }
            //页数
            strBillNo = dt.Rows[0]["报关建议书号"].ToString();
            int datacount = 1;//记录一个报关建议书的条数
            for (int p = 0; p < dt.Rows.Count; p++)
            {
                if (dt.Rows[p]["报关建议书号"].ToString() == strBillNo)
                {
                    dt.Rows[p]["页数"] = Math.Ceiling(datacount / 8.0);
                }
                else
                {
                    datacount = 1;
                    dt.Rows[p]["页数"] = Math.Ceiling(datacount / 8.0);
                }
                datacount++;
                strBillNo = dt.Rows[p]["报关建议书号"].ToString();
            }

          
        }
        return dt;

        ////判断是否有多条报关建议书号 OLD 写法
        //ArrayList list = new ArrayList();
        //list.Add(dt.Rows[0]["报关建议书号"].ToString());
        //foreach (DataRow rowt in dt.Rows)
        //{
        //    if (!list.Contains(rowt["报关建议书号"].ToString()))
        //    {
        //        list.Add(rowt["报关建议书号"].ToString());
        //    }
        //}
        //if (list.Count > 0)
        //{
        //    //说明是查询界面打印，传入了多个BILLID，则需要根据不同的BILLID来单独判断行数是否是5的倍数，不足补入
        //    for (int intl = 0; intl < list.Count; intl++)
        //    {
        //        DataRow[] rowBillNo = dt.Select("报关建议书号='" + list[intl].ToString() + "'");
        //        //取模后的数量 用5在减去后就是要加的行数
        //        int lessCount = 5 - (rowBillNo.Length % 5);
        //        if (lessCount > 0)
        //        {
        //            DataRow rowT = null;
        //            //需要加的行数
        //            for (int m = 0; m < lessCount; m++)
        //            {
        //                rowT = dt.NewRow();
        //                for (int k = 0; k < dt.Columns.Count; k++)
        //                {
        //                    if (dt.Columns[k].ColumnName == "报关建议书号")
        //                    {
        //                        rowT[k] = list[intl].ToString();
        //                    }
        //                    else
        //                    {
        //                        //加入空白数据
        //                        rowT[k] = DBNull.Value;
        //                    }
        //                }
        //                //加入到原TABLE中
        //                dt.Rows.Add(rowT);
        //            }
        //        }
        //    }

        //}
          
        

    }

    private void SetPrintValue(DataTable dt)
    {
        if (dt.Columns.Contains("系统_公司名称"))
        {
            string printname = Session["PrintName"].ToString();
            string PrintTel = Session["PrintTel"].ToString();
            string PrintAddress = Session["PrintAddress"].ToString();
            string printname_e = Session["PrintName_E"].ToString();

            foreach (DataRow row in dt.Rows)
            {
                row["系统_公司名称"] = printname;
                row["系统_公司英文名称"] = printname_e;
                row["系统_公司电话"] = PrintTel;
                row["系统_公司地址"] = PrintAddress;
            }
            dt.AcceptChanges();
        }
        if (dt.Columns.Contains("打印人"))
        {
            string code = ((UserInfo)Session["CurrentUserInfo"]).Code;

            foreach (DataRow row in dt.Rows)
            {
                row["打印人"] = code;
            }
            dt.AcceptChanges();
        }
        if (dt.Columns.Contains("打印时间"))
        {
            foreach (DataRow row in dt.Rows)
            {
                row["打印时间"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            dt.AcceptChanges();
        }
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