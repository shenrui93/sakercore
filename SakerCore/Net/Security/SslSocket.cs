/***************************************************************************
 * 
 * 创建时间：   2016/12/6 11:32:17
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供一个安全的通讯Socket
 * 
 * *************************************************************************/

using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using static SakerCore.Net.SocketConstManager;

namespace SakerCore.Net.Security
{
    /// <summary>
    /// 提供一个安全的通讯Socket
    /// </summary>
    internal sealed class SslAppectSocket : IPipelineSocket
    {
        private Socket baseSocket;
        private SslStream sslStream;
        private readonly int _sessionID;
        private IO.NetworkStream sendBuffer = new IO.NetworkStream();

        /*状态同步变量*/
        private int _isSend = 0;
        private int _isReceive = 0;
        private int _isDisposed = 0;

        /// <summary>
        /// 表示当前的连接是否已经成功完成验证操作
        /// </summary>
        private bool _isAuthenticate = false;
        BufferReceiveEventArgs receive_e;
        private readonly byte[] byte_buffer;




        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseSocket"></param>
        public SslAppectSocket(Socket baseSocket)
        {
            this.baseSocket = baseSocket;
            _sessionID = this.baseSocket.Handle.ToInt32();
            this.sslStream = new SslStream(new SocketNetworkStream(this.baseSocket));
            byte_buffer = new byte[BufferCount];
            receive_e = new BufferReceiveEventArgs(byte_buffer);

        }

        //基本对象初始化
        private void Init()
        {

            this.baseSocket.SendTimeout = 100;

            this.baseSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            this.baseSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            this.baseSocket.UseOnlyOverlappedIO = false;

#if !Unity && KeepAlive
            //设置 KeepAlive 时间
            int dummySize = Marshal.SizeOf((uint)0);
            var inOptionValues = new byte[dummySize * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)15000).CopyTo(inOptionValues, dummySize);
            BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, dummySize * 2);

            this.baseSocket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
#endif
        }



        /// <summary>
        /// 验证数据连接
        /// </summary>
        public void BeginAuthenticateAsServer(X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
        {
            try
            {
                this.sslStream.BeginAuthenticateAsServer(serverCertificate, asyncCallback, asyncState);
            }
            catch (System.Exception)
            {
                this.Dispose();
            }
        }
        /// <summary>
        /// 完成通讯连接的验证
        /// </summary>
        /// <param name="iar"></param>
        public void EndAuthenticateAsServer(IAsyncResult iar)
        {
            this.sslStream.EndAuthenticateAsServer(iar);
            _isAuthenticate = true;
        }


        /// <summary>
        /// 
        /// </summary>
        public int BufferSize => 1024 * 4;
        /// <summary>
        /// 
        /// </summary>
        public bool Connected => baseSocket?.Connected == true;
        /// <summary>
        /// 
        /// </summary>
        public EventHandler Disposed { get; set; } = (sender, e) => { };
        /// <summary>
        /// 
        /// </summary>
        public int Port => ((IPEndPoint)this.baseSocket.RemoteEndPoint).Port;
        /// <summary>
        /// 
        /// </summary>
        public EventHandler<BufferReceiveEventArgs> ReceiveCompleted { get; set; } = (sender, e) => { };
        /// <summary>
        /// 
        /// </summary>
        public EventHandler<EventArgs> SendCompleted { get; set; } = (sender, e) => { };
        /// <summary>
        /// 
        /// </summary>
        public int SessionID => _sessionID;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reuseSocket"></param>
        public void Close(bool reuseSocket)
        {
            this.Dispose();
        }

        #region 不受支持的方法

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string ip, int port)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="iaCallBack"></param>
        public void BeginConnect(string ip, int port, Action<bool> iaCallBack)
        {
            throw new NotSupportedException();
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            sslStream?.Dispose();
            this.baseSocket?.Close();
            Disposed(this, EventArgs.Empty);

        }

        #region 运行时修改IP地址的支持

        private string RemoteIP = null;

        /// <summary>
        /// 
        /// </summary>
        public string IPAdress
        {
            get
            {
                try
                {
                    if (RemoteIP != null) return RemoteIP;
                    return ((IPEndPoint)this.baseSocket.RemoteEndPoint).Address.ToString();
                }
                catch
                {
                    return "";
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public uint IPAdressUInt32
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
        /// 
        /// </summary>
        public bool IsSetRemoteIP => RemoteIP != null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        public void SetRemoteIP(string ip)
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

            this.RemoteIP = ip;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool StartReceive()
        {
            if (!_isAuthenticate)
            {
                //数据的收发必须建立在安全的套接字连接上
                throw new System.Exception("数据的收发必须建立在安全的套接字连接上");
            }

            ProcessReceive();

            return true;


        }

        #region 接收数据

        private void ProcessReceive()
        {
            if (Interlocked.CompareExchange(ref _isReceive, 1, 0) != 0) return;
            try
            {
                this.sslStream.BeginRead(byte_buffer, 0, BufferCount, this.ReceiveCallback, null);
            }
            catch (System.Exception)
            {
                this.Dispose();
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!ar.IsCompleted)
            {
                return;
            }
            Receive_end();
            try
            {
                int count = this.sslStream.EndRead(ar);
                if (count <= 0)
                {
                    this.Dispose();
                    return;
                }
                receive_e.Count = count;
                receive_e.Offset = 0;
                ReceiveCompleted(this, receive_e);
                ProcessReceive();
            }
            catch
            {
                this.Dispose();
            }
        }
        private void Receive_end()
        {
            Interlocked.CompareExchange(ref _isReceive, 0, 1);
        }


        #endregion

        #region 发送数据

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
            sendBuffer.Write(buffer, offset, count);
            ProcessSend();
        }

        private void ProcessSend()
        {
            if (Interlocked.CompareExchange(ref _isSend, 1, 0) != 0) return;
            try
            {

                var count = sendBuffer.Count;
                count = count >= 1024 ? 1024 : count;
                var data = sendBuffer.ReadAndRemoveBytes(count);
                this.sslStream.BeginWrite(data, 0, count, this.SendCallback, null);
            }
            catch (System.Exception)
            {
                this.Dispose();
            }

        }
        private void SendCallback(IAsyncResult ar)
        {
            if (!ar.IsCompleted) return;
            try
            {
                this.sslStream.EndWrite(ar);
                SendCompleted(this, EventArgs.Empty);
            }
            catch (System.Exception)
            {
                this.Dispose();
            }
            finally
            {
                Send_end();
            }
            if (sendBuffer.Count > 0)
            {
                ProcessSend();
            }
        }
        private void Send_end()
        {
            Interlocked.CompareExchange(ref _isSend, 0, 1);
        }


        #endregion


        public Socket BaseSocket => this.baseSocket;

    }
}
