/***************************************************************************
 * 
 * 创建时间：   2016/4/11 13:08:48
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   表示一个 WebSocket 的通讯会话信息
 * 
 * *************************************************************************/

using System;
using SakerCore.Net;
using System.Threading;

namespace SakerCore.WebSocketServer
{
    /// <summary>
    /// 表示一个 WebSocket 的通讯会话信息
    /// </summary>
    public sealed class WebSocketSession : IDisposable, IWebSocketSession
    {
        // 表达当前连接的基础连接信息
        internal IPipelineSocket AcceptSocket;
        //表示当前WebSocket请求数据的解析器
        internal WebSocket_Handle WebsocketHandle;
        //指示当前的实例是否已经被释放
        long _isDisposed;
        //表示当前连接所属的连接服务监听器
        private readonly WebSocketServer _webSocketServer; 

        /// <summary>
        ///  初始化一个新的 <see cref="WebSocketSession"/> 实例
        /// </summary>
        /// <param name="acceptSocket">监听器完成连接的基础通讯连接</param>
        /// <param name="webSocketServer">表示当前连接所属的连接服务监听器</param>
        public WebSocketSession(IPipelineSocket acceptSocket, WebSocketServer webSocketServer)
        {
            //初始化当前连接的监听器
            _webSocketServer = webSocketServer;

            //实例化为一个基础的消息解析函数器
            WebsocketHandle = new WebSocket_Handle(this);
            AcceptSocket = acceptSocket;
            //通知事件的绑定
            AcceptSocket.ReceiveCompleted = AcceptSocket_ReceiveCompleted;
            AcceptSocket.Disposed = AcceptSocket_Disposed;
            //启动数据接收
            AcceptSocket.StartReceive();
        }
        /// <summary>
        /// 表示当前当前会话被关闭的通知方法
        /// </summary> 
        private void AcceptSocket_Disposed(object sender, EventArgs e)
        {
            //连接断开，对象释放
            Dispose();
        }
        /// <summary>
        /// 当前会话收到新的数据消息的通知方法
        /// </summary>
        private void AcceptSocket_ReceiveCompleted(object sender, BufferReceiveEventArgs e)
        {
            //var data = new byte[e.Count];
            ////将数据Copy入缓冲区，启动数据接收完成的处理方法
            //Buffer.BlockCopy(e.Buffer, e.Offset, data, 0, e.Count);
            WebsocketHandle.ProcessReceive(e.Buffer, e.Offset, e.Count);
        }

        /// <summary>
        /// 设置消息处理函数管道
        /// </summary>
        /// <param name="handle"></param>
        internal void SetHandle(WebSocket_Handle handle)
        {
            if (handle == null) return;
            WebsocketHandle = handle;
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        internal void SendDataToClient(byte[] data)
        {
            AcceptSocket?.Send(data);
        }



        /// <summary>
        /// 连接的远端IP
        /// </summary>
        public string ClientAddress => AcceptSocket.IPAdress;

        /// <summary>
        /// 通讯操作的回话ID
        /// </summary>
        public int SessionId => AcceptSocket.SessionID;

        /// <summary>
        /// 获取当前通讯的支撑基础 <see cref="IPipelineSocket"/> 对象
        /// </summary>
        public IPipelineSocket BasePipelineSocket => AcceptSocket;

        /// <summary>
        /// 消息到来的时间处理方法
        /// </summary>
        public EventHandler<WebSocketSessionMessageComingArg> MessageComing { get; set; } = (sender, e) => { };
        /// <summary>
        /// 会话连接关系的处理方法
        /// </summary>
        public EventHandler WebSocketSessionClose { get; set; } = (sender, e) => { };

        /// <summary>
        /// 消息到来的时间处理方法
        /// </summary>
        internal void OnMessageComing(WebSocketSessionMessageComingArg arg)
        {
            MessageComing(this, arg);
        }
        /// <summary>
        /// 会话连接关系的处理方法
        /// </summary>
        internal void OnWebSocketSessionClose()
        {
            WebSocketSessionClose(this, EventArgs.Empty);
        }
        /// <summary>
        /// 通知一个连接建立完成
        /// </summary>
        /// <param name="callBack"></param>
        internal void OnConnect(System.Action callBack)
        {
            _webSocketServer.OnAccepted(this, callBack);
        }
        /// <summary>
        /// 发送文本数据
        /// </summary>
        /// <param name="message">需要发送的文本数据消息</param>
        public void Send(string message)
        {
            //发送数据给远端
            WebsocketHandle.SendData(WebSocketOpcode.Text, message.GetUtf8Bytes());
        }
        /// <summary>
        /// 发送二进制数据
        /// </summary>
        /// <param name="data">需要发送的二进制数据</param>
        public void Send(byte[] data)
        {
            //发送数据给二进制流
            Send(data, 0, data.Length);
        }
        /// <summary>
        /// 发送二进制数据
        /// </summary>
        /// <param name="buffer">需要发送数据缓冲区</param>
        /// <param name="offset">发送数据偏移量</param>
        /// <param name="count">发送数据的数量</param>
        public void Send(byte[] buffer, int offset, int count)
        {
            WebsocketHandle.SendData(WebSocketOpcode.Binary, buffer, offset, count);
        }

        /// <summary>
        /// 释放对象资源
        /// </summary>
        public void Dispose()
        {
            //检查冗余调用
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            //会话处理函数释放
            WebsocketHandle?.Dispose();
            //连接对象释放
            AcceptSocket?.Dispose();
            //通知当前的会话数据已经关闭
            OnWebSocketSessionClose();
        }


        /// <summary>
        /// 向对端发送一条Ping指令
        /// </summary>
        public void Ping()
        {
            this.WebsocketHandle.Ping();
        }

    }
     

    #region 辅助扩展类

    internal static class __
    {
        /// <summary>
        /// 获取指定字符串的Ascii编码字节数据
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回转换后的数据</returns>
        public static byte[] GetAsciiBytes(this string str) => string.IsNullOrEmpty(str) ? new byte[0] : System.Text.Encoding.ASCII.GetBytes(str);
        /// <summary>
        /// 获取指定字符串的Utf8编码字节数据
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回转换后的数据</returns>
        public static byte[] GetUtf8Bytes(this string str) => string.IsNullOrEmpty(str) ? new byte[0] : System.Text.Encoding.UTF8.GetBytes(str);
    }

    #endregion

}
