/******************************************************************
 * 功能概述：为基础类型数据提供序列化和反序列化的功能。
 * 修改人员：沈瑞
 * 修改时间：2015年9月22日 09:57:30
 * 
 * 产品版本：1.0
 * CLR 版本：4.0.30319.34014
 ******************************************************************/

#define INT64_DOUBLE


#pragma warning disable 1591
using System;
using System.Data;
using System.IO;
using System.Text;

namespace SakerCore.Serialization
{

    /**************************************************************************************************************
     * 
     * bool      -> System.Boolean (布尔型，其值为 true 或者 false)
     * byte      -> System.Byte (字节型，占 1 字节，表示 8 位正整数，范围 0 ~ 255)
     * sbyte     -> System.SByte (带符号字节型，占 1 字节，表示 8 位整数，范围 -128 ~ 127)
     * char      -> System.Char (字符型，占有两个字节，表示 1 个 Unicode 字符)
     * short     -> System.Int16 (短整型，占 2 字节，表示 16 位整数，范围 -32,768 ~ 32,767)
     * ushort    -> System.UInt16 (无符号短整型，占 2 字节，表示 16 位正整数，范围 0 ~ 65,535)
     * uint      -> System.UInt32 (无符号整型，占 4 字节，表示 32 位正整数，范围 0 ~ 4,294,967,295)
     * int       -> System.Int32 (整型，占 4 字节，表示 32 位整数，范围 -2,147,483,648 到 2,147,483,647)
     * float     -> System.Single (单精度浮点型，占 4 个字节)
     * ulong     -> System.UInt64 (无符号长整型，占 8 字节，表示 64 位正整数，范围 0 ~ 大约 10 的 20 次方)
     * long      -> System.Int64 (长整型，占 8 字节，表示 64 位整数，范围大约 -(10 的 19) 次方 到 10 的 19 次方)
     * double    -> System.Double (双精度浮点型，占8 个字节) 
     * decimal   -> System.Decimal (128位浮点型，占16 个字节) 
     * 
     *************************************************************************************************************/

    namespace LittleEndian      //小端传输存储序
    {
        #region LittleEndianPrimitiveTypeSerializerBase

        /// <summary>
        /// 为系统基础类型字节编码提供统一的序列化和反序列化编码能力(小端存储序)
        /// </summary>
        public sealed class LittleEndianPrimitiveTypeSerializer : StaticTypeSerializerOperationBase<sbyte> { }
        #endregion
    }

    namespace BigEndian  //大端传输存储序
    {
        /// <summary>
        /// 为系统基础类型字节编码提供统一的序列化和反序列化编码能力(大端存储序)
        /// </summary>
        public sealed class BigEndianPrimitiveTypeSerializer : StaticTypeSerializerOperationBase<byte>
        { 
        }
    }







    #region 时间格式的序列化和反序列化操作

    static class DateTimeExpand
    {

        static readonly DateTime Utc = new DateTime(1970, 1, 1, 0, 0, 0);
        internal static double ToTimeStamp(DateTime date)
        { 
            //获取世界时间
            var utcTime = date.ToUniversalTime(); 
            //返回世界时间戳，毫秒级别
            return Math.Floor((utcTime - Utc).TotalMilliseconds);
        }
        internal static DateTime FromTimeStamp(double value)
        {
            //计算时间戳表示的时间 
            var utcTime = Utc.AddMilliseconds(value);
            return utcTime.ToLocalTime();
        }
    }

    #endregion

    #region TypeSerializerOperationBase

    public abstract class TypeSerializerOperationBase : ITypeSerializerOperation
    {
        public readonly static byte[] EmptyBytes = new byte[0];
        static object root = new object();


        #region 提供静态的单根实例


        private static TypeSerializerOperationBase _littleEndianSerializerOperation;
        private static TypeSerializerOperationBase _bigEndianSerializerOperation;

        public static TypeSerializerOperationBase LittleEndianSerializerOperation
        {
            get
            {
                if (_littleEndianSerializerOperation != null) return _littleEndianSerializerOperation;

                lock (root)
                {
                    if (_littleEndianSerializerOperation != null) return _littleEndianSerializerOperation;


                    if (BitConverter.IsLittleEndian)
                    {
                        _littleEndianSerializerOperation = new BigEndianTypeSerializerOperation();
                    }
                    else
                    {
                        _littleEndianSerializerOperation = new LittleEndianTypeSerializerOperation();
                    }

                    return _littleEndianSerializerOperation;
                }

            }
        }
        public static TypeSerializerOperationBase BigEndianSerializerOperation
        {
            get
            {
                if (_bigEndianSerializerOperation != null) return _bigEndianSerializerOperation;

                lock (root)
                {
                    if (_bigEndianSerializerOperation != null) return _bigEndianSerializerOperation;


                    if (BitConverter.IsLittleEndian)
                    {
                        _bigEndianSerializerOperation = new LittleEndianTypeSerializerOperation();
                    }
                    else
                    {
                        _bigEndianSerializerOperation = new BigEndianTypeSerializerOperation();
                    }

                    return _bigEndianSerializerOperation;
                }

            }
        }


