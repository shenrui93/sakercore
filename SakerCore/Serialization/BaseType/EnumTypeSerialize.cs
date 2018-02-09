using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SakerCore.Serialization.BaseType
{
    internal class EnumTypeSerialize : TypeSerializationBase<int>
    {
        static EnumTypeSerialize _instance;

        public static EnumTypeSerialize Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EnumTypeSerialize();
                return _instance;
            }
        }


        public override void Serialize(int obj, System.IO.Stream stream)
        {
            Write((byte)obj, stream);
        }
        public override int Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        }

        internal static void Write(byte val, System.IO.Stream stream)
        { 
            stream.WriteByte(val);
        }
        internal static byte Read(System.IO.Stream stream)
        {
            return (byte)stream.ReadByte();
        }

        private EnumTypeSerialize()
        {
            this._serializerType = typeof(Enum);
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


        public override unsafe int UnsafeDeserialize(byte* stream, int* pos, int length)
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
            delUnsafeReaderMethod<byte> reader = UnsafeRead;
            return reader.Method;
        }

    }
}
