using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SakerCore.Serialization.BaseType
{
    internal class DecimalTypeSerialize : TypeSerializationBase<decimal>
    {
        public override void Serialize(decimal obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override decimal Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        }

        public static void Write(decimal obj, System.IO.Stream stream)
        {
           SakerCore.Serialization. BigEndian.BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static decimal Read(System.IO.Stream stream)
        {
            decimal value;
            SakerCore.Serialization.BigEndian.BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
            return value;
        }

        public DecimalTypeSerialize()
        {
            this._serializerType = typeof(decimal);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, decimal> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<decimal, System.IO.Stream> writer = Write;
            return writer.Method;
        }


        public override unsafe decimal UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static decimal UnsafeRead(byte* stream, int* pos, int length)
        {
            decimal v;
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
