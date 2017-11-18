using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLP.NAV
{
    /// <summary>
    /// 数据解析后的对象
    /// </summary>
    public class NavMessageObj
    {
        #region 主体信息
        /// <summary>
        /// A 同步码 第一个字节
        /// </summary>
        public string SynchronousCode1 { get; set; }

        /// <summary>
        /// A 同步码 第二个字节
        /// </summary>
        public string SynchronousCode2 { get; set; }

        /// <summary>
        /// B 帧长度 目前为136
        /// </summary>
        public byte? FrameLength { get; set; }

        /// <summary>
        /// C 高 4 位 目的地址 00H 测控副站，01H 测控主站，02H 目标 1，03H 目标 2，04H 目标 3，0FH 广播地址；
        /// </summary>
        public string TargetAddress { get; set; }

        /// <summary>
        /// C 低 4 位源地址 00H 测控副站，01H 测控主站，02H 目标 1，03H 目标 2，04H 目标 3，0FH 广播地址；
        /// </summary>
        public string SourceAddress { get; set; }

        /// <summary>
        /// D 数据类型；1 字节，2AH 表示遥测数据帧；
        /// </summary>
        public string FrameDataType { get; set; }

        /// <summary>
        /// E 帧编号，1 字节；
        /// </summary>
        public string FrameCode { get; set; }

        /// <summary>
        /// F1 备用，2 字节；
        /// </summary>
        public string StandyData1 { get; set; }

        /// <summary>
        /// F2 备用，2 字节；
        /// </summary>
        public string StandyData2 { get; set; }
        #endregion

        #region 第一副帧需要用到的数据，主要是速度 高度 目标等信息
        /// <summary>
        /// 当前俯仰角-- 单位为0.1°度 
        /// </summary>
        public int? PitchingAngle { get; set; }

        /// <summary>
        /// 目标俯仰角-- 单位为0.1°度 
        /// </summary>
        public int? TargetPitchingAngle { get; set; }

        /// <summary>
        /// 当前滚转角-- 单位为0.1°度 
        /// </summary>
        public int? RollAngle { get; set; }

        /// <summary>
        /// 目标滚转角-- 单位为0.1°度 
        /// </summary>
        public int? TargetRollAngle { get; set; }

        /// <summary>
        /// 当前机头方位角-- 单位为0.1°度 
        /// </summary>
        public int? NoseAzimuth { get; set; }

        /// <summary>
        /// 目标机头方位角-- 单位为0.1°度 
        /// </summary>
        public int? TargetNoseAzimuth { get; set; }

        /// <summary>
        /// GPS 航迹向-- 单位为0.1°度 
        /// </summary>
        public int? GPSTrack { get; set; }

        /// <summary>
        /// 当前对地高度--单位为 0.1m 米
        /// </summary>
        public int? GroundHeight { get; set; }

        /// <summary>
        /// 目标对地高度--单位为 m 米
        /// </summary>
        public int? TargetGroundHeight { get; set; }

        /// <summary>
        /// 当前地速--单位为 m/s 米/每秒
        /// </summary>
        public byte GroundSpeed { get; set; }

        /// <summary>
        /// 当前空速--单位为 m/s 米/每秒
        /// </summary>
        public byte AirSpeed { get; set; }

        /// <summary>
        /// 目标速度--单位为 m/s 米/每秒
        /// </summary>
        public byte TargetSpeed { get; set; }

        /// <summary>
        /// 升降速率--单位为 0.1 m/s 米/每秒
        /// </summary>
        public byte LiftingRate { get; set; }
        #endregion

        #region 第二副帧需要用到的数据，主要是控制、报警、盘旋、开关等状态信息

        /// <summary>
        /// 控制状态--0 手动遥控 1 半自主 2 全自主
        /// </summary>
        public int? ControlMode { get; set; }

        /// <summary>
        /// 飞行器模态 0：固定翼 1：过渡期 2：多旋翼
        /// </summary>
        public int? VehicleModal { get; set; }

        /// <summary>
        /// 系统状态 0：正常飞行状态 1：校准状态 2：保护状态
        /// </summary>
        public int? SystemStatus { get; set; }

        /// <summary>
        /// 飞行阶段标志 0：巡航段 1：起飞段 2：降落段 3：地面等待
        /// </summary>
        public int? FlightPhase { get; set; }

        /// <summary>
        /// 报警状态-主电压低 0：正常 1：报警
        /// </summary>
        public bool Alarm_MainV_Low { get; set; }

        /// <summary>
        /// 报警状态-舵机电压低 0：正常 1：报警
        /// </summary>
        public bool Alarm_ServoV_Low { get; set; }

        /// <summary> 
        /// 报警状态-电流低 0：正常 1：报警
        /// </summary>
        public bool Alarm_Electricity_Low { get; set; }


        /// <summary>
        /// 报警状态-状态监控 1 0：正常 1：报警
        /// </summary>
        public bool Alarm_Status1 { get; set; }

        /// <summary>
        /// 报警状态-转速异常 0：正常 1：报警
        /// </summary>
        public bool Alarm_AbnormalSpeed { get; set; }

        /// <summary>
        /// 报警状态-空速异常 0：正常 1：报警
        /// </summary>
        public bool Alarm_SpaceVelocityAnomaly { get; set; }

        /// <summary>
        /// 报警状态-状态监控 2 0：正常 1：报警
        /// </summary>
        public bool Alarm_Status2 { get; set; }

        /// <summary>
        /// 遥控解锁状态--0 解锁 1 未解锁
        /// </summary>
        public bool ControlUnlocked { get; set; }

        /// <summary>
        /// 报警状态-GPS 定位精度低 0：正常 1：报警
        /// </summary>
        public bool Alarm_GPSLoction_Low { get; set; }

        /// <summary>
        /// 报警状态-航姿传感器状态 0：正常 1：报警
        /// </summary>
        public bool Alarm_PositionSensor { get; set; }

        /// <summary>
        /// 报警状态-高度报警 0：正常 1：报警
        /// </summary>
        public bool Alarm_GrandHigh { get; set; }

        /// <summary>
        /// 报警状态-超出安全围栏 0：正常 1：报警
        /// </summary>
        public bool Alarm_BeyondSecurityFence { get; set; }

        /// <summary>
        /// 报警状态-遥控 RC 超距 0：正常 1：报警
        /// </summary>
        public bool Alarm_ControlRCDistance { get; set; }

        /// <summary>
        /// 报警状态-升降速度报警 0：正常 1：报警
        /// </summary>
        public bool Alarm_LiftingSpeed { get; set; }

        /// <summary>
        /// 报警状态-遥控上行状态 0：正常 1：报警
        /// </summary>
        public bool Alarm_ControlUpward { get; set; }

        /// <summary>
        /// 报警状态-飞控硬件状态 0：正常 1：报警
        /// </summary>
        public bool Alarm_FlightControlHardware  { get; set; }


        /// <summary>
        /// 开关--发电机并网 0 无效 1 并网 改为拦阻钩 0 收起 1 放下
        /// </summary>
        public bool Switch_GeneratorInterconnection { get; set; }

        /// <summary>
        /// 开关--发动机启动 0 无效 1 启动
        /// </summary>
        public bool Switch_EngineStart { get; set; }

        /// <summary>
        /// 开关--盘旋 0 无效 1 盘旋
        /// </summary>
        public bool Switch_Circle { get; set; }

        /// <summary>
        /// 开关--归航 0 无效 1 归航 
        /// </summary>
        public bool Switch_Homing { get; set; }

        /// <summary>
        /// 开关--关车 0 无效 1 关车
        /// </summary>
        public bool Switch_Closing { get; set; }

        /// <summary>
        /// 开关--襟翼 0 收起 1 放下
        /// </summary>
        public bool Switch_PutDownFlaps { get; set; }

        /// <summary>
        /// 开关--开伞 0 无效 1 开伞
        /// </summary>
        public bool Switch_OpenUmbrella { get; set; }

        /// <summary>
        /// 开关--夜航灯 0 关闭 1 打开
        /// </summary>
        public bool Switch_NightLights { get; set; }

        /// <summary>
        /// 当前盘旋圈数 单位：圈 范围：0~255
        /// </summary>
        public byte CurrentCircles { get; set; }

        /// <summary>
        /// 目标盘旋圈数 单位：圈 范围：0~250 若该值>250 圈，则一直盘旋
        /// </summary>
        public byte TargetCircles { get; set; }

        /// <summary>
        /// 拍照次数 单位：次 范围：0~65535
        /// </summary>
        public int? TakePictures { get; set; } 


        #endregion

        #region 第三副帧需要用到的数据，主要是经纬度、航线、目标距离、GPS时间等信息
        /// <summary>
        /// 经度 单位： 度0.000001° 范围：-180000000～180000000
        /// </summary>
        public long Loction_Longitude { get; set; }

        /// <summary>
        /// 纬度 单位：度0.000001° 范围：-180000000～180000000
        /// </summary>
        public long Loction_Latitude { get; set; }

        /// 高度 单位：m 米 非对地
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 侧偏距离--0.1m 米
        /// </summary>
        public int? DistanceCornering { get; set; }

        /// <summary>
        /// 目标距离--0.1m 米
        /// </summary>
        public int? DistanceTarget { get; set; }


        /// <summary>
        /// 原点距离--m 米
        /// </summary>
        public int? DistanceOrigin { get; set; }

        /// <summary>
        /// 当前航线 1~10：普通用户航线  0：返航航线 11:盘旋航线
        /// </summary>
        public int? CurrentLine { get; set; }

        /// <summary>
        /// 目标航点  与 Data[17]的 bit6~ bit7 组成 10位数 bit6 对应目标航点编号的 bit8，bit7 对应目标航点编号的 bit9
        /// </summary>
        public string CurrentTargetPoint { get; set; }

        /// <summary>
        /// GPS 时 单位：小时 范围：0~23（北京时间）
        /// </summary>
        public byte GPS_H { get; set; }

        /// <summary>
        ///GPS 分 单位：分钟 范围：0~59
        /// </summary>
        public byte GPS_M { get; set; }

        /// <summary>
        ///GPS 秒 单位：秒 范围：0~59
        /// </summary>
        public byte GPS_S { get; set; }

        /// <summary>
        ///是否起飞准备ok 
        /// </summary>
        public bool Standby_isOff { get; set; }
        #endregion
    }

    /// <summary>
    /// 控制状态
    /// </summary>
    public enum ControlMode
    { 
        手动遥控 = 0,
        半自主 = 1,
        全自主 = 2 
    }
    /// <summary>
    /// 飞行器模态
    /// </summary>
    public enum VehicleModal
    {
        固定翼 = 0,
        过渡期 = 1,
        多旋翼 = 2
    }

    /// <summary>
    /// 系统状态
    /// </summary>
    public enum SystemStatus
    {
        正常飞行状态 = 0,
        校准状态 = 1,
        保护状态 = 2
    }

    /// <summary>
    /// 飞行阶段标志
    /// </summary>
    public enum FlightPhase
    {
        巡航段 = 0,
        起飞段 = 1,
        降落段 = 2,
        地面等待 = 3
    }
}
