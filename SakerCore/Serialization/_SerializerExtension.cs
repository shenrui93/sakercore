/***************************************************************************
 * 
 * 创建时间：   2017/1/13 9:50:32
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   序列化器扩展
 * 
 * *************************************************************************/

#define BigEndian

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
#if BigEndian
using SakerCore.Serialization.BigEndian; 
#else
using Uyi.Serialization.LittleEndian;
#endif

namespace SakerCore.Serialization.Extension
{
    /// <summary>
    /// 类_SerializerExtension的注释信息
    /// </summary>
    public static class _SerializerExtension
    {
        private static readonly Encoding defaultstringencoding = Encoding.UTF8;




        /// <summary>
        /// 读取一个有符号的 16 位整数
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的有符号的 16 位整数</returns>
        public static short ReadInt16(this Stream stream)
        {
            short value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个有符号的 32 位整数
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的有符号的 32 位整数</returns>
        public static int ReadInt32(this Stream stream)
        {
            int value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个有符号的 64 位整数
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的有符号的 64 位整数</returns>
        public static long ReadInt64(this Stream stream)
        {
            long value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个 decimal 浮点数据
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的 decimal 浮点数据</returns>
        public static decimal ReadDecimal(this Stream stream)
        {
            decimal value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个 float 浮点数据
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的 float 浮点数据</returns>
        public static float ReadFloat(this Stream stream)
        {
            float value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个 float 浮点数据
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的 float 浮点数据</returns>
        public static float ReadSingle(this Stream stream)
        {
            float value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个 double 浮点数据
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的 double 浮点数据</returns>
        public static double ReadDouble(this Stream stream)
        {
            double value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个字符串
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的字符串</returns>
        public static string ReadString(this Stream stream)
        {

            string value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个 DateTime 时间数据
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的 DateTime 时间数据</returns>
        public static DateTime ReadDateTime(this Stream stream)
        {

            DateTime value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个 TimeSpan 时间间隔数据
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的 TimeSpan 时间间隔数据</returns>
        public static TimeSpan ReadTimeSpan(this Stream stream)
        {

            TimeSpan value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个 Guid 标识符数据
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的 TimeSpan 时间间隔数据</returns>
        public static Guid ReadGuid(this Stream stream)
        {

            Guid value;
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.ReadValue(stream, out value);
#endif
            return value;
        }
        /// <summary>
        /// 读取一个 DataTable 数据表数据
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的 DataTable 数据表数据</returns>
        public static DataTable ReadDataTable(this Stream stream)
        {  
          return  DataTableTypeSerializer.Deserializer(stream); 
        }
        /// <summary>
        /// 读取一个表示数据长度的数据
        /// </summary>
        /// <param name="stream">从中读取的流</param>
        /// <returns>返回读取到的长度信息数据</returns>
        /// <exception cref="InvalidDataException">当遇到文件流的末尾时引发</exception>
        public static int ReadLengthData(this Stream stream)
        {
            var len = stream.ReadByte();
            if (len < 0)
            {
                throw new InvalidDataException("遇到文件尾");
            }

            if (len == 254)
            {
                len = stream.ReadUInt16();
            }
            else if (len == 255)
            {
                len = stream.ReadInt32();
            }

            return len;
        }
        /// <summary>
        /// 读取一个有符号的字节数据
        /// </summary>
        /// <param name="stream">读取流</param>
        /// <returns>返回读取到的数据</returns>
        public static sbyte ReadSByte(this Stream stream)
        {
            int ret = stream.ReadByte();
            if (ret < 0)
            {
                throw new InvalidDataException("遇到文件尾");
            }
            return (sbyte)ret;
        }
        /// <summary>
        /// 读取一个无符号的 16 位整数
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的无符号的 16 位整数</returns>
        public static ushort ReadUInt16(this Stream stream)
        {
            return (ushort)stream.ReadInt16();
        }
        /// <summary>
        /// 读取一个无符号的 32 位整数
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的无符号的 32 位整数</returns>
        public static uint ReadUInt32(this Stream stream)
        {
            return (uint)stream.ReadInt32();
        }
        /// <summary>
        /// 读取一个无符号的 64 位整数
        /// </summary>
        /// <param name="stream">希望读取的流</param>
        /// <returns>返回读取到的无符号的 64 位整数</returns>
        public static ulong ReadUInt64(this Stream stream)
        {
            return (ulong)stream.ReadInt64();
        }


        /// <summary>
        /// 写入一个表示数据长度的数据
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="len">希望写入的长度数据</param>
        public static void WriteLengthData(this Stream stream, int len)
        {
            //写入消息长度
            if (len < 254)
            {
                //直接写消息长度
                stream.WriteValue((byte)len);
            }
            else if (len <= ushort.MaxValue)
            {
                //直接写消息长度
                stream.WriteValue(254);
                stream.WriteValue((short)len);
            }
            else
            {
                //直接写消息长度
                stream.WriteValue(255);
                stream.WriteValue(len);
            }
        }


        /// <summary>
        /// 写入一个无符号的 64 位整数
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, ulong value)
        {
            stream.WriteValue((long)value);
        }
        /// <summary>
        /// 写入一个无符号的 32 位整数
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, uint value)
        {
            stream.WriteValue((int)value);
        }
        /// <summary>
        /// 写入一个无符号的 16 位整数
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, ushort value)
        {
            stream.WriteValue((short)value);
        }
        /// <summary>
        /// 写入一个有符号的字节数据
        /// </summary>
        /// <param name="stream">写入到的流</param>
        /// <param name="value">写入的数据</param>
        public static void WriteValue(this Stream stream, sbyte value)
        {
            stream.WriteValue((byte)value);
        }
        /// <summary>
        /// 写入一个 Guid 标识符数据
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, byte value)
        {
            stream.WriteByte(value);
        }
        /// <summary>
        /// 写入一个 Guid 标识符数据
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, DataTable value)
        {
            DataTableTypeSerializer.Serializer(stream, value);
        }
        /// <summary>
        /// 写入一个 Guid 标识符数据
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, Guid value)
        {
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }
        /// <summary>
        /// 写入一个 TimeSpan 时间间隔数据
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, TimeSpan value)
        {
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }
        /// <summary>
        /// 写入一个 DateTime 时间数据
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, DateTime value)
        {
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }
        /// <summary>
        /// 写入一个字符串
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, string value)
        {
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }
        /// <summary>
        /// 写入一个有符号的 16 位整数
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, short value)
        {
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }
        /// <summary>
        /// 写入一个有符号的 32 位整数
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, int value)
        {

#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }
        /// <summary>
        /// 写入一个有符号的 64 位整数
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, long value)
        {
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }
        /// <summary>
        /// 写入一个 decimal 浮点数据
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, decimal value)
        {
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }
        /// <summary>
        /// 写入一个 float 浮点数据
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, float value)
        {
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }
        /// <summary>
        /// 写入一个 double 浮点数据
        /// </summary>
        /// <param name="stream">希望写入的流</param>
        /// <param name="value">希望写入的值</param>
        public static void WriteValue(this Stream stream, double value)
        {
#if BigEndian
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value);
#else
            LittleEndianPrimitiveTypeSerializerBase.Instance.WriteValue(stream, value);
#endif
        }











    }


}
