

using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;
using SakerCore.Serialization;
using System.Runtime.InteropServices;
using System.Net.Security;

#if HPSocket
using HPSocketCS;
#endif

namespace SakerCore.Net.Pipeline
{

#if HPSocket


    /// <summary>
    /// 管道通信Socket封装
    /// </summary>
    public sealed class PipelineSocket : IPipelineSocket
    {
        private int bufferSize;
        private HPSocketCS.TcpClient socket;

        //发送数据缓冲区
        private IByteBuffer sendbuffer = new ByteBuffer();
        //接收数据缓冲区
        private IByteBuffer recbuffer = new ByteBuffer();

        //对象状态
        private int _isSend = 0;
        private int _isRecvice = 0;
        private int _isRecviceSatus = 0;
        private int _isDisposed;
        private string _remoteIP = "";
        private ushort _port;
        //线程同步根
        //private object root = new object();
        private AutoResetEvent connection_are = new AutoResetEvent(false);
     //   private int _connectionID = SocketIDProvider.NewID();

        private readonly int BufferSize = 1024;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufferSize"></param>
        public PipelineSocket(int bufferSize)
        {
            this.bufferSize = bufferSize;
            socket = new HPSocketCS.TcpClient();

            SetListener();
        }

        /// <summary>
        /// 设置监听器
        /// </summary>
        private void SetListener()
        {
            this.socket.OnConnect += OnConnect;
            this.socket.OnClose += OnClose;
            this.socket.OnError += OnError;
            this.socket.OnReceive += OnReceive;
            this.socket.OnSend += OnSend;
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {

            if (e.IsAvailable)
            {
                if (sendbuffer.Count <= 0)
                {
                    this.Send(new byte[] { 1 });
                }
            }
            else
            {
                //网络不可用，调用关闭方法
                this.Dispose();
            }
        }

    #region 监听方法


        private HandleResult OnConnect(HPSocketCS.TcpClient sender)
        {
            sender.GetListenAddress(ref _remoteIP, ref _port);
            return HandleResult.Ok;
        }
        private HandleResult OnClose(HPSocketCS.TcpClient sender)
        {
            this.Dispose();
            return HandleResult.Ok;
        }
        private HandleResult OnError(HPSocketCS.TcpClient sender, SocketOperation enOperation, int errorCode)
        {
            this.Dispose();
            return HandleResult.Ok;
        }
        private HandleResult OnReceive(HPSocketCS.TcpClient sender, IntPtr pData, int length)
        {
            try
            {
                //获取消息

                if (length == 0)
                {
                    this.Dispose();
                    return HandleResult.Error;
                }

                byte[] data = new byte[length];
                Marshal.Copy(pData, data, 0, length);

                recbuffer.Add(data);
                StatisticsManage.AddReceiveBytes(length);
                ProcessReceive();
            }
            catch (Exception)
            {
                this.Dispose();
                return HandleResult.Error;
            }

            return HandleResult.Ok;
        }
        private HandleResult OnSend(HPSocketCS.TcpClient sender, IntPtr pData, int length)
        {
            //Interlocked.CompareExchange(ref _isSend, 0, 1);
            //if (sendbuffer.Count > 0)
            //{
            //    ProcessSend();
            //}
            return HandleResult.Ok;
        }


    #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="v"></param>
        [Obsolete]
        public PipelineSocket(Socket socket, int v)
        {
            //this.socket = socket;
            //this.v = v;
        }


        private void OnDisposed()
        {
            if (Disposed != null)
            {
                Disposed(this, EventArgs.Empty);
            }
        }
        private void ProcessReceive()
        {
            if (recbuffer.Count <= 0) return;
            if (Interlocked.CompareExchange(ref _isRecvice, 1, 1) != 1) return;
            if (Interlocked.CompareExchange(ref _isRecviceSatus, 1, 0) != 0) return;
            OnReceiveCompleted(this, recbuffer);
            Interlocked.CompareExchange(ref _isRecviceSatus, 0, 1);
            ProcessReceive();
        }
        //触发数据接收转发事件
        /// <summary>
        /// 触发数据接收转发事件
        /// </summary>
        private void OnReceiveCompleted(object sender, IByteBuffer e)
        {
            while (recbuffer.Count > 0)
            {
                var argtemp = new BufferReceiveEventArgs();
                argtemp.Buffer = recbuffer.ReadAndRemoveBytes(0, recbuffer.Count);
                if (ReceiveCompleted != null)
                {
                    ReceiveCompleted(this, argtemp);
                }
            }
        }

        private void ProcessSend()
        {
            //设置客户端进入发送状态
            if (Interlocked.CompareExchange(ref _isSend, 1, 0) != 0) return;
            while (sendbuffer.Count > 0)
            {
                var count = sendbuffer.Count;
                count = count > this.BufferSize ? this.BufferSize : count;

                //读取数据
                var data = sendbuffer.ReadAndRemoveBytes(0, count);
                try
                {
                    this.socket.Send(data, 0, count);
                    continue;
                }
                catch (ObjectDisposedException ex)
                {
                    Interlocked.CompareExchange(ref _isSend, 0, 1);
                }
                catch (Exception ex)
                {
                    //发送数据时候引发未知的通信异常，管道通信已经被迫终止
                    GameIF.SystemExceptionProcessorBase.RunError(new Exception("发送数据时候引发未知的通信异常，管道通信已经被迫终止", ex));
                    //对象释放，关闭通信管道
                    this.Dispose();
                    //程序返回
                    return;
                }
                return;
            }
            Interlocked.CompareExchange(ref _isSend, 0, 1);
            if (sendbuffer.Count > 0)
            {
                ProcessSend();
                return;
            }
            OnSendCompleted(this, EventArgs.Empty);
        }
        //触发数据发送完毕事件
        private void OnSendCompleted(object sender, EventArgs e)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0)
                return;
            if (SendCompleted != null)
            {
                SendCompleted(sender, e);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.socket.ConnectionId.ToInt32();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            if (this.socket != null && this.socket.IsStarted)
                this.socket.Stop();
            OnDisposed();
        }

