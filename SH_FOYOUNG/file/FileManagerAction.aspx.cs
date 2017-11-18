using Nordasoft.Data.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class file_FileManagerAction : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
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
    protected void BeforeInvoke(String methodName) { }

    //日志管理
    protected void AfterInvoke(String methodName) { }

    //获取上传文件的信息
    public void GetFileNameInfo()
    {
        string _MainID = Request["MainID"];
        string _DBConnection = Request["DBConnection"];

        Nordasoft.Data.Sql.DataBase db = new DataBase(VLP.WMS.Common.Decrypt(_DBConnection.Replace(":", "=")));

        System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand();
        cmmd.CommandText = " SELECT FileID,FileDName,IsZIP,CREATE_BY_Name,CREATE_DATE FROM File_Data_File WHERE ISDISABLED=0 AND MainID = @MainID";
        cmmd.CommandType = System.Data.CommandType.Text;
        cmmd.Parameters.AddWithValue("@MainID", _MainID);
        System.Data.DataTable dt = db.ExecuteDataTable(cmmd);
        if (dt != null&&dt.Rows.Count>0)
        {
            Response.Write(VLP.JSON.Encode(VLP.JSON.DataTable2ArrayList(dt)));
            return;
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    public void UploadData()
    {
        //上传文件
        HttpPostedFile uploadFile = Request.Files["Fdata"];
        string _MainID = Request["MainID"];
        string _DBConnection = Request["DBConnection"];
        string _CREATE_BY_Name = Request["CREATE_BY_Name"];
        if (uploadFile != null)
        {
            string tempFile = Request.PhysicalApplicationPath;// 获得程序路径
            if (uploadFile.ContentLength > 0)
            {
                string guiid = Guid.NewGuid().ToString();

                string strFileDir = string.Format("{0}{1}{2}", tempFile, "file\\filedata\\", guiid + uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf('.')));
                if (dbWriteData(uploadFile, _MainID, _DBConnection, _CREATE_BY_Name))
                {
                    //开始进行数据的写入
                    Response.Write(strFileDir);
                }
            }
            return;
        }
    }

    /// <summary>
    /// 数据保存写入
    /// </summary>
    /// <returns></returns>
    private bool dbWriteData(HttpPostedFile uploadFile, string _MainID, string _DBConnection, string _CREATE_BY_Name)
    {
        bool flag = false;
        try
        {
            if (uploadFile.ContentLength > 0)
            {
                string FileName = uploadFile.FileName;
                byte[] bytefuerr = VLP.Zip.CompressStream(uploadFile.InputStream);
                byte[] guiid = VLP.MD5.MakeMD5(bytefuerr);
                string sql = @"
                     INSERT INTO dbo.File_Data_File
                    ( MainID ,FileDName ,GuiID ,IsZIP ,CREATE_BY_Name ,CREATE_DATE 
                    )
                     VALUES  (@MainID,@FileDName,@GuiID,1,@CREATE_BY_Name,GETDATE())
                    IF NOT EXISTS(SELECT 1 FROM dbo.File_Data WHERE GuiID =  @GuiID)
                    BEGIN
                        INSERT INTO dbo.File_Data
                                ( GuiID, FileData )
                        VALUES  ( @GuiID, @FileData)
                    END

                ";
                Nordasoft.Data.Sql.DataBase db = new DataBase(VLP.WMS.Common.Decrypt(_DBConnection.Replace(":", "=")));

                System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand();
                cmmd.CommandText = sql;
                cmmd.CommandType = System.Data.CommandType.Text;
                cmmd.Parameters.Add("@MainID", System.Data.SqlDbType.Int);
                cmmd.Parameters["@MainID"].Value = _MainID;
                cmmd.Parameters.Add("@FileDName", System.Data.SqlDbType.NVarChar,30);
                cmmd.Parameters["@FileDName"].Value = uploadFile.FileName;
                cmmd.Parameters.Add("@GuiID", System.Data.SqlDbType.Binary, 16);
                cmmd.Parameters["@GuiID"].Value = guiid;
                cmmd.Parameters.Add("@CREATE_BY_Name", System.Data.SqlDbType.NVarChar, 20);
                cmmd.Parameters["@CREATE_BY_Name"].Value = _CREATE_BY_Name;
                cmmd.Parameters.Add("@FileData", System.Data.SqlDbType.VarBinary);
                cmmd.Parameters["@FileData"].Value = bytefuerr;

                if (db.ExecuteNonQuery(cmmd) != -1)
                {
                    flag = true;
                }
            }
        }
        catch
        {
            flag = false; ;
        }

        return flag;
    }
}