        #endregion

        public abstract void ReadValue(byte[] stream, out uint value);
        public abstract void ReadValue(byte[] stream, out ushort value);
        public abstract void ReadValue(byte[] stream, out ulong value);
        public abstract void WriteValue(byte[] stream, uint value);







        public abstract double ReadDouble(Stream stream);
        public abstract short ReadInt16(Stream stream);
        public abstract int ReadInt32(Stream stream);
        public abstract float ReadFloat(Stream stream);
        public abstract long ReadInt64(Stream stream);
        public abstract void WriteFloat(Stream stream, float value);
        public abstract void WriteDouble(Stream stream, double value);
        public abstract void WriteInt64(Stream stream, long value);
        public abstract void WriteInt32(Stream stream, int value);
        public abstract void WriteInt16(Stream stream, short value);
        public abstract unsafe double ReadDouble(byte* stream, int* pos, int length);
        public abstract unsafe long ReadInt64(byte* stream, int* pos, int length);
        public abstract unsafe float ReadFloat(byte* stream, int* pos, int length);
        public abstract unsafe short ReadInt16(byte* stream, int* pos, int length);
        public abstract unsafe int ReadInt32(byte* stream, int* pos, int length);


        #region 数据的处理方法



        public int ReadLength(Stream stream)
        {
            var len = stream.ReadByte();

            switch (len)
            {
                case 254:
                    {
                        ushort t;
                        this.ReadValue(stream, out t);
                        return t;
                    }
                case 255:
                    {
                        int t;
                        this.ReadValue(stream, out t);
                        return t;
                    }
                default:
                    {
                        return len < 0 ? 0 : len;
                    }
            }
        }
        public void WriteLength(Stream stream, int value)
        {
            if (value <= 0)
            {
                stream.WriteByte(0);
                return;
            }
            if (value < 254)
            {
                stream.WriteByte((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                stream.WriteByte(254);
                WriteValue(stream, (ushort)value);
            }
            else
            {
                stream.WriteByte(255);
                WriteValue(stream, value);
            }
        }


        public virtual void ReadValue(Stream stream, out byte value)
        {
            value = (byte)stream.ReadByte();
        }
        public virtual void ReadValue(Stream stream, out byte[] value)
        {
            var length = stream.ReadByte();
            if (length == 254)
            {
                ushort t;
                ReadValue(stream, out t);
                length = t;
            }
            else if (length == 255)
            {
                ReadValue(stream, out length);
            }
            byte[] data = new byte[length];
            if (length > 0)
                stream.Read(data, 0, length);
            value = data;
        }
        public virtual void ReadValue(Stream stream, out ushort value)
        {
            value = (ushort)ReadInt16(stream);
        }
        public virtual void ReadValue(Stream stream, out uint value)
        {
            value = (uint)ReadInt32(stream);
        }
        public virtual void ReadValue(Stream stream, out ulong value)
        {
            long val;
            ReadValue(stream, out val);
            value = (ulong)val;
        }
        public virtual void ReadValue(Stream stream, out short value)
        {
            value = ReadInt16(stream);
        }
        public virtual void ReadValue(Stream stream, out int value)
        {
            value = ReadInt32(stream);
        }
        public virtual void ReadValue(Stream stream, out long value)
        {
            value = ReadInt64(stream);
        }
        public virtual void ReadValue(Stream stream, out bool value)
        {
            var b = stream.ReadByte();
            value = b != 0;
        }
        public virtual void ReadValue(Stream stream, out sbyte value)
        {
            value = (sbyte)stream.ReadByte();
        }
        public virtual void ReadValue(Stream stream, out char value)
        {
            value = (char)(ushort)ReadInt16(stream);
        }
        public virtual void ReadValue(Stream stream, out float value)
        {
            value = ReadFloat(stream);
        }
        public virtual void ReadValue(Stream stream, out double value)
        {
            value = ReadDouble(stream);
        }
        public virtual void ReadValue(Stream stream, out TimeSpan value)
        {
            var totals = ReadDouble(stream);
            value = TimeSpan.FromSeconds(totals);
        }
        public virtual void ReadValue(Stream stream, out DateTime value)
        {
            double val = 0;
            ReadValue(stream, out val);
            value = DateTimeExpand.FromTimeStamp(val);
        }
        public virtual void ReadValue(Stream stream, out string value)
        {
            byte[] bytes;
            ReadValue(stream, out bytes);
            if (bytes.Length <= 0)
            {
                value = string.Empty;
                return;
            }
            value = System.Text.Encoding.UTF8.GetString(bytes);
        }
        public virtual void ReadValue(Stream stream, out Guid value)
        {
            var bytes = new byte[16];

            stream.Read(bytes, 0, 16);

            value = new Guid(bytes);
        }
        public virtual void ReadValue(Stream stream, out DataTable value)
        {
            value = DataTableTypeSerializer.Deserializer(stream);
        }
        public virtual void ReadValue(Stream stream, out decimal value)
        {
            int[] num = new int[4];
            ReadValue(stream, out num[0]);
            ReadValue(stream, out num[1]);
            ReadValue(stream, out num[2]);
            ReadValue(stream, out num[3]);

            value = new decimal(num);
        }

        public virtual void WriteValue(Stream stream, byte value)
        {
            stream.WriteByte(value);
        }
        public virtual void WriteValue(Stream stream, byte[] value)
        {
            int length = value == null ? 0 : value.Length;

            if (length < 254)
            {
                stream.WriteByte((byte)length);
            }
            else if (length < ushort.MaxValue)
            {
                stream.WriteByte(254);
                WriteValue(stream, (ushort)length);
            }
            else
            {
                stream.WriteByte(255);
                WriteValue(stream, length);

            }
            if (length > 0)
                stream.Write(value, 0, length);
        }
        public virtual void WriteValue(Stream stream, ushort value)
        {
            WriteValue(stream, (short)value);
        }
        public virtual void WriteValue(Stream stream, uint value)
        {
            WriteValue(stream, (int)value);
        }
        public virtual void WriteValue(Stream stream, ulong value)
        {
            WriteValue(stream, (long)value);
        }
        public virtual void WriteValue(Stream stream, short value)
        {
            WriteInt16(stream, value);
        }
        public virtual void WriteValue(Stream stream, int value)
        {
            WriteInt32(stream, value);
        }
        public virtual void WriteValue(Stream stream, long value)
        {
            WriteInt64(stream, value);
        }
        public virtual void WriteValue(Stream stream, bool value)
        {
            WriteValue(stream, value ? (byte)1 : (byte)0);
        }
        public virtual void WriteValue(Stream stream, sbyte value)
        {
            WriteValue(stream, (byte)value);
        }
        public virtual void WriteValue(Stream stream, char value)
        {
            WriteValue(stream, (ushort)value); // 2 字节的 Unicode 字符。
        }
        public virtual void WriteValue(Stream stream, double value)
        {
            WriteDouble(stream, value);
        }
        public virtual void WriteValue(Stream stream, float value)
        {
            WriteFloat(stream, value);
        }
        public virtual void WriteValue(Stream stream, DateTime value)
        {
            double v = DateTimeExpand.ToTimeStamp(value);
            WriteValue(stream, v);
        }
        public virtual void WriteValue(Stream stream, TimeSpan value)
        {
            double v = Math.Floor(value.TotalSeconds);
            WriteValue(stream, v);
        }
        public virtual void WriteValue(Stream stream, string value)
        {
            byte[] bytes;
            if (string.IsNullOrEmpty(value))
                bytes = new byte[0];
            else
            {
                value = value.Trim();
                bytes = System.Text.Encoding.UTF8.GetBytes(value);
            }
            WriteValue(stream, bytes);
        }
        public virtual void WriteValue(Stream stream, Guid value)
        {
            var bytes = value.ToByteArray();
            stream.Write(bytes, 0, 16);
        }
        public virtual void WriteValue(Stream stream, DataTable value)
        {
            DataTableTypeSerializer.Serializer(stream, value);
        }
        public virtual void WriteValue(Stream stream, decimal value)
        {
            int[] num = decimal.GetBits(value);
            WriteValue(stream, num[0]);
            WriteValue(stream, num[1]);
            WriteValue(stream, num[2]);
            WriteValue(stream, num[3]);
        }

        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out byte value)
        {
            byte* readstart = stream + *pos;
            *pos += 1;
            if (*pos >= length)
            {
                value = 0;
            }
            value = *readstart;
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out byte[] value)
        {
            byte* readstart = stream + *pos;

            byte by;
            ReadValue(stream, pos, length, out by);
            int bytelength;
            switch (by)
            {
                case 254:
                    {
                        ushort t;
                        ReadValue(stream, pos, length, out t);
                        bytelength = t;
                        break;
                    }
                case 255:
                    {
                        ReadValue(stream, pos, length, out bytelength);
                        break;
                    }
                default:
                    {
                        bytelength = by;
                        break;
                    }
            }
            byte[] result = new byte[bytelength];
            byte* copyStart = stream + *pos;
            *pos += bytelength;
            if (*pos >= length)
            {
                value = EmptyBytes;
                return;
            }
            ToolsMethods.MemoryCopy(copyStart, result, 0, bytelength);
            value = result;
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out ushort value)
        {
            value = (ushort)ReadInt16(stream, pos, length);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out uint value)
        {
            value = (uint)ReadInt32(stream, pos, length);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out ulong value)
        {
            long val;
            ReadValue(stream, pos, length, out val);
            value = (ulong)val;
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out short value)
        {
            value = ReadInt16(stream, pos, length);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out int value)
        {
            value = ReadInt32(stream, pos, length);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out long value)
        {
            value = ReadInt64(stream, pos, length);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out bool value)
        {
            byte b;
            ReadValue(stream, pos, length, out b);
            value = b != 0;
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out sbyte value)
        {
            byte b;
            ReadValue(stream, pos, length, out b);
            value = (sbyte)b;
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out char value)
        {
            value = (char)ReadInt16(stream, pos, length);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out float value)
        {
            value = ReadFloat(stream, pos, length);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out double value)
        {
            value = ReadDouble(stream, pos, length);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out TimeSpan value)
        {
            var totals = ReadDouble(stream, pos, length);
            value = TimeSpan.FromSeconds(totals);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out DateTime value)
        {
            double val = 0;
            ReadValue(stream, pos, length, out val);
            value = DateTimeExpand.FromTimeStamp(val);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out string value)
        {
            byte[] bytes;
            ReadValue(stream, pos, length, out bytes);
            if (bytes.Length <= 0)
            {
                value = string.Empty;
                return;
            }
            value = Encoding.UTF8.GetString(bytes);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out Guid value)
        {
            byte* readpos = stream + *pos;
            *pos += 16;
            if (*pos >= length)
            {
                value = Guid.Empty;
                return;
            }
            var bytes = new byte[16];
            ToolsMethods.MemoryCopy(readpos, bytes, 0, 16);

            value = new Guid(bytes);
        }
        public unsafe virtual void ReadValue(byte* stream, int* pos, int length, out decimal value)
        {
            int[] num = new int[4];
            ReadValue(stream, pos, length, out num[0]);
            ReadValue(stream, pos, length, out num[1]);
            ReadValue(stream, pos, length, out num[2]);
            ReadValue(stream, pos, length, out num[3]);

            value = new decimal(num);
        }



        #endregion

    }

