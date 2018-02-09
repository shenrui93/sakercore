using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SakerCore.Serialization.BigEndian;


namespace SakerCore.Serialization.BaseType
{
    internal class FloatTypeSerialize : TypeSerializationBase<float>
    {
        public override void Serialize(float obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override float Deserialize(System.IO.Stream stream)
        {
            return Read(stream);

        }

        public static void Write(float obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static float Read(System.IO.Stream stream)
        {
            float v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public FloatTypeSerialize()
        {
            this._serializerType = typeof(float);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, float> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<float, System.IO.Stream> writer = Write;
            return writer.Method;
        }

        public override unsafe float UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static float UnsafeRead(byte* stream, int* pos, int length)
        {
            float v;
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
