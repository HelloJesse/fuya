using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VLP.NAV;

namespace LocalServer
{
    public class Server
    {

        /// <summary>
        /// 发送服务
        /// </summary>
        private static Socket _sender;
        private static string _serverHost;
        private static string _serverIpAddress;
        private static int _tcpServerPort;
        private static int _localUdbPort;
        private static string _password;
        private static string _telemetryPassword;
        private static int _telemetryPort;
        private static Nordasoft.Common.IO.FileLog _FileLog = new Nordasoft.Common.IO.FileLog(
            AppDomain.CurrentDomain.BaseDirectory, string.Format("{0}.log", DateTime.Now.ToString("yyyyMMdd")), 50);
        private static string _healthCheckRequestUrl = System.Configuration.ConfigurationManager.AppSettings["WebHealthCheckurl"];


        public Server(Dtos.BaseStationConfig config)
        {
            _serverHost = config.ServerHost;
            _serverIpAddress = config.ServerIp;

            _tcpServerPort = config.CommandPort;
            _password = config.CommandPassword;

            _telemetryPort = config.InfoPort;
            _telemetryPassword = config.CommandPassword;

            _localUdbPort = config.LocalServerPort;
        }

        public void HandleLocalHeathCheck()
        {
            if (_sender == null)
                return;

            while (true)
            {
                // 心跳检查
                var isConnect = !(_sender.Poll(1000, SelectMode.SelectRead) && (_sender.Available == 0));

                var url = string.Format("{0}&status={1}", _healthCheckRequestUrl, isConnect ? 1 : 0);
                var reqeust = HttpWebRequest.Create(url);
                reqeust.GetResponse();

                Thread.Sleep(3000);
            }
        }

        public void initLocalReceiver()
        {
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            

            server.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _localUdbPort));//绑定端口号和IP
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            var data = new byte[512];