    #region 事件

        /**********************事件***********************/

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler Disposed;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<BufferReceiveEventArgs> ReceiveCompleted;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventArgs> SendCompleted;


        /**********************事件***********************/

    #endregion

    #region IPipelineSocket  接口定义

        int IPipelineSocket.BufferSize
        {
            get
            {
                return 1024;
            }
        }

        bool IPipelineSocket.Connected
        {
            get
            {
                if (this.socket == null) return false;
                return this.socket.IsStarted;
            }
        }

        string IPipelineSocket.IPAdress
        {
            get
            {
                return _remoteIP;
            }
        }

        uint IPipelineSocket.IPAdressUInt32
        {
            get
            {
                byte[] ipAry = new byte[4];
                string[] ipstr = this._remoteIP.Split('.');
                if (ipstr.Length != 4) return 0;

                for (int index = 0; index < 4; index++)
                {
                    if (!byte.TryParse(ipstr[index], out ipAry[index])) return 0;
                }

                return BitConverter.ToUInt32(ipAry, 0);

            }
        }

        int IPipelineSocket.Port
        {
            get
            {
                return _port;
            }
        }

        void IPipelineSocket.BeginConnect(string ip, int port, Action<bool> iaCallBack)
        {

            System.Action action = () =>
            {
                var result = false;
                try
                {
                    result = this.Connect(ip, port);
                }
                catch (Exception ex)
                {
                    LogerProvide.OnRunErrorMessage(this, ex);
                    return;
                }

                iaCallBack(result);

            };

            System.Threading.Tasks.Task.Factory.StartNew(action);
        }

        void IPipelineSocket.Close(bool reuseSocket)
        {
            this.Dispose();
        }
        /// <summary>
        /// 连接到指定的远端终结点
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string ip, int port)
        {
            return this.socket.Connetion(ip, (ushort)port);
        }

        void IPipelineSocket.Send(object msg)
        {
            var data = PacketCodecHandlerInternal.Instance.Encode(msg);

            Uyi.Net.StatisticsManage.AddSendMessages();
            Send(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            if (buffer.Length == 0) return;
            sendbuffer.Add(buffer);
            ProcessSend();
        }

        void IPipelineSocket.SetRemoteIP(string ip)
        {

            //检查IP格式
            string[] ipAry = ip.Split('.');
            if (ipAry.Length != 4) return;
            int ipcode;
            foreach (var r in ipAry)
            {
                if (!int.TryParse(r, out ipcode)) return;
                if (ipcode > 255 || ipcode < 0) return;
            }

            this._remoteIP = ip;
        }

        bool IPipelineSocket.StartReceive()
        {
            if (Interlocked.CompareExchange(ref _isRecvice, 1, 0) != 0) return false;
            ProcessReceive();
            return true;
        }

        /// <summary>
        /// 表示当前socket的回话ID
        /// </summary>
        int IPipelineSocket.SessionID
        {
            get
            {
                return this.socket.ConnectionId.ToInt32();
            }
        }

        /// <summary>
        /// 当前socket是否已经修改了远端IP
        /// </summary>
        bool IPipelineSocket.IsSetRemoteIP
        {
            get
            {
                return !string.IsNullOrEmpty(this._remoteIP);
            }
        }

    #endregion

    }


#else


