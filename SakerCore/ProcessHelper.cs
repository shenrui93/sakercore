/***************************************************************************
 * 
 * 创建时间：   2017/2/13 14:34:14
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供进程管理处理逻辑
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SakerCore.IO;
using SakerCore.Extension;


namespace SakerCore
{
    /// <summary>
    /// 提供进程管理处理逻辑
    /// </summary>
    public static class ProcessHelper
    {
        static System.IO.Stream fs;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static int Cmd(string cmdStr,out string output)
        { 

            System.Diagnostics.Process p = new System.Diagnostics.Process();


            try
            {
                p.StartInfo.FileName = "cmd.exe ";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序 
                          //向cmd窗口发送输入信息
                p.StandardInput.WriteLine(cmdStr+"&exit");

                p.StandardInput.AutoFlush = true;
                //p.StandardInput.WriteLine("exit");
                //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
                //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令



                //获取cmd窗口的输出信息

                //StreamReader reader = p.StandardOutput;
                //string line = reader.ReadLine();
                //while (!reader.EndOfStream)
                //{s
                //    str += line + "  ";
                //    line = reader.ReadLine();
                //}

                //获取cmd窗口的输出信息
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(3 * 1000); //等待程序执行完退出进程 


                return 1;
            }
            finally
            {
                try
                {
                    p.Kill();
                }
                catch
                {
                }
            }


        }
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        public static int CmdAsync(string cmdStr,int waitTime = 5 * 1000)
        { 

            System.Diagnostics.Process p = new System.Diagnostics.Process();


            try
            {
                p.StartInfo.FileName = "cmd.exe ";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序 
                          //向cmd窗口发送输入信息
                p.StandardInput.WriteLine(cmdStr+"&exit");

                p.StandardInput.AutoFlush = true;

                //获取cmd窗口的输出信息
                p.WaitForExit(waitTime); //等待程序执行完退出进程 


                return 1;
            }
            finally
            {
                try
                {
                    p.Kill();
                }
                catch
                {
                }
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        public static void SaveProcessId(string n)
        {
            var process = System.Diagnostics.Process.GetCurrentProcess(); 
            var path = $@"{FileHelper.ProcessBaseDir}\handle\{n}.pid";
            FileHelper.CheckAndCreateDir(path);
            fs = FileHelper.OpenWriterOrCreate(path);

            fs.SetLength(0);
            fs.WriteBytes(process.Id.ToString().GetBytes());
            fs.Flush();
        } 
    }
}
