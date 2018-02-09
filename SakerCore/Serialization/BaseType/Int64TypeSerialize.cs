using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SakerCore.Serialization.BigEndian;

namespace SakerCore.Serialization.BaseType
{
    internal class Int64TypeSerialize : TypeSerializationBase<long>
    {
        public override void Serialize(long obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override long Deserialize(System.IO.Stream stream)
        {
            return Read(stream);

        }

        public static void Write(long obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static long Read(System.IO.Stream stream)
        {
            long v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public Int64TypeSerialize()
        {
            this._serializerType = typeof(long);
            this._serializerName = this._serializerType.FullName;
        }
        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, long> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<long, System.IO.Stream> writer = Write;
            return writer.Method;
        }

        public override unsafe long UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static long UnsafeRead(byte* stream, int* pos, int length)
        {
            long v;
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
