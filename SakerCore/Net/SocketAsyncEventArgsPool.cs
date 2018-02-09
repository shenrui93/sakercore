/***************************************************************************
 * 
 * 创建时间：   2017/7/12 17:45:44
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace SakerCore.Net
{
    /// <summary>
    /// 
    /// </summary>
    public class SocketAsyncEventArgsPool
    {
        internal static SocketAsyncEventArgsMetadata GetNewAsyncEventArgs()
        {
            return new SocketAsyncEventArgsMetadata();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SocketAsyncEventArgsMetadata : SocketAsyncEventArgs, IDisposable
    {
        /// <summary>
        /// 用于完成异步操作的事件
        /// </summary>
        public new event EventHandler<SocketAsyncEventArgsMetadata> Completed;
        private int _isDisposed = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            if (Completed != null)
            {
                Completed(this, this);
            }
        }

        /// <summary>
        ///  释放由 System.Net.Sockets.SocketAsyncEventArgsMetadata 实例使用的非托管资源，并可选择释放托管资源。
        /// </summary> 
        public new void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;

            this.Completed = null;
            base.Dispose();
        }

    }
}
