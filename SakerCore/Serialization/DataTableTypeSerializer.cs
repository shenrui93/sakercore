/***************************************************************************
 * 
 * 创建时间：   2016/7/7 10:32:22
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供对数据库表 DataTable 的数据传输服务
                目前数据表的数据传输不传输 DBNull 数据，全部转换为默认值
                该类的序列化仅支持大端数据传输序列
 * 
 * *************************************************************************/

using System;
using System.Data;
using System.IO;
using System.Text;
using SakerCore.Serialization.BigEndian;
using SakerCore.Serialization.Extension;


namespace SakerCore.Serialization
{
    /// <summary>
    /// 提供对数据库表 DataTable 的数据传输服务
    /// </summary>
    public class DataTableTypeSerializer
    {

        const byte IsDBNullTypeCode = 255;

        internal static DataTable Deserializer(Stream stream)
        {
            //读入一个字节来判断对象 NULL 状态
            byte isnotnull;
           BigEndianPrimitiveTypeSerializer.Instance. ReadValue(stream, out isnotnull);
            if (isnotnull != 1) return null;
            var tb = new DataTable();

            //开始反序列化对象操作
            Deserializer_ReadColumnArray(stream, tb.Columns);
            Deserializer_ReadRowsArray(stream, tb);

            return tb;

        }
        internal static void Serializer(Stream stream, DataTable value)
        {
            //判断对象是否为null
            if (value == null)
            {
                BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (byte)0);
                return;
            }
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (byte)1);

            byte[] typecodeArray;

