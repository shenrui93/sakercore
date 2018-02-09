using System;
using SakerCore.Tools;

namespace SakerCore.Tools
{
    /// <summary>
    /// 提供消息队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageQueue<T> : IDisposable
       //where T : class
    {
        /// <summary>
        /// 清空消息队列
        /// </summary>
        void Clear();
        /// <summary>
        /// 将消息排入消息队列集合内
        /// </summary>
        /// <param name="item"></param>
        void Enqueue(T item);
        /// <summary>
        /// 消息队列内的消息到达通知方法
        /// </summary>
        EventHandle<IMessageQueue<T>, MessageEventArgs<T>> MessageComing { get; set; }
        /// <summary>
        /// 
        /// </summary>
        bool SetFailRequestCurrentMesage();
        /// <summary>
        /// 重启消息对列
        /// </summary>
        void Restart();
        /// <summary>
        /// 
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// 
        /// </summary>
        int Count { get; }
        /// <summary>
        /// 消息队列内的消息到达通知方法
        /// </summary>
        EventHandle<IMessageQueue<T>, LongProcessMessage<T>> LongProcessMessage { get; set; }
    }
    /// <summary>
    /// 为消息对列提供对列优先级属性的基础接口
    /// </summary>
    public interface IMessagePriority
    {
        /// <summary>
        /// 消息的优先级
        /// </summary>
        MessagePriority MPriority { get; }
    }

    /// <summary>
    /// 指示消息的优先级
    /// </summary>
    public enum MessagePriority
    {
        /// <summary>
        /// 消息属于低优先级
        /// </summary>
        Low,
        /// <summary>
        /// 消息属于高优先级
        /// </summary>
        Hign
    }
}
