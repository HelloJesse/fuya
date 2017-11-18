using System;
using System.Collections;

namespace VLP.BS
{
    public class VLPHandler:System.Web.IHttpHandler
    {
        static System.Collections.Queue _ChangeFile;
        static System.Threading.Timer _Timer;
        static System.IO.FileSystemWatcher _Watcher;
        static KeyValue MyKeyValue = new KeyValue();
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(System.Web.HttpContext context)
        {

            string key = context.Request.Url.AbsolutePath;
            if (MyKeyValue.ContainsKey(key)==false)
            {
                string file = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, key);
                System.IO.FileInfo f = new System.IO.FileInfo(file);
                if (f.Exists)
                {
                    using (System.IO.FileStream stream = f.OpenRead())
                    {
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        byte[] zbytes = Zip.GZipCompress(bytes);
                        Content c = new Content();
                        c.Obytes = bytes;
                        c.Zbytes = zbytes;

                        c.DT = f.LastWriteTimeUtc.ToString("f", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        c.Etag = MD5.ByteArrayToHexString(MD5.MakeMD5(bytes));
                        lock (MyKeyValue)
                        {
                            if (MyKeyValue.ContainsKey(key) == false)
                                MyKeyValue.Add(key, c);
                        }
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;//未找到
                    return;
                }
            }

            
            string cachecontrol = context.Request.Headers["Cache-Control"];
            Content content = MyKeyValue[key];
            string requestetag = context.Request.Headers["If-None-Match"];
            string requesteDT = context.Request.Headers["If-Modified-Since"];
            if (cachecontrol == null)
            {
                cachecontrol = "";
            }
            bool nomodifyflag=false;
            if (cachecontrol.IndexOf("max-age") >= 0 || cachecontrol.IndexOf("no-cache") == -1)
            {
                if (requestetag == content.Etag)    //仅根据Etag来判断,不需要根据时间requesteDT == content.DT &&
                {
                    nomodifyflag = true;
                }
            }
            if (nomodifyflag)
            {
                //返回304
                context.Response.Headers["ETag"] = content.Etag;
                context.Response.Headers["Last-Modified"] = content.DT;
                context.Response.StatusCode = 304;
            }
            else
            {
                string acceptcode = context.Request.Headers["Accept-Encoding"];
                if (acceptcode.IndexOf("gzip") >= 0)
                {
                    context.Response.Headers["Content-Encoding"] = "gzip";
                    context.Response.Headers["ETag"] = content.Etag;
                    context.Response.Headers["Last-Modified"] = content.DT;
                    context.Response.BinaryWrite(content.Zbytes);
                }
                else
                {
                    context.Response.BinaryWrite(content.Obytes);
                }
            }
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 处理更新的文件
        /// </summary>
        /// <param name="fileInfo"></param>
        public static void HandlerUpdateFile(System.IO.FileInfo fileInfo)
        {
            string key = string.Format("/{0}", fileInfo.FullName.Replace(AppDomain.CurrentDomain.BaseDirectory, string.Empty).Replace("\\","/"));
            if (MyKeyValue.ContainsKey(key))
            {
                if (fileInfo.Exists)
                {
                    using (System.IO.FileStream stream = fileInfo.OpenRead())
                    {
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        byte[] zbytes = Zip.GZipCompress(bytes);
                        Content c = MyKeyValue[key];
                        c.Obytes = bytes;
                        c.Zbytes = zbytes;
                        c.DT = fileInfo.LastWriteTimeUtc.ToString("f",System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        c.Etag = MD5.ByteArrayToHexString(MD5.MakeMD5(bytes));
                    }
                }
            }
        }
        /// <summary>
        /// 处理更改文件方法
        /// </summary>
        public static void HandChangeFile()
        {

            _ChangeFile = new Queue();
             _Watcher = new System.IO.FileSystemWatcher(AppDomain.CurrentDomain.BaseDirectory, "*.html");
            _Watcher.EnableRaisingEvents = true;
            _Watcher.IncludeSubdirectories = true;
            _Watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            _Watcher.Changed += watcher_Changed;
            _Timer = new System.Threading.Timer(_Timer_Tick, null, 2000, 2000);
            
        }
        /// <summary>
        /// 释放处理更改文件程序资源
        /// </summary>
        public static void HandChangeFileDispose()
        {
            _Watcher.Dispose();
            _Timer.Dispose();
            _ChangeFile.Clear();
        }
        static void _Timer_Tick(object sender)
        {

            while (_ChangeFile.Count > 0)
            {
                lock (_ChangeFile)
                {
                    if (_ChangeFile.Count == 0)
                        break;
                    string filename = (string)_ChangeFile.Dequeue();
                    System.IO.FileInfo f = new System.IO.FileInfo(filename);
                    VLP.BS.VLPHandler.HandlerUpdateFile(f);
                }
            }
        }

        static void watcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            if (e.ChangeType == System.IO.WatcherChangeTypes.Changed)
            {
                if (_ChangeFile.Contains(e.FullPath) == false)
                {
                    _ChangeFile.Enqueue(e.FullPath);
                }
            }
        }
    }
    class Content
    {
        public byte[] Obytes;
        public byte[] Zbytes;
        public string Etag;
        public string DT;
    }
    class KeyValue : System.Collections.Generic.Dictionary<string, Content>
    {
        
    }
     
}