    /// <summary>
    /// 管道通信Socket封装
    /// </summary>
    public sealed class PipelineSocket : IPipelineSocket
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Connected
        {
            get
            { 
                if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0)
                    return false;
                return _socket == null ? false : _socket.Connected;
            }
        }


        //发送数据缓冲区
        private IO.NetworkStream sendbuffer = new IO.NetworkStream();
        //事件参数
        BufferReceiveEventArgs arg;



        SocketNetworkStream networkStream;
        byte[] rec_buffer = new byte[1024];




        //管道支撑Socket对象
        private Socket _socket;
        //对象状态
        private int _isSend = 0;
        private int _isRecvice = 0;
        private long _isDisposed;
        private int _handleID = 0;
        private string RemoteIP = null; 

        /// <summary>
        /// 服务器断开连接事件
        /// </summary>
        public EventHandler Disposed { get; set; } = (sender, e) => { };
        /// <summary>
        /// 接收完成事件
        /// </summary>
        public EventHandler<BufferReceiveEventArgs> ReceiveCompleted { get; set; } = (sender, e) => { };
        /// <summary>
        /// 数据发送完成事件
        /// </summary>
        public EventHandler<EventArgs> SendCompleted { get; set; } = (sender, e) => { };

        #region 构造函数

        /// <summary>
        /// 初始化管道通信Socket封装对象新实例
        /// </summary>
        /// <param name="bufferSize">用作操作的缓冲区大小</param>
        public PipelineSocket(int bufferSize)
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //端口绑定，动态分配
            this._socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            this.networkStream = new SocketNetworkStream(this._socket);
            this.BufferSize = bufferSize;

            Init();

        }

        /// <summary>
        /// 初始化管道通信Socket封装对象新实例
        /// </summary>
        /// <param name="bufferSize">用作操作的缓冲区大小</param>
        /// <param name="isIPv6Cilent">指示创建的客户端类型</param>
        public PipelineSocket(int bufferSize, bool isIPv6Cilent)
        {
            if (isIPv6Cilent)
            {
                this._socket = SocketHelper.GetNewSocketIPv6Info();
                //端口绑定，动态分配
                this._socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
            }
            else
            {
                this._socket = SocketHelper.GetNewSocketInfo();
                //端口绑定，动态分配
                this._socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            }

            this.networkStream = new SocketNetworkStream(this._socket);
            this.BufferSize = bufferSize;

            Init();

        }

        /// <summary>
        /// 从基本Socket对象初始化管道通信Socket封装对象的新实例
        /// </summary>
        /// <param name="socket">用于管道支撑的连接Socket</param>
        /// <param name="bufferSize">用作操作的缓冲区大小</param>
        public PipelineSocket(Socket socket, int bufferSize)
        {
            this._socket = socket;
            this.networkStream = new SocketNetworkStream(this._socket);
            this.BufferSize = bufferSize;

            Init();
        }

        #endregion

        #region 属性参数

        /// <summary>
        /// 数据缓冲区大小
        /// </summary>
        public int BufferSize
        {
            get;
            private set;
        }
        /// <summary>
        /// 表示Socket是否处于数据接收环节
        /// </summary>
        public bool IsRecviced
        {
            get { return Interlocked.CompareExchange(ref _isRecvice, 0, 0) != 0; }
        }

        #endregion

        /// <summary>
        /// 连接到指定的远端终结点对象
        /// </summary>
        /// <param name="ip">要连接到的远端ip地址</param>
        /// <param name="port">要连接到的远端终结点的端口号</param>
        /// <returns>返回一个bool值，表示是否连接成功</returns>
        public bool Connect(string ip, int port)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0)
                return false;

            if (!this._socket.Connected)
            {
                try
                {
                    this._socket.Connect(ip, port);
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }

            }
            return true;

        }

        /// <summary>
        /// 释放使用的非托管资源，并可根据需要释放托管资源。
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
                return;

            //socket资源释放
            this._socket.Dispose();

            //信号量和异步参数缓冲区资源释放释放操作
            this.networkStream?.Dispose(); 
            this.sendbuffer?.Dispose();
             
            OnDisposed(this, EventArgs.Empty); 
            //事件绑定清除
            NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;

            Interlocked.CompareExchange(ref _isRecvice, 0, 1);
            Interlocked.CompareExchange(ref _isSend, 0, 1);


        }
        /// <summary>
        /// 关闭套接字对象连接
        /// </summary>
        /// <param name="reuseSocket">该值表示是否允许重用套接字</param>
        public void Close(bool reuseSocket)
        {
            if (reuseSocket)
            {
                if (this._socket.Connected)
                {
                    try
                    {
                        this._socket.Disconnect(reuseSocket);
                    }
                    catch (System.Exception ex)
                    {
                        SystemRunErrorPorvider.CatchException(ex);
                    }
                }
                return;
            }
            this.Dispose();
        }
        /// <summary>
        /// 启动数据接收处理器
        /// </summary>
        /// <returns></returns>
        public bool StartReceive()
        {
            if (!this._socket.Connected)
            {
                return false;
            }
            return ProcessReceive();

        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            Send(buffer, 0, buffer.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Send(byte[] buffer, int offset, int count)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0)
                return;
            if (buffer == null || buffer.Length <= 0)
                return;
            sendbuffer.Write(buffer, offset, count);
            this.ProcessSend();

        }






        /// <summary>
        /// 创建一个新的管道通信对象
        /// </summary>
        /// <param name="bufferSize">缓冲区大小</param>
        /// <returns></returns>
        public static IPipelineSocket CreateNewPipelineSocket(int bufferSize = 1024)
        {
            return new PipelineSocket(bufferSize);
        }
        /// <summary>
        /// 设置远端主机的IP地址
        /// 这个方法不会实际的更改的连接，只是修改了IP属性显示的IP字符串，一般不需要调用这个方法
        /// </summary>
        /// <param name="ip"></param>
        public void SetRemoteIP(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return;

            //检查IP格式
            string[] ipAry = ip.Split('.');
            if (ipAry.Length != 4) return;
            int ipcode;
            foreach (var r in ipAry)
            {
                if (!int.TryParse(r, out ipcode)) return;
                if (ipcode > 255 || ipcode < 0) return;
            }

            this.RemoteIP = ip;
        }
        /// <summary>
        /// 触发数据接收转发事件
        /// </summary>
        private void OnReceiveCompleted(object sender, int offset, int count)
        {
            try
            {
                arg.Offset = offset;
                arg.Count = count;
                ReceiveCompleted(sender, arg);
            }
            catch (System.Exception ex)
            {
                SystemRunErrorPorvider.CatchException(ex);
            }
        }


        #region 发送数据


        //异步发送进程
        private void ProcessSend()
        {
            if (Interlocked.CompareExchange(ref _isSend, 1, 0) != 0)
                return;
            if (sendbuffer.Count > 0)
            {
                var count = sendbuffer.Count;
                count = count > this.BufferSize ? this.BufferSize : count;

                //写入数据
                var data = sendbuffer.ReadAndRemoveBytes(count);
                try
                {
                    this.networkStream.BeginWrite(data, 0, count, SendCallback, null);
                    //增加发送字节量计数器
                    StatisticsManage.AddSendBytes(count);
                }
                catch //(Exception ex)
                {
                    //发送数据时候引发未知的通信异常，管道通信已经被迫终止
                    //SystemRunErrorPorvider.CatchException(new Exception("发送数据时候引发未知的通信异常，管道通信已经被迫终止", ex));
                    //对象释放，关闭通信管道
                    this.Dispose();
                    //程序返回
                    return;
                }
                return;
            }
            Send_End();
            if (sendbuffer.Count > 0)
            {
                ProcessSend();
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            if (!ar.IsCompleted) return;
            Send_End();
            try
            {
                this.networkStream.EndWrite(ar);
                OnSendCompleted(this, EventArgs.Empty);
            }
            catch (System.Exception)
            {
                this.Dispose();
                return;
            }
            ProcessSend();
        }
        private void Send_End()
        {
            Interlocked.CompareExchange(ref _isSend, 0, 1);
        }

        #endregion

        #region 数据接收

        private bool ProcessReceive()
        {
            if (Interlocked.CompareExchange(ref _isRecvice, 1, 0) != 0)
                return false;

            try
            {
                this.networkStream.BeginRead(rec_buffer, 0, 1024, ReceiveCallback, null);
                return true;
            }
            catch //(Exception ex)
            {
                this.Dispose();
                return false;
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!ar.IsCompleted) return;
            Receive_end();
            try
            {
                var count = this.networkStream.EndRead(ar);
                if (count <= 0)
                {
                    this.Dispose();
                    return;
                }
                OnReceiveCompleted(this, 0, count);
                ProcessReceive();
            }
            catch (System.Exception)
            {
                this.Dispose();
            }
        }
        private void Receive_end()
        {
            Interlocked.CompareExchange(ref _isRecvice, 0, 1);
        }


        #endregion


        //当连接被关闭触发的事件
        private void OnDisposed(object sender, EventArgs e)
        {
            try
            {
                Disposed(sender, e);
            }
            catch //(Exception)
            {
            }
        }
        //触发数据发送完毕事件
        private void OnSendCompleted(object sender, EventArgs e)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0)
                return;
            try
            {
                SendCompleted(sender, e);
            }
            catch (System.Exception)
            {
                this.Dispose();
            }
        }
        //基本对象初始化
        private void Init()
        {
            arg = new BufferReceiveEventArgs(this.rec_buffer);
            //获取管道绑定的ID 
            this._handleID = this._socket.Handle.ToInt32();

            this._socket.SendTimeout = 100;

            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            this._socket.UseOnlyOverlappedIO = false;

#if !Unity && KeepAlive
            //设置 KeepAlive 时间
            int dummySize = Marshal.SizeOf((uint)0);
            var inOptionValues = new byte[dummySize * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)15000).CopyTo(inOptionValues, dummySize);
            BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, dummySize * 2);

            this._socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);

