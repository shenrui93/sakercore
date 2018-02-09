/***************************************************************************
 * 
 * 创建时间：   2016/4/11 13:04:59
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   WebSocketServer
 * 
 * *************************************************************************/

using SakerCore.Net;
using System;

namespace SakerCore.WebSocketServer
{
    /// <summary>
    /// 表示 WebSocket 的服务连接监听器
    /// </summary>
    public class WebSocketServer
    {
        /// <summary>
        ///  维持服务WebSocket连接的下层Tcp连接监听器实例
        /// </summary>
        protected IListen Listen;
        /// <summary>
        /// 实例化一个新的 <see cref="WebSocketServer"/> 实例
        /// </summary>
        public WebSocketServer(string ip, int port)
        {
            //创建一个新的Socket连接监听器，并绑定监听IP和端口
            Listen = new Net.Pipeline.SocketServer(ip, port)
            {
                IsIPv6Model = true,             //设置当前的监听器是否支持IPV6
                Accepted = Listen_Accepted      //连接监听通知方法
            };
        }
        /// <summary>
        /// 实例化一个新的 <see cref="WebSocketServer"/> 实例
        /// </summary>
        public WebSocketServer()
        { 
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void Initializer(string ip, int port)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool Start()
        {
            return Listen.StartAccept();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Listen_Accepted(object sender, AcceptedEventArgs e)
        {
            try
            { 
                var webSocketSession = new WebSocketSession(e.AcceptSocket, this);
            }
            catch (System.Exception ex)
            {
                e.AcceptSocket?.Dispose();
                SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<WebSocketServerAcceptedArgs> Accepted;

        internal void OnAccepted(WebSocketSession session)
        {
            Accepted?.Invoke(this, new WebSocketServerAcceptedArgs()
            {
                Session = session
            });
        }

        /// <summary>
        /// 停止监听服务
        /// </summary>
        public void Stop()
        {
            Listen.StopAccept();
        }

        internal void OnAccepted(WebSocketSession session, System.Action callBack)
        {
            Accepted?.Invoke(this, new WebSocketServerAcceptedArgs()
            {
                Session = session,
                Call = callBack,
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class WebSocketServerAcceptedArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public IWebSocketSession Session;
        /// <summary>
        /// 
        /// </summary>
        public System.Action Call;
    }
}
