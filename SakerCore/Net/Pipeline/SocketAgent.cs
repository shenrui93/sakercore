/***************************************************************************
 * 
 * 创建时间：   2016/5/12 14:08:42
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供Socket代理信息
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

namespace SakerCore.Net.Pipeline
{
    /// <summary>
    /// 提供Socket代理信息
    /// </summary>
    public sealed class SocketAgent : IDisposable
    {
        IListen _listen = null;
        SocketAgentConfig _config = null;
        /// <summary>
        /// 
        /// </summary>
        public SocketAgentConfig SocketAgentConfig { get { return _config; } }

        Dictionary<int, SocketAgentClientSession> session_dic = new Dictionary<int, SocketAgentClientSession>();
        object root = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localPort"></param>
        /// <param name="remIP"></param>
        /// <param name="remPort"></param>
        public SocketAgent(int localPort, string remIP, int remPort)
        {
            this._config = new SocketAgentConfig()
            {
                localPort = localPort,
                remIP = remIP,
                remPort = remPort,
            };
            _listen = new SocketServer("0.0.0.0", localPort);
            _listen.Accepted = AcceptedCallback;
        }
        private void AcceptedCallback(object sender, AcceptedEventArgs e)
        {
            try
            {
                var session = new SocketAgentClientSession(e.AcceptSocket, _config, this); 
            }
            catch (System.Exception ex)
            {
                SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
            }
        }
        /// <summary>
        /// 启动代理服务
        /// </summary>
        public void StartServer()
        {
            _listen?.StartAccept();
        }
        /// <summary>
        /// 停止代理服务
        /// </summary>
        public void StopServer()
        {
            _listen?.StopAccept();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _listen?.Dispose();
            lock (root)
            {
                List<SocketAgentClientSession> list = new List<SocketAgentClientSession>();
                foreach (var r in session_dic)
                {
                    var a = r.Value;
                    if (a != null)
                    {
                        list.Add(a);
                    }
                }
                var data = list.ToArray();
                for (var index = 0; index < data.Length; index++)
                {
                    data[index]?.Dispose();
                }
                session_dic = null;
            }
        }

        /// <summary>
        /// 新的客户端代理建立时触发时间
        /// </summary>
        public SakerCore.EventHandle<ISocketAgentClientSession> NewAgentClientSession { get; set; } = (sender, e) => { };
        /// <summary>
        /// 一个客户端关闭时调用
        /// </summary>
        public EventHandle<ISocketAgentClientSession> AgentClientSessionClose { get; set; } = (sender, e) => { };

        #region SocketAgentClientSession

        class SocketAgentClientSession : IDisposable, ISocketAgentClientSession
        {

            SocketAgent SocketAgentServer;
            SocketAgentConfig _config;
            private IPipelineSocket acceptSocket;
            private IPipelineSocket remSocket;
            long _isDisposed = 0;

            public SocketAgentClientSession(IPipelineSocket acceptSocket
                , SocketAgentConfig _config
                , SocketAgent SocketAgentServer
                )
            {
                this.SocketAgentServer = SocketAgentServer;
                this._config = _config;
                this.acceptSocket = acceptSocket;
                this.remSocket = new PipelineSocket(1024);
                Initializer();
            }

            private void Initializer()
            {
                if (this.SocketAgentServer != null)
                {
                    lock (this.SocketAgentServer.root)
                    {
                        this.SocketAgentServer.session_dic[this.SessionID] = this;
                    }
                }

                this.acceptSocket.ReceiveCompleted = acceptSocket_ReceiveCompletedCallback;
                this.acceptSocket.Disposed = DisposedCallback;

                this.remSocket.ReceiveCompleted = remSocket_ReceiveCompletedCallback;
                this.remSocket.Disposed = DisposedCallback;

                this.remSocket.BeginConnect(this._config.remIP, this._config.remPort, (result) =>
                {
                    remSocket_BeginConnectCallback(result);

                });
            }

            private void remSocket_BeginConnectCallback(bool result)
            {
                if (!result)
                {
                    this.Dispose();
                    return;
                }
                SocketAgentServer.NewAgentClientSession(SocketAgentServer, this);
                this.acceptSocket.StartReceive();
                this.remSocket.StartReceive();

            }
            private void remSocket_ReceiveCompletedCallback(object sender, BufferReceiveEventArgs e)
            {
                this.acceptSocket.Send(e.Buffer,e.Offset,e.Count);
            }
            private void DisposedCallback(object sender, EventArgs e)
            {
                this.Dispose();
            }
            private void acceptSocket_ReceiveCompletedCallback(object sender, BufferReceiveEventArgs e)
            {
                this.remSocket.Send(e.Buffer, e.Offset, e.Count);
            }

            /// <summary>
            /// 释放资源对象
            /// </summary>
            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;

                if (this.SocketAgentServer != null)
                {
                    lock (this.SocketAgentServer.root)
                    {
                        this.SocketAgentServer.session_dic[this.SessionID] = null;
                    }
                }
                this.SocketAgentServer.AgentClientSessionClose(SocketAgentServer, this);
                this.acceptSocket?.Dispose();
                this.remSocket?.Dispose();
            }
            /// <summary>
            /// 会话ID
            /// </summary>
            public int SessionID { get { return acceptSocket.SessionID; } }
            /// <summary>
            /// 获取接收的客户端对象
            /// </summary>
            public IPipelineSocket AcceptSocket { get { return acceptSocket; } }
        }

        #endregion

        #region SocketAgentConfig


        #endregion
    }
    /// <summary>
    /// socket 代理对象的配置信息
    /// </summary>
    public class SocketAgentConfig
    {
        /// <summary>
        /// 本地代理端口
        /// </summary>
        public int localPort { get; internal set; }
        /// <summary>
        /// 代理的远端IP
        /// </summary>
        public string remIP { get; internal set; }
        /// <summary>
        /// 代理的远端端口
        /// </summary>
        public int remPort { get; internal set; }
    }

}
