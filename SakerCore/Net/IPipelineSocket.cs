using System;
namespace SakerCore.Net
{
    /// <summary>
    /// 定义一个接口，接口包含管道数据交换对象的基本通信方法
    /// </summary>
    public interface IPipelineSocket : IDisposable
    {
        /// <summary>
        /// 关闭管道通信连接
        /// </summary>
        void Close(bool reuseSocket);
        /// <summary>
        /// 获取基础连接套接字对象
        /// </summary>
        System.Net.Sockets.Socket BaseSocket { get; }

        #region 事件接口

        /// <summary>
        /// 另一端的数据接收事件
        /// </summary>
        EventHandler<BufferReceiveEventArgs> ReceiveCompleted { get; set; }
        /// <summary>
        /// 发送完毕的触发事件
        /// </summary>
        EventHandler<EventArgs> SendCompleted { get; set; }
        /// <summary>
        /// 对象被释放后的通知事件
        /// </summary>
        EventHandler Disposed { get; set; }

        #endregion

        /// <summary>
        /// 向管道另一端发送字节数据
        /// </summary>
        void Send(byte[] buffer);
        /// <summary>
        /// 连接到指定的远端终结点
        /// </summary>
        /// <param name="ip">要连接到的远端ip地址</param>
        /// <param name="port">要连接到的远端终结点的端口号</param>
        /// <returns>返回一个bool值，表示是否连接成功</returns>
        bool Connect(string ip, int port);
        /// <summary>
        /// 数据缓冲区大小
        /// </summary>
        int BufferSize
        {
            get;
            //set;
        }
        /// <summary>
        /// 连接成功状态
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// 开始接收数据
        /// </summary>
        /// <returns></returns>
        bool StartReceive();
        /// <summary>
        /// 进行异步连接
        /// </summary>
        /// <param name="ip">远程IP</param>
        /// <param name="port">远端端口</param>
        /// <param name="iaCallBack">执行完毕后回调</param>
        void BeginConnect(string ip, int port, Action<bool> iaCallBack);
        /// <summary>
        /// 连接的远端IP
        /// </summary>
        string IPAdress { get; }
        /// <summary>
        /// 通讯绑定的端口信息
        /// </summary>
        int Port { get; }
        /// <summary>
        /// 返回IP的整型数值的表示形式
        /// </summary>
        uint IPAdressUInt32 { get; }
        /// <summary>
        /// 与客户端通讯回话
        /// </summary>
        int SessionID { get; }
        /// <summary>
        /// 设置远端主机的IP地址
        /// 这个方法不会实际的更改的连接，只是修改了IP属性显示的IP字符串，一般不需要调用这个方法
        /// </summary>
        /// <param name="ip">设置的IP</param>
        void SetRemoteIP(string ip);
        /// <summary>
        /// 表示当前记录的对方IP信息是否已经被修改
        /// </summary>
        bool IsSetRemoteIP
        {
            get;
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        void Send(byte[] buffer, int offset, int count);
    }

}
