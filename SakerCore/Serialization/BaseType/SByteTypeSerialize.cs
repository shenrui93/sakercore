/***************************************************************************
 * 
 * 创建时间：   2016/3/8 14:42:26
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.Serialization.BaseType
{
    /// <summary>
    /// 类SByteTypeSerialize的注释信息
    /// </summary>
    internal class SByteTypeSerialize : TypeSerializationBase<sbyte>
    {
        public override void Serialize(sbyte obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override sbyte Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        }

        public static void Write(sbyte obj, System.IO.Stream stream)
        {
            stream.WriteByte((byte)obj);
        }
        public static sbyte Read(System.IO.Stream stream)
        {
            return (sbyte)stream.ReadByte();
        }

        public SByteTypeSerialize()
        {
            this._serializerType = typeof(sbyte);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<Stream, sbyte> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<sbyte, System.IO.Stream> writer = Write;
            return writer.Method;
        }

        public override unsafe sbyte UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static sbyte UnsafeRead(byte* stream, int* pos, int length)
        {
            sbyte v;
            BigEndian.BigEndianPrimitiveTypeSerializer
                .Instance.ReadValue(stream, pos, length, out v);
            return v;

        }
        public unsafe override System.Reflection.MethodInfo GetUnsafeReaderMethod()
        {
            delUnsafeReaderMethod reader = UnsafeRead;
            return reader.Method;
        }

    }
}