    #endregion

    #region TypeSerializerOperation

    public sealed class TypeSerializerOperation : TypeSerializerOperationBase
    {

        private TypeSerializerOperationBase _serializer;

        public TypeSerializerOperation() : this((BitConverter.IsLittleEndian ? OperationEndianOption.LittleEndian : OperationEndianOption.BigEndian)) { }
        public TypeSerializerOperation(OperationEndianOption option)
        {
            if (BitConverter.IsLittleEndian)
            {
                if (option == OperationEndianOption.LittleEndian)
                {
                    _serializer = new BigEndianTypeSerializerOperation();
                }
                else
                {
                    _serializer = new LittleEndianTypeSerializerOperation();
                }
            }
            else
            {
                if (option == OperationEndianOption.LittleEndian)
                {
                    _serializer = new LittleEndianTypeSerializerOperation();
                }
                else
                {
                    _serializer = new BigEndianTypeSerializerOperation();
                }

            }
        }

        public override void ReadValue(byte[] stream, out uint value) => _serializer.ReadValue(stream,out value);
        public override void ReadValue(byte[] stream, out ushort value) => _serializer.ReadValue(stream, out value);
        public override void ReadValue(byte[] stream, out ulong value) => _serializer.ReadValue(stream, out value);
        public override void WriteValue(byte[] stream, uint value) => _serializer.WriteValue(stream, value);




