using System;
using System.Collections.Generic;
using System.Text;

namespace SakerCore.Net
{
    /// <summary>
    /// 通讯框架数据统计管理器
    /// </summary>
    public static class StatisticsManage
    {
        private static long totalReceiveBytes;
        private static long totalSendBytes;
        private static long totalReceiveMessages;
        private static long totalSendMessages;

        /// <summary>
        /// 通讯框架总共接收消息量
        /// </summary>
        public static long TotalReceiveMessages
        {
            get { return StatisticsManage.totalReceiveMessages; }
        }

        /// <summary>
        /// 通讯框架总共发送消息量
        /// </summary>
        public static long TotalSendMessages
        {
            get { return StatisticsManage.totalSendMessages; }
        }

        /// <summary>
        /// 通讯框架总发送数据字节量
        /// </summary>
        public static long TotalSendBytes
        {
            get { return StatisticsManage.totalSendBytes; }
        }

        /// <summary>
        /// 通讯框架总接受数据字节量
        /// </summary>
        public static long TotalReceiveBytes
        {
            get { return StatisticsManage.totalReceiveBytes; }
        }



        /// <summary>
        /// 增加消息接收字节量
        /// </summary>
        /// <param name="count"></param>
        public static void AddReceiveBytes(long count)
        {
            System.Threading.Interlocked.Add(ref totalReceiveBytes, count);
        }

        /// <summary>
        /// 增加消息发送字节量
        /// </summary>
        /// <param name="count"></param>
        public static void AddSendBytes(long count)
        {
            System.Threading.Interlocked.Add(ref totalSendBytes, count);
        }

        /// <summary>
        /// 递增发送消息量
        /// </summary>
        public static void AddSendMessages()
        {
            System.Threading.Interlocked.Increment(ref totalSendMessages);
        }

        /// <summary>
        /// 递增接收消息量
        /// </summary>
        public static void AddReceiveMessages()
        {
            System.Threading.Interlocked.Increment(ref totalReceiveMessages);
        }

        /// <summary>
        /// 重置计数器
        /// </summary>
        public static void SetToZero()
        {
            totalReceiveBytes = totalSendBytes = totalReceiveMessages = totalSendMessages = 0;
        }
    }
}
