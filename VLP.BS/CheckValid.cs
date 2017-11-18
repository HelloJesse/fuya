using System;
using System.Collections.Generic;
using System.Text;

namespace VLP
{
    public class CheckValid
    {
        /// <summary>
        /// 检查箱号是否有效
        /// </summary>
        /// <param name="ContainerNo">集装箱号</param>
        /// <returns></returns>
        public static bool ContainerNoValid(string ContainerNo)
        {
            if (ContainerNo.Length != 11) return false;  //必须为11位

            ContainerNo = ContainerNo.ToUpper();
            //CBHU3202732
            //前四位为英文
            int total = 0;
            byte count = 0;
            byte value = 0;
            foreach (char c in ContainerNo)
            {
                if (count == 10) break; //第11为验证码
                if (count <= 3)
                {
                    if (!(c >= 'A' && c <= 'Z'))
                    {
                        return false;   //前四位为字母
                    }
                }
                else
                {
                    if (!(c >= '0' && c <= '9'))
                    {
                        return false;//必须为数字
                    }
                }
                if (c >= 65)
                {
                    value = byte.Parse(((c - 65) + 10).ToString());
                    if (value >= 11)
                    {
                        value++;
                    }
                    if (value >= 22)
                    {
                        value++;
                    }
                    if (value >= 33)
                    {
                        value++;
                    }
                }
                else
                {
                    value = byte.Parse(c.ToString());
                }
                total = total + value * int.Parse(System.Math.Pow(2, count).ToString());
                count++;
            }
            if (ContainerNo[10] != (total % 11).ToString()[0])
            {
                return false;   //校验码不对
            }

            return true;
        }
    }
}
