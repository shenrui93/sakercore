
using System;
using System.Threading;
using Uyi.Net.Analysis;
using Uyi.Net.Message;
using Uyi.IO;
using System.Net.Sockets;

namespace Uyi.Net.Pipeline
{

    /// <summary>
    /// 表示服务器管道数据处理对象
    /// </summary>
    public abstract class Pipeline : IPipeline
    {
        private int _isDisposed = 0;
        private IPipelineSocket _client;



        //  接收数据缓冲区
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        protected Uyi.IO.NetworkStream recbuffer = new Uyi.IO.NetworkStream();   //事件参数 


        /// <summary>
        /// 一个空的客户端管道，该管道的任何方法调用均保证不会有任何实际动作
        ///  ---- 该字段是只读的
        /// </summary>
        public static readonly IPipeline EmptyPipeline;

        static Pipeline()
        {
            EmptyPipeline = new EmptyPipeline();
        }

        /// <summary>
        /// 
        /// </summary>
        public PipelineEventHandle<MessageComingArgs> MessageComing { get; set; } = (S, E) => { };
        /// <summary>
        /// 管道释放事件
        /// </summary>
        public PipelineEventHandle<PipelineDisposedEventArgs> PipelineDisposed { get; set; } = (S, E) => { };
        /// <summary>
        /// 数据发送完成事件
        /// </summary>
        public PipelineEventHandle<EventArgs> PipelineSendCompleted { get; set; } = (S, E) => { };
        /// <summary>
        /// 
        /// </summary>
        public Action<byte[]> BeforeSendDataEventHandle { get; set; } = (r) => { };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        protected virtual void OnBeforeSendDataEventHandle(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0) return;
            BeforeSendDataEventHandle(buffer);
        } 

        void _client_SendCompleted(object sender, EventArgs e)
        {
            if (PipelineSendCompleted != null)
            {
                PipelineSendCompleted(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IPipelineSocket Client
        {
            get { return this._client; }
            set
            {
                if (_client != null && !object.ReferenceEquals(_client, value))
                {
                    _client.Dispose();
                }
                this._client = value;
                if (_client != null)
                {
                    _client.ReceiveCompleted = Client_ReceiveCompleted;
                    _client.SendCompleted = _client_SendCompleted;
                    this._client.Disposed = Client_Disposed;
                }
            }
        }


        /// <summary>
        /// 管道编号
        /// </summary>
        public virtual int SessionID
        {
            get { return this.Client.SessionID; }
        }

        /// <summary>
        /// 关闭管道并释放资源
        /// </summary>
        public virtual void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// 向管道绑定的另一端发送数据
        /// </summary>
        /// <param name="msg"></param>
        public virtual void SendData(object msg)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0)
                return;
            var data = PacketCodecHandlerInternal.Encode(msg);
            StatisticsManage.AddSendMessages();
            this.SendData(data);
        }

        /// <summary>
        /// 向管道绑定的另一端发送数据
        /// </summary>
        /// <param name="data"></param>
        public virtual void SendData(byte[] data)
        {
            if (data == null || data.Length <= 0) return;

            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0)
                return;
            OnBeforeSendDataEventHandle(data);
            this._client.Send(data);
        }

