
using System;
using System.Net.Sockets;
using Uyi.Net.Analysis;
using Uyi.Net.Message;
using Uyi.Net.SocketPipeline;
using Uyi.Serialization;

namespace Uyi.Net.Pipeline
{
    /// <summary>
    /// 一个简单的单根数据管道
    /// </summary>
    public class SocketClient : Pipeline
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer_size"></param>
        public SocketClient(int buffer_size = 1024)
        {
            this.Client = new PipelineSocket(buffer_size);
        }

        private SocketClient()
        {

        }

        /// <summary>
        /// 从一个已经连接的通讯IPipelineSocket基础上创建一个客户端单向管道实例
        /// </summary>
        public static SocketClient CreateNewSocketSinglePipelineFromSocket(IPipelineSocket socket, int bufferSize)
        {
            if (socket == null || !socket.Connected)
            {
                var ex = new Exception("只有是连接成功的IPipelineSocket对象才能进行管道创建");
                //SystemRunErrorPorvider.CatchException(ex);
                throw ex;
            }
            var _pipelineSocket = socket;
            var pip = new SocketClient();
            pip.Client = _pipelineSocket;
            return pip;
        }
        /// <summary>
        /// 数据管道接收到数据时候的事件处理程序
        /// </summary>
        protected override void Client_ReceiveCompleted(object sender, BufferReceiveEventArgs e)
        {
            recbuffer.Write(e.Buffer, e.Offset, e.Count);
            PipelineOperation.OnMessageComing(this, ref recbuffer, this.OnMessageComing); 
        }
    }
}
