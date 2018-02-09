using System;
using System.Collections.Generic;
using System.Text;

namespace SakerCore.Net
{

    /// <summary>
    /// 表示数据接收完成事件参数数据
    /// </summary>
    public class BufferReceiveEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        public BufferReceiveEventArgs(byte[] buffer)
        {
            this.buffer = buffer;
        }

        /// <summary>
        /// 数据缓冲区数据
        /// </summary>
        public byte[] Buffer { get { return buffer; } }

        /// <summary>
        /// 接收到的数据在缓冲区中的偏移量
        /// </summary>
        public int Offset;
        /// <summary>
        /// 接收到的数据数量
        /// </summary>
        public int Count;

        private byte[] buffer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            this.buffer = buffer;
            this.Offset = offset;
            this.Count = count;
        }
    }

}
