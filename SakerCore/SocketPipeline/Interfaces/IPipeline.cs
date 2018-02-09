using System;
using Uyi.Net.Message;
namespace Uyi.Net
{
    /// <summary>
    /// 为服务器管道对象定义处理接口
    /// </summary>
    public interface IPipeline : IDisposable, IPipelineDispose
    {
        /// <summary>
        /// 有新消息的通知事件
        /// </summary>
          PipelineEventHandle<MessageComingArgs> MessageComing { get; set; } 

        /// <summary>
        /// 数据发送触发事件
        /// </summary>
        PipelineEventHandle<EventArgs> PipelineSendCompleted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Action<byte[]> BeforeSendDataEventHandle { get; set; }

        /// <summary>
        /// 客户端通信管道
        /// </summary>
        IPipelineSocket Client { get; set; }

        /// <summary>
        /// 管道唯一标识符
        /// </summary>
        int SessionID
        {
            get; 
        }

        /// <summary>
        /// 关闭管道
        /// </summary>
        void Close();

        /// <summary>
        /// 发送数据对象给远端数据
        /// </summary>
        /// <param name="msg">发送消息</param>
        void SendData(object msg);

        /// <summary>
        /// 发送数据对象给远端数据
        /// </summary>
        /// <param name="data"></param>
        void SendData(byte[] data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userObject"></param>
        void Dispose(object userObject);

        ///// <summary>
        ///// 注册消息处理器，以实现消息的处理链条化
        ///// </summary>
        ///// <typeparam name="TMessage">消息类型</typeparam>
        ///// <param name="m_handle">消息委托</param>
        ///// <returns></returns>
        //void RegistereMessageInHandle<TMessage>(System.Action<IPipeline, TMessage> m_handle) where TMessage : object;

        /// <summary>
        /// 对象携带的用户对象
        /// </summary>
        object Tag { get; set; }

        /// <summary>
        /// 发送一个Ping心跳包指令
        /// </summary>
        void Ping();

    }

    /// <summary>
    /// 为服务器游戏双通管道定义处理接口
    /// </summary>
    public interface IGamePipeline : IPipeline, IDisposable, IPipelineDispose
    {
         
         

        /// <summary>
        /// 游戏服务器数据通讯交换管道
        /// </summary>
        IPipelineSocket GameServer { get; set; }

        ///// <summary>
        ///// 创建游戏服务器连接
        ///// </summary>
        //bool ConnectGameServer(string ip, int port, int bufferSize);

        ///// <summary>
        ///// 创建游戏服务器连接
        ///// </summary>
        //bool ConnectGameServer(uint ip, int port, int bufferSize);

        /// <summary>
        /// 开始一个异步操作来创建游戏服务器连接
        /// </summary>
        IAsyncResult BeginConnectGameServer(string ip, int port, AsyncCallback callback, object userState);


        /// <summary>
        /// 开始一个异步操作来创建游戏服务器连接
        /// </summary>
        IAsyncResult BeginConnectGameServer(uint ip, int port, AsyncCallback callback, object userState);


        /// <summary>
        /// 结束游戏服务器连接操作
        /// </summary>
        /// <param name="iar"></param>
        /// <returns></returns>
        bool EndConnectGameServer(IAsyncResult iar);


        /// <summary>
        /// 关闭游戏服务器连接
        /// </summary>
        void DisConnectGameServer();

        /// <summary>
        /// 开始一个异步操作来创建游戏服务器连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="callback"></param>
        void BeginConnectGameServer(string ip, int port, Action<bool> callback);



    }
}