            while (true)
            {
                int recv = server.ReceiveFrom(data, ref remote);
                if (recv == 1)
                {
                    HandleLocalHeathCheck();
                }

                _FileLog.AddLogInfo(string.Format("接收到 {0} 字节本地遥控信号 ", recv));

                var sendData = new byte[recv + 5];
                sendData[0] = 0xEC;
                sendData[1] = 0x91;
                sendData[2] = Convert.ToByte(Common.putintTo16string(sendData.Length, 2), 16);
                sendData[3] = 0xC2;

                Array.Copy(data, 0, sendData, 4, recv);
                long n = 0;
                for (int i = 0; i < sendData.Length - 1; i++)
                {
                    n += sendData[i];
                }

                sendData[sendData.Length - 1] = Convert.ToByte(Common.putintTo16string(n, 2), 16);

                // 启动本地发送服务
                System.Threading.ThreadPool.QueueUserWorkItem(sendToServer, sendData);

                System.Threading.Thread.Sleep(1000);
            }
        }

        public void sendToServer(object obj)
        {
            if (obj != null)
            {
                var bytes = (byte[])obj;
                var sendBytes = _sender.Send(bytes);
                Console.WriteLine("{0} bytes sent.", sendBytes);

                _FileLog.AddLogInfo(string.Format("{0} 字节遥控信号已发送服务器 ", sendBytes));
            }
        }

        public bool preCheck()
        {
            _FileLog.AddLogInfo("开始作遥测信号测试.....");

            EndPoint endPoint;
            if (string.IsNullOrWhiteSpace(_serverIpAddress))
                endPoint = new DnsEndPoint(_serverHost, _telemetryPort);
            else
                endPoint = new IPEndPoint(IPAddress.Parse(_serverIpAddress), _telemetryPort);

            var checkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            checkSocket.Connect(endPoint);

            byte[] recBytes = new byte[5];
            int bytes = checkSocket.Receive(recBytes, recBytes.Length, 0);
            if (bytes == 5 && recBytes[3].ToString("X2") == "C0")
            {
                Console.WriteLine("Benging to do pre check, connecting to telemtry server");

                var pwdBytes = new byte[4];

                pwdBytes[0] = 0xEC;
                pwdBytes[1] = 0x91;
                pwdBytes[3] = 0xC1;

                pwdBytes = pwdBytes.Concat(Encoding.Unicode.GetBytes(_telemetryPassword)).ToArray();
                pwdBytes[2] = Convert.ToByte(Common.putintTo16string(pwdBytes.Length + 1, 2), 16);

                long n = 0;
                for (int i = 0; i < pwdBytes.Length - 1; i++)
                {
                    n += pwdBytes[i];
                }

                pwdBytes = pwdBytes.Concat(new[] { Convert.ToByte(Common.putintTo16string(n, 2), 16) }).ToArray();
                var sendBytes = checkSocket.Send(pwdBytes);
                var checkBytes = new byte[512];

                for (var i = 0; i < 3; i++)
                {
                    Console.WriteLine("try receive telemtry data...");
                    var recv = checkSocket.Receive(checkBytes);
                    if (recv > 0)
                    {
                        _FileLog.AddLogInfo("遥测连接并接收信息成功");
                        Console.WriteLine("Pre check done, connected to telemtry server");

                        // 关闭遥测连接
                        checkSocket.Shutdown(SocketShutdown.Receive);
                        checkSocket.Close();
                        checkSocket.Dispose();

                        return true;
                    }
                    else
                    {
                        Console.WriteLine("0 data received from telmetry server");
                    }
                }
            }

            _FileLog.AddLogInfo("遥测连接并接收信息失败，程序将退出");
            return false;
        }

        public void initSocket()
        {
            Console.WriteLine("Begin to connect");

            EndPoint remote;
            if (string.IsNullOrWhiteSpace(_serverIpAddress))
                remote = new DnsEndPoint(_serverHost, _tcpServerPort);
            else
                remote = new IPEndPoint(IPAddress.Parse(_serverIpAddress), _tcpServerPort);

            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sender.Connect(remote);

            byte[] recBytes = new byte[5];
            int bytes = _sender.Receive(recBytes, recBytes.Length, 0);
            if (bytes == 5 && recBytes[3].ToString("X2") == "C0")
            {
                Console.WriteLine("Beging to send password");

                var pwdBytes = new byte[4];

                pwdBytes[0] = 0xEC;
                pwdBytes[1] = 0x91;
                pwdBytes[3] = 0xC1;

                pwdBytes = pwdBytes.Concat(Encoding.Unicode.GetBytes(_password)).ToArray();
                pwdBytes[2] = Convert.ToByte(Common.putintTo16string(pwdBytes.Length + 1, 2), 16);

                long n = 0;
                for (int i = 0; i < pwdBytes.Length - 1; i++)
                {
                    n += pwdBytes[i];
                }

                pwdBytes = pwdBytes.Concat(new[] { Convert.ToByte(Common.putintTo16string(n, 2), 16) }).ToArray();

                var sendBytes = _sender.Send(pwdBytes);

                Console.WriteLine("Password sent successfully");
            }
        }

        public void Close()
        {
            if (_sender != null)
            {
                _sender.Shutdown(SocketShutdown.Both);
                _sender.Close();
                _sender.Dispose();

                System.Threading.Thread.Sleep(50000);
            }
        }
        public void StartMonitorLoaclCommand()
        {
            // 启动本机监听服务，从UDP接收服务
            try
            {
                var receiveThread = new Thread(new ThreadStart(initLocalReceiver));
                receiveThread.Start();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        public void StartSendHealthCheckData()
        {
            // 发送连结状态到网页端
            var receiveThread = new Thread(new ThreadStart(HandleLocalHeathCheck));
            receiveThread.Start();
        }


        private bool initSocketWithTryCatch()
        {
            try {
                initSocket();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void ExecuteWithTryCatch(Action action)
        {
            try
            {
                action();
            }
            catch(Exception ex)
            {
                Console.WriteLine("网络异常,重新尝试连接");
                Close();


                var connected = initSocketWithTryCatch();
                while (!connected)
                {
                    Console.WriteLine("连接失败,正在重试...");

                    Thread.Sleep(2000);
                    connected = initSocketWithTryCatch();
                }                
            }
        }
    }
}