#endif
        }
        //网络状态更改通知事件
        void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            try
            {
                if (e.IsAvailable)
                {
                    if (sendbuffer.Count <= 0)
                    {
                        this.Send(new byte[] { 1 });
                    }
                }
                else
                {
                    //网络不可用，调用关闭方法
                    this.Dispose();
                }
            }
            catch
            {
                this.Dispose();
            }
        }
        /// <summary>
        /// 发起对远端主机的异步连接请求
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="iaCallBack"></param>
        public void BeginConnect(string ip, int port, Action<bool> iaCallBack)
        {
            var socket = this._socket;
            if (socket == null || socket.Connected)
            { 
                try
                {
                    iaCallBack?.Invoke(socket?.Connected == true);
                }
                catch (System.Exception)
                {
                    this.Dispose();
                }
                return;
            }
            try
            {
                var e = SocketAsyncEventArgsPool.GetNewAsyncEventArgs();
                e.AcceptSocket = socket;
                e.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                e.Completed += (s, e_data) =>
                {
                    try
                    {
                        iaCallBack?.Invoke(e_data.SocketError == SocketError.Success);
                    }
                    catch //(Exception)
                    {
                        this.Dispose();
                    }
                    finally
                    {
                        e.Dispose();
                    }
                };
                if (!socket.ConnectAsync(e))
                { 
                    try
                    {
                        iaCallBack?.Invoke(e.SocketError == SocketError.Success);
                    }
                    catch //(Exception)
                    {
                        this.Dispose();
                    }
                    finally
                    {
                        e.Dispose();
                    }
                }
            }
            catch //(Exception EX)
            {
                iaCallBack(false);
            }


        }
        /// <summary>
        /// 获取连接的远端IP
        /// </summary>
        public string IPAdress
        {
            get
            {
                try
                {
                    if (RemoteIP != null) return RemoteIP;
                    return ((IPEndPoint)this._socket.RemoteEndPoint).Address.ToString();
                }
                catch
                {
                    return "";
                }
            }
        }
        /// <summary>
        /// 获取Socket当前远端的通信端口
        /// </summary>
        public int Port
        {
            get
            {
                return ((IPEndPoint)this._socket.RemoteEndPoint).Port;
            }
        }
        uint IPipelineSocket.IPAdressUInt32
        {
            get
            {
                byte[] ipAry = new byte[4];
                string[] ipstr = this.IPAdress.Split('.');
                if (ipstr.Length != 4) return 0;

                for (int index = 0; index < 4; index++)
                {
                    if (!byte.TryParse(ipstr[index], out ipAry[index])) return 0;
                }

                return BitConverter.ToUInt32(ipAry, 0);

            }
        }
        /// <summary>
        /// 表示当前通讯组件对象的会话ID
        /// </summary>
        public int SessionID
        {
            get
            {
                return this._handleID;
            }
        }
        /// <summary>
        /// 表示当前记录的对方IP信息是否已经被修改
        /// </summary>
        bool IPipelineSocket.IsSetRemoteIP { get { return RemoteIP != null; } }

        public Socket BaseSocket => this._socket;
    }



#endif



}