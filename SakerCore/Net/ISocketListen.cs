using System;
namespace SakerCore.Net
{
    /// <summary>
    /// 服务器监听器接口
    /// </summary>
    public interface IListen : IDisposable
    {
        /// <summary>
        /// 新的连接请求到达事件
        /// </summary>
          EventHandler<AcceptedEventArgs> Accepted { get; set; }

        /// <summary>
        /// 表示服务器是否处于关闭或停止状态
        /// </summary>
        bool IsStop { get; }

        /// <summary>
        /// 启动监听器，接受连接请求
        /// </summary>
        /// <returns></returns>
        bool StartAccept();

        /// <summary>
        /// 关闭监听器，拒绝连接请求
        /// </summary>
        void StopAccept();
        /// <summary>
        /// 
        /// </summary>
        bool IsIPv6Model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int ListenPort { get; }
    }
}
