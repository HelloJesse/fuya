using Nordasoft.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;

namespace VLP.NAV
{
    /// <summary>
    /// 复亚系统网页调用方法
    /// </summary>
    public class NavWebaction
    {

        private DataBase _db;//数据库调用db
        private int _nva_id = 0;//飞机的ID
        private int _task_id = 0;//任务的ID
        private string _ipaddress = "";//接口命令地址的IP
        private string _callModule = "";//通话模块标识
        private int _portnum = 0;//接口命令的端口号
        private decimal _baseLongitude = 0;//基站的经度
        private decimal _baseLatitude = 0;//基站的纬度
        private NavSendMessage sendmsg;//发送接口命令的对象

        /// <summary>
        /// 飞机发送命令接口的构造函数
        /// </summary>
        /// <param name="db">数据库访问db</param>
        /// <param name="nva_id">飞机的ID，用于取飞机的发送或者获取哪个飞机的信息</param>
        /// <param name="ipaddress"></param>
        /// <param name="portnum"></param>
        public NavWebaction(DataBase db, int nva_id, int task_id)
        {
            NavWebactionCommon(db, nva_id, task_id);
        }

        /// <summary>
        /// 飞机接口参数赋值
        /// </summary>
        /// <param name="db"></param>
        /// <param name="nva_id"></param>
        /// <param name="task_id"></param>
        private void NavWebactionCommon(DataBase db, int nva_id, int task_id)
        {
            _db = db;
            _nva_id = nva_id;
            _task_id = task_id;
            sendmsg = new NavSendMessage(_nva_id, _db);

            _ipaddress = "127.0.0.1";
            int.TryParse(ConfigurationSettings.AppSettings["LocalUdbPort"], out _portnum);
        }
        /// <summary>
        ///飞机发送命令接口的构造函数:通话模块使用 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="nva_id"></param>
        /// <param name="task_id"></param>
        /// <param name="callModule">callModule</param>
        public NavWebaction(DataBase db, int nva_id, int task_id, string callModule)
        {
            _callModule = callModule;
            NavWebactionCommon(db, nva_id, task_id);
        }

        #region 根据飞机ID获取基站的位置及发送接收的IP地址端口号
        /// <summary>
        /// 获取基站信息
        /// </summary>
        private void GetBaseStionInfo()
        {
            try
            {

                DataTable dt = new DataTable();
                //sp_Nav_Get_FlySignals
                SqlCommand sqlcom = new SqlCommand();
                sqlcom.CommandText = @"SELECT A.*
                                        FROM dbo.D_Uav_BaseStation AS A
                                        INNER JOIN dbo.D_Uav_UavSet AS B ON A.ID = B.BaseStationID
                                        WHERE A.ID = @nva_id";
                sqlcom.CommandType = System.Data.CommandType.Text;
                sqlcom.Parameters.Add("@nva_id", SqlDbType.Int);
                sqlcom.Parameters["@nva_id"].Value = _nva_id;//飞机的ID 
                dt = _db.ExecuteDataTable(sqlcom);
                if (dt != null && dt.Rows.Count > 0)
                {
                    //if (!string.IsNullOrEmpty(_callModule))
                    //{
                    //    _ipaddress = dt.Rows[0]["SendCallIPaddress"].ToString();
                    //    _portnum = Convert.ToInt16(dt.Rows[0]["SendCallPortNum"]);
                    //}
                    //else
                    //{
                    //    _ipaddress = dt.Rows[0]["SendMsgIPaddress"].ToString();
                    //    _portnum = Convert.ToInt16(dt.Rows[0]["SendMsgPortNum"]);
                    //}


                    _baseLongitude = Convert.ToDecimal(dt.Rows[0]["Longitude"]);
                    _baseLatitude = Convert.ToDecimal(dt.Rows[0]["Latitude"]);
                }
            }
            catch (Exception ex) { throw ex; }
        }

        #endregion

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="bytes"></param>
        private void SendData(byte[] bytes)
        {
            //System.Threading.Thread.Sleep(300);  //300毫秒
            System.Threading.Thread.Sleep(100);  //100毫秒
            //设置服务IP，设置TCP端口号
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(_ipaddress), _portnum);

            //定义网络类型，数据连接类型和网络协议UDP
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.SendTo(bytes, bytes.Length, SocketFlags.None, ip);
        }

