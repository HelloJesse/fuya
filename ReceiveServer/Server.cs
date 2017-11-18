using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using VLP.NAV;
namespace ReceiveServer
{
    /// <summary>
    /// 保存到DB
    /// </summary>
    /// <param name="data"></param>
    public delegate void SaveToDB(VLP.NAV.Common.ReceiveData[] datas);
    public class Server : IDisposable
    {
        private static Nordasoft.Common.IO.FileLog _ErrLog = new Nordasoft.Common.IO.FileLog(AppDomain.CurrentDomain.BaseDirectory, "Err.log");
        /// <summary>
        /// 保存到DB委托
        /// </summary>
        public SaveToDB ToDBHandler { get; set; }
        /// <summary>
        /// 全局缓存
        /// </summary>
        Queue _CacheQueue = new Queue();
        /// <summary>
        /// 全局临时缓存
        /// </summary>
        Queue _TempQueue = null;
        int _Port;
        int _Size;
        System.Timers.Timer _Timer;
        Socket _Socket;
        /// <summary>
        /// 是否正在执行
        /// </summary>
        bool _TimerIsRunning = false;
        bool _CanReceive = false;
        public Server(int port, int size)
        {
            _Port = port;
            _Size = size;
            _Timer = new System.Timers.Timer(1000);
        }

        private string _host;
        private string _password;
        public Server(string host, int port, int size, string password)
        {
            _host = host;
            _Port = port;
            _password = password;
            _Size = size;

            _Timer = new System.Timers.Timer(1000);
        }



        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            _CanReceive = false;
            _Timer.Elapsed += _Timer_Elapsed;
            _TempQueue = new Queue();
            _Timer.Start();

            while (!_CanReceive)
            {
                // 尝试重新连接
                _CanReceive = Connect() && Authenticate();
                if (_CanReceive)
                {
                    try
                    {
                        Receive();
                    }
                    catch (SocketException ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            _CanReceive = false;
            _Socket.Shutdown(SocketShutdown.Receive);
            _Socket.Dispose();
            _Timer.Stop();
            _Timer.Elapsed -= _Timer_Elapsed;
            _Timer.Close();

            ToDB();
        }


     

        private bool Connect()
        {
            try
            {
                var endPoint = new DnsEndPoint(_host, _Port);
                _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _Socket.Connect(endPoint);
            }
            catch (Exception ex)
            {
                var msg = "Connecting to server failed: " + ex.Message;
                _ErrLog.AddLogInfo(msg);
                Console.WriteLine(msg);
                return false;
            }
            return true;
        }

        private bool Authenticate()
        {
            try
            {
                byte[] recBytes = new byte[5];
                int bytes = _Socket.Receive(recBytes, recBytes.Length, 0);
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

                    var sendBytes = _Socket.Send(pwdBytes);

                    Console.WriteLine("password sent");
                    
                }
            }
            catch (Exception ex)
            {
                var msg = "Authenticate failed " + ex.Message;
                _ErrLog.AddLogInfo(msg);
                Console.WriteLine(msg);
                return false;
            }

            return true;
        }

        private void Receive()
        {
            int recv;
            byte[] data = new byte[_Size];
            byte[] tempdata = null;

            while (_CanReceive)
            {
                recv = 0;
                try
                {
                    recv = _Socket.Receive(data, 109, SocketFlags.None);
                }
                catch (SocketException err)
                {
                    _CanReceive = false;
                    throw err;
                }

                if (!DataValidate(data))
                {
                    _ErrLog.AddLogInfo("invalid data skiped");
                    Console.WriteLine("invalid data skiped");
                    continue;
                }
                     

                DateTime receivetime = DateTime.Now;
                tempdata = new byte[104];
                Array.Copy(data, 4, tempdata, 0, 104);
                //将接收数据放入队列
                lock (_TempQueue)
                {
                    Console.WriteLine("Message received from {0}: 数据长度:{1}时间：{2}", _host, recv, DateTime.Now.ToLongTimeString());
                    _TempQueue.Enqueue(new VLP.NAV.Common.ReceiveData(tempdata, receivetime));
                    if (_TempQueue.Count >= 1000)
                    {
                        _ErrLog.AddLogInfo("大于1000");
                        //大于时，转移到全局缓存
                        lock (_CacheQueue)
                        {
                            _CacheQueue.Enqueue(_TempQueue.Clone());
                        }
                        _TempQueue.Clear();
                    }
                }
            }
        }

        private bool DataValidate(byte[] data)
        {
            if (data[0] != 0xEC || data[1] != 0x91 || data[3] != 0xC2)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 定时存入DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_TimerIsRunning)
            {
                Console.WriteLine("TODB正在运行。。。。。");
                //假如上次还没执行完,则跳出
                return;
            }
            _TimerIsRunning = true;
            try
            {
                ToDB();
            }
            catch (Exception err)
            {
                _ErrLog.AddLogInfo("ToDB Error", err.ToString());
            }
            finally
            {
                _TimerIsRunning = false;
            }

        }
        private Queue tempq = new Queue();

        /// <summary>
        /// 存入DB
        /// </summary>
        public void ToDB()
        {
            VLP.NAV.Common.ReceiveData[] datas = null;

            lock (_CacheQueue)
            {
                if (_CacheQueue.Count > 0)
                {
                    Queue q = (Queue)_CacheQueue.Dequeue();
                    while (q != null)
                    {
                        datas = new VLP.NAV.Common.ReceiveData[q.Count];
                        try
                        {
                            ToDB(datas);
                        }
                        catch (Exception err)
                        {
                            datas = null;
                            _ErrLog.AddLogInfo("_CacheQueue Copy Err:" + _CacheQueue.Count.ToString(), err.ToString());
                        }
                        q = (Queue)_CacheQueue.Dequeue();
                    }
                }
            }


            //检查临时缓存
            lock (_TempQueue)
            {
                if (_TempQueue.Count > 0)
                {
                    try
                    {
                        datas = new VLP.NAV.Common.ReceiveData[_TempQueue.Count];
                        _TempQueue.CopyTo(datas, 0);
                        _TempQueue.Clear();
                    }
                    catch (Exception err)
                    {
                        _ErrLog.AddLogInfo("_TempQueue Copy Err:" + _TempQueue.Count.ToString(), err.ToString());
                    }
                }
            }
            ToDB(datas);

           
        }
        private void ToDB(VLP.NAV.Common.ReceiveData[] datas)
        {
            if (datas != null && ToDBHandler != null)
            {
                try
                {

                    var sb = new StringBuilder();
                    foreach (var arr in datas)
                    {
                        foreach (var b in arr.Data)
                        {
                            sb.Append(Common.putintTo16string(b, 2));
                            sb.Append(" ");
                        }
                    }
                    

                    ToDBHandler(datas);
                }
                catch (Exception err)
                {
                    _ErrLog.AddLogInfo("ToDBHandler Err:", err.ToString());
                }
            }
        }
        //}

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    Close();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Server() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
   
}
