using System;

namespace SakerCore.WebSocketServer
{
    /// <summary>
    /// 定义一个接口，其表示一个 WebSocket 在会话期间的操作信息对象
    /// </summary>
    public interface IWebSocketSession
    {

        /// <summary>
        /// 获取当前客户端连接远端IP地址
        /// </summary>
        string ClientAddress { get; }
        /// <summary>
        /// 表示当前会话消息到达的事件通知方法
        /// </summary>
        EventHandler<WebSocketSessionMessageComingArg> MessageComing { get; set; }
        /// <summary>
        /// 表示当前连接关闭的通知事件方法
        /// </summary>
        EventHandler WebSocketSessionClose { get; set; }
        /// <summary>
        /// 释放对象
        /// </summary>
        void Dispose();
        /// <summary>
        /// 发送文本数据
        /// </summary>
        /// <param name="message">需要发送的文本数据</param>
        void Send(string message);
        /// <summary>
        /// 发送二进制数据
        /// </summary>
        /// <param name="data">表示需要发送的二进制数据</param>
        void Send(byte[] data);
        /// <summary>
        /// 连接的会话Id
        /// </summary>
        int SessionId { get; }

        /// <summary>
        /// 向对端发送一条Ping指令
        /// </summary>
        void Ping();
        /// <summary>
        /// 获取当前连接的基础套接字对象
        /// </summary>
        SakerCore.Net.IPipelineSocket BasePipelineSocket { get; }
        /// <summary>
        /// 向对端发送数据
        /// </summary>
        /// <param name="buffer">数据缓冲区</param>
        /// <param name="offset">数据起始偏移量</param>
        /// <param name="count">数据数量</param>
        void Send(byte[] buffer, int offset, int count);
    }
}