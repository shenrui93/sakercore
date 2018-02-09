using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;



namespace SakerCore.Serialization.BaseType
{
    /// <summary>
    /// 为字节数组的序列化单独创建一个序列化器，以增加在序列化和反序列化字节数组的系统性能
    /// </summary>
    internal class ByteArrayTypeSerialize : TypeSerializationBase<byte[]>
    {
        public override void Serialize(byte[] obj, System.IO.Stream stream)
        {
            Write(obj as byte[], stream);
        }
        public override byte[] Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        }

        public static void Write(byte[] obj, System.IO.Stream stream)
        {
            int length = obj == null ? 0 : obj.Length; 
            if (length < 254)
            {
                stream.WriteByte((byte)length);
            }
            else if (length < ushort.MaxValue)
            {
                stream.WriteByte(254);
                SakerCore.Serialization.BigEndian.BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (ushort)length);
            }
            else
            {
                stream.WriteByte(255);
                SakerCore.Serialization.BigEndian.BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, length);

            }
            if (length > 0)
                stream.Write(obj, 0, length); 
        }
        public static byte[] Read(System.IO.Stream stream)
        {
            var length = stream.ReadByte();
            if (length == 254)
            {
                ushort t;
                SakerCore.Serialization.BigEndian.BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out t);
                length = t;
            }
            else if (length == 255)
            {
                SakerCore.Serialization.BigEndian.BigEndianPrimitiveTypeSerializer.Instance. ReadValue(stream, out length);
            }
            byte[] data = new byte[length];
            if (length > 0)
                stream.Read(data, 0, length);
            return data;
        }

        /// <summary>
        /// 创建为字节数组的序列化器
        /// </summary>
        public ByteArrayTypeSerialize()
        {
            this._serializerType = typeof(byte[]);
            this._serializerName = this._serializerType.FullName;
        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, byte[]> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<byte[], System.IO.Stream> writer = Write;
            return writer.Method;
        }


        public override unsafe byte[] UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static byte[] UnsafeRead(byte* stream, int* pos, int length)
        {
            byte[] v;
            BigEndian.BigEndianPrimitiveTypeSerializer
                .Instance.ReadValue(stream, pos, length, out v);
            return v;

        }
        public unsafe override MethodInfo GetUnsafeReaderMethod()
        {
            delUnsafeReaderMethod reader = UnsafeRead;
            return reader.Method;
        }



    }
}
