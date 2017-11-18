using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Nordasoft.Data.Sql;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

/// <summary>
/// Common 的摘要说明
/// </summary>
public class Common
{

    //地球半径，单位米 计算两个经纬度的距离
    private const double EARTH_RADIUS = 6378137;

	public Common()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
	}
    ///// <summary>
    ///// 获取语言版本
    ///// </summary>
    ///// <param name="request"></param>
    ///// <returns></returns>
    //public static string GetLanguage(HttpRequest request)
    //{
    //    string language = string.Empty;
    //    string requesturl = request.Headers["Referer"].ToString();
    //    if (requesturl.IndexOf("/en/") > 0)
    //    {
    //        language = "E";
    //    }
    //    return language;
    //}

    ///// <summary>
    ///// 获取语言版本参数 
    ///// </summary>
    ///// <param name="request"></param>
    ///// <returns></returns>
    //public static string GetLanguageParam(HttpRequest request)
    //{
    //    string language = string.Empty;
    //    string requesturl = request.Headers["Referer"].ToString();
    //    if (requesturl.IndexOf("/en/") > 0)
    //    {
    //        language = "EN";
    //    }
    //    else
    //    {
    //        language = "CN";
    //    }
    //    return language;
    //}

    /// <summary>
    /// 获取系统级别的公司
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public static string GetSysCompany(System.Web.SessionState.HttpSessionState session)
    {
        return session["SystemCompany"].ToString();
    }
    public static Nordasoft.Data.Sql.DataBase GetDB(System.Web.SessionState.HttpSessionState session)
    {
        return BasePage.GetDB(session);
        //string cnnstr = session["SystemConnection"].ToString();
        //Nordasoft.Data.Sql.DataBase db = new DataBase(cnnstr);
        //return db;

    }
//    /// <summary>
//    /// 获取连接字符串
//    /// </summary>
//    /// <param name="session"></param>
//    /// <returns></returns>
//    public static string GetConnectionString(System.Web.SessionState.HttpSessionState session)
//    {
//        return session["SystemConnection"].ToString();
//    }
//    /// <summary>
//    /// 根据配置，获取按钮表
//    /// </summary>
//    /// <param name="btnconfigs"></param>
//    /// <returns></returns>
//    public static System.Data.DataTable GetButtonTable(System.Collections.ArrayList btnconfigs, System.Web.SessionState.HttpSessionState session, string language)
//    {
//        System.Data.DataTable dtviewbtn = new System.Data.DataTable();
//        dtviewbtn.Columns.Add("PID", typeof(string));   //父结点
//        dtviewbtn.Columns.Add("PText", typeof(string));   //父结点
//        dtviewbtn.Columns.Add("BtnID", typeof(string));
//        dtviewbtn.Columns.Add("Text", typeof(string));
//        dtviewbtn.Columns.Add("Ico", typeof(string));
//        dtviewbtn.Columns.Add("Click", typeof(string));
//        string systemCompany = Common.GetSysCompany(session);

//        foreach (string c in btnconfigs)
//        {
//            //判断是否为多级菜单
//            string[] mconfig = c.Split(':');
//            if (mconfig.Length == 2)
//            {
//                //多级菜单
//                string[] config = mconfig[1].Split('|');
//                foreach (string mc in config)
//                {
//                    string[] mcc = mc.Split('.');
//                    if (mcc.Length == 2)
//                    {
//                        InitButtonRow(dtviewbtn, mcc, mconfig[0], language);
//                    }
//                    else
//                        continue;
//                }

//            }
//            else
//            {
//                string[] config = c.Split('.');
//                if (config.Length == 2)
//                {
//                    InitButtonRow(dtviewbtn, config, string.Empty, language);
//                }
//            }
            
//        }
//        return dtviewbtn;
//    }

//    private static void InitButtonRow(System.Data.DataTable dtviewbtn, string[] config, string pid, string language)
//    {
//        System.Data.DataTable functionTable=CacheManager.FunctionTable;
//        System.Data.DataRow[] rows = functionTable.Select(string.Format("ID={0}", config[0]));
//        if (rows.Length != 1)
//        {
//            throw new ApplicationException(string.Format("未找到ID为[{0}]的功能项.", config[0]));
//        }
//        string namefield = "Name";
//        if (string.IsNullOrEmpty(language) == false)
//        {
//            namefield = string.Format("{0}_{1}", namefield, language);
//        }
//        string name = rows[0][namefield].ToString();
//        if (string.IsNullOrEmpty(name))
//        {
//            name = rows[0]["Name"].ToString();
//        }
//        string ico = rows[0]["ICO"].ToString();
//        string code = rows[0]["Code"].ToString();

