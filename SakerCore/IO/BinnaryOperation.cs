/***************************************************************************
 * 
 * 创建时间：   2017/4/21 10:06:39
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   读写字节数据的缓冲区数组
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.IO
{

    /// <summary>
    /// 读写字节数据的缓冲区数组
    /// </summary>
    public class BinnaryOperation : IBinnaryOperation
    {
#pragma warning disable CS1591
        public BinnaryOperation(Stream stream) : this(stream, (BitConverter.IsLittleEndian ? OperationEndianOption.LittleEndian : OperationEndianOption.BigEndian)) { }
        public BinnaryOperation(Stream stream, OperationEndianOption option) : this(stream, option, Encoding.UTF8) { }
        public BinnaryOperation(Stream stream, Encoding encode) : this(stream, (BitConverter.IsLittleEndian ? OperationEndianOption.LittleEndian : OperationEndianOption.BigEndian), encode) { }
        public BinnaryOperation(Stream stream, OperationEndianOption option, Encoding encode)
        {
            if (BitConverter.IsLittleEndian)
            {
                if (option == OperationEndianOption.LittleEndian)
                {
                    serializer = new BigEndianSerializer(stream);
                }
                else
                {
                    serializer = new LittleEndianSerializer(stream);
                }
            }
            else
            {
                if (option == OperationEndianOption.LittleEndian)
                {
                    serializer = new LittleEndianSerializer(stream);
                }
                else
                {
                    serializer = new BigEndianSerializer(stream);
                }
            }
            _encode = encode;
        }

        internal void Write(byte[] data, int offset, int count)
        {
            serializer.Write(data, offset, count);
        }


        static readonly byte[] EmptyBytes = new byte[0];

        IBinnarySerializer serializer;
        Encoding _encode;

        #region 数据写入

        /// <summary>
        /// 写入一个有符号的字节
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(sbyte value)
        {
            WriteValue((byte)value);
        }
        /// <summary>
        /// 写入一个无符号的字节
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(byte value)
        {
            serializer.WriteByte(value);
        }
        /// <summary>
        /// 写入一个无符号的 16 位整数值
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(ushort value)
        {
            WriteValue((short)value);
        }
        /// <summary>
        /// 写入一个有符号的 16 位整数值
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(short value)
        {
            serializer.WriteValue(value);
        }
        /// <summary>
        /// 写入一个无符号的 32 位整数值
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(uint value)
        {
            WriteValue((int)value);
        }
        /// <summary>
        /// 写入一个有符号的 32 位整数值
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(int value)
        {
            serializer.WriteValue(value);
        }
        /// <summary>
        /// 写入一个无符号的 64 位整数值
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(ulong value)
        {
            WriteValue((long)value);
        }
        /// <summary>
        /// 写入一个有符号的 64 位整数值
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(long value)
        {
            serializer.WriteValue(value);
        }
        /// <summary>
        /// 写入表示一个单精度浮点数字
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(float value)
        {
            serializer.WriteValue(value);
        }
        /// <summary>
        /// 写入表示一个双精度浮点数字
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(double value)
        {
            serializer.WriteValue(value);
        }
        /// <summary>
        /// 写入表示一个单精度浮点数字
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(decimal value)
        {
            unsafe
            {
                byte* pos = (byte*)&value;

                serializer.WriteByte(pos[00]);
                serializer.WriteByte(pos[01]);
                serializer.WriteByte(pos[02]);
                serializer.WriteByte(pos[03]);
                serializer.WriteByte(pos[04]);
                serializer.WriteByte(pos[05]);
                serializer.WriteByte(pos[06]);
                serializer.WriteByte(pos[07]);
                serializer.WriteByte(pos[08]);
                serializer.WriteByte(pos[09]);
                serializer.WriteByte(pos[10]);
                serializer.WriteByte(pos[11]);
                serializer.WriteByte(pos[12]);
                serializer.WriteByte(pos[13]);
                serializer.WriteByte(pos[14]);
                serializer.WriteByte(pos[15]);
            }

            //var i = decimal.GetBits(value);

            //WriteInt32(i[0]);
            //WriteInt32(i[1]);
            //WriteInt32(i[2]);
            //WriteInt32(i[3]);

        }
        /// <summary>
        /// 写入表示一个双精度浮点数字
        /// </summary>
        /// <param name="value">需要写入的值</param>
        unsafe public void WriteValue(Guid value)
        {
            var data = value.ToByteArray();
            serializer.Write(data, 0, 16);
        }
        /// <summary>
        /// 写入一个字符串
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                this.WriteLength(0);
                return;
            }
            WriteValue(_encode.GetBytes(value));
        }
        /// <summary>
        /// 写入一个字节数组
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(byte[] value)
        {
            var len = value?.Length ?? 0;
            WriteLength(len);
            if (len > 0)
            {
                serializer.Write(value, 0, len);
            }
        }

        /// <summary>
        /// 写入表示时间的刻度值
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(DateTime value)
        {
            serializer.WriteValue(value.ToBinary());
        }
        /// <summary>
        /// 写入一个时间间隔表示的时间值
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteValue(TimeSpan value)
        {
            serializer.WriteValue((long)value.TotalSeconds);
        }

        #endregion

        #region 数据读取

        public void ReadValue(out byte value)
        {
            value = serializer.ReadByte();
        }
        public void ReadValue(out sbyte value)
        {
            value = (sbyte)serializer.ReadByte();
        }
        public void ReadValue(out byte[] value)
        {
            var len = ReadLength();
            if (len <= 0)
            {
                value = EmptyBytes;
            }
            var buffer = new byte[len];

            serializer.Read(buffer, 0, len);

            value = buffer;
        }
        public void ReadValue(out DateTime value)
        {
            long t;
            ReadValue(out t);
            value = DateTime.FromBinary(t);
        }
        public void ReadValue(out decimal value)
        {
            unsafe
            {
                decimal v1;
                byte* pos = (byte*)&v1;
                pos[00] = serializer.ReadByte();
                pos[01] = serializer.ReadByte();
                pos[02] = serializer.ReadByte();
                pos[03] = serializer.ReadByte();
                pos[04] = serializer.ReadByte();
                pos[05] = serializer.ReadByte();
                pos[06] = serializer.ReadByte();
                pos[07] = serializer.ReadByte();
                pos[08] = serializer.ReadByte();
                pos[09] = serializer.ReadByte();
                pos[10] = serializer.ReadByte();
                pos[11] = serializer.ReadByte();
                pos[12] = serializer.ReadByte();
                pos[13] = serializer.ReadByte();
                pos[14] = serializer.ReadByte();
                pos[15] = serializer.ReadByte();

                value = v1;
            }
        }
        public void ReadValue(out double value)
        {
            value = serializer.ReadDouble();
        }
        public void ReadValue(out float value)
        {

            value = serializer.ReadFloat();
        }

        public void ReadValue(out Guid value)
        {

            byte[] buffer = new byte[16];
            serializer.Read(buffer, 0, 16);
            value = new Guid(buffer);
        }
        public void ReadValue(out short value)
        {

            value = serializer.ReadInt16();
        }

        public void ReadValue(out int value)
        {
            value = serializer.ReadInt32();

        }
        public void ReadValue(out long value)
        {
            value = serializer.ReadInt64();

        }
        public void ReadValue(out string value)
        {
            byte[] bytes;
            ReadValue(out bytes);
            value = _encode.GetString(bytes);
        }
        public void ReadValue(out TimeSpan value)
        {
            long t;
            ReadValue(out t);
            value = new TimeSpan(t * TimeSpan.TicksPerSecond);
        }
        public void ReadValue(out ushort value)
        {
            value = (ushort)serializer.ReadInt16();
        }
        public void ReadValue(out uint value)
        {

            value = (uint)serializer.ReadInt32();
        }
        public void ReadValue(out ulong value)
        {
            value = (ulong)serializer.ReadInt64();

        }



        #endregion

        /// <summary>
        /// 读取一个表示数组长度的值
        /// </summary>
        /// <returns></returns>
        public int ReadLength()
        {
            var len = serializer.ReadByte();

            switch (len)
            {
                case 254:
                    {
                        short t;
                        ReadValue(out t);
                        return t;
                    }
                case 255:
                    {
                        int t;
                        ReadValue(out t);
                        return t;
                    }
                default:
                    return len;
            }

        }
        /// <summary>
        /// 写入一个表示数组长度的值
        /// </summary>
        /// <param name="value">需要写入的值</param>
        public void WriteLength(int value)
        {
            if (value < 254)
            {
                serializer.WriteByte((byte)value);
            }
            else if (value < ushort.MaxValue)
            {
                serializer.WriteByte(254);
                WriteValue((ushort)value);
            }
            else
            {
                serializer.WriteByte(255);
                WriteValue(value);
            }
        }


    }

    #region IBinnarySerializer


    interface IBinnarySerializer
    {

        double ReadDouble();
        float ReadFloat();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();


        void WriteValue(int value);
        void WriteValue(long value);
        void WriteValue(float value);
        void WriteValue(double value);
        void WriteValue(short value);

        int Read(byte[] buffer, int offset, int count);
        byte ReadByte();
        void Write(byte[] data, int offset, int count);
        void WriteByte(byte value);
    }

    #endregion


    unsafe class BigEndianSerializer : IBinnarySerializer
    {
        private Stream _stream;

        public BigEndianSerializer(Stream stream)
        {
            this._stream = stream;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public byte ReadByte()
        {
            return (byte)_stream.ReadByte();
        }

        public double ReadDouble()
        {
            double value = 0;

            byte* pos = (byte*)&value;
            pos[0] = (byte)_stream.ReadByte();
            pos[1] = (byte)_stream.ReadByte();
            pos[2] = (byte)_stream.ReadByte();
            pos[3] = (byte)_stream.ReadByte();
            pos[4] = (byte)_stream.ReadByte();
            pos[5] = (byte)_stream.ReadByte();
            pos[6] = (byte)_stream.ReadByte();
            pos[7] = (byte)_stream.ReadByte();

            return value;

        }
        public float ReadFloat()
        {

            float value = 0;

            byte* pos = (byte*)&value;
            pos[0] = (byte)_stream.ReadByte();
            pos[1] = (byte)_stream.ReadByte();
            pos[2] = (byte)_stream.ReadByte();
            pos[3] = (byte)_stream.ReadByte();

            return value;
        }
        public short ReadInt16()
        {

            short value = 0;

            byte* pos = (byte*)&value;
            pos[0] = (byte)_stream.ReadByte();
            pos[1] = (byte)_stream.ReadByte();

            return value;
        }
        public int ReadInt32()
        {

            int value = 0;

            byte* pos = (byte*)&value;
            pos[0] = (byte)_stream.ReadByte();
            pos[1] = (byte)_stream.ReadByte();
            pos[2] = (byte)_stream.ReadByte();
            pos[3] = (byte)_stream.ReadByte();

            return value;
        }
        public long ReadInt64()
        {

            long value = 0;

            byte* pos = (byte*)&value;
            pos[0] = (byte)_stream.ReadByte();
            pos[1] = (byte)_stream.ReadByte();
            pos[2] = (byte)_stream.ReadByte();
            pos[3] = (byte)_stream.ReadByte();
            pos[4] = (byte)_stream.ReadByte();
            pos[5] = (byte)_stream.ReadByte();
            pos[6] = (byte)_stream.ReadByte();
            pos[7] = (byte)_stream.ReadByte();

            return value;
        }

        public void Write(byte[] data, int offset, int count)
        {
            _stream.Write(data, offset, count);
        }

        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }
        public void WriteValue(double value)
        {
            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[0]);
            _stream.WriteByte(pos[1]);
            _stream.WriteByte(pos[2]);
            _stream.WriteByte(pos[3]);
            _stream.WriteByte(pos[4]);
            _stream.WriteByte(pos[5]);
            _stream.WriteByte(pos[6]);
            _stream.WriteByte(pos[7]);

        }
        public void WriteValue(short value)
        {

            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[0]);
            _stream.WriteByte(pos[1]);
        }
        public void WriteValue(float value)
        {

            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[0]);
            _stream.WriteByte(pos[1]);
            _stream.WriteByte(pos[2]);
            _stream.WriteByte(pos[3]);
        }
        public void WriteValue(long value)
        {

            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[0]);
            _stream.WriteByte(pos[1]);
            _stream.WriteByte(pos[2]);
            _stream.WriteByte(pos[3]);
            _stream.WriteByte(pos[4]);
            _stream.WriteByte(pos[5]);
            _stream.WriteByte(pos[6]);
            _stream.WriteByte(pos[7]);
        }
        public void WriteValue(int value)
        {

            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[0]);
            _stream.WriteByte(pos[1]);
            _stream.WriteByte(pos[2]);
            _stream.WriteByte(pos[3]);
        }
    }
    unsafe class LittleEndianSerializer : IBinnarySerializer
    {
        private Stream _stream;

        public LittleEndianSerializer(Stream stream)
        {
            this._stream = stream;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public byte ReadByte()
        {
            return (byte)_stream.ReadByte();
        }

        public double ReadDouble()
        {
            double value = 0;

            byte* pos = (byte*)&value;
            pos[7] = (byte)_stream.ReadByte();
            pos[6] = (byte)_stream.ReadByte();
            pos[5] = (byte)_stream.ReadByte();
            pos[4] = (byte)_stream.ReadByte();
            pos[3] = (byte)_stream.ReadByte();
            pos[2] = (byte)_stream.ReadByte();
            pos[1] = (byte)_stream.ReadByte();
            pos[0] = (byte)_stream.ReadByte();

            return value;

        }
        public float ReadFloat()
        {
            float value = 0;

            byte* pos = (byte*)&value;

            pos[3] = (byte)_stream.ReadByte();
            pos[2] = (byte)_stream.ReadByte();
            pos[1] = (byte)_stream.ReadByte();
            pos[0] = (byte)_stream.ReadByte();

            return value;
        }
        public short ReadInt16()
        {

            short value = 0;

            byte* pos = (byte*)&value;
            pos[1] = (byte)_stream.ReadByte();
            pos[0] = (byte)_stream.ReadByte();

            return value;
        }
        public int ReadInt32()
        {


            int value = 0;

            byte* pos = (byte*)&value;
            pos[3] = (byte)_stream.ReadByte();
            pos[2] = (byte)_stream.ReadByte();
            pos[1] = (byte)_stream.ReadByte();
            pos[0] = (byte)_stream.ReadByte();

            return value;
        }
        public long ReadInt64()
        {


            long value = 0;

            byte* pos = (byte*)&value;
            pos[7] = (byte)_stream.ReadByte();
            pos[6] = (byte)_stream.ReadByte();
            pos[5] = (byte)_stream.ReadByte();
            pos[4] = (byte)_stream.ReadByte();
            pos[3] = (byte)_stream.ReadByte();
            pos[2] = (byte)_stream.ReadByte();
            pos[1] = (byte)_stream.ReadByte();
            pos[0] = (byte)_stream.ReadByte();

            return value;
        }

        public void Write(byte[] data, int offset, int count)
        {
            _stream.Write(data, offset, count);
        }

        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }
        public void WriteValue(double value)
        {
            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[7]);
            _stream.WriteByte(pos[6]);
            _stream.WriteByte(pos[5]);
            _stream.WriteByte(pos[4]);
            _stream.WriteByte(pos[3]);
            _stream.WriteByte(pos[2]);
            _stream.WriteByte(pos[1]);
            _stream.WriteByte(pos[0]);

        }
        public void WriteValue(short value)
        {
            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[1]);
            _stream.WriteByte(pos[0]);

        }
        public void WriteValue(float value)
        {

            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[3]);
            _stream.WriteByte(pos[2]);
            _stream.WriteByte(pos[1]);
            _stream.WriteByte(pos[0]);

        }
        public void WriteValue(long value)
        {

            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[7]);
            _stream.WriteByte(pos[6]);
            _stream.WriteByte(pos[5]);
            _stream.WriteByte(pos[4]);
            _stream.WriteByte(pos[3]);
            _stream.WriteByte(pos[2]);
            _stream.WriteByte(pos[1]);
            _stream.WriteByte(pos[0]);

        }
        public void WriteValue(int value)
        {
            byte* pos = (byte*)&value;
            _stream.WriteByte(pos[3]);
            _stream.WriteByte(pos[2]);
            _stream.WriteByte(pos[1]);
            _stream.WriteByte(pos[0]);

        }
    }







}

