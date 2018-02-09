
#define UseUnsafe

/********************************************* 平 台 通 信 拆 包 组 包 自 定 义 协 议 **********************************************************************************
 *
 * 序号.名称            C# 数据类型          起始位置             长度            备注
 * ------------------------------------------------------------------------------------------------------------------------------
 * 1.开始符                sbyte               0                   1              固定是 0x7E。标识通信包的开始。
 *
 * 2.包长度                ushort              1                   2              包头部分和包体部分字节数之和，不包括开始符和结束符。
 * 4.应用域                byte               3                   1              
 * 5.消息号                byte               4                   1               每个消息帧都对应一个消息号。在每个应用领域内消息号是唯一的。
 * 6.头部CRC               byte               5                   1               从开始符起至目标地址止范围内 15 个字节按异或运算进行计算。
 *
 * 7.包体                  byte[]               6                   N             变长。可变长度=有效数据长度。在本框架内采用自定义序列化与反序列化协议。
 *
 * 8.结束符                byte            Length - 1              1              固定是 0x7F。标识通信包的结束。
 * 
 ***********************************************************************************************************************************************************************/

using System;
using Uyi.Net.DataPacket;
using Uyi.Serialization;

namespace Uyi.Net.Analysis
{

    /// <summary>
    /// 以开始符和结束符分割且以一个短整型（2字节）表示包长度的固定头部的通信包编解码器
    /// </summary>
    public class PacketCodecHandlerInternal
    {  
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] Encode(TransferMessage message) => message.UPacketData.PayloadData;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] Encode(object message) => new TransferMessage(message).UPacketData.PayloadData;
        /// <summary>
        /// 解包方法
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="data"></param>
        /// <param name="needBytesPackdata"></param>
        /// <returns></returns>
        public static bool ParsePacketInternal(ref IO.NetworkStream buffer, out TransferPacket data, bool needBytesPackdata = false)
        {
            data = TransferPacket.FromDataParam(buffer, needBytesPackdata);
            var code = (byte)data.Code;

            return (code >= 0x80 && code <= 0x82);
        }

    }

}
