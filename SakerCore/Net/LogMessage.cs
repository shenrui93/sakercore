using System;

namespace SakerCore.Net
{
    /// <summary>
    /// 通讯组件通讯异常的提供程序
    /// </summary>
    internal static class SystemRunErrorPorvider
    { 

        internal static void CatchException(Exception ex)
        {
            SakerCore.SystemErrorProvide.OnSystemErrorHandleEvent(null, ex);
        }
    }
}
