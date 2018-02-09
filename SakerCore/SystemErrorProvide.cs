/***************************************************************************
 * 
 * 创建时间：   2016/4/14 13:18:08
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   服务器异常信息的统一处理程序
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic; 
using System.IO; 
using System.Text; 

namespace SakerCore
{
    /// <summary>
    /// 服务器异常信息的统一处理程序
    /// </summary>
    public class SystemErrorProvide
    {

        /// <summary>
        /// 
        /// </summary>
        public static event EventHandle<Exception> UyiSystemErrorHandle;
        /// <summary>
        /// 
        /// </summary>
        public static event EventHandle<string> UyiSystemLogHandle;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public static void OnSystemErrorHandleEvent(object sender, System.Exception ex)
        {
            if (ex == null) return;
            if (UyiSystemErrorHandle == null) return;

            UyiSystemErrorHandle(sender, ex);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        [System.Diagnostics.Conditional("TRACE")]
        public static void OnSystemErrorHandleEventTrace(object sender, System.Exception ex)
        {
            if (ex == null) return;
            if (UyiSystemErrorHandle == null)
            {
                throw ex;
            }
            UyiSystemErrorHandle(sender, ex);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        [System.Diagnostics.Conditional("TRACE")]
        public static void OnSystemLogHandle(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            if (UyiSystemLogHandle == null) {
                return;
            }

            UyiSystemLogHandle(null, msg);
        }


    }

}
