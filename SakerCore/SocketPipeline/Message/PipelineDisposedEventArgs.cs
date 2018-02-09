using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uyi.Net
{
    /// <summary>
    /// 管道释放事件通知事件
    /// </summary>
    public class PipelineDisposedEventArgs : EventArgs
    {
        /// <summary>
        /// 表示一个空的释放参数
        /// </summary>
        public readonly new static PipelineDisposedEventArgs Empty = new PipelineDisposedEventArgs();

        /// <summary>
        /// 用户主动调用释放时所传递的用户对象
        /// </summary>
        public object UserObject { get; set; }
    }
}
