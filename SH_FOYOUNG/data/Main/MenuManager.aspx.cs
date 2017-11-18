using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class data_System_MenuManager : BasePage
{
    UserInfo currentUserInfo = null;
    //菜单处理
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
    protected void BeforeInvoke(String methodName)
    {
     
    }

    //日志管理
    protected void AfterInvoke(String methodName)
    {
        
    }
    #region 待处理数据+图表显示
    /// <summary>
    /// 获取用户要显示的待处理数据
    /// </summary>
    public void GetNeedDealThing()
    {
        currentUserInfo = GetUserInfo;
        if (IsOut(this.Context)) return;
        SqlCommand sqlcom = new SqlCommand();

        StringBuilder sql = new StringBuilder();
        sql.AppendFormat(" exec sp_Sys_GetBacklog {0};exec sp_Sys_GetStatistics {0}", currentUserInfo.ID); //获取待办事项sql
        sqlcom.CommandText = sql.ToString();
        sqlcom.CommandType = CommandType.Text; ;

        Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "true";
        DataSet ds = DB.ExecuteDataset(sqlcom);


        if (ds != null && ds.Tables.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            //第一个是 待办事项
            if (ds.Tables.Count > 0 && ds.Tables[0] != null)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    sb.AppendFormat(@" 
                     <div value='{4}' LinkUrl='{5}' ShowName='{6}' class='Task' style='background-color: {0}; float: left; margin: 12px;' onclick = 'LoadDetailThing(this)'>
                                <i></i>
                                <div class='centre'>
                                    <div class='icon'>
                                        <img src='{1}' />
                                    </div>
                                    <div class='Number'>{2}</div>
                                </div>
                                <div class='text'>
                                    {3}
                                </div>
                            </div>
                    ", row["BackGroudColor"], row["ImagesUrl"], row["Total"], row["NAME"], row["ID"], row["LinkUrl"], row["NAME"]);
                }
                result["needAlertThing"] = sb.ToString();
            }
            //第一个是 待办事项
            if (ds.Tables.Count > 1 && ds.Tables[1] != null)
            {
                sb = new StringBuilder();
                foreach (DataRow row in ds.Tables[1].Rows)
                {
                    sb.AppendFormat(@" 
                     <div value='{3}' ShowName='{4}' class='Task' style='background-color: {0}; float: left; margin: 12px;' onclick = 'EchartsDetial(this)'>
                                <i></i>
                                <div class='centre'>
                                    <div class='icon'>
                                        <img src='{1}' />
                                    </div>
                                </div>
                                <div class='text'>
                                    {2}
                                </div>
                            </div>
                    ", row["BackGroudColor"], row["ImagesUrl"],row["NAME"], row["ID"], row["NAME"]);
                }
                result["echartsThing"] = sb.ToString();
            }
        }
        else
        {//echartsThing
            result["msg"] = "";
            result["needAlertThing"] = "";
            result["echartsThing"] = "";
        }
        String json = VLP.JSON.Encode(result);
        Response.Write(json);

    }

    /// <summary>
    /// 获取用户要显示的待处理数据明细
    /// </summary>
    public void GetNeedDealThingDetail()
    {
        currentUserInfo = GetUserInfo;
        if (IsOut(this.Context)) return;
        string ids = Request.Form.Get("ID");
        SqlCommand sqlcom = new SqlCommand();
        sqlcom.CommandText = "sp_Sys_GetBacklogDetail ";
        sqlcom.CommandType = CommandType.StoredProcedure;
        sqlcom.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = ids });
        sqlcom.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = currentUserInfo.ID });
        sqlcom.Parameters.Add(new SqlParameter("@Language", SqlDbType.VarChar,2) { Value = currentUserInfo.UserLan });

        DataTable dt = DB.ExecuteDataTable(sqlcom);
        Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "true";
        if (dt != null && dt.Rows.Count > 0)
        {
            StringBuilder sb_head = new StringBuilder();//表头
            StringBuilder sb_body = new StringBuilder();//数据
            sb_head.Append("<thead><tr>");
            string widt = ((100-5) / ((dt.Columns.Count - 1) == 0 ? 1 : dt.Columns.Count - 1)).ToString();

            //优先添加序号列
            sb_head.Append(@"<td style='width: 5%;'><div style='text-align: center;' class='table-header'>序号</div></td>");
            foreach (DataColumn dc in dt.Columns)
            {
                sb_head.AppendFormat(@"<td style='width: {1}%;' {2}>
                                            <div class='table-header'>{0}</div>
                                        </td>", dc.ColumnName, widt, dc.ColumnName == "BILLID" ? "hidden='hidden'" : "");
            }
            sb_head.Append("</tr></thead>");
            sb_body.Append("<tbody>");//style='overflow: auto;'
            for (int row = 0; row < dt.Rows.Count;row++)
            {
                sb_body.Append("<tr>");
                sb_body.AppendFormat(" <td style='text-align: center;'>{0}</td>", row + 1);
                for (int dc = 0; dc < dt.Columns.Count; dc++)
                {
                    //第一列当做主键ID ID 为 BILLID
                    //第二列当做单号， ID 为BILLNO
                    if (dc == 0)
                    {//BILLID
                        sb_body.AppendFormat(" <td {1}>{0}</td>", dt.Rows[row][dt.Columns[dc].ColumnName]," id='BILLID'; hidden='hidden'");
                    }
                    else if (dc == 1)
                    {//BILLNO
                        sb_body.AppendFormat(" <td {1} onclick='DetailClick(this)'><a href='#'>{0}</a></td>", dt.Rows[row][dt.Columns[dc].ColumnName], " id='BILLNO';");
                    }
                    else
                    {
                        sb_body.AppendFormat(" <td>{0}</td>", dt.Rows[row][dt.Columns[dc].ColumnName]);
                    }
                }
                sb_body.Append("</tr>");
            }
            sb_body.Append("</tbody>");
            result["sbstring"] = sb_head.ToString() + sb_body.ToString();
        }
        else
        {
            result["msg"] = "";
            result["sbstring"] = "";
        }
        String json = VLP.JSON.Encode(result);
        Response.Write(json);

    }


    /// <summary>
    /// 获取用户要显示的具体图表信息
    /// </summary>
    public void GetEchartsData()
    {
        currentUserInfo = GetUserInfo;
        if (IsOut(this.Context)) return;
        string ids = Request.Form.Get("ID");
        string Charttype = Request.Form.Get("Charttype");//图表的类型 pie:饼图 line:线图 bar:柱图
        string MonthType = Request.Form.Get("MonthType");//时间范围的类型 week:一周内(显示日) month:一月内(显示周) year:一年内(显示月)
        string nameType = Request.Form.Get("NameType");//统计名称
        SqlCommand sqlcom = new SqlCommand();
        sqlcom.CommandText = "sp_Sys_GetStatisticsData ";
        sqlcom.CommandType = CommandType.StoredProcedure;
        sqlcom.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = ids });
        sqlcom.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = currentUserInfo.ID });
        int TimeFlag = 0; //时间类型参数 0代表日1代表月2代表年3代表不按时间分组
        int IsGroup = 1;// --1代表按GroupField分组,0不分组 默认为分组，不分组的情况待定
        DateTime dateStart = new DateTime();
        DateTime dateEnd = new DateTime();
        Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "true";
        if (MonthType == "week")
        {
            TimeFlag = 0;
            dateStart = DateTime.Now.AddDays(-7);
            dateEnd = DateTime.Now;
        }
        else if (MonthType == "month")
        {
            TimeFlag = 0;
            dateStart = DateTime.Now.AddMonths(-1);
            dateEnd = DateTime.Now;
        }
        else if (MonthType == "year")
        {
            TimeFlag = 1;
            dateStart = DateTime.Now.AddYears(-1);
            dateEnd = DateTime.Now;
        }
        else if (MonthType == "selftime")
        {
            string seldtemss = Request.Form.Get("Selftimes");
            string seldtemse = Request.Form.Get("Selftimee");
            if (DateTime.TryParse(seldtemss, out dateStart) == false)
            {
                result["msg"] = "开始日期范围不正确.";
                String json1 = VLP.JSON.Encode(result);
                Response.Write(json1);
                return;
            }
            if (DateTime.TryParse(seldtemse, out dateEnd) == false)
            {
                result["msg"] = "结束日期范围不正确.";
                String json2 = VLP.JSON.Encode(result);
                Response.Write(json2);
                return;
            }
        }
        sqlcom.Parameters.Add(new SqlParameter("@TimeFlag", SqlDbType.TinyInt) { Value = TimeFlag });
        sqlcom.Parameters.Add(new SqlParameter("@IsGroup", SqlDbType.Bit) { Value = IsGroup });
        sqlcom.Parameters.Add(new SqlParameter("@BeginDate", SqlDbType.SmallDateTime) { Value = dateStart });
        sqlcom.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.SmallDateTime) { Value = dateEnd });
        sqlcom.Parameters.Add(new SqlParameter("@Language", SqlDbType.VarChar, 2) { Value = currentUserInfo.UserLan });

        DataTable dt = DB.ExecuteDataTable(sqlcom);
      
        if (dt != null && dt.Rows.Count > 0)
        {
            string name_1 = dt.Columns[0].ColumnName;
            string name_2 = dt.Columns[1].ColumnName;
            string name_3 = "TOTAL"; //第三列必须为TOTAL
            StringBuilder sb = new StringBuilder();

            if (Charttype == "line")
            { //折线数据源
                #region 折线数据源
                DataTable dt_legend = new DataTable();
                dt_legend.Columns.Add("legenddata", typeof(string));
                foreach (DataRow row in dt.Rows)
                {//legend 数据处理
                    if (dt_legend.Select(string.Format("legenddata='{0}'", row[name_1])).Length == 0 && row[name_1] != null && row[name_1].ToString() != "")
                    {
                        DataRow r = dt_legend.NewRow();
                        r["legenddata"] = row[name_1];
                        dt_legend.Rows.Add(r);
                    }
                }
                //开始处理要显示的X轴数据
                DataTable dt_xAxis = new DataTable();
                dt_xAxis.Columns.Add("xddata", typeof(string));

                if (MonthType == "week")
                {
                    //首先确认开始的查询的那天，是周几
                    string[] Day = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
                    for (int i = 1; i < 8; i++)
                    {
                        DataRow r = dt_xAxis.NewRow();
                        r["xddata"] = Day[Convert.ToInt16(dateStart.AddDays(i).DayOfWeek)];
                        dt_xAxis.Rows.Add(r);
                    }
                    DateTime ts = new DateTime();
                    foreach (DataRow row in dt.Rows)
                    {//legend 数据处理

                        if (row[name_2] != null && row[name_2].ToString() != "")
                        {
                            if (DateTime.TryParse(row[name_2].ToString(), out ts))
                            {
                                row[name_2] = Day[Convert.ToInt16(ts.DayOfWeek)];
                            }

                        }
                    }
                }
                else if (MonthType == "month")
                {//进一个月的数据

                    TimeSpan ts1 = new TimeSpan(dateStart.Ticks);
                    TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
                    TimeSpan ts = ts1.Subtract(ts2).Duration();
                    for (int i = 0; i <= ts.Days; i++)
                    {
                        DataRow r = dt_xAxis.NewRow();
                        r["xddata"] = dateStart.AddDays(i).ToString("yyyy-MM-dd");
                        dt_xAxis.Rows.Add(r);
                    }
                }
                else if (MonthType == "year")
                {//进一年的数据

                    int year = dateEnd.Year - dateStart.Year;
                    int month = dateEnd.Month - dateStart.Month;
                    int total = year * 12 + month;
                    for (int i = 0; i <= total; i++)
                    {
                        DataRow r = dt_xAxis.NewRow();
                        r["xddata"] = dateStart.AddMonths(i).ToString("yyyy-M");
                        dt_xAxis.Rows.Add(r);
                    }
                }
                else if (MonthType == "selftime")
                {//指定范围统计

                    TimeSpan ts1 = new TimeSpan(dateStart.Ticks);
                    TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
                    TimeSpan ts = ts1.Subtract(ts2).Duration();
                    for (int i = 0; i <= ts.Days; i++)
                    {
                        DataRow r = dt_xAxis.NewRow();
                        r["xddata"] = dateStart.AddDays(i).ToString("yyyy-MM-dd");
                        dt_xAxis.Rows.Add(r);
                    }
                }
                //foreach (DataRow row in dt.Rows)
                //{//x轴 数据处理
                //    if (dt_xAxis.Select(string.Format("xddata='{0}'", row[name_2])).Length == 0 && row[name_2] != null && row[name_2].ToString() != "")
                //    {
                //        DataRow r = dt_xAxis.NewRow();
                //        r["xddata"] = row[name_2];
                //        dt_xAxis.Rows.Add(r);
                //    }
                //}

                sb.Append(@"{
                        tooltip: {
                            trigger: 'axis'
                        },
                         legend: {
                             data: [");
                for (int i = 0; i < dt_legend.Rows.Count; i++)
                {
                    sb.AppendFormat("{0}'{1}'", i == 0 ? "" : ",", dt_legend.Rows[i]["legenddata"]);
                }

                sb.Append(@" ]
                        },
                         toolbox: {
                            show: true,
                            feature: {
                                mark: { show: true },
                                dataView: { show: true, readOnly: false },
                                magicType: { show: true, type: ['line', 'bar', 'stack', 'tiled'] },
                                restore: { show: true },
                                saveAsImage: { show: true }
                            }
                        },
                        calculable: true,
                        xAxis: [
                            {
                                type: 'category',
                                boundaryGap: false,
                                data: [");
                for (int i = 0; i < dt_xAxis.Rows.Count; i++)
                {
                    sb.AppendFormat("{0}'{1}'", i == 0 ? "" : ",", dt_xAxis.Rows[i]["xddata"]);
                }
                sb.Append(@" ]
                            }
                        ],
                        yAxis: [
                            {
                                type: 'value'
                            }
                        ],
                         series: [");

                for (int i = 0; i < dt_legend.Rows.Count; i++)
                {
                    sb.Append(@"" + (i == 0 ? "" : ",") + @"{
                                name: '" + dt_legend.Rows[i]["legenddata"].ToString() + @"',
                                type: 'line',
                                tiled: '票数',
                                data: [
                    ");

                    for (int j = 0; j < dt_xAxis.Rows.Count; j++)
                    {
                        sb.AppendFormat("{0}{1}", j == 0 ? "" : ",", dt.Select(string.Format("{0}='{1}' and {2}='{3}'", name_1, dt_legend.Rows[i]["legenddata"], name_2, dt_xAxis.Rows[j]["xddata"])).Length == 0 ? "0" :
                            dt.Compute(string.Format("sum({0})", name_3), string.Format("{0}='{1}' and {2}='{3}'", name_1, dt_legend.Rows[i]["legenddata"], name_2, dt_xAxis.Rows[j]["xddata"])));
                    }

                    sb.Append(@"]
                      }");
                }

                sb.Append(@" ]
                    }");
                #endregion

            }
            else if (Charttype == "pie")
            { //饼图
                #region 饼图数据源
                DataTable dt_Bing = new DataTable();
                dt_Bing.Columns.Add(name_1, typeof(string));
                dt_Bing.Columns.Add(name_3, typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    if (dt_Bing.Select(string.Format("{0}='{1}'", name_1, row[name_1])).Length == 0)
                    {
                        DataRow r = dt_Bing.NewRow();
                        r[name_1] = row[name_1];
                        r[name_3] = dt.Compute(string.Format("sum({0})", name_3), string.Format("{0}='{1}'", name_1, row[name_1]));
                        dt_Bing.Rows.Add(r);
                    }
                }

                string text = "";
                string subtext = "";
                if (MonthType == "week")
                {
                    text = dateStart.ToString("yyyy-MM-dd") + " 至 " + dateEnd.ToString("yyyy-MM-dd");
                    subtext =nameType+ "   一周内：" + dt.Compute(string.Format("sum({0})", name_3), "1=1").ToString();
                }
                else if (MonthType == "month")
                {
                    text = dateStart.ToString("yyyy-MM-dd") + " 至 " + dateEnd.ToString("yyyy-MM-dd");
                    subtext = nameType + "   一月内：" + dt.Compute(string.Format("sum({0})", name_3), "1=1").ToString();
                }
                else if (MonthType == "year")
                {
                    text = dateStart.ToString("yyyy-M") + " 至 " + dateEnd.ToString("yyyy-M");
                    subtext = nameType + "   一年内：" + dt.Compute(string.Format("sum({0})", name_3), "1=1").ToString();
                }
                else if (MonthType == "selftime")
                {
                    text = dateStart.ToString("yyyy-MM-dd") + " 至 " + dateEnd.ToString("yyyy-MM-dd");
                    subtext = nameType + "   指定范围统计：" + dt.Compute(string.Format("sum({0})", name_3), "1=1").ToString();
                }
                sb.Append(@"{
                    title: {
                        text: '"+text+@"',
                        subtext: '"+subtext+@"',
                        x: 'center'
                    },
                    tooltip: {
                        trigger: 'item',
                        formatter: '{a} <br/>{b} : {c} ({d}%)'
                    }, legend: {
                    orient: 'vertical',
                    left: 'left',
                    data: [");
                for (int i = 0; i < dt_Bing.Rows.Count; i++)
                {
                    sb.AppendFormat("{0}'{1}'", i == 0 ? "" : ",", dt_Bing.Rows[i][name_1]);
                }
                sb.Append(@"]
                            },
                            series: [
                                {
                                    name: '"+nameType+@"',
                                    type: 'pie',
                                    radius: '55%',
                                    center: ['50%', '60%'],
                                    data: [");
                for (int i = 0; i < dt_Bing.Rows.Count; i++)
                {
                    sb.Append((i == 0 ? "" : ",")+"{ value: "+dt_Bing.Rows[i][name_3].ToString()+", name: '"+dt_Bing.Rows[i][name_1].ToString()+"' }");
                }
                sb.Append(@"],
                            itemStyle: {
                                 normal:{ 
                                  label:{ 
                                    show: true, 
                                    formatter: '{b} : {c} ({d}%)' 
                                  }, 
                                  labelLine :{show:true} 
                                },
                                emphasis: {
                                    shadowBlur: 10,
                                    shadowOffsetX: 0,
                                    shadowColor: 'rgba(0, 0, 0, 0.5)'
                                }
                            }
                        }
                    ]
                }");
                #endregion
               
            }


            result["option"] = sb.ToString();
        }
        else
        {
            result["msg"] = "";
            result["option"] = "";
        }
        String json = VLP.JSON.Encode(result);
        Response.Write(json);
        /*
         *  //饼图
    var option = {
        title: {
            text: '某站点用户访问来源',
            subtext: '纯属虚构',
            x: 'center'
        },
        tooltip: {
            trigger: 'item',
            formatter: "{a} <br/>{b} : {c} ({d}%)"
        },
        legend: {
            orient: 'vertical',
            left: 'left',
            data: ['直接访问', '邮件营销', '联盟广告', '视频广告', '搜索引擎']
        },
        series: [
            {
                name: '访问来源',
                type: 'pie',
                radius: '55%',
                center: ['50%', '60%'],
                data: [
                    { value: 335, name: '直接访问' },
                    { value: 310, name: '邮件营销' },
                    { value: 234, name: '联盟广告' },
                    { value: 135, name: '视频广告' },
                    { value: 1548, name: '搜索引擎' }
                ],
                itemStyle: {
                    emphasis: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }
        ]
    };
    //折线图
    option = {
        tooltip: {
            trigger: 'axis'
        },
        legend: {
            data: ['邮件营销', '联盟广告', '视频广告', '直接访问', '搜索引擎']
        },
        toolbox: {
            show: true,
            feature: {
                mark: { show: true },
                dataView: { show: true, readOnly: false },
                magicType: { show: true, type: ['line', 'bar', 'stack', 'tiled'] },
                restore: { show: true },
                saveAsImage: { show: true }
            }
        },
        calculable: true,
        xAxis: [
            {
                type: 'category',
                boundaryGap: false,
                data: ['周一', '周二', '周三', '周四', '周五', '周六', '周日']
            }
        ],
        yAxis: [
            {
                type: 'value'
            }
        ],
        series: [
            {
                name: '邮件营销',
                type: 'line',
                stack: '总量',
                data: [120, 132, 101, 134, 90, 230, 210]
            },
            {
                name: '联盟广告',
                type: 'line',
                stack: '总量',
                data: [220, 182, 191, 234, 290, 330, 310]
            },
            {
                name: '视频广告',
                type: 'line',
                stack: '总量',
                data: [150, 232, 201, 154, 190, 330, 410]
            },
            {
                name: '直接访问',
                type: 'line',
                stack: '总量',
                data: [320, 332, 301, 334, 390, 330, 320]
            },
            {
                name: '搜索引擎',
                type: 'line',
                stack: '总量',
                data: [820, 932, 901, 934, 1290, 1330, 1320]
            }
        ]
    };
         */
      

    }
    #endregion

    //根据用户获取菜单
    public void GetMenuList()
    {
        currentUserInfo = GetUserInfo;
        if (IsOut(this.Context)) return;

        SqlCommand sqlcom = new SqlCommand();
        sqlcom.CommandText = "sp_Sys_GetMenu";
        sqlcom.CommandType = CommandType.StoredProcedure;
        sqlcom.Parameters.Add(new SqlParameter("@Userid", SqlDbType.Int) { Value = currentUserInfo.ID });
        sqlcom.Parameters.Add(new SqlParameter("@LanguageType", SqlDbType.VarChar,5) { Value = currentUserInfo.UserLan });
        //Hashtable ps = new Hashtable();
        //ps.Add("@Userid",0);
        //ps.Add("@LanguageType", "CN");
        DataTable dt = DB.ExecuteDataTable(sqlcom);
        if (dt != null && dt.Rows.Count>0)
        {
            String json = VLP.JSON.Encode(VLP.JSON.DataTable2ArrayList(dt));
             Response.Write(json);
        }        
    }
    public void Layout()
    {
        string sql = @"
IF NOT EXISTS(SELECT 1 FROM dbo.Sys_LOGIN_LOG WHERE SessionID=@SessionID)
BEGIN
    DELETE Sys_LOGIN_LOG WHERE SessionID=@SessionID
END
";
        try
        {
            SqlCommand cmmd = new SqlCommand(sql);
            cmmd.Parameters.Add(new SqlParameter("@SessionID", SqlDbType.VarChar, 50)).Value = this.Session.SessionID;
            this.DB.ExecuteNonQuery(cmmd);
        }
        catch { }

        this.Session.Clear();
        this.Response.Write("{\"isOut\":\"1\"}");
    }
    public void Login()
    {
        string systemCompany = System.Web.HttpUtility.UrlDecode(Request["CompanyName"]);
        string strUserCode = Request["userCode"];
        string strUserPassword = Request["password"];
        System.Collections.Hashtable result = new System.Collections.Hashtable();
        result["msg"] = "false";
        //String json = "false";
        string cnnstr = System.Configuration.ConfigurationManager.ConnectionStrings["VLP"].ConnectionString;
        string vlpstr = string.Empty;

        if (string.IsNullOrEmpty(systemCompany) || systemCompany=="undefined")
        {
            systemCompany = "复亚飞控系统";
        }
        using (SqlConnection cnn = new SqlConnection(cnnstr))
        {
            SqlCommand cmmd = new SqlCommand("SELECT ConnStr, PrintName,PrintName_E, PrintTel, PrintAddress,IsCustomerGoods  FROM CompanyConfig WHERE Company=@Company");
            SqlParameter sp = new SqlParameter("@Company", SqlDbType.NVarChar, 50);
            sp.Value = systemCompany;
            cmmd.Parameters.Add(sp);
            cmmd.Connection = cnn;
            //cnn.Open();
            //using (SqlDataReader dr = cmmd.ExecuteReader())
            //{
            //    if (dr.Read())
            //    {
            //        vlpstr = dr["ConnStr"].ToString();
            //        Session["SystemConnection"] = vlpstr;
            //        Session["PrintName"] = dr["PrintName"].ToString();
            //        Session["PrintName_E"] = dr["PrintName_E"].ToString();
            //        Session["PrintTel"] = dr["PrintTel"].ToString();
            //        Session["PrintAddress"] = dr["PrintAddress"].ToString();
            //        Session["IsCustomerGoods"] = dr["IsCustomerGoods"];
            //    }
            //}
            SqlDataAdapter da = new SqlDataAdapter(cmmd);
            DataTable dtcompany = new DataTable();
            da.Fill(dtcompany);
            if (dtcompany.Rows.Count > 0)
            {
                DataRow dr = dtcompany.Rows[0];
                vlpstr = dr["ConnStr"].ToString();
                Session["SystemConnection"] = vlpstr;
                Session["PrintName"] = dr["PrintName"].ToString();
                Session["PrintName_E"] = dr["PrintName_E"].ToString();
                Session["PrintTel"] = dr["PrintTel"].ToString();
                Session["PrintAddress"] = dr["PrintAddress"].ToString();
                Session["IsCustomerGoods"] = dr["IsCustomerGoods"];
            }
        }
        //tiler 注释，这里VLP.BS.AutoCommandManager._CNN 所有系统设置，都采用配置库中的信息，不需要设置 2016.04.21
        //VLP.BS.AutoCommandManager._CNN = vlpstr;

        string strUserLan = Request["userLan"];
        string strSql = @"SELECT TOP 1 Sys_D_USER.*,@UserLan AS UserLan,dbo.Sys_D_Company.NAME AS CompanyNm,dbo.Sys_D_Company.NAME_E AS CompanyNmE,
                        dbo.Sys_D_Company.TELE AS CompanyTel,dbo.Sys_D_Company.ADDRESS AS CompanyAddress,dbo.D_Customer.Name AS CustomerName 
                        FROM dbo.Sys_D_USER 
                        LEFT JOIN dbo.Sys_D_Company ON dbo.Sys_D_USER.CompanyID=dbo.Sys_D_Company.ID
                        LEFT JOIN dbo.D_Customer ON dbo.Sys_D_USER.CustomerID = dbo.D_Customer.ID
                        WHERE Sys_D_USER.CODE=@UserCode AND Sys_D_USER.USER_PWD=@UserPassword  AND Sys_D_USER.Activated=1;
IF @@ROWCOUNT=1
BEGIN
    DECLARE @USERID INT;
    SELECT @USERID=ID FROM Sys_D_USER WHERE Sys_D_USER.CODE=@UserCode AND Activated=1;
	INSERT INTO dbo.Sys_LOGIN_LOG
	        ( SessionID ,
              USERID,
	          Login_Date ,
	          UserHostAddress ,
	          UserHostName,
              Flag  
	        )
	VALUES  ( @SessionID , -- SessionID - varchar(50)
              @USERID,
	          GETDATE() , -- Login_Date - datetime
	          @UserHostAddress , -- UserHostAddress - varchar(50)
	          @UserHostName,  -- UserHostName - nvarchar(50)
              1
	        )
END
ELSE
BEGIN
    INSERT INTO dbo.Sys_LOGIN_LOG
	        ( SessionID ,
              USERID,
	          Login_Date ,
	          UserHostAddress ,
	          UserHostName,
              Flag  
	        )
	VALUES  ( @SessionID, -- SessionID - varchar(50)
              -1,
	          GETDATE() , -- Login_Date - datetime
	          @UserHostAddress , -- UserHostAddress - varchar(50)
	          @UserHostName,  -- UserHostName - nvarchar(50)
              0
	        )
END
";

        SqlCommand sqlcom = new SqlCommand();
        sqlcom.CommandText = strSql;
        sqlcom.CommandType = CommandType.Text;
        sqlcom.Parameters.Add("@UserCode", SqlDbType.VarChar);
        sqlcom.Parameters["@UserCode"].Value = strUserCode;
        sqlcom.Parameters.Add("@UserLan", SqlDbType.VarChar);
        sqlcom.Parameters["@UserLan"].Value = strUserLan;
        sqlcom.Parameters.Add("@UserPassword", SqlDbType.VarChar);
        sqlcom.Parameters["@UserPassword"].Value = VLP.WMS.Common.Encrypt(strUserPassword);//"NexfPEcwQ1JVDNgKPr/BRQ=="; //strUserPassword;//TODO:密码加密处理
        sqlcom.Parameters.Add("@SessionID", SqlDbType.VarChar, 50).Value = this.Session.SessionID;
        sqlcom.Parameters.Add("@UserHostAddress", SqlDbType.VarChar, 50).Value = this.Request.UserHostAddress;
        sqlcom.Parameters.Add("@UserHostName", SqlDbType.VarChar, 50).Value = this.Request.UserHostName;
        DataTable dt = DB.ExecuteDataTable(sqlcom);
        
        if (dt != null && dt.Rows.Count == 1)
        {
            dt.Columns.Add("SystemCompanyName", typeof(string));
            dt.Columns.Add("IsCustomerGoods", typeof(bool));
            dt.Rows[0]["IsCustomerGoods"] = Session["IsCustomerGoods"];
            UserInfo tempuserinfo = TableToEntity<UserInfo>(dt).FirstOrDefault();
           // json = "true";
            result["msg"] = "true";
            result["userName"] = tempuserinfo.Name;
            result["CustomerName"] = tempuserinfo.CustomerName;//用户所属客户
            Session["CurrentUserInfo"] = tempuserinfo;
            Session["SystemCompany"] = systemCompany;

            //if (LoginManager.UpdateSessionUserInfo(this.Session.SessionID, tempuserinfo.ID.ToString(), systemCompany, new Nordasoft.Data.Sql.DataBase(cnnstr)))
            //{
            //    json = "true";
            //    Session["CurrentUserInfo"] = tempuserinfo;
            //    Session["SystemCompany"] = systemCompany;
            //}
            //List<UserInfo> userInfo = TableToEntity<UserInfo>(dt);
            //userInfo.FirstOrDefault().SystemCompanyName = systemCompany;

            //Session["CurrentUserInfo"] = userInfo.FirstOrDefault();
            //Session["SystemCompany"] = systemCompany;
            if (!string.IsNullOrEmpty(dt.Rows[0]["CompanyNm"].ToString()))
            {
                Session["PrintName"] = dt.Rows[0]["CompanyNm"].ToString();
            }
            if (!string.IsNullOrEmpty(dt.Rows[0]["CompanyNmE"].ToString()))
            {
                Session["PrintName_E"] = dt.Rows[0]["CompanyNmE"].ToString();
            }
            if (!string.IsNullOrEmpty(dt.Rows[0]["CompanyTel"].ToString()))
            {
                Session["PrintTel"] = dt.Rows[0]["CompanyTel"].ToString();
            }
            if (!string.IsNullOrEmpty(dt.Rows[0]["CompanyAddress"].ToString()))
            {
                Session["PrintAddress"] = dt.Rows[0]["CompanyAddress"].ToString();
            }
        }
        Response.Write(VLP.JSON.Encode(result));
    }


    public void GetCurrentUserInfo()
    {
        String json = string.Empty;
        //if (Session["CurrentUserInfo"] != null && !string.IsNullOrEmpty(Session["CurrentUserInfo"].ToString()))
        //{
        //    json = Session["CurrentUserInfo"].ToString();
        //}

        currentUserInfo = GetUserInfo;
        if (currentUserInfo != null)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(currentUserInfo.GetType());
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, currentUserInfo);
            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);
            json= Encoding.UTF8.GetString(dataBytes);

     
        }
        Response.Write(json);
    }

    //public void GetBaseData()
    //{
    //    string js = Common.GetBaseDataString(DB.GetConnection().ConnectionString);
    //    Response.Write(js);
    //}

    private static List<T> TableToEntity<T>(DataTable dt) where T : class,new()
    {
        Type type = typeof(T);
        List<T> list = new List<T>();

        foreach (DataRow row in dt.Rows)
        {
            PropertyInfo[] pArray = type.GetProperties();
            T entity = new T();
            foreach (PropertyInfo p in pArray)
            {
                if (row[p.Name] is Int64)
                {
                    p.SetValue(entity, Convert.ToInt32(row[p.Name]), null);
                    continue;
                }
                if (row[p.Name] is Int16)
                {
                    //if (row[p.Name] is DBNull)
                    //{
                    //    p.SetValue(entity, null, null);
                    //    continue;
                    //}
                    p.SetValue(entity, Convert.ToInt32(row[p.Name]), null);
                    continue;
                }
                if (row[p.Name] is DBNull)
                {
                    p.SetValue(entity, null, null);
                    continue;
                }
                p.SetValue(entity, row[p.Name], null);
            }
            list.Add(entity);
        }
        return list;
    } 

}