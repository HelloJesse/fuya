using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using Nordasoft.Data.Sql;
using System.Collections.ObjectModel;
using System.IO;
using ExcelLibrary.SpreadSheet;
using ICSharpCode.SharpZipLib.Zip;
using VLP.BS;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;


public partial class data_ExportUavTrack : BasePage
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
            //Hashtable result = new Hashtable();
            //result["error"] = -1;
            //result["message"] = ex.Message;
            //result["stackTrace"] = ex.StackTrace;
            //String json = VLP.JSON.Encode(result);
            //Response.Clear();
            //Response.Write(json);
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
    /// 下载飞机任务航线编辑列表内容
    /// </summary>
    public void ExportEditLineList() 
    {
        string billid = Request.Form.Get("ExcelBILLID");

        SqlCommand cmmd = new SqlCommand(@"SELECT  LineCode AS '航线编码', Sortid AS '航点序号',
           Jd AS '经度', Wd AS '纬度', Height_G AS '高度' FROM   D_Uav_FlyLineRecord WHERE TaskID = @BILLID;");
        cmmd.Parameters.Add("@BILLID", SqlDbType.NVarChar, -1);
        cmmd.Parameters["@BILLID"].Value = billid;
        DataTable dt = DB.ExecuteDataTable(cmmd);
        if (dt == null || dt.Rows.Count <= 0)
        {
            //没有查到数据
            string errormsg = "没有数据可导出!";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");//设置输出流为简体中文
            Response.AppendHeader("Content-Disposition", "attachment;filename=" + "error.txt");
            Response.ContentType = "text/xml";//设置输出文件类型为xml文件。
            EnableViewState = false;
            Response.Write(errormsg);
            return;
        }
        else
        {
            //文件名称
            string fileName = "UavLineInfo-" + DateTime.Now.ToString("yyyyMMddHHmm");
            MemoryStream file = Common.DataModuleTableToExcel(dt, fileName);
            file.Position = 0;
            byte[] bytes = new byte[file.Length];
            file.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            file.Seek(0, SeekOrigin.Begin);

            Response.Clear();
            Response.Buffer = true;
            Response.AppendHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName) + ".xls");
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");//设置输出流为简体中文
            Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。
            EnableViewState = false;
            Response.BinaryWrite(bytes);
            Response.End();
        }
    }



    /// <summary>
    /// 下载飞机实际飞行航线
    /// </summary>
    public void ExportUavTrack()
    {
        string billids = Request.Form.Get("BillIDS");
        try
        {
            SqlCommand cmmd = new SqlCommand("sp_GetUavTrackList");
            cmmd.CommandType = CommandType.StoredProcedure;
            cmmd.Parameters.Add("@BILLID", SqlDbType.NVarChar, -1);
            cmmd.Parameters["@BILLID"].Value = billids;
            DataTable dt = DB.ExecuteDataTable(cmmd);

            if (dt == null || dt.Rows.Count <=0)
            {
                //没有查到数据
                string errormsg = "没有数据可导出!";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");//设置输出流为简体中文
                Response.AppendHeader("Content-Disposition", "attachment;filename=" + "error.txt");
                Response.ContentType = "text/xml";//设置输出文件类型为xml文件。
                EnableViewState = false;
                Response.Write(errormsg);
                return;
            }
            else
            {
                //文件名称
                string fileName = "Uav_Track-" + DateTime.Now.ToString("yyyyMMddHHmm");
                //if (dt.Columns.Contains("创建日期"))
                //{
                //    dt.Columns["创建日期"].ExtendedProperties.Add("Format", "yyyy-MM-dd HH:mm:ss");
                //}
                if (dt.Rows.Count > 60000)
                {
                    Response.Clear();
                    Response.Buffer = true;
                    Response.AppendHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName) + ".xls");
                    Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");//设置输出流为简体中文
                    Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。
                    EnableViewState = false;

                    Response.BinaryWrite(Common.ExportTable(dt)); //支持 6w条数数据以上
                }
                else
                {
                    MemoryStream file = Common.DataModuleTableToExcel(dt, fileName);
                    file.Position = 0;
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, bytes.Length);
                    // 设置当前流的位置为流的开始
                    file.Seek(0, SeekOrigin.Begin);

                    Response.Clear();
                    Response.Buffer = true;
                    Response.AppendHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName) + ".xls");
                    Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");//设置输出流为简体中文
                    Response.ContentType = "application/ms-excel";//设置输出文件类型为excel文件。
                    EnableViewState = false;
                    Response.BinaryWrite(bytes);
                    Response.End();
                }
            }
           
        }
        catch (Exception ex)
        {
           
        }
    }

}

