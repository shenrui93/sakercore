using System;

namespace Uyi.Net.Message
{
    /// <summary>
    /// 消息通知事件参数
    /// </summary>
    public class MessageComingArgs : EventArgs
    { 

        /// <summary>
        /// 实例化对象
        /// </summary>
        public MessageComingArgs()
        {
        }


        /// <summary>
        /// 通知的数据消息
        /// </summary>
        public object Message;
        /// <summary>
        /// 当前消息的负载数据
        /// </summary>
        public byte[] PackData;

        /// <summary>
        /// 应用主码
        /// </summary>
        public byte wMainCode;
        /// <summary>
        /// 应用副码
        /// </summary>
        public byte wSubCode;
        /// <summary>
        /// 消息所属的会话ID
        /// </summary>
        public int SessionID;
    }

}
