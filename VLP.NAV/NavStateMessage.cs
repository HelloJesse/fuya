using Nordasoft.Data.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VLP.NAV
{
    
    /// <summary>
    /// 解析飞机接受遥测信号方法
    /// </summary>
    public class NavStateMessage
    {
        public static int nav_id = 1;// 默认给1
        
       // public static string dbstring = "server=114.55.132.105,58888;database=uav_foyoung;user id=foyoung;password=vlp@foyoung;";//数据库连接串

        /// <summary>
        /// 保存遥测信号到数据库
        /// </summary>
        /// <param name="mybyte"></param>
        public static void SaveTelemetrySignals(VLP.NAV.Common.ReceiveData[] datas,string cnn)
        {

            DataBase db = new DataBase(cnn);
            string ems = "";
            foreach (VLP.NAV.Common.ReceiveData myReceiveData in datas)
            {
                byte[] mybyte = myReceiveData.Data;
                SaveTelemetrySignals(db, mybyte, out ems);
            }
        }

        /// <summary>
        /// 保存遥测信息到数据库
        /// </summary>
        /// <param name="db">数据库连接处理</param>
        /// <param name="mybyte">处理的数据</param>
        /// <param name="errmsg">错误信息</param>
        /// <returns>成功：true 错误：false</returns>
        public static bool SaveTelemetrySignals(DataBase db, byte[] mybyte, out string errmsg)
        {
            bool flag = true;
            errmsg = "";
            try
            {
                NavMessageObj obj = ParsingTelemetrySignals(mybyte, out errmsg);
                if (string.IsNullOrEmpty(errmsg))
                {
                    #region 插入到数据表中
                    string sql = @" 
                              INSERT INTO [dbo].[Uav_Track]
                               (UavID
                               ,Loction_Longitude
                               ,Loction_Latitude
                               ,Height
                               ,CurrentLine
                               ,CurrentTargetPoint
                               ,DistanceCornering
                               ,DistanceTarget
                               ,DistanceOrigin
                               ,GPS_H
                               ,GPS_M
                               ,GPS_S
                               ,PitchingAngle
                               ,TargetPitchingAngle
                               ,RollAngle
                               ,TargetRollAngle
                               ,NoseAzimuth
                               ,TargetNoseAzimuth
                               ,GPSTrack
                               ,GroundHeight
                               ,TargetGroundHeight
                               ,GroundSpeed
                               ,AirSpeed
                               ,TargetSpeed
                               ,LiftingRate
                               ,CurrentCircles
                               ,TargetCircles
                               ,ControlMode
                               ,VehicleModal
                               ,SystemStatus
                               ,FlightPhase
                               ,Alarm_MainV_Low
                               ,Alarm_ServoV_Low
                               ,Alarm_Electricity_Low
                               ,Alarm_Status1
                               ,Alarm_AbnormalSpeed
                               ,Alarm_SpaceVelocityAnomaly
                               ,Alarm_Status2
                               ,ControlUnlocked
                               ,Alarm_GPSLoction_Low
                               ,Alarm_PositionSensor
                               ,Alarm_GrandHigh
                               ,Alarm_BeyondSecurityFence
                               ,Alarm_ControlRCDistance
                               ,Alarm_LiftingSpeed
                               ,Alarm_ControlUpward
                               ,Alarm_FlightControlHardware
                               ,Switch_GeneratorInterconnection
                               ,Switch_EngineStart
                               ,Switch_Circle
                               ,Switch_Homing
                               ,Switch_Closing
                               ,Switch_PutDownFlaps
                               ,Switch_OpenUmbrella
                               ,Switch_NightLights
                               ,TakePictures
                               ,Standby_isOff)
                         VALUES
                               (@UavID
                               ,@Loction_Longitude
                               ,@Loction_Latitude
                               ,@Height
                               ,@CurrentLine
                               ,@CurrentTargetPoint
                               ,@DistanceCornering
                               ,@DistanceTarget
                               ,@DistanceOrigin
                               ,@GPS_H
                               ,@GPS_M
                               ,@GPS_S
                               ,@PitchingAngle
                               ,@TargetPitchingAngle
                               ,@RollAngle
                               ,@TargetRollAngle
                               ,@NoseAzimuth
                               ,@TargetNoseAzimuth
                               ,@GPSTrack
                               ,@GroundHeight
                               ,@TargetGroundHeight
                               ,@GroundSpeed
                               ,@AirSpeed
                               ,@TargetSpeed
                               ,@LiftingRate
                               ,@CurrentCircles
                               ,@TargetCircles
                               ,@ControlMode
                               ,@VehicleModal
                               ,@SystemStatus
                               ,@FlightPhase
                               ,@Alarm_MainV_Low
                               ,@Alarm_ServoV_Low
                               ,@Alarm_Electricity_Low
                               ,@Alarm_Status1
                               ,@Alarm_AbnormalSpeed
                               ,@Alarm_SpaceVelocityAnomaly
                               ,@Alarm_Status2
                               ,@ControlUnlocked
                               ,@Alarm_GPSLoction_Low
                               ,@Alarm_PositionSensor
                               ,@Alarm_GrandHigh
                               ,@Alarm_BeyondSecurityFence
                               ,@Alarm_ControlRCDistance
                               ,@Alarm_LiftingSpeed
                               ,@Alarm_ControlUpward
                               ,@Alarm_FlightControlHardware
                               ,@Switch_GeneratorInterconnection
                               ,@Switch_EngineStart
                               ,@Switch_Circle
                               ,@Switch_Homing
                               ,@Switch_Closing
                               ,@Switch_PutDownFlaps
                               ,@Switch_OpenUmbrella
                               ,@Switch_NightLights
                               ,@TakePictures
                               ,@Standby_isOff)";
                    SqlCommand sqlcom = new SqlCommand();
                    sqlcom.CommandText = sql;
                    sqlcom.CommandType = System.Data.CommandType.Text;
                    sqlcom.Parameters.Add("@UavID", SqlDbType.Int);
                    sqlcom.Parameters["@UavID"].Value = nav_id;//飞机的ID 默认给1
                    sqlcom.Parameters.Add("@Loction_Longitude", SqlDbType.Decimal);
                    sqlcom.Parameters["@Loction_Longitude"].Value = (obj.Loction_Longitude == null ? 0 : obj.Loction_Longitude) * 0.000001; //经度
                    sqlcom.Parameters.Add("@Loction_Latitude", SqlDbType.Decimal);
                    sqlcom.Parameters["@Loction_Latitude"].Value = (obj.Loction_Latitude == null ? 0 : obj.Loction_Latitude) * 0.000001; //纬度
                    sqlcom.Parameters.Add("@Height", SqlDbType.Int);
                    sqlcom.Parameters["@Height"].Value = obj.Height; //高度
                    sqlcom.Parameters.Add("@CurrentLine", SqlDbType.TinyInt);
                    sqlcom.Parameters["@CurrentLine"].Value = obj.CurrentLine; //航线
                    sqlcom.Parameters.Add("@CurrentTargetPoint", SqlDbType.NVarChar, 30);
                    sqlcom.Parameters["@CurrentTargetPoint"].Value = obj.CurrentTargetPoint; //航点
                    sqlcom.Parameters.Add("@DistanceCornering", SqlDbType.Int);
                    sqlcom.Parameters["@DistanceCornering"].Value = obj.DistanceCornering;
                    sqlcom.Parameters.Add("@DistanceTarget", SqlDbType.Int);
                    sqlcom.Parameters["@DistanceTarget"].Value = obj.DistanceTarget;
                    sqlcom.Parameters.Add("@DistanceOrigin", SqlDbType.Int);
                    sqlcom.Parameters["@DistanceOrigin"].Value = obj.DistanceOrigin;
                    sqlcom.Parameters.Add("@GPS_H", SqlDbType.TinyInt);
                    sqlcom.Parameters["@GPS_H"].Value = obj.GPS_H;
                    sqlcom.Parameters.Add("@GPS_M", SqlDbType.TinyInt);
                    sqlcom.Parameters["@GPS_M"].Value = obj.GPS_M;
                    sqlcom.Parameters.Add("@GPS_S", SqlDbType.TinyInt);
                    sqlcom.Parameters["@GPS_S"].Value = obj.GPS_S;
                    sqlcom.Parameters.Add("@PitchingAngle", SqlDbType.Decimal);
                    sqlcom.Parameters["@PitchingAngle"].Value = (obj.PitchingAngle == null ? 0 : obj.PitchingAngle) * 0.1; //当前俯仰角
                    sqlcom.Parameters.Add("@TargetPitchingAngle", SqlDbType.Decimal);
                    sqlcom.Parameters["@TargetPitchingAngle"].Value = (obj.TargetPitchingAngle == null ? 0 : obj.TargetPitchingAngle) * 0.1; //目标俯仰角
                    sqlcom.Parameters.Add("@RollAngle", SqlDbType.Decimal);
                    sqlcom.Parameters["@RollAngle"].Value = (obj.RollAngle == null ? 0 : obj.RollAngle) * 0.1; //当前滚转角
                    sqlcom.Parameters.Add("@TargetRollAngle", SqlDbType.Decimal);
                    sqlcom.Parameters["@TargetRollAngle"].Value = (obj.TargetRollAngle == null ? 0 : obj.TargetRollAngle) * 0.1; //目标滚转角
                    sqlcom.Parameters.Add("@NoseAzimuth", SqlDbType.Decimal);
                    sqlcom.Parameters["@NoseAzimuth"].Value = (obj.NoseAzimuth == null ? 0 : obj.NoseAzimuth) * 0.1; //当前机头方位角
                    sqlcom.Parameters.Add("@TargetNoseAzimuth", SqlDbType.Decimal);
                    sqlcom.Parameters["@TargetNoseAzimuth"].Value = (obj.TargetNoseAzimuth == null ? 0 : obj.TargetNoseAzimuth) * 0.1; //目标机头方位角
                    sqlcom.Parameters.Add("@GPSTrack", SqlDbType.Decimal);
                    sqlcom.Parameters["@GPSTrack"].Value = (obj.GPSTrack == null ? 0 : obj.GPSTrack) * 0.1; //GPS 航迹向
                    sqlcom.Parameters.Add("@GroundHeight", SqlDbType.Decimal);
                    sqlcom.Parameters["@GroundHeight"].Value = (obj.GroundHeight == null ? 0 : obj.GroundHeight) * 0.1; //当前对地高度
                    sqlcom.Parameters.Add("@TargetGroundHeight", SqlDbType.Int);
                    sqlcom.Parameters["@TargetGroundHeight"].Value = obj.TargetGroundHeight; //目标对地高度
                    sqlcom.Parameters.Add("@GroundSpeed", SqlDbType.Int);
                    sqlcom.Parameters["@GroundSpeed"].Value = obj.GroundSpeed; //当前地速
                    sqlcom.Parameters.Add("@AirSpeed", SqlDbType.Int);
                    sqlcom.Parameters["@AirSpeed"].Value = obj.AirSpeed; //当前空速
                    sqlcom.Parameters.Add("@TargetSpeed", SqlDbType.Int);
                    sqlcom.Parameters["@TargetSpeed"].Value = obj.TargetSpeed; //目标速度
                    sqlcom.Parameters.Add("@LiftingRate", SqlDbType.Decimal);
                    sqlcom.Parameters["@LiftingRate"].Value = obj.LiftingRate * 0.1; //升降速率
                    sqlcom.Parameters.Add("@CurrentCircles", SqlDbType.Int);
                    sqlcom.Parameters["@CurrentCircles"].Value = obj.CurrentCircles; //当前盘旋圈数
                    sqlcom.Parameters.Add("@TargetCircles", SqlDbType.Int);
                    sqlcom.Parameters["@TargetCircles"].Value = obj.TargetCircles; //目标盘旋圈数
                    sqlcom.Parameters.Add("@ControlMode", SqlDbType.TinyInt);
                    sqlcom.Parameters["@ControlMode"].Value = obj.ControlMode; //控制状态--0 手动遥控 1 半自主 2 全自主
                    sqlcom.Parameters.Add("@VehicleModal", SqlDbType.TinyInt);
                    sqlcom.Parameters["@VehicleModal"].Value = obj.VehicleModal; //飞行器模态 0：固定翼 1：过渡期 2：多旋翼
                    sqlcom.Parameters.Add("@SystemStatus", SqlDbType.TinyInt);
                    sqlcom.Parameters["@SystemStatus"].Value = obj.SystemStatus; //系统状态 0：正常飞行状态 1：校准状态 2：保护状态
                    sqlcom.Parameters.Add("@FlightPhase", SqlDbType.TinyInt);
                    sqlcom.Parameters["@FlightPhase"].Value = obj.FlightPhase; //飞行阶段标志 0：巡航段 1：起飞段 2：降落段 3：地面等待
                    sqlcom.Parameters.Add("@Alarm_MainV_Low", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_MainV_Low"].Value = obj.Alarm_MainV_Low; //报警状态
                    sqlcom.Parameters.Add("@Alarm_ServoV_Low", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_ServoV_Low"].Value = obj.Alarm_ServoV_Low; //报警状态
                    sqlcom.Parameters.Add("@Alarm_Electricity_Low", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_Electricity_Low"].Value = obj.Alarm_Electricity_Low; //报警状态
                    sqlcom.Parameters.Add("@Alarm_Status1", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_Status1"].Value = obj.Alarm_Status1; //报警状态
                    sqlcom.Parameters.Add("@Alarm_AbnormalSpeed", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_AbnormalSpeed"].Value = obj.Alarm_AbnormalSpeed; //报警状态
                    sqlcom.Parameters.Add("@Alarm_SpaceVelocityAnomaly", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_SpaceVelocityAnomaly"].Value = obj.Alarm_SpaceVelocityAnomaly; //报警状态
                    sqlcom.Parameters.Add("@Alarm_Status2", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_Status2"].Value = obj.Alarm_Status2; //报警状态
                    sqlcom.Parameters.Add("@ControlUnlocked", SqlDbType.Bit);
                    sqlcom.Parameters["@ControlUnlocked"].Value = obj.ControlUnlocked; //报警状态
                    sqlcom.Parameters.Add("@Alarm_GPSLoction_Low", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_GPSLoction_Low"].Value = obj.Alarm_GPSLoction_Low; //报警状态
                    sqlcom.Parameters.Add("@Alarm_PositionSensor", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_PositionSensor"].Value = obj.Alarm_PositionSensor; //报警状态
                    sqlcom.Parameters.Add("@Alarm_GrandHigh", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_GrandHigh"].Value = obj.Alarm_GrandHigh; //报警状态
                    sqlcom.Parameters.Add("@Alarm_BeyondSecurityFence", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_BeyondSecurityFence"].Value = obj.Alarm_BeyondSecurityFence; //报警状态
                    sqlcom.Parameters.Add("@Alarm_ControlRCDistance", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_ControlRCDistance"].Value = obj.Alarm_ControlRCDistance; //报警状态
                    sqlcom.Parameters.Add("@Alarm_LiftingSpeed", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_LiftingSpeed"].Value = obj.Alarm_LiftingSpeed; //报警状态
                    sqlcom.Parameters.Add("@Alarm_ControlUpward", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_ControlUpward"].Value = obj.Alarm_ControlUpward; //报警状态
                    sqlcom.Parameters.Add("@Alarm_FlightControlHardware", SqlDbType.Bit);
                    sqlcom.Parameters["@Alarm_FlightControlHardware"].Value = obj.Alarm_FlightControlHardware; //报警状态
                    sqlcom.Parameters.Add("@Switch_GeneratorInterconnection", SqlDbType.Bit);
                    sqlcom.Parameters["@Switch_GeneratorInterconnection"].Value = obj.Switch_GeneratorInterconnection; //报警状态
                    sqlcom.Parameters.Add("@Switch_EngineStart", SqlDbType.Bit);
                    sqlcom.Parameters["@Switch_EngineStart"].Value = obj.Switch_EngineStart; //开关
                    sqlcom.Parameters.Add("@Switch_Circle", SqlDbType.Bit);
                    sqlcom.Parameters["@Switch_Circle"].Value = obj.Switch_Circle; //开关
                    sqlcom.Parameters.Add("@Switch_Homing", SqlDbType.Bit);
                    sqlcom.Parameters["@Switch_Homing"].Value = obj.Switch_Homing; //开关
                    sqlcom.Parameters.Add("@Switch_Closing", SqlDbType.Bit);
                    sqlcom.Parameters["@Switch_Closing"].Value = obj.Switch_Closing; //开关
                    sqlcom.Parameters.Add("@Switch_PutDownFlaps", SqlDbType.Bit);
                    sqlcom.Parameters["@Switch_PutDownFlaps"].Value = obj.Switch_PutDownFlaps; //开关
                    sqlcom.Parameters.Add("@Switch_OpenUmbrella", SqlDbType.Bit);
                    sqlcom.Parameters["@Switch_OpenUmbrella"].Value = obj.Switch_OpenUmbrella; //开关
                    sqlcom.Parameters.Add("@Switch_NightLights", SqlDbType.Bit);
                    sqlcom.Parameters["@Switch_NightLights"].Value = obj.Switch_NightLights; //开关
                    sqlcom.Parameters.Add("@TakePictures", SqlDbType.Int);
                    sqlcom.Parameters["@TakePictures"].Value = obj.TakePictures;//拍照次数
                    sqlcom.Parameters.Add("@Standby_isOff", SqlDbType.Bit);
                    sqlcom.Parameters["@Standby_isOff"].Value = obj.Standby_isOff; //起飞准备
                   
                    #endregion
                   int num = db.ExecuteNonQuery(sqlcom);
                   if (num == 0)
                   {
                       errmsg = "数据插入错误.";
                       flag = false;
                   }
                   Uav_AutoControl(obj, db);//飞机的状态控制--暂时不进行自动的控制
                }
                else
                {
                    flag = false;
                }
            }
            catch (Exception ex) {
                errmsg = ex.Message;
                flag = false;
            }
            
            return flag;
        }

        /// <summary>
        /// 自动飞行控制，判断飞机当前的状态如果是等待，则切换到下一个航点
        /// 或者飞机所在的位置与船的位置小于500米时，自动进行盘旋切换。盘旋命令 时
        /// 如果是跟踪任务的时候，需要根据实际的需要来修改航点的信息
        /// 约定：航线1为任务航线 航线9为：回家航线 航线0为系统默认的回家航线 航线10为系统自动的降落航线
        /// </summary>
        /// <param name="obj"></param>
        private static void Uav_AutoControl(NavMessageObj obj, DataBase _db)
        {
            try
            {
                /*
                 * 1.发送起飞后，飞机是 多旋翼飞到3m高度悬停
                 * 2.起飞悬停到3m后，发送 巡航指令，飞机开始盘旋切换，飞行器模态：固定翼 飞行阶段标志：巡航段 代表飞机可以切换到航线1执行任务了
                 * 3.切换到航线1的航点1
                 * 4.当1号航线结束后，要切换到航线9的第一个航点
                 * 5.当航线9的最后一个航点飞完了之后，要自动切换到 0 号航线的航点1
                 * 6.注意每次发送航线任务的时候，要等300毫秒，保证飞机状态处理正确
                 */

                 

                int task_id = 0;//任务ID
                int line_point = 0;//航点信息
                double line_lat = 0;
                double line_lng = 0;
                string errmsg = "";
                #region 根据飞机的ID取得飞机当前的任务ID
                string sql = "SELECT  BILLID,FlyWay,ShipId FROM dbo.D_Uav_MainTask WHERE UavSetID = @Uav_id AND TaskStatus = 3 AND ISDISABLED=0";
                SqlCommand sqlcom = new SqlCommand();
                sqlcom.CommandText = sql;
                sqlcom.CommandType = System.Data.CommandType.Text;
                sqlcom.Parameters.Add("@Uav_id", SqlDbType.Int);
                sqlcom.Parameters["@Uav_id"].Value = nav_id;//飞机的ID 默认给1
                DataTable dt = _db.ExecuteDataTable(sqlcom);//获取要处理的任务信息
                if (dt != null && dt.Rows.Count > 0)
                {
                    task_id = Convert.ToInt32(dt.Rows[0]["BILLID"]);
                }
                else
                {//没有找到任务ID，则直接返回
                    return;
                }
                #endregion
                NavWebaction sendmsg = new NavWebaction(_db, nav_id, task_id);//发送数据的对象

                if (obj.FlightPhase!=null&&obj.FlightPhase == 1 && obj.VehicleModal != null && obj.VehicleModal == 2 && obj.GroundHeight != null && obj.GroundHeight >= 3)
                {//起飞段&&多旋翼&&对地高度>=3
                    return;
                    #region 起飞完成，高度在3m了，则发送巡航指令
                    sendmsg.Nav_Control_TakeOff(true, out errmsg);//如果失败，则不处理，等待下次发送
                    return;
                    #endregion
                }
                else if (obj.FlightPhase != null && obj.FlightPhase == 0 && obj.VehicleModal != null && obj.VehicleModal == 0)
                {//巡航段&&固定翼 则说明飞机已经
                    return;
                    #region 巡航结束后，自动切换到第一个航线的第一个点
                    sendmsg.Nav_Change_Line(1, 1, out errmsg);//如果失败，则不处理，等待下次发送
                    return;
                    #endregion
                }
                else if (obj.CurrentLine == 1 || obj.CurrentLine == 9)
                {
                    return;
                    #region //动态处理飞机形态
                    /*
                     *  1. 如过超出安全位置报警，则直接切换到下一个航点
                     *  2. 如果将要飞到大桥，则提前把高度提高到200米
                     *  3. 如果当前航线之后，没有最后的航点了，则切换到下一个航点  
                     */
                    sql = string.Format(@"SELECT TOP 1 Sortid,Jd,Wd FROM dbo.D_Uav_FlyLineRecord WHERE TaskID = {0} AND LineCode = 1 AND UavID = {1} AND IsUpdateFlag = 0 ORDER BY Sortid DESC;", task_id, nav_id);
                    sqlcom = new SqlCommand();
                    sqlcom.CommandText = sql;
                    sqlcom.CommandType = System.Data.CommandType.Text;
                    DataTable dt_Line = _db.ExecuteDataTable(sqlcom);//
                    if (dt_Line != null && dt_Line.Rows.Count > 0)
                    {
                        line_point = Convert.ToInt32(dt_Line.Rows[0]["Sortid"]);
                        line_lat = Convert.ToDouble(dt_Line.Rows[0]["Wd"]);
                        line_lng = Convert.ToDouble(dt_Line.Rows[0]["Jd"]);
                    }
                    if (dt.Rows[0]["FlyWay"].ToString() == "0")
                    { //跟踪任务
                        #region 跟踪任务
                        int shipid = 0; //船舶的ID
                        shipid = Convert.ToInt32(dt.Rows[0]["ShipId"]);
                        //获取当前船舶的位置，来判断
                        double ship_lat = 0;
                        double ship_lng = 0;
                        sql = string.Format(@"SELECT lat,lon FROM dbo.Ship_Track WHERE ShipID = {0} AND TaskID = {1} ORDER BY ID DESC ;", shipid, task_id);
                        sqlcom = new SqlCommand();
                        sqlcom.CommandText = sql;
                        sqlcom.CommandType = System.Data.CommandType.Text;
                        DataTable dt_ship = _db.ExecuteDataTable(sqlcom);//获取船舶的位置信息
                        if (dt_ship != null && dt_ship.Rows.Count > 0)
                        {
                            ship_lat = Convert.ToDouble(dt_ship.Rows[0]["lat"]);
                            ship_lng = Convert.ToDouble(dt_ship.Rows[0]["lon"]);
                        }
                        //如果 当前航点的位置距离最后一个航点为
                        if (GetDistance((obj.Loction_Latitude * 0.000001), (obj.Loction_Longitude * 0.000001), ship_lat, ship_lng) >= 500)
                        {
                            //此时需要插入一个新的航点位置，且更新给飞机
                            sql = @" INSERT INTO dbo.D_Uav_FlyLineRecord
                                            ( TaskID ,
                                              LineCode ,
                                              UavID ,
                                              Sortid ,
                                              Jd ,
                                              Wd ,
                                              GUID ,
                                              0
                                            )
                                    VALUES  ( @TaskID , -- TaskID - int
                                              1 , -- LineCode - int
                                              @UavID , -- UavID - int
                                              @Sortid , -- Sortid - int
                                              @Jd , -- Jd - decimal
                                              @Wd , -- Wd - decimal
                                              0  -- IsUpdateFlag - bit
                                            )";
                            sqlcom = new SqlCommand();
                            sqlcom.CommandText = sql;
                            sqlcom.CommandType = System.Data.CommandType.Text;
                            sqlcom.Parameters.Add("@TaskID", SqlDbType.Int);
                            sqlcom.Parameters["@TaskID"].Value = task_id;
                            sqlcom.Parameters.Add("@UavID", SqlDbType.Int);
                            sqlcom.Parameters["@UavID"].Value = nav_id;
                            sqlcom.Parameters.Add("@Sortid", SqlDbType.Int);
                            sqlcom.Parameters["@Sortid"].Value = line_point + 1;
                            sqlcom.Parameters.Add("@Jd", SqlDbType.Decimal);
                            sqlcom.Parameters["@Jd"].Value = ship_lng;
                            sqlcom.Parameters.Add("@Wd", SqlDbType.Decimal);
                            sqlcom.Parameters["@Wd"].Value = ship_lat;
                            int num = _db.ExecuteNonQuery(sqlcom);
                            if (num == 0)
                            {
                                AirlineModel lineobj = new AirlineModel();
                                lineobj.id = 1;
                                lineobj.Airlinecode = 1;//都在航线1进行处理
                                lineobj.Aircycle = false;
                                Airlinepoint pointobj = new Airlinepoint(); //新增的航点
                                pointobj.Airpointcode = line_point + 1;
                                pointobj.Lineswitch = 0;
                                pointobj.Heightcontrol = 0;
                                pointobj.Longitude = (long)(ship_lng * 100000);
                                pointobj.Latitude = (long)(ship_lat * 100000);
                                pointobj.height = 150;
                                pointobj.NextAreaSpeed = 0;
                                pointobj.Isok_vedio = false;
                                pointobj.Isok_pictrue = false;
                                pointobj.Isaction_vedio = false;
                                pointobj.Isaction_pictrue = false;
                                pointobj.CycleNum = 0;
                                pointobj.PicDist = 0;
                                lineobj.Airlinepoint = new List<Airlinepoint>() { pointobj };
                                sendmsg.Nav_Set_Line(lineobj, out errmsg);//设置新的航点位置
                                return;
                            }
                        }
                        else { 
                            //切换到盘旋
                            NavCycleobj objcyc = new NavCycleobj();
                            objcyc.CycleNum = 30;
                            objcyc.Longitude = Convert.ToInt64(ship_lng * 100000);
                            objcyc.Latitude = Convert.ToInt64(ship_lat*100000);
                            objcyc.Cycleradius = 200;
                            objcyc.Cycletype = 0;//盘旋跟踪某一个目标
                            objcyc.Boatincremental = 0; //度数
                            objcyc.Boatdistnct = 0;//暂时不知道如何使用
                            if (objcyc.Longitude == 0 || objcyc.Latitude == 0)
                            {
                                return;
                            }
                            #region 3.切换到盘旋航线
                            sendmsg.Nav_Change_Line(objcyc, out errmsg);
                            if (string.IsNullOrEmpty(errmsg) == false)
                            {//如果失败，则再次发送一遍
                                sendmsg.Nav_Change_Line(objcyc, out errmsg);
                                return;
                            }
                            #endregion
                        }
                        #endregion
                    }

                    #region 处理航线的自动切换 如果航线1飞完，则飞航线9，如果航线9飞完，则切换到航线0 回家
                    if (obj.CurrentLine == 1 && obj.CurrentTargetPoint == "0")
                    {//说明此时飞到了最后一个航点
                        #region  最后一个航点后，切换到回家航线
                        sendmsg.Nav_Change_Line(0, 1, out errmsg);//如果失败，则不处理，等待下次发送
                        return;
                        #endregion
                    }

                    #endregion

                    #endregion
                }
                
            }
            catch
            { 
            }
 
        }
        private static double rad(double d)
        {
            return d * Math.PI / 180.0;
        }
        /// <summary>
        /// 获取2个坐标点之间的距离
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double EARTH_RADIUS = 6378.137;//地球半径
            double radLat1 = lat1;
            double radLat2 = lat2;
            double a = radLat1 - radLat2;
            double b = lng1 - lng2;

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

        /// <summary>
        /// 解析飞机遥测信息信息
        /// </summary>
        /// <param name="mybyte">数据Byte数组</param>
        /// <param name="errmsg">如果解析错误则返回错误原因,解析正确则为""</param>
        /// <returns>成功：ture 失败：false</returns>
        public static NavMessageObj ParsingTelemetrySignals(byte[] mybyte, out string errmsg)
        {
            NavMessageObj obj = new NavMessageObj();
            errmsg = "";
            try
            {
                if (mybyte == null)
                {
                    errmsg = "参数数据为null.";
                    return null;
                }
                //if (mybyte.Length != 136)
                //{
                //    errmsg = "参数数据长度非136,请检查.";
                //    return null;
                //}
                if (mybyte.Length != 104)
                {
                    errmsg = "参数数据长度非104,请检查.";
                    return null;
                }
                /*
                    下面开始解析
                 */
                byte[] headerbyte = new byte[8];//帧头数据，8个字节
                byte[] body_1 = new byte[24];//第一副帧数据，24个字节一个副帧，飞行遥测
                byte[] body_2 = new byte[24];//第二副帧数据，24个字节一个副帧，飞行遥测
                byte[] body_3 = new byte[24];//第三副帧数据，24个字节一个副帧，飞行遥测
                byte[] body_4 = new byte[24];//第四副帧数据，24个字节一个副帧，飞行遥测
                //byte[] body_5 = new byte[24];//第五副帧数据，24个字节一个副帧，载荷遥测
                //byte[] body_6 = new byte[24];//第六副帧数据，24个字节一个副帧，备用
                Array.Copy(mybyte, 0, headerbyte, 0, 8);
                Array.Copy(mybyte, 8, body_1, 0, 24);
                Array.Copy(mybyte, 32, body_2, 0, 24);
                Array.Copy(mybyte, 56, body_3, 0, 24);
                Array.Copy(mybyte, 80, body_4, 0, 24);
                //Array.Copy(mybyte, 104, body_5, 0, 24);
                //Array.Copy(mybyte, 128, body_6, 0, 8);
                #region 帧头信息
                obj.SynchronousCode1 = Convert.ToString(headerbyte[0], 16).ToUpper();
                obj.SynchronousCode2 = Convert.ToString(headerbyte[1], 16).ToUpper();
                obj.FrameLength = mybyte[2];//默认应该为136，一个字节存储一个数
                obj.TargetAddress = Convert.ToString(headerbyte[3], 16).Substring(0, 1);//第三个字节的高位为目的地址
                obj.SourceAddress = Convert.ToString(headerbyte[3], 16).Substring(1, 1);//第三个字节的低位为源地址
                obj.FrameDataType = Convert.ToString(headerbyte[4], 16).ToUpper();
                obj.FrameCode = Convert.ToString(headerbyte[5], 16).ToUpper();
                obj.StandyData1 = Convert.ToString(headerbyte[6], 16);//备用1
                obj.StandyData2 = Convert.ToString(headerbyte[7], 16);//备用2
                #endregion
                #region 第一副帧数据
                obj.TargetPitchingAngle = System.BitConverter.ToInt16(body_1, 1);//1-2 字节   目标俯仰角
                obj.TargetRollAngle = System.BitConverter.ToInt16(body_1, 3);//3-4 字节       目标滚转角
                obj.TargetNoseAzimuth = System.BitConverter.ToInt16(body_1, 5);//5-6 字节     目标方位角
                obj.PitchingAngle = System.BitConverter.ToInt16(body_1, 7);//7-8 字节         俯仰角
                obj.RollAngle = System.BitConverter.ToInt16(body_1, 9);//9-10 字节            滚转角
                obj.NoseAzimuth = System.BitConverter.ToInt16(body_1, 11);//11-12 字节        机头方位角
                obj.GPSTrack = System.BitConverter.ToInt16(body_1, 13);//13-14 字节           GPS 航迹向
                obj.GroundHeight = System.BitConverter.ToInt16(body_1, 15);//15-16 字节       对地高度
                obj.TargetSpeed = body_1[17];//17 1字节                                       目标速度
                obj.GroundSpeed = body_1[18];//18 1字节                                       地速
                obj.AirSpeed = body_1[19];//19 1字节                                          空速
                obj.LiftingRate = body_1[20];//20 1字节                                       升降速率
                obj.TargetGroundHeight = System.BitConverter.ToInt16(body_1, 21);//21-22 字节 目标高度                                     
                //第23字节为校验和 暂时不知道用处
                #endregion 
                #region 第二副帧数据
                obj.ControlMode = Common.GetByteBitNum(body_2[11], 0, 1); //                                    控制状态                   
                obj.VehicleModal = Common.GetByteBitNum(body_2[11], 2, 3);//                                    飞行器模态
                obj.SystemStatus = Common.GetByteBitNum(body_2[11], 4, 5);//                                    系统状态
                obj.FlightPhase = Common.GetByteBitNum(body_2[11], 6, 7);//                                     飞行阶段标志
                obj.Alarm_MainV_Low = Convert.ToBoolean(Common.GetByteBitNum(body_2[12], 0));//                 主电压低
                obj.Alarm_ServoV_Low = Convert.ToBoolean(Common.GetByteBitNum(body_2[12], 1));//                舵机电压低
                obj.Alarm_Electricity_Low = Convert.ToBoolean(Common.GetByteBitNum(body_2[12], 2));//           电流低
                obj.Alarm_Status1 = Convert.ToBoolean(Common.GetByteBitNum(body_2[12], 3));//                   状态监控1
                obj.Alarm_AbnormalSpeed = Convert.ToBoolean(Common.GetByteBitNum(body_2[12], 4));//             转速异常
                obj.Alarm_SpaceVelocityAnomaly = Convert.ToBoolean(Common.GetByteBitNum(body_2[12], 5));//      空速异常
                obj.Alarm_Status2 = Convert.ToBoolean(Common.GetByteBitNum(body_2[12], 6));//                   状态监控2
                obj.ControlUnlocked = Convert.ToBoolean(Common.GetByteBitNum(body_2[12], 7));//                 遥控解锁状态
                obj.Alarm_GPSLoction_Low = Convert.ToBoolean(Common.GetByteBitNum(body_2[13], 0));//            GPS 定位精度低
                obj.Alarm_PositionSensor = Convert.ToBoolean(Common.GetByteBitNum(body_2[13], 1));//            航姿传感器状态
                obj.Alarm_GrandHigh = Convert.ToBoolean(Common.GetByteBitNum(body_2[13], 2));//                 高度报警
                obj.Alarm_BeyondSecurityFence = Convert.ToBoolean(Common.GetByteBitNum(body_2[13], 3));//       超出安全围栏
                obj.Alarm_ControlRCDistance = Convert.ToBoolean(Common.GetByteBitNum(body_2[13], 4));//         遥控 RC 超距
                obj.Alarm_LiftingSpeed = Convert.ToBoolean(Common.GetByteBitNum(body_2[13], 5));//              升降速度报警
                obj.Alarm_ControlUpward = Convert.ToBoolean(Common.GetByteBitNum(body_2[13], 6));//             遥控上行状态
                obj.Alarm_FlightControlHardware = Convert.ToBoolean(Common.GetByteBitNum(body_2[13], 7));//     飞控硬件状态
                obj.Switch_GeneratorInterconnection = Convert.ToBoolean(Common.GetByteBitNum(body_2[18], 0));// 发电机并网
                obj.Switch_EngineStart = Convert.ToBoolean(Common.GetByteBitNum(body_2[18], 1));//              发动机启动
                obj.Switch_Circle = Convert.ToBoolean(Common.GetByteBitNum(body_2[18], 2));//                   盘旋
                obj.Switch_Homing = Convert.ToBoolean(Common.GetByteBitNum(body_2[18], 3));//                   归航
                obj.Switch_Closing = Convert.ToBoolean(Common.GetByteBitNum(body_2[18], 4));//                  关车
                obj.Switch_PutDownFlaps = Convert.ToBoolean(Common.GetByteBitNum(body_2[18], 5));//             襟翼
                obj.Switch_OpenUmbrella = Convert.ToBoolean(Common.GetByteBitNum(body_2[18], 6));//             开伞
                obj.Switch_NightLights = Convert.ToBoolean(Common.GetByteBitNum(body_2[18], 7));//              夜航灯
                obj.CurrentCircles = body_2[20];//                                                       当前盘旋圈数
                obj.TargetCircles = body_2[19];//                                                        目标盘旋圈数
                obj.TakePictures = System.BitConverter.ToInt16(body_2, 21);//                            拍照次数
                #endregion
                #region 第三副帧数据
                obj.Loction_Longitude = System.BitConverter.ToInt32(body_3, 1);//                        经度
                obj.Loction_Latitude = System.BitConverter.ToInt32(body_3, 5);//                         纬度
                obj.Height = System.BitConverter.ToInt16(body_3, 9);//                                   高度
                obj.DistanceCornering = System.BitConverter.ToInt16(body_3, 11);//                       侧偏距离
                obj.DistanceTarget = System.BitConverter.ToInt16(body_3, 13);//                          侧偏距离
                obj.DistanceOrigin = System.BitConverter.ToInt16(body_3, 15);//                          原点距离
                obj.Standby_isOff = Convert.ToBoolean(Common.GetByteBitNum(body_3[17], 4));//           2017-1-5 第三副帧的bit4位 0：不能起飞 1：可以起飞
                string numstr = Common.putintTo2string(body_3[17], 4);
                obj.CurrentLine = Convert.ToInt16(numstr,2);//                                           当前航线编码
                //obj.CurrentTargetPoint = Convert.ToInt32(numstr + Common.putintTo2string(body_3[18], 8));//                       目标航点需要17与18的组合
                obj.CurrentTargetPoint = numstr + Common.putintTo2string(body_3[18], 8);//                       目标航点需要17与18的组合
                obj.GPS_H = body_3[19];//                                                                GPS小时
                obj.GPS_M = body_3[20];//                                                                GPS分钟
                obj.GPS_S = body_3[21];//                                                                GPS秒
                #endregion

            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
                return null;
            }

            return obj;
        }

       

    }

}
