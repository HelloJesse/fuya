using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace VLP
{
    /// <summary>
    /// MD5
    /// </summary>
    public class MD5
    {
        /// <summary>  
        /// 生成MD5码 
        /// </summary>  
        /// <param name="original">数据源</param>  
        /// <returns>MD5码</returns>  
        public static byte[] MakeMD5(byte[] original)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider hashmd5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] keyhash = hashmd5.ComputeHash(original);
            hashmd5.Clear();
            hashmd5 = null; return keyhash;
        }

        /// <summary>
        /// //获取流的MD5码
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <returns>MD5码</returns>
        public static byte[] CretaeMD5(Stream stream)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(stream);
            md5.Clear();
            md5 = null;
            return hash;
        }
        /// <summary>
        /// 将字节转换为16进制字符
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] bytes)
        {
            byte[] buffer = MakeMD5(bytes);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                builder.Append(buffer[i].ToString("x2"));
            }
            return builder.ToString();
        }
        /// <summary>
        /// 获取字符串MD5值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string MankMD5(string value)
        {
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(value);
            byte[] md5bytes = MakeMD5(bytes);
            return ByteArrayToHexString(md5bytes);
        }
        /// <summary>
        /// 字符串解密
        /// </summary>
        /// <param name="pToDecrypt">待解密字符串</param>
        /// <returns></returns>
        public static string Decrypt(string pToDecrypt)
        {
            string sKey = "vjingkey";   //sKey为固定密钥必需要为英文8位
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            //Put  the  input  string  into  the  byte  array  
            byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (System.Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            //建立加密对象的密钥和偏移量，此值重要，不能修改  
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            //Flush  the  data  through  the  crypto  stream  into  the  memory  stream  
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            //Get  the  decrypted  data  back  from  the  memory  stream  
            //建立StringBuild对象，CreateDecrypt使用的是流对象，必须把解密后的文本变成流对象  
            StringBuilder ret = new StringBuilder();

            return System.Text.Encoding.Default.GetString(ms.ToArray());
        }
    }
}
