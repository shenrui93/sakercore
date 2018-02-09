using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SakerCore.Serialization.BigEndian;


namespace SakerCore.Serialization.BaseType
{
    internal class StringTypeSerialize : TypeSerializationBase<string>
    {

        const char Eof = '\0';

        static StringTypeSerialize _instance;
        internal static StringTypeSerialize Instance
        {
            get
            {
                return _instance ?? (_instance = new StringTypeSerialize());
            }
        }

        public override void Serialize(string obj, Stream stream)
        {
            Write((string)obj, stream);
        }
        public override string Deserialize(Stream stream)
        {
            return Read(stream);
        }

        public static void Write(string obj, Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static string Read(Stream stream)
        {
            string value;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out value);
            return value;
        }

        public StringTypeSerialize()
        {
            this._serializerType = typeof(string);
            this._serializerName = this._serializerType.FullName;
        }



        public override MethodInfo GetReaderMethod()
        {
            Func<Stream, string> reader = Read;
            return reader.Method;
        }
        public override MethodInfo GetWriterMethod()
        {
            Action<string, Stream> writer = Write;
            return writer.Method;
        }

        public override unsafe string UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static string UnsafeRead(byte* stream, int* pos, int length)
        {
            string v;
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
