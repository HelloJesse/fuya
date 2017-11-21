using System;

namespace GetSingleShipInfo
{
    public class SpecialFunctions
    {
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public SpecialFunctions()
        { }

        #endregion


        #region 方法

        /// <summary>
        /// 数字经纬度和度分秒经纬度转换 (Digital degree of latitude and longitude and vehicle to latitude and longitude conversion)
        /// </summary>
        /// <param name="digitalLati_Longi">数字经纬度</param>
        /// <return>度分秒经纬度</return>
        static public double ConvertDigitalToDegrees(string digitalLati_Longi)
        {
            double digitalDegree = Convert.ToDouble(digitalLati_Longi);
            return ConvertDigitalToDegrees(digitalDegree);
        }

        /// <summary>
        /// 数字经纬度和度分秒经纬度转换 (Digital degree of latitude and longitude and vehicle to latitude and longitude conversion)
        /// </summary>
        /// <param name="digitalDegree">数字经纬度</param>
        /// <return>度分秒经纬度</return>
        static public double ConvertDigitalToDegrees(double digitalDegree)
        {
            int degree = (int)digitalDegree;
            double minute = (digitalDegree - degree) * 60;
            return degree * 1.0 + minute;
            //const double num = 60;
            //int degree = (int)digitalDegree;
            //double tmp = (digitalDegree - degree) * num;
            //int minute = (int)tmp;
            //double second = (tmp - minute) * num;
            //string degrees = "" + degree + "°" + minute + "′" + second + "″";
            //return degrees;
        }


        /// <summary>
        /// 度分秒经纬度(必须含有'°')和数字经纬度转换
        /// </summary>
        /// <param name="digitalDegree">度分秒经纬度</param>
        /// <return>数字经纬度</return>
        static public double ConvertDegreesToDigital(string degrees)
        {
            const double num = 60;
            double digitalDegree = 0.0;
            int d = degrees.IndexOf('°');           //度的符号对应的 Unicode 代码为：00B0[1]（六十进制），显示为°。
            if (d < 0)
            {
                return digitalDegree;
            }
            string degree = degrees.Substring(0, d);
            digitalDegree += Convert.ToDouble(degree);

            int m = degrees.IndexOf('′');           //分的符号对应的 Unicode 代码为：2032[1]（六十进制），显示为′。
            if (m < 0)
            {
                return digitalDegree;
            }
            string minute = degrees.Substring(d + 1, m - d - 1);
            digitalDegree += ((Convert.ToDouble(minute)) / num);

            int s = degrees.IndexOf('″');           //秒的符号对应的 Unicode 代码为：2033[1]（六十进制），显示为″。
            if (s < 0)
            {
                return digitalDegree;
            }
            string second = degrees.Substring(m + 1, s - m - 1);
            digitalDegree += (Convert.ToDouble(second) / (num * num));

            return digitalDegree;
        }


        /// <summary>
        /// 度分秒经纬度(必须含有'/')和数字经纬度转换
        /// </summary>
        /// <param name="digitalDegree">度分秒经纬度</param>
        /// <param name="cflag">分隔符</param>
        /// <return>数字经纬度</return>
        static public double ConvertDegreesToDigital_default(string degrees)
        {
            char ch = '/';
            return ConvertDegreesToDigital(degrees, ch);
        }

        /// <summary>
        /// 度分秒经纬度和数字经纬度转换
        /// </summary>
        /// <param name="digitalDegree">度分秒经纬度</param>
        /// <param name="cflag">分隔符</param>
        /// <return>数字经纬度</return>
        static public double ConvertDegreesToDigital(string degrees, char cflag)
        {
            const double num = 60;
            double digitalDegree = 0.0;
            int d = degrees.IndexOf(cflag);
            if (d < 0)
            {
                return digitalDegree;
            }
            string degree = degrees.Substring(0, d);
            digitalDegree += Convert.ToDouble(degree);

            int m = degrees.IndexOf(cflag, d + 1);
            if (m < 0)
            {
                return digitalDegree;
            }
            string minute = degrees.Substring(d + 1, m - d - 1);
            digitalDegree += ((Convert.ToDouble(minute)) / num);

            int s = degrees.Length;
            if (s < 0)
            {
                return digitalDegree;
            }
            string second = degrees.Substring(m + 1, s - m - 1);
            digitalDegree += (Convert.ToDouble(second) / (num * num));

            return digitalDegree;
        }

        /// <summary>
        /// 度数 度分秒转为小数点的度数 方便函数计算
        /// </summary>
        /// <param name="dgrees">度数</param>
        /// <returns></returns>
        static public double ConvertDegreesToDegrees(string dgrees)
        {
            double digitalDegree = 0.0;
            string strDgress = string.Empty, strMin = string.Empty, strSec = string.Empty;
            string[] strDgr;
            if (dgrees.IndexOf("°") > -1)
            {
                strDgr = dgrees.Split('°');
                if (strDgr.Length > 0)
                    strDgress = strDgr[0];
                if (strDgr.Length > 1)
                    strMin = strDgr[1];
                if (strMin.IndexOf("′") > -1)
                {
                    strDgr = strMin.Split('′');
                    strMin = strDgr[0];
                    if (strDgr.Length > 1)
                        strSec = strDgr[1];
                    if (strSec.IndexOf("″") > -1)
                    {
                        strDgr = strSec.Split('″');
                        strSec = strDgr[0];
                    }
                }

            }
            else if (dgrees.IndexOf("′") > -1)
            {
                strDgr = dgrees.Split('′');
                if (strDgr.Length > 0)
                    strMin = strDgr[0];
                if (strDgr.Length > 1)
                    strSec = strDgr[1];
                if (strSec.IndexOf("″") > -1)
                {
                    strDgr = strSec.Split('″');
                    strSec = strDgr[0];
                }
            }
            else if (dgrees.IndexOf("″") > -1)
            {
                strDgr = dgrees.Split('″');
                strSec = strDgr[0];
            }

            if (!string.IsNullOrEmpty(strDgress))
            {
                digitalDegree += int.Parse(strDgress);
            }
            if (!string.IsNullOrEmpty(strMin))
            {
                digitalDegree += long.Parse(strMin) * 1.0 / 60;
            }
            if (!string.IsNullOrEmpty(strSec))
            {
                digitalDegree += double.Parse(strSec) * 1.0 / 3600;
            }
            return digitalDegree;
        }
        #region 时间转换 Unix 时间互转

        /// <summary>
        /// 将Unix时间戳转换为DateTime类型时间
        /// </summary>
        /// <param name="d">double 型数字</param>
        /// <returns>DateTime</returns>
        public static System.DateTime ConvertIntDateTime(double d)
        {
            DateTime startUnixTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local);
            return startUnixTime.AddSeconds(d);
        }

        /// <summary>
        /// 将c# DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>long</returns>
        public static long ConvertDateTimeInt(DateTime time)
        {
            return ((time.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
        }
        #endregion

        #endregion
    }
}