//        System.Data.DataRow row = dtviewbtn.NewRow();

//        if (string.IsNullOrEmpty(pid) == false)
//        {
//            //读取名称
//            System.Data.DataRow[] prows = functionTable.Select(string.Format("ID={0}", pid));
//            if (prows.Length != 1)
//            {
//                throw new ApplicationException(string.Format("未找到ID为[{0}]的功能项.", pid));
//            }
//            row["PID"] = string.Format("btn_{0}_{1}", pid, prows[0]["Code"]);
//            //row["PID"] = string.Format("{0}", pid);
//            //row["PID"] = "8";
//            //row["PID"] = pid;
//            row["PText"] = prows[0]["Name"];
//        }
//        else
//        {
//            row["PID"] = string.Empty;
//        }
//        row["BtnID"] = string.Format("btn_{0}_{1}", config[0], code);
//        row["Text"] = name;
//        row["Ico"] = ico;
//        row["click"] = config[1];
//        dtviewbtn.Rows.Add(row);
//    }
////    /// <summary>
////    /// 获取基础数据JS
////    /// </summary>
////    /// <param name="cnnstr"></param>
////    /// <returns></returns>
////    public static string GetBaseDataString(string cnnstr)
////    {
////        System.Data.SqlClient.SqlCommand cmmd = new System.Data.SqlClient.SqlCommand(@"
////IF EXISTS(SELECT 1  FROM [dbo].[Sys_ComboBox] WHERE ([LastUpdateTime]!=[LastGetTime] OR [LastGetTime] IS NULL)) --暂时去掉限制条件
////BEGIN
////    DECLARE @SQL	NVARCHAR(MAX);
////    SET @SQL='';
////
////    DECLARE @TNAME	VARCHAR(MAX);
////    SET @TNAME='';
////    SELECT @SQL=@SQL+'SELECT ID,CONVERT(VARCHAR(10),CODE) AS CODE,NAME FROM '+TableName+' WHERE Activated=1 '+ ISNULL(OrderBySQL,'')+';',@TNAME=@TNAME+ObjectName+';' FROM Sys_ComboBox
////    SET @SQL=@SQL+'
////	UPDATE [Sys_ComboBox] SET [LastGetTime]=ISNULL([LastGetTime],LastUpdateTime) WHERE ([LastUpdateTime]!=[LastGetTime] OR [LastGetTime] IS NULL)
////	'
////    SELECT @TNAME
////    EXEC SP_EXECUTESQL @SQL
////END
////");
////        Nordasoft.Data.Sql.DataBase db = new DataBase(cnnstr);
////        System.Data.DataSet ds = db.ExecuteDataset(cmmd);
////        if (ds.Tables.Count == 0)
////            return string.Empty;
////        System.Text.StringBuilder sb = new System.Text.StringBuilder();

////        string[] tablenames = ds.Tables[0].Rows[0][0].ToString().Split(';');
////        for (int i = 1; i < ds.Tables.Count; i++)
////        {
////            sb.AppendFormat("var {0}={1}", tablenames[i - 1], JSON.Encode(ds.Tables[i]));
////            sb.AppendLine();
////        }
////        sb.Replace("\"ID\"", "ID");
////        sb.Replace("\"CODE\"", "CODE");
////        sb.Replace("\"NAME\"", "NAME");
////        return sb.ToString();

////    }
//    //public static void CreateJSFile()
//    //{

        
//    //    System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter("SELECT Company,ConnStr FROM CompanyConfig",
//    //        System.Configuration.ConfigurationManager.ConnectionStrings["VLP"].ConnectionString);
//    //    System.Data.DataTable dt = new System.Data.DataTable();
//    //    da.Fill(dt);
//    //    foreach (System.Data.DataRow row in dt.Rows)
//    //    {
//    //        string js = GetBaseDataString(row["ConnStr"].ToString());
//    //        if (string.IsNullOrEmpty(js) == false)
//    //        {
//    //            System.IO.StreamWriter writer = System.IO.File.CreateText(string.Format("{0}js\\basedata\\{1}_jsdata.js", AppDomain.CurrentDomain.BaseDirectory, row["Company"]));
//    //            writer.Write(js);
//    //            writer.WriteLine();
//    //            writer.Write("try {setComboxData();} catch(e) {}");
//    //            writer.Flush();
//    //            writer.Close();
//    //        }
//    //    }
 

