/***************************************************************************
 * 
 * 创建时间：   2016/4/11 19:01:39
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   表示一个WebSocket接收数据时的通知对象
 * 
 * *************************************************************************/

using System;
using System.Text;

namespace SakerCore.WebSocketServer
{
    #region WebSocketSessionMessageComingArg

    /// <summary>
    /// 表示一个WebSocket接收数据时的通知对象
    /// </summary>
    public class WebSocketSessionMessageComingArg : EventArgs
    {
        /// <summary>
        /// 实例化一个新的 <see cref="WebSocketSessionMessageComingArg"/> 实例
        /// </summary>
        /// <param name="buffer">初始化的接收的负载数据</param>
        public WebSocketSessionMessageComingArg(byte[] buffer)
        {
            //设置只读的负载数据值
            PayData = buffer;
        }

        /// <summary>
        /// 数据缓冲区
        /// </summary>
        public byte[] PayData { get; }

        /// <summary>
        /// 指示数据类别
        /// </summary>
        public WebSocketOpcode Opcode;

        /// <summary>
        /// 接收数据的偏移量
        /// </summary>
        public int Offset;
        /// <summary>
        /// 接收数据的数量
        /// </summary>
        public int Count;
        /// <summary>返回表示当前 <see cref="WebSocketSessionMessageComingArg" /> 的 <see cref="T:System.String" />。</summary>
        /// <returns>
        /// <see cref="T:System.String" />，表示当前的 <see cref="T:System.Object" />。</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        { 
            switch (Opcode)
            {
                case WebSocketOpcode.Binary:
                case WebSocketOpcode.Text:
                    {
                        //如果是数据帧直接输出字节数据所表示的字符串信息
                        return Encoding.UTF8.GetString(PayData);
                    }
                default:
                    {
                        //如果是 其他数据帧则直接输出当前数据帧的数据类别
                        return Opcode.ToString();
                    }
            }
        }
    }

    #endregion
}
