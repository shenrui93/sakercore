

#define AcceptAsync

using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
#if HPSocket
using HPSocketCS;
#endif
using static SakerCore.Net.Pipeline.SocketHelper;

namespace SakerCore.Net.Pipeline
{

#if AcceptAsync

    /// <summary>
    /// 服务器连接侦听器
    /// </summary>
    public class SocketServer : IListen
    {
        long _isListened = 0;
        Socket listen;
        Socket listenIPv6;
        string listen_ip;
        int listen_port;
        private long _isDispose;

        SocketAsyncEventArgsMetadata accept_listen_async_arg = null;
        SocketAsyncEventArgsMetadata accept_listen_IPv6_async_arg = null;

        /// <summary>
        /// 只是当前示例是否支持IPv6连接模式
        /// </summary>
        public bool IsIPv6Model { get; set; } = false;

        int maxlength = 1000;
        /// <summary>
        /// 初始化一个新的侦听器新实例
        /// </summary>
        public SocketServer(string ip, int port, int maxlength = 1000)
        {
            this.listen_ip = ip;
            this.listen_port = port;

            this.maxlength = maxlength;
        }

        /// <summary>
        /// 新的连接请求到达事件
        /// </summary>
        public EventHandler<AcceptedEventArgs> Accepted { get; set; } = (s, e) => { };

        /// <summary>
        /// 表示服务器是否处于关闭或停止状态
        /// </summary>
        public bool IsStop
        {
            get { return Interlocked.Read(ref _isListened) != 1; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int ListenPort
        {
            get
            {
                if (listen_port == 0)
                {
                    listen_port = ((IPEndPoint)this.listen.LocalEndPoint).Port;
                    return listen_port;
                }
                else
                {
                    return listen_port;
                }

            }
        }

        EventHandler<AcceptedEventArgs> IListen.Accepted
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 启动侦听器，接受连接请求
        /// </summary>
        /// <returns></returns>
        public bool StartAccept()
        {
            try
            {
                if (Interlocked.Read(ref _isListened) != 0)
                    return true;
                if (IsIPv6Model)
                {
                    InitStartIpv6();
                }
                else
                {
                    InitStart();
                }
                return true;
            }
            catch (System.Exception ex)
            {
                SystemRunErrorPorvider.CatchException(ex);
                return false;
            }
        }
        /// <summary>
        /// 关闭侦听器，拒绝连接请求
        /// </summary>
        public void StopAccept()
        {
            if (Interlocked.CompareExchange(ref _isListened, 0, 1) != 1) return;
            this.listen?.Close();
            this.listenIPv6?.Close();
        }

        private void InitStartIpv6()
        {
            if (Interlocked.CompareExchange(ref _isListened, 1, 0) != 0) return;
            listen?.Close();
            listen = GetNewSocketInfo();
            if (Net.SocketHelper.PortInUse(this.listen_port))
            {
                var pro = Net.SocketHelper.PortInUseProcess(this.listen_port);
                if (pro != null)
                    throw new System.Exception($@"服务端口【{ this.listen_port}】已经被其他进程占用。占用进程信息：
PID:{pro.Id}
ProcessName:{pro.ProcessName}");
            }
            listen.Bind(new IPEndPoint(IPAddress.Any, this.listen_port));
            listen.Listen(100);
            var e = SocketAsyncEventArgsPool.GetNewAsyncEventArgs();
            accept_listen_async_arg = e;
            e.Completed += (S, EE) =>
            {
                AcceptAsyncCallback(listen, EE);
            };
            ProessAccept(listen, e);


            listenIPv6?.Close();
            listenIPv6 = GetNewSocketIPv6Info();
            listenIPv6.Bind(new IPEndPoint(IPAddress.IPv6Any, this.listen_port));
            listenIPv6.Listen(100);
            var e_ipv6 = SocketAsyncEventArgsPool.GetNewAsyncEventArgs();
            accept_listen_IPv6_async_arg = e_ipv6;
            e_ipv6.Completed += (S, EE) =>
            {
                AcceptAsyncCallback(listenIPv6, EE);
            };
            ProessAccept(listenIPv6, e_ipv6);
        }
        private void InitStart()
        {
            if (Interlocked.CompareExchange(ref _isListened, 1, 0) != 0) return;
            listen?.Close();
            listen = GetNewSocketInfo();
            listen.Bind(new IPEndPoint(IPAddress.Parse(this.listen_ip), this.listen_port));
            listen.Listen(100);
            var e = SocketAsyncEventArgsPool.GetNewAsyncEventArgs();
            accept_listen_async_arg = e;
            e.Completed += (S, EE) =>
            {
                AcceptAsyncCallback(listen, EE);
            };
            ProessAccept(listen, e);
        }

        private void ProessAccept(Socket accept_listen, SocketAsyncEventArgsMetadata e)
        {
            if (Interlocked.Read(ref _isDispose) != 0)
                return;
            try
            {
                e.AcceptSocket = null;
                if (!accept_listen.AcceptAsync(e))
                {
                    AcceptAsyncCallback(accept_listen, e);
                }
            }
            catch //(Exception ex)
            {
            }

        }
        private void AcceptAsyncCallback(Socket accept_listen, SocketAsyncEventArgsMetadata e)
        {
            if (e.SocketError == SocketError.Success && e.LastOperation == SocketAsyncOperation.Accept)
            {
                var socket = e.AcceptSocket;

                if (socket != null && socket.Connected)
                {
                    Accepted(this, new AcceptedEventArgs(new PipelineSocket(socket, 1024)));
                }
                else if (socket != null)
                {
                    socket.Close();
                }

            }
            ProessAccept(accept_listen, e);
        }


        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDispose, 1, 0) != 0)
                return;
            try
            {
                this.listen?.Close();
            }
            catch (NullReferenceException)
            {
            } 
        }
    } 
#else


