using System;
namespace Uyi.Net
{
    /// <summary>
    /// 向客户端通知一个对象已经被释放
    /// </summary>
    public interface IObjectDispose
    {
        /// <summary>
        /// 对象被释放后的通知事件
        /// </summary>
        EventHandler Disposed { get; set; }
    }

    /// <summary>
    /// 向客户端通知一个管道已经被释放
    /// </summary>
    public interface IPipelineDispose
    {
        /// <summary>
        /// 对象被释放后的通知事件
        /// </summary>
        PipelineEventHandle<PipelineDisposedEventArgs> PipelineDisposed { get; set; }
    }


}
