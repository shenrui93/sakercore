/***************************************************************************
 * 
 * 创建时间：   2016/11/29 10:14:32
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供UDP服务程序支持
 * 
 * *************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SakerCore.Net.Pipeline
{

    #region UdpServer


    /// <summary>
    /// 提供UDP服务程序支持
    /// </summary>
    public class UdpServer : IDisposable
    {
        Socket server;
        SocketAsyncEventArgsMetadata rec_arg;
        private string _ip;
        private int _port;

        long _isReceived = 0;
        long _isDisposed = 0;
        long _isStared = 0;

        /// <summary>
        /// 初始化
        /// </summary>
        public UdpServer(string ip, int port)
        {
            this._ip = ip;
            this._port = port;
        }

        private void Initializer()
        {
            server?.Close();
            rec_arg?.Dispose();

            _isReceived = 0;
            _isDisposed = 0;


            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            rec_arg = SocketAsyncEventArgsPool.GetNewAsyncEventArgs();
            rec_arg.AcceptSocket = server;
            rec_arg.SetBuffer(new byte[1024 * 4], 0, 1024 * 4);
            rec_arg.Completed += Rec_arg_Completed;
            server.Bind(new IPEndPoint(IPAddress.Parse(_ip), _port));
        }
        private void Rec_arg_Completed(object sender, SocketAsyncEventArgsMetadata e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                EndReceived();
                ProcessReceive();
                return;
            } 
            var data_len = e.BytesTransferred;
            var data = new byte[data_len];
            System.Buffer.BlockCopy(e.Buffer, 0, data, 0, data_len);
            IPEndPoint endpoint = e.RemoteEndPoint as IPEndPoint;
            EndReceived();
            ProcessReceive();
            UdpServerReceived(this, new UdpServerReceiveEventArgs()
            {
                Data = data,
                RemoteEndPoint = endpoint,
            });
        }
        private void EndReceived()
        {
            if (Interlocked.CompareExchange(ref _isReceived, 0, 1) != 1) return;
        }
        private void ProcessReceive()
        {
            while (true)
            {
                if (Interlocked.Read(ref _isDisposed) == 1)  return; 
                try
                {
                    if (Interlocked.CompareExchange(ref _isReceived, 1, 0) != 0) return;
                    rec_arg.RemoteEndPoint = CreateEmptyIpendpoint();
                    if (!server.ReceiveFromAsync(rec_arg))
                    {
                        Rec_arg_Completed(this, rec_arg);
                    }
                }
                catch //(Exception ex)
                {
                    if (Interlocked.CompareExchange(ref _isStared, 1, 1) == 1)
                    {
                        if (Interlocked.Read(ref _isDisposed) == 1) return;
                        Initializer();
                        continue;
                    }
                }
                break;
            }
        }
        private EndPoint CreateEmptyIpendpoint()
        {
            return new IPEndPoint(0, 0);
        }
        /// <summary>
        /// 开启服务
        /// </summary>
        public bool Start()
        {
            try
            {
                if(Interlocked.Read(ref _isDisposed) == 1)
                {
                    do_Disposed();
                    return false;
                }
                if (Interlocked.CompareExchange(ref _isStared, 1, 0) != 0)
                {
                    //服务已经开启
                    return true;
                }
                Initializer();
                ProcessReceive();
                return true;
            }
            catch  
            {
                this.Dispose();
            }
            return false;
        }


        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0)
            {
                //对象已经被释放
                return;
            }
            if (Interlocked.CompareExchange(ref _isStared, 0, 1) != 1)
            {
                //服务没有正确开启
                return;
            }
            server?.Close();
            rec_arg?.Dispose();
            _isReceived = 0;
        }

        /// <summary>
        /// 释放使用所占用的资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            do_Disposed();
        }
        private void do_Disposed()
        {
            server?.Close();
            rec_arg?.Dispose();
            _isReceived = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        public EventHandle<UdpServerReceiveEventArgs> UdpServerReceived = (o, r) => { };
        /// <summary>
        /// 对象被释放
        /// </summary>
        public EventHandle Disposed = (o, r) => { };

    }

    /// <summary>
    /// Udp 数据包数据接收事件参数信息
    /// </summary>
    public class UdpServerReceiveEventArgs : System.EventArgs
    {
        /// <summary>
        /// 事件数据
        /// </summary>
        public byte[] Data { get; internal set; }
        /// <summary>
        /// 数据发送的远端信息
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; internal set; }
    }


    #endregion

    #region UdpClient


    /// <summary>
    /// Udp 客户端服务对象
    /// </summary>
    public class UdpClient : IDisposable
    {
        Socket client;
        SocketAsyncEventArgsMetadata send_arg;
        ConcurrentQueue<UdpSendMessageData> _send_query = new ConcurrentQueue<UdpSendMessageData>();
        long _isSend = 0;


        /// <summary>
        /// 
        /// </summary>
        public UdpClient()
        {
            Initializer();
        }

        private void Initializer()
        {
            client?.Close();
            send_arg?.Dispose();

            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            send_arg = SocketAsyncEventArgsPool.GetNewAsyncEventArgs();
            send_arg.AcceptSocket = client;
            send_arg.SetBuffer(new byte[1024 * 2], 0, 1024 * 2);
            send_arg.Completed += Send_arg_Completed;

        }
        private void Send_arg_Completed(object sender, SocketAsyncEventArgsMetadata e)
        {
            var send_msg = e.UserToken as UdpSendMessageData;
            var event_arg = new UdpSendCompleteEventArg()
            {
                Message = send_msg,
                SocketError = e.SocketError
            };
            if (e.SocketError != SocketError.Success)
            {
                Initializer();
            }
            if (send_msg != null)
            {
                UdpSendComplete?.Invoke(this, event_arg);
            }
            EndSend();
            ProcessSend();
        }
        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="data"></param>
        public void SendData(string ip, int port, byte[] data)
        {
            if (data == null || data.Length == 0) return;
            this.SendData(new UdpSendMessageData()
            {
                data = data,
                ip = ip,
                port = port,
            });
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="message"></param>
        public void SendData(UdpSendMessageData message)
        {
            _send_query.Enqueue(message);
            ProcessSend();
        }

        private void ProcessSend()
        {
            if (Interlocked.CompareExchange(ref _isSend, 1, 0) != 0) return;
            UdpSendMessageData o;
            while (_send_query.TryDequeue(out o))
            {
                var data = o.data;
                if (data == null || data.Length <= 0)
                {
                    continue;
                }
                IPAddress ipadress;
                if (!IPAddress.TryParse(o.ip, out ipadress))
                {
                    continue;
                }

                send_arg.RemoteEndPoint = new IPEndPoint(ipadress, o.port);
                send_arg.SetBuffer(data, 0, data.Length);
                send_arg.UserToken = o;
                if (!client.SendToAsync(send_arg))
                {
                    Threading.ThreadPoolProviderManager.QueueUserWorkItem(() =>
                    {
                        Send_arg_Completed(this, send_arg);
                    });
                }
                return;
            }
            EndSend();
            if (!_send_query.IsEmpty)
            {
                ProcessSend();
            }
        }
        private void EndSend()
        {
            Interlocked.CompareExchange(ref _isSend, 0, 1);
        }
        /// <summary>
        /// 通知数据发送完成通知事件
        /// </summary>
        public EventHandle<UdpClient, UdpSendCompleteEventArg> UdpSendComplete = (s, e) => { };

    }


    #endregion

    /// <summary>
    /// 发送的消息对象数据
    /// </summary>
    public class UdpSendMessageData
    {
        /// <summary>
        /// 要将数据发送到的IP地址
        /// </summary>
        public string ip;
        /// <summary>
        /// 端口
        /// </summary>
        public int port;
        /// <summary>
        /// 指示需要发送的数据
        /// </summary>
        public byte[] data;
    }
    /// <summary>
    /// Udp客户端吧数据发送完成的通知事件参数信息
    /// </summary>
    public class UdpSendCompleteEventArg : System.EventArgs
    {
        /// <summary>
        /// 发送的消息
        /// </summary>
        public UdpSendMessageData Message { get; internal set; }
        /// <summary>
        /// 指示在请求过程指示的操作结果枚举
        /// </summary>
        public SocketError SocketError { get; internal set; }
    }
}