//    //}

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="pageid"></param>
//    /// <param name="ids"></param>
//    /// <returns></returns>
//    public static System.Data.DataTable GetDataByIDS(string pageid, string ids, string userid, DataBase db)
//    {
//        string sql = string.Format(@"
//	DECLARE @SQL	NVARCHAR(MAX);
//    DECLARE @V      NVARCHAR(MAX);
//	DECLARE @PKNAME	VARCHAR(50);
//	SELECT @SQL=SU.SELECTSQL,@PKNAME=S.PKName FROM dbo.Sys_Search_Page_User SU INNER JOIN dbo.Sys_Search_Page S ON S.PageID=SU.PageID
//		WHERE SU.UserID=@UserID AND SU.PageID=@PageID;
//    SET @V='INNER JOIN dbo.SplitIndx(''{0}'')excelIndx  ON [excelIndx].INDX=M.'+@PKNAME
//	+' ORDER BY [excelIndx].ID'
//    SET @SQL=@SQL+@V;
//	EXEC sys.sp_executesql @SQL
//
//", ids);
//        System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sql);
//        cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
//        cmd.Parameters["@UserID"].Value = userid;
//        cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageID", System.Data.SqlDbType.SmallInt));
//        cmd.Parameters["@PageID"].Value = pageid;
//        //cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@IDS", System.Data.SqlDbType.VarChar, -1));
//        //cmd.Parameters["@IDS"].Value = ids;
//        cmd.CommandText = sql;
//        return db.ExecuteDataTable(cmd);
//    }

    /// <summary>
    /// 根据类型名，获取表或视图名
    /// </summary>
    /// <param name="typename"></param>
    /// <returns></returns>
    public static string GetTableNameByTypeName(string typename)
    {
        string tablename = typename;
        switch (typename)
        {
            case "Customer":
                tablename = "D_Customer";
                break;
            case "Customer_Address":
                tablename = "D_Customer_Address";
                break;
            case "Customer_Address_Consignee":
                tablename = "D_Customer_Address";
                break;
            case "MRG_COP_G_NO":
                tablename = "V_MRG_COP_G_NO";
                break;
            default:
                break;
        }
        return tablename;
    }
    /// <summary>
    /// 处理下拉事件中的委托
    /// </summary>
    /// <param name="context"></param>
    /// <param name="tableName"></param>
    /// <param name="ParamsID"></param>
    /// <param name="sender"></param>
    public static void ComboBoxTableNameHander(HttpContext context, ref string tableName, ref string ParamsID, object sender)
    {
        //如果是小料号的货物信息，则进行验证
        if (tableName.Equals("GoodsIDByCustomer"))
        {
            UserInfo tempuser = (UserInfo)BasePage.GetCurrentUser(context.Session);
            if (tempuser != null && tempuser.IsCustomerGoods == false)
            {
                tableName = "GoodsIDByCustomer_all";
                ParamsID = "";
            }
        }
    }
    public static void InitSpecialComboBoxSqlSetting(object sender, EventArgs e)
    {
        VLP.BS.CacheManager.ComboBoxSQLAdd("GoodsLotTemplate", " SELECT TOP 15 T2.ID,T2.CODE,T2.NAME,T2.NAME_E,T2.LotTemplateBillID,D.LotTemplateName FROM  dbo.D_GoodsInfo AS T2  LEFT JOIN dbo.D_LotTemplate AS D ON D.ID=T2.LotTemplateBillID  WHERE T2.Activated=1 AND (T2.CODE LIKE @V OR T2.NAME LIKE @V OR T2.NAME_E LIKE @V)");
        VLP.BS.CacheManager.ComboBoxSQLAdd("LotTemplate", " SELECT ID,LotTemplateCode AS CODE,LotTemplateName AS NAME FROM dbo.D_LotTemplate WHERE Activated=1 AND (LotTemplateCode LIKE @V OR LotTemplateName LIKE @V) ");
       VLP.BS.CacheManager.ComboBoxSQLAdd("DeptByCompanyID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.Sys_D_Department WHERE Activated=1 AND CompanyID = @Param");
        VLP.BS.CacheManager.ComboBoxSQLAdd("YesOrNo", @"SELECT 0 AS ID,0 AS CODE,'否' AS NAME UNION SELECT 1 AS ID,1 AS CODE,'是' AS NAME ");
        VLP.BS.CacheManager.ComboBoxSQLAdd("CityByProviceID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_City WHERE Activated=1 AND ProvinceID=@Param");
        VLP.BS.CacheManager.ComboBoxSQLAdd("PackageByGoodsID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.V_D_GoodPackage WHERE GoodsID=@Param");
        //改用客户货物关系表中取数据
        VLP.BS.CacheManager.ComboBoxSQLAdd("GoodsIDByCustomer", "SELECT TOP 15 T2.ID,T2.CODE,T2.NAME,T2.NAME_E FROM D_Customer_Goods AS T1 INNER JOIN dbo.D_GoodsInfo AS T2 ON T1.GoodsID=T2.ID AND T2.Activated=1 WHERE T1.CustomerID=@Param AND (T2.CODE LIKE @V OR T2.NAME LIKE @V OR T2.NAME_E LIKE @V)");
        //下拉全部的货物信息
        VLP.BS.CacheManager.ComboBoxSQLAdd("GoodsIDByCustomer_all", "SELECT TOP 15 T2.ID,T2.CODE,T2.NAME,T2.NAME_E  FROM  dbo.D_GoodsInfo AS T2  WHERE T2.Activated=1  AND (T2.CODE LIKE @V OR T2.NAME LIKE @V OR T2.NAME_E LIKE @V)");
        //根据业务员取大料号的数据
        VLP.BS.CacheManager.ComboBoxSQLAdd("MRG_COP_G_NOBySaleBy", "  SELECT TOP 15 T1.ID,T1.CODE,T1.NAME,T1.NAME_E FROM V_MRG_COP_G_NO AS T1 WHERE  (T1.Sales_By=@Param) AND (T1.CODE LIKE @V OR T1.NAME LIKE @V OR T1.NAME_E LIKE @V)");

        //根据业务员取小料号的数据
        VLP.BS.CacheManager.ComboBoxSQLAdd("GoodsIDBySaleBy", "SELECT TOP 15 T2.ID,T2.CODE,T2.NAME,T2.NAME_E FROM dbo.D_GoodsInfo AS T2 INNER JOIN dbo.D_WBK_MRG AS T1 ON T1.ID=T2.MRG_SM_ID  WHERE  T2.Activated=1 AND (T1.Sales_By=@Param) AND (T2.CODE LIKE @V OR T2.NAME LIKE @V OR T2.NAME_E LIKE @V)");

        //VLP.BS.CacheManager.ComboBoxSQLAdd("GoodsIDByCustomer", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_GoodsInfo WHERE  Activated=1 and CustomerID= @Param");
        VLP.BS.CacheManager.ComboBoxSQLAdd("WHouse", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='2' and Activated=1 and WhareHouseID= @Param");

        //根据库区查询库位
        VLP.BS.CacheManager.ComboBoxSQLAdd("LoctionByAreaID", "SELECT  ID,CODE,NAME,NAME_E FROM dbo.D_Location WHERE  Activated=1 and AreaID=@Param AND (CODE LIKE @V OR NAME LIKE @V OR NAME_E LIKE @V)");

        //根据库区查询库位
        VLP.BS.CacheManager.ComboBoxSQLAdd("AreaIDByWhouse", "SELECT  ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE  Activated=1 AND WhareHouseID=@Param AND (CODE LIKE @V OR NAME LIKE @V OR NAME_E LIKE @V)");


        VLP.BS.CacheManager.ComboBoxSQLAdd("HandoverUser", "SELECT ID,CODE,NAME,NAME_E FROM dbo.M_EXPRESS_USER WHERE Activated=1 AND ISNULL(CustomerNoType,0)=0 AND (CODE LIKE @V OR NAME LIKE @V OR NAME_E LIKE @V) ");

        //库区类型收货区
        VLP.BS.CacheManager.ComboBoxSQLAdd("AreaByWHouseID_AT2", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='2' and Activated=1 and WhareHouseID= @Param");
        //库区类型存储区
        VLP.BS.CacheManager.ComboBoxSQLAdd("AreaByWHouseID_AT1", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE (AreaType='1' OR AreaType='4' OR AreaType='5')and Activated=1 and WhareHouseID= @Param AND (CODE LIKE @V OR NAME LIKE @V)");
        //如果加上@V条件则联动报错，不知道为何要增加这个，如果有其他用的地方需要增加的，则单独摘除，此处我先修改为原方式，有问题修改到这的 联系我 joey 2016-5-5
        //库存明细查询  库区下拉 不分库区类型
        VLP.BS.CacheManager.ComboBoxSQLAdd("AreaByWHouseID_AT12", "SELECT TOP 15 ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE Activated=1 and WhareHouseID= @Param AND (CODE LIKE @V OR NAME LIKE @V OR NAME_E LIKE @V)");

        //库区类型发货区
        VLP.BS.CacheManager.ComboBoxSQLAdd("AreaByWHouseID_AT3", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='3' and Activated=1 and WhareHouseID= @Param");

        //库区类型备货区
        VLP.BS.CacheManager.ComboBoxSQLAdd("AreaByWHouseID_AT4", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='4' and Activated=1 and WhareHouseID= @Param");

        //发货人
        VLP.BS.CacheManager.ComboBoxSQLAdd("Customer_Address", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Customer_Address WHERE Activated=1 AND SHIPPER = 1 AND Customer_Id= @Param");

        //收货人
        VLP.BS.CacheManager.ComboBoxSQLAdd("Customer_Address_Consignee", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Customer_Address WHERE Activated=1 AND CONSIGNEE = 1 AND Customer_Id= @Param");

        //提供货单对应的业务号
        VLP.BS.CacheManager.ComboBoxSQLAdd("BusiNoByTPDNO", "SELECT BUSIBILLNO AS ID,BUSIBILLNO AS CODE,BUSIBILLNO AS NAME,'' AS NAME_E FROM V_B_TRAN_PICK_DELIVER_BUSINO WHERE TPDBILLNO=@Param");

        //根据货物ID取批次
        VLP.BS.CacheManager.ComboBoxSQLAdd("GoodsPropertyIDByGoodsID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.V_B_GoodsPropertyID WHERE GoodsID=@Param and Activated=1 ");

        //_ComboBoxSQLSetting = new NameValueCollection();
        ////根据公司ID获取对应的部门信息--参数请用@Param 进行配置
        //_ComboBoxSQLSetting.Add("DeptByCompanyID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.Sys_D_Department WHERE Activated=1 AND CompanyID = @Param");
        //_ComboBoxSQLSetting.Add("YesOrNo", @"SELECT 0 AS ID,0 AS CODE,'否' AS NAME UNION SELECT 1 AS ID,1 AS CODE,'是' AS NAME ");
        //_ComboBoxSQLSetting.Add("CityByProviceID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_City WHERE Activated=1 AND ProvinceID=@Param");
        //_ComboBoxSQLSetting.Add("PackageByGoodsID", "SELECT ID,CODE,NAME,NAME_E FROM dbo.V_D_GoodPackage WHERE GoodsID=@Param");
        //_ComboBoxSQLSetting.Add("GoodsIDByCustomer", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_GoodsInfo WHERE  Activated=1 and CustomerID= @Param");
        //_ComboBoxSQLSetting.Add("WHouse", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='2' and Activated=1 and WhareHouseID= @Param");



        ////库区类型收货区
        //_ComboBoxSQLSetting.Add("AreaByWHouseID_AT2", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='2' and Activated=1 and WhareHouseID= @Param");
        ////库区类型存储区
        //_ComboBoxSQLSetting.Add("AreaByWHouseID_AT1", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='1' and Activated=1 and WhareHouseID= @Param");

        ////库区类型发货区
        //_ComboBoxSQLSetting.Add("AreaByWHouseID_AT3", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='3' and Activated=1 and WhareHouseID= @Param");

        ////库区类型备货区
        //_ComboBoxSQLSetting.Add("AreaByWHouseID_AT4", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Area WHERE AreaType='4' and Activated=1 and WhareHouseID= @Param");

        ////发货人
        //_ComboBoxSQLSetting.Add("Customer_Address", "SELECT ID,CODE,NAME,NAME_E FROM dbo.D_Customer_Address WHERE Activated=1 AND Customer_Id= @Param");
    }


    /// <summary>
    /// 飞机实际飞行航迹 ToExcel
    /// </summary>
    /// <param name="dtDetail"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static MemoryStream DataModuleTableToExcel(DataTable dtDetail, string fileName)
    {
        HSSFWorkbook hssfworkbook = new HSSFWorkbook();

        int colindx = 0;
        int rowindx = 0;
        ICellStyle styleLine = hssfworkbook.CreateCellStyle();
        IFont fontLine = hssfworkbook.CreateFont();
        fontLine.FontName = "宋体";
        fontLine.FontHeightInPoints = (short)10;
        styleLine.SetFont(fontLine);
        styleLine.BorderTop = NPOI.SS.UserModel.BorderStyle.THIN;//上边框 //细线
        styleLine.BorderLeft = NPOI.SS.UserModel.BorderStyle.THIN;//左边框 
        styleLine.BorderBottom = NPOI.SS.UserModel.BorderStyle.THIN;//下边框 //粗线
        styleLine.BorderRight = NPOI.SS.UserModel.BorderStyle.THIN;//右边框
        //styleLine.Alignment = HSSFCellStyle.ALIGN_CENTER;



        ISheet sheet1 = hssfworkbook.CreateSheet("飞机实际飞行航迹");//"Sheet1"
        System.Web.UI.HtmlControls.HtmlTable extab = new System.Web.UI.HtmlControls.HtmlTable();
        extab.Border = 1;
        extab.BorderColor = "Black";
        extab.CellPadding = 1;
        extab.CellSpacing = 1;
        sheet1.PrintSetup.Landscape = true;
        sheet1.PrintSetup.PaperSize = 9;
        sheet1.SetMargin(MarginType.LeftMargin, (double)0.2);// 页边距（左） 
        sheet1.SetMargin(MarginType.RightMargin, (double)0.2);// 页边距（右） 

        //黑色  加粗 大小10 (明细表头）
        ICellStyle style1 = hssfworkbook.CreateCellStyle();
        IFont font1 = hssfworkbook.CreateFont();
        font1.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.BOLD;
        font1.FontName = "宋体";
        font1.FontHeightInPoints = (short)10;
        style1.SetFont(font1);
        style1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.CENTER;
        style1.BorderTop = NPOI.SS.UserModel.BorderStyle.THICK;//上边框 //细线
        style1.BorderLeft = NPOI.SS.UserModel.BorderStyle.THIN;//左边框 
        style1.BorderBottom = NPOI.SS.UserModel.BorderStyle.THIN;//下边框 //粗线
        style1.BorderRight = NPOI.SS.UserModel.BorderStyle.THIN;//右边框

        //明细的表头
        IRow headrow = sheet1.CreateRow(0);
        ICell cell = headrow.CreateCell(0);
        foreach (DataColumn col in dtDetail.Columns)
        {
            if (col.ColumnName != "BILLNO")
            {
                cell = headrow.CreateCell(colindx);
                cell.SetCellValue(col.ColumnName);
                cell.CellStyle = style1;
                sheet1.SetColumnWidth(cell.ColumnIndex, 3000);
                colindx++;
            }
        }

        colindx = 0;
        rowindx = 0;

        //中间明细部分
        foreach (DataRow row in dtDetail.Rows)
        {
            rowindx++;
            IRow temrow = sheet1.CreateRow(rowindx);
            colindx = 0;
            foreach (DataColumn col in dtDetail.Columns)
            {
                ICell tempcell = temrow.CreateCell(colindx);
                tempcell.CellStyle = styleLine;
                Type datatype = row[col.ColumnName].GetType();
                if (datatype == typeof(System.String))
                    tempcell.SetCellValue(row[col.ColumnName].ToString());
                else if (row[col.ColumnName].GetType() == typeof(System.DateTime) && row[col.ColumnName] != null && row[col.ColumnName] != DBNull.Value)
                {
                    DateTime dtv = Convert.ToDateTime(row[col.ColumnName]);
                    tempcell.SetCellValue(dtv.ToString("yyyy-MM-dd"));
                }
                else if (row[col.ColumnName].GetType() == typeof(System.Boolean) && row[col.ColumnName] != null)
                {
                    if (row[col.ColumnName].ToString().ToUpper() == "TRUE")
                    {
                        tempcell.SetCellValue("是");
                    }
                    else { tempcell.SetCellValue("否"); }
                }
                else if (datatype == typeof(decimal) || datatype == typeof(int) || datatype == typeof(float)
                || datatype == typeof(double))
                {
                    double dvalue = 0;
                    double.TryParse(row[col.ColumnName].ToString(), out dvalue);
                    tempcell.SetCellValue(dvalue);
                }
                else { tempcell.SetCellValue(row[col.ColumnName].ToString()); }
                colindx++;
            }
        }

        MemoryStream file = new MemoryStream();
        hssfworkbook.Write(file);
        return file;
    }

    #region 拼接字符串生成Excel
    /// <summary>
    /// 拼接字符串生成Excel
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static byte[] ExportTable(System.Data.DataTable dt)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

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
        foreach (DataColumn column in dt.Columns)
        {

            sb.AppendLine("<td>" + column.ColumnName + "</td>");

        }
        sb.AppendLine("</tr>");

        //写出数据
        foreach (DataRow row in dt.Rows)
        {
            foreach (DataColumn column in dt.Columns)
            {
                string filed = column.ColumnName;
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
        byte[] byteArray = Encoding.UTF8.GetBytes(sb.ToString());
        return byteArray;
    }
    #endregion


    /// <summary>
    /// 记录命令动作日志
    /// </summary>
    /// <param name="taskID">任务ID</param>
    /// <param name="uavID">飞机ID</param>
    /// <param name="typeName">命令动作名称</param>
    /// <param name="num">盘旋圈数 or 上升下降度数 or 左飞右飞度数 </param>
    /// <param name="isOK">成功 or 失败</param>
    /// <param name="msg">失败信息</param>
    /// <param name="type">命令类型： 0 点击命令  1 定时器命令</param>
    /// <returns></returns>
    public static SqlCommand saveControlTypeLog(string taskID, string uavID, string typeName, string num, bool isOK, string msg, int type, int userId)
    {
        SqlCommand cmmd = new SqlCommand("sp_saveControlTypeLog");
        cmmd.CommandType = CommandType.StoredProcedure;

        cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);

        cmmd.Parameters.Add("@UavID", SqlDbType.Int);
        cmmd.Parameters["@UavID"].Value = Convert.ToInt32(uavID);

        cmmd.Parameters.Add("@TypeName", SqlDbType.NVarChar, 500);//操作动作名称
        cmmd.Parameters["@TypeName"].Value = typeName;

        cmmd.Parameters.Add("@Num", SqlDbType.NVarChar, 20);
        cmmd.Parameters["@Num"].Value = (num == null || "" == num) ? "" : num;

        cmmd.Parameters.Add("@IsOK", SqlDbType.Int);
        cmmd.Parameters["@IsOK"].Value = isOK ? 1 : 0;

        cmmd.Parameters.Add("@Msg", SqlDbType.NVarChar, -1);
        cmmd.Parameters["@Msg"].Value = (msg == "" || null == msg) ? "" : msg;

        cmmd.Parameters.Add("@USERID", SqlDbType.Int);
        cmmd.Parameters["@USERID"].Value = userId;

        cmmd.Parameters.Add("@Type", SqlDbType.Int);
        cmmd.Parameters["@Type"].Value = type;

        return cmmd;
    }


    /// <summary>
    /// 计算两点位置的距离，返回两点的距离，单位 米
    /// 该公式为GOOGLE提供，误差小于0.2米
    /// </summary>
    /// <param name="lat1">第一点纬度</param>
    /// <param name="lng1">第一点经度</param>
    /// <param name="lat2">第二点纬度</param>
    /// <param name="lng2">第二点经度</param>
    /// <returns></returns>
    public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
    {
        double radLat1 = Rad(lat1);
        double radLng1 = Rad(lng1);
        double radLat2 = Rad(lat2);
        double radLng2 = Rad(lng2);
        double a = radLat1 - radLat2;
        double b = radLng1 - radLng2;
        double result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
        return result;
    }

    /// <summary>
    /// 经纬度转化成弧度
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    private static double Rad(double d)
    {
        return (double)d * Math.PI / 180d;
    }
}