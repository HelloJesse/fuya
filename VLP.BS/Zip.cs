using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;


namespace VLP
{
    public class Zip
    {
        /// <summary>
        /// 压缩流
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        public static byte[] CompressStream(Stream sourceStream)
        {
            byte[] buffer = new byte[sourceStream.Length];
            int checkCounter = sourceStream.Read(buffer, 0, buffer.Length);

            if (checkCounter != buffer.Length)
            {
                throw new ApplicationException();
            }
            return Compress(buffer);
        }


        ///////////////////////////////
        /// <summary>
        /// 数据压缩类
        /// </summary>
        /// <param name="pBytes">需要压缩的byte数组</param>
        /// <returns></returns>
        static public byte[] Compress(byte[] pBytes)
        {

            using (MemoryStream mMemory = new MemoryStream())
            {

                ICSharpCode.SharpZipLib.Zip.Compression.Deflater mDeflater =
                    new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_COMPRESSION);
                using (ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream mStream =
                    new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(mMemory, mDeflater, 1024*1024))
                {
                    mStream.Write(pBytes, 0, pBytes.Length);
                    mStream.Close();
                }
                return mMemory.ToArray();
            }

        }

        /// <summary>
        /// 数据解压缩类
        /// </summary>
        /// <param name="pBytes">需要解压缩的byte数组</param>
        /// <returns></returns>
        static public byte[] Uncompress(byte[] pBytes)
        {
            using (ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream mStream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(new MemoryStream(pBytes)))
            {

                MemoryStream mMemory = new MemoryStream();
                Int32 mSize = 0;

                byte[] mWriteData = new byte[4096];

                while (mSize != -1)
                {
                    mSize = mStream.Read(mWriteData, 0, mWriteData.Length);
                    if (mSize > 0)
                    {
                        mMemory.Write(mWriteData, 0, mSize);
                    }
                    else
                    {
                        mSize = -1;
                    }
                }

                mStream.Close();
                return mMemory.ToArray();
            }
        }
        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static byte[] GZipCompress(byte[] rawData)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }
        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static byte[] GZipCompress(string value)
        {
            byte[] rawData = System.Text.Encoding.UTF8.GetBytes(value);
            return GZipCompress(rawData);
        }
        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public static byte[] GZipUnCompress(byte[] zippedData)
        {
            using (MemoryStream ms = new MemoryStream(zippedData))
            {
                using (GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream outBuffer = new MemoryStream())
                    {
                        byte[] block = new byte[1024];
                        while (true)
                        {
                            int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                            if (bytesRead <= 0)
                                break;
                            else
                                outBuffer.Write(block, 0, bytesRead);
                        }
                        compressedzipStream.Close();
                        return outBuffer.ToArray();
                    }
                }
            }
        }
        /// <summary>
        /// 将传入的二进制字符串资料以GZip算法解压缩
        /// </summary>
        /// <param name="zippedString">经GZip压缩后的二进制字符串</param>
        /// <returns>原始未压缩字符串</returns>
        public static string GZipUnCompressString(string zippedString)
        {
            if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] zippedData = Convert.FromBase64String(zippedString.ToString());
                return (string)(System.Text.Encoding.UTF8.GetString(GZipUnCompress(zippedData)));
            }
        }
    }
}