        public override double ReadDouble(Stream stream) => _serializer.ReadDouble(stream);
        public override short ReadInt16(Stream stream) => _serializer.ReadInt16(stream);
        public override int ReadInt32(Stream stream) => _serializer.ReadInt32(stream);
        public override long ReadInt64(Stream stream) => _serializer.ReadInt64(stream);
        public override float ReadFloat(Stream stream) => _serializer.ReadFloat(stream);



        public override unsafe double ReadDouble(byte* stream, int* pos, int length) => _serializer.ReadDouble(stream, pos, length);
        public override unsafe float ReadFloat(byte* stream, int* pos, int length) => _serializer.ReadFloat(stream, pos, length);
        public override unsafe int ReadInt32(byte* stream, int* pos, int length) => _serializer.ReadInt32(stream, pos, length);
        public override unsafe short ReadInt16(byte* stream, int* pos, int length) => _serializer.ReadInt16(stream, pos, length);
        public override unsafe long ReadInt64(byte* stream, int* pos, int length) => _serializer.ReadInt64(stream, pos, length);



        public override void WriteDouble(Stream stream, double value) => _serializer.WriteDouble(stream, value);
        public override void WriteFloat(Stream stream, float value) => _serializer.WriteFloat(stream, value);
        public override void WriteInt16(Stream stream, short value) => _serializer.WriteInt16(stream, value);
        public override void WriteInt32(Stream stream, int value) => _serializer.WriteInt32(stream, value);
        public override void WriteInt64(Stream stream, long value) => _serializer.WriteInt64(stream, value);


    }

