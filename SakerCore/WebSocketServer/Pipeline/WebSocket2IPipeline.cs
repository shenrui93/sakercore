/***************************************************************************
 * 
 * 创建时间：   2017/4/7 12:58:57
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   完成一个webSocket到平台管理通讯管道 IPipelineSocket 的包装器
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SakerCore.Net;

namespace SakerCore.WebSocketServer.Pipeline
{ 

    /// <summary>
    /// 完成一个webSocket到平台管理通讯管道 IPipelineSocket 的包装器
    /// </summary>
    public class WebSocket2IPipeline : IPipelineSocket
    {

        public System.Net.Sockets.Socket BaseSocket
        {
            get
            {
                throw new NotSupportedException();
            }
        }
        long _isDisposed = 0;
        IWebSocketSession session;
        BufferReceiveEventArgs receive_e = new BufferReceiveEventArgs(null);

        /// <summary>
        /// 完成一个webSocket到平台管理通讯管道 Socket 的包装器
        /// </summary>
        /// <param name="session"></param>
        public WebSocket2IPipeline(IWebSocketSession session)
        {
            this.session = session;
            session.MessageComing = Session_MessageComing;
            session.WebSocketSessionClose = Session_WebSocketSessionClose;
        }

        private void Session_WebSocketSessionClose(object sender, EventArgs e)
        {
            this.Dispose();
        }
        private void Session_MessageComing(object sender, WebSocketSessionMessageComingArg e)
        {
            if (e.Opcode == WebSocketOpcode.Binary)
            {
                receive_e.SetBuffer(e.PayData, 0, e.PayData.Length);
                ReceiveCompleted(this, receive_e);
            }
        }
        private void OnDisposed()
        {
            Disposed(this, EventArgs.Empty);
        }
#pragma warning disable CS1591

        public int BufferSize => 1024 * 4;
        public bool Connected => this.session?.BasePipelineSocket?.Connected == true;
        public string IPAdress => this.session?.ClientAddress ?? "";
        public uint IPAdressUInt32 => this.session.BasePipelineSocket.IPAdressUInt32;
        public bool IsSetRemoteIP => this.session.BasePipelineSocket.IsSetRemoteIP;
        public int Port => this.session.BasePipelineSocket.Port;
        public int SessionID => this.session.SessionId;
        public EventHandler Disposed { get; set; } = (s, e) => { };
        public EventHandler<BufferReceiveEventArgs> ReceiveCompleted { get; set; } = (s, e) => { };
        public EventHandler<EventArgs> SendCompleted { get; set; } = (s, e) => { };


        public void BeginConnect(string ip, int port, Action<bool> iaCallBack)
        {
            throw new NotSupportedException();
        }
        public void Close(bool reuseSocket)
        {
            this.Dispose();
        }
        public bool Connect(string ip, int port)
        {
            throw new NotSupportedException();
        }
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            OnDisposed();
            this.session?.Dispose();
        }
        public void Send(byte[] buffer)
        {
            Send(buffer, 0, buffer.Length);
        }
        public void Send(byte[] buffer, int offset, int count)
        {
            this.session.Send(buffer, offset, count);
        }

        public void SetRemoteIP(string ip)
        {
            this.session.BasePipelineSocket.SetRemoteIP(ip);
        }
        public bool StartReceive()
        {
            return this.session.BasePipelineSocket.StartReceive();
        }

    }
     }

