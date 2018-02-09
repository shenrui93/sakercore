/***************************************************************************
 * 
 * 创建时间：   2016/4/11 20:22:01
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供websocket数据传输操作的杂项方法
 * 
 * *************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using static SakerCore.Security.Encryption;
using static SakerCore.IO.FileHelper;
using SakerCore.Tools;

using static SakerCore.SystemErrorProvide;
using SakerCore.Web;
using SakerCore.IO;

namespace SakerCore.WebSocketServer
{
    #region WebSocket_Handle 消息函数的处理基类

    /// <summary>
    /// 提供websocket数据传输操作的杂项方法
    /// </summary>
    class WebSocket_Handle : IDisposable
    {
        private static string AppendServerHeader = "";
        static WebSocket_Handle()
        {
            AppendServerHeader = $@"Server: WSS/{Environment.MachineName}
";
        }


        //表示 WebSocket 操作的会话信息对象
        protected WebSocketSession webSocketSession;
        protected NetworkStream NStream;
        protected StreamReader Headreader;
        protected StringBuilder Headinfo;
        protected NetworkStream Buffer;
        private long _isDisposed;
        internal IWebParamData _header = WebParamData.Empty;
        /// <summary>
        /// 该请求的请求头
        /// </summary>
        public IWebParamData Header => _header;


        /// <summary>
        /// 服务器接口版本号
        /// </summary>
        private const int MaxServerVer = 13;

        /// <summary>
        /// 服务器接收客户端连接请求的响应头格式化文本
        /// </summary>
        const string AcceptRequest =
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            "Sec-WebSocket-Accept: {0}\r\n" +
            "{1}";

        /// <summary>
        /// 表示当前 WebSocket 通讯回话对象
        /// </summary>
        public WebSocketSession WebSocketSession => webSocketSession;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="webSocketSession"></param>
        public WebSocket_Handle(WebSocketSession webSocketSession)
        {
            this.webSocketSession = webSocketSession;
            //执行初始化操作
            this.Initializer();
        }

        public WebSocket_Handle()
        {

        }

        /// <summary>
        /// 对象初始化操作
        /// </summary>
        private void Initializer()
        {
            NStream = new NetworkStream();
            Headinfo = new StringBuilder();
            Headreader = new StreamReader(NStream, Encoding.UTF8);
        }

        /// <summary>
        /// 数据接收完成的处理函数
        /// </summary>
        /// <param name="buffer">数据的缓冲区</param>
        /// <param name="offset">数据偏移量</param>
        /// <param name="count">接收数据的数量</param>
        public virtual void ProcessReceive(byte[] buffer, int offset, int count)
        {
            NStream.Write(buffer, offset, count);
            ProcessReceiveCompletedHandle();
        }
        protected virtual byte[] PacketResponseData(WebMessageData webMessageData)
        {
            return new byte[0];
        }


        private void ReadHeaderEnd(string data)
        {
            HttpRequestMatch match;
            if (!HttpRequestMatch.TryParse(data, out match))
            {
                ErrorRequest();
                return;
            }
            var head = match.Header;
            this._header = head;

            string accept;
            int version;
            var response = "";
            if (!CheckHederData(head, out accept, out version))
            {
                ErrorRequest();
                return;
            }
            switch (version)
            {
                case 13:
                    {
                        #region Version:13

                        var additionalFields = string.Empty;
                        var protocol = head["Sec-WebSocket-Protocol"];
                        if (!string.IsNullOrEmpty(protocol))
                        {
                            additionalFields += "Sec-WebSocket-Protocol: " + protocol + DefaultValueManager.NewLine;
                        }
                        //追加一条服务器信息
                        additionalFields += AppendServerHeader;


                        additionalFields += DefaultValueManager.NewLine;
                        response = string.Format(AcceptRequest, accept, additionalFields);

                        var ip = head["X-Forwarded"];
                        if (string.IsNullOrEmpty(ip))
                        {
                            ip = head["X-Real-IP"];
                        }
                        if (!string.IsNullOrEmpty(ip))
                        {
                            this.webSocketSession?.AcceptSocket?.SetRemoteIP(ip);
                        }

                        this.webSocketSession.SetHandle(new WebSocketHandleProcessV13(this.webSocketSession)
                        {
                            _header = _header
                        });
                        WebSocketSession.SendDataToClient(response.GetUtf8Bytes());

                        #endregion
                        this.webSocketSession.OnConnect(DefaultValueManager.EmptyMethod);
                        return;
                    }
                default:
                    {
                        //服务器暂时不支持其他协议版本的websocket
                        response = "HTTP/1.1 401" + DefaultValueManager.NewLine + "Sec-WebSocket-Version:13";
                        WebSocketSession.SendDataToClient(response.GetUtf8Bytes());
                        return;

                    }
            }

        }

        public virtual void Ping()
        {
            this.SendData(WebSocketOpcode.Ping);
        }

        private void ProcessReceiveCompletedHandle()
        {
            bool isn = false;
            int charcode;
            while ((charcode = NStream.ReadByte()) >= 0)
            {
                var ch = (char)charcode;
                Headinfo.Append((char)charcode);
                if (ch == '\n')
                {
                    isn = true;
                    continue;
                }
                if (isn)
                {
                    if (ch == '\r')
                    {
                        NStream.ReadByte();
                        Threading.ThreadPoolProviderManager.QueueUserWorkItem(() =>
                        {
                            try
                            {
                                ReadHeaderEnd(Headinfo.ToString());
                            }
                            catch (System.Exception ex)
                            {
                                OnSystemErrorHandleEventTrace(this, ex);
                                this.Dispose();
                            }
                        });
                        return;
                    }
                    else
                        isn = false;
                }
            }
        }

        private bool CheckHederData(IWebParamData s, out string accept, out int version)
        {
            accept = "";
            version = 0;


            var upgrade = (s["Upgrade"] + "").Replace(" ", "");
            if (upgrade.IndexOf("websocket", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }

            var connection = (s["Connection"] + "").Replace(" ", "");
            if (connection.IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }

            var secWebSocketVersion = (s["Sec-WebSocket-Version"] + "").Replace(" ", "");
            version = GetVersionList(secWebSocketVersion);


            switch (version)
            {
                case 13:
                    break;
                default:
                    {
                        //版本错误 
                        return false;
                    }
            }

            var secWebSocketKey = s["Sec-WebSocket-Key"] + "";

            //检查webSocket的请求头
            if (string.IsNullOrEmpty(secWebSocketKey))
            {
                return false;
            }
            var keyData = ToBase64Bytes(secWebSocketKey);
            if (keyData.Length != 16)
            {
                return false;
            }

            accept = Sha1Base64String(secWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
            return true;
        }
        private void ErrorRequest()
        {
            var body = "403 Bad Request";
            var bytesCount = Encoding.UTF8.GetByteCount(body);

            var response = $@"HTTP/1.1 403
Connect: Close
Content-Length: {bytesCount}
{AppendServerHeader}
{body}";
            WebSocketSession.SendDataToClient(response.GetAsciiBytes());
        }
        private int GetVersionList(string secWebSocketVersion)
        {
            return secWebSocketVersion.Split(',').Select(p =>
            {
                int v;
                int.TryParse(p, out v);
                return v;
            })
             .Where(p => p <= MaxServerVer).Max();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;

            this.webSocketSession?.Dispose();
            this.Headreader?.Dispose();
            this.NStream?.Dispose();
        }
        public void SendData(WebSocketOpcode opcode, byte[] data)
        {
            if (Interlocked.Read(ref _isDisposed) != 0) return;
            if (data == null || data.Length == 0) return;
#if ADDMSGID
            var msgid = Interlocked.Increment(ref _curMsgId);
#endif
            SendData(new WebMessageData()
            {
#if ADDMSGID
                MessageId = (byte)msgid,
#endif
                Data = data,
                OpCode = opcode
            });
        }
        public void SendData(WebSocketOpcode opcode, byte[] buffer, int offset, int count)
        {
            if (buffer.Length == count && offset == 0)
            {
                SendData(opcode, buffer);
                return;
            }

            var data = new byte[count];
            System.Buffer.BlockCopy(buffer, offset, data, 0, count);
            SendData(opcode, data);

        }
        public void SendData(WebSocketOpcode opcode)
        {
            SendData(new WebMessageData() { OpCode = opcode });
        }
        public void SendData(WebMessageData msg)
        {
            var send_data = PacketResponseData(msg);
            this.webSocketSession?.SendDataToClient(send_data);
        }

    }



    #endregion


    /// <summary>
    /// 
    /// </summary>
    internal static class WebsocketDebug
    {

        static readonly MessageQueue<string> Logic = new SakerCore.Tools.MessageQueue<string>();
        static WebsocketDebug()
        {
            Logic.MessageComing = _logic_MessageComing;
        }

        private static void _logic_MessageComing(IMessageQueue<string> sender, MessageEventArgs<string> e)
        {
            var message = e.Message;
            if (string.IsNullOrEmpty(message)) return;
            var now = DateTime.Now;
            AppendAllText($"{ProcessBaseDir}/Log/websocket/{now:yyyy/MM/dd/HH}/debug.log"
                , $@"【{now:HH:mm:ss}】{message}
");
        }
        //[System.Diagnostics.Conditional("DEBUG")]
        public static void WriteDebugLog(string msg)
        {
            //将日志消息处理添加到
            Logic.Enqueue(msg);
        }
    }
}
