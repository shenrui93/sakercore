using System;
using System.Threading;
using Uyi.Net.Analysis;
using Uyi.Net.Message;
using Uyi.Net.SocketPipeline;
using Uyi.Serialization;
using Uyi.Threading;

namespace Uyi.Net.Pipeline
{

    /// <summary>
    /// 表示数据交换处理管道
    /// </summary>
    public class PipelineBox : Pipeline, IGamePipeline
    {

        //字段
        private IPipelineSocket _gameServer;
        private object root = new object();
        private int isNormalExit = 0;
        private int isDispose = 0;
        private IO.NetworkStream game_recbuffer = new IO.NetworkStream();


        /// <summary>
        /// 从一个已经连接的通讯IPipelineSocket基础上创建一个客户端管道实例
        /// </summary>
        /// <param name="socket">绑定IPipelineSocket</param>
        /// <param name="bufferSize">数据交换的缓冲区大小</param>
        /// <returns></returns>
        public static PipelineBox CreateNewPipelineBoxFromSocket(IPipelineSocket socket, int bufferSize)
        {
            if (socket == null || !socket.Connected)
            {
                var ex = new Exception("只有是连接成功的IPipelineSocket对象才能进行管道创建");
                SystemRunErrorPorvider.CatchException(ex);
                throw ex;
            }



            var _pipelineSocket = socket;

            //创建管道
            var pip = new PipelineBox(_pipelineSocket.GetHashCode());
            pip.Client = _pipelineSocket;
            return pip;
        }


        /// <summary>
        /// 初始化数据交换处理管道新实例
        /// </summary>
        public PipelineBox(int _pipelineID)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="p"></param>
        public PipelineBox(IPipelineSocket client, int p)
            : this(p)
        {
            this.Client = client;
        }



        //属性
        /// <summary>
        /// 管道与游戏服务器的连接管道
        /// </summary>
        public virtual IPipelineSocket GameServer
        {
            get { return _gameServer; }
            set
            {

                if (this._gameServer != null && !object.Equals(this._gameServer, value))
                {
                    //throw new Exception();
                    this._gameServer.Dispose();
                }

                lock (this.root)
                {
                    _gameServer = value;
                    if (this._gameServer != null)
                    {
                        this._gameServer.ReceiveCompleted = this._gameServer_ReceiveCompleted;
                        this._gameServer.Disposed = _gameServer_Disposed;
                    }
                }




            }
        }

        /// <summary>
        /// 管道释放
        /// </summary>
        /// <param name="userObject"></param>
        public override void Dispose(object userObject)
        {
            if (Interlocked.CompareExchange(ref isDispose, 1, 0) != 0)
                return; 
                this._gameServer?.Dispose();  
            base.Dispose(userObject);
        }
         
        /// <summary>
        /// 关闭管道的游戏服务器连接
        /// </summary>
        public virtual void DisConnectGameServer()
        {
            Interlocked.CompareExchange(ref isNormalExit, 1, 0);
            if (_gameServer != null)
            {
                this._gameServer.Dispose();
                this._gameServer = null;
            }
        }


        //私有处理方法 
        void _gameServer_ReceiveCompleted(object sender, BufferReceiveEventArgs e)
        {
            //从游戏服务器接收到数据
            game_recbuffer.Write(e.Buffer,e.Offset,e.Count);
            PipelineOperation.OnServer2ClientMessageComing(this, ref game_recbuffer, this.OnMessageComing);

        }
        void _gameServer_Disposed(object sender, EventArgs e)
        {
            if (Interlocked.CompareExchange(ref isNormalExit, 0, 0) != 0)
                this._gameServer = null;
            else
                this.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Client_ReceiveCompleted(object sender, BufferReceiveEventArgs e)
        {
            recbuffer.Write(e.Buffer, e.Offset, e.Count);
            PipelineOperation.OnClient2ServerMessageComing(this, ref recbuffer, this.OnMessageComing);
        }


        /// <summary>
        /// 开始一个异步操作来创建游戏服务器连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="callback"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        public IAsyncResult BeginConnectGameServer(string ip, int port, AsyncCallback callback, object userState)
        {
            //异步状态创建
            AsyncConnectGameServerResult iar = new AsyncConnectGameServerResult()
            {
                asyncPipeline = this,
                userState = userState
            };
            this.BeginConnectGameServer(ip, port, r =>
            {
                iar.result = r;
                iar.isCompleted = true;
                callback(iar);
            });


            return iar;
        }
        /// <summary>
        /// 开始一个异步操作来创建游戏服务器连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="callback"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        public IAsyncResult BeginConnectGameServer(uint ip, int port, AsyncCallback callback, object userState)
        {
            return BeginConnectGameServer(new System.Net.IPAddress(ip).ToString(), port, callback, userState);
        }
        /// <summary>
        /// 结束游戏服务器连接操作
        /// </summary>
        /// <param name="iar"></param>
        /// <returns></returns>
        public bool EndConnectGameServer(IAsyncResult iar)
        {
            var gameIAr = iar as AsyncConnectGameServerResult;
            if (gameIAr == null) throw new InvalidOperationException();
            if (!gameIAr.asyncPipeline.Equals(this)) throw new InvalidOperationException();
            return gameIAr.result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="callback"></param>
        public void BeginConnectGameServer(string ip, int port, Action<bool> callback)
        {
            if (Interlocked.CompareExchange(ref isDispose, 0, 0) != 0)
            {
                callback(false);
                return;
            }
            //置于正常结束状态
            Interlocked.CompareExchange(ref isNormalExit, 1, 0);
            if (this._gameServer != null)
            {
                if (this._gameServer != null)
                {
                    //判断连接的服务器信息是否与原来的相同
                    if (this.GameServer.Connected)
                    {

                        if (this.GameServer.IPAdress == ip && (this.GameServer.Port == port))
                        {
                            Interlocked.CompareExchange(ref isNormalExit, 0, 1);
                            {
                                callback(true);
                                return;
                            }
                        }
                    }
                    this._gameServer.Dispose(); 
                }
            }
            //创建通讯支撑管道
            var pipsocket = new PipelineSocket(1024);
            this.GameServer = pipsocket;
            if (Interlocked.CompareExchange(ref isDispose, 0, 0) != 0)
            {
                _gameServer.Dispose();
                {
                    callback(false);
                    return;
                }
            }
            this._gameServer.BeginConnect(ip, port, r =>
            {
                if (r)
                {
                    Interlocked.CompareExchange(ref isNormalExit, 0, 1);
                    this._gameServer.StartReceive();
                    callback(true);
                }
                else
                {
                    Interlocked.CompareExchange(ref isNormalExit, 1, 0);
                    if (this._gameServer != null)
                    {
                        //结束与游戏服务器的连接
                        this._gameServer.Dispose(); 
                    }
                    callback(false);
                }
            });
        }


    }

}
