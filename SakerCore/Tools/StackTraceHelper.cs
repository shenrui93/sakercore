/***************************************************************************
 * 
 * 创建时间：   2016/9/24 13:26:12
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供运行堆栈的获取操作能力
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SakerCore.Tools
{
    /// <summary>
    /// 提供运行堆栈的获取操作能力
    /// </summary>
    public static class StackTraceHelper
    {
        /// <summary>
        /// 获取当前执行的堆栈信息
        /// </summary>
        /// <param name="skipcount"></param>
        /// <returns></returns>
        public static string GetStackTraceInfo(int skipcount)
        {
            skipcount = skipcount + 1;
            skipcount = skipcount <= 0 ? 1 : skipcount;
            int count = 0;

            var sb = new System.Text.StringBuilder();

            var stackinfo = new System.Diagnostics.StackTrace(true).GetFrames();
            foreach (var st in stackinfo)
            {
                count++;
                if (count <= skipcount)
                {
                    continue;
                }
                var filename = st.GetFileName();
                var fileLineNumber = st.GetFileLineNumber();
                if (filename != null)
                    sb.AppendLine($@"在 {st.GetMethod()} 位置：{filename} 行号：{fileLineNumber}");
                else
                {
                    sb.AppendLine($@"在 {st.GetMethod()}");
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 获取当前方法的调用堆栈信息
        /// </summary>
        /// <returns></returns>
        public static string GetStackTraceInfo()
        {
            return GetStackTraceInfo(1);
        }
    }
}
