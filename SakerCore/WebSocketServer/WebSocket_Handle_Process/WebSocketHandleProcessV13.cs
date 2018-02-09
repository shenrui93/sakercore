#define ADDMSGID2

using System;
using SakerCore.IO;
using SakerCore.Serialization.BigEndian;

namespace SakerCore.WebSocketServer
{
    ///**********************************************************/

    #region WebSocket_Handle_Process_V13


    class WebSocketHandleProcessV13 : WebSocket_Handle
    {

        public WebSocketHandleProcessV13(WebSocketSession webSocketSession)
        {
            //设置当前处理器所属的会话信息
            this.webSocketSession = webSocketSession;
            //执行初始化操作
            this.Initializer();

        }

        private void Initializer()
        {
            Buffer = new NetworkStream();
            NStream = new NetworkStream();
        }
        public override void ProcessReceive(byte[] data, int offset, int count)
        {
            Buffer.Write(data, offset, count);
            ProcessReceiveCompletedHandle();
        }
        private void ProcessReceiveCompletedHandle()
        {
            WebSocketOpcode opcode;
            while (true)
            {
                var code = PacketData(ref Buffer, ref NStream, out opcode);

                switch (code)
                {
                    case ParsePacketInternalCode.HasNextData:
                        {
                            continue;
                        }
                    case ParsePacketInternalCode.NotAllData:
                        {
                            return;
                        }
                    case ParsePacketInternalCode.Success:
                        {
                            switch (opcode)
                            {
                                case WebSocketOpcode.Text:
                                    break;
                                case WebSocketOpcode.Binary:
                                    {
                                        var databuffer = NStream?.ToArray();
                                        this.webSocketSession?.OnMessageComing(new WebSocketSessionMessageComingArg(databuffer)
                                        {
                                            Opcode = opcode,
                                            Count = databuffer.Length,
                                            Offset = 0
                                        });
                                        break;
                                    }
                                case WebSocketOpcode.Ping:
                                    {
                                        SendData(WebSocketOpcode.Pong);
                                        break;
                                    }
                                case WebSocketOpcode.Pong:
                                    {
                                        continue;
                                    }
                                case WebSocketOpcode.Close:
                                    {
                                        SendData(WebSocketOpcode.Close);
                                        this.Dispose();
                                        return;
                                    }
                                case WebSocketOpcode.Unkonown:
                                    break;
                                case WebSocketOpcode.Go:
                                    break;
                                default:
                                    {
                                        SendData(WebSocketOpcode.Close);
                                        this.Dispose();
                                        return;

                                    }
                            }
                            continue;
                        }
                    default:
                        {
                            SendData(WebSocketOpcode.Close);
                            this.Dispose();
                            return;
                        }
                }
            }
        }

        #region 数据编解码协议

        protected override byte[] PacketResponseData(WebMessageData data)
        {
            return Static_PacketResponseData(data);
        }
        private static ParsePacketInternalCode PacketData(ref NetworkStream buffer, ref NetworkStream nStream, out WebSocketOpcode opCode)
        {
            opCode = WebSocketOpcode.Unkonown;
            if (buffer.Count < 2) return ParsePacketInternalCode.NotAllData;
            var readPos = 0;       //数据流读取游标

            byte b = buffer[readPos];
            var isEof = b >> 7;             //判断当前帧是否是数据的结束帧
            var rsv1 = (b >> 6) & 0x01;
            var rsv2 = (b >> 5) & 0x01;
            var rsv3 = (b >> 4) & 0x01;

            if (rsv1 != 0 || rsv2 != 0 || rsv3 != 0)
            {
                //数据包错误
                return ParsePacketInternalCode.ErrorData;
            }

            opCode = (WebSocketOpcode)((b) & 0xF);  //Opcode

            readPos = 1;
            b = buffer[readPos];
            var mask = b >> 7;              //掩码值
            if (mask != 1)
            {
                //掩码值解析错误
                return ParsePacketInternalCode.ErrorData;
            }
            int dataLen = (b & 0x7F);
            readPos = 2;
            if (dataLen == 126)
            {
                if (buffer.Count < readPos + 2) return ParsePacketInternalCode.NotAllData;

                //16位长度字节
                var data = buffer.ReadArray(readPos, 2);

                ushort t;
                BigEndianPrimitiveTypeSerializer.Instance.ReadValue(data, out t);
                dataLen = t;

                readPos += 2;

            }
            else if (dataLen == 127)
            {
                if (buffer.Count < readPos + 8) return ParsePacketInternalCode.NotAllData;
                //64位长度字节
                var data = buffer.ReadArray(readPos, 8);

                ulong t;
                BigEndianPrimitiveTypeSerializer.Instance.ReadValue(data, out t);
                dataLen = (int)t;
                //64位字节数据长度 
                readPos += 8;
            }

            var allLen = (int)(readPos + (int)dataLen) + 4;

            if (buffer.Count < allLen)
                return ParsePacketInternalCode.NotAllData;

            //掩码值key
            var maskingKey = buffer.ReadArray(readPos, 4);
            readPos += 4;
            var payData = buffer.ReadArray(readPos, dataLen);

            //去除掩码
            for (var i = 0; i < payData.Length; i++)
            {
                payData[i] = (byte)(payData[i] ^ maskingKey[i % 4]);
            }
            nStream.Write(payData, 0, dataLen);
            //删除数据
            buffer.Remove(allLen);
            return isEof == 1 ? ParsePacketInternalCode.Success : ParsePacketInternalCode.HasNextData;
        }


        #endregion

        public override void Ping()
        {
            this.webSocketSession?.SendDataToClient(PingBuffer);
        }

        static byte[] PingBuffer = Static_PacketResponseData(WebMessageData.Ping);

        static byte[] Static_PacketResponseData(WebMessageData data)
        {
            //消息组包协议
            var dataLen = data.Data.Length;

            switch (data.OpCode)
            {
                case WebSocketOpcode.Text:
                case WebSocketOpcode.Binary:
                    if (dataLen == 0) return new byte[0];
                    break;
            }

            using (var stream = new NetworkStream())
            {
                Static_PacketResponseDataStream(stream, data);
                return stream.ToArray();
            }

        }
        private static void Static_PacketResponseDataStream(NetworkStream writer, WebMessageData data)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

#if ADDMSGID

            //消息组包协议（我们规定消息总长度是消息编号1位+数组长度）
            var dataLen = data.Data.Length + 1;

#else 
            var dataLen = data.Data.Length;  
#endif

            if (dataLen == 0) return;
            byte b = 0x80;

            b = (byte)(b | ((byte)data.OpCode & 0xF));
            //写入opcode
            writer.WriteByte(b);

            if (dataLen <= 125)
            {
                writer.WriteByte((byte)(dataLen & 0x7F));
            }
            else if (dataLen < ushort.MaxValue)
            {
                writer.WriteByte(126 & 0x7F);
                BigEndianPrimitiveTypeSerializer.Instance.WriteValue(writer, (ushort)dataLen);
            }
            else
            {
                writer.WriteByte(127 & 0x7F);
                BigEndianPrimitiveTypeSerializer.Instance.WriteValue(writer, (uint)dataLen);
            }
#if ADDMSGID
            //写入消息编号
            writer.WriteByte(data.MessageId);
            //写入消息数据
            writer.Write(data.Data, 0, dataLen - 1);

#else
//写入消息数据
            writer.Write(data.Data, 0, dataLen);
#endif
        }
    }
    #endregion

}