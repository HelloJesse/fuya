using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VLP.NAV
{
    public class Common
    {
        public class ReceiveData
        {
            public DateTime ReceiveTime { get; set; }
            public byte[] Data { get; set; }
            public ReceiveData(byte[] data, DateTime receivetime)
            {
                ReceiveTime = receivetime;
                Data = data;
            }

        }

        #region 用到的byte数组方法
        /// <summary>
        /// 把整数转换成符合位数二进制显示字符串
        /// </summary>
        /// <param name="value">要处理的值</param>
        /// <param name="num">取的位数</param>
        /// <returns>符合要求的二进制字符串</returns>
        public static string putintTo2string(Int64 value, int num)
        {
            string str = "";

            str = Convert.ToString(value, 2);
            if (str.Length > num)
            {
                str = str.Substring(str.Length - num, num);
            }
            else if (str.Length < num)
            {
                int s = num - str.Length;
                for (int i = 0; i < s; i++)
                    str = "0" + str;
            }
            return str;
        }

        /// <summary>
        /// 把整数转换成符合位数16进制显示字符串
        /// </summary>
        /// <param name="value">要处理的值</param>
        /// <param name="num">取的位数</param>
        /// <returns>符合要求的16进制字符串</returns>
        public static string putintTo16string(Int64 value, int num)
        {
            string str = "";

            //if (num == 4)
            //{//16位数字，取4位16进制字符串
            //    str = Convert.ToString((Int16)value, 16);
            //}
            //else if (num == 16)
            //{//64位的数字
            //    str = Convert.ToString((Int64)value, 16);
            //}
            str = Convert.ToString(value, 16);
            if (str.Length > num)
            {
                str = str.Substring(str.Length - num, num);
            }
            else if (str.Length < num)
            {
                int s = num - str.Length;
                for (int i = 0; i < s; i++)
                    str = "0" + str;
            }


            return str;
        }

        
        /// <summary>
        /// 获取8个字节2个位的数据组合的10进制整数
        /// </summary>
        /// <param name="bt">处理的字节</param>
        /// <param name="i1">第一位，低位</param>
        /// <param name="i2">第二位，高位</param>
        /// <returns>组合的整数</returns>
        public static int GetByteBitNum(byte bt, int i1, int i2)
        {
            int s = 0;
            int j1 = GetByteBitNum(bt, i1);
            int j2 = GetByteBitNum(bt, i2);
            if (j1 == 1 && j2 == 1)
            {
                s = 3;
            }
            else if (j1 == 0 && j2 == 1)
            {
                s = 2;
            }
            else if (j1 == 1 && j2 == 0)
            {
                s = 1;
            }
            else
            {
                s = 0;
            }
            return s;
        }

        /// <summary>
        /// 获取一个字节某一位上是 0 / 1
        /// </summary>
        /// <param name="bt">处理的字节</param>
        /// <param name="i">位</param>
        /// <returns></returns>
        public static int GetByteBitNum(byte bt, int i)
        {
            return (bt & (byte)System.Math.Pow(2, i)) == System.Math.Pow(2, i) ? 1 : 0; ;
        }

      
        #endregion
    }
}
