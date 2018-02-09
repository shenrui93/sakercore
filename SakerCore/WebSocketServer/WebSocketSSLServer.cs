/***************************************************************************
 * 
 * 创建时间：   2016/12/6 11:25:38
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

using System.Security.Cryptography.X509Certificates;

namespace SakerCore.WebSocketServer
{
    /// <summary>
    /// 表示一个使用指定证书验证的安全WebSocket 服务监听器
    /// </summary>
    public class WebSocketSslServer: WebSocketServer
    {
        /// <summary>
        /// 创建一个新的 <see cref="WebSocketSslServer"/> 实例
        /// </summary>
        /// <param name="ip">绑定监听器的Ip</param>
        /// <param name="port">绑定监听的端口</param>
        /// <param name="serverCertificate">需要使用的监听器连接证书</param>
        public WebSocketSslServer(string ip,int port,X509Certificate serverCertificate)
        {
            //创建一个安全连接的服务监听器
            Listen = new Net.Security.SslSocketServer(ip, port, serverCertificate)
            {
                IsIPv6Model = false,        //指示是否支持IPV6
                Accepted = Listen_Accepted  //设置监听器的连接通知方法
            };
        } 
    }
}
