using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace SakerCore.Serialization.BaseType
{
    internal class BooleanTypeSerialize : TypeSerializationBase<bool>
    {
        public override void Serialize(bool obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override bool Deserialize(System.IO.Stream stream)
        {
            return Read(stream);

        }

        public static void Write(bool obj, System.IO.Stream stream)
        {
            SakerCore.Serialization.BigEndian.BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj); 
        }
        public static bool Read(System.IO.Stream stream)
        {
            return stream.ReadByte() > 0;
        }

        public BooleanTypeSerialize()
        {
            this._serializerType = typeof(bool);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, bool> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<bool, System.IO.Stream> writer = Write;
            return writer.Method;
        }


        public override unsafe bool UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static bool UnsafeRead(byte* stream, int* pos, int length)
        {
            bool v;
            BigEndian.BigEndianPrimitiveTypeSerializer
                .Instance.ReadValue(stream, pos, length, out v);
            return v;

        }
        public unsafe override MethodInfo GetUnsafeReaderMethod()
        {
            delUnsafeReaderMethod reader = UnsafeRead;
            return reader.Method;
        }


    }
}
