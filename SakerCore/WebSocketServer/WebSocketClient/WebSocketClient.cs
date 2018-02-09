
#define ADDMSGID

/***************************************************************************
 *
 * 创建时间：   2016/4/23 11:27:51
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

using System;
using System.Text;
using SakerCore.Net;
using SakerCore.WebSocketServer;
using SakerCore.Serialization.BigEndian;
using System.Threading;
using System.Linq;
using SakerCore.IO;

namespace SakerCore.WebSocketClient
{
    /// <summary>
    /// 表示一个 WebSocket 客户端对象
    /// </summary>
    public class WebSocketClient : IDisposable
    {
        /*
        User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0
            */

        //        /// <summary>
        //        /// 请求头,WebSocket 非标准基础请求头样本
        //        /// </summary>
        //        public const string RequestHeader = @"GET {0} HTTP/1.1
        //Host: {1}
        //Upgrade: websocket
        //Connection: Upgrade
        //Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==
        //Sec-WebSocket-Protocol: chat, superchat
        //Sec-WebSocket-Version: 13
        //Param: UyiTestClient
        //User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) UyiTestClient/1.2


        //";
        /// <summary>
        /// 请求头,WebSocket 非标准基础请求头样本
        /// </summary>
        public const string RequestHeader = @"GET {0} HTTP/1.1
Host: {1}
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
Accept-Language: zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3
Accept-Encoding: gzip, deflate
Sec-WebSocket-Version: 13
Origin: {2}
Sec-WebSocket-Extensions: permessage-deflate
Sec-WebSocket-Key: 3KlQ+zrAZaDiliQ+UScY+Q==
Connection: keep-alive, Upgrade
Pragma: no-cache
Cache-Control: no-cache
Upgrade: websocket

