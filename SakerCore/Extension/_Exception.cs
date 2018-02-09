/***************************************************************************
 * 
 * 创建时间：   2017/9/12 17:52:34
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供Exception类型的方法扩展
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.Extension
{
    /// <summary>
    /// 提供Exception类型的方法扩展
    /// </summary>
    public static  class _Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetExceptionFormatString(this System.Exception ex)
        {


            if (ex == null)
                throw new NullReferenceException();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("*********************异常文本*********************");
            sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());

            sb.AppendLine("【异常类型】：" + ex.GetType().Name);
            sb.AppendLine("【异常信息】：" + ex.Message);
            sb.AppendLine("【堆栈调用】：" + ex.StackTrace);

            sb.AppendLine("******************************************************************");

            if (ex.InnerException != null)
            {
                sb.AppendLine(GetExceptionFormatString(ex.InnerException));
            }

            return sb.ToString();
        }
    }
}
