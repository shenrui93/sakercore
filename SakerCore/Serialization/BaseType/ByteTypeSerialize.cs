using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SakerCore.Serialization.BaseType
{
    internal class ByteTypeSerialize : TypeSerializationBase<byte>
    {

        public override void Serialize(byte obj, System.IO.Stream stream)
        {
            Write((byte)obj, stream);
        }
        public override byte Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        }

        public static void Write(byte obj, System.IO.Stream stream)
        {
            stream.WriteByte(obj);
        }
        public static byte Read(System.IO.Stream stream)
        {
            return (byte)stream.ReadByte();
        }

        public ByteTypeSerialize()
        {
            this._serializerType = typeof(byte);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, byte> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<byte, System.IO.Stream> writer = Write;
            return writer.Method;
        }

        public override unsafe byte UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static byte UnsafeRead(byte* stream, int* pos, int length)
        {
            byte v;
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
