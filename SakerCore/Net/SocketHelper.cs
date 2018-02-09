/***************************************************************************
 * 
 * 创建时间：   2016/11/17 13:45:43
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace SakerCore.Net
{
    /// <summary>
    /// 
    /// </summary>
    public static class SocketHelper
    {
        /// <summary>
        /// 检查指定的端口是否已经被占用
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool PortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            } 
            return inUse;
        }

        /// <summary>
        /// 检查指定的端口是否已经被占用
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Process PortInUseProcess(int port)
        {
            try
            {

                Process pro = new Process();
                // 设置命令行、参数
                pro.StartInfo.FileName = "cmd.exe";
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.RedirectStandardInput = true;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.RedirectStandardError = true;
                pro.StartInfo.CreateNoWindow = true;
                // 启动CMD
                pro.Start();
                // 运行端口检查命令
                pro.StandardInput.WriteLine("netstat -ano");
                pro.StandardInput.WriteLine("exit");
                // 获取结果
                Regex reg = new Regex("\\s+", RegexOptions.Compiled);
                string line = null;

                var checkstr = ":" + port;

                while ((line = pro.StandardOutput.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
                    {
                        line = reg.Replace(line, ",");
                        string[] arr = line.Split(',');
                        if (arr[1].EndsWith(checkstr))
                        {
                            int pid = int.Parse(arr[4]);
                            try
                            {
                                return Process.GetProcessById(pid);
                            }
                            catch (System.Exception)
                            {
                                return null;
                            } 
                        }
                    }
                }
            }
            catch (System.Exception)
            {
            }
                return null;
        }
    }
}
