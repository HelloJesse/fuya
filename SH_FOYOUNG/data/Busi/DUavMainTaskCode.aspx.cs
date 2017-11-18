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
using VLP.NAV;
using System.IO;
using ExcelLibrary.SpreadSheet;
using ICSharpCode.SharpZipLib.Zip;
using VLP.BS;
using System.Text;

public partial class data_DUavMainTaskCode : BasePage
{
    UserInfo currentUserInfo = null;

    private const string _FlyWay = "0";//飞行方式：0 船舶跟踪   1：巡航任务

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

    /// <summary>
    /// 基站 航前检查
    /// </summary>
    public void UpdateTaskTrackCheck()
    {
        string bILLID = Request.Form.Get("BILLID");
        string trackCheckType = Request.Form.Get("TrackCheckType");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            SqlCommand cmmd = new SqlCommand("sp_UpdateTaskTrackCheck");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@BILLID", SqlDbType.NVarChar, -1);
            cmmd.Parameters["@BILLID"].Value = bILLID;
            cmmd.Parameters.Add("@TrackCheckType", SqlDbType.Int);
            cmmd.Parameters["@TrackCheckType"].Value = Convert.ToInt32(trackCheckType);
            cmmd.Parameters.Add("@USERID", SqlDbType.Int);
            cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

            DB.ExecuteNonQuery(cmmd);
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 安排任务
    /// 调用补点存储过程注意： 巡航任务不需要再次调用补点存储过程。只有船舶跟踪需要
    /// </summary>
    public void PlanMainTask()
    {
        string bILLID = Request.Form.Get("BILLID");
        string baseStationID = Request.Form.Get("BaseStationID");
        string uavSetID = Request.Form.Get("UavSetID");
        string flyWay = Request.Form.Get("FlyWay");
        string lineHeight = Request.Form.Get("LineHeight");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        try
        {
            SqlCommand cmmd = new SqlCommand("sp_PlanMainTask");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@BILLID", SqlDbType.Int);
            cmmd.Parameters["@BILLID"].Value = Convert.ToInt32(bILLID);
            cmmd.Parameters.Add("@BaseStationID", SqlDbType.Int);
            cmmd.Parameters["@BaseStationID"].Value = Convert.ToInt32(baseStationID);
            cmmd.Parameters.Add("@UavSetID", SqlDbType.Int);
            cmmd.Parameters["@UavSetID"].Value = Convert.ToInt32(uavSetID);
            cmmd.Parameters.Add("@USERID", SqlDbType.Int);
            cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt != null && dt.Rows.Count >= 1 && dt.Rows[0][0] != null && dt.Rows[0][1] != null && dt.Rows[0][2] != null)
            {
                //取出任务表中的 LineTask 记录的点坐标
                string lineTask = dt.Rows[0][0].ToString();
                //基站经纬度
                string latitude = dt.Rows[0][1].ToString();
                string longitude = dt.Rows[0][2].ToString();

                ArrayList rows = (ArrayList)VLP.JSON.Decode(lineTask);
                if (rows == null || rows.Count <= 0)
                {
                    result.IsOK = false;
                    result.ErrMessage = "航线格式化出错";
                }
                else if (string.IsNullOrEmpty(flyWay))
                {
                    result.IsOK = false;
                    result.ErrMessage = "任务类型未指定";
                }
                else
                {
                    Collection<SqlCommand> cmmds = new Collection<SqlCommand>();
                    //航线中的航点高度
                    int heightG = Convert.ToInt32(lineHeight);

                    #region  飞机航线补点操作
                    if (flyWay.Equals(_FlyWay))//船舶跟踪
                    {
                        string Jd1 = ((Hashtable)rows[0])["lng"].ToString();
                        string Wd1 = ((Hashtable)rows[0])["lat"].ToString();
                        string Jd2 = ((Hashtable)rows[1])["lng"].ToString();
                        string Wd2 = ((Hashtable)rows[1])["lat"].ToString();
                        DataTable dtPoint = GetLinepointMiddle(Jd1, Wd1, Jd2, Wd2);
                        if (dtPoint == null || dtPoint.Rows.Count <= 0)//没有补的点。就直接添加
                        {
                            cmmds.Add(saveLineInfoCmmd(Jd1, Wd1, bILLID, uavSetID, 1, 1, heightG));
                            cmmds.Add(saveLineInfoCmmd(Jd2, Wd2, bILLID, uavSetID, 2, 1, heightG));
                        }
                        else
                        {
                            for (int i = 0; i <= dtPoint.Rows.Count; i++)
                            {
                                if (i == 0)//第一段航线的 第一个点
                                {
                                    cmmds.Add(saveLineInfoCmmd(Jd1, Wd1, bILLID, uavSetID, 1, 1, heightG));
                                }
                                else if (i == dtPoint.Rows.Count)//第一段航线的末尾点
                                {
                                    cmmds.Add(saveLineInfoCmmd(Jd2, Wd2, bILLID, uavSetID, (dtPoint.Rows.Count + 1), 1, heightG));
                                    //说明：巡航任务目前不需要，返航-航线 LineCode 8
                                    //addNewFlyBackLine(cmmds, Jd2, Wd2, longitude, latitude, bILLID, uavSetID, heightG);
                                }
                                else
                                {
                                    cmmds.Add(saveLineInfoCmmd(dtPoint.Rows[i]["Jd"].ToString(), dtPoint.Rows[i]["Wd"].ToString(), bILLID, uavSetID, i + 1, 1, heightG));
                                }
                            }
                        }

                    }
                    else//巡航任务
                    {
                        for (int i = 0; i < rows.Count; i++)
                        {
                            cmmds.Add(saveLineInfoCmmd(((Hashtable)rows[i])["lng"].ToString(), ((Hashtable)rows[i])["lat"].ToString(), bILLID, uavSetID, i + 1, 1, heightG));//航段默认都是1
                            if (i == rows.Count - 1)
                            {
                                //添加返航-航线 LineCode 8，需要显示在飞控页面<即：航线1最后一个点 连接到起飞点>
                                addNewFlyBackLine(cmmds, ((Hashtable)rows[i])["lng"].ToString(),
                                    ((Hashtable)rows[i])["lat"].ToString(), longitude, latitude, bILLID, uavSetID, heightG);
                            }
                        }
                    }
                    //返航0 和 降落10
                    //cmmds.Add(saveLineInfoCmmd(longitude, latitude, bILLID, uavSetID, 1, 0, heightG));
                    //cmmds.Add(saveLineInfoCmmd(longitude, latitude, bILLID, uavSetID, 1, 10, heightG));
                    #endregion
                   
                    string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
                    if (!string.IsNullOrEmpty(res))
                    {
                        result.IsOK = false;
                        result.ErrMessage = res;
                    }
                    else
                    {
                        //调用飞机设置航线接口
                        callNav_Set_Line(bILLID, uavSetID, result, msg);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 添加返航航线 ：8
    /// </summary>
    /// <param name="cmmds"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="longitude"></param>
    /// <param name="latitude"></param>
    private void addNewFlyBackLine(Collection<SqlCommand> cmmds, string Jd, string Wd,
        string longitude, string latitude, string bILLID, string uavSetID, int heightG)
    {
        //先进行补点
        DataTable dt = GetLinepointMiddle(Jd, Wd, longitude, latitude);
        if (dt == null || dt.Rows.Count <= 0)
        {
            cmmds.Add(saveLineInfoCmmd(Jd, Wd, bILLID, uavSetID, 1, 8, heightG));
            cmmds.Add(saveLineInfoCmmd(longitude, latitude, bILLID, uavSetID, 2, 8, heightG));
        }
        else
        {
            for (int i = 0; i <= dt.Rows.Count; i++)
            {
                if (i == 0)//第 8 段航线的 第一个点
                {
                    cmmds.Add(saveLineInfoCmmd(Jd, Wd, bILLID, uavSetID, 1, 8, heightG));
                }
                else if (i == dt.Rows.Count)//第 8 段航线的末尾点
                {
                    cmmds.Add(saveLineInfoCmmd(longitude, latitude, bILLID, uavSetID, (dt.Rows.Count + 1), 8, heightG));
                }
                else
                {
                    cmmds.Add(saveLineInfoCmmd(dt.Rows[i]["Jd"].ToString(), dt.Rows[i]["Wd"].ToString(), bILLID, uavSetID, i + 1, 8, heightG));
                }
            }
        }

    }

    /// <summary>
    /// 补点
    /// </summary>
    /// <param name="Jd1"></param>
    /// <param name="Wd1"></param>
    /// <param name="Jd2"></param>
    /// <param name="Wd2"></param>
    /// <returns></returns>
    private DataTable GetLinepointMiddle(string Jd1, string Wd1, string Jd2, string Wd2)
    {
        SqlCommand cmmd = new SqlCommand("sp_Uav_GetLinepointMiddle");
        cmmd.CommandType = CommandType.StoredProcedure;

        cmmd.Parameters.Add("@jd1", SqlDbType.Decimal);
        cmmd.Parameters["@jd1"].Value = Convert.ToDecimal(Jd1);
        cmmd.Parameters.Add("@wd1", SqlDbType.Decimal);
        cmmd.Parameters["@wd1"].Value = Convert.ToDecimal(Wd1);

        cmmd.Parameters.Add("@jd2", SqlDbType.Decimal);
        cmmd.Parameters["@jd2"].Value = Convert.ToDecimal(Jd2);
        cmmd.Parameters.Add("@wd2", SqlDbType.Decimal);
        cmmd.Parameters["@wd2"].Value = Convert.ToDecimal(Wd2);

        return DB.ExecuteDataTable(cmmd);
    }

    /// <summary>
    /// 保存飞机航线补点操作 
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private SqlCommand saveLineInfoCmmd(string Jd, string Wd, string bILLID, string uavID, int sortid, int lineCode,int high) 
    {
        SqlCommand cmmd = new SqlCommand(@"INSERT INTO dbo.D_Uav_FlyLineRecord ( TaskID , LineCode , UavID , Sortid , Jd , Wd,Height_G)
                VALUES  ( @TaskID , @LineCode , @UavID , @Sortid , @Jd , @Wd,@Height_G);");

        cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(bILLID);
        cmmd.Parameters.Add("@LineCode", SqlDbType.Int);
        cmmd.Parameters["@LineCode"].Value = lineCode;
        cmmd.Parameters.Add("@UavID", SqlDbType.Int);
        cmmd.Parameters["@UavID"].Value = Convert.ToInt32(uavID);
        cmmd.Parameters.Add("@Sortid", SqlDbType.Int);
        cmmd.Parameters["@Sortid"].Value = sortid;

        cmmd.Parameters.Add("@Jd", SqlDbType.Decimal);
        cmmd.Parameters["@Jd"].Value = Convert.ToDecimal(Jd);
        cmmd.Parameters.Add("@Wd", SqlDbType.Decimal);
        cmmd.Parameters["@Wd"].Value = Convert.ToDecimal(Wd);

        cmmd.Parameters.Add("@Height_G", SqlDbType.Int);
        cmmd.Parameters["@Height_G"].Value = Convert.ToInt32(high);

        return cmmd; 
    }

    /// <summary>
    /// 调用飞机设置航线接口 1
    /// </summary>
    /// <param name="bILLID"></param>
    /// <param name="uavSetID"></param>
    /// <param name="result"></param>
    /// <param name="msg"></param>
    private void callNav_Set_Line(string bILLID, string uavSetID, LoadFormResultN result, string msg)
    {
        SqlCommand cmmdCode = new SqlCommand("SELECT LineCode FROM D_Uav_FlyLineRecord WHERE TaskID = @TaskID GROUP BY LineCode;");
        cmmdCode.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmdCode.Parameters["@TaskID"].Value = Convert.ToInt32(bILLID);
        DataTable dtLineCode = DB.ExecuteDataTable(cmmdCode);
        if (dtLineCode != null && dtLineCode.Rows.Count > 0)
        {
            //取出所有该任务的航点
            SqlCommand cmmdPoint = new SqlCommand("SELECT LineCode, Sortid, Jd, Wd,Height_G FROM D_Uav_FlyLineRecord WHERE TaskID = @TaskID ORDER BY LineCode, Sortid;");
            cmmdPoint.Parameters.Add("@TaskID", SqlDbType.Int);
            cmmdPoint.Parameters["@TaskID"].Value = Convert.ToInt32(bILLID);
            DataTable dtPoint = DB.ExecuteDataTable(cmmdPoint);
            int lineCode = -1;
            NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavSetID), Convert.ToInt32(bILLID));

            foreach (DataRow dr in dtLineCode.Rows)
            {
                lineCode = Convert.ToInt32(dr["LineCode"].ToString());
                DataRow[] drRows = dtPoint.Select(string.Format("LineCode = {0}", lineCode), "Sortid");

                //循环调用设置航线
                result.IsOK = nav.Nav_Set_Line(setAirlineModel(lineCode, drRows), out msg);
                if (!result.IsOK || !string.IsNullOrEmpty(msg))
                {
                    result.ErrMessage = "设置飞行航线错误,航线编码【" + lineCode + "】错误信息：" + msg;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 调用飞机设置航线接口 2
    /// </summary>
    /// <param name="bILLID"></param>
    /// <param name="uavSetID"></param>
    /// <param name="lineCode">设置指令航线编码</param>
    /// <param name="result"></param>
    /// <param name="msg"></param>
    private void callNav_Set_Line(string bILLID, string uavSetID, int lineCode, LoadFormResultN result, string msg)
    {
        
        //取出指定航线编码任务的航点
        SqlCommand cmmdPoint = new SqlCommand("SELECT LineCode, Sortid, Jd, Wd,Height_G FROM D_Uav_FlyLineRecord WHERE TaskID = @TaskID AND LineCode = @LineCode ORDER BY LineCode, Sortid;");
        cmmdPoint.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmdPoint.Parameters["@TaskID"].Value = Convert.ToInt32(bILLID);
        cmmdPoint.Parameters.Add("@LineCode", SqlDbType.Int);
        cmmdPoint.Parameters["@LineCode"].Value = lineCode;

        DataTable dtPoint = DB.ExecuteDataTable(cmmdPoint);

        NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavSetID), Convert.ToInt32(bILLID));

        DataRow[] drRows = dtPoint.Select();

        result.IsOK = nav.Nav_Set_Line(setAirlineModel(lineCode, drRows), out msg);
        if (!result.IsOK || !string.IsNullOrEmpty(msg))
        {
            result.ErrMessage = "设置飞行航线错误,航线编码【" + lineCode + "】错误信息：" + msg;
        }
    }


    /// <summary>
    /// 设置航线对象
    /// </summary>
    /// <param name="lineCode"></param>
    /// <param name="drRows"></param>
    /// <returns></returns>
    private AirlineModel setAirlineModel(int lineCode, DataRow[] drRows) 
    {
        //设置航点信息
        List<Airlinepoint> airlinepointList = setAirlinepoint(drRows);

        //设置航线对象
        AirlineModel airlineModel = new AirlineModel();
        airlineModel.id = lineCode;
        airlineModel.Airlinecode = lineCode;
        airlineModel.Aircycle = false;
        airlineModel.Airlinepoint = airlinepointList;
        return airlineModel;
    }


    /// <summary>
    /// 航点信息
    /// </summary>
    /// <returns></returns>
    private List<Airlinepoint> setAirlinepoint(DataRow[] drRows)
    {
        List<Airlinepoint> list = new List<Airlinepoint>();
        Airlinepoint airlinepoint = null;
        foreach (DataRow dr in drRows)
        {
            airlinepoint = new Airlinepoint();
            airlinepoint.Airpointcode = (dr["Sortid"] == null || string.IsNullOrEmpty(dr["Sortid"].ToString())) ? 0 : Convert.ToInt32(dr["Sortid"]);
            airlinepoint.Lineswitch = 0;
            airlinepoint.Heightcontrol = 0;
            airlinepoint.Longitude = (dr["Jd"] == null || string.IsNullOrEmpty(dr["Jd"].ToString())) ? 0 : Convert.ToInt64(Convert.ToDecimal(dr["Jd"].ToString()) * 1000000);
            airlinepoint.Latitude = (dr["Wd"] == null || string.IsNullOrEmpty(dr["Wd"].ToString())) ? 0 : Convert.ToInt64(Convert.ToDecimal(dr["Wd"].ToString()) * 1000000);
            airlinepoint.height = (dr["Height_G"] == null || string.IsNullOrEmpty(dr["Height_G"].ToString())) ? 0 : Convert.ToInt32(dr["Height_G"]); ;
            airlinepoint.NextAreaSpeed = 0;
            airlinepoint.Isok_vedio = false;
            airlinepoint.Isok_pictrue = false;
            airlinepoint.Isaction_vedio = false;
            airlinepoint.Isaction_pictrue = false;
            airlinepoint.CycleNum = 0;
            airlinepoint.PicDist = 0;
            list.Add(airlinepoint);
        }
        return list;
    }

    /// <summary>
    /// 修改任务状态
    /// </summary>
    public void UpdateTaskStatus()
    {
        string bILLID = Request.Form.Get("BILLID");
        string taskStatus = Request.Form.Get("TaskStatus");
        string reason = Request.Form.Get("Reason");
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            SqlCommand cmmd = new SqlCommand("sp_UpdateTaskStatus");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@BILLID", SqlDbType.NVarChar, -1);
            cmmd.Parameters["@BILLID"].Value = bILLID;
            cmmd.Parameters.Add("@TaskStatus", SqlDbType.Int);
            cmmd.Parameters["@TaskStatus"].Value =  Convert.ToInt32(taskStatus);
            cmmd.Parameters.Add("@NoAcceptTaskReason", SqlDbType.NVarChar, 500);
            cmmd.Parameters["@NoAcceptTaskReason"].Value = reason;
            cmmd.Parameters.Add("@USERID", SqlDbType.Int);
            cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

            DB.ExecuteNonQuery(cmmd);
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 保存任务
    /// </summary>
    public void SaveMainTask()
    {
        string jsonData = Request.Form.Get("Data");
        string lineTask = Request.Form.Get("LineTask");
        string shipInfo = Request.Form.Get("ShipInfo");

        Hashtable row = new Hashtable();
        if (!string.IsNullOrEmpty(jsonData))
        {
            row = (Hashtable)VLP.JSON.Decode(jsonData);
        }

        Hashtable shipRow = new Hashtable();
        if (!string.IsNullOrEmpty(shipInfo))
        {
            shipRow = (Hashtable)VLP.JSON.Decode(shipInfo);
        }

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            SqlCommand cmmd = new SqlCommand("sp_SaveMainTask");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@TaskSource", SqlDbType.Int);
            cmmd.Parameters["@TaskSource"].Value = (row["TaskSource"] == null || row["TaskSource"].ToString() == "") ? DBNull.Value : row["TaskSource"];

            cmmd.Parameters.Add("@TaskBTime", SqlDbType.SmallDateTime);
            cmmd.Parameters["@TaskBTime"].Value = row["TaskBTime"] == null || row["TaskBTime"].ToString() == "" ? DBNull.Value : row["TaskBTime"];
            cmmd.Parameters.Add("@TaskETime", SqlDbType.SmallDateTime);
            cmmd.Parameters["@TaskETime"].Value = row["TaskETime"] == null || row["TaskETime"].ToString() == "" ? DBNull.Value : row["TaskETime"];

            cmmd.Parameters.Add("@FlyWay", SqlDbType.Int);
            cmmd.Parameters["@FlyWay"].Value = (row["FlyWay"] == null || row["FlyWay"].ToString() == "") ? DBNull.Value : row["FlyWay"];

            cmmd.Parameters.Add("@LineTask", SqlDbType.NVarChar, -1);
            cmmd.Parameters["@LineTask"].Value = lineTask;

            cmmd.Parameters.Add("@TaskDescription", SqlDbType.NVarChar, 500);
            cmmd.Parameters["@TaskDescription"].Value = (row["TaskDescription"] == null || row["TaskDescription"].ToString() == "") ? "" : row["TaskDescription"].ToString();

            cmmd.Parameters.Add("@ShipId", SqlDbType.NVarChar, 50);
            cmmd.Parameters["@ShipId"].Value = (row["FlyWay"] != null && row["FlyWay"].ToString() == "0") ? shipRow["shipId"] : "";

            //航线高度：如果为null 默认给 140m
            cmmd.Parameters.Add("@LineHeight", SqlDbType.Int);
            cmmd.Parameters["@LineHeight"].Value = (row["LineHeight"] == null || row["LineHeight"].ToString() == "") ? 140 : row["LineHeight"];

            cmmd.Parameters.Add("@USERID", SqlDbType.Int);
            cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;
            
            DataTable dt = DB.ExecuteDataTable(cmmd);
            //跟踪任务，保存船舶信息
            if (row["FlyWay"] != null && row["FlyWay"].ToString().Equals(_FlyWay))
            {
                if (dt != null && dt.Rows.Count >= 1 && dt.Rows[0][0] != null)
                {
                    //保存船舶信息，用于测试任务中回显船舶
                    string taskId = dt.Rows[0][0].ToString();
                    saveShipMainTask(shipRow, taskId, 0);
                }
            }
            
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 保存跟踪的船舶信息
    /// </summary>
    /// <param name="shipRow">船舶信息</param>
    /// <param name="taskId">任务ID</param>
    /// <param name="trackType">0为任务界面创建的跟踪任务 1：控制界面新的船舶目标跟踪</param>
    private void saveShipMainTask(Hashtable shipRow, string taskId, int trackType)
    {
        SqlCommand cmmd = new SqlCommand("sp_SaveShipMainTask");
        cmmd.CommandType = CommandType.StoredProcedure;
        cmmd.Parameters.Add("@TaskId", SqlDbType.Int);
        cmmd.Parameters["@TaskId"].Value = Convert.ToInt32(taskId);

        cmmd.Parameters.Add("@IMO", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@IMO"].Value = (shipRow["IMO"] == null || shipRow["IMO"].ToString() == "") ? "" : shipRow["IMO"];

        cmmd.Parameters.Add("@MMSI", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@MMSI"].Value = (shipRow["MMSI"] == null || shipRow["MMSI"].ToString() == "") ? "" : shipRow["MMSI"];

        cmmd.Parameters.Add("@beam", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@beam"].Value = (shipRow["beam"] == null || shipRow["beam"].ToString() == "") ? "" : shipRow["beam"];

        cmmd.Parameters.Add("@callsign", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@callsign"].Value = (shipRow["callsign"] == null || shipRow["callsign"].ToString() == "") ? "" : shipRow["callsign"];

        cmmd.Parameters.Add("@cargoType", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@cargoType"].Value = (shipRow["cargoType"] == null || shipRow["cargoType"].ToString() == "") ? "" : shipRow["cargoType"];

        cmmd.Parameters.Add("@country", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@country"].Value = (shipRow["country"] == null || shipRow["country"].ToString() == "") ? "" : shipRow["country"];

        cmmd.Parameters.Add("@course", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@course"].Value = (shipRow["course"] == null || shipRow["course"].ToString() == "") ? "" : shipRow["course"];

        cmmd.Parameters.Add("@dest", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@dest"].Value = (shipRow["dest"] == null || shipRow["dest"].ToString() == "") ? "" : shipRow["dest"];

        cmmd.Parameters.Add("@draught", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@draught"].Value = (shipRow["draught"] == null || shipRow["draught"].ToString() == "") ? "" : shipRow["draught"];

        cmmd.Parameters.Add("@eta", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@eta"].Value = (shipRow["eta"] == null || shipRow["eta"].ToString() == "") ? "" : shipRow["eta"];

        cmmd.Parameters.Add("@heading", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@heading"].Value = (shipRow["heading"] == null || shipRow["heading"].ToString() == "") ? "" : shipRow["heading"];

        cmmd.Parameters.Add("@lastTime", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@lastTime"].Value = (shipRow["lastTime"] == null || shipRow["lastTime"].ToString() == "") ? "" : shipRow["lastTime"];

        cmmd.Parameters.Add("@lat", SqlDbType.Decimal);
        cmmd.Parameters["@lat"].Value = (shipRow["lat"] == null) ? DBNull.Value : shipRow["lat"];

        cmmd.Parameters.Add("@left", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@left"].Value = (shipRow["left"] == null || shipRow["left"].ToString() == "") ? "" : shipRow["left"];

        cmmd.Parameters.Add("@length", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@length"].Value = (shipRow["length"] == null || shipRow["length"].ToString() == "") ? "" : shipRow["length"];

        cmmd.Parameters.Add("@lng", SqlDbType.Decimal);
        cmmd.Parameters["@lng"].Value = (shipRow["lng"] == null) ? DBNull.Value : shipRow["lng"];

        cmmd.Parameters.Add("@name", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@name"].Value = (shipRow["name"] == null || shipRow["name"].ToString() == "") ? "" : shipRow["name"];

        cmmd.Parameters.Add("@rot", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@rot"].Value = (shipRow["rot"] == null || shipRow["rot"].ToString() == "") ? "" : shipRow["rot"];

        cmmd.Parameters.Add("@shipId", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@shipId"].Value = (shipRow["shipId"] == null || shipRow["shipId"].ToString() == "") ? "" : shipRow["shipId"];

        cmmd.Parameters.Add("@speed", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@speed"].Value = (shipRow["speed"] == null || shipRow["speed"].ToString() == "") ? "" : shipRow["speed"];

        cmmd.Parameters.Add("@status", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@status"].Value = (shipRow["status"] == null || shipRow["status"].ToString() == "") ? "" : shipRow["status"];

        cmmd.Parameters.Add("@trail", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@trail"].Value = (shipRow["trail"] == null || shipRow["trail"].ToString() == "") ? "" : shipRow["trail"];

        cmmd.Parameters.Add("@type", SqlDbType.NVarChar, 50);
        cmmd.Parameters["@type"].Value = (shipRow["type"] == null || shipRow["type"].ToString() == "") ? "" : shipRow["type"];

        cmmd.Parameters.Add("@TrackType", SqlDbType.Int);
        cmmd.Parameters["@TrackType"].Value = trackType;

        cmmd.Parameters.Add("@USERID", SqlDbType.Int);
        cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

        DB.ExecuteNonQuery(cmmd);
    }

    /// <summary>
    /// 取基站经纬度 
    /// </summary>
    public void GetBaseStationInfo()
    {
        string baseStationID = Request["BaseStationID"];
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            SqlCommand cmmd = new SqlCommand("SELECT Latitude,Longitude FROM dbo.D_Uav_BaseStation WHERE ID =  @ID");
            cmmd.Parameters.Add("@ID", SqlDbType.Int);
            cmmd.Parameters["@ID"].Value = Convert.ToInt32(baseStationID);
            result.DataList = DB.ExecuteDataTable(cmmd);
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 取出航道经纬度坐标图
    /// </summary>
    public void GetLaneLatLng()
    {
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            SqlCommand cmmd = new SqlCommand("sp_GetD_Uav_EnabledLatLng");
            cmmd.CommandType = CommandType.StoredProcedure;
            DataSet ds = DB.ExecuteDataset(cmmd);
            if (ds != null)
            {
                result.DataListLat = ds.Tables[0];//纬度
                result.DataListLng = ds.Tables[1];//经度
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 保存区域坐标
    /// </summary>
    public void SaveLaneLatLngArray()
    {
        string json = Request.Form.Get("data");
        string code = Request.Form.Get("code");

        ArrayList rows = (ArrayList)VLP.JSON.Decode(json);
        if (rows == null || rows.Count <= 0)
        {
            return;
        }
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            Collection<SqlCommand> cmmds = new Collection<SqlCommand>();

            foreach (Hashtable row in rows)
            {
                cmmds.Add(SaveLaneLatLngArrayInfoCmmd(row, code));
            }
            string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
            if (!string.IsNullOrEmpty(res))
            {
                result.IsOK = false;
                result.ErrMessage = res;
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));

    }

    private SqlCommand SaveLaneLatLngArrayInfoCmmd(Hashtable row, string code)
    {
        SqlCommand cmmd = new SqlCommand("sp_SaveLaneLatLngArrayInfoCmmd");
        cmmd.CommandType = CommandType.StoredProcedure;
        
        cmmd.Parameters.Add("@Code", SqlDbType.NVarChar, 20);
        cmmd.Parameters["@Code"].Value = code;

        cmmd.Parameters.Add("@Lat", SqlDbType.Decimal);
        cmmd.Parameters["@Lat"].Value = row["lat"];

        cmmd.Parameters.Add("@Lng", SqlDbType.Decimal);
        cmmd.Parameters["@Lng"].Value = row["lng"];

        return cmmd;
    }

    /// <summary>
    /// 航前检查
    /// </summary>
    public void onFlyCheckFlag()
    {
        string bILLID = Request.Form.Get("BILLID");
        string arrFlag = Request.Form.Get("Data");

        string[] checkArray = null;
        if (!string.IsNullOrEmpty(arrFlag))
        {
            checkArray = arrFlag.Split(',');
        }

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            if (checkArray != null)
            {
                Collection<SqlCommand> cmmds = new Collection<SqlCommand>();
                for (int i = 0; i < checkArray.Length; i++)
                {
                    cmmds.Add(onFlyCheckFlagCmmd(checkArray[i], bILLID, arrFlag));
                }

                string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
                if (!string.IsNullOrEmpty(res))
                {
                    result.IsOK = false;
                    result.ErrMessage = res;
                }
            }
            else
            {
                result.IsOK = false;
                result.ErrMessage = "航前检查错误,没有需要检查项";
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="row"></param>
    /// <param name="bILLID"></param>
    /// <returns></returns>
    private SqlCommand onFlyCheckFlagCmmd(string flagId, string bILLID, string arrFlag)
    {
        SqlCommand cmmd = new SqlCommand("sp_onFlyCheckFlagCmmd");
        cmmd.CommandType = CommandType.StoredProcedure;

        cmmd.Parameters.Add("@BILLID", SqlDbType.Int);
        cmmd.Parameters["@BILLID"].Value = bILLID;

        cmmd.Parameters.Add("@FlagId", SqlDbType.Int);
        cmmd.Parameters["@FlagId"].Value = flagId;

        cmmd.Parameters.Add("@ArrFlag", SqlDbType.NVarChar, 200);
        cmmd.Parameters["@ArrFlag"].Value = arrFlag;

        cmmd.Parameters.Add("@USERID", SqlDbType.Int);
        cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

        return cmmd;
    }

    
    /// <summary>
    /// 添加点，验证是否在可点区域内
    /// </summary>
    public void CheckOptionsLatLng()
    {
        string lat = Request.Form.Get("Lat");
        string lng = Request.Form.Get("Lng");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        //查找指定的区域块
        try
        {
            //1.验证点击的，是否在指定划分的某个模块中
            SqlCommand cmmd = new SqlCommand("SELECT TOP 1 Code FROM dbo.D_Uav_EnabledLatLng WHERE  @Lng>=LeftLng AND @Lat>=DownLat  ORDER BY LeftLng DESC,DownLat DESC");
            cmmd.Parameters.Add("@Lat", SqlDbType.Decimal);
            cmmd.Parameters["@Lat"].Value = lat;
            cmmd.Parameters.Add("@Lng", SqlDbType.Decimal);
            cmmd.Parameters["@Lng"].Value = lng;
            DataTable dtModule = DB.ExecuteDataTable(cmmd);
            
            DataTable dtArea = null;
            Boolean res = false;
            string code = "";
            //2.验证点击的模块,是否允许被点击
            if (dtModule != null && dtModule.Rows.Count > 0)
            {
                code = dtModule.Rows[0]["Code"].ToString();
                SqlCommand cmmdArea = new SqlCommand("SELECT Code FROM dbo.D_Uav_EnabledLatLng WHERE IsEnabled = 1 AND Code=@Code ");
                cmmdArea.Parameters.Add("@Code", SqlDbType.NVarChar, 20);
                cmmdArea.Parameters["@Code"].Value = code;
                dtArea = DB.ExecuteDataTable(cmmdArea);
                //3.模块为可点击，验证是否点击的是模块中的指定区域
                if (dtArea != null && dtArea.Rows.Count > 0)
                {
                    res = CheckOptionsArea(code, lat, lng);
                }
            }
            if (dtModule == null || dtModule.Rows.Count <= 0 || dtArea == null || dtArea.Rows.Count <= 0 || !res)
            {
                result.IsOK = false;
                result.ErrMessage = "不是可添加的点坐标区域，请重新点击添加！";//+ code 可现实具体点击的模块
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 验证点明细区域，是否可以点击
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    private bool CheckOptionsArea(string code, string lat, string lng)
    {
        try
        {
            SqlCommand cmmd = new SqlCommand(@"SELECT
                (SELECT COUNT(1) FROM D_Uav_EnabledLatLng_Dtl WHERE Code = @Code) AS Num,
                (SELECT MAX(Lat) FROM D_Uav_EnabledLatLng_Dtl WHERE Code = @Code) AS maxLat,
                (SELECT MAX(Lng) FROM D_Uav_EnabledLatLng_Dtl WHERE Code = @Code) AS maxLng,
                (SELECT MIN(Lat) FROM D_Uav_EnabledLatLng_Dtl WHERE Code = @Code) AS minLat,
                (SELECT MIN(Lng) FROM D_Uav_EnabledLatLng_Dtl WHERE Code = @Code) AS minLng");
            cmmd.Parameters.Add("@Code", SqlDbType.NVarChar);
            cmmd.Parameters["@Code"].Value = code;
            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt == null || dt.Rows.Count <=0)
            {
                return false; 
            }
            int num = int.Parse(dt.Rows[0]["Num"].ToString());
            if (num == 0)
            {
                return false; 
            }
            decimal maxLat = decimal.Parse(dt.Rows[0]["maxLat"].ToString());
            decimal maxLng = decimal.Parse(dt.Rows[0]["maxLng"].ToString());
            decimal minLat = decimal.Parse(dt.Rows[0]["minLat"].ToString());
            decimal minLng = decimal.Parse(dt.Rows[0]["minLng"].ToString());
            //验证一
            decimal dLat =decimal.Parse(lat); 
            decimal dLng =decimal.Parse(lng); 
            if (dLat < minLat || dLat >maxLat || dLng < minLng || dLng > maxLng)
            {
                return false; 
            }

            SqlCommand cmmdL = new SqlCommand(@"SELECT Lat,Lng FROM D_Uav_EnabledLatLng_Dtl WHERE Code = @Code ORDER BY ID");
            cmmdL.Parameters.Add("@Code", SqlDbType.NVarChar);
            cmmdL.Parameters["@Code"].Value = code;
            DataTable dtL = DB.ExecuteDataTable(cmmdL);

            decimal[] vertx = new decimal[dtL.Rows.Count];
            decimal[] verty = new decimal[dtL.Rows.Count];

            for (int i = 0; i < dtL.Rows.Count; i++)
            {
                vertx[i] = decimal.Parse(dtL.Rows[i]["Lat"].ToString());
                verty[i] = decimal.Parse(dtL.Rows[i]["Lng"].ToString()); 
            }
            //验证二
            int j = 0, cnt = 0;
            for (int i = 0; i < vertx.Length; i++)
            {
                j = (i == vertx.Length - 1) ? 0 : j + 1;
                if ((verty[i] != verty[j])
                    && (((dLng >= verty[i])&& (dLng < verty[j])) || ((dLng >= verty[j]) && (dLng < verty[i])))
                    && (dLat < (vertx[j] - vertx[i]) * (dLng - verty[i]) / (verty[j] - verty[i]) + vertx[i])) 
                    cnt++;
            }
            return (cnt % 2 > 0) ? true : false;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// 补点动作
    /// </summary>
    public void GetLinepointMiddle()
    {
        string jd1 = Request["jd1"];
        string wd1 = Request["wd1"];
        string jd2 = Request["jd2"];
        string wd2 = Request["wd2"];

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            SqlCommand cmmd = new SqlCommand("sp_Uav_GetLinepointMiddle");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@jd1", SqlDbType.Decimal);
            cmmd.Parameters["@jd1"].Value = Convert.ToDecimal(jd1);

            cmmd.Parameters.Add("@wd1", SqlDbType.Decimal);
            cmmd.Parameters["@wd1"].Value = Convert.ToDecimal(wd1);

            cmmd.Parameters.Add("@jd2", SqlDbType.Decimal);
            cmmd.Parameters["@jd2"].Value = Convert.ToDecimal(jd2);

            cmmd.Parameters.Add("@wd2", SqlDbType.Decimal);
            cmmd.Parameters["@wd2"].Value = Convert.ToDecimal(wd2);

            result.DataList = DB.ExecuteDataTable(cmmd);
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result)); 
    }

    /// <summary>
    /// Name_E:区域点开关 0：关闭开关(关闭点限制)  1：打开开关(开启点限制)
    /// centerCode :1 岳阳 2 上海 
    /// V_AreaPointCheck ：只查询一条有效的数据
    /// </summary>
    public void AreaPointCheck()
    {
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true; 
        try
        {
            SqlCommand cmmd = new SqlCommand("SELECT Name_E, centerCode, lat, lng, shipLat, shipLng,baseStationLat,baseStationLng  FROM V_AreaPointCheck;");
            DataTable dt = DB.ExecuteDataTable(cmmd);
            if (dt != null && dt.Rows.Count > 0)
            {
                string flag = dt.Rows[0]["Name_E"].ToString();
                if (flag.Equals("1"))
                {
                    result.IsOK = true;
                }
                else
                {
                    result.IsOK = false;
                }
                //--海图中心点坐标 1：岳阳 2:上海
                if (dt.Rows[0]["centerCode"] != null)
                {
                    result.centerCode = dt.Rows[0]["centerCode"].ToString();
                }

                //判断当前用户是否设置了海图 中心坐标点
                SqlCommand cmmdUser = new SqlCommand(string.Format("SELECT Lat, Lng, UserId FROM D_SeaCenterForUser WHERE UserId = {0};", currentUserInfo.ID));
                DataTable dtUser = DB.ExecuteDataTable(cmmdUser);
                if (dtUser != null && dtUser.Rows.Count >= 1)
                {
                    result.lat = dtUser.Rows[0]["Lat"].ToString();
                    result.lng = dtUser.Rows[0]["Lng"].ToString();
                }
                else
                { 
                    //默认加载 配置中心点坐标 经纬度
                    if (dt.Rows[0]["lat"] != null)
                    {
                        result.lat = dt.Rows[0]["lat"].ToString();
                    }
                    if (dt.Rows[0]["lng"] != null)
                    {
                        result.lng = dt.Rows[0]["lng"].ToString();
                    }
                }
                //上海 测试船经纬度
                if (dt.Rows[0]["shipLat"] != null)
                {
                    result.shipLat = dt.Rows[0]["shipLat"].ToString();
                }
                if (dt.Rows[0]["shipLng"] != null)
                {
                    result.shipLng = dt.Rows[0]["shipLng"].ToString();
                }

                //基站经纬度
                if (dt.Rows[0]["baseStationLat"] != null)
                {
                    result.baseStationLat = dt.Rows[0]["baseStationLat"].ToString();
                }
                if (dt.Rows[0]["baseStationLng"] != null)
                {
                    result.baseStationLng = dt.Rows[0]["baseStationLng"].ToString();
                }  
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result)); 
        
    }

    /// <summary>
    /// 保存航线航点信息
    /// TaskID:任务ID
    /// UavID:飞机ID
    /// </summary>
    public void SetD_Uav_MainTaskLine()
    {
        string TaskID = Request["TaskID"];
        string UavID = Request["UavID"];
        string json = Request.Form.Get("data");

        ArrayList rows = (ArrayList)VLP.JSON.Decode(json);
        if (rows == null || rows.Count <= 0)
        {
            return;
        }
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            Collection<SqlCommand> cmmds = new Collection<SqlCommand>();
            foreach (Hashtable row in rows)
            {
                String id = row["ID"] != null ? row["ID"].ToString() : "";
                //根据记录状态，进行不同的增加、修改操作
                String state = row["_state"] != null ? row["_state"].ToString() : "";
                if (state == "added" || id == "" || id == "0")           //新增：id为空，或_state为added
                {
                    cmmds.Add(UpdateD_Uav_FlyLineRecordCmmd(row, TaskID, UavID, 0));
                }
                else if (state == "modified" || state == "") //更新：_state为空或modified
                {
                    cmmds.Add(UpdateD_Uav_FlyLineRecordCmmd(row, TaskID, UavID, 1));
                }
                else if (state == "removed" || state == "deleted")
                {
                    cmmds.Add(UpdateD_Uav_FlyLineRecordCmmd(row, TaskID, UavID, 2));
                }
            }
            string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
            if (!string.IsNullOrEmpty(res))
            {
                result.IsOK = false;
                result.ErrMessage = res;
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 保存航线 航点信息
    /// </summary>
    /// <param name="row"></param>
    /// <param name="TaskID"></param>
    /// <param name="UavID"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    private SqlCommand UpdateD_Uav_FlyLineRecordCmmd(Hashtable row, string TaskID, string UavID, int Flag)
    {
        SqlCommand cmmd = new SqlCommand("sp_UpdateD_Uav_FlyLineRecord");
        cmmd.CommandType = CommandType.StoredProcedure;
        cmmd.Parameters.Add("@ID", SqlDbType.Int);
        if (Flag == 0)
        {
            cmmd.Parameters["@ID"].Value = "0";
        }
        else
        {
            cmmd.Parameters["@ID"].Value = row["ID"];
        }

        cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(TaskID);

        cmmd.Parameters.Add("@UavID", SqlDbType.Int);
        cmmd.Parameters["@UavID"].Value = Convert.ToInt32(UavID);

        cmmd.Parameters.Add("@LineCode", SqlDbType.Int);
        cmmd.Parameters["@LineCode"].Value = row["LineCode"];

        cmmd.Parameters.Add("@Sortid", SqlDbType.Int);
        cmmd.Parameters["@Sortid"].Value = row["Sortid"];

        cmmd.Parameters.Add("@Jd", SqlDbType.Decimal);
        cmmd.Parameters["@Jd"].Value = row["Jd"];

        cmmd.Parameters.Add("@Wd", SqlDbType.Decimal);
        cmmd.Parameters["@Wd"].Value = row["Wd"];

        cmmd.Parameters.Add("@Height_G", SqlDbType.Int);
        cmmd.Parameters["@Height_G"].Value = row["Height_G"];

        cmmd.Parameters.Add("@USERID", SqlDbType.Int);
        cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

        cmmd.Parameters.Add("@Flag", SqlDbType.Int);
        cmmd.Parameters["@Flag"].Value = Flag;

        return cmmd;
    }

    /// <summary>
    /// 方法重构 ：保存航线 航点信息
    /// </summary>
    /// <param name="row"></param>
    /// <param name="TaskID"></param>
    /// <param name="UavID"></param>
    /// <param name="Flag"></param>
    /// <returns></returns>
    private SqlCommand UpdateD_Uav_FlyLineRecordCmmd(DataRow row, string TaskID, string UavID, int Flag)
    {
        SqlCommand cmmd = new SqlCommand("sp_UpdateD_Uav_FlyLineRecord");
        cmmd.CommandType = CommandType.StoredProcedure;
        cmmd.Parameters.Add("@ID", SqlDbType.Int);
        if (Flag == 0)
        {
            cmmd.Parameters["@ID"].Value = "0";
        }
        else
        {
            cmmd.Parameters["@ID"].Value = row["ID"];
        }

        cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(TaskID);

        cmmd.Parameters.Add("@UavID", SqlDbType.Int);
        cmmd.Parameters["@UavID"].Value = Convert.ToInt32(UavID);

        cmmd.Parameters.Add("@LineCode", SqlDbType.Int);
        cmmd.Parameters["@LineCode"].Value = row["航线编码"];

        cmmd.Parameters.Add("@Sortid", SqlDbType.Int);
        cmmd.Parameters["@Sortid"].Value = row["航点序号"];

        cmmd.Parameters.Add("@Jd", SqlDbType.Decimal);
        cmmd.Parameters["@Jd"].Value = row["经度"];

        cmmd.Parameters.Add("@Wd", SqlDbType.Decimal);
        cmmd.Parameters["@Wd"].Value = row["纬度"];

        cmmd.Parameters.Add("@Height_G", SqlDbType.Int);
        cmmd.Parameters["@Height_G"].Value = row["高度"];

        cmmd.Parameters.Add("@USERID", SqlDbType.Int);
        cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

        cmmd.Parameters.Add("@Flag", SqlDbType.Int);
        cmmd.Parameters["@Flag"].Value = Flag;

        return cmmd;
    }

    /// <summary>
    /// 导入航线航点
    /// </summary>
    public void ImportD_Uav_FlyLineRecord()
    {
        string TaskID = Request["TaskID"];
        string UavID = Request["UavID"];
        string FileDir = Request["FileDir"];

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            string res = ImportD_Uav_FlyLineRecordExcel(FileDir, TaskID, UavID);
            if (!string.IsNullOrEmpty(res))
            {
                result.IsOK = false;
                result.ErrMessage = res;
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = "读取导入的文件时出错，请核实文件！" + ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }
    /// <summary>
    /// 读取 Excel 数据
    /// </summary>
    /// <param name="FileDir"></param>
    /// <param name="TaskID"></param>
    /// <param name="UavID"></param>
    /// <returns></returns>
    private string ImportD_Uav_FlyLineRecordExcel(string fileDir, string TaskID, string UavID)
    {
        Collection<SqlCommand> cmmds = new Collection<SqlCommand>();
        DataTable dtFile = null;

        using (FileStream fileStream = new FileStream(fileDir, FileMode.Open))
        {
            ExcelLibrary.SpreadSheet.Workbook book = Workbook.Load(fileStream);
            Worksheet worksheet = book.Worksheets[0];
            dtFile = new DataTable();
            if (worksheet.Cells.LastRowIndex < 1)
            {
                return "文件不对";
            }

            for (int j = 0; j <= worksheet.Cells.LastColIndex; j++)
            {
                string cname = worksheet.Cells[0, j].Value.ToString().Trim();
                if (string.IsNullOrEmpty(cname))
                {
                    break;
                }
                else
                {
                    if (!cname.Equals("航线编码") && !cname.Equals("航点序号") &&
                        !cname.Equals("经度") && !cname.Equals("纬度") && !cname.Equals("高度"))
                    {
                        return "列名称不对,请检查";
                    }
                    dtFile.Columns.Add(cname);
                }
            }

            for (int i = 1; i <= worksheet.Cells.LastRowIndex; i++)//从第一行读取数据
            {
                DataRow row = dtFile.NewRow();
                for (int j = 0; j <= worksheet.Cells.LastColIndex; j++)
                {
                    row[j] = worksheet.Cells[i, j].Value == null ? DBNull.Value : worksheet.Cells[i, j].Value;
                }
                dtFile.Rows.Add(row);
            }
        }
        try
        {
            if (File.Exists(fileDir))
            {
                File.Delete(fileDir);
            }
        }
        catch { }

        if (dtFile.Rows.Count <= 0)
        {
            return "未发现需要导入的数据";
        }

        foreach (DataRow dr in dtFile.Rows)
        {
            cmmds.Add(UpdateD_Uav_FlyLineRecordCmmd(dr, TaskID, UavID, 0));//0 代表导入新增
        }
        if (cmmds.Count > 0)
        {
            return DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
        }
        return "未发现需要导入的数据";
    }

    /// <summary>
    /// 下载导入模板
    /// </summary>
    public void DownLoadInExcel()
    {
        string feeUrl = Server.MapPath("/kindeditor/attached/导入航线模板");
        if (Directory.Exists(feeUrl) == false)
        {
            Directory.CreateDirectory(feeUrl);
        }

        List<string> urlList = new List<string>();
        string fileUrl = string.Format("kindeditor\\attached\\导入航线模板\\");
        foreach (string file in Directory.GetFiles(feeUrl))
        {
            urlList.Add(file);
        }

        //直接下载
        if (urlList.Count > 0)
        {
            MemoryStream ms = new MemoryStream();
            byte[] buffer = null;
            using (ZipFile file = ZipFile.Create(ms))
            {
                file.BeginUpdate();
                file.NameTransform = new MyNameTransfom_R();//通过这个名称格式化器，可以将里面的文件名进行一些处理。默认情况下，会自动根据文件的路径在zip中创建有关的文件夹。
                foreach (string url in urlList)
                {
                    file.Add(url);
                }
                file.CommitUpdate();

                buffer = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buffer, 0, buffer.Length);
            }

            //输出帐单资料
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("content-disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode("导入航线模板下载") + ".zip");
            Response.BinaryWrite(buffer);
            Response.Flush();
            Response.End();
        }
    }

    /// <summary>
    /// 批量设置 航线航点高度
    /// </summary>
    public void UpdateUav_FlyLineRecordHeight()
    {
        string IDs = Request["ID"];
        string TaskID = Request["TaskID"];
        string height = Request["Height"];

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            SqlCommand cmmd = new SqlCommand("sp_UpdateUav_FlyLineRecordHeight");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@ID", SqlDbType.NVarChar, -1);
            cmmd.Parameters["@ID"].Value = IDs;
            cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
            cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(TaskID);
            cmmd.Parameters.Add("@Height", SqlDbType.Int);
            cmmd.Parameters["@Height"].Value = Convert.ToInt32(height);
            cmmd.Parameters.Add("@USERID", SqlDbType.Int);
            cmmd.Parameters["@USERID"].Value = currentUserInfo.ID;

            DB.ExecuteNonQuery(cmmd);
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 从新上传航线
    /// </summary>
    public void UploadLineRecord()
    {
        string TaskID = Request["TaskID"];
        string UavID = Request["UavID"];

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        try
        {
            //调用飞机设置航线接口
            callNav_Set_Line(TaskID, UavID, result, msg);
            if (!string.IsNullOrEmpty(msg))
            {
                result.IsOK = false;
                result.ErrMessage = msg;
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 生成跟踪目标航线
    /// </summary>
    public void onCreateTrackLine()
    {
        string lat = Request.Form.Get("lat");
        string lng = Request.Form.Get("lng");
        string taskId = Request.Form.Get("TaskId");
        string uavID = Request.Form.Get("UavID");
        string lineHeight = Request.Form.Get("LineHeight");
        string shipInfo = Request.Form.Get("ShipInfo");

        Hashtable shipRow = new Hashtable();
        if (!string.IsNullOrEmpty(shipInfo))
        {
            shipRow = (Hashtable)VLP.JSON.Decode(shipInfo);
        }

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        int lineCode = 7;//跟踪目标航线编码 默认给 7

        try
        {
            NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskId));
            //1.获取飞机当前最新 经纬度坐标
            //2.根据 跟踪目标经纬度点，生成跟踪航线.需要补点
            //3.切换 到跟踪目标航线
            //4.回显 跟踪航线 在地图上
            DataTable dt_YaoCe = nav.Nav_Get_FlySignals(true, out msg);//获取最新的遥测信号
            if (dt_YaoCe == null || dt_YaoCe.Rows.Count <= 0)
            {
                result.IsOK = false;
                result.ErrMessage = "没有获取到最新的飞机遥测信号,生成跟踪航线失败,操作已返回！";
            }
            else if (dt_YaoCe.Rows[0]["CurrentLine"].ToString() == "0" || dt_YaoCe.Rows[0]["CurrentLine"].ToString() == "8")
            {
                result.IsOK = false;
                result.ErrMessage = "飞机目前处于返航/降落航线,不允许添加新的跟踪目标,操作已返回！";
            }
            else
            {
                //判断飞机与船舶距离，小于300M直接发送目标盘旋命令
                string Jd1 = dt_YaoCe.Rows[0]["Loction_Longitude"].ToString();
                string Wd1 = dt_YaoCe.Rows[0]["Loction_Latitude"].ToString();

                double targetDistanceM = Common.GetDistance(Convert.ToDouble(Wd1), Convert.ToDouble(Jd1), Convert.ToDouble(lat), Convert.ToDouble(lng));
                if (targetDistanceM <= 300)
                {
                    NavCycleobj obj = new NavCycleobj();
                    obj.CycleNum = Convert.ToByte(3);
                    obj.Longitude = Convert.ToInt64(Convert.ToDecimal(lng) * 1000000);
                    obj.Latitude = Convert.ToInt64(Convert.ToDecimal(lat) * 1000000);
                    obj.Cycleradius = 200;
                    obj.Cycletype = 0;//盘旋跟踪某一个目标
                    obj.Boatincremental = 0; //度数
                    obj.Boatdistnct = 0;//暂时不知道如何使用
                    if (obj.Longitude == 0 || obj.Latitude == 0)
                    {
                        result.IsOK = false;
                        result.ErrMessage = "请确认盘旋中心点坐标正确.";
                    }

                    result.IsOK = nav.Nav_Change_Line(obj, out msg);

                    //保存操作动作
                    SqlCommand cmmdOrder = Common.saveControlTypeLog(taskId, uavID, "3:首次时时更新目标船舶盘旋点.经度[" + lng + "]纬度[" + lat + "]", null, result.IsOK, result.ErrMessage, 1, currentUserInfo.ID);
                    DB.ExecuteNonQuery(cmmdOrder);
                }
                else
                {
                    //1.需要删除原先新的已跟踪的船舶信息 TrackType = 1 参数标识
                    //2.保存新跟踪的 船舶信息
                    //3.原先的跟踪航线删除掉
                    saveShipMainTask(shipRow, taskId, 1);

                    //新跟踪航线高度
                    int heightG = Convert.ToInt32(lineHeight);

                    Collection<SqlCommand> cmmds = new Collection<SqlCommand>();

                    DataTable dtPoint = GetLinepointMiddle(Jd1, Wd1, lng, lat);
                    if (dtPoint == null || dtPoint.Rows.Count <= 0)//没有补的点。就直接添加
                    {
                        cmmds.Add(saveLineInfoCmmd(Jd1, Wd1, taskId, uavID, 1, lineCode, heightG));
                        cmmds.Add(saveLineInfoCmmd(lng, lat, taskId, uavID, 2, lineCode, heightG));
                    }
                    else
                    {
                        for (int i = 0; i <= dtPoint.Rows.Count; i++)
                        {
                            if (i == 0)//第一段航线的 第一个点
                            {
                                cmmds.Add(saveLineInfoCmmd(Jd1, Wd1, taskId, uavID, 1, lineCode, heightG));
                            }
                            else if (i == dtPoint.Rows.Count)//第一段航线的末尾点
                            {
                                cmmds.Add(saveLineInfoCmmd(lng, lat, taskId, uavID, (dtPoint.Rows.Count + 1), lineCode, heightG));
                            }
                            else
                            {
                                cmmds.Add(saveLineInfoCmmd(dtPoint.Rows[i]["Jd"].ToString(), dtPoint.Rows[i]["Wd"].ToString(), taskId, uavID, i + 1, lineCode, heightG));
                            }
                        }
                    }

                    //增加新的跟踪航线
                    string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
                    if (!string.IsNullOrEmpty(res))
                    {
                        result.IsOK = false;
                        result.ErrMessage = res;
                    }
                    else
                    {
                        //调用飞机设置跟踪航线接口,指定默认航线编码 上传
                        callNav_Set_Line(taskId, uavID, lineCode, result, msg);

                        //切换到跟踪目标航线
                        result.IsOK = nav.Nav_Change_Line(Convert.ToByte(lineCode), Convert.ToInt32(1), out msg);

                        //取出跟踪航线
                        result.DataList = getTrackLine(taskId, lineCode);

                        //保存操作动作
                        SqlCommand cmmdOrder = Common.saveControlTypeLog(taskId, uavID, "2:首次时时更新:[新目标航线][新目标轨迹]", null, result.IsOK, result.ErrMessage, 1, currentUserInfo.ID);
                        DB.ExecuteNonQuery(cmmdOrder);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }
    
    /// <summary>
    /// 取出跟踪航线 
    /// </summary>
    /// <returns></returns>
    private DataTable getTrackLine(string taskId, int lineCode) 
    {
        SqlCommand cmmd = new SqlCommand(@"SELECT Jd,Wd FROM dbo.D_Uav_FlyLineRecord
            WHERE TaskID = @TaskID  AND LineCode = @LineCode ORDER BY Sortid;");

        cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskId);
        cmmd.Parameters.Add("@LineCode", SqlDbType.Int);
        cmmd.Parameters["@LineCode"].Value = lineCode;
        return DB.ExecuteDataTable(cmmd);
    }

    /// <summary>
    /// 删除原先的跟踪航线
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="lineCode"></param>
    private void delTrackLine(string taskId, int lineCode)
    {
        SqlCommand cmmd = new SqlCommand(@"DELETE FROM dbo.D_Uav_FlyLineRecord WHERE TaskID = @TaskID  AND LineCode = @LineCode;");
        cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskId);
        cmmd.Parameters.Add("@LineCode", SqlDbType.Int);
        cmmd.Parameters["@LineCode"].Value = lineCode;
        DB.ExecuteNonQuery(cmmd); 
    }

    /// <summary>
    /// 时时获取船舶gps轨迹
    /// 0:取出最新的飞机经纬度信息
    /// 1:取出船舶最新经纬度信息(1.首先获取当前任务是否存在 新目标船舶跟踪)
    /// 2:生成新的跟踪目标航线
    /// 3:上传新航线
    /// 4:切入航点
    /// </summary>
    public void onGetShipSignals()
    {
        string taskID = Request.Form.Get("TaskID");
        string uavID = Request.Form.Get("UavID");
        string lineHeight = Request.Form.Get("LineHeight");
        string flyWay = Request.Form.Get("FlyWay");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        string msg = "";
        int lineCode = 1;//任务跟踪航线编码 默认给 1
        int newLineCode = 7;//飞行过程中 新目标船舶跟踪 航线编码给 7
        bool newTrackFlag = false;//判断是否存在飞行过程中新目标. 用于切入航线航线  提示信息使用
        bool trackFlag = false;//用于表示船舶跟踪任务是否获取到轨迹信息

        try
        {
            if (string.IsNullOrEmpty(taskID) || string.IsNullOrEmpty(uavID))
            {
                result.IsOK = false;
                result.ErrMessage = "所选任务单号或飞机为空,请重新选择！";
            }
            else
            {
                NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));
                DataTable dt_YaoCe = nav.Nav_Get_FlySignals(true, out msg);

                if (dt_YaoCe == null || dt_YaoCe.Rows.Count <= 0)
                {
                    result.IsOK = false;
                    result.ErrMessage = "获取飞机的遥测信号失败";
                }
                else
                {
                    //飞机的最新经纬度
                    string Jd1 = dt_YaoCe.Rows[0]["Loction_Longitude"].ToString();
                    string Wd1 = dt_YaoCe.Rows[0]["Loction_Latitude"].ToString();
                    //跟踪航线高度
                    int heightG = Convert.ToInt32(lineHeight);


                    #region 飞行过程中新目标跟踪船舶 经纬度信息
                    //0. sp_GetShipTrackLineInfo 已检查该任务是否 存在跟踪的船舶(不分跟踪船舶任务 和 新目标跟踪)
                    //1. 调用 sp_GetShipTrackLineInfo 存储过程查找 TrackType = 1 的飞行过程中新目标跟踪船舶 轨迹信息
                    SqlCommand shipCmmd = new SqlCommand("sp_GetShipTrackLineInfo");
                    shipCmmd.CommandType = CommandType.StoredProcedure;
                    shipCmmd.Parameters.Add("@TaskID", SqlDbType.Int);
                    shipCmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);
                    shipCmmd.Parameters.Add("@TrackType", SqlDbType.Int);
                    shipCmmd.Parameters["@TrackType"].Value = 1;
                    shipCmmd.Parameters.Add("@LineCode", SqlDbType.Int);
                    shipCmmd.Parameters["@LineCode"].Value = newLineCode;
                    DataSet newDs = DB.ExecuteDataset(shipCmmd);
                    //如果存在飞行过程中,新目标船舶跟踪
                    if (newDs != null && newDs.Tables.Count >= 1 && newDs.Tables[0].Rows.Count >= 1 && newDs.Tables[1].Rows.Count >= 1)
                    {
                        newTrackFlag = true;//获取新目标gps标识
                        result.shipLineNew = newDs.Tables[0];
                        result.ShipDtNew = newDs.Tables[1];
                        string lng = newDs.Tables[1].Rows[0]["lng"].ToString();
                        string lat = newDs.Tables[1].Rows[0]["lat"].ToString();

                        Collection<SqlCommand> cmmds = new Collection<SqlCommand>();
                        //调用补点存储过程
                        DataTable dtPoint = GetLinepointMiddle(Jd1, Wd1, lng, lat);
                        if (dtPoint == null || dtPoint.Rows.Count <= 0)//没有补的点。就直接添加
                        {
                            cmmds.Add(saveLineInfoCmmd(Jd1, Wd1, taskID, uavID, 1, newLineCode, heightG));
                            cmmds.Add(saveLineInfoCmmd(lng, lat, taskID, uavID, 2, newLineCode, heightG));
                        }
                        else
                        {
                            for (int i = 0; i <= dtPoint.Rows.Count; i++)
                            {
                                if (i == 0)//第一段航线的 第一个点
                                {
                                    cmmds.Add(saveLineInfoCmmd(Jd1, Wd1, taskID, uavID, 1, newLineCode, heightG));
                                }
                                else if (i == dtPoint.Rows.Count)//第一段航线的末尾点
                                {
                                    cmmds.Add(saveLineInfoCmmd(lng, lat, taskID, uavID, (dtPoint.Rows.Count + 1), newLineCode, heightG));
                                }
                                else
                                {
                                    cmmds.Add(saveLineInfoCmmd(dtPoint.Rows[i]["Jd"].ToString(), dtPoint.Rows[i]["Wd"].ToString(), taskID, uavID, i + 1, newLineCode, heightG));
                                }
                            }
                        }

                        //增加新的跟踪航线
                        string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
                        if (!string.IsNullOrEmpty(res))
                        {
                            result.IsOK = false;
                            result.ErrMessage = res;
                        }
                        else
                        {
                            //取出新的飞行过程中 新目标跟踪航线
                            result.DataListNew = getTrackLine(taskID, newLineCode);
                        }
                    }
                    #endregion

                    #region  船舶跟踪类型，需要时时更新 船舶轨迹
                    //如果当前任务是船舶跟踪类型，则需要时时更新原目标船舶轨迹
                    if (flyWay.Equals(_FlyWay))
                    {
                        //2.调用 sp_GetShipTrackLineInfo 存储过程查找 TrackType = 0 的任务界面创建的跟踪船舶任务 轨迹信息
                        //改用存储过程，分别查出轨迹和最新的一条船舶经纬度信息
                        shipCmmd.Parameters.Clear();
                        shipCmmd.Parameters.Add("@TaskID", SqlDbType.Int);
                        shipCmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);
                        shipCmmd.Parameters.Add("@TrackType", SqlDbType.Int);
                        shipCmmd.Parameters["@TrackType"].Value = 0;
                        shipCmmd.Parameters.Add("@LineCode", SqlDbType.Int);
                        shipCmmd.Parameters["@LineCode"].Value = lineCode;

                        DataSet ds = DB.ExecuteDataset(shipCmmd);
                        if (ds != null && ds.Tables.Count >= 1 && ds.Tables[0].Rows.Count >= 1 && ds.Tables[1].Rows.Count >= 1)
                        {
                            trackFlag = true;//获取gps轨迹标识

                            result.shipLine = ds.Tables[0];
                            result.ShipDt = ds.Tables[1];
                            //取出船舶最新经纬度信息
                            //1.显示测试船舶图标。
                            //2.生成新的跟踪航线。
                            string lng = ds.Tables[1].Rows[0]["lng"].ToString();
                            string lat = ds.Tables[1].Rows[0]["lat"].ToString();

                            Collection<SqlCommand> cmmds = new Collection<SqlCommand>();
                            //调用补点存储过程
                            DataTable dtPoint = GetLinepointMiddle(Jd1, Wd1, lng, lat);
                            if (dtPoint == null || dtPoint.Rows.Count <= 0)//没有补的点。就直接添加
                            {
                                cmmds.Add(saveLineInfoCmmd(Jd1, Wd1, taskID, uavID, 1, lineCode, heightG));
                                cmmds.Add(saveLineInfoCmmd(lng, lat, taskID, uavID, 2, lineCode, heightG));
                            }
                            else
                            {
                                for (int i = 0; i <= dtPoint.Rows.Count; i++)
                                {
                                    if (i == 0)//第一段航线的 第一个点
                                    {
                                        cmmds.Add(saveLineInfoCmmd(Jd1, Wd1, taskID, uavID, 1, lineCode, heightG));
                                    }
                                    else if (i == dtPoint.Rows.Count)//第一段航线的末尾点
                                    {
                                        cmmds.Add(saveLineInfoCmmd(lng, lat, taskID, uavID, (dtPoint.Rows.Count + 1), lineCode, heightG));
                                    }
                                    else
                                    {
                                        cmmds.Add(saveLineInfoCmmd(dtPoint.Rows[i]["Jd"].ToString(), dtPoint.Rows[i]["Wd"].ToString(), taskID, uavID, i + 1, lineCode, heightG));
                                    }
                                }
                            }

                            //增加新的跟踪航线
                            string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
                            if (!string.IsNullOrEmpty(res))
                            {
                                result.IsOK = false;
                                result.ErrMessage = res;
                            }
                            else
                            {
                                //取出新的跟踪航线
                                result.DataList = getTrackLine(taskID, lineCode);
                            }
                        }
                        else
                        {
                            result.IsOK = false;
                            result.ErrMessage = "船舶跟踪任务,没有获取到经纬度信息";
                        }
                    }
                    #endregion

                    //记录定时器命令
                    Collection<SqlCommand> cmmdsOrder = new Collection<SqlCommand>();

                    //调用飞机设置跟踪航线接口,指定航线编码 7
                    if (newTrackFlag)
                    {
                        callNav_Set_Line(taskID, uavID, newLineCode, result, msg);
                        //保存上传航线操作动作
                        SqlCommand cmmdOrder = Common.saveControlTypeLog(taskID, uavID, "2:时时更新:设置飞机跟踪航线接口,航线编码[7]", null, result.IsOK, result.ErrMessage, 1, currentUserInfo.ID);
                        cmmdsOrder.Add(cmmdOrder);
                    }
                    //调用飞机设置跟踪航线接口,指定航线编码 1
                    if (trackFlag)
                    {
                        callNav_Set_Line(taskID, uavID, lineCode, result, msg);
                        //保存上传航线操作动作
                        SqlCommand cmmdOrder = Common.saveControlTypeLog(taskID, uavID, "2:时时更新:设置飞机跟踪航线接口,航线编码[1]", null, result.IsOK, result.ErrMessage, 1, currentUserInfo.ID);
                        cmmdsOrder.Add(cmmdOrder);
                    }
                    if (!newTrackFlag && !trackFlag)
                    {
                        result.IsOK = false;
                        result.ErrMessage = "未检测到有需要更新,跟踪船舶的飞行航线";
                    }

                    //切换到跟踪目标航线
                    //与飞控端核对：直接从第2个航点切入。
                    //说明：如果存在新目标跟踪，优先切入新目标航线，否则切入存在的 跟踪目标航线
                    if (newTrackFlag)
                    {
                        result.IsOK = nav.Nav_Change_Line(Convert.ToByte(newLineCode), Convert.ToInt32(2), out msg);
                        SqlCommand cmmdOrder = Common.saveControlTypeLog(taskID, uavID, "2:时时更新:[新目标航线][新目标轨迹]", null, result.IsOK, result.ErrMessage, 1, currentUserInfo.ID);
                        cmmdsOrder.Add(cmmdOrder);
                    }
                    else if (trackFlag)
                    {
                        result.IsOK = nav.Nav_Change_Line(Convert.ToByte(lineCode), Convert.ToInt32(2), out msg);
                        SqlCommand cmmdOrder = Common.saveControlTypeLog(taskID, uavID, "2:时时更新:[跟踪任务航线][船舶轨迹]", null, result.IsOK, result.ErrMessage, 1, currentUserInfo.ID);
                        cmmdsOrder.Add(cmmdOrder);
                    }

                    //保存定时器操作动作
                    if (cmmdsOrder.Count >= 1)
                    {
                       DataBase.ExceuteCommands(cmmdsOrder, DB.GetConnection().ConnectionString);
                    }

                }
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    public class MyNameTransfom_Out : ICSharpCode.SharpZipLib.Core.INameTransform
    {

        #region INameTransform 成员

        public string TransformDirectory(string name)
        {
            return null;
        }

        public string TransformFile(string name)
        {
            return Path.GetFileName(name);
        }

        #endregion
    }

    public class MyNameTransfom_R : ICSharpCode.SharpZipLib.Core.INameTransform
    {

        #region INameTransform 成员

        public string TransformDirectory(string name)
        {
            return null;
        }

        public string TransformFile(string name)
        {
            return Path.GetFileName(name);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class LoadFormResultN
    {
        public string centerCode = null;
        public string lat = null;
        public string lng = null;

        public string shipLat = null;
        public string shipLng = null;

        public string baseStationLat = null;
        public string baseStationLng = null;

        public DataTable DataList = null;

        public DataTable DataListNew = null;
        /// <summary>
        /// DataList 
        /// </summary>
        public DataTable DataListLat = null;
        /// <summary>
        /// DataList 
        /// </summary>
        public DataTable DataListLng = null;
        /// <summary>
        /// 跟踪任务的船舶信息
        /// </summary>
        public DataTable ShipDt = null;
        /// <summary>
        /// 船舶轨迹信息 
        /// </summary>
        public DataTable shipLine = null;

        /// <summary>
        /// 飞行过程中跟踪任务的船舶信息
        /// </summary>
        public DataTable ShipDtNew = null;
        /// <summary>
        /// 飞行过程中船舶轨迹信息 
        /// </summary>
        public DataTable shipLineNew = null;
        /// <summary>
        /// 其它
        /// </summary>
        public bool IsOK = false;
        public string ErrMessage = string.Empty;
    }
}

