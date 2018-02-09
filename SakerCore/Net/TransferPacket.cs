/***************************************************************************
 * 
 * 创建时间：   2017/12/23 20:56:52
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   这个是为对象传输序列化和反序列化提供基础的数据封包协议实现。
 * 
 * *************************************************************************/

using System;
using SakerCore.IO;
using SakerCore.Serialization;
using SakerCore.Serialization.BigEndian;
using SakerCore.Extension;


/*
                      数 据 包 封 包 协 议 
 ==========================================================================
     0                   1                   2                   3 
     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    +--------------+ - - - - - - - - - - - - - - - - - - - - - - - -+
    |   0x81       |  HeartBeatPing 心跳包 Ping 请求                |
    +--------------+ - - - - - - - - - - - - - - - - - - - - - - - -+
    |   0x82       |  HeartBeatPong 心跳包 Pong 响应                |
    +--------------+ - - - - - - - - - - - - - - - - - - - - - - - -+
    +--------------+---------------+--------------------------------+
    |   0x80       |  data_len     |   extend data_len (16/32)      |
    |              |               |   if data_len = (254/255)      |
    +---------------------------------------------------------------+
    |              TransferData(data_len/extend data_len)           |
    +---------------------------------------------------------------+

    封包操作先写入一个字节表示数据类型
    如果是 0x81 或者是 0x82 表示心跳包消息操作直接返回
    如果是 0x80 表示是传输数据继续
        写入一个字节表示数据长度
            如果数据长度小于 254 直接写入其值表示传输的数据长度
            如果长度大于等于 254 小于 2^16 写入254 
                写入两个字节为ushort长度值 
            如果长度大于等于 2^16 小于 2^31 写入255
                写入两个字节为int长度值
            如果大于 2^31 传输异常
        开始写入传输数据
     
     
     */

namespace SakerCore.Net
{

    /// <summary>
    /// 表示数据交换时封装的数据包信息
    /// </summary>
    public struct TransferPacket
    {

        #region 基本操作支持方法 


        internal static byte[] UnsafePacketData(TransferPacketType opCode, byte[] data)
        {
            switch (opCode)
            {
                case TransferPacketType.HeartBeatPing:
                    return TransferPacket.HeartBeatPingData;
                case TransferPacketType.HeartBeatPong:
                    return TransferPacket.HeartBeatPongData;
                case TransferPacketType.Null:
                    return EmptyByteArray;

            }

            int length = data.Length;
            if (length > int.MaxValue)
            {
                //传输数据量过大
                return EmptyByteArray;
            }

            int lenPing;
            int resultLength;

            //计算输出结果长度
            if (length < 254)
            {
                lenPing = 1;
            }
            else if (length <= ushort.MaxValue)
            {
                lenPing = 3;
            }
            else
            {
                lenPing = 5;
            }

            resultLength = lenPing + length + 1;
            int _rPos;

            var result = new byte[resultLength];
            unsafe
            {
                fixed (byte* r = result)
                {
                    //消息头,指定消息类别
                    r[0] = (byte)opCode;
                    switch (lenPing)
                    {
                        case 1:
                            {
                                r[1] = (byte)length;
                                break;
                            }
                        case 3:
                            {
                                r[1] = 254;
                                UnsafeWriteInt16(r + 2, (ushort)length);
                                break;
                            }
                        case 5:
                            {
                                r[1] = 255;
                                UnsafeWriteInt32(r + 2, length);
                                break;
                            }
                        default:
                            return EmptyByteArray;
                    }
                    _rPos = lenPing + 1;
                }
            }
            //写入消息数据
            Buffer.BlockCopy(data, 0, result, _rPos, length);
            return result;
        }



        /****************************/
        unsafe delegate void delUnsafeWriteInt16(byte* stream, ushort value);
        unsafe delegate void delUnsafeWriteInt32(byte* stream, int value);

        static delUnsafeWriteInt16 UnsafeWriteInt16;
        static delUnsafeWriteInt32 UnsafeWriteInt32;

        unsafe static TransferPacket()
        { 
            if (BitConverter.IsLittleEndian)
            {
                UnsafeWriteInt16 = UnsafeWriteInt16LittleEndian;
                UnsafeWriteInt32 = UnsafeWriteInt32LittleEndian;
            }
            else
            {
                UnsafeWriteInt16 = UnsafeWriteInt16BigEndian;
                UnsafeWriteInt32 = UnsafeWriteInt32BigEndian;
            }


        }