    #endregion

    #region StaticTypeSerializerOperationBase

    public class StaticTypeSerializerOperationBase<HostType>
    {

        static StaticTypeSerializerOperationBase()
        {
            var type = typeof(HostType);

            if (type != typeof(byte))
            {
                _host = TypeSerializerOperationBase.LittleEndianSerializerOperation;
            }
            else
            {
                _host = TypeSerializerOperationBase.BigEndianSerializerOperation;
            }
        }
        public static ITypeSerializerOperation Instance => _host;

        private static TypeSerializerOperationBase _host;

    }



    #endregion






    #region 私有处理类，提供数据序列化排布顺序的处理

    unsafe class BigEndianTypeSerializerOperation : TypeSerializerOperationBase
    {

        public override void ReadValue(byte[] stream, out uint value)
        {

            uint r = 0;
            byte* pos = (byte*)&r;

            pos[0] = stream[0];
            pos[1] = stream[1];
            pos[2] = stream[2];
            pos[3] = stream[3]; 
            value = r;
        }
        public override void ReadValue(byte[] stream, out ushort value)
        {

            ushort r = 0;
            byte* pos = (byte*)&r;

            pos[0] = stream[0];
            pos[1] = stream[1]; 
            value = r;
        }
        public override void ReadValue(byte[] stream, out ulong value)
        {

           ulong r = 0;
            byte* pos = (byte*)&r;

            pos[0] = stream[0];
            pos[1] = stream[1];
            pos[2] = stream[2];
            pos[3] = stream[3];
            pos[4] = stream[4];
            pos[5] = stream[5];
            pos[6] = stream[6];
            pos[7] = stream[7];
            value = r;

        }
        public override void WriteValue(byte[] stream, uint value)
        {  
            byte* pos = (byte*)&value;

            stream[0] = pos[0];
            stream[1] = pos[1];
              stream[2] = pos[2];
              stream[3] = pos[3];
             
        }


        public override double ReadDouble(Stream stream)
        {
            double value;
            byte* pos = (byte*)&value;

            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            pos[0] = buffer[0];
            pos[1] = buffer[1];
            pos[2] = buffer[2];
            pos[3] = buffer[3];
            pos[4] = buffer[4];
            pos[5] = buffer[5];
            pos[6] = buffer[6];
            pos[7] = buffer[7];

            return value;


        }
        public override short ReadInt16(Stream stream)
        {

            short value;
            byte* pos = (byte*)&value;

            pos[0] = (byte)stream.ReadByte();
            pos[1] = (byte)stream.ReadByte();

            return value;

        }
        public override int ReadInt32(Stream stream)
        {

            int value;
            byte* pos = (byte*)&value;

            pos[0] = (byte)stream.ReadByte();
            pos[1] = (byte)stream.ReadByte();
            pos[2] = (byte)stream.ReadByte();
            pos[3] = (byte)stream.ReadByte();

            return value;
        }
        public override long ReadInt64(Stream stream)
        {
#if INT64_DOUBLE
            return (long)ReadDouble(stream);
#endif

            long value;
            byte* pos = (byte*)&value;

            pos[0] = (byte)stream.ReadByte();
            pos[1] = (byte)stream.ReadByte();
            pos[2] = (byte)stream.ReadByte();
            pos[3] = (byte)stream.ReadByte();
            pos[4] = (byte)stream.ReadByte();
            pos[5] = (byte)stream.ReadByte();
            pos[6] = (byte)stream.ReadByte();
            pos[7] = (byte)stream.ReadByte();

            return value;
        }
        public override float ReadFloat(Stream stream)
        {

            float value;
            byte* pos = (byte*)&value;

            pos[0] = (byte)stream.ReadByte();
            pos[1] = (byte)stream.ReadByte();
            pos[2] = (byte)stream.ReadByte();
            pos[3] = (byte)stream.ReadByte();

            return value;
        }