        #region 航线下载
        /// <summary>
        /// 航线下载
        /// </summary>
        /// <param name="airlinecode">航线编码</param>
        /// <param name="pointcode">航点编号</param>
        /// <param name="msg">如果出错则返回错误信息，否则为空</param>
        /// <returns>成功：true 失败：false</returns>
        public bool Nav_Down_Line(byte airlinecode, int pointcode, out string msg)
        {
            msg = "";
            bool flag = true;

            try
            {
                byte[] mybyte = sendmsg.Nav_AirLineDownload(airlinecode, pointcode, out msg);
                if (string.IsNullOrEmpty(msg))
                {
                    SendData(mybyte);
                }
                else
                {
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }
        #endregion

        #region 控制界面<=0.获取遥测信息 =1.设置/更新航线(航点)=2.起飞=3.返航=4.降落=5.盘旋=6.飞机控制 升高、下降、左飞、右飞=7.云台控制(摄像机)=8.获取控制权==============================>

        #region 0.获取遥测信息
        /// <summary>
        /// 获取飞机的遥测信号数据
        /// </summary>
        /// <param name="onlylastone">是否只有最后一条，true只有最后一条</param>
        /// <param name="msg">如果出错返回错误信息</param>
        /// <returns></returns>
        public DataTable Nav_Get_FlySignals(bool onlylastone,out string msg)
        {
            msg = string.Empty;
            DataTable dt = new DataTable();
            try
            {
                //sp_Nav_Get_FlySignals
                SqlCommand sqlcom = new SqlCommand();
                sqlcom.CommandText = @"IF @onlylastone = 0
	                                    BEGIN
		                                    SELECT * FROM Uav_Track WHERE UavID=@nva_id AND Isolddata = 0 
	                                    END
                                        ELSE  
	                                    BEGIN
		                                    SELECT TOP 1 * FROM Uav_Track WHERE UavID=@nva_id AND Isolddata = 0 ORDER BY ID DESC
	                                    END";
                sqlcom.CommandType = System.Data.CommandType.Text;
                sqlcom.Parameters.Add("@nva_id", SqlDbType.Int);
                sqlcom.Parameters["@nva_id"].Value = _nva_id;//飞机的ID 
                sqlcom.Parameters.Add("@onlylastone", SqlDbType.Bit);
                sqlcom.Parameters["@onlylastone"].Value = onlylastone;//是否只返回最新的 
                dt = _db.ExecuteDataTable(sqlcom);
            }
            catch(Exception ex)
            {
                msg = ex.Message;
                return null;
            }

            return dt;
        }
        #endregion

        #region 1.设置/更新航线(航点)
        /// <summary>
        /// 设置航线 航线编码 0：回家航线  1~9：普通用户航线  10：降落航线  11：盘旋航线
        /// 0:回家航线 为从当前结束点到基站坐标点的一条航线，如果不设置此航线，点击返航时，飞机会直线飞回基站位置
        /// 1~9：客户自定义航线，默认规定飞行顺序为从1航线开始进行起飞，也就是起飞后先执行1航线，然后按顺序执行下去
        /// 10:降落航线 为一个基站航点的信息，切换到降落航线的时候，飞机会自动降落到该点
        /// </summary>
        /// <param name="line">航线的对象</param>
        /// <param name="msg">错误信息</param>
        /// <returns>成功：true 失败：false</returns>
        public bool Nav_Set_Line(AirlineModel line, out string msg)
        {
            msg = "";
            bool flag = true;
            try
            {
                List<byte[]> mybytes = sendmsg.Nav_WaypointSet(line, out msg);
                if (string.IsNullOrEmpty(msg))
                {
                    foreach (byte[] mybyte in mybytes)
                    {
                        
                        SendData(mybyte);
                    }

                }
                else
                {
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }
        #endregion

        #region 2.起飞
        /// <summary>
        /// 飞机起飞
        /// </summary>
        /// <param name="isonlyoff">true:巡航段 false:起飞段</param>
        /// <returns></returns>
        public bool Nav_Control_TakeOff(bool isonlyoff,out string msg)
        {
            msg = string.Empty;
            bool flag = true;
            try
            {
                //起飞之前，验证当前遥测信号处于可起飞的状态
                DataTable dt_Takeoff = Nav_Get_FlySignals(true, out msg);//获取最新的遥测信号
                if (string.IsNullOrEmpty(msg) == false)
                { return false; }
                if (dt_Takeoff != null && dt_Takeoff.Rows.Count > 0)
                {
                    //if (isonlyoff == false)
                    //{//起飞定高
                    //    if (dt_Takeoff.Rows[0]["Standby_isOff"].ToString().ToUpper() == "FALSE" || dt_Takeoff.Rows[0]["Standby_isOff"].ToString().ToUpper() == "0")
                    //    {
                    //        msg = "飞机遥测显示起飞准备状态尚未完成，不能起飞.";
                    //        return false;
                    //    }
                    //}
                    //else
                    //{ //开始巡航
                    //    if (Convert.ToDecimal(dt_Takeoff.Rows[0]["GroundHeight"] == null || dt_Takeoff.Rows[0]["GroundHeight"].ToString() == "" ? "0" : dt_Takeoff.Rows[0]["GroundHeight"].ToString()) < 3)
                    //    {
                    //        msg = "飞机当前尚未处于3m以上，不能巡航请确认.";
                    //        return false;
                    //    }
                    //}
                }
                else
                {
                    msg = "未获取到任何遥测信号.";
                    return false;
                }
                //开始发送命令
                byte[] mybyte = sendmsg.Nav_TakeOff(!isonlyoff, out msg);
                SendData(mybyte);
            }
            catch(Exception ex){
                msg = ex.Message;
                flag = false;
            }

            return flag;
        }
        #endregion

        #region 3.返航
        /// <summary>
        /// 飞机返航
        /// </summary>
        /// <returns></returns>
        public bool Nav_Control_GoHome(out string msg)
        {
            msg = string.Empty;
            bool flag = true;
            try
            {
                /*
                 * 1.如果飞机遥测信号已经在返航或者降落航线了，则不需要不允许再次处理
                 * 2.如果飞机并未开始返航航线，则需要处理以下问题 
                 *  2.1：飞机当前位置与返航航线第一个点距离比较近，则可以直接开始第一个航点
                 *  2.2：飞机当前位置与基站的位置更加的靠近一些，且在一个方位，则可以直接从其他靠近的航点开始执行
                 *  2.2：飞机当前位置与基站的位置更加的靠近一些，但是在一个方位，则整个返航的航线都需要更新
                */
                
                #region 1.验证飞机是否已经处于飞行返航或者降落航线
                DataTable dt_YaoCe = Nav_Get_FlySignals(true, out msg);//获取最新的遥测信号
                if (string.IsNullOrEmpty(msg)==false)
                { return false; }
                if (dt_YaoCe != null && dt_YaoCe.Rows.Count > 0)
                {
                    if (dt_YaoCe.Rows[0]["CurrentLine"].ToString() == "0" || dt_YaoCe.Rows[0]["CurrentLine"].ToString() == "8")
                    {
                        msg = "飞机目前处于返航/降落航线，无需发送返航.";
                        return false;
                    }
                }
                else
                {
                    msg = "未获取到任何遥测信号.";
                    return false;
                }
                #endregion

                #region 2.其他验证后的处理,比如是否需要更新航线，是否需要重新规划航线等
                #endregion
                
                #region 3.切换到回家航线,规定8号航线为回家航线--实际飞机飞的时候，要切换到航线9
                flag = Nav_Change_Line(8, 1, out msg);
                #endregion
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                flag = false;
            }

            return flag;
        }
        #endregion

        #region 4.降落
        /// <summary>
        /// 飞机降落
        /// </summary>
        /// <returns></returns>
        public bool Nav_Control_TouchDown(out string msg)
        {
            msg = string.Empty;
            bool flag = true;
            try
            {
                #region 1.验证飞机是否已经处于飞行返航或者降落航线
                DataTable dt_YaoCe = Nav_Get_FlySignals(true, out msg);//获取最新的遥测信号
                if (string.IsNullOrEmpty(msg) == false)
                { return false; }
                if (dt_YaoCe != null && dt_YaoCe.Rows.Count > 0)
                {
                    if (dt_YaoCe.Rows[0]["CurrentLine"].ToString() == "0")
                    {
                        msg = "飞机目前非处于返航航线，不能进行降落.";
                        return false;
                    }
                    if (dt_YaoCe.Rows[0]["CurrentLine"].ToString() == "10")
                    {
                        msg = "飞机目前非处于降落航线，无需发送降落.";
                        return false;
                    }
                }
                else
                {
                    msg = "未获取到任何遥测信号.";
                    return false;
                }
                #endregion

                #region 2.其他验证后的处理,比如是否需要更新航线，是否需要重新规划航线等
                #endregion

                #region 3.切换到降落航线 0 为降落航线
                flag = Nav_Change_Line(0, 1, out msg);
                #endregion
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                flag = false;
            }

            return flag;
        }
        #endregion

        #region 5.盘旋
        /// <summary>
        /// 飞机盘旋
        /// </summary>
        /// <param name="CycleNum">盘旋圈数</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Nav_Control_Cycle(byte CycleNum,byte Cycleradius,out string msg)
        {
            msg = string.Empty;
            bool flag = true;
            try
            {
                if (CycleNum <= 0 || CycleNum >= 250)
                {
                    msg = "盘旋圈数只能为1-250之间.";
                    return false;
                }
             
               
                #region 1.验证飞机是否已经处于飞行返航或者降落航线
                DataTable dt_YaoCe = Nav_Get_FlySignals(true, out msg);//获取最新的遥测信号
                if (string.IsNullOrEmpty(msg) == false)
                { return false; }
                if (dt_YaoCe != null && dt_YaoCe.Rows.Count > 0)
                {
                    //if (dt_YaoCe.Rows[0]["CurrentLine"].ToString() == "10")
                    //{
                    //    msg = "飞机目前降落航线，不能进行盘旋.";
                    //    return false;
                    //}
                    if (dt_YaoCe.Rows[0]["CurrentLine"].ToString() == "11")
                    {
                        msg = "飞机目前正处于盘旋航线，若要再次盘旋，请先停止.";
                        return false;
                    }
                }
                else
                {
                    msg = "未获取到任何遥测信号.";
                    return false;
                }
                #endregion

                #region 2.其他验证后的处理,比如是否需要更新航线，是否需要重新规划航线等
                #endregion

                NavCycleobj obj = new NavCycleobj();
                obj.CycleNum = CycleNum;
                obj.Longitude = Convert.ToInt64(Convert.ToDecimal(dt_YaoCe.Rows[0]["Loction_Longitude"]) * 1000000);
                obj.Latitude = Convert.ToInt64(Convert.ToDecimal(dt_YaoCe.Rows[0]["Loction_Latitude"]) * 1000000);
                obj.Cycleradius = 200;
                obj.Cycletype = 0;//盘旋跟踪某一个目标
                obj.Boatincremental = 0; //度数
                obj.Boatdistnct = 0;//暂时不知道如何使用
                if (obj.Longitude == 0 || obj.Latitude == 0)
                {
                    msg = "请确认盘旋中心点坐标正确.";
                    return false;
                }
                #region 3.切换到盘旋航线
                flag = Nav_Change_Line(obj, out msg);
                #endregion
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                flag = false;
            }

            return flag;
        }

        #endregion

        #region 6.飞机控制 升高、下降、左飞、右飞
        /// <summary>
        /// 飞机控制 升高、下降、左飞、右飞
        /// </summary>
        /// <param name="num">控制值数值，改变方向时，此数值为移动的度数 ° 改变高度时，为m数</param>
        /// <param name="control">控制的类型</param>
        /// <param name="boatdistnct">目标距离</param> 
        /// <param name="msg">如果出现错误，则赋值否则为空</param>
        /// <returns>成功：true 失败：false</returns>
        public bool Nav_Control_Uav(int Degrees, ControlType control, int boatdistnct, out string msg)
        {
            msg = "";
            bool flag = true;
            try
            {
                //限制去掉 升高、下降指令：会先左飞 0 °，切出当前航线
                //if (Degrees <= 0)
                //{
                //    msg = "改变的值小于等于0";
                //    return false;
                //}
                DataTable dt_YaoCe = Nav_Get_FlySignals(true, out msg);//获取最新的遥测信号
                #region 获取飞机最新的状态
                if (string.IsNullOrEmpty(msg) == false)
                { return false; }
                if ((dt_YaoCe != null && dt_YaoCe.Rows.Count > 0)==false)
                {
                    msg = "未获取到任何遥测信号.";
                    return false;
                }
                #endregion

                if (control == ControlType.升高)
                {
                    if (Degrees >= 300)
                    {
                        msg = "飞机升高时一次最度改变300m";
                        return false;
                    }
                }
                else if (control == ControlType.下降)
                {
                    if (Degrees >= 150)
                    {
                        msg = "飞机下降时一次最多改变150m";
                        return false;
                    }
                    if (Convert.ToInt16(dt_YaoCe.Rows[0]["Height"]) - Degrees < 15)
                    {
                        msg = "下降高度过大，目标高度超过飞机安全高度.";
                        return false;
                    }
                    Degrees = 0 - Degrees;
                }
                else if (control == ControlType.右飞)
                {
                    //if (Degrees > 5)
                    //{
                    //    msg = "改变航向时，一次最多调整5°";
                    //    return false;
                    //}
                }
                else if (control == ControlType.左飞)
                {
                    //if (Degrees > 5)
                    //{
                    //    msg = "改变航向时，一次最多调整5°";
                    //    return false;
                    //}
                    Degrees = 0 - Degrees;
                }
                if (control == ControlType.升高 || control == ControlType.下降)
                {
                    flag = Nav_Control_changeheight(Degrees, out msg);
                }
                else
                {
                    NavCycleobj obj = new NavCycleobj();
                    obj.CycleNum = 3;//盘旋圈数完成后，飞机还会回归航线 指定盘旋3圈
                    obj.Longitude = Convert.ToInt64(Convert.ToInt64(dt_YaoCe.Rows[0]["Loction_Longitude"]) * 0.000001);
                    obj.Latitude = Convert.ToInt64(Convert.ToInt64(dt_YaoCe.Rows[0]["Loction_Latitude"]) * 0.000001);
                    obj.Cycleradius = 0;
                    obj.Cycletype = 2;//改变飞行航向
                    //obj.Boatincremental = (short)(0.1 * Degrees);//度数
                    obj.Boatincremental = (short)(Degrees); //度数
                    obj.Boatdistnct = (short)boatdistnct;//目标距离 单位：m
                    flag = Nav_Change_Line(obj, out msg);
                }

            }
            catch (Exception ex)
            {
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }
        #endregion

        #region 7.云台控制(摄像机)
        /// <summary>
        /// 云台控制命令-摄像机 高 降 左 右 
        /// </summary>
        /// <param name="num">控制值数值，此数值为移动的度数 °</param>
        /// <param name="control">控制的类型</param>
        /// <param name="msg">如果出现错误，则赋值否则为空</param>
        /// <returns>成功：true 失败：false</returns>
        public bool Nav_Control_Camera(byte Degrees, CameraControlType control, out string msg)
        {
            msg = "";
            bool flag = true;
            try
            {
                byte[] mybyte = sendmsg.Nav_ControlNav(Degrees, control, out msg);
                if (string.IsNullOrEmpty(msg))
                {
                    SendData(mybyte);
                }
                else
                {
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }
        #endregion

        #region 8.获取控制权
        /// <summary>
        /// 获取控制权
        /// </summary>
        /// <returns></returns>
        public bool Nav_Control_GetControl(out string msg)
        {
            msg = string.Empty;
            bool flag = true;
            try
            {

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                flag = false;
            }

            return flag;
        }
        #endregion

        #endregion


        #region 具体发送处理接口 --勿删
        #region 高度改变--勿删
        /// <summary>
        /// 改变相对高度命令
        /// </summary>
        /// <param name="num">增高的相对高度,如果是降低高度则给 -值 负值</param>
        /// <param name="msg">失败时返回失败的原因，默认为""</param>
        /// <returns>返回成功的byte数组,出错时为null</returns>
        private bool Nav_Control_changeheight(int num, out string msg)
        {
            msg = "";
            bool flag = true;
            try
            {
                byte[] mybyte = sendmsg.Nav_ChangeHigh(num, out msg);
                if (string.IsNullOrEmpty(msg))
                {
                    SendData(mybyte);
                }
                else
                {
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }

        #endregion

        #region 控制相关方法 --勿删
        /// <summary>
        /// 云台控制命令-升高 下降 左飞 右飞 
        /// </summary>
        /// <param name="num">控制值数值，此数值为移动的度数 °</param>
        /// <param name="control">控制的类型</param>
        /// <param name="msg">如果出现错误，则赋值否则为空</param>
        /// <returns>成功：true 失败：false</returns>
        private bool Nav_Control_up(byte Degrees, CameraControlType control, out string msg)
        {
            msg = "";
            bool flag = true;
            try
            {
                byte[] mybyte = sendmsg.Nav_ControlNav(Degrees, control, out msg);
                if (string.IsNullOrEmpty(msg))
                {
                    SendData(mybyte);
                }
                else
                {
                    flag = false;
                }
            }
            catch(Exception ex){
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }

         
        #endregion

        #region 航线切换的方法-勿删
        /// <summary>
        /// 切换航线 到某一个航线的某一个航点
        /// </summary>
        /// <param name="lineno">航线编号</param>
        /// <param name="pointno">航点编号</param>
        /// <param name="msg">错误信息</param>
        /// <returns>成功：true 失败：false</returns>
        public bool Nav_Change_Line(byte lineno,int pointno, out string msg)
        {
            msg = "";
            bool flag = true;
            try
            {
                byte[] mybyte = sendmsg.Nav_AirpointSwith(lineno, pointno, out msg);
                if (string.IsNullOrEmpty(msg))
                {
                    SendData(mybyte);
                }
                else
                {
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }
        #endregion

        #region 盘旋指令的方法--勿删
        /// <summary>
        /// 盘旋切换指令  
        /// </summary>
        /// <param name="cycle">盘旋信息</param>
        /// <param name="msg">错误信息</param>
        /// <returns>成功：true 失败：false</returns>
        public bool Nav_Change_Line(NavCycleobj cycle, out string msg)
        {
            msg = "";
            bool flag = true;
            try
            {
                byte[] mybyte = sendmsg.Nav_CycleSwith(cycle, out msg);
                if (string.IsNullOrEmpty(msg))
                {
                    SendData(mybyte);
                }
                else
                {
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }
        #endregion
        #endregion


        #region 王斯处理
        #region 固定翼模式/四旋翼模式  起飞、返航、悬停、盘旋、降落、获取控制 功能按钮
        /// <summary>
        /// 飞机功能按钮控制
        /// </summary>
        /// <param name="control">
        /// 获取控制=0, 起飞=1, 返航=2, 盘旋=3, 悬停=4, 降落=5
        /// </param>
        /// <param name="msg">错误信息</param>
        /// <returns>成功：true 失败：false</returns>
        public static bool Nav_Change_Control(FunctionBtnControlType control, out string msg) 
        {
            msg = "";
            bool flag = true;
            try
            {

            }
            catch (Exception ex)
            {
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }

        #endregion

        #region 相机拍摄角度控制
        /// <summary>
        /// 相机角度控制
        /// </summary>
        /// <param name="control">
        /// 上调 = 1, 下调 = 2, 左调 = 3, 右调 = 4
        /// </param>
        /// <param name="msg">错误信息</param>
        /// <returns>成功：true 失败：false</returns>
        public static bool Camera_Change_Control(DataBase db,CameraControlType control, out string msg)
        {
            msg = "";
            bool flag = true;
            try
            {
                int _taskID = 0;

                string sql = "SELECT  BILLID,FlyWay,ShipId FROM dbo.D_Uav_MainTask WHERE UavSetID = @Uav_id AND TaskStatus = 3 AND ISDISABLED=0";
                SqlCommand sqlcom = new SqlCommand();
                sqlcom.CommandText = sql;
                sqlcom.CommandType = System.Data.CommandType.Text;
                sqlcom.Parameters.Add("@Uav_id", SqlDbType.Int);
                sqlcom.Parameters["@Uav_id"].Value = 1;//飞机的ID 默认给1
                DataTable dt = db.ExecuteDataTable(sqlcom);//获取要处理的任务信息
                if (dt != null && dt.Rows.Count > 0)
                {
                    _taskID = Convert.ToInt32(dt.Rows[0]["BILLID"]);
                }
                else
                {//没有找到任务ID，则直接返回
                    return false;
                }

                NavWebaction nav = new NavWebaction(db, 1, _taskID);

                CameraControlType cam = new CameraControlType();
                if(control.GetHashCode().ToString()=="1")
                {
                    cam = CameraControlType.上调;
                }else  if(control.GetHashCode().ToString()=="2")
                {
                    cam = CameraControlType.下调;
                }
                else  if(control.GetHashCode().ToString()=="3")
                {
                    cam = CameraControlType.左调;
                }
                else  if(control.GetHashCode().ToString()=="4")
                {
                    cam = CameraControlType.右调;
                }
                else if (control.GetHashCode().ToString() == "5")
                {//归中
                    cam = CameraControlType.上调;
                    nav.Nav_Control_Camera(0, cam, out msg);
                    cam = CameraControlType.左调;
                    nav.Nav_Control_Camera(0, cam, out msg);
                    return true;
                }
                else if (control.GetHashCode().ToString() == "6")
                {//变倍加
                    cam = CameraControlType.变倍加; 
                }
                else if (control.GetHashCode().ToString() == "7")
                {//变倍减
                    cam = CameraControlType.变倍减;
                }
               
                nav.Nav_Control_Camera(40, cam, out msg);
            }
            catch (Exception ex)
            {
                flag = false;
                msg = ex.Message;
            }
            return flag;
        }
        #endregion
        #endregion

    }
}
