using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using System.Runtime.InteropServices;
using VLP.NAV;

namespace ReceiveServer
{
    class Program
    {
        public delegate bool ControlCtrlDelegate(int CtrlType);
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
        private static ControlCtrlDelegate cancelHandler = new ControlCtrlDelegate(HandlerRoutine);
        private static Nordasoft.Common.IO.FileLog _FileLog = new Nordasoft.Common.IO.FileLog(AppDomain.CurrentDomain.BaseDirectory, "data.log",50);

        /// <summary>
        /// 接收服务
        /// </summary>
        private static Server _Server;
        /// <summary>
        /// 关闭时释放资源
        /// </summary>
        /// <param name="CtrlType"></param>
        /// <returns></returns>
        public static bool HandlerRoutine(int CtrlType)
        {
            if (_Server != null)
            {

                _Server.Close();
                System.Threading.Thread.Sleep(50000);
            }

            switch (CtrlType)
            {
                case 0:
                    Console.WriteLine("0工具被强制关闭"); //Ctrl+C关闭  
                    break;
                case 2:
                    Console.WriteLine("2工具被强制关闭");//按控制台关闭按钮关闭  
                    break;
            }
            return false;
        }
        static void Main(string[] args)
        {
            Console.Title = "遥测接收";

            //System.Collections.Queue q = new System.Collections.Queue();
            //q.Enqueue("afsdaf");
            //q.Enqueue("afsdaf34223");
            //System.Collections.Queue aa= System.Collections.Queue.Synchronized(q);
            //q.Enqueue("af");
            //q.Clear();
            SetConsoleCtrlHandler(cancelHandler, true);
            int port = 0;

            string host = ConfigurationManager.AppSettings["Host"];
            string password = ConfigurationManager.AppSettings["Password"];

            if (string.IsNullOrWhiteSpace(host))
            {
                Console.WriteLine("请指定TCP服务地址"); return;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings["Port"], out port))
            {
                Console.WriteLine("请指定端口号."); return;
            }
           
            int datasize = 0;
            if (!int.TryParse(ConfigurationManager.AppSettings["DataSize"], out datasize))
            {
                Console.WriteLine("请指定接收数据大小."); return;
            }
            string cnn = ConfigurationManager.ConnectionStrings["cnn"].ConnectionString;//数据连接
            if (string.IsNullOrEmpty(cnn))
            {
                Console.WriteLine("请指定正确的数据库连接."); return;
            }
            try
            {
                _Server = new Server(host, port, 109, password);
                _Server.ToDBHandler = SaveToDB;
                _Server.Start();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }


            Console.WriteLine("服务停止....");
            Console.ReadKey();
        }
        static void SaveToDB(VLP.NAV.Common.ReceiveData[] datas)
        {
            //ToFile(datas);

            System.Threading.ThreadPool.QueueUserWorkItem(ToFile, datas);

            //_FileLog.AddLogInfo()
        }

        static void ToFile(object data)
        {
            if (data == null) return;
            string cnn = ConfigurationManager.ConnectionStrings["cnn"].ConnectionString;//数据连接
            VLP.NAV.Common.ReceiveData[] datas = (VLP.NAV.Common.ReceiveData[])data;
            NavStateMessage.SaveTelemetrySignals(datas, cnn);
            StringBuilder sb = new StringBuilder();
            //foreach (VLP.NAV.Common.ReceiveData rd in datas)
            //{
            //    string v = byteToHexStr(rd.Data);
            //    sb.AppendFormat("{0} {1}{2}", rd.ReceiveTime.ToString("yyyy-MM-dd HH:mm:ss fff"), v, Environment.NewLine);
                
            //}
            //lock (_FileLog)
            //{
            //    _FileLog.AddLogInfo(sb.ToString());//暂时不写入了
            //}
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2")+" ";
                }
            }
            return returnStr;
        }
    }
}
