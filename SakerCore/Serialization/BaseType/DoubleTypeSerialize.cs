using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SakerCore.Serialization.BigEndian;


namespace SakerCore.Serialization.BaseType
{
    internal class DoubleTypeSerialize : TypeSerializationBase<double>
    {
        public override void Serialize(double obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override double Deserialize(System.IO.Stream stream)
        {
            return Read(stream);

        }

        public static void Write(double obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static double Read(System.IO.Stream stream)
        {
            double v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, double> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<double, System.IO.Stream> writer = Write;
            return writer.Method;
        }


        public DoubleTypeSerialize()
        {
            this._serializerType = typeof(double);
            this._serializerName = this._serializerType.FullName;
        }



        public override unsafe double UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static double UnsafeRead(byte* stream, int* pos, int length)
        {
            double v;
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
