using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using VLP.NAV;

namespace LocalServer
{
    class Program
    {
      
        public delegate bool ControlCtrlDelegate(int CtrlType);
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
        private static ControlCtrlDelegate cancelHandler = new ControlCtrlDelegate(HandlerRoutine);
        private static Nordasoft.Common.IO.FileLog _FileLog = new Nordasoft.Common.IO.FileLog(
            AppDomain.CurrentDomain.BaseDirectory, string.Format("{0}.log", DateTime.Now.ToString("yyyyMMdd")), 50);

        private static Server _server;

        /// <summary>
        /// 关闭时释放资源
        /// </summary>
        /// <param name="CtrlType"></param>
        /// <returns></returns>
        public static bool HandlerRoutine(int CtrlType)
        {

            _server.Close();

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
            Console.Title = "遥控发送";
            SetConsoleCtrlHandler(cancelHandler, true);

            _FileLog.AddLogInfo("程序启动");

            Console.WriteLine("开始监听本机端口：");

            int tcpServerPort; 
            string serverhost;
            string serverIpAddress;
            string tcpServerPassword;

            int localUdbPort;


            int telemetryPort; //遥测端口
            string telemetryPassword; //遥测密码

            if (!int.TryParse(ConfigurationManager.AppSettings["ServerPort"], out tcpServerPort))
            {
                Console.WriteLine("请指定TCP服务端口"); return;
            }

            serverhost = ConfigurationManager.AppSettings["ServerHost"];
            serverIpAddress = ConfigurationManager.AppSettings["serverIpAddress"];

            tcpServerPassword = ConfigurationManager.AppSettings["ServerPassword"];
            telemetryPassword = ConfigurationManager.AppSettings["TelemetryPassword"];

            if (string.IsNullOrWhiteSpace(serverhost) && string.IsNullOrWhiteSpace(serverIpAddress))
            {
                Console.WriteLine("请指定TCP服务地址或IP"); return;
            }

            if (string.IsNullOrWhiteSpace(tcpServerPassword))
            {
                Console.WriteLine("请指定TCP服务密码"); return;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings["ListenPort"], out localUdbPort))
            {
                Console.WriteLine("请指定本机UDP监听端口"); return;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings["TelemetryPort"], out telemetryPort))
            {
                Console.WriteLine("请指定遥测服务TCP端口"); return;
            }

            var config = new Dtos.BaseStationConfig
            {
                CommandPassword = tcpServerPassword,
                CommandPort = tcpServerPort,
                InfoPassword = telemetryPassword,
                InfoPort = telemetryPort,
                ServerHost = serverhost,
                ServerIp = serverIpAddress,
                LocalServerPort = localUdbPort,

            };
            _server = new Server(config);
            try
            {  
                var isTelemtryServerValid = _server.preCheck();
                if (!isTelemtryServerValid)
                {
                    Console.WriteLine("遥测数据不可连接...服务不可启动。");
                }

                // 启动TCP发送服务，并发送密码认证
                _server.initSocket();
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生错误: " + ex.Message);
                Console.WriteLine("请重启程序。");

                _FileLog.AddLogInfo("程序异常:" + ex.ToString());
            }

            _server.StartMonitorLoaclCommand();
            _server.StartSendHealthCheckData();

            Console.ReadLine();
        }


        
    
    }
}
