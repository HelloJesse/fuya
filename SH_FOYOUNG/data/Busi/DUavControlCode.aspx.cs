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

public partial class data_DUavControlCode : BasePage
{
    UserInfo currentUserInfo = null;//当前用户

    //飞行方式：0 船舶跟踪   1：巡航任务
    private const string _FlyWay = "0";

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
    /// 相机角度控制
    /// direction 调整的度数
    /// type 方向
    /// 
    /// </summary>
    public void onCameraControl()
    {
        string type = Request.Form.Get("Type");
        string taskID = Request.Form.Get("TaskID");
        string uavID = Request.Form.Get("UavID");
        string direction = Request.Form.Get("Direction");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        try
        {
            //验证飞控任务 是否已经最总确认
            Boolean checkFlag = onTrackCheckFlag(taskID);
            if (!checkFlag)//未最终确认
            {
                result.IsOK = false;
                result.ErrMessage = "已安排的任务未最终确认,操作已返回!";
            }
            else if (string.IsNullOrEmpty(direction))
            {
                result.IsOK = false;
                result.ErrMessage = "调整的度数有误,操作已返回!";
            }
            else
            {
                NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));

                if (CameraControlType.上调.ToString().Equals(type))
                {
                    result.IsOK = nav.Nav_Control_Camera(byte.Parse(direction), CameraControlType.上调, out msg);
                }
                else if (CameraControlType.下调.ToString().Equals(type))
                {
                    result.IsOK = nav.Nav_Control_Camera(byte.Parse(direction), CameraControlType.下调, out msg);
                }
                else if (CameraControlType.右调.ToString().Equals(type))
                {
                    result.IsOK = nav.Nav_Control_Camera(byte.Parse(direction), CameraControlType.右调, out msg);
                }
                else if (CameraControlType.左调.ToString().Equals(type))
                {
                    result.IsOK = nav.Nav_Control_Camera(byte.Parse(direction), CameraControlType.左调, out msg);
                }
                else if (CameraControlType.归中.ToString().Equals(type))
                {
                    result.IsOK = nav.Nav_Control_Camera(byte.Parse(direction), CameraControlType.归中, out msg);
                }
                else if (CameraControlType.变倍加.ToString().Equals(type))
                {   //特此说明，调用接口取反为正确
                    result.IsOK = nav.Nav_Control_Camera(byte.Parse(direction), CameraControlType.变倍减, out msg);
                }
                else if (CameraControlType.变倍减.ToString().Equals(type))
                {
                    result.IsOK = nav.Nav_Control_Camera(byte.Parse(direction), CameraControlType.变倍加, out msg); 
                }
                if (!string.IsNullOrEmpty(msg))
                {
                    result.ErrMessage = msg;
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
    /// 飞行方向控制
    /// controlNum 调整的度数
    /// type 方向
    /// </summary>
    public void onFlyControl()
    {
        string controlNum = Request.Form.Get("ControlNum");
        string typeName = Request.Form.Get("Type");
        string typeId = Request.Form.Get("TypeId");

        string taskID = Request.Form.Get("TaskID");
        string uavID = Request.Form.Get("UavID");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        try
        {
            if (string.IsNullOrEmpty(controlNum) || string.IsNullOrEmpty(typeName) ||
                string.IsNullOrEmpty(typeId) || string.IsNullOrEmpty(taskID) || string.IsNullOrEmpty(uavID))
            {
                result.IsOK = false;
                result.ErrMessage = "调用飞行控制方法参数为空,操作已返回！";
            }
            else
            { 
                //验证飞控任务 是否已经最总确认
                Boolean checkFlag = onTrackCheckFlag(taskID);
                if (!checkFlag)//未最终确认
                {
                    result.IsOK = false;
                    result.ErrMessage = "已安排的任务未最终确认,操作已返回!";
                }
                else
                {
                    //从配置中取出目标距离
                    int boatdistnct = getBoatdistnct(typeId);

                    NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));
                    if (ControlType.升高.GetHashCode().ToString().Equals(typeId))
                    {
                        //说明：升高命令 需要先调用先左偏 0 度命令，切出当前航线
                        //result.IsOK = nav.Nav_Control_Uav(0, ControlType.左飞, 0, out msg);
                        result.IsOK = nav.Nav_Control_Uav(Convert.ToInt32(controlNum), ControlType.升高, boatdistnct, out msg);
                    }
                    else if (ControlType.下降.GetHashCode().ToString().Equals(typeId))
                    {
                        //说明：下降命令 需要先调用先左偏 0 度命令，切出当前航线
                        //result.IsOK = nav.Nav_Control_Uav(0, ControlType.左飞, 0, out msg);
                        result.IsOK = nav.Nav_Control_Uav(Convert.ToInt32(controlNum), ControlType.下降, boatdistnct, out msg);
                    }
                    else if (ControlType.右飞.GetHashCode().ToString().Equals(typeId))
                    {
                        result.IsOK = nav.Nav_Control_Uav(Convert.ToInt32(Convert.ToDecimal(controlNum) * 10), ControlType.右飞, boatdistnct, out msg);
                    }
                    else if (ControlType.左飞.GetHashCode().ToString().Equals(typeId))
                    {
                        result.IsOK = nav.Nav_Control_Uav(Convert.ToInt32(Convert.ToDecimal(controlNum) * 10), ControlType.左飞, boatdistnct, out msg);
                    }

                    if (!string.IsNullOrEmpty(msg))
                    {
                        result.ErrMessage = msg;
                    }

                    //保存操作动作
                    SqlCommand cmmd = Common.saveControlTypeLog(taskID, uavID, typeName, controlNum, result.IsOK, result.ErrMessage, 0, currentUserInfo.ID);
                    DB.ExecuteNonQuery(cmmd);
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
    /// 从配置中取出目标距离
    /// </summary>
    /// <param name="typeId">控制方向标识</param>
    /// <returns></returns>
    private int getBoatdistnct(string typeId)
    {
        SqlCommand cmmd = new SqlCommand(string.Format(@"SELECT Code,Name_E AS Boatdistnct FROM dbo.D_SystemCode_Dtl 
                WHERE ParentID=(SELECT ID FROM D_SystemCode WHERE Code='Boatdistnct') AND Activated=1 AND Code = {0};", typeId));
        DataTable dt = DB.ExecuteDataTable(cmmd);
        if (dt != null && dt.Rows.Count >= 1)
        {
            return Convert.ToInt32(dt.Rows[0]["Boatdistnct"]);
        }
        return 0;
    }

    /// <summary>
    /// 飞机功能按钮控制
    /// Flag: 获取控制=0, 起飞=1, 返航=2, 盘旋=3, 悬停=4, 降落=5
    /// offtype:0 起飞定高 1：巡航
    /// </summary>
    public void onBtnControlType()
    {
        string flag = Request.Form.Get("Flag");
        string typeName = Request.Form.Get("TypeName");
        string num = Request.Form.Get("Num");
        string offtype = Request.Form.Get("offtype");
        string lineHeight = Request.Form.Get("LineHeight");

        string taskID = Request.Form.Get("TaskID");
        string uavID = Request.Form.Get("UavID");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        try
        {
            //获取当前用户所属部门
            result.departmentID = Convert.ToInt32(currentUserInfo.DepartmentID);

            if (string.IsNullOrEmpty(flag) || string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(taskID) || string.IsNullOrEmpty(uavID))
            {
                result.IsOK = false;
                result.ErrMessage = "调用飞机功能按钮控制参数为空,操作已返回！";
            }
            else
            {
                //验证飞控任务 是否已经最总确认
                Boolean checkFlag = onTrackCheckFlag(taskID);
                if (!checkFlag)//未最终确认
                {
                    result.IsOK = false;
                    result.ErrMessage = "已安排的任务未最终确认,操作已返回!";
                }
                else
                {
                    NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));
                    if (FunctionBtnControlType.获取控制.GetHashCode().ToString().Equals(flag))
                    {
                        result.IsOK = nav.Nav_Control_GetControl(out msg);
                    }
                    else if (FunctionBtnControlType.起飞.GetHashCode().ToString().Equals(flag))
                    {
                        result.IsOK = nav.Nav_Control_TakeOff(Convert.ToBoolean(Convert.ToInt32(offtype == "" ? "0" : offtype)), out msg);
                    }
                    else if (FunctionBtnControlType.返航.GetHashCode().ToString().Equals(flag))
                    {
                        DataTable dt_YaoCe = nav.Nav_Get_FlySignals(true, out msg);//获取最新的遥测信号
                        if (dt_YaoCe == null || dt_YaoCe.Rows.Count <= 0)
                        {
                            result.IsOK = false;
                            result.ErrMessage = "没有获取到最新的飞机遥测信号,生成返航航线失败,操作已返回！";
                        }
                        else
                        {
                            //返航航线高度
                            int heightG = Convert.ToInt32(lineHeight);

                            //先删除原先的返航航线，在生成已飞机最新经纬度坐标 返航的航线
                            SqlCommand checkCmmd = new SqlCommand(string.Format(@" DELETE FROM D_Uav_FlyLineRecord WHERE TaskID = {0} AND LineCode = 8; ", taskID));
                            DB.ExecuteNonQuery(checkCmmd);

                            string Jd = dt_YaoCe.Rows[0]["Loction_Longitude"].ToString();
                            string Wd = dt_YaoCe.Rows[0]["Loction_Latitude"].ToString();
                            //获取基站经纬度--也可以说是 起飞点的经纬度
                            SqlCommand baseCmmd = new SqlCommand("SELECT TOP 1 Longitude,Latitude FROM dbo.D_Uav_BaseStation WHERE Activated = 1;");
                            DataTable baseDt = DB.ExecuteDataTable(baseCmmd);
                            if (baseDt == null && baseDt.Rows.Count <= 0)
                            {
                                result.IsOK = false;
                                result.ErrMessage = "没有获取到基站经纬度,生成返航航线失败,操作已返回！";
                            }
                            else
                            {
                                string longitude = baseDt.Rows[0]["Longitude"].ToString();
                                string latitude = baseDt.Rows[0]["Latitude"].ToString();

                                Collection<SqlCommand> cmmds = new Collection<SqlCommand>();
                                //cmmds.Add(saveLineInfoCmmd(Jd, Wd, taskID, uavID, 1, 8, 140));
                                //cmmds.Add(saveLineInfoCmmd(longitude, latitude, taskID, uavID, 2, 8, 140));
                                //返航航线 需要补点
                                addNewFlyBackLine(cmmds, Jd, Wd, longitude, latitude, taskID, uavID, heightG);

                                string res = DataBase.ExceuteCommands(cmmds, DB.GetConnection().ConnectionString);
                                if (!string.IsNullOrEmpty(res))
                                {
                                    result.IsOK = false;
                                    result.ErrMessage = res;
                                }
                            }
                        }
                        //需要从新上传新的 返航航线
                        callNav_Set_BackLine(taskID, uavID, result, msg, nav);
                        if (result.IsOK)
                        {
                            //执行返航 接口
                            result.IsOK = nav.Nav_Control_GoHome(out msg);
                        }

                    }
                    else if (FunctionBtnControlType.盘旋.GetHashCode().ToString().Equals(flag))
                    {
                        if (string.IsNullOrEmpty(num))
                        {
                            num = "0";
                        }
                        result.IsOK = nav.Nav_Control_Cycle(Convert.ToByte(num), 20, out msg);
                    }
                    else if (FunctionBtnControlType.悬停.GetHashCode().ToString().Equals(flag))
                    {
                        //暂无方法
                    }
                    else if (FunctionBtnControlType.降落.GetHashCode().ToString().Equals(flag))
                    {
                        result.IsOK = nav.Nav_Control_TouchDown(out msg);
                    }

                    if (!string.IsNullOrEmpty(msg))
                    {
                        result.ErrMessage = msg;
                    }

                    //保存操作动作
                    SqlCommand cmmd = Common.saveControlTypeLog(taskID, uavID, typeName, num, result.IsOK, result.ErrMessage, 0, currentUserInfo.ID);
                    DB.ExecuteNonQuery(cmmd);
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
    /// 调用飞机设置 返航航线
    /// </summary>
    /// <param name="taskID"></param>
    /// <param name="uavID"></param>
    /// <param name="result"></param>
    /// <param name="msg"></param>
    private void callNav_Set_BackLine(string bILLID, string uavSetID, LoadFormResultN result, string msg, NavWebaction nav)
    {
        //取出返航 航线航点
        SqlCommand cmmdPoint = new SqlCommand("SELECT LineCode, Sortid, Jd, Wd,Height_G FROM D_Uav_FlyLineRecord WHERE TaskID = @TaskID AND LineCode = 8 ORDER BY Sortid;");
        cmmdPoint.Parameters.Add("@TaskID", SqlDbType.Int);
        cmmdPoint.Parameters["@TaskID"].Value = Convert.ToInt32(bILLID);
        DataTable dtPoint = DB.ExecuteDataTable(cmmdPoint);

        DataRow[] drRows = dtPoint.Select();

        //循环调用设置航线
        result.IsOK = nav.Nav_Set_Line(setAirlineModel(8, drRows), out msg);
        if (!result.IsOK || !string.IsNullOrEmpty(msg))
        {
            result.ErrMessage = "设置飞行航线错误,航线编码【" + 8 + "】错误信息：" + msg;
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
    /// 验证 已安排的任务是否最终确认
    /// </summary>
    /// <param name="taskID">任务单号</param>
    /// <returns></returns>
    private bool onTrackCheckFlag(string taskID)
    {
        SqlCommand cmmd = new SqlCommand(string.Format(@"SELECT BILLID FROM dbo.D_Uav_MainTask WHERE TaskStatus = 3 AND TrackCheckFlag = 1 AND BILLID = {0};", taskID));
        DataTable dt = DB.ExecuteDataTable(cmmd);
        if (dt != null && dt.Rows.Count >=1)
        {
            return true; 
        }
        return false;
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
    private SqlCommand saveLineInfoCmmd(string Jd, string Wd, string bILLID, string uavID, int sortid, int lineCode, int high)
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
    /// 1:获取飞机的遥测信号数据
    /// 2:获取飞机实际飞行航线数据，回显到地图上
    /// </summary>
    public void onGetNavFlySignals() 
    {
        string taskID = Request.Form.Get("TaskID");
        string uavID = Request.Form.Get("UavID");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        try
        {
            //获取当前用户所属部门
            result.departmentID = Convert.ToInt32(currentUserInfo.DepartmentID);

            if (string.IsNullOrEmpty(taskID) || string.IsNullOrEmpty(uavID))
            {
                result.IsOK = false;
                result.ErrMessage = "所选任务单号或飞机为空,请重新选择！";
            }
            else
            {
                NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));
                result.dataTable = nav.Nav_Get_FlySignals(true, out msg);

                if (result.dataTable == null || result.dataTable.Rows.Count <= 0)
                {
                    result.IsOK = false;
                    result.ErrMessage = "时时接收飞机遥测信号失败";
                }
                else
                {   
                    //记录飞机遥测信号时间
                    result.TrackDate = result.dataTable.Rows[0]["CREATE_DATE"].ToString();

                    //获取飞机实际飞行航线数据，回显到地图上
                    SqlCommand cmmd = new SqlCommand(@"SELECT MAX(ID) AS ID,Loction_Longitude,Loction_Latitude FROM dbo.Uav_Track 
                        WHERE Isolddata = 0 AND UavID = @UavID AND Activated = 1 GROUP BY Loction_Longitude, Loction_Latitude ORDER BY ID;");

                    cmmd.Parameters.Add("@UavID", SqlDbType.Int);
                    cmmd.Parameters["@UavID"].Value = Convert.ToInt32(uavID);
                    result.uavTrackLine = DB.ExecuteDataTable(cmmd);

                    //状态提醒.如果飞机当前不是在归航航线上，则进行提醒
                    //提醒规则：当前航线距离最后一个目标点300M内，弹出提示框进行状态提醒
                    result.currentLine = result.dataTable.Rows[0]["CurrentLine"].ToString();
                    if (!result.currentLine.Equals("0") && !result.currentLine.Equals("8"))
                    {
                        //飞机的经纬度
                        double lng1 = Convert.ToDouble(result.dataTable.Rows[0]["Loction_Longitude"].ToString());
                        double lat1 = Convert.ToDouble(result.dataTable.Rows[0]["Loction_Latitude"].ToString());

                        //取出当前任务，当前航线最后一个航点坐标
                        SqlCommand cmmdLine = new SqlCommand(@"SELECT TOP 1 Jd,Wd,Sortid FROM D_Uav_FlyLineRecord 
                            WHERE TaskID = @TaskID AND LineCode = @LineCode ORDER BY Sortid DESC;");

                        cmmdLine.Parameters.Add("@TaskID", SqlDbType.Int);
                        cmmdLine.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);
                        cmmdLine.Parameters.Add("@LineCode", SqlDbType.Int);
                        cmmdLine.Parameters["@LineCode"].Value = Convert.ToInt32(result.currentLine);
                        DataTable dt = DB.ExecuteDataTable(cmmdLine);
                        if (dt != null && dt.Rows.Count >= 1)
                        {
                            result.currentPoint = dt.Rows[0]["Sortid"].ToString();
                            result.targetPointM = Common.GetDistance(lat1, lng1,
                                Convert.ToDouble(dt.Rows[0]["Wd"].ToString()), Convert.ToDouble(dt.Rows[0]["Jd"].ToString())); 
                        }
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
    /// 显示飞机航线
    /// </summary>
    public void onViewFlyLine()
    {
        string bILLID = Request.Form.Get("BILLID");
        string flyWay = Request.Form.Get("FlyWay");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            SqlCommand cmmd = new SqlCommand("sp_GetonViewFlyLine");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
            cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(bILLID);
            cmmd.Parameters.Add("@FlyWay", SqlDbType.Int);
            cmmd.Parameters["@FlyWay"].Value = Convert.ToInt32(flyWay);

            DataSet ds = DB.ExecuteDataset(cmmd);
            if (ds != null && ds.Tables.Count > 0)
            {
                //获取补点后的飞机规划任务航线，系统生成的航线1 和 航线 8
                result.FlyLine = ds.Tables[0];
                //获取其它 测试航线 比如 2-5 航线
                result.FlyLineOther = ds.Tables[1];

                if (_FlyWay.Equals(flyWay))//船舶跟踪任务，海图显示船舶信息
                {
                    result.ShipDt =ds.Tables[2];
                }
            }
            //获取当前用户所属部门
            result.departmentID = Convert.ToInt32(currentUserInfo.DepartmentID);
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 设置当前地图加载区域为地图中心点坐标
    /// </summary>
    public void getSeaCenter()
    {
        string lat = Request.Form.Get("lat");
        string lng = Request.Form.Get("lng");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;

        try
        {
            SqlCommand cmmd = new SqlCommand("sp_SetSeaCenter");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@Lat", SqlDbType.Decimal);
            cmmd.Parameters["@Lat"].Value = Convert.ToDecimal(lat);
            cmmd.Parameters.Add("@Lng", SqlDbType.Decimal);
            cmmd.Parameters["@Lng"].Value = Convert.ToDecimal(lng);
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
    /// 继续巡航按钮功能
    /// 1：取出所有该任务的航点
    /// 2：取出飞机最新经纬度坐标
    /// 3：计算航点与飞机经纬度的距离
    /// 4：切入最近的航线航点
    /// </summary>
    public void onConCruiseLine() 
    {
        string taskID = Request.Form.Get("TaskId");
        string uavID = Request.Form.Get("UavID");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";

        try
        {
            //获取当前用户所属部门
            result.departmentID = Convert.ToInt32(currentUserInfo.DepartmentID);

            //1:
            SqlCommand cmmdPoint = new SqlCommand(@"SELECT LineCode, Sortid, Jd, Wd, 0.00 AS result 
                        FROM D_Uav_FlyLineRecord WHERE TaskID = @TaskID AND LineCode BETWEEN 1 AND 5 ORDER BY LineCode, Sortid;");
            cmmdPoint.Parameters.Add("@TaskID", SqlDbType.Int);
            cmmdPoint.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);
            DataTable dtPoint = DB.ExecuteDataTable(cmmdPoint);
            //2:
            NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));
            DataTable dt_YaoCe = nav.Nav_Get_FlySignals(true, out msg);//获取最新的遥测信号
            if (dt_YaoCe == null || dt_YaoCe.Rows.Count <= 0)
            {
                result.IsOK = false;
                result.ErrMessage = "没有获取到最新的飞机遥测信号,继续巡航操作已返回！";
            }
            else if (dtPoint == null || dtPoint.Rows.Count <= 0)
            {
                result.IsOK = false;
                result.ErrMessage = "没有获取到任务航线,继续巡航操作已返回！";
            }
            else
            {
                //查看飞机的航行状态字段 
                //首先检查当前飞机 是否有新目标船舶跟踪,如果有 则返回航行状态 
                //检查是否有跟踪任务 如果有 则返回航行状态 
                //TrackCycleFlag 值为 0 或 1
                SqlCommand cmmdPlane = new SqlCommand("sp_GetPlaneTrackCycleFlag");
                cmmdPlane.CommandType = CommandType.StoredProcedure;
                cmmdPlane.Parameters.Add("@TaskID", SqlDbType.Int);
                cmmdPlane.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);
                DataTable dtPlane = DB.ExecuteDataTable(cmmdPlane);
                if (dtPlane != null && dtPlane.Rows.Count >= 1)
                {
                    result.trackCycleFlag = dtPlane.Rows[0]["TrackCycleFlag"].ToString();
                }

                //3:
                //飞机的经纬度
                double lng1 = Convert.ToDouble(dt_YaoCe.Rows[0]["Loction_Longitude"].ToString());
                double lat1 = Convert.ToDouble(dt_YaoCe.Rows[0]["Loction_Latitude"].ToString());
                for (int i = 0; i < dtPoint.Rows.Count; i++)
                {
                    dtPoint.Rows[i]["result"] = Common.GetDistance(lat1, lng1, 
                        Convert.ToDouble(dtPoint.Rows[i]["Wd"].ToString()), Convert.ToDouble(dtPoint.Rows[i]["Jd"].ToString())); 
                }
                //取出飞机经纬度，距离最近的航线航点
                dtPoint.DefaultView.Sort = "result ASC";
                dtPoint = dtPoint.DefaultView.ToTable();
                //取出 航线 航点
                string cLine = dtPoint.Rows[0]["LineCode"].ToString();
                string cPoint = dtPoint.Rows[0]["Sortid"].ToString();

                //4：
                result.IsOK = nav.Nav_Change_Line(Convert.ToByte(cLine), Convert.ToInt32(cPoint), out msg);
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
    /// 时时获取飞机与船舶距离
    /// </summary>
    public void onGetTargetDistance()
    {
        string taskID = Request.Form.Get("TaskID");
        string uavID = Request.Form.Get("UavID");
        string flyWay = Request.Form.Get("FlyWay");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";

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
                    result.ErrMessage = "获取飞机的遥测信号失败,时时获取飞机与船舶距离操作已返回";
                }
                else
                {
                    //飞机的最新经纬度
                    double Jd1 = Convert.ToDouble(dt_YaoCe.Rows[0]["Loction_Longitude"].ToString());
                    double Wd1 = Convert.ToDouble(dt_YaoCe.Rows[0]["Loction_Latitude"].ToString());

                    //检查该任务是否 存在跟踪的船舶(不分跟踪船舶任务 和 新目标跟踪)
                    SqlCommand shipCommd = new SqlCommand(string.Format(@"SELECT ID,TaskID FROM D_Ship_MainTask WHERE TaskID = {0};",taskID));
                    DataTable shipDT = DB.ExecuteDataTable(shipCommd);
                    if (shipDT != null && shipDT.Rows.Count >= 1)
                    {
                        string sql = @"SELECT TOP 1  lon AS lng, lat AS lat 
                        FROM dbo.Ship_Track WHERE TaskID = {0} AND TrackType = {1} ORDER BY ID DESC";

                        //1.查找 TrackType = 1 的飞行过程中新目标跟踪船舶 经纬度信息
                        SqlCommand cmmd = new SqlCommand(string.Format(sql, taskID, 1));
                        DataTable newDt = DB.ExecuteDataTable(cmmd);

                        //如果在飞行过程中,存在新目标船舶跟踪. 则获取飞机与新目标的距离
                        if (newDt != null && newDt.Rows.Count >= 1)
                        {
                            double Jd2 = Convert.ToDouble(newDt.Rows[0]["lng"].ToString());
                            double Wd2 = Convert.ToDouble(newDt.Rows[0]["lat"].ToString());
                            result.targetDistanceM = Common.GetDistance(Wd1, Jd1, Wd2, Jd2);
                        }
                        else
                        {
                            // 跟踪任务获取飞机与跟踪船舶的距离
                            if (flyWay.Equals(_FlyWay))
                            {
                                cmmd = new SqlCommand(string.Format(sql, taskID, 0));
                                DataTable dt = DB.ExecuteDataTable(cmmd);
                                if (dt != null && dt.Rows.Count >= 1)
                                {
                                    double Jd2 = Convert.ToDouble(dt.Rows[0]["lng"].ToString());
                                    double Wd2 = Convert.ToDouble(dt.Rows[0]["lat"].ToString());
                                    result.targetDistanceM = Common.GetDistance(Wd1, Jd1, Wd2, Jd2);
                                }
                            }
                        }
                    }
                    else
                    {
                        //没有需要获取飞机与目标距离
                        result.targetDistanceM = -1;
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
    /// 时时更新目标船舶盘旋点
    /// </summary>
    public void onGetTargetCycle()
    {
        string taskID = Request.Form.Get("TaskID");
        string uavID = Request.Form.Get("UavID");
        string cycleNum = Request.Form.Get("CycleNum");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";
        string lng = "", lat = ""; 

        try
        {
            //0.取出船舶的最新经纬度点信息. 更新飞机航行状态为目标盘旋模式
            //1.查找 TrackType = 1 的飞行过程中新目标跟踪船舶 经纬度信息
            SqlCommand cmmd = new SqlCommand("sp_GetShipTargetCycle");
            cmmd.CommandType = CommandType.StoredProcedure;
            cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
            cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);
            cmmd.Parameters.Add("@TrackType", SqlDbType.Int);
            cmmd.Parameters["@TrackType"].Value = 1;
            DataTable newDt = DB.ExecuteDataTable(cmmd);

            //如果在飞行过程中,存在新目标船舶跟踪. 则获取新目标的经纬度
            if (newDt != null && newDt.Rows.Count >= 1)
            {
                lng = newDt.Rows[0]["lng"].ToString();
                lat = newDt.Rows[0]["lat"].ToString();
            }
            else
            {
                // 跟踪任务获取船舶的经纬度信息
                cmmd.Parameters.Clear();
                cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
                cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);
                cmmd.Parameters.Add("@TrackType", SqlDbType.Int);
                cmmd.Parameters["@TrackType"].Value = 0;

                DataTable dt = DB.ExecuteDataTable(cmmd);
                if (dt != null && dt.Rows.Count >= 1)
                {
                    lng = dt.Rows[0]["lng"].ToString();
                    lat = dt.Rows[0]["lat"].ToString();
                }
            }

            if (string.IsNullOrEmpty(lng) || string.IsNullOrEmpty(lat))
            {
                result.IsOK = false;
                result.ErrMessage = "获取跟踪船舶经纬度信息失败";
            }
            else
            {
                NavWebaction nav = new NavWebaction(DB, Convert.ToInt32(uavID), Convert.ToInt32(taskID));
                NavCycleobj obj = new NavCycleobj();
                obj.CycleNum = Convert.ToByte(cycleNum);
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
                //用于前台显示
                result.cycleLat = lat;
                result.cycleLng = lng;

                result.IsOK = nav.Nav_Change_Line(obj, out msg);

                //取出新目标和原目标 轨迹
                SqlCommand cmmdLine = new SqlCommand("sp_GetShipTrackLineForCycle");
                cmmdLine.CommandType = CommandType.StoredProcedure;
                cmmdLine.Parameters.Add("@TaskID", SqlDbType.Int);
                cmmdLine.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);
                DataSet ds = DB.ExecuteDataset(cmmdLine);
                //新目标船舶轨迹
                result.shipLineNew = ds.Tables[0];
                result.ShipDtNew = ds.Tables[1];
                //原目标
                result.shipLine = ds.Tables[2];
                result.ShipDt = ds.Tables[3];

                //保存操作动作
                SqlCommand cmmdOrder = Common.saveControlTypeLog(taskID, uavID, "3:时时更新目标船舶盘旋点.经度[" + lng + "]纬度[" + lat + "]", null, result.IsOK, result.ErrMessage, 1, currentUserInfo.ID);
                DB.ExecuteNonQuery(cmmdOrder);
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
    /// 删除掉原先飞行过程中跟踪的目标
    /// </summary>
    public void delNewTarget()
    {
        string taskID = Request.Form.Get("TaskID");
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            SqlCommand cmmd = new SqlCommand("sp_delNewTarget");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@TaskID", SqlDbType.Int);
            cmmd.Parameters["@TaskID"].Value = Convert.ToInt32(taskID);

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
    /// 清除飞行航迹
    /// </summary>
    public void onClearTrack()
    {
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            SqlCommand cmmd = new SqlCommand("UPDATE dbo.Uav_Track SET Activated = 0 WHERE (Isolddata = 0 OR OldTaskID IS NULL);");
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
    /// 获取当前登录用户
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public void GetNowUserDept()
    {
       LoadFormResultN result = new LoadFormResultN();
       result.IsOK = true;
       
       //获取当前用户所属部门
       result.departmentID = Convert.ToInt32(currentUserInfo.DepartmentID);
       Response.Write(VLP.JSON.Encode(result));
    }

    public partial class LoadFormResultN
    {

        /// <summary>
        /// 记录飞机遥测信号时间
        /// </summary>
        public string TrackDate = null;
        /// <summary>
        /// DataList 
        /// </summary>
        public DataTable dataTable = null;
        /// <summary>
        /// Uav_Track 飞机实际飞行航线 
        /// </summary>
        public DataTable uavTrackLine = null;
        /// <summary>
        /// 飞机已安排规划的航线 航线 1 和航线 8 一条完成的航线
        /// </summary>
        public DataTable FlyLine = null;
        /// <summary>
        /// 飞机其它测试的航线
        /// </summary>
        public DataTable FlyLineOther = null;
        /// <summary>
        /// 跟踪任务的船舶信息
        /// </summary>
        public DataTable ShipDt = null;
        /// <summary>
        /// 船舶轨迹信息 
        /// </summary>
        public DataTable shipLine = null;
        /// <summary>
        /// 飞机与船舶距离
        /// </summary>
        public double targetDistanceM = -1;
        /// <summary>
        /// 飞机与当前航线最后一个航点距离
        /// </summary>
        public double targetPointM = -1;
        /// <summary>
        /// 当前航线
        /// </summary>
        public string currentLine = null;
        /// <summary>
        /// 当前航线末尾点 
        /// </summary>
        public string currentPoint = null;
        /// <summary>
        /// 0 飞机飞行状态为目标跟踪  1 飞机飞行状态为目标盘旋
        /// </summary>
        public string trackCycleFlag = null;

        /// <summary>
        /// 时时回显 目标盘旋点经纬度信息
        /// </summary>
        public string cycleLng = null;
        /// <summary>
        /// 
        /// </summary>
        public string cycleLat = null;
        /// <summary>
        /// 当前用户所属部门 
        /// </summary>
        public int departmentID = -1;
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