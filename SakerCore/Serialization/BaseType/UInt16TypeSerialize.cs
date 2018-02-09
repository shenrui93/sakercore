using System;
using SakerCore.Serialization.BigEndian;

namespace SakerCore.Serialization.BaseType
{
    internal class UInt16TypeSerialize : TypeSerializationBase<ushort>
    {
        public override void Serialize(ushort obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override ushort Deserialize(System.IO.Stream stream)
        {
            return Read(stream);

        }

        public static void Write(ushort obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static ushort Read(System.IO.Stream stream)
        {
            ushort v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public UInt16TypeSerialize()
        {
            this._serializerType = typeof(ushort);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, ushort> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<ushort, System.IO.Stream> writer = Write;
            return writer.Method;
        }


        public override unsafe ushort UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static ushort UnsafeRead(byte* stream, int* pos, int length)
        {
            ushort v;
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
