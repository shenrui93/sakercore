using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SakerCore.Serialization.BigEndian;

namespace SakerCore.Serialization.BaseType
{
    internal class Int16TypeSerialize : TypeSerializationBase<short>
    {
        public override void Serialize(short obj, System.IO.Stream stream)
        {
            Write((short)obj, stream);
        }
        public override short Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        }
        public static void Write(short obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static short Read(System.IO.Stream stream)
        {
            short v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }
        public Int16TypeSerialize()
        {
            this._serializerType = typeof(short);
            this._serializerName = _serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, short> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<short, System.IO.Stream> writer = Write;
            return writer.Method;
        }

        public override unsafe short UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static short UnsafeRead(byte* stream, int* pos, int length)
        {
            short v;
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
