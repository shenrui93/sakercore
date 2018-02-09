/***************************************************************************
 * 
 * 创建时间：   2016/6/10 12:10:29
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
using SakerCore.Serialization.BigEndian;

namespace SakerCore.Serialization.BaseType
{
    /// <summary>
    /// GUID的类型序列化器
    /// </summary>
    internal class GUIDTypeSerialize : TypeSerializationBase<Guid>
    { 
        public override Guid Deserialize(Stream stream)
        {
           return Read(stream);
        }
        public override void Serialize(Guid obj, Stream stream)
        {
            Write(obj, stream);
        }


        public static void Write(Guid obj, Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static Guid Read(Stream stream)
        {
            Guid v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public GUIDTypeSerialize()
        {
            this._serializerType = typeof(Guid);
            this._serializerName = this._serializerType.FullName;
        }



        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, Guid> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<Guid, System.IO.Stream> writer = Write;
            return writer.Method;
        }

        public override unsafe Guid UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static Guid UnsafeRead(byte* stream, int* pos, int length)
        {
            Guid v;
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
