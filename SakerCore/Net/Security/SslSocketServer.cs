/***************************************************************************
 * 
 * 创建时间：   2016/12/6 14:44:46
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供一个实现 X509Certificate 安全验证协议的安全套接字监听器
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using SakerCore.Net.Pipeline;
using static SakerCore.Net.Pipeline.SocketHelper;

namespace SakerCore.Net.Security
{
    /// <summary>
    /// 提供一个实现 X509Certificate 安全验证协议的安全套接字监听器
    /// </summary>
    public sealed class SslSocketServer : IListen
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
        
        private readonly X509Certificate serverCertificate;     //监听器证书

        /// <summary>
        /// 初始化一个新的侦听器新实例
        /// </summary>
        public SslSocketServer(string ip, int port, X509Certificate serverCertificate)
        {
            this.listen_ip = ip;
            this.listen_port = port;
            this.serverCertificate = serverCertificate;

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
                //开始一个新连接的安全验证
                do_ProcessSafeConnect(socket);

            }
            ProessAccept(accept_listen, e);
        }
        private void do_ProcessSafeConnect(Socket socket)
        { 
            try
            {
                if (socket == null) return;
                var ssl_socket = new SslAppectSocket(socket);
                ssl_socket.BeginAuthenticateAsServer(serverCertificate, this.SafeConnectAuthenticateCallback, ssl_socket);
            }
            catch (System.Exception ex)
            {
                SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
            }
        }
        private void SafeConnectAuthenticateCallback(IAsyncResult ar)
        {
            if (!ar.IsCompleted) return;
            var ssl_socket = ar.AsyncState as SslAppectSocket;
            if (ssl_socket == null) return;

            try
            {
                ssl_socket.EndAuthenticateAsServer(ar);
                this.Accepted(this, new AcceptedEventArgs(ssl_socket));
            }
            catch //(Exception ex)
            {
                ssl_socket?.Dispose(); 
            }

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
    }
}
