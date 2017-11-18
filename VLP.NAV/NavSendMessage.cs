using Nordasoft.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace VLP.NAV
{
    /// <summary>
    /// 发送飞机控制数据解析的方法
    /// </summary>
    public class NavSendMessage
    {
        /// <summary>
        /// 飞机的id,数据表中的ID
        /// </summary>
        private int _nav_id = -1;
        public int nav_id
        {
            get { return _nav_id; }
            set { _nav_id = value; }
        }
        /// <summary>
        /// 数据库连接db
        /// </summary>
        private DataBase _db;
        /// <summary>
        /// 飞机的编码 目前写死为0xFF高4位即15
        /// </summary>
        private int _targetaddrees = 15;

        /// <summary>
        /// 基站的编码 目前写死为0xFF低4位即15
        /// </summary>
        private int _sourceaddrees = 15;

        /// <summary>
        /// 基站的纬度
        /// </summary>
        private long _base_Latitude = 0;

        /// <summary>
        /// 基站的经度
        /// </summary>
        private long _base_Longitude = 0;

        /// <summary>
        /// 数据的长度，目前为32位字节数组
        /// </summary>
        private int _dataLengh = 32;
      
        /// <summary>
        /// 构造方法，需要飞机的固定参数
        /// </summary>
        /// <param name="nva_id">飞机的ID</param>
        /// <param name="db">数据库连接db</param>
        /// <param name="msg">执行是否正确，如果错误则返回错误信息</param>
        public NavSendMessage(int nva_id,DataBase db)
        {
            _nav_id = nva_id;
            _db = db;
        }

        /// <summary>
        /// 根据飞机的ID，获取飞机需要的信息
        /// </summary>
        /// <returns></returns>
        private string getNav()
        {
            string errmsg = "";
            try
            {
                string sql = @"SELECT ISNULL(A.UavEdiCode,15) AS UavEdiCode,
                                      ISNULL(B.BaseEdiCode,15) AS BaseEdiCode,
	                                  B.Longitude,
	                                  B.Latitude 
                                FROM dbo.D_Uav_UavSet AS A
                                INNER JOIN dbo.D_Uav_BaseStation AS B ON A.BaseStationID = B.ID
                                WHERE A.ID = @nav_id";
                SqlCommand sqlcom = new SqlCommand();
                sqlcom.CommandText = sql;
                sqlcom.CommandType = System.Data.CommandType.Text;
                sqlcom.Parameters.Add("@nav_id", SqlDbType.Int);
                sqlcom.Parameters["@nav_id"].Value = _nav_id;
                DataTable dt = _db.ExecuteDataTable(sqlcom);
                if (dt != null && dt.Rows.Count > 0)
                {
                    _targetaddrees = (int)dt.Rows[0]["UavEdiCode"];
                    _sourceaddrees = (int)dt.Rows[0]["BaseEdiCode"];
                    _base_Latitude = putstringtolong(dt.Rows[0]["Latitude"]);
                    _base_Longitude = putstringtolong(dt.Rows[0]["Longitude"]);
                }
                else
                {
                    errmsg = "未获取到有效的飞机信息.飞机id:" + _nav_id.ToString();
                }
            }
            catch(Exception ex)
            {
                errmsg = ex.Message;
            }

            return errmsg;
        }

        /// <summary>
        /// 对string类型的坐标点，返回符合要求的long类型整型
        /// </summary>
        /// <returns></returns>
        private long putstringtolong(object obj)
        {
            long n = 0;
            if (obj == null)
            {
                return 0;
            }
            string str = obj.ToString();
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }
            try
            {
                if (str.Contains("."))
                {
                    string[] sp = str.Split('.');
                    if (sp[1].Length >= 6)
                    {//小数后大于6位则截取一下
                        n = Convert.ToInt64(sp[0] + sp[1].Substring(0, 5));
                    }
                    else
                    {
                        string bustr = "";
                        for (int i = 0; i < 6 - sp[1].Length; i++)
                        {
                            bustr += "0";
                        }
                        n = Convert.ToInt64(sp[0] + sp[1] + bustr);
                    }
                }
                else
                {
                    _base_Latitude = Convert.ToInt64(str + "000000");
                }
            }
            catch
            {
                throw;
            }

            return n;
        }

        #region 实际使用到的接口
        /// <summary>
        /// 改变相对高度命令
        /// </summary>
        /// <param name="num">增高的相对高度,如果是降低高度则给 -值 负值</param>
        /// <param name="msg">失败时返回失败的原因，默认为""</param>
        /// <returns>返回成功的byte数组,出错时为null</returns>
        public byte[] Nav_ChangeHigh(int num, out string msg)
        {
            if (Math.Abs(num) > 1000)
            {
                msg = "操作的高度不能超过1000";
                return null;
            }
            if (Math.Abs(num) ==0)
            {
                msg = "高度为0,无需动作.";
                return null;
            }
            byte[] mybtye = new byte[_dataLengh];//目前为32个字节
            msg = "";
            try
            {
                mybtye[0] = 0xEB;//第一位为0xEB
                mybtye[1] = 0x90;//第二位为0x90
                mybtye[2] = Convert.ToByte(_dataLengh);//第二位为0x20 固定长度32字节
                mybtye[3] = Convert.ToByte((Common.putintTo2string(_targetaddrees, 4) + Common.putintTo2string(_sourceaddrees, 4)), 2);//2个地址组成一个
                mybtye[4] = 0x00;
                mybtye[5] = 0x00;
                mybtye[6] = 0x00;
                mybtye[7] = 0x00;
                mybtye[8] = 0xED;//改变相对高度命令
                mybtye[9] = Convert.ToByte("01000000", 2);//bit6：高度增量有效位 其它位无效，保持为 0 其它位：无意义 0 无效  1 有效   实际就是0x40
                mybtye[10] = 0x00;
                mybtye[11] = 0x00;
                mybtye[12] = 0x00;
                mybtye[13] = 0x00;
                string numstr = Common.putintTo16string(num, 4);
                mybtye[14] = Convert.ToByte(numstr.Substring(2, 2), 16);//高度低字节
                mybtye[15] = Convert.ToByte(numstr.Substring(0, 2), 16);//高度高字节
                long n = 0;
                for (int i = 8; i <= 30; i++)
                {
                    n += mybtye[i];
                }
                mybtye[31] = Convert.ToByte(Common.putintTo16string(n, 2), 16);//校验和：Data[8]～Data[30]的累加和取最低字节  先累加再次

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }


            return mybtye;

        }

        /// <summary>
        /// 设置航点
        /// </summary>
        /// <param name="num"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public List<byte[]> Nav_WaypointSet(AirlineModel airline, out string msg)
        {
            List<byte[]> rebyte = new List<byte[]>();
            msg = "";
            if (airline == null)
            {
                msg = "传入的航线对象为Null.";
                return null;
            }
            if (airline.Airlinepoint == null)
            {
                msg = "航线下的航点对象List为Null.";
                return null;
            }
            try
            {
                byte[] mybtye = null;
                foreach (Airlinepoint pt in airline.Airlinepoint)
                {
                    mybtye = new byte[_dataLengh];//目前为32个字节
                    mybtye[0] = 0xEB;//第一位为0xEB
                    mybtye[1] = 0x90;//第二位为0x90
                    mybtye[2] = Convert.ToByte(_dataLengh);//第二位为0x20 固定长度32字节
                    mybtye[3] = Convert.ToByte((Common.putintTo2string(_targetaddrees, 4) + Common.putintTo2string(_sourceaddrees, 4)), 2);//2个地址组成一个
                    mybtye[8] = 0xE1;//航点设置 

                    string Airlinecode9 = Common.putintTo2string(airline.Airlinecode, 4);//航线 编码取低四位
                    string Aircynum10 = Common.putintTo2string(airline.Airlinepoint.Count, 10);//航点总数，取10位，低开始的8位给10，剩下的2位给29
                    string Airpointcode11 = Common.putintTo2string(pt.Airpointcode, 10);//航点编码，10位
                    mybtye[9] = Convert.ToByte(Airpointcode11.Substring(0, 2) + "0" + (airline.Aircycle ? "1" : "0") + Common.putintTo2string(airline.Airlinecode, 4), 2);//第11位航点转成二进制后的最高2位，正好是9这个二进制的最高二位
                    mybtye[10] = Convert.ToByte(Aircynum10.Substring(2, 8), 2);//航点总数，取10位，低开始的8位给10，剩下的2位给29
                    //mybtye[10] = Convert.ToByte(Aircynum10.Substring(0, 8), 2);//航点总数，取10位，高开始的8位给10，剩下的2位给29
                    mybtye[11] = Convert.ToByte(Airpointcode11.Substring(2, 8), 2);//航点编码二进制的低位4+高位5,6
                    mybtye[12] = Convert.ToByte((Common.putintTo2string(pt.Heightcontrol, 4) + Common.putintTo2string(pt.Lineswitch, 4)), 2);//航线切换  低四位：航线切换方式  高四位：高度控制方式 
                    string numstr = Common.putintTo16string(pt.Longitude, 8);
                    mybtye[13] = Convert.ToByte(numstr.Substring(6, 2), 16);//低字节
                    mybtye[14] = Convert.ToByte(numstr.Substring(4, 2), 16);//次低字节 
                    mybtye[15] = Convert.ToByte(numstr.Substring(2, 2), 16);//次高字节 
                    mybtye[16] = Convert.ToByte(numstr.Substring(0, 2), 16);//高字节  
                    numstr = Common.putintTo16string(pt.Latitude, 8);
                    mybtye[17] = Convert.ToByte(numstr.Substring(6, 2), 16);//低字节
                    mybtye[18] = Convert.ToByte(numstr.Substring(4, 2), 16);//次低字节 
                    mybtye[19] = Convert.ToByte(numstr.Substring(2, 2), 16);//次高字节 
                    mybtye[20] = Convert.ToByte(numstr.Substring(0, 2), 16);//高字节  
                    numstr = Common.putintTo16string(pt.height, 4);
                    mybtye[21] = Convert.ToByte(numstr.Substring(2, 2), 16);//低字节 
                    mybtye[22] = Convert.ToByte(numstr.Substring(0, 2), 16);//高字节  
                    mybtye[23] = pt.NextAreaSpeed;//下一段航速
                    mybtye[24] = Convert.ToByte(string.Format("000000{1}{0}", pt.Isok_vedio ? "1" : "0", pt.Isok_pictrue ? "1" : "0"), 2);//航点动作有效 
                    mybtye[25] = Convert.ToByte(string.Format("000000{1}{0}", pt.Isaction_vedio ? "1" : "0", pt.Isaction_pictrue ? "1" : "0"), 2);//航点动作 
                    mybtye[26] = pt.CycleNum;//盘旋圈数 
                    numstr = Common.putintTo16string(pt.PicDist, 4);
                    mybtye[27] = Convert.ToByte(numstr.Substring(2, 2), 16);//低字节 
                    mybtye[28] = Convert.ToByte(numstr.Substring(0, 2), 16);//高字节  
                    mybtye[29] = Convert.ToByte(Airpointcode11.Substring(0, 2) + "000001", 2);//第11位航点转成二进制后的最高2位，正好是9这个二进制的最高二位,默认全部下载
                    //mybtye[29] = Convert.ToByte(Aircynum10.Substring(8, 2) + "000001", 2);//第11位航点转成二进制后的最高2位，正好是9这个二进制的最高二位,默认全部下载
                    long n = 0;
                    for (int i = 8; i <= 30; i++)
                    {
                        n += mybtye[i];
                    }
                    mybtye[31] = Convert.ToByte(Common.putintTo16string(n, 2), 16);//校验和：Data[8]～Data[30]的累加和取最低字节  先累加再次
                    rebyte.Add(mybtye);//添加到list
                }
            }
            catch(Exception ex)
            {
                msg = ex.Message;
                return null;
            }

            return rebyte;
        }

        /// <summary>
        /// 航点切换命令
        /// </summary>
        /// <param name="airlinecode">航线编码</param>
        /// <param name="pointcode">航点编号</param>
        /// <param name="msg">如果出错则返回错误信息，否则为空</param>
        /// <returns>命令byte数组</returns>
        public byte[] Nav_AirpointSwith(byte airlinecode, int pointcode, out string msg)
        {
            byte[] mybtye = new byte[_dataLengh];//目前是32个字节
            msg = "";
            if (airlinecode > 11)
            {
                msg = "航线编码大于11,请确认是否存在此航线.";
                return null;
            }
            if (pointcode > 800)
            {
                msg = "航点编码大于800,请确认是否存在此航点.";
                return null;
            }

            try {
                mybtye[0] = 0xEB;//第一位为0xEB
                mybtye[1] = 0x90;//第二位为0x90
                mybtye[2] = Convert.ToByte(_dataLengh);//第二位为0x20 固定长度32字节
                mybtye[3] = Convert.ToByte((Common.putintTo2string(_targetaddrees, 4) + Common.putintTo2string(_sourceaddrees, 4)), 2);//2个地址组成一个
                mybtye[8] = 0xE3;//航点切换指令
                string numstr = Common.putintTo2string(pointcode, 10);//取航点编号的10位二进制
                mybtye[9] = Convert.ToByte((numstr.Substring(0, 2)+"00"+Common.putintTo2string(airlinecode, 4)),2);//航线编码取低4位，5.6补齐 0 高2位取航点编码的8 9
                mybtye[10] = Convert.ToByte(numstr.Substring(2, 8),2);//截取低位的8位存储到10航点编码
                long n = 0;
                for (int i = 8; i < 31; i++)
                {
                    n += mybtye[i];
                }
                mybtye[31] = Convert.ToByte(Common.putintTo16string(n, 2), 16);//校验和：Data[8]～Data[30]的累加和取最低字节
            
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }

            return mybtye;
        }

        /// <summary>
        /// 云台控制命令
        /// </summary>
        /// <param name="num">控制值数值</param>
        /// <param name="control">控制的类型</param>
        /// <param name="msg">如果出现错误，则赋值否则为空</param>
        /// <returns>返回命令byte数组</returns>
        public byte[] Nav_ControlNav(byte num, CameraControlType control, out string msg)
        {
            byte[] mybtye = new byte[_dataLengh];//目前是32个字节
            msg = "";
            

            try
            {
                mybtye[0] = 0xEB;//第一位为0xEB
                mybtye[1] = 0x90;//第二位为0x90
                mybtye[2] = Convert.ToByte(_dataLengh);//第二位为0x20 固定长度32字节
                mybtye[3] = Convert.ToByte((Common.putintTo2string(_targetaddrees, 4) + Common.putintTo2string(_sourceaddrees, 4)), 2);//2个地址组成一个
                mybtye[8] = 0x40;//云台控制命令
                #region 实际控制命令，7个字节
                mybtye[9] = 0xFF;//帧头
                mybtye[10] = 1;// Convert.ToByte(Common.putintTo2string(_targetaddrees, 4), 2);//地址，应该为飞机的唯一编码，暂时写死给1
                mybtye[11] = 0x00;//暂时为0 
                //mybtye[12] = Convert.ToByte(control.GetHashCode());//控制类型 4：右调 3：左调 1：上调 2：下调  6:变倍加 7:变倍减
                if (control.GetHashCode() == 1)
                {
                    mybtye[12] = 0x08;
                }
                else if (control.GetHashCode() == 2)
                {
                    mybtye[12] = 0x10;
                }
                else if (control.GetHashCode() == 3)
                {
                    mybtye[12] = 0x04;
                }
                else if (control.GetHashCode() == 4)
                {
                    mybtye[12] = 0x02;
                }
                else if (control.GetHashCode() == 6)
                {
                    mybtye[12] = 0x20;
                }
                else if (control.GetHashCode() == 7)
                {
                    mybtye[12] = 0x40;
                }

                if (control.GetHashCode() == 3 || control.GetHashCode() == 4)
                {
                    mybtye[13] = num;
                }
                if (control.GetHashCode() == 1 || control.GetHashCode() == 2)
                {
                    mybtye[14] = num;
                }
                long n = 0;
                for (int i = 10; i < 15; i++)
                {
                    n += mybtye[i];
                }
                mybtye[15] = Convert.ToByte(Common.putintTo16string(n, 2), 16);//校验码：（地址+ 功能字 1 + 功能字 2 + 参数 1 + 参数 2）累加和取最低字节；
                #endregion
                n = 0;
                for (int i = 8; i < 31; i++)
                {
                    n += mybtye[i];
                }
                mybtye[31] = Convert.ToByte(Common.putintTo16string(n, 2), 16);//校验和：Data[8]～Data[30]的累加和取最低字节

            }
            catch(Exception ex) 
            {
                msg = ex.Message;
                return null;
            }

            return mybtye;
        }

        /// <summary>
        /// 盘旋切换指令
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public byte[] Nav_CycleSwith(NavCycleobj obj, out string msg)
        {
            byte[] mybtye = new byte[_dataLengh];//目前是32个字节
            msg = "";
          

            try {
                mybtye[0] = 0xEB;//第一位为0xEB
                mybtye[1] = 0x90;//第二位为0x90
                mybtye[2] = Convert.ToByte(_dataLengh);//第二位为0x20 固定长度32字节
                mybtye[3] = Convert.ToByte((Common.putintTo2string(_targetaddrees, 4) + Common.putintTo2string(_sourceaddrees, 4)), 2);//2个地址组成一个
                mybtye[8] = 0xE3;//盘旋切换命令
                mybtye[9] = 11;//11：盘旋航线
                mybtye[11] = obj.CycleNum;//目标盘旋圈数 若该值>250 圈，则一直盘旋 默认 255；
                //mybtye[12] = 0;//bit0: 盘旋中心点经度、纬度有效位 bit1: 盘旋半径有效位00000011
                mybtye[12] = 3;//bit0: 盘旋中心点经度、纬度有效位 bit1: 盘旋半径有效位00000011

                string numstr = Common.putintTo16string(obj.Longitude, 8);//把经度转为8位16进制
                mybtye[13] = Convert.ToByte(numstr.Substring(6, 2), 16);//低字节
                mybtye[14] = Convert.ToByte(numstr.Substring(4, 2), 16);//次低字节
                mybtye[15] = Convert.ToByte(numstr.Substring(2, 2), 16);//次高字节
                mybtye[16] = Convert.ToByte(numstr.Substring(0, 2), 16);//高字节
                numstr = Common.putintTo16string(obj.Latitude, 8);//把纬度转为8位16进制
                mybtye[17] = Convert.ToByte(numstr.Substring(6, 2), 16);//低字节
                mybtye[18] = Convert.ToByte(numstr.Substring(4, 2), 16);//次低字节
                mybtye[19] = Convert.ToByte(numstr.Substring(2, 2), 16);//次高字节
                mybtye[20] = Convert.ToByte(numstr.Substring(0, 2), 16);//高字节  
                //半径处理
                numstr = Common.putintTo16string(obj.Cycleradius, 4);//把半径转为4位16进制 
                mybtye[21] = Convert.ToByte(numstr.Substring(2, 2), 16);//次高字节
                mybtye[22] = Convert.ToByte(numstr.Substring(0, 2), 16);//高字节  
                if (obj.Cycletype == 0)
                {//跟踪地面目标
                    numstr = "00000001";
                }
                else if (obj.Cycletype == 1)
                {//沿航线飞向目标点
                    numstr = "00000010";
                }
                else if (obj.Cycletype == 2)
                {//改变飞行航向
                    numstr = "00000100";
                }
                mybtye[23] = Convert.ToByte(numstr, 2);

                //目标航向增量
                numstr = Common.putintTo16string(obj.Boatincremental, 4);//把目标航向增量转为4位16进制 
                mybtye[24] = Convert.ToByte(numstr.Substring(2, 2), 16);//次高字节
                mybtye[25] = Convert.ToByte(numstr.Substring(0, 2), 16);//高字节  
                //目标距离
                //numstr = Common.putintTo16string(obj.Boatincremental, 4);//把目标距离转为4位16进制 
                numstr = Common.putintTo16string(obj.Boatdistnct, 4);//把目标距离转为4位16进制 
                mybtye[26] = Convert.ToByte(numstr.Substring(2, 2), 16);//次高字节
                mybtye[27] = Convert.ToByte(numstr.Substring(0, 2), 16);//高字节  
                long n = 0;
                for (int i = 8; i < 31; i++)
                {
                    n += mybtye[i];
                }
                mybtye[31] = Convert.ToByte(Common.putintTo16string(n, 2), 16);//校验和：Data[8]～Data[30]的累加和取最低字节
            }
            catch(Exception ex) 
            {
                msg = ex.Message;
                return null;
            }

            return mybtye;
        }

        /// <summary>
        /// 起飞/巡航指令
        /// </summary>
        /// <param name="isonlyoffground">起飞与巡航的指令命令  true:起飞 false:巡航</param>
        /// <param name="msg">错误则返回</param>
        /// <returns></returns>
        public byte[] Nav_TakeOff(bool isonlyoffground,out string msg)
        {
            byte[] mybtye = new byte[_dataLengh];//起飞指令为32字节数组
            msg = "";


            try
            {
                mybtye[0] = 0xEB;//第一位为0xEB
                mybtye[1] = 0x90;//第二位为0x90
                mybtye[2] = Convert.ToByte(_dataLengh);//第二位为0x20 固定长度32字节
                mybtye[3] = Convert.ToByte((Common.putintTo2string(_targetaddrees, 4) + Common.putintTo2string(_sourceaddrees, 4)), 2);//2个地址组成一个
                mybtye[8] = 0xEF;//起飞/巡航指令
                mybtye[9] = 0x02;//bit1 飞行阶段控制为 0：无效 1：有效
                if (isonlyoffground)
                {
                    mybtye[11] = 0x01;//0x01:起飞，定高3米盘旋 0x02:巡航(垂直爬升至高度下限以上，并切换固定翼巡航)
                }
                else
                {
                    mybtye[11] = 0x02;//0x01:起飞，定高3米盘旋 0x02:巡航(垂直爬升至高度下限以上，并切换固定翼巡航)
                }
                long n = 0;
                for (int i = 8; i < 31; i++)
                {
                    n += mybtye[i];
                }
                mybtye[31] = Convert.ToByte(Common.putintTo16string(n, 2), 16);//校验和：Data[8]～Data[30]的累加和取最低字节
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }

            return mybtye;
        }

        /// <summary>
        /// 航线下载命令
        /// </summary>
        /// <param name="airlinecode">航线编码</param>
        /// <param name="pointcode">航点编号 0:的时候则全部下载</param>
        /// <param name="msg">如果出错则返回错误信息，否则为空</param>
        /// <returns>命令byte数组</returns>
        public byte[] Nav_AirLineDownload(byte airlinecode, int pointcode, out string msg)
        {
            byte[] mybtye = new byte[_dataLengh];//目前是32个字节
            msg = "";
            if (airlinecode > 11)
            {
                msg = "航线编码大于11,请确认是否存在此航线.";
                return null;
            }
            if (pointcode > 800)
            {
                msg = "航点编码大于800,请确认是否存在此航点.";
                return null;
            }

            try
            {
                mybtye[0] = 0xEB;//第一位为0xEB
                mybtye[1] = 0x90;//第二位为0x90
                mybtye[2] = Convert.ToByte(_dataLengh);//第二位为0x20 固定长度32字节
                mybtye[3] = Convert.ToByte((Common.putintTo2string(_targetaddrees, 4) + Common.putintTo2string(_sourceaddrees, 4)), 2);//2个地址组成一个
                mybtye[8] = 0xE2;//航线下载命令
                string numstr = Common.putintTo2string(pointcode, 10);//取航点编号的10位二进制
                mybtye[9] = Convert.ToByte((numstr.Substring(0, 2) + "00" + Common.putintTo2string(airlinecode, 4)), 2);//航线编码取低4位，5.6补齐 0 高2位取航点编码的8 9
                mybtye[10] = Convert.ToByte(numstr.Substring(2, 8), 2);//截取低位的8位存储到10航点编码
                long n = 0;
                for (int i = 8; i < 31; i++)
                {
                    n += mybtye[i];
                }
                mybtye[31] = Convert.ToByte(Common.putintTo16string(n, 2), 16);//校验和：Data[8]～Data[30]的累加和取最低字节

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }

            return mybtye;
        }
        #endregion

    }

    /// <summary>
    /// 飞机盘旋切换指令对象
    /// </summary>
    public class NavCycleobj
    {
        /// <summary>
        /// 盘旋的圈数 1-250若该值>250 圈，则一直盘旋 默认 255
        /// </summary>
        public byte CycleNum { get; set; }

        /// <summary>
        /// 盘旋的中心点经度  单位：0.000001°
        /// </summary>
        public long Longitude { get; set; }

        /// <summary>
        /// 盘旋的中心点纬度 单位：0.000001°
        /// </summary>
        public long Latitude { get; set; }

        /// <summary>
        /// 盘旋半径
        /// </summary>
        public Int16 Cycleradius { get; set; }

        /// <summary>
        /// 跟踪控制类型 0.跟踪地面目标 1.沿航线飞向目标点 2.改变飞行航向
        /// </summary>
        public byte Cycletype { get; set; }

        /// <summary>
        /// 目标航向增量 单位：0.1°改变飞行航向控制字有效时，启用
        /// </summary>
        public Int16 Boatincremental { get; set; }

        /// <summary>
        /// 目标距离 单位：m 改变飞行航向控制字有效时，启用
        /// </summary>
        public Int16 Boatdistnct { get; set; }
    }



    /// <summary>
    /// 控制类型
    /// </summary>
    public enum ControlType
    {
        升高=8,
        下降=16,
        左飞=4,
        右飞=2
    }

    /// <summary>
    /// 飞机功能按钮控制类型
    /// </summary>
    public enum FunctionBtnControlType
    { 
        获取控制=0,
        起飞=1,
        返航=2,
        盘旋=3,
        悬停=4,
        降落=5
    }

    /// <summary>
    /// 相机角度控制类型
    /// </summary>
    public enum CameraControlType
    {
        上调 = 1,
        下调 = 2,
        左调 = 3,
        右调 = 4,
        归中 = 5,
        变倍加 = 6,
        变倍减 = 7
    }

    /// <summary>
    /// 航线对象
    /// </summary>
    public class AirlineModel
    {
        /// <summary>
        /// 航线id--系统后台表index
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 航线编码 0：回家航线  1~9：普通用户航线  10：降落航线  11：盘旋航线 
        /// </summary>
        public int Airlinecode { get; set; }

        /// <summary>
        /// 是否航线循环 
        /// </summary>
        public bool Aircycle { get; set; }


        /// <summary>
        /// 航点信息，可以存放多个
        /// </summary>
        public List<Airlinepoint> Airlinepoint { get; set; }
    }

    /// <summary>
    /// 航点对象
    /// </summary>
    public class Airlinepoint
    {
        /// <summary>
        /// 航点编号
        /// </summary>
        public int Airpointcode { get; set; }

        /// <summary>
        /// 航线切换 正常切换=0,切换到盘旋航线=1,切换至自主降落=2,切换至悬停=3,切换到回家航线=0xF 
        /// </summary>
        public int Lineswitch { get; set; }

        /// <summary>
        /// 高度控制 高度正常控制 = 0,高度坡度控制 = 1,先盘旋至目标高度  = 2,到目标点后再盘旋至目标高度  = 3
        /// </summary>
        public int Heightcontrol { get; set; }

        /// <summary>
        /// 航点经度
        /// </summary>
        public long Longitude { get; set; }

        /// <summary>
        /// 航点纬度
        /// </summary>
        public long Latitude { get; set; }

        /// <summary>
        /// 航点高度
        /// </summary>
        public int height { get; set; }

        /// <summary>
        /// 下一航段速度 
        /// </summary>
        public byte NextAreaSpeed { get; set; }

        /// <summary>
        /// 是否有效-录像,默认给0
        /// </summary>
        public bool Isok_vedio { get; set; }

        /// <summary>
        /// 是否有效-拍照,默认给0
        /// </summary>
        public bool Isok_pictrue { get; set; }


        /// <summary>
        /// 是否动作-录像,默认给0
        /// </summary>
        public bool Isaction_vedio { get; set; }

        /// <summary>
        /// 是否动作-拍照,默认给0
        /// </summary>
        public bool Isaction_pictrue { get; set; }

        /// <summary>
        /// 盘旋圈数 
        /// </summary>
        public byte CycleNum { get; set; }

        /// <summary>
        /// 拍照间隔距离  
        /// </summary>
        public int PicDist { get; set; }
    }



    /// <summary>
    /// 航线切换
    /// </summary>
    public enum Lineswitch
    {
        正常切换=0,
        切换到盘旋航线=1,
        切换至自主降落=2,
        切换至悬停=3,
        切换到回家航线=0xF 
    }

    /// <summary>
    /// 高度控制
    /// </summary>
    public enum Heightcontrol
    {
        高度正常控制 = 0,
        高度坡度控制 = 1,
        先盘旋至目标高度  = 2,
        到目标点后再盘旋至目标高度  = 3
    }
}
