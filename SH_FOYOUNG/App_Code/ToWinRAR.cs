using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

/// <summary>
/// ToWinRAR 的摘要说明
/// </summary>
public class ToWinRAR:BasePage
{
	public ToWinRAR()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
	}

    /// <summary>
    /// 生成RAR
    /// </summary>
    /// <param name="path">目标文件夹路径 如/data/Base</param>
    /// <param name="rarName">生成压缩文件的文件名 如dnd.rar</param>
    /// <param name="rarType">文件类型 如费用一览等</param>
    public string CompressRar(string path, string rarName,string rarType)
    {
        try
        {
            string rarPath = "/WinRAR/" + rarType + "/" + GetUserInfo.Code + "/" + DateTime.Now.ToString("yyyyMMdd");
            path = Server.MapPath(path);
            string url = rarPath;
            rarPath =Server.MapPath(rarPath);

            DirectoryInfo dirpath = new DirectoryInfo(path);
            if (!dirpath.Exists)
            {
                return "目标文件不存在！";
            }
            DirectoryInfo dirInfo = new DirectoryInfo(rarPath);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            FileInfo fileinfo=new FileInfo(rarPath+"\\"+rarName);
            if (fileinfo.Exists)
            {
                fileinfo.Delete();
            }
            
            String winRarPath = null;
            if (!ExistsRar(out winRarPath)) return "未找到WinRar压缩程序";
            //验证WinRar是否安装。
            var pathInfo = String.Format("a -ep1 \"{0}\" \"{1}\"", rarName, path);
            #region WinRar 用到的命令注释
            //[a] 添加到压缩文件
            //afzip 执行zip压缩方式，方便用户在不同环境下使用。
            //（取消该参数则执行rar压缩）
            //-m0 存储 添加到压缩文件时不压缩文件。共6个级别【0-5】，值越大效果越好，也越慢
            //ep1 依名称排除主目录（生成的压缩文件不会出现不必要的层级）
            //r 修复压缩档案
            //t 测试压缩档案内的文件
            //as 同步压缩档案内容 
            //-p 给压缩文件加密码方式为：-p123456
            #endregion
            //打包文件存放目录
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = winRarPath,//执行的文件名
                    Arguments = pathInfo,//需要执行的命令
                    UseShellExecute = false,//使用Shell执行
                    WindowStyle = ProcessWindowStyle.Hidden,//隐藏窗体
                    WorkingDirectory = rarPath,//rar 存放位置
                    CreateNoWindow = false,//不显示窗体
                },
            };
            process.Start();//开始执行
            //process.WaitForExit();//等待完成并退出
            process.Dispose();
            process.Close();//关闭调用 cmd 
            return "保存成功";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }


    /// <summary>
    /// 验证WinRar是否安装。
    /// </summary>
    /// <returns>true：已安装，false：未安装</returns>
    private bool ExistsRar(out String winRarPath)
    {
        winRarPath = String.Empty;
        //通过Regedit（注册表）找到WinRar文件
        var registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinRAR.exe");
        if (registryKey == null) return false;//未安装
        //registryKey = theReg;可以直接返回Registry对象供会面操作
        winRarPath = registryKey.GetValue("").ToString();
        //这里为节约资源，直接返回路径，反正下面也没用到
        registryKey.Close();//关闭注册表
        return !String.IsNullOrEmpty(winRarPath);
    }
}