            //开始写入数据表列相关，并且临时编排类型序列化方法调用顺序
            Serializer_WriterColumnArray(stream, value.Columns, out typecodeArray);
            //写入数据行数据
            Serializer_WriterRowsArray(stream, value.Rows, typecodeArray);
        }

        #region 序列化操作方法

        /***********         序列化操作方法       ***************/

        private static void Serializer_WriterRowsArray(Stream stream, DataRowCollection rows, byte[] typecodeArray)
        {
            var len = rows.Count;
            stream.WriteLengthData(len);

            //开始写行数据
            foreach (DataRow dr in rows)
            {
                Serializer_WriterRows(stream, dr, typecodeArray);
            }

        }
        private static void Serializer_WriterRows(Stream stream, DataRow dr, byte[] typecodeArray)
        {
            byte index = 0;
            //对单行数据来进行数据写入
            foreach (var o in dr.ItemArray)
            {
                byte typecode;
                if (Convert.IsDBNull(o))
                {
                    typecode = IsDBNullTypeCode;
                }
                else
                {
                    typecode = typecodeArray[index];
                }

                index++;

                //写入数据，保证最大兼容性 
                SetDataVale(stream, typecode, o);
            }
        }
        private static void Serializer_WriterColumnArray(Stream stream, DataColumnCollection columns, out byte[] typecodeArray)
        {
            typecodeArray = new byte[columns.Count];
            //写入数据列个数，最多255个字段列 
            stream.WriteByte((byte)columns.Count);

            int index = 0;
            //按照顺序写入列结构
            foreach (DataColumn col in columns)
            {
                stream.WriteValue(col.ColumnName);
                var type = GetTypeCode(col.DataType);

                //写入类别信息
                stream.WriteValue(type);

                typecodeArray[index++] = type;
            }
        }


        #endregion

        #region 反序列化操作方法


        /***********         反序列化操作方法       ***************/

        private static void Deserializer_ReadRowsArray(Stream stream, DataTable tb)
        {
            int len = stream.ReadLengthData();

            //开始读取行数据
            for (var index = 0; index < len; index++)
            {
                var newrow = tb.NewRow();
                Deserializer_ReadRows(stream, ref newrow);
                tb.Rows.Add(newrow);
            }
        }
        private static void Deserializer_ReadRows(Stream stream, ref DataRow newrow)
        {
            var count = newrow.ItemArray.Length;

            for (var index = 0; index < count; index++)
            {
                //读取字符串数据写入
                object val = GetDataVale(stream);
                newrow[index] = val;
            }

        }
        private static void Deserializer_ReadColumnArray(Stream stream, DataColumnCollection columns)
        {
            //读入数据列个数，最多255个字段列 
            int count = stream.ReadByte();
            string c_name;
            while (--count >= 0)
            {
                c_name = stream.ReadString();
                var colType = GetCodeType((byte)stream.ReadByte());
                columns.Add(c_name,colType);
            }
        }


        #endregion



        static byte GetTypeCode(Type type)
        {
            switch (type.FullName)
            {
                case "System.Byte": return 1;
                case "System.SByte": return 2;
                case "System.Int16": return 3;
                case "System.UInt16": return 4;
                case "System.Int32": return 5;
                case "System.UInt32": return 6;
                case "System.Int64": return 7;
                case "System.UInt64": return 8;
                case "System.Single": return 9;
                case "System.Double": return 10;
                case "System.Guid": return 11;
                case "System.DateTime": return 12;
                case "System.TimeSpan": return 13;
                case "System.Decimal": return 14;

                default: return 200;
            }
        }
        static Type GetCodeType(byte typeCode)
        {

            switch (typeCode)
            {
                case 1: return typeof(System.Byte);
                case 2: return typeof(System.SByte);
                case 3: return typeof(System.Int16);
                case 4: return typeof(System.UInt16);
                case 5: return typeof(System.Int32);
                case 6: return typeof(System.UInt32);
                case 7: return typeof(System.Int64);
                case 8: return typeof(System.UInt64);
                case 9: return typeof(System.Single);
                case 10: return typeof(System.Double);
                case 11: return typeof(System.Guid);
                case 12: return typeof(System.DateTime);
                case 13: return typeof(System.TimeSpan);
                case 14: return typeof(System.Decimal);

                default: return typeof(string);
            }
        }
        static object GetDataVale(Stream stream)
        {
            int typecode = stream.ReadByte();
            switch (typecode)
            {
                case 1: { return stream.ReadByte(); }
                case 2: { return stream.ReadSByte(); }
                case 3: { return stream.ReadInt16(); }
                case 4: { return stream.ReadUInt16(); }
                case 5: { return stream.ReadInt32(); }
                case 6: { return stream.ReadUInt32(); }
                case 7: { return stream.ReadInt64(); }
                case 8: { return stream.ReadUInt64(); }
                case 9: { return stream.ReadSingle(); }
                case 10: { return stream.ReadDouble(); }
                case 11: { return stream.ReadGuid(); }
                case 12: { return stream.ReadDateTime(); }
                case 13: { return stream.ReadTimeSpan(); }
                case 14: { return stream.ReadDecimal(); }

                case IsDBNullTypeCode: return DBNull.Value;
                default:
                    {
                        string value;
                        BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
                        return value;
                    }
            }
        }
        static void SetDataVale(Stream stream, byte typecode, object value)
        {
            stream.WriteByte(typecode);
            switch (typecode)
            {
                case 1: { stream.WriteValue((System.Byte)value); break; }
                case 2: { stream.WriteValue((System.SByte)value); break; }
                case 3: { stream.WriteValue((System.Int16)value); break; }
                case 4: { stream.WriteValue((System.UInt16)value); break; }
                case 5: { stream.WriteValue((System.Int32)value); break; }
                case 6: { stream.WriteValue((System.UInt32)value); break; }
                case 7: { stream.WriteValue((System.Int64)value); break; }
                case 8: { stream.WriteValue((System.UInt64)value); break; }
                case 9: { stream.WriteValue((System.Single)value); break; }
                case 10: { stream.WriteValue((System.Double)value); break; }
                case 11: { stream.WriteValue((System.Guid)value); break; }
                case 12: { stream.WriteValue((System.DateTime)value); break; }
                case 13: { stream.WriteValue((System.TimeSpan)value); break; }
                case 14: { stream.WriteValue((System.Decimal)value); break; }

                case IsDBNullTypeCode: { return; }
                default: { BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, value.ToString()); return; }
            }
        }


        #region UyiDataTableSerializerCodeBulid


        /// <summary>
        /// 类UyiDataTableSerializerCodeBulid的注释信息
        /// </summary>
        public class DataTableSerializerCodeBulid
        {
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static string bulidCode()
            {
                StringBuilder strb_g = new StringBuilder();
                StringBuilder strb_w = new StringBuilder();
                StringBuilder strb_r = new StringBuilder();
                StringBuilder strb_ct = new StringBuilder();

                int index = 1;
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(byte), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(sbyte), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(short), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(ushort), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(int), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(uint), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(long), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(ulong), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(float), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(double), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(Guid), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(DateTime), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(TimeSpan), ref index);
                s_r(strb_ct, strb_g, strb_w, strb_r, typeof(decimal), ref index);


                return $@"
static byte GetTypeCode(Type type)
        {{
            switch (type.FullName)
            {{ 
                {strb_g.ToString()}
                default: return 200;
            }}
        }}

        static Type GetCodeType(byte typeCode)
        {{

            switch (typeCode)
            {{
                {strb_ct.ToString()}
                default: return typeof(string);
            }}
        }}
        static object GetDataVale(Stream stream)
        {{
            int typecode = stream.ReadByte();
            switch (typecode)
            {{ 
                {strb_r.ToString()}
                case {nameof(IsDBNullTypeCode)}:  return DBNull.Value;
                default:
                    {{
                        string value;
                        ReadValue(stream, out value);
                        return value;
                    }}
            }}
        }}
        static void SetDataVale(Stream stream, byte typecode, object value)
        {{
            stream.WriteByte(typecode);
            switch (typecode)
            {{ 
                {strb_w.ToString()}
                case {nameof(IsDBNullTypeCode)}: {{ return; }}
                default: {{ WriteValue(stream, value.ToString()); return; }}
            }}
        }} 
";
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="strb_ct"></param>
            /// <param name="strb_g"></param>
            /// <param name="strb_w"></param>
            /// <param name="strb_r"></param>
            /// <param name="type"></param>
            /// <param name="index"></param>
            public static void s_r(
                StringBuilder strb_ct,
                StringBuilder strb_g,
                StringBuilder strb_w,
                StringBuilder strb_r,
                Type type,
                ref int index)
            {
                strb_ct.AppendLine($@"case {index}: return typeof({type.FullName});");
                strb_g.AppendLine($@"case ""{type.FullName}"" : return   {index};");
                strb_r.AppendLine($@"case {index}: {{ return stream.Read{type.Name}(); }}");
                strb_w.AppendLine($@"case {index}: {{ stream.WriteValue(({type.FullName})value); break; }}");

                index++;
            }

        }

        #endregion


    }
}
