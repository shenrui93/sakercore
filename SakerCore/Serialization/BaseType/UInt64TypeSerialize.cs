using System;
using SakerCore.Serialization.BigEndian;

namespace SakerCore.Serialization.BaseType
{
    internal class UInt64TypeSerialize : TypeSerializationBase<ulong>
    {
        public override void Serialize(ulong obj, System.IO.Stream stream)
        {
            Write((ulong)obj, stream);
        }
        public override ulong Deserialize(System.IO.Stream stream)
        {
            return Read(stream);

        }

        public static void Write(ulong obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static ulong Read(System.IO.Stream stream)
        {
            ulong v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public UInt64TypeSerialize()
        {
            this._serializerType = typeof(ulong);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, ulong> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<ulong, System.IO.Stream> writer = Write;
            return writer.Method;
        }

        public override unsafe ulong UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static ulong UnsafeRead(byte* stream, int* pos, int length)
        {
            ulong v;
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
