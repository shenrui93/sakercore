using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SakerCore.Net
{
    /// <summary>
    /// 
    /// </summary>
    public class AcceptedEventArgs : EventArgs
    {

        /// <summary>
        /// 客户端通讯IPipelineSocket
        /// </summary>
        public IPipelineSocket AcceptSocket;

        /// <summary>
        /// 创建一个新实例
        /// </summary>
        /// <param name="s"></param>
        public AcceptedEventArgs(IPipelineSocket s)
        {
            this.AcceptSocket = s;
        }
    }
     
}