        /// <summary>
        /// 尝试进行远端终结点连接
        /// </summary>
        /// <param name="ip">远端IP</param>
        /// <param name="port">远端端口</param>
        /// <returns></returns>
        public virtual bool Connect(string ip, int port)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0)
                return false;
            if (this.Client.Connect(ip, port))
            {
                //this.Client.StartReceive();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 释放对象所占用的资源
        /// </summary>
        public virtual void Dispose()
        {
            this.Dispose(null);
        }

        /// <summary>
        /// 释放对象所占用的资源
        /// </summary>
        /// <param name="userObject"></param>
        public virtual void Dispose(object userObject)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
                return;
            this._client.Dispose();

            OnDisposed(userObject);
        }

        /// <summary>
        /// 触发管道被释放事件
        /// </summary>
        /// <param name="userObject"></param>
        protected void OnDisposed(object userObject)
        {
            PipelineDisposed(this, new PipelineDisposedEventArgs() { UserObject = userObject });
        }

        /// <summary>
        /// 触发消息事件
        /// </summary>
        /// <param name="e"></param>
        internal virtual void OnMessageComing(MessageComingArgs e)
        {
            try
            {
                if (MessageComing == null)
                    return;
                StatisticsManage.AddReceiveMessages();
                MessageComing(this, e);
            }
            catch (Exception ex)
            {
                SystemRunErrorPorvider.CatchException(ex);
            }
        }

        /// <summary>
        /// 客户端数据交换管道被释放事件处理程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Client_Disposed(object sender, EventArgs e)
        {
            this.Dispose();
        }
        /// <summary>
        /// 数据管道收到数据时的事件处理程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected abstract void Client_ReceiveCompleted(object sender, BufferReceiveEventArgs e);
        /// <summary>
        /// 向对端发送一条Ping指令
        /// </summary>
        public virtual void Ping()
        {

        }


        /// <summary>
        /// 当前管道通讯绑定的用户对象
        /// </summary>
        public object Tag
        {
            get; set;
        }

    }

    /// <summary>
    /// 一个空的客户端管道，该管道的任何方法调用均保证不会有任何实际动作
    /// </summary>
    internal class EmptyPipeline : IPipeline
    {
#pragma warning disable CS0067
        private IPipelineSocket ps = new EmptyPipelineSocket();
        /// <summary>
        /// 
        /// </summary>
        public PipelineEventHandle<MessageComingArgs> MessageComing { get; set; } = (S, E) => { };
        /// <summary>
        /// 
        /// </summary>
        public PipelineEventHandle<EventArgs> PipelineSendCompleted { get; set; } = (S, E) => { };

        public IPipelineSocket Client
        {
            get
            {
                return ps;
            }
            set
            {

            }
        }

        public int SessionID
        {
            get
            {
                return -1;
            }
            set
            {

            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public void SendData(object msg)
        {
        }

        public void SendData(byte[] data)
        {
        }

        public void Dispose(object userObject)
        {
            PipelineDisposed(this, new PipelineDisposedEventArgs()
            {
                UserObject = userObject
            });
        }

        public void Dispose()
        {
            if (PipelineSendCompleted != null)
                PipelineDisposed(this, PipelineDisposedEventArgs.Empty);
        }

        public PipelineEventHandle<PipelineDisposedEventArgs> PipelineDisposed { get; set; } = (S, E) => { };
        public object Tag
        {
            get; set;
        }

        public Action<byte[]> BeforeSendDataEventHandle
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
        /// 向对端发送一条Ping指令
        /// </summary>
        public virtual void Ping()
        {

        }
    }

    internal class EmptyPipelineSocket : IPipelineSocket
    {
        public void Close(bool reuseSocket)
        {
            Disposed(this, EventArgs.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        public EventHandler<BufferReceiveEventArgs> ReceiveCompleted { get; set; } = (S, E) => { };

        public void Send(byte[] buffer)
        {
            if (SendCompleted != null)
                SendCompleted(this, EventArgs.Empty);
        }
        public void Send(byte[] buffer, int offset, int count)
        {
            if (SendCompleted != null)
                SendCompleted(this, EventArgs.Empty);
        }
        public bool Connect(string ip, int port)
        {
            return true;
        }

        public int BufferSize
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public void Send(object msg)
        {
            if (SendCompleted != null)
                SendCompleted(this, EventArgs.Empty);
        }

        public EventHandler<EventArgs> SendCompleted { get; set; } = (S, E) => { };

        public bool Connected
        {
            get { return true; }
        }

        public bool StartReceive()
        {
            return true;
        }

        public void Dispose()
        {
            if (this.Disposed != null)
                Disposed(this, EventArgs.Empty);
        }

        public EventHandler Disposed { get; set; } = (S, E) => { };


        public void BeginConnect(string ip, int port, Action<bool> iaCallBack)
        {
            iaCallBack(true);
        }

        public void SetRemoteIP(string ip)
        {
            //throw new NotImplementedException();
        }


        public string IPAdress
        {
            get { return string.Empty; }
        }

        public int Port
        {
            get
            {
                return -1;
            }
        }

        public uint IPAdressUInt32
        {
            get
            {
                return 0;
            }
        }
        public object Tag
        {
            get; set;
        }

        public int SessionID
        {
            get
            {
                return 0;
            }
        }

        public bool IsSetRemoteIP
        {
            get
            {
                return false;
            }
        }

        public Socket BaseSocket
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

}
