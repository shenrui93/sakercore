/***************************************************************************
 * 
 * 创建时间：   2016/4/11 19:22:50
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/


namespace SakerCore.WebSocketServer
{
    /// <summary>
    /// 类WebMessageData的注释信息
    /// </summary>
    public class WebMessageData
    {
        /// <summary>
        /// 消息编号
        /// </summary>
        public byte MessageId = 0;
        /// <summary>
        /// 欲发送的数据
        /// </summary>
        public byte[] Data = { };
        /// <summary>
        /// 
        /// </summary>
        public WebSocketOpcode OpCode;

        /// <summary>
        /// 
        /// </summary>
        public static readonly WebMessageData Ping = new WebMessageData()
        {
            OpCode = WebSocketOpcode.Ping
        };
    }
}
