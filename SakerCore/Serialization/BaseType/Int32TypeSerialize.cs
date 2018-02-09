using System;
using SakerCore.Serialization.BigEndian;

namespace SakerCore.Serialization.BaseType
{
    internal class Int32TypeSerialize : TypeSerializationBase<int>
    {
        public override void Serialize(int obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override int Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        }

        public static void Write(int obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static int Read(System.IO.Stream stream)
        {
            int v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public Int32TypeSerialize()
        {
            this._serializerType = typeof(int);
            this._serializerName = _serializerType.FullName;
        }

        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, int> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<int, System.IO.Stream> writer = Write;
            return writer.Method;
        }



        public override unsafe int UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static int UnsafeRead(byte* stream, int* pos, int length)
        {
            int v;
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
