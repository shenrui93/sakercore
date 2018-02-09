using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SakerCore.Serialization.BigEndian;


namespace SakerCore.Serialization.BaseType
{
    internal class DateTimeTypeSerialize : TypeSerializationBase<DateTime>
    {
        public override void Serialize(DateTime obj, System.IO.Stream stream)
        {
            Write((DateTime)obj, stream);
        }
        public override DateTime Deserialize(System.IO.Stream stream)
        {
            return Read(stream);

        }


        public static void Write(DateTime obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static DateTime Read(System.IO.Stream stream)
        {
            DateTime v;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public DateTimeTypeSerialize()
        {
            
            this._serializerType = typeof(DateTime);
            this._serializerName = this._serializerType.FullName;
        }



        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, DateTime> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<DateTime, System.IO.Stream> writer = Write;
            return writer.Method;
        }


        public override unsafe DateTime UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static DateTime UnsafeRead(byte* stream, int* pos, int length)
        {
            DateTime v;
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