";


        /// <summary>
        /// 实例化基础的Tcp通讯管道
        /// </summary>
        public Net.Pipeline.PipelineSocket WSocket = new Net.Pipeline.PipelineSocket(1024);

        void Connect(string ip, int port, string path = "/", string host = "127.0.0.1", string orgion = "http://127.0.0.1")
        {
            WSocket.ReceiveCompleted = ReceiveCompletedCallBack;
            WSocket.Disposed = DisposedCallBack;
            WSocket.BeginConnect(ip, port, (result) =>
            {
                if (!result) Dispose();
                OnConnecting(this, null);
                WSocket.StartReceive();

                var requestHeader = string.Format(RequestHeader, path, host, orgion);
                //发送模拟的客户端请求
                WSocket.Send(Encoding.ASCII.GetBytes(requestHeader));
            });
        }

        /// <summary>
        /// 连接到远端的服务器地址
        /// </summary>
        /// <param name="url">远端的Ip</param> 
        public void ConnectByUrl(string url)
        {
            if (url.StartsWith("ws://", StringComparison.OrdinalIgnoreCase))
            {
                url = url.Replace("ws://", "http://");
            }
            else if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                throw new InvalidCastException("指定了一个无效的Url地址");
            }


            var host = uri.Host;
            var port = uri.Port;
            var path = uri.PathAndQuery;
            var origin = uri.OriginalString;

            var ip = System.Net.Dns.GetHostAddresses(host).FirstOrDefault();

            if (ip == null) throw new System.Exception($"无法解析主机:{host}");

            this.Connect(ip.ToString(), port, path, host, origin);


        }

        /// <summary>
        /// 指示当前的对象是否已经被释放
        /// </summary>
        private long _isDisposed;


        private void DisposedCallBack(object sender, EventArgs e)
        {
            this.CurrentSession.Dispose();
        }
        private void ReceiveCompletedCallBack(object sender, BufferReceiveEventArgs e)
        {
            var data = new byte[e.Count];
            Buffer.BlockCopy(e.Buffer, e.Offset, data, 0, e.Count);
            this.CurrentSession.ProcessReceiveCompletedCallBack(data);
        }




        /***********事件注册模块********/
        /// <summary>
        /// 
        /// </summary>
        public EventHandle OnConnecting = (s, e) => { };
        /// <summary>
        /// 
        /// </summary>
        public EventHandle<IWebSocketSession> OnConnected = (s, e) => { };
        /// <summary>
        /// 
        /// </summary>
        public EventHandle OnDisposed = (s, e) => { };
        /// <summary>
        /// 当前连接的客户端Ip地址
        /// </summary>
        public string ClientAddress => this.WSocket.IPAdress;
        /// <summary>
        /// 档案连接的会话对象信息
        /// </summary>
        public WebClientSession CurrentSession { get; }
        /// <summary>
        /// 连接的会话编号
        /// </summary>
        public int SessionID => WSocket.SessionID;


        /// <summary>
        /// 
        /// </summary>
        public WebSocketClient()
        {
            CurrentSession = new WebClientSession(this);
        }
        private void Send(byte[] sendData)
        {
            this.WSocket?.Send(sendData);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            this.WSocket?.Dispose();
            this.OnDisposed(this, EventArgs.Empty);
        }


        #region WebSocket_Handle

        /// <summary>
        /// 
        /// </summary>
        private class WebSocketHandle : IDisposable
        {
            private readonly NetworkStream _stream;
            private readonly StringBuilder _headinfo;
            private readonly WebClientSession _session;
            private System.Action _processReceive;
            private NetworkStream _nStream = new NetworkStream();
            private static readonly byte[] Mark = { 1, 2, 3, 4 };

            public void Dispose()
            {
            }
            public WebSocketHandle(WebClientSession session)
            {
                this._session = session;
                _stream = new NetworkStream();
                _headinfo = new StringBuilder();
                _processReceive = ReadHeader;
            }

            private void ReadHeader()
            {
                bool isn = false;
                int charcode;
                while ((charcode = _stream.ReadByte()) >= 0)
                {
                    var ch = (char)charcode;
                    _headinfo.Append((char)charcode);
                    if (ch == '\n')
                    {
                        isn = true;
                        continue;
                    }
                    if (isn)
                    {
                        if (ch == '\r')
                        {
                            _stream.ReadByte();
                            ReadHeaderEnd();
                            return;
                        }
                        else
                            isn = false;
                    }
                }
            }
            private void ReadHeaderEnd()
            {
                var response_headrer = _headinfo.ToString();



                _session.OnConnect();
                _processReceive = ProcessReceive_13;
                _processReceive();
            }
            private void ProcessReceive_13()
            {
                var buffer = _stream;

                WebSocketOpcode opcode;
                while (true)
                {
                    var code = PacketData(ref buffer, ref _nStream, out opcode);

                     
                    switch (code)
                    {
                        case ParsePacketInternalCode.HasNextData:
                        case ParsePacketInternalCode.NotAllData:
                            return;
                        case ParsePacketInternalCode.Success:
                            {
                                switch (opcode)
                                {
                                    case WebSocketOpcode.Text:
                                        break;
                                    case WebSocketOpcode.Binary:
                                        {
                                            var data = _nStream.ToArray();
                                            _session?.OnMessageComing(new WebSocketSessionMessageComingArg(data)
                                            {
                                                Opcode = opcode,
                                                Count = data.Length,
                                                Offset = 0
                                            });
                                            break;
                                        }
                                    case WebSocketOpcode.Ping:
                                        {
                                            SendData(WebSocketOpcode.Pong);
                                            break;
                                        }
                                    case WebSocketOpcode.Pong:
                                        {
                                            continue;
                                        }
                                    case WebSocketOpcode.Close:
                                        {
                                            SendData(WebSocketOpcode.Close);
                                            this.Dispose();
                                            return;
                                        }
                                }

                                continue;
                            }
                        default:
                            {
                                SendData(WebSocketOpcode.Close);
                                this.Dispose();
                                return;
                            }
                    }
                }


            }
            private void SendData(WebSocketOpcode opcode)
            { 
                var sendData = PacketResponseData(new WebMessageData() { Data = new byte[1], OpCode = opcode });
                _session?.SendDataToServer(sendData);
            }


            public static byte[] PacketResponseData(WebMessageData webData)
            {
                using (var stream = new NetworkStream())
                {
                    var data = webData.Data;

                    byte b = 0x80;
                    b = (byte)(b | (byte)webData.OpCode);
                    stream.WriteByte(b);

                    b = 0x80;
                    var len = data.Length;
                    if (len < 126)
                    {
                        b = (byte)(b | len);
                        stream.WriteByte(b);
                    }
                    else if (len < 65536)
                    {
                        b = (byte)(b | 126);
                        stream.WriteByte(b);
                        BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (ushort)len);

                    }
                    else
                    {
                        b = (byte)(b | 127);
                        stream.WriteByte(b);
                        BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (ulong)len);
                    }

                    //写入掩码
                    stream.Write(Mark, 0, 4);

                    for (var index = 0; index < len; index++)
                    {
                        data[index] = (byte)(data[index] ^ Mark[index % 4]);
                    }

                    //写入负载数据
                    stream.Write(data, 0, len);
                    return stream.ToArray();
                }

            }
            public static ParsePacketInternalCode PacketData(ref NetworkStream buffer, ref NetworkStream nStream, out WebSocketOpcode opCode)
            {
                opCode = WebSocketOpcode.Unkonown;
                if (buffer.Count < 2) return ParsePacketInternalCode.NotAllData;
                int readPos = 0;

                byte b = buffer[readPos];
                var isEof = b >> 7;             //是否是数据的结束帧
                //var rsv1 = (b >> 6) & 0x01;
                //var rsv2 = (b >> 5) & 0x01;
                //var rsv3 = (b >> 4) & 0x01;

                //if (rsv1 != 0 || rsv2 != 0 || rsv3 != 0)
                //{
                //    return ParsePacketInternalCode.ErrorData;
                //}
                opCode = (WebSocketOpcode)((b) & 0xF);  //Opcode

                readPos = 1;
                b = buffer[readPos];
                var mask = b >> 7;              //掩码值
                if (mask != 0)
                {
                    //掩码值解析错误
                    return ParsePacketInternalCode.ErrorData;
                }
                var dataLen = (b & 0x7F);
                readPos = 2;
                switch (dataLen)
                {
                    case 126:
                        {
                            if (buffer.Count < readPos + 2) return ParsePacketInternalCode.NotAllData;

                            //16位长度字节
                            var data = buffer.ReadArray(readPos, 2);

                            ushort t;
                            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(data, out t);
                            dataLen = t;

                            readPos += 2;

                        }
                        break;
                    case 127:
                        {
                            if (buffer.Count < readPos + 8) return ParsePacketInternalCode.NotAllData;
                            //64位长度字节
                            var data = buffer.ReadArray(readPos, 8);

                            ulong t;
                            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(data, out t);
                            dataLen = (int)t;
                            //64位字节数据长度 
                            readPos += 8;
                        }
                        break;
                }

                var allLen = readPos + dataLen; //+ 4;

                if (buffer.Count < allLen)
                    return ParsePacketInternalCode.NotAllData;


                var payData = buffer.ReadArray(readPos, dataLen);


                nStream.Write(payData, 0, dataLen);
                //删除数据
                buffer.Remove(allLen);
                return isEof == 1 ? ParsePacketInternalCode.Success : ParsePacketInternalCode.HasNextData;
            }






            internal void ProcessReceiveCompletedCallBack(byte[] buffer)
            {
                _stream.Write(buffer);
                _processReceive();
            }
        }

        #endregion

        #region WebClientSession

        /// <summary>
        /// 表示一个客户端的连接通讯会话对象
        /// </summary>
        public class WebClientSession : IWebSocketSession, IDisposable
        {
            readonly WebSocketHandle _handle;
            private readonly WebSocketClient _webSocketClient;
            long _isDisposed;
            /// <summary>
            /// 初始化新实例
            /// </summary>
            /// <param name="webSocketClient"></param>
            public WebClientSession(WebSocketClient webSocketClient)
            {
                _webSocketClient = webSocketClient;
                _handle = new WebSocketHandle(this);
            }
            /// <summary>
            /// 
            /// </summary>
            public string ClientAddress => this._webSocketClient.ClientAddress;
            /// <summary>
            /// 
            /// </summary>
            public int SessionId => this._webSocketClient.SessionID;

            /// <summary>
            /// 数据消息到达时的通知
            /// </summary>
            public EventHandler<WebSocketSessionMessageComingArg> MessageComing { get; set; } = (s, e) => { };
            /// <summary>
            /// 连接关闭时的回调通知
            /// </summary>
            public EventHandler WebSocketSessionClose { get; set; } = (s, e) => { };
            /// <summary>
            /// 获取当前连接的基础套接字对象
            /// </summary>
            public IPipelineSocket BasePipelineSocket => this._webSocketClient?.WSocket;

            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
                _webSocketClient?.Dispose();
                WebSocketSessionClose(this, EventArgs.Empty);
            }
            public void Send(byte[] data)
            {
                Send(new WebMessageData() { Data = data, OpCode = WebSocketOpcode.Binary });
            }
            public void Send(string message)
            {
                Send(new WebMessageData() { Data = Encoding.UTF8.GetBytes(message), OpCode = WebSocketOpcode.Text });
            }
            void Send(WebMessageData message)
            {
                var buffer = WebSocketHandle.PacketResponseData(message);
                SendDataToServer(buffer);
            }


            public void ProcessReceiveCompletedCallBack(byte[] buffer)
            {
                //数据接收处理程序
                _handle.ProcessReceiveCompletedCallBack(buffer);
            }

            internal void SendDataToServer(byte[] sendData)
            {
                _webSocketClient.Send(sendData);
            }
            internal void OnMessageComing(WebSocketSessionMessageComingArg e)
            {
                MessageComing(this, e);
            }
            internal void OnConnect()
            {
                _webSocketClient.OnConnected(_webSocketClient, this);
            }

            /// <summary>
            /// 向对端发送一条Ping指令
            /// </summary>
            public virtual void Ping()
            {
                this.Send(PingBuffer);
            }

            public void Send(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            static readonly byte[] PingBuffer = WebSocketHandle.PacketResponseData(WebMessageData.Ping);
        }

        #endregion

    }







}