        public override unsafe double ReadDouble(byte* stream, int* pos, int length)
        {
            const int valLen = 8;

            double value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }
            var startPos = stream + *pos;

            valpos[0] = startPos[0];
            valpos[1] = startPos[1];
            valpos[2] = startPos[2];
            valpos[3] = startPos[3];
            valpos[4] = startPos[4];
            valpos[5] = startPos[5];
            valpos[6] = startPos[6];
            valpos[7] = startPos[7];

            *pos += valLen; 

            return value;
        }
        public override unsafe float ReadFloat(byte* stream, int* pos, int length)
        {

            const int valLen = 4;

            float value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }
            var startPos = stream + *pos;

            valpos[0] = startPos[0];
            valpos[1] = startPos[1];
            valpos[2] = startPos[2];
            valpos[3] = startPos[3];

            *pos += valLen; 

            return value;
        }
        public override unsafe short ReadInt16(byte* stream, int* pos, int length)
        {

            const int valLen = 2;

            short value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }

            var startPos = stream + *pos;
            valpos[0] = startPos[0];
            valpos[1] = startPos[1];

            *pos += valLen; 

            return value;
        }
        public override unsafe int ReadInt32(byte* stream, int* pos, int length)
        {

            const int valLen = 4;

            int value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }
            var startPos = stream + *pos;

            valpos[0] = startPos[0];
            valpos[1] = startPos[1];
            valpos[2] = startPos[2];
            valpos[3] = startPos[3];

            *pos += valLen; 

            return value;
        }
        public override unsafe long ReadInt64(byte* stream, int* pos, int length)
        {

#if INT64_DOUBLE
            return (long)ReadDouble(stream,pos,length);
#endif

            const int valLen = 8;

            long value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }
            var startPos = stream + *pos;

            valpos[0] = startPos[0];
            valpos[1] = startPos[1];
            valpos[2] = startPos[2];
            valpos[3] = startPos[3];
            valpos[4] = startPos[4];
            valpos[5] = startPos[5];
            valpos[6] = startPos[6];
            valpos[7] = startPos[7];

            *pos += valLen; 

            return value;
        }



        public override void WriteDouble(Stream stream, double value)
        {
            byte* pos = (byte*)&value;
            stream.WriteByte(pos[0]);
            stream.WriteByte(pos[1]);
            stream.WriteByte(pos[2]);
            stream.WriteByte(pos[3]);
            stream.WriteByte(pos[4]);
            stream.WriteByte(pos[5]);
            stream.WriteByte(pos[6]);
            stream.WriteByte(pos[7]);

        }
        public override void WriteFloat(Stream stream, float value)
        {
            byte* pos = (byte*)&value;
            stream.WriteByte(pos[0]);
            stream.WriteByte(pos[1]);
            stream.WriteByte(pos[2]);
            stream.WriteByte(pos[3]);
        }
        public override void WriteInt16(Stream stream, short value)
        {
            byte* pos = (byte*)&value;
            stream.WriteByte(pos[0]);
            stream.WriteByte(pos[1]);
        }
        public override void WriteInt32(Stream stream, int value)
        {
            byte* pos = (byte*)&value;
            stream.WriteByte(pos[0]);
            stream.WriteByte(pos[1]);
            stream.WriteByte(pos[2]);
            stream.WriteByte(pos[3]);
        }
        public override void WriteInt64(Stream stream, long value)
        {

#if INT64_DOUBLE
            WriteDouble(stream, value);
            return;
#endif

            byte* pos = (byte*)&value;
            stream.WriteByte(pos[0]);
            stream.WriteByte(pos[1]);
            stream.WriteByte(pos[2]);
            stream.WriteByte(pos[3]);
            stream.WriteByte(pos[4]);
            stream.WriteByte(pos[5]);
            stream.WriteByte(pos[6]);
            stream.WriteByte(pos[7]);
        }


    }
    unsafe class LittleEndianTypeSerializerOperation : TypeSerializerOperationBase
    {


        public override void ReadValue(byte[] stream, out uint value)
        {


            uint r = 0;
            byte* pos = (byte*)&r;

            pos[3] = stream[0];
            pos[2] = stream[1];
            pos[1] = stream[2];
            pos[0] = stream[3]; 
            value = r;
        }
        public override void ReadValue(byte[] stream, out ushort value)
        {


            ushort r = 0;
            byte* pos = (byte*)&r;

            pos[1] = stream[0];
            pos[0] = stream[1]; 
            value = r;
        }
        public override void ReadValue(byte[] stream, out ulong value)
        {

            ulong r = 0;
            byte* pos = (byte*)&r;

            pos[7] = stream[0];
            pos[6] = stream[1];
            pos[5] = stream[2];
            pos[4] = stream[3];
            pos[3] = stream[4];
            pos[2] = stream[5];
            pos[1] = stream[6];
            pos[0] = stream[7];
            value = r;

        }
        public override void WriteValue(byte[] stream, uint value)
        {
            byte* pos = (byte*)&value;

            stream[3] = pos[0];
            stream[2] = pos[1];
            stream[1] = pos[2];
            stream[0] = pos[3];

        }



        public override double ReadDouble(Stream stream)
        {
            double value;
            byte* pos = (byte*)&value;

            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            pos[7] = buffer[0];
            pos[6] = buffer[1];
            pos[5] = buffer[2];
            pos[4] = buffer[3];
            pos[3] = buffer[4];
            pos[2] = buffer[5];
            pos[1] = buffer[6];
            pos[0] = buffer[7];

            return value;


        }
        public override short ReadInt16(Stream stream)
        {

            short value;
            byte* pos = (byte*)&value;

            pos[1] = (byte)stream.ReadByte();
            pos[0] = (byte)stream.ReadByte();

            return value;

        }
        public override int ReadInt32(Stream stream)
        {

            int value;
            byte* pos = (byte*)&value;

            pos[3] = (byte)stream.ReadByte();
            pos[2] = (byte)stream.ReadByte();
            pos[1] = (byte)stream.ReadByte();
            pos[0] = (byte)stream.ReadByte();

            return value;
        }
        public override long ReadInt64(Stream stream)
        {


#if INT64_DOUBLE
            return (long)ReadDouble(stream);
#endif

            long value;
            byte* pos = (byte*)&value;

            pos[7] = (byte)stream.ReadByte();
            pos[6] = (byte)stream.ReadByte();
            pos[5] = (byte)stream.ReadByte();
            pos[4] = (byte)stream.ReadByte();
            pos[3] = (byte)stream.ReadByte();
            pos[2] = (byte)stream.ReadByte();
            pos[1] = (byte)stream.ReadByte();
            pos[0] = (byte)stream.ReadByte();

            return value;
        }
        public override float ReadFloat(Stream stream)
        {

            float value;
            byte* pos = (byte*)&value;

            pos[3] = (byte)stream.ReadByte();
            pos[2] = (byte)stream.ReadByte();
            pos[1] = (byte)stream.ReadByte();
            pos[0] = (byte)stream.ReadByte();

            return value;
        }


        public override unsafe double ReadDouble(byte* stream, int* pos, int length)
        {
            const int valLen = 8;

            double value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }
            var startPos = stream + *pos;

            valpos[7] = startPos[0];
            valpos[6] = startPos[1];
            valpos[5] = startPos[2];
            valpos[4] = startPos[3];
            valpos[3] = startPos[4];
            valpos[2] = startPos[5];
            valpos[1] = startPos[6];
            valpos[0] = startPos[7];

            *pos += valLen; 

            return value;
        }
        public override unsafe float ReadFloat(byte* stream, int* pos, int length)
        {

            const int valLen = 4;

            float value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }
            var startPos = stream + *pos;

            valpos[3] = startPos[0];
            valpos[2] = startPos[1];
            valpos[1] = startPos[2];
            valpos[0] = startPos[3];

            *pos += valLen; 

            return value;
        }
        public override unsafe short ReadInt16(byte* stream, int* pos, int length)
        {

            const int valLen = 2;

            short value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }
            var startPos = stream + *pos;

            valpos[1] = startPos[0];
            valpos[0] = startPos[1];

            *pos += valLen; 

            return value;
        }
        public override unsafe int ReadInt32(byte* stream, int* pos, int length)
        {

            const int valLen = 4;

            int value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }
            var startPos = stream + *pos;

            valpos[3] = startPos[0];
            valpos[2] = startPos[1];
            valpos[1] = startPos[2];
            valpos[0] = startPos[3];

            *pos += valLen; 

            return value;
        }
        public override unsafe long ReadInt64(byte* stream, int* pos, int length)
        {

#if INT64_DOUBLE
            return (long)ReadDouble(stream, pos, length);
#endif

            const int valLen = 8;

            long value;
            byte* valpos = (byte*)&value;

            if (((*pos) + valLen) >= length)
            {
                throw new IndexOutOfRangeException();
            }
            var startPos = stream + *pos;

            valpos[7] = startPos[0];
            valpos[6] = startPos[1];
            valpos[5] = startPos[2];
            valpos[4] = startPos[3];
            valpos[3] = startPos[4];
            valpos[2] = startPos[5];
            valpos[1] = startPos[6];
            valpos[0] = startPos[7];

            *pos += valLen; 

            return value;
        }



        public override void WriteDouble(Stream stream, double value)
        {
            byte* pos = (byte*)&value;
            stream.WriteByte(pos[7]);
            stream.WriteByte(pos[6]);
            stream.WriteByte(pos[5]);
            stream.WriteByte(pos[4]);
            stream.WriteByte(pos[3]);
            stream.WriteByte(pos[2]);
            stream.WriteByte(pos[1]);
            stream.WriteByte(pos[0]);

        }
        public override void WriteFloat(Stream stream, float value)
        {
            byte* pos = (byte*)&value;
            stream.WriteByte(pos[3]);
            stream.WriteByte(pos[2]);
            stream.WriteByte(pos[1]);
            stream.WriteByte(pos[0]);
        }
        public override void WriteInt16(Stream stream, short value)
        {
            byte* pos = (byte*)&value;
            stream.WriteByte(pos[1]);
            stream.WriteByte(pos[0]);
        }
        public override void WriteInt32(Stream stream, int value)
        {
            byte* pos = (byte*)&value;
            stream.WriteByte(pos[3]);
            stream.WriteByte(pos[2]);
            stream.WriteByte(pos[1]);
            stream.WriteByte(pos[0]);
        }
        public override void WriteInt64(Stream stream, long value)
        {
#if INT64_DOUBLE
            WriteDouble(stream, value);
            return;
#endif

            byte* pos = (byte*)&value;
            stream.WriteByte(pos[7]);
            stream.WriteByte(pos[6]);
            stream.WriteByte(pos[5]);
            stream.WriteByte(pos[4]);
            stream.WriteByte(pos[3]);
            stream.WriteByte(pos[2]);
            stream.WriteByte(pos[1]);
            stream.WriteByte(pos[0]);
        }


    }

    #endregion


    #region 接口定义

    public interface ITypeSerializerOperation
    {
        int ReadLength(Stream stream);
        void WriteLength(Stream stream, int value);


        void ReadValue(byte[] stream, out uint value);
        void ReadValue(byte[] stream, out ushort value);
        void ReadValue(byte[] stream, out ulong value);
        void WriteValue(byte[] stream, uint value);

        void ReadValue(Stream stream, out char value);
        void ReadValue(Stream stream, out DataTable value);
        void ReadValue(Stream stream, out byte[] value);
        void ReadValue(Stream stream, out byte value);
        void ReadValue(Stream stream, out double value);
        void ReadValue(Stream stream, out long value);
        void ReadValue(Stream stream, out TimeSpan value);
        void ReadValue(Stream stream, out uint value);
        void ReadValue(Stream stream, out string value);
        void ReadValue(Stream stream, out short value);
        void ReadValue(Stream stream, out sbyte value);
        void ReadValue(Stream stream, out int value);
        void ReadValue(Stream stream, out Guid value);
        void ReadValue(Stream stream, out float value);
        void ReadValue(Stream stream, out decimal value);
        void ReadValue(Stream stream, out DateTime value);
        void ReadValue(Stream stream, out ulong value);
        void ReadValue(Stream stream, out ushort value);
        void ReadValue(Stream stream, out bool value);


        unsafe void ReadValue(byte* stream, int* pos, int length, out char value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out decimal value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out float value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out int value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out sbyte value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out string value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out uint value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out ushort value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out ulong value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out TimeSpan value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out short value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out long value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out Guid value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out double value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out DateTime value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out byte[] value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out byte value);
        unsafe void ReadValue(byte* stream, int* pos, int length, out bool value);


        void WriteValue(Stream stream, string value);
        void WriteValue(Stream stream, TimeSpan value);
        void WriteValue(Stream stream, ulong value);
        void WriteValue(Stream stream, ushort value);
        void WriteValue(Stream stream, uint value);
        void WriteValue(Stream stream, decimal value);
        void WriteValue(Stream stream, double value);
        void WriteValue(Stream stream, Guid value);
        void WriteValue(Stream stream, int value);
        void WriteValue(Stream stream, long value);
        void WriteValue(Stream stream, sbyte value);
        void WriteValue(Stream stream, short value);
        void WriteValue(Stream stream, float value);
        void WriteValue(Stream stream, char value);
        void WriteValue(Stream stream, DataTable value);
        void WriteValue(Stream stream, DateTime value);
        void WriteValue(Stream stream, byte[] value);
        void WriteValue(Stream stream, byte value);
        void WriteValue(Stream stream, bool value);


    }


    #endregion


}