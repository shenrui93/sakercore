using System;
using SakerCore.Serialization.BigEndian;

namespace SakerCore.Serialization.BaseType
{
    internal class UInt32TypeSerialize : TypeSerializationBase<uint>
    {
        public override void Serialize(uint obj, System.IO.Stream stream)
        {
            Write((uint)obj, stream);
        }
        public override uint Deserialize(System.IO.Stream stream)
        {
            return Read(stream);

        }

        public static void Write(uint obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static uint Read(System.IO.Stream stream)
        {
            uint v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public UInt32TypeSerialize()
        {
            this._serializerType = typeof(uint);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, uint> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<uint, System.IO.Stream> writer = Write;
            return writer.Method;
        }

        public override unsafe uint UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static uint UnsafeRead(byte* stream, int* pos, int length)
        {
            uint v;
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
