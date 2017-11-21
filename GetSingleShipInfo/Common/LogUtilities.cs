//-----------------------------------------------------------------
// All Rights Reserved , Copyright (C) 2016 , Hairihan TECH, Ltd.  
//-----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace GetSingleShipInfo
{
    using DotNet.Utilities;
    using DotNet.Utilities.Log;

    /// <summary>日志等级</summary>
    public enum LogLevels : byte
    {
        /// <summary>打开所有日志记录</summary>
        All = 0,

        /// <summary>最低调试。细粒度信息事件对调试应用程序非常有帮助</summary>
        Debug,

        /// <summary>普通消息。在粗粒度级别上突出强调应用程序的运行过程</summary>
        Info,

        /// <summary>警告</summary>
        Warn,

        /// <summary>错误</summary>
        Error,

        /// <summary>严重错误</summary>
        Fatal,

        /// <summary>关闭所有日志记录</summary>
        Off = 0xFF
    }

    /// <summary>
    /// 日志类，包含跟踪调试功能
    /// 
    /// 
    /// 修改记录
    ///     2017.04.15 版本：1.1    刘海洋   增加日志文件大小限制，超过1G后从头开始重新写日志，覆盖旧的内容。
    ///     2016.07.29 版本：1.0    刘海洋
    ///	
    /// <author>
    ///		<name>刘海洋</name>
    ///		<date>2016.07.29</date>
    /// </author> 
    /// </summary>
    /// <remarks>
    /// 该静态类包括写日志、写调用栈和Dump进程内存等调试功能。
    /// 
    /// 默认写日志到文本文件，可通过修改<see cref="Log"/>属性来增加日志输出方式。
    /// 对于控制台工程，可以直接通过<see cref="UseConsole"/>方法，把日志输出重定向为控制台输出，并且可以为不同线程使用不同颜色。
    /// </remarks>
    public static class LogUtilities
    {
        /// <summary>
        /// 在本地写入错误日志
        /// </summary>
        /// <param name="exception"></param> 错误信息
        public static void WriteLog(Exception exception)
        {
            WriteException(exception);
            //lock (writeFile)
            //{
            //    try
            //    {
            //        DateTime dt = DateTime.Now;
            //        string directPath = string.Format(@"{0}\Log",
            //                                          AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            //        //记录错误日志文件的路径
            //        if (!Directory.Exists(directPath))
            //        {
            //            Directory.CreateDirectory(directPath);
            //        }
            //        directPath += string.Format(@"\{0}.log", dt.ToString("yyyy-MM-dd"));
            //        if (streamWriter == null)
            //        {
            //            InitLog(directPath);
            //        }
            //        streamWriter.WriteLine("***********************************************************************");
            //        streamWriter.WriteLine(dt.ToString("HH:mm:ss"));
            //        // streamWriter.WriteLine("输出信息：错误信息");
            //        if (exception != null)
            //        {
            //            string message = "错误对象:" + exception.Source.Trim() + Environment.NewLine
            //                           + "异常方法:" + exception.TargetSite.Name + Environment.NewLine
            //                           + "堆栈信息:" + exception.GetType() + ":" + exception.Message.Trim() + Environment.NewLine
            //                           + exception.StackTrace.Replace(" ", "");
            //            streamWriter.WriteLine("异常信息：\r\n" + message);
            //        }
            //    }
            //    finally
            //    {
            //        if (streamWriter != null)
            //        {
            //            streamWriter.Flush();
            //            streamWriter.Dispose();
            //            streamWriter = null;
            //        }
            //    }
            //}
        }

        public static void WriteLog(string exception)
        {
            WriteLine(exception);
        }

        private static void InitLog(string filePath)
        {
            LogPath = filePath;
            //streamWriter = !File.Exists(filPath) ? File.CreateText(filPath) : File.AppendText(filPath);
        }

        #region 写日志
        /// <summary>文本文件日志</summary>
        private static ILog _Log;
        /// <summary>日志提供者，默认使用文本文件日志</summary>
        public static ILog Log { get { InitLog(); return _Log; } set { _Log = value; } }

        /// <summary>输出日志</summary>
        /// <param name="msg">信息</param>
        public static void WriteLine(String msg)
        {
            InitLog();

            Log.Info(msg);
        }

        /// <summary>写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLine(String format, params Object[] args)
        {
            InitLog();

            Log.Info(format, args);
        }

        /// <summary>异步写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLineAsync(String format, params Object[] args)
        {
            ThreadPool.QueueUserWorkItem(s => WriteLine(format, args));
        }

        /// <summary>输出异常日志</summary>
        /// <param name="ex">异常信息</param>
        //[Obsolete("不再支持！")]
        public static void WriteException(Exception ex)
        {
            InitLog();

            Log.Error("{0}", ex);
        }
        #endregion

        #region 构造
        static LogUtilities()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

#if Android
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var msg = "" + e.ExceptionObject;
            WriteLine(msg);
            if (e.IsTerminating)
            {
                Log.Fatal("异常退出！");
            }
        }
#endif

        static object _lock = new object();

        /// <summary>
        /// 2012.11.05 修正初次调用的时候，由于同步BUG，导致Log为空的问题。
        /// </summary>
        static void InitLog()
        {
            if (_Log != null) return;

            lock (_lock)
            {
                if (_Log != null) return;

#if !Android
                _Log = TextFileLog.Create(LogPath);
#else
                _Log = new NetworkLog();
#endif
            }

            WriteVersion();
        }
        #endregion

        #region 使用控制台输出
#if !Android
        /// <summary>使用控制台输出日志，只能调用一次</summary>
        /// <param name="useColor">是否使用颜色，默认使用</param>
        /// <param name="useFileLog">是否同时使用文件日志，默认使用</param>
        public static void UseConsole(Boolean useColor = true, Boolean useFileLog = true)
        {
            if (!Runtime.IsConsole) return;

            // 适当加大控制台窗口
            try
            {
                if (Console.WindowWidth <= 80) Console.WindowWidth = Console.WindowWidth * 3 / 2;
                if (Console.WindowHeight <= 25) Console.WindowHeight = Console.WindowHeight * 3 / 2;
            }
            catch { }

            var clg = _Log as ConsoleLog;
            var ftl = _Log as TextFileLog;
            var cmp = _Log as CompositeLog;
            if (cmp != null)
            {
                ftl = cmp.Get<TextFileLog>();
                clg = cmp.Get<ConsoleLog>();
            }

            // 控制控制台日志
            if (clg == null)
                clg = new ConsoleLog { UseColor = useColor };
            else
                clg.UseColor = useColor;

            if (!useFileLog)
            {
                // 如果原有提供者是文本日志，则直接替换
                if (ftl != null)
                {
                    Log = clg;
                    ftl.Dispose();
                }
                // 否则组件复合日志
                else
                {
                    if (cmp != null)
                    {
                        cmp.Remove(clg);
                        if (cmp.Logs.Count == 0) _Log = null;
                    }

                    cmp = new CompositeLog();
                    cmp.Add(clg);
                    if (_Log != null) cmp.Add(_Log);
                    Log = cmp;
                }
            }
            else
            {
                cmp = new CompositeLog();
                cmp.Add(clg);
                if (ftl == null)
                {
                    //if (_Log != null) cmp.Add(_Log);
                    ftl = TextFileLog.Create(null);
                }
                cmp.Add(ftl);
                Log = cmp;
            }

            WriteVersion();
        }
#endif
        #endregion

        #region 拦截WinForm异常
#if !Android
        private static Int32 initWF = 0;
        private static Boolean _ShowErrorMessage;
        //private static String _Title;

        /// <summary>拦截WinForm异常并记录日志，可指定是否用<see cref="MessageBox"/>显示。</summary>
        /// <param name="showErrorMessage">发为捕获异常时，是否显示提示，默认显示</param>
        public static void UseWinForm(Boolean showErrorMessage = true)
        {
            _ShowErrorMessage = showErrorMessage;

            if (initWF > 0 || Interlocked.CompareExchange(ref initWF, 1, 0) != 0) return;
            //if (!Application.MessageLoop) return;

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var show = _ShowErrorMessage && Application.MessageLoop;
            var msg = "" + e.ExceptionObject;
            WriteLine(msg);
            if (e.IsTerminating)
            {
                //WriteLine("异常退出！");
                Log.Fatal("异常退出！" + msg);
                //LogUtilities.WriteMiniDump(null);
                if (show) MessageBox.Show(msg, "异常退出", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (show) MessageBox.Show(msg, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            WriteException(e.Exception);

            var show = _ShowErrorMessage && Application.MessageLoop;
            if (show) MessageBox.Show("" + e.Exception, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>在WinForm控件上输出日志，主要考虑非UI线程操作</summary>
        /// <remarks>不是常用功能，为了避免干扰常用功能，保持UseWinForm开头</remarks>
        /// <param name="control">要绑定日志输出的WinForm控件</param>
        /// <param name="useFileLog">是否同时使用文件日志，默认使用</param>
        /// <param name="maxLines">最大行数</param>
        public static void UseWinFormControl(this Control control, Boolean useFileLog = true, Int32 maxLines = 1000)
        {
            var clg = _Log as TextControlLog;
            var ftl = _Log as TextFileLog;
            var cmp = _Log as CompositeLog;
            if (cmp != null)
            {
                ftl = cmp.Get<TextFileLog>();
                clg = cmp.Get<TextControlLog>();
            }

            // 控制控制台日志
            if (clg == null) clg = new TextControlLog();
            clg.Control = control;
            clg.MaxLines = maxLines;

            if (!useFileLog)
            {
                Log = clg;
                if (ftl != null) ftl.Dispose();
            }
            else
            {
                if (ftl == null) ftl = TextFileLog.Create(null);
                Log = new CompositeLog(clg, ftl);
            }
        }
#endif
        #endregion

        #region 属性
        private static Boolean? _Debug;
        /// <summary>是否调试。如果代码指定了值，则只会使用代码指定的值，否则每次都读取配置。</summary>
        public static Boolean Debug
        {
            get
            {
                if (_Debug != null) return _Debug.Value;

                try
                {
                    //return Config.GetConfig<Boolean>("NewLife.Debug", Config.GetConfig<Boolean>("Debug", false));
                    //return Config.GetMutilConfig<Boolean>(false, "NewLife.Debug", "Debug");
                    return SystemSetting.Current.Debug;
                }
                catch { return false; }
            }
            set { _Debug = value; }
        }

        private static String _LogPath;
        /// <summary>文本日志目录</summary>
        public static String LogPath
        {
            get
            {
                // Web日志目录默认放到外部
                //if (_LogPath == null) _LogPath = Config.GetConfig<String>("NewLife.LogPath", Runtime.IsWeb ? "../Log" : "Log");
                if (_LogPath == null) _LogPath = SystemSetting.Current.LogPath;
                return _LogPath;
            }
            set { _LogPath = value; }
        }

        private static String _TempPath;
        /// <summary>临时目录</summary>
        public static String TempPath
        {
            get
            {
                if (_TempPath != null) return _TempPath;

                // 这里是TempPath而不是_TempPath，因为需要格式化处理一下
                //TempPath = Config.GetConfig<String>("NewLife.TempPath", "XTemp");
                _TempPath = SystemSetting.Current.TempPath.GetFullPath();
                return _TempPath;
            }
            set
            {
                _TempPath = value.GetFullPath();
            }
        }
        #endregion

        #region Dump
        /// <summary>写当前线程的MiniDump</summary>
        /// <param name="dumpFile">如果不指定，则自动写入日志目录</param>
        public static void WriteMiniDump(String dumpFile)
        {
            if (String.IsNullOrEmpty(dumpFile))
            {
                dumpFile = String.Format("{0:yyyyMMdd_HHmmss}.dmp", DateTime.Now);
                if (!String.IsNullOrEmpty(LogPath)) dumpFile = Path.Combine(LogPath, dumpFile);
            }

            MiniDump.TryDump(dumpFile, MiniDump.MiniDumpType.WithFullMemory);
        }

        /// <summary>
        /// 该类要使用在windows 5.1 以后的版本，如果你的windows很旧，就把Windbg里面的dll拷贝过来，一般都没有问题。
        /// DbgHelp.dll 是windows自带的 dll文件 。
        /// </summary>
        static class MiniDump
        {
            [DllImport("DbgHelp.dll")]
            private static extern Boolean MiniDumpWriteDump(IntPtr hProcess, Int32 processId, IntPtr fileHandle, MiniDumpType dumpType, ref MinidumpExceptionInfo excepInfo, IntPtr userInfo, IntPtr extInfo);

            /// <summary>MINIDUMP_EXCEPTION_INFORMATION</summary>
            struct MinidumpExceptionInfo
            {
                public UInt32 ThreadId;
                public IntPtr ExceptionPointers;
                public UInt32 ClientPointers;
            }

            [DllImport("kernel32.dll")]
            private static extern uint GetCurrentThreadId();

            public static Boolean TryDump(String dmpPath, MiniDumpType dmpType)
            {
                //使用文件流来创健 .dmp文件
                using (var stream = new FileStream(dmpPath, FileMode.Create))
                {
                    //取得进程信息
                    var process = Process.GetCurrentProcess();

                    // MINIDUMP_EXCEPTION_INFORMATION 信息的初始化
                    var mei = new MinidumpExceptionInfo();

                    mei.ThreadId = (UInt32)GetCurrentThreadId();
                    mei.ExceptionPointers = Marshal.GetExceptionPointers();
                    mei.ClientPointers = 1;

                    //这里调用的Win32 API
                    var fileHandle = stream.SafeFileHandle.DangerousGetHandle();
                    var res = MiniDumpWriteDump(process.Handle, process.Id, fileHandle, dmpType, ref mei, IntPtr.Zero, IntPtr.Zero);

                    //清空 stream
                    stream.Flush();
                    stream.Close();

                    return res;
                }
            }

            public enum MiniDumpType
            {
                None = 0x00010000,
                Normal = 0x00000000,
                WithDataSegs = 0x00000001,
                WithFullMemory = 0x00000002,
                WithHandleData = 0x00000004,
                FilterMemory = 0x00000008,
                ScanMemory = 0x00000010,
                WithUnloadedModules = 0x00000020,
                WithIndirectlyReferencedMemory = 0x00000040,
                FilterModulePaths = 0x00000080,
                WithProcessThreadData = 0x00000100,
                WithPrivateReadWriteMemory = 0x00000200,
                WithoutOptionalData = 0x00000400,
                WithFullMemoryInfo = 0x00000800,
                WithThreadInfo = 0x00001000,
                WithCodeSegs = 0x00002000
            }
        }
        #endregion

        #region 调用栈
        /// <summary>堆栈调试。
        /// 输出堆栈信息，用于调试时处理调用上下文。
        /// 本方法会造成大量日志，请慎用。
        /// </summary>
        public static void DebugStack()
        {
            var msg = GetCaller(2, 0, Environment.NewLine);
            WriteLine("调用堆栈：" + Environment.NewLine + msg);
        }

        /// <summary>堆栈调试。</summary>
        /// <param name="maxNum">最大捕获堆栈方法数</param>
        public static void DebugStack(int maxNum)
        {
            var msg = GetCaller(2, maxNum, Environment.NewLine);
            WriteLine("调用堆栈：" + Environment.NewLine + msg);
        }

        /// <summary>堆栈调试</summary>
        /// <param name="start">开始方法数，0是DebugStack的直接调用者</param>
        /// <param name="maxNum">最大捕获堆栈方法数</param>
        public static void DebugStack(int start, int maxNum)
        {
            // 至少跳过当前这个
            if (start < 1) start = 1;
            var msg = GetCaller(start + 1, maxNum, Environment.NewLine);
            WriteLine("调用堆栈：" + Environment.NewLine + msg);
        }

        /// <summary>获取调用栈</summary>
        /// <param name="start">要跳过的方法数，默认1，也就是跳过GetCaller</param>
        /// <param name="maxNum">最大层数</param>
        /// <param name="split">分割符号，默认左箭头加上换行</param>
        /// <returns></returns>
        public static String GetCaller(int start = 1, int maxNum = 0, String split = null)
        {
            // 至少跳过当前这个
            if (start < 1) start = 1;
            var st = new StackTrace(start, true);

            if (String.IsNullOrEmpty(split)) split = "<-" + Environment.NewLine;

            Type last = null;
            var asm = Assembly.GetEntryAssembly();
            var entry = asm == null ? null : asm.EntryPoint;

            int count = st.FrameCount;
            var sb = new StringBuilder(count * 20);
            //if (maxNum > 0 && maxNum < count) count = maxNum;
            for (int i = 0; i < count && maxNum > 0; i++)
            {
                var sf = st.GetFrame(i);
                var method = sf.GetMethod();

                // 跳过<>类型的匿名方法
                if (method == null || String.IsNullOrEmpty(method.Name) || method.Name[0] == '<' && method.Name.Contains(">")) continue;

                // 跳过有[DebuggerHidden]特性的方法
                if (method.GetCustomAttribute<DebuggerHiddenAttribute>() != null) continue;

                var type = method.DeclaringType ?? method.ReflectedType;
                if (type != null) sb.Append(type.Name);
                sb.Append(".");

                var name = method.ToString();
                // 去掉前面的返回类型
                var p = name.IndexOf(" ");
                if (p >= 0) name = name.Substring(p + 1);
                // 去掉前面的System
                name = name
                    .Replace("System.Web.", null)
                    .Replace("System.", null);

                sb.Append(name);

                // 如果到达了入口点，可以结束了
                if (method == entry) break;

                if (i < count - 1) sb.Append(split);

                last = type;

                maxNum--;
            }
            return sb.ToString();
        }
        #endregion

        #region 版本信息
        static void WriteVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            WriteVersion(asm);

            var asm2 = Assembly.GetEntryAssembly();
            if (asm2 != asm) WriteVersion(asm2);
        }

        /// <summary>输出程序集版本</summary>
        /// <param name="asm"></param>
        public static void WriteVersion(this Assembly asm)
        {
            if (asm == null) return;

            var asmx = AssemblyHelper.Create(asm);
            if (asmx != null) WriteLine("{0,-12} v{1,-13} Build {2:yyyy-MM-dd HH:mm:ss}", asmx.Name, asmx.FileVersion, asmx.Compile);
        }
        #endregion
    }

}

/// <summary>
/// 日志相关的接口、实现。
/// 
/// 修改记录
///     2016.07.29 版本：1.0    刘海洋
///	
/// <author>
///		<name>刘海洋</name>
///		<date>2016.07.29</date>
/// </author> 
/// </summary>
namespace DotNet.Utilities.Log
{

    /// <summary>日志接口</summary>
    public interface ILog
    {
        /// <summary>写日志</summary>
        /// <param name="level">日志级别</param>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Write(LogLevels level, String format, params Object[] args);

        /// <summary>调试日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Debug(String format, params Object[] args);

        /// <summary>信息日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Info(String format, params Object[] args);

        /// <summary>警告日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Warn(String format, params Object[] args);

        /// <summary>错误日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Error(String format, params Object[] args);

        /// <summary>严重错误日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Fatal(String format, params Object[] args);

        /// <summary>是否启用日志</summary>
        Boolean Enable { get; set; }

        /// <summary>日志等级，只输出大于等于该级别的日志，默认Info，打开NewLife.Debug时默认为最低的Debug</summary>
        LogLevels Level { get; set; }
    }

    /// <summary>日志基类。提供日志的基本实现</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public abstract class Logger : ILog
    {
        #region 主方法
        /// <summary>调试日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public virtual void Debug(String format, params Object[] args) { Write(LogLevels.Debug, format, args); }

        /// <summary>信息日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public virtual void Info(String format, params Object[] args) { Write(LogLevels.Info, format, args); }

        /// <summary>警告日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public virtual void Warn(String format, params Object[] args) { Write(LogLevels.Warn, format, args); }

        /// <summary>错误日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public virtual void Error(String format, params Object[] args) { Write(LogLevels.Error, format, args); }

        /// <summary>严重错误日志</summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public virtual void Fatal(String format, params Object[] args) { Write(LogLevels.Fatal, format, args); }
        #endregion

        #region 核心方法
        /// <summary>写日志</summary>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public virtual void Write(LogLevels level, String format, params Object[] args)
        {
            if (Enable && level >= Level) OnWrite(level, format, args);
        }

        /// <summary>写日志</summary>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        protected abstract void OnWrite(LogLevels level, String format, params Object[] args);

        ///// <summary>输出异常日志</summary>
        ///// <param name="ex">异常信息</param>
        //public abstract void WriteException(Exception ex);

        ///// <summary>写日志</summary>
        ///// <param name="format">格式化字符串</param>
        ///// <param name="args">格式化参数</param>
        //public abstract void WriteLine(LogLevels level, String format, params Object[] args);
        #endregion

        #region 辅助方法
        /// <summary>格式化参数，特殊处理异常和时间</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual String Format(String format, Object[] args)
        {
            //处理时间的格式化
            if (args != null && args.Length > 0)
            {
                // 特殊处理异常
                if (args.Length == 1 && args[0] is Exception && (String.IsNullOrEmpty(format) || format == "{0}"))
                {
                    return "" + args[0];
                }

                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] != null && args[i].GetType() == typeof(DateTime))
                    {
                        // 根据时间值的精确度选择不同的格式化输出
                        var dt = (DateTime)args[i];
                        if (dt.Millisecond > 0)
                            args[i] = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        else if (dt.Hour > 0 || dt.Minute > 0 || dt.Second > 0)
                            args[i] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        else
                            args[i] = dt.ToString("yyyy-MM-dd");
                    }
                }
            }
            if (args == null || args.Length < 1) return format;

            //format = format.Replace("{", "{{").Replace("}", "}}");

            return String.Format(format, args);
        }
        #endregion

        #region 属性
        private Boolean _Enable = true;
        /// <summary>是否启用日志。默认true</summary>
        public virtual Boolean Enable { get { return _Enable; } set { _Enable = value; } }

        private LogLevels? _Level;
        /// <summary>日志等级，只输出大于等于该级别的日志，默认Info，打开NewLife.Debug时默认为最低的Debug</summary>
        public LogLevels Level
        {
            get
            {
                if (_Level != null) return _Level.Value;

                return SystemSetting.Current.LogLevel;
                //try
                //{
                //    var def = LogLevels.Info;
                //    //if (Config.GetConfig<Boolean>("NewLife.Debug")) def = LogLevels.Debug;
                //    if (LogUtilities.Debug) def = LogLevels.Debug;

                //    return Config.GetConfig<LogLevel>("NewLife.LogLevel", def);
                //}
                //catch { return LogLevels.Info; }
            }
            set { _Level = value; }
        }
        #endregion

        #region 静态空实现
        private static ILog _Null = new NullLogger();
        /// <summary>空日志实现</summary>
        public static ILog Null { get { return _Null; } }

        class NullLogger : Logger
        {
            public override bool Enable { get { return false; } set { } }

            protected override void OnWrite(LogLevels level, string format, params object[] args) { }
        }
        #endregion

        #region 日志头
        /// <summary>输出日志头，包含所有环境信息</summary>
        protected static String GetHead()
        {
            var process = Process.GetCurrentProcess();
            var name = String.Empty;
            var asm = Assembly.GetEntryAssembly();
            if (asm != null)
            {
                if (String.IsNullOrEmpty(name))
                {
                    var att = asm.GetCustomAttribute<AssemblyTitleAttribute>();
                    if (att != null) name = att.Title;
                }

                if (String.IsNullOrEmpty(name))
                {
                    var att = asm.GetCustomAttribute<AssemblyProductAttribute>();
                    if (att != null) name = att.Product;
                }

                if (String.IsNullOrEmpty(name))
                {
                    var att = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
                    if (att != null) name = att.Description;
                }
            }
            if (String.IsNullOrEmpty(name))
            {
                try
                {
                    if(!process.HasExited)
                        name = process.ProcessName;
                }
                catch { }
            }
            var sb = new StringBuilder();
            sb.AppendFormat("#Software: {0}\r\n", name);
            sb.AppendFormat("#ProcessID: {0}{1}\r\n", process.Id, Runtime.Is64BitProcess ? " x64" : "");
            sb.AppendFormat("#AppDomain: {0}\r\n", AppDomain.CurrentDomain.FriendlyName);

            var fileName = String.Empty;
            try
            {
                fileName = process.StartInfo.FileName;
#if !Android
                // MonoAndroid无法识别MainModule，致命异常
                if (fileName.IsNullOrWhiteSpace()) fileName = process.MainModule.FileName;
#endif
            }
            catch { }
            if (!String.IsNullOrEmpty(fileName)) sb.AppendFormat("#FileName: {0}\r\n", fileName);

            // 应用域目录
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            sb.AppendFormat("#BaseDirectory: {0}\r\n", baseDir);

            // 当前目录。如果由别的进程启动，默认的当前目录就是父级进程的当前目录
            var curDir = Environment.CurrentDirectory;
            //if (!curDir.EqualIC(baseDir) && !(curDir + "\\").EqualIC(baseDir))
            if (!baseDir.EqualIgnoreCase(curDir, curDir + "\\"))
                sb.AppendFormat("#CurrentDirectory: {0}\r\n", curDir);

            // 命令行不为空，也不是文件名时，才输出
            // 当使用cmd启动程序时，这里就是用户输入的整个命令行，所以可能包含空格和各种符号
            var line = Environment.CommandLine;
            if (!String.IsNullOrEmpty(line))
            {
                line = line.Trim().TrimStart('\"');
                if (!String.IsNullOrEmpty(fileName) && line.StartsWithIgnoreCase(fileName))
                    line = line.Substring(fileName.Length).TrimStart().TrimStart('\"').TrimStart();
                if (!String.IsNullOrEmpty(line))
                {
                    sb.AppendFormat("#CommandLine: {0}\r\n", line);
                }
            }

#if Android
            sb.AppendFormat("#ApplicationType: {0}\r\n", "Android");
#else
            sb.AppendFormat("#ApplicationType: {0}\r\n", Runtime.IsWeb ? "Web" : (Runtime.IsConsole ? "Console" : "WinForm"));
#endif
            sb.AppendFormat("#CLR: {0}\r\n", Environment.Version);

            sb.AppendFormat("#OS: {0}, {1}/{2}\r\n", Runtime.OSName, Environment.UserName, Environment.MachineName);
#if !Android
            var hi = HardInfo.Current;
            sb.AppendFormat("#CPU: {0}\r\n", hi.Processors);
            sb.AppendFormat("#Memory: {0:n0}M/{1:n0}M\r\n", Runtime.AvailableMemory,
                Runtime.PhysicalMemory);
#endif

            sb.AppendFormat("#Date: {0:yyyy-MM-dd}\r\n", DateTime.Now);
            sb.AppendFormat("#字段: 时间 线程ID 线程池Y网页W普通N 线程名 消息内容\r\n");
            sb.AppendFormat("#Fields: Time ThreadID IsPoolThread ThreadName Message\r\n");

            return sb.ToString();
        }
        #endregion
    }

    /// <summary>
    /// 文本文件日志类。提供向文本文件写日志的能力
    /// 
    /// 修改记录
    ///     2017.04.17 版本：1.2    刘海洋   增加分类，异常类信息单独写一个日志文件。
    ///     2017.04.15 版本：1.1    刘海洋   增加日志文件大小限制，超过1G后从头开始重新写日志，覆盖旧的内容。
    /// </summary>
    public class TextFileLog : Logger, IDisposable
    {
        #region 构造
        private TextFileLog(String path, Boolean isfile)
        {
            if (!isfile)
                LogPath = path;
            else
                LogFile = path;
        }
        /// <summary>该构造函数没有作用，为了继承而设置</summary>
        public TextFileLog() { }

        static DictionaryCache<String, TextFileLog> cache = new DictionaryCache<String, TextFileLog>(StringComparer.OrdinalIgnoreCase);
        /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
        /// <param name="path">日志目录或日志文件路径</param>
        /// <returns></returns>
        public static TextFileLog Create(String path)
        {
            if (String.IsNullOrEmpty(path)) path = LogUtilities.LogPath;

            String key = path.ToLower();
            return cache.GetItem<String>(key, path, (k, p) => new TextFileLog(p, false));
        }

        /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
        /// <param name="path">日志目录或日志文件路径</param>
        /// <returns></returns>
        public static TextFileLog CreateFile(String path)
        {
            if (String.IsNullOrEmpty(path)) return Create(path);

            String key = path.ToLower();
            return cache.GetItem<String>(key, path, (k, p) => new TextFileLog(p, true));
        }

        /// <summary>销毁</summary>
        public void Dispose()
        {
            if (LogWriter != null)
            {
                LogWriter.Dispose();
                LogWriter = null;
            }

            if (ErrorWriter != null)
            {
                ErrorWriter.Dispose();
                ErrorWriter = null;
            }
        }
        #endregion

        #region 属性
        private String _LogFile;
        /// <summary>日志文件</summary>
        public String LogFile { get { return _LogFile; } set { _LogFile = value; } }

        private String _CurrentLogFile;
        private string _CurrentErrorLogFile;

        /// <summary>当前写入的日志文件</summary>
        public String GetCurrentLogFile(LogLevels logLevel = LogLevels.All)
        {
            string logFile = "";
            switch (logLevel)
           {
                case LogLevels.Error:
                    logFile = _CurrentErrorLogFile;
                    break;
                default:
                    logFile = _CurrentLogFile;
                    break;
            }
            return logFile;
        }

        private String _LogPath;
        /// <summary>日志目录</summary>
        public String LogPath
        {
            get
            {
                if (String.IsNullOrEmpty(_LogPath) && !String.IsNullOrEmpty(LogFile))
                    _LogPath = Path.GetDirectoryName(LogFile).GetFullPath().EnsureEnd(@"\");
                return _LogPath;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    _LogPath = value;
                else
                    _LogPath = value.GetFullPath().EnsureEnd(@"\");
            }
        }

        /// <summary>是否当前进程的第一次写日志</summary>
        private Boolean isFirst = false;
        #endregion

        #region 内部方法
        private StreamWriter LogWriter;
        private StreamWriter ErrorWriter;

        /// <summary>初始化日志记录文件</summary>
        private StreamWriter InitLog(LogLevels logLevel = LogLevels.All)
        {
            String path = LogPath.EnsureDirectory(false);


            StreamWriter writer = null;
            var logfile = LogFile;

            if (!String.IsNullOrEmpty(logfile))
            {
                switch (logLevel)
                {
                    case LogLevels.Error:
                        logfile = Path.GetFileNameWithoutExtension(logfile) + ".error." + Path.GetExtension(logfile);
                        break;
                }
                try
                {
                    var stream = new FileStream(logfile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    //如果是打开已有文件，移动到文件末尾。
                    if (stream.Length > 0)
                    {
                        stream.Position = stream.Length;
                    }
                    writer = new StreamWriter(stream, Encoding.UTF8);
                    writer.AutoFlush = true;
                }
                catch { }
            }

            if (writer == null)
            {
                logfile = Path.Combine(path, DateTime.Now.ToString("yyyy_MM_dd") + ".log");
                switch (logLevel)
                {
                    case LogLevels.Error:
                        logfile = Path.Combine(path, Path.GetFileNameWithoutExtension(logfile) + ".error" + Path.GetExtension(logfile));
                        break;
                }
                int i = 0;
                while (i < 10)
                {
                    try
                    {
                        var stream = new FileStream(logfile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                        //如果是打开已有文件，移动到文件末尾。
                        if (stream.Length > 0)
                        {
                            stream.Position = stream.Length + 1;
                        }
                        writer = new StreamWriter(stream, Encoding.UTF8);
                        writer.AutoFlush = true;
                        break;
                    }
                    catch
                    {
                        if (logfile.EndsWith("_" + i + ".log"))
                            logfile = logfile.Replace("_" + i + ".log", "_" + (++i) + ".log");
                        else
                            logfile = logfile.Replace(@".log", @"_0.log");
                    }
                }
            }
            if (writer == null) throw new Exception("无法写入日志！");

            // 这里赋值，会导致log文件名不会随时间而自动改变
            //LogFile = logfile;

            if (!isFirst)
            {
                isFirst = true;

                // 通过判断LogWriter.BaseStream.Length，解决有时候日志文件为空但仍然加空行的问题
                //if (File.Exists(logfile) && LogWriter.BaseStream.Length > 0) LogWriter.WriteLine();
                // 因为指定了编码，比如UTF8，开头就会写入3个字节，所以这里不能拿长度跟0比较
                if (writer.BaseStream.Length > 10) writer.WriteLine();

                //WriteHead(writer);
                writer.Write(GetHead());
            }

            switch (logLevel)
            {
                case LogLevels.Error:
                    ErrorWriter = writer;
                    _CurrentErrorLogFile = logfile;
                    break;
                default:
                    LogWriter = writer;
                    _CurrentLogFile = logfile;
                    break;
            }

            return writer;
        }

        /// <summary>停止日志</summary>
        protected virtual void CloseWriter(Object obj)
        {
            LogLevels logLevel = LogLevels.All;
            if (obj is LogLevels)
                logLevel = (LogLevels)obj;
            var writer = GetLogWriter(logLevel);
            if (writer == null) return;
            lock (Log_Lock)
            {
                try
                {
                    if (writer == null) return;
                    writer.Close();
                    writer.Dispose();
                    switch (logLevel)
                    {
                        case LogLevels.Error:
                            ErrorWriter = null;
                            break;
                        default:
                            LogWriter = null;
                            break;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 根据日志级别获取关联的日志写入器。
        /// </summary>
        /// <param name="logLevel">日志级别。</param>
        /// <returns></returns>
        private StreamWriter GetLogWriter(LogLevels logLevel)
        {
            StreamWriter writer = null;
            switch (logLevel)
            {
                case LogLevels.Error:
                    writer = ErrorWriter;
                    break;
                default:
                    writer = LogWriter;
                    break;
            }

            return writer;
        }
        #endregion

        #region 异步写日志
        private System.Threading.Timer AutoCloseWriterTimer;
        private object Log_Lock = new object();
        private Boolean LastIsNewLine = true;

        /// <summary>使用线程池线程异步执行日志写入动作</summary>
        /// <param name="e"></param>
        protected virtual void PerformWriteLog(WriteLogEventArgs e)
        {
            lock (Log_Lock)
            {
                try
                {
                    LogLevels logLevel = e.Level;
                    //日志信息中包含了异常，则认为是错误信息。
                    if (e.Exception != null || (e.Message != null
                         && e.Message.Contains("Exception")))
                        logLevel = LogLevels.Error;

                    if (logLevel != LogLevels.Error)
                        logLevel = LogLevels.All;

                    StreamWriter writer = GetLogWriter(logLevel);

                    // 初始化日志读写器
                    if (writer == null) writer = InitLog(logLevel);

                    string currentLogFile = GetCurrentLogFile(logLevel);

                    //检查当前日期和日志文件中的包含的文件名是否匹配，不匹配生成新的日志文件;
                    if (currentLogFile != LogFile)
                    {
                        string logDir = Path.GetDirectoryName(currentLogFile);
                        string fileName = Path.GetFileName(currentLogFile);
                        string logFileName = fileName.Substring(0, fileName.IndexOf("."));
                        string newLogFileName = DateTime.Now.ToString("yyyy_MM_dd");
                        if (!logFileName.StartsWith(newLogFileName))
                        {
                            CloseWriter(logLevel);
                            InitLog(logLevel);
                        }
                    }

                    // 写日志
                    if (LastIsNewLine)
                    {
                        // 如果上一次是换行，则这次需要输出行头信息
                        if (e.IsNewLine)
                            writer.WriteLine(e.ToString());
                        else
                        {
                            writer.Write(e.ToString());
                            LastIsNewLine = false;
                        }
                    }
                    else
                    {
                        // 如果上一次不是换行，则这次不需要行头信息
                        var msg = e.Message + e.Exception;
                        if (e.IsNewLine)
                        {
                            writer.WriteLine(msg);
                            LastIsNewLine = true;
                        }
                        else
                            writer.Write(msg);
                    }

                    //每个日志文件最大充许1G，超过从头开始写。1000000000
                    if (writer.BaseStream.Length > 1000000000 && writer.BaseStream.Position >= writer.BaseStream.Length)
                    {
                        writer.Flush();
                        writer.BaseStream.Position = 0;
                        writer.Write(GetHead());
                    }

                    // 声明自动关闭日志读写器的定时器。无限延长时间，实际上不工作
                    if (AutoCloseWriterTimer == null) AutoCloseWriterTimer = new System.Threading.Timer(new TimerCallback(CloseWriter), logLevel, Timeout.Infinite, Timeout.Infinite);
                    // 改变定时器为5秒后触发一次。如果5秒内有多次写日志操作，估计定时器不会触发，直到空闲五秒为止
                    AutoCloseWriterTimer.Change(5000, Timeout.Infinite);

                    // 清空日志对象
                    e.Clear();
                }
                catch { }
            }
        }
        #endregion

        #region 写日志
        /// <summary>写日志</summary>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        protected override void OnWrite(LogLevels level, String format, params Object[] args)
        {
            var e = WriteLogEventArgs.Current.Set(level);
            // 特殊处理异常对象
            if (args != null && args.Length == 1 && args[0] is Exception && (String.IsNullOrEmpty(format) || format == "{0}"))
                PerformWriteLog(e.Set(null, args[0] as Exception, true));
            else
                PerformWriteLog(e.Set(Format(format, args), null, true));
        }

        /// <summary>输出日志</summary>
        /// <param name="msg">信息</param>
        public void Write(String msg)
        {
            PerformWriteLog(WriteLogEventArgs.Current.Set(msg, null, false));
        }

        /// <summary>写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Write(String format, params Object[] args)
        {
            Write(Format(format, args));
        }

        /// <summary>输出日志</summary>
        /// <param name="msg">信息</param>
        public void WriteLine(String msg)
        {
            // 小对象，采用对象池的成本太高了
            PerformWriteLog(WriteLogEventArgs.Current.Set(msg, null, true));
        }

        /// <summary>写日志</summary>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLine(LogLevels level, String format, params Object[] args)
        {
            WriteLine(Format(format, args));
        }

        /// <summary>输出异常日志</summary>
        /// <param name="ex">异常信息</param>
        public void WriteException(Exception ex)
        {
            PerformWriteLog(WriteLogEventArgs.Current.Set(null, ex, false));
        }
        #endregion

        #region 辅助
        /// <summary>已重载。</summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(LogFile))
                return String.Format("{0} {1}", this.GetType().Name, LogFile);
            else
                return String.Format("{0} {1}", this.GetType().Name, LogPath);
        }
        #endregion
    }

    /// <summary>文本控件输出日志</summary>
    public class TextControlLog : Logger
    {
        private Control _Control;
        /// <summary>文本控件</summary>
        public Control Control { get { return _Control; } set { _Control = value; } }

        private Int32 _MaxLines = 1000;
        /// <summary>最大行数，超过该行数讲清空文本控件。默认1000行</summary>
        public Int32 MaxLines { get { return _MaxLines; } set { _MaxLines = value; } }

        /// <summary>写日志</summary>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        protected override void OnWrite(LogLevels level, String format, params Object[] args)
        {
            WriteLog(Control, Format(format, args) + Environment.NewLine, MaxLines);
        }

        /// <summary>在WinForm控件上输出日志，主要考虑非UI线程操作</summary>
        /// <remarks>不是常用功能，为了避免干扰常用功能，保持UseWinForm开头</remarks>
        /// <param name="control">要绑定日志输出的WinForm控件</param>
        /// <param name="msg">日志</param>
        /// <param name="maxLines">最大行数</param>
        public static void WriteLog(Control control, String msg, Int32 maxLines = 1000)
        {
            if (control == null) return;

            var txt = control as TextBoxBase;
            if (txt == null) throw new Exception(string.Format("不支持的控件类型{0}！", control.GetType()));

            txt.Append(msg, maxLines);
        }
    }

    /// <summary>控制台输出日志</summary>
    public class ConsoleLog : Logger
    {
        private Boolean _UseColor = true;
        /// <summary>是否使用多种颜色，默认使用</summary>
        public Boolean UseColor { get { return _UseColor; } set { _UseColor = value; } }

        /// <summary>写日志</summary>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        protected override void OnWrite(LogLevels level, String format, params Object[] args)
        {
            var e = WriteLogEventArgs.Current.Set(level).Set(Format(format, args), null, true);

            if (!UseColor)
            {
                ConsoleWriteLog(e);
                return;
            }

            var cc = Console.ForegroundColor;
            switch (level)
            {
                case LogLevels.Warn:
                    cc = ConsoleColor.Yellow;
                    break;
                case LogLevels.Error:
                case LogLevels.Fatal:
                    cc = ConsoleColor.Red;
                    break;
                default:
                    cc = GetColor(e.ThreadID);
                    break;
            }

            var old = Console.ForegroundColor;
            Console.ForegroundColor = cc;
            ConsoleWriteLog(e);
            Console.ForegroundColor = old;
        }

        private Boolean LastIsNewLine = true;
        private void ConsoleWriteLog(WriteLogEventArgs e)
        {
            if (LastIsNewLine)
            {
                var msg = e.ToString();
                // 如果上一次是换行，则这次需要输出行头信息
                if (e.IsNewLine)
                    Console.WriteLine(msg);
                else
                {
                    Console.Write(msg);
                    LastIsNewLine = false;
                }
            }
            else
            {
                // 如果上一次不是换行，则这次不需要行头信息
                var msg = e.Message + e.Exception;
                if (e.IsNewLine)
                {
                    Console.WriteLine(msg);
                    LastIsNewLine = true;
                }
                else
                    Console.Write(msg);
            }
        }

        static Dictionary<Int32, ConsoleColor> dic = new Dictionary<Int32, ConsoleColor>();
        static ConsoleColor[] colors = new ConsoleColor[] { ConsoleColor.Gray, ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.White, ConsoleColor.DarkGray, ConsoleColor.DarkYellow, ConsoleColor.DarkMagenta, ConsoleColor.DarkRed, ConsoleColor.DarkCyan, ConsoleColor.DarkGreen };
        private ConsoleColor GetColor(Int32 threadid)
        {
            // 好像因为dic.TryGetValue也会引发线程冲突，真是悲剧！
            lock (dic)
            {
                ConsoleColor cc;
                var key = threadid;
                if (!dic.TryGetValue(key, out cc))
                {
                    //lock (dic)
                    {
                        //if (!dic.TryGetValue(key, out cc))
                        {
                            cc = colors[dic.Count % colors.Length];
                            dic[key] = cc;
                        }
                    }
                }

                return cc;
            }
        }

        /// <summary>已重载。</summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0} UseColor={1}", this.GetType().Name, UseColor);
        }
    }

    /// <summary>复合日志提供者，多种方式输出</summary>
    public class CompositeLog : Logger
    {
        private List<ILog> _Logs = new List<ILog>();
        /// <summary>日志提供者集合</summary>
        public List<ILog> Logs { get { return _Logs; } set { _Logs = value; } }

        /// <summary>实例化</summary>
        public CompositeLog() { }

        /// <summary>实例化</summary>
        /// <param name="log"></param>
        public CompositeLog(ILog log) { Logs.Add(log); Level = log.Level; }

        /// <summary>实例化</summary>
        /// <param name="log1"></param>
        /// <param name="log2"></param>
        public CompositeLog(ILog log1, ILog log2)
        {
            Add(log1).Add(log2);
            Level = log1.Level;
            if (Level > log2.Level) Level = log2.Level;
        }

        /// <summary>添加一个日志提供者</summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public CompositeLog Add(ILog log) { Logs.Add(log); return this; }

        /// <summary>删除日志提供者</summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public CompositeLog Remove(ILog log) { if (Logs.Contains(log)) Logs.Remove(log); return this; }

        /// <summary>写日志</summary>
        /// <param name="level"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        protected override void OnWrite(LogLevels level, String format, params Object[] args)
        {
            if (Logs != null)
            {
                foreach (var item in Logs)
                {
                    item.Write(level, format, args);
                }
            }
        }

        /// <summary>从复合日志提供者中提取指定类型的日志提供者</summary>
        /// <typeparam name="TLog"></typeparam>
        /// <returns></returns>
        public TLog Get<TLog>() where TLog : class
        {
            foreach (var item in Logs)
            {
                if (item != null)
                {
                    if (item is TLog) return item as TLog;

                    // 递归获取内层日志
                    var cmp = item as CompositeLog;
                    if (cmp != null)
                    {
                        var log = cmp.Get<TLog>();
                        if (log != null) return log;
                    }
                }
            }

            return null;
        }

        //public ILog Get(Type type)
        //{
        //    foreach (var item in Logs)
        //    {
        //        if (item != null && type.IsAssignableFrom(item.GetType())) return item;
        //    }

        //    return null;
        //}

        /// <summary>已重载。</summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().Name);

            foreach (var item in Logs)
            {
                sb.Append(" ");
                sb.Append(item + "");
            }

            return sb.ToString();
        }
    }

    /// <summary>写日志事件参数</summary>
    public class WriteLogEventArgs : EventArgs
    {
        #region 属性
        private LogLevels _Level;
        /// <summary>日志等级</summary>
        public LogLevels Level { get { return _Level; } set { _Level = value; } }

        private String _Message;
        /// <summary>日志信息</summary>
        public String Message { get { return _Message; } set { _Message = value; } }

        private Exception _Exception;
        /// <summary>异常</summary>
        public Exception Exception { get { return _Exception; } set { _Exception = value; } }

        private Boolean _IsNewLine = true;
        /// <summary>是否换行</summary>
        public Boolean IsNewLine { get { return _IsNewLine; } set { _IsNewLine = value; } }
        #endregion

        #region 扩展属性
        private DateTime _Time;
        /// <summary>时间</summary>
        public DateTime Time { get { return _Time; } set { _Time = value; } }

        private Int32 _ThreadID;
        /// <summary>线程编号</summary>
        public Int32 ThreadID { get { return _ThreadID; } set { _ThreadID = value; } }

        private Boolean _IsPoolThread;
        /// <summary>是否线程池线程</summary>
        public Boolean IsPoolThread { get { return _IsPoolThread; } set { _IsPoolThread = value; } }

        private Boolean _IsWeb;
        /// <summary>是否Web线程</summary>
        public Boolean IsWeb { get { return _IsWeb; } set { _IsWeb = value; } }

        private String _ThreadName;
        /// <summary>线程名</summary>
        public String ThreadName { get { return _ThreadName; } set { _ThreadName = value; } }
        #endregion

        #region 构造
        /// <summary>实例化一个日志事件参数</summary>
        internal WriteLogEventArgs() { }

        /// <summary>构造函数</summary>
        /// <param name="message">日志</param>
        public WriteLogEventArgs(String message) : this(message, null, true) { }

        /// <summary>构造函数</summary>
        /// <param name="message">日志</param>
        /// <param name="exception">异常</param>
        public WriteLogEventArgs(String message, Exception exception) : this(message, null, true) { }

        /// <summary>构造函数</summary>
        /// <param name="message">日志</param>
        /// <param name="exception">异常</param>
        /// <param name="isNewLine">是否换行</param>
        public WriteLogEventArgs(String message, Exception exception, Boolean isNewLine)
        {
            Message = message;
            Exception = exception;
            IsNewLine = isNewLine;

            Init();
        }
        #endregion

        #region 线程专有实例
        /*2015-06-01 @宁波-小董
         * 将Current以及Set方法组从internal修改为Public
         * 原因是 Logger在进行扩展时，重载OnWrite需要用到该静态属性以及方法，internal无法满足扩展要求
         * */
        [ThreadStatic]
        private static WriteLogEventArgs _Current;
        /// <summary>线程专有实例。线程静态，每个线程只用一个，避免GC浪费</summary>
        public static WriteLogEventArgs Current { get { return _Current ?? (_Current = new WriteLogEventArgs()); } }
        #endregion

        #region 方法
        /// <summary>初始化为新日志</summary>
        /// <param name="level">日志等级</param>
        /// <returns>返回自身，链式写法</returns>
        public WriteLogEventArgs Set(LogLevels level)
        {
            Level = level;

            return this;
        }

        /// <summary>初始化为新日志</summary>
        /// <param name="message">日志</param>
        /// <param name="exception">异常</param>
        /// <param name="isNewLine">是否换行</param>
        /// <returns>返回自身，链式写法</returns>
        public WriteLogEventArgs Set(String message, Exception exception, Boolean isNewLine)
        {
            Message = message;
            Exception = exception;
            IsNewLine = isNewLine;

            Init();

            return this;
        }

        /// <summary>清空日志特别是异常对象，避免因线程静态而导致内存泄漏</summary>
        public void Clear()
        {
            Message = null;
            Exception = null;
        }

        void Init()
        {
            Time = DateTime.Now;
            var thread = Thread.CurrentThread;
            ThreadID = thread.ManagedThreadId;
            IsPoolThread = thread.IsThreadPoolThread;
            ThreadName = thread.Name;
#if !Android
          //  IsWeb = HttpContext.Current != null;
#endif
        }

        //private static DateTime _Last;
        ///// <summary>已重载。</summary>
        ///// <returns></returns>
        //public string ToShortString()
        //{
        //    if (Exception != null) Message += Exception.ToString();

        //    var sb = new StringBuilder();

        //    // 屏蔽小时和分钟部分，仅改变时显示一次
        //    var now = DateTime.Now;
        //    if (now.Hour == _Last.Hour && now.Minute == _Last.Minute)
        //        sb.AppendFormat("{0:ss.fff} {1,2}", Time, ThreadID);
        //    else
        //    {
        //        _Last = now;
        //        sb.AppendFormat("{0:HH:mm:ss.fff} {1,2}", Time, ThreadID);
        //    }

        //    if (!Runtime.IsConsole)
        //        sb.AppendFormat(" {0}", IsPoolThread ? (IsWeb ? 'W' : 'Y') : 'N');

        //    if (!ThreadName.IsNullOrEmpty())
        //        sb.AppendFormat(" {0}", ThreadName);
        //    sb.AppendFormat(" {0}", Message);

        //    return sb.ToString();
        //}

        /// <summary>已重载。</summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Exception != null) Message += Exception.ToString();

            var name = ThreadName;
            if (name.IsNullOrEmpty()) name = "-";
#if Android
            if (name.EqualIgnoreCase("Threadpool worker")) name = "P";
            if (name.EqualIgnoreCase("IO Threadpool worker")) name = "IO";
#endif

            return String.Format("{0:HH:mm:ss.fff} {1,2} {2} {3} {4}", Time, ThreadID, IsPoolThread ? (IsWeb ? 'W' : 'Y') : 'N', name, Message);
        }
        #endregion
    }
}