        /// <summary>
        /// 向指定的指针位置的指定偏移位置写入指定的值
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        private static unsafe void UnsafeWriteInt16BigEndian(byte* stream, ushort value)
        {
            stream[0] = (byte)value;
            stream[1] = (byte)(value >> 8);
        }
        private static unsafe void UnsafeWriteInt32BigEndian(byte* stream, int value)
        {
            stream[0] = (byte)value;
            stream[1] = (byte)(value >> 8);
            stream[2] = (byte)(value >> 16);
            stream[3] = (byte)(value >> 24);
        }
        private static unsafe void UnsafeWriteInt16LittleEndian(byte* stream, ushort value)
        {
            stream[0] = (byte)(value >> 8);
            stream[1] = (byte)value;
        }
        private static unsafe void UnsafeWriteInt32LittleEndian(byte* stream, int value)
        {
            stream[0] = (byte)(value >> 24);
            stream[1] = (byte)(value >> 16);
            stream[2] = (byte)(value >> 8);
            stream[3] = (byte)value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packdata"></param>
        /// <param name="needBytesPackdata"></param>
        /// <returns></returns>
        private static bool UnPacketData(NetworkStream buffer, out TransferPacket packdata, bool needBytesPackdata = false)
        {
            packdata = Null;
            byte[] data = null;
            byte[] bytespackdata = null;
            int index = 0;
            int read = 2;
            while (true)
            {
                if (buffer.Count < index + 1) return false;
                switch (buffer[index])
                {
                    case (byte)TransferPacketType.HeartBeatPing:
                        {
                            #region 心跳包消息

                            //心跳包消息
                            packdata = TransferPacket.HeartBeatPing;
                            //删除前导无效数据
                            buffer.Remove(index + 1);
                            return true;

                            #endregion
                        }
                    case (byte)TransferPacketType.HeartBeatPong:
                        {
                            #region 心跳包响应消息

                            //心跳包响应消息
                            packdata = TransferPacket.HeartBeatPong;
                            //删除前导无效数据
                            buffer.Remove(index + 1);
                            return true;

                            #endregion
                        }
                    case (byte)TransferPacketType.Binnary:
                        {
                            #region 二进制流数据拆包


                            //删除前导无效数据
                            buffer.Remove(index);
                            if (buffer.Count < 1) return false;
                            int len;
                            if (!ReadLength(buffer, out len, ref read)) return false;
                            if (buffer.Count < len + read) return false;

                            data = buffer.ReadArray(read, len);
                            if (needBytesPackdata)
                                bytespackdata = buffer.ReadAndRemoveBytes(len + read);
                            else
                                buffer.Remove(len + read);
                            packdata = new TransferPacket(TransferPacketType.Binnary, data, bytespackdata);

                            return true;

                            #endregion 
                        };
                    default:
                        {
                            index++;
                            continue;
                        }

                }
            }
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packdata"></param>
        /// <param name="needBytesPackdata"></param>
        /// <returns></returns>
        private static unsafe bool UnPacketData(byte[] buffer, out TransferPacket packdata, bool needBytesPackdata = false)
        {
            return UnPacketData(new NetworkStream(buffer), out packdata, needBytesPackdata);
        }
        private static bool ReadLength(NetworkStream buffer, out int len, ref int read)
        {
            //读取解析数据长度
            len = buffer[1];
            if (len == 254)
            {
                if (buffer.Count < 4) return false;
                var len_data = buffer.ReadArray(read, 2);
                read += 2;
                ushort t;
                BigEndianPrimitiveTypeSerializer.Instance.ReadValue(len_data, out t);
                len = t;

            }
            else if (len == 255)
            {
                if (buffer.Count < 6) return false;
                var len_data = buffer.ReadArray(read, 4);
                read += 4;
                uint t;
                BigEndianPrimitiveTypeSerializer.Instance.ReadValue(len_data, out t);
                len = (int)t;
            }

            return true;
        }


        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param> 
        /// <param name="payloadData"></param>
        public TransferPacket(TransferPacketType code, byte[] payloadData = null) : this(code, null, payloadData) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <param name="payloadData"></param>
        public TransferPacket(TransferPacketType code, byte[] data, byte[] payloadData)
        {
            this._code = code;
            this._data = data;
            this._payloadData = payloadData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToPacketData()
        {
            if (_payloadData != null)
                return _payloadData;

            switch (this.Code)
            {
                case TransferPacketType.HeartBeatPing:
                    return HeartBeatPingData;
                case TransferPacketType.HeartBeatPong:
                    return HeartBeatPongData;
            }
            var data = this.Data;
            int length;

            if (data == null)
                length = 0;
            else
                length = data.Length;
            if (length > int.MaxValue)
            {
                //传输数据量过大
                return EmptyByteArray;
            }

            int lenPing;
            int resultLength;

            //计算输出结果长度
            if (length < 254)
            {
                lenPing = 1;
            }
            else if (length <= ushort.MaxValue)
            {
                lenPing = 3;
            }
            else
            {
                lenPing = 5;
            }

            resultLength = lenPing + length + 1;
            int _rPos;

            var result = new byte[resultLength];
            unsafe
            {
                fixed (byte* r = result)
                {
                    //消息头,指定消息类别
                    r[0] = (byte)this.Code;
                    switch (lenPing)
                    {
                        case 1:
                            {
                                r[1] = (byte)length;
                                break;
                            }
                        case 3:
                            {
                                r[1] = 254;
                                UnsafeWriteInt16(r + 2, (ushort)length);
                                break;
                            }
                        case 5:
                            {
                                r[1] = 255;
                                UnsafeWriteInt32(r + 2, length);
                                break;
                            }
                        default:
                            return EmptyByteArray;
                    }
                    _rPos = lenPing + 1;
                }
            }
            //写入消息数据
            Buffer.BlockCopy(result, 0, result, _rPos, length);
            return result;

        }





        /// <summary>
        /// 
        /// </summary>
        public static readonly byte[] EmptyByteArray = new byte[0];

        /// <summary>
        /// 
        /// </summary>
        public static readonly TransferPacket Null = new TransferPacket(TransferPacketType.Null, null);
        /// <summary>
        /// 
        /// </summary>
        public static readonly TransferPacket HeartBeatPing = new TransferPacket(TransferPacketType.HeartBeatPing, null);
        /// <summary>
        /// 
        /// </summary>
        public static readonly TransferPacket HeartBeatPong = new TransferPacket(TransferPacketType.HeartBeatPong, null);
        /// <summary>
        /// 
        /// </summary>
        public static readonly byte[] HeartBeatPingData = new byte[] { (byte)TransferPacketType.HeartBeatPing };
        /// <summary>
        /// 
        /// </summary>
        public static readonly byte[] HeartBeatPongData = new byte[] { (byte)TransferPacketType.HeartBeatPong };

        TransferPacketType _code;
        byte[] _data;
        byte[] _payloadData;

        /// <summary>
        /// 
        /// </summary>
        public TransferPacketType Code=>_code;

        /// <summary>
        /// 
        /// </summary>
        public byte[] Data
        {
            get
            {
                switch (this.Code)
                {
                    case TransferPacketType.HeartBeatPing: return HeartBeatPingData;
                    case TransferPacketType.HeartBeatPong: return HeartBeatPongData;
                    case TransferPacketType.Null: return EmptyByteArray;
                    default:
                        {
                            if (_data != null) return _data;
                            if (this._payloadData == null)
                            {
                                _data = EmptyByteArray;
                                return _data;
                            }
                            TransferPacket packdata;
                            UnPacketData(this._payloadData, out packdata);
                            this._code = packdata._code;
                            this._data = packdata.Data;
                            return _data;
                        }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public byte[] PayloadData
        {
            get
            {
                switch (this.Code)
                {
                    case TransferPacketType.HeartBeatPing: return HeartBeatPingData;
                    case TransferPacketType.HeartBeatPong: return HeartBeatPongData;
                    case TransferPacketType.Null: return EmptyByteArray;
                    default:
                        {
                            if (_payloadData != null) return _payloadData;
                            if (this._data == null)
                            {
                                _payloadData = EmptyByteArray;
                                return _payloadData;
                            }
                            _payloadData = ToPacketData();
                            return _payloadData;
                        }
                }

            }
        }
        /// <summary>
        /// 指示消息是否为null
        /// </summary>
        public bool IsNull => this._code == TransferPacketType.Null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="needBytesPackdata"></param>
        /// <returns></returns>
        public static TransferPacket FromDataParam(NetworkStream stream, bool needBytesPackdata = false)
        {
            if (!stream.CanRead) return Null;
            TransferPacket packdata;
            UnPacketData(stream, out packdata, needBytesPackdata);
            return packdata;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator byte[] (TransferPacket value) => value._payloadData ?? EmptyByteArray;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator TransferPacket(byte[] value) => new TransferPacket(TransferPacketType.Binnary, null, value);





    }
    /// <summary>
    /// 表示传输时控制的数据包类型
    /// </summary>
    public enum TransferPacketType
    {
        /// <summary>
        /// 
        /// </summary>
        Null = 0,
        /// <summary>
        /// 
        /// </summary>
        HeartBeatPing = 0x81,
        /// <summary>
        /// 
        /// </summary>
        HeartBeatPong = 0x82,
        /// <summary>
        /// 
        /// </summary>
        Binnary = 0x80,
    }

    /// <summary>
    /// 表示包装后的消息对象
    /// </summary>
    public class TransferMessage
    {
        private object _message;
        private bool IsNull = false;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="meeage"></param>
        public TransferMessage(TransferPacket meeage)
        {
            this.UPacketData = meeage;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meeage"></param>
        public TransferMessage(byte[] meeage)
        {
            this.UPacketData = new TransferPacket(TransferPacketType.Binnary, null, meeage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meeage"></param>
        public TransferMessage(string meeage)
        {
            this.UPacketData = TransferPacket.UnsafePacketData(TransferPacketType.Binnary, meeage.GetBytes());
            this._message = meeage;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meeage"></param>
        public TransferMessage(object meeage)
        {
            this.UPacketData = Encode(meeage);
            this._message = meeage;
        }




        /// <summary>
        /// 
        /// </summary>
        public TransferPacket UPacketData { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public object Result
        {
            get
            {
                if (IsNull) return null;
                InitResult();
                return _message;
            }
        }
        private void InitResult()
        {
            byte main;
            byte sub;
            _message = Decode(UPacketData.Data, out main, out sub);
            if (_message == null)
            {
                IsNull = true;
                return;
            }
            this.wMainCmdID = main;
            this.wSubCmdID = sub;
        }

        /// <summary>
        /// 消息主码
        /// </summary>
        public byte wMainCmdID { get; private set; }
        /// <summary>
        /// 消息辅码
        /// </summary>
        public byte wSubCmdID { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meeage"></param>
        public static explicit operator TransferMessage(TransferPacket meeage)
        {
            var s = new TransferMessage(meeage);
            s.InitResult();
            return s;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meeage"></param>
        public static implicit operator TransferMessage(string meeage) => new TransferMessage(meeage);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meeage"></param>
        public static implicit operator TransferMessage(byte[] meeage) => new TransferMessage(meeage);


        static byte[] Encode(object message)
        {
            if (message == null) return TransferPacket.EmptyByteArray;
            var type = message.GetType();
            var typeCode = BinarySerializer.GetTypeCode(type);
            byte[] body;
            try
            {
                body = BinarySerializer.Serialize(message);
            }
            catch (System.Exception ex)
            {
                //序列化报文异常
                SystemRunErrorPorvider.CatchException(ex);
                return TransferPacket.EmptyByteArray;
            }
            var data = new byte[2 + body.Length];

            unsafe
            {
                // 消息主码
                data[0] = (byte)(typeCode / 256);
                //消息辅码
                data[1] = (byte)typeCode;
                // 2.包体。
                Buffer.BlockCopy(body, 0, data, 2, body.Length);

                return TransferPacket.UnsafePacketData(TransferPacketType.Binnary, data);

            }
        }
        /// <summary>
        /// 提供消息的反序列化操作
        /// </summary>
        /// <param name="buffer">包含消息数据包的字节数组</param>
        /// <param name="main">消息主码</param>
        /// <param name="sub">消息辅码</param>
        /// <returns></returns>
        private object Decode(byte[] buffer, out byte main, out byte sub)
        {
            main = sub = 0;
            try
            {
                if (buffer == null || buffer.Length < 2) return null;
                unsafe
                {
                    object obj;
                    int len = buffer.Length;
                    fixed (byte* b = buffer)
                    {
                        main = b[0];
                        sub = b[1];
                        int typeCode = (main << 8 | sub);
                        ITypeSerializer serializer;
                        if (!BinarySerializer.CanDeserialize(typeCode, out serializer)) return null;
                        try
                        {
                            obj = BinarySerializer.UnsafeDeserialize(b + 2, len, serializer); return obj;
                        }
                        catch //(Exception ex)
                        {
                            //SystemRunErrorPorvider.CatchException(ex);
                            return null;
                        }
                    }

                }
            }
            catch
            {
                return null;
            }

        }
    }
     

}
