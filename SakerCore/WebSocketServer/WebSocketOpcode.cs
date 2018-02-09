/***************************************************************************
 * 
 * 创建时间：   2016/4/11 17:36:49
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   为 WEBSOCKET 传输数据操作定义枚举类型
 * 
 * *************************************************************************/


namespace SakerCore.WebSocketServer
{
    /// <summary>
    /// 表示 WebSocketOpcode 操作类型值
    /// </summary>
    public enum WebSocketOpcode
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unkonown = 0xFF,
        /// <summary>
        /// 代表一个继续帧
        /// </summary>
        Go = 0x0,//代表一个继续帧
        /// <summary>
        /// 代表一个文本帧
        /// </summary>
        Text = 0x1,// 代表一个文本帧
        /// <summary>
        /// 代表一个二进制帧
        /// </summary>
        Binary = 0x2,//代表一个二进制帧 
        /// <summary>
        /// 代表连接关闭
        /// </summary>
        Close = 0x8,//代表连接关闭
        /// <summary>
        /// 代表ping
        /// </summary>
        Ping = 0x9,//代表ping
        /// <summary>
        /// 代表pong
        /// </summary>
        Pong = 0xA,//代表pong 

    }
    /// <summary>
    /// 数据解析操作结果数据
    /// </summary>
    internal enum ParsePacketInternalCode
    {

        /// <summary>
        /// 操作成功
        /// </summary>
        Success = 0xF0,
        /// <summary>
        /// 还有后续数据
        /// </summary>
        HasNextData = 0xF1,
        /// <summary>
        /// 错误数据
        /// </summary>
        ErrorData = 0xF2,
        /// <summary>
        /// 数据分片不完整
        /// </summary>
        NotAllData = 0xF3,
    }

}
