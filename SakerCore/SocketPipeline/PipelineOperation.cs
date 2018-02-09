/***************************************************************************
 * 
 * 创建时间：   2017/12/23 20:20:08
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

using System;
using Uyi.IO;
using Uyi.Net.Analysis;
using Uyi.Net.DataPacket;
using Uyi.Net.Message;

namespace Uyi.Net.SocketPipeline
{
    /// <summary>
    /// --
    /// </summary>
    public static class PipelineOperation
    {
        //最小游戏主码值
        private const byte GameMinMainCode = 40;
        private const byte ServerMaxMainCode = 30;

        public static void OnMessageComing(IPipeline piep, ref NetworkStream recbuffer, Action<MessageComingArgs> OnMessageComing)
        {
            TransferPacket data;
            while (PacketCodecHandlerInternal.ParsePacketInternal(ref recbuffer, out data, true))
            {
                switch (data.Code)
                {
                    case TransferPacketType.HeartBeatPing:
                        {
                            piep.SendData(TransferPacket.HeartBeatPongData);
                            return;
                        }
                    case TransferPacketType.Binnary: break;
                    default: return;
                }
                var msg = (TransferMessage)data;
                if (msg != null)
                {
                    MessageComingArgs args = new MessageComingArgs()
                    {
                        Message = msg.Result,
                        PackData = data.PayloadData,
                        SessionID = piep.SessionID,
                        wMainCode = msg.wMainCmdID,
                        wSubCode = msg.wSubCmdID,
                    };
                    OnMessageComing?.Invoke(args);
                }
            }
        }
        public static void OnClient2ServerMessageComing(IGamePipeline piep, ref NetworkStream recbuffer, Action<MessageComingArgs> OnMessageComing)
        {

            TransferPacket data;
            while (PacketCodecHandlerInternal.ParsePacketInternal(ref recbuffer, out data, true))
            {
                switch (data.Code)
                {
                    case TransferPacketType.HeartBeatPing:
                        {
                            piep.SendData(TransferPacket.HeartBeatPongData);
                            return;
                        }
                    case TransferPacketType.Binnary: break;
                    default: return;
                }

                var msg = (TransferMessage)data;
                var wMianID = msg.wMainCmdID;

                #region ----------------------------------

                if (wMianID >= GameMinMainCode)
                {
                    try
                    {
                        piep.GameServer?.Send(data.PayloadData);
                    }
                    catch
                    {
                    }
                }
                else
                {

                    if (msg != null)
                    {
                        OnMessageComing(new MessageComingArgs()
                        {
                            Message = msg.Result,
                            PackData = data.PayloadData,
                            SessionID = piep.SessionID,
                            wMainCode = msg.wMainCmdID,
                            wSubCode = msg.wSubCmdID,
                        });

                    }
                }

                #endregion
            }

        }
        public static void OnServer2ClientMessageComing(IGamePipeline piep, ref NetworkStream recbuffer, Action<MessageComingArgs> OnMessageComing)
        {

            TransferPacket data;
            while (PacketCodecHandlerInternal.ParsePacketInternal(ref recbuffer, out data, true))
            {
                switch (data.Code)
                {
                    case TransferPacketType.HeartBeatPing:
                        {
                            piep.SendData(TransferPacket.HeartBeatPongData);
                            return;
                        }
                    case TransferPacketType.Binnary: break;
                    default: return;
                } 
                var msg = (TransferMessage)data;
                //解析出完整的消息包
                var main = msg.wMainCmdID;
                if (main < ServerMaxMainCode)
                {
                    //消息属于平台消息，通知平台处理
                    if (msg != null)
                    {
                        OnMessageComing(new MessageComingArgs()
                        {
                            Message = msg.Result,
                            PackData = data.PayloadData,
                            SessionID = piep.SessionID,
                            wMainCode = msg.wMainCmdID,
                            wSubCode = msg.wSubCmdID,
                        });
                    }
                    continue;
                }
                piep.SendData(data.PayloadData);
            }
        }
    }
}
