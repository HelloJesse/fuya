using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using Nordasoft.Data.Sql;
using System.Collections.ObjectModel;
using VLP.NAV;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public partial class data_TcpControlCode : BasePage
{
    UserInfo currentUserInfo = null;

    private IPAddress localAddress;// IP地址
    private const int port = 8005;//端口 
    private TcpListener tcpListener;//监听套接字
    private TcpClient tcpClient;//服务端与客户端建立连接 

    private NetworkStream newworkStream;//利用NetworkStream对象与远程主机发送数据或接收数据\ 
    private BinaryReader binaryReader;//读取 BinaryReader与BinaryWriter类在命名空间using System.IO下 
    private BinaryWriter binaryWrite;//写入 

    //private string _ipaddress = "116.62.103.98";//接口命令地址的IP
    private string _ipaddress = "192.168.1.103";//接口命令地址的IP

    protected void Page_Load(object sender, EventArgs e)
    {
        currentUserInfo = GetUserInfo;
        if (IsOut(this.Context)) return;

        string methodName = Request["method"];
        Type type = this.GetType();
        MethodInfo method = type.GetMethod(methodName);
        if (method == null) throw new Exception("method is null");
        try
        {
            BeforeInvoke(methodName);
            method.Invoke(this, null);
        }
        catch (Exception ex)
        {
            Hashtable result = new Hashtable();
            result["error"] = -1;
            result["message"] = ex.Message;
            result["stackTrace"] = ex.StackTrace;
            String json = VLP.JSON.Encode(result);
            Response.Clear();
            Response.Write(json);
        }
        finally
        {
            AfterInvoke(methodName);
        }
    }

    //权限管理
    protected void BeforeInvoke(String methodName)
    {

    }

    //日志管理
    protected void AfterInvoke(String methodName)
    {

    }

    /// <summary>
    /// 开启服务器的按钮
    /// </summary>
    public void onStartTcpServer()
    {
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            onCreateTcpListener(); 
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 创建服务器端对象
    /// </summary>
    private void onCreateTcpListener()
    {
        IPAddress[] listenerIp = Dns.GetHostAddresses(_ipaddress);//返回主机指定的IP地址 
        localAddress = listenerIp[0]; //初始化IP地址为本地地址 
        tcpListener = new TcpListener(localAddress, port);
        //tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();//开始监听 

        Thread thread = new Thread(onListenClientConnect);//启动一个线程接收请求 
        thread.Start();//启动线程 
    }

    /// <summary>
    /// 监听连接
    /// </summary>
    private void onListenClientConnect()
    {
        while (true)
        {
            try
            {
                tcpClient = tcpListener.AcceptTcpClient();//从端口接收一个连接，并赋予它TcpClient对象 
                if (tcpClient != null)
                {
                    newworkStream = tcpClient.GetStream();
                    binaryReader = new BinaryReader(newworkStream);
                    binaryWrite = new BinaryWriter(newworkStream);
                } 
            }
            catch (Exception)
            {
                break;
            } 
        }
    }

    /// <summary>
    /// 关闭服务器的按钮
    /// </summary>
    public void onStopTcpServer()
    {
        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        try
        {
            if (null != binaryReader)
            {
                binaryReader.Close();
            }
            if (null != binaryWrite)
            {
                binaryWrite.Close();
            }
            if (null != tcpClient)
            {
                tcpClient.Close();
            }
            if (null != tcpListener)
            {
                tcpListener.Stop();
            }
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 通话模块
    /// Flag 0 讲话发射 1 接听接收
    /// </summary>
    public void onCallModule()
    {
        string flag = Request.Form.Get("Flag");

        LoadFormResultN result = new LoadFormResultN();
        result.IsOK = true;
        string msg = "";

        try
        {
            byte[] mybyte = null;

            if (flag.Equals("0"))
            {
                mybyte = Nav_onCallModuleSend(out msg);
            }
            if (flag.Equals("1"))
            {
                mybyte = Nav_onCallModuleReceive(out msg);
            }
            if (mybyte != null)
            {
                if (tcpClient == null || newworkStream == null || binaryReader ==null || binaryWrite == null)
                {

                    onCreateTcpListener();

                    //发送命令
                    MemoryStream memory = new MemoryStream();
                    BinaryFormatter format = new BinaryFormatter();
                    format.Serialize(memory, mybyte);
                    byte[] bytes = memory.ToArray();
                    binaryWrite.Write(bytes);
                    binaryWrite.Flush();
                    //SendData(mybyte);
                }
            }
           
        }
        catch (Exception ex)
        {
            result.IsOK = false;
            result.ErrMessage = ex.Message;
        }
        Response.Write(VLP.JSON.Encode(result));
    }

    /// <summary>
    /// 发送命令
    /// </summary>
    /// <param name="bytes"></param>
    private void SendData(byte[] bytes)
    {
        System.Threading.Thread.Sleep(300);  //100毫秒
        //设置服务IP，设置TCP端口号
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse(_ipaddress), port);

        //定义网络类型，数据连接类型和网络协议UDP
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        server.SendTo(bytes, bytes.Length, SocketFlags.None, ip);
    }

    /// <summary>
    /// 通话模块 讲话发射命令
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public byte[] Nav_onCallModuleSend(out string msg)
    {
        byte[] mybtye = new byte[5];//目前是5个字节
        msg = "";
        try
        {
            mybtye[0] = 0xBB;
            mybtye[1] = 0x01;
            mybtye[2] = 0x01;
            mybtye[3] = 0xBD;
            mybtye[4] = 0x0E;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            return null;
        }
        return mybtye;
    }

    /// <summary>
    /// 通话模块 接听接收命令
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public byte[] Nav_onCallModuleReceive(out string msg)
    {
        byte[] mybtye = new byte[5];//目前是5个字节
        msg = "";
        try
        {
            mybtye[0] = 0xBB;
            mybtye[1] = 0x01;
            mybtye[2] = 0x02;
            mybtye[3] = 0xBE;
            mybtye[4] = 0x0E;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            return null;
        }
        return mybtye;
    }
   
    public partial class LoadFormResultN
    {
        /// <summary>
        /// DataList 
        /// </summary>
        public DataTable dataTable = null;
        /// <summary>
        /// 其它
        /// </summary>
        public bool IsOK = false;
        /// <summary>
        /// 正常消息
        /// </summary>
        public string Message = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string ErrMessage = string.Empty;
    }
}