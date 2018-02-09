using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SakerCore.Serialization.BigEndian;


namespace SakerCore.Serialization.BaseType
{
    internal class CharTypeSerialize : TypeSerializationBase<char>
    {

        public override void Serialize(char obj, System.IO.Stream stream)
        {
            Write((char)obj, stream);
        }
        public override char Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        }

        public static void Write(char obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static char Read(System.IO.Stream stream)
        {
            char v;
            SakerCore.Serialization.BigEndian.BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out v);
            return v;
        }

        public CharTypeSerialize()
        {
            this._serializerType = typeof(char);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, char> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<char, System.IO.Stream> writer = Write;
            return writer.Method;
        }


        public override unsafe char UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static char UnsafeRead(byte* stream, int* pos, int length)
        {
            char v;
            BigEndianPrimitiveTypeSerializer
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