    /// <summary>
    /// 服务器连接侦听器
    /// </summary>
    public class SocketServer : TcpListener, IListen
    {

        int maxlength = 1000;
        /// <summary>
        /// 初始化一个新的侦听器新实例
        /// </summary>
        public SocketServer(string ip, int port, int maxlength = 1000)
            : base(new System.Net.IPEndPoint(string.IsNullOrEmpty(ip) ? IPAddress.Any : IPAddress.Parse(ip), port))
        {
            this.maxlength = maxlength;
        }

        /// <summary>
        /// 新的连接请求到达事件
        /// </summary>
        public EventHandler<AcceptedEventArgs> Accepted { get; set; } = (s, e) => { };
        private int _isDispose;


        /// <summary>
        /// 表示服务器是否处于关闭或停止状态
        /// </summary>
        public bool IsStop
        {
            get { return !base.Active; }
        }

        /// <summary>
        /// 启动侦听器，接受连接请求
        /// </summary>
        /// <returns></returns>
        public bool StartAccept()
        {
            try
            {
                if (base.Active)
                    return true;

                base.Start(maxlength);
                ProessAccept();

                return true;
            }
            catch (Exception ex)
            {
                SystemRunErrorPorvider.CatchException(ex);
                return false;
            }
        }

        /// <summary>
        /// 关闭侦听器，拒绝连接请求
        /// </summary>
        public void StopAccept()
        {
            this.Stop();
        }

        //接受连接进程
        void ProessAccept()
        {
            try
            {
                if (Interlocked.CompareExchange(ref _isDispose, 0, 0) != 0)
                    return;
                if (!this.Active)
                    return;
                this.BeginAcceptSocket(new AsyncCallback(AcceptCallBack), this);
            }
            catch
            {
                this.Dispose();
            }
        }
        //异步接受连接回调
        void AcceptCallBack(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                try
                {
                    var socket = this.EndAcceptSocket(ar);

                    if (Accepted != null && socket != null && socket.Connected)
                    {
                        Accepted(this, new AcceptedEventArgs(new PipelineSocket(socket, 1024)));
                    }
                    else if (socket != null)
                    {
                        socket.Close();
                    }
                }
                catch
                {
                    // Uyi.Net.Message.LogerProvide.OnRunErrorMessage(this, ex);
                }
            }

            ProessAccept();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDispose, 1, 0) != 0)
                return;
            try
            {
                if (this.Server != null)
                    this.Server.Close();
            }
            catch (NullReferenceException)
            {
            }
            GC.SuppressFinalize(this);
        }
    }

#endif

    /// <summary>
    /// Socket 的创建帮助类
    /// </summary>
    public static class SocketHelper
    {
        /// <summary>
        /// 获取一个新的Socket对象，基于通讯版本 IPV4
        /// </summary>
        /// <returns></returns>
        public static Socket GetNewSocketInfo()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        /// <summary>
        /// 获取一个新的Socket对象，基于通讯版本 IPV6
        /// </summary>
        /// <returns></returns>
        public static Socket GetNewSocketIPv6Info()
        {
            return new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
