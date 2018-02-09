using System;
using SakerCore.Serialization.BigEndian;

namespace SakerCore.Serialization.DynmaicType
{

#if Version35

    /// <summary>
    /// 数组类型序列化器
    /// </summary>
    internal class ArrayTypeSerialize : TypeSerializationBase<Array>
    {
        private Type baseType;
        private ITypeSerializer serializer;
        //字节数组序列化类型
        static readonly Type byteType = typeof(byte[]);

        public override void Serialize(Array obj, System.IO.Stream stream)
        {
            Array ary = obj as Array;
           // int length = ary == null ? 0 : ary.Length;


            int length = ary == null ? 0 : ary.Length;
            if (length < 254)
            {
                WriteValue(stream, (byte)length);
            }
            else if (length <= ushort.MaxValue)
            {
                WriteValue(stream, (byte)254);
                WriteValue(stream, (ushort)length);
            }
            else
            {
                WriteValue(stream, (byte)255);
                WriteValue(stream, (ushort)length);
            }

            //Int32TypeSerialize.Write(length, stream);
            if (length > 0)
            {
                foreach (var r in ary)
                {
                    serializer.Serialize(r, stream);
                }
            }
        }
        public override Array Deserialize(System.IO.Stream stream)
        { 
            int length = stream.ReadByte();
            if (length == 254)
            {
                ushort t;
                ReadValue(stream, out t);
                length = t;
            }
            if (length == 255)
            {
                ReadValue(stream, out length);
            }

            var array = Array.CreateInstance(baseType, length);
            if (length > 0)
            {
                int index = 0;
                while (index < length)
                {
                    var obj = serializer.Deserialize(stream);
                    array.SetValue(obj, index++);
                }
            }
            return array;
        }

        public ArrayTypeSerialize(Type ArrayType)
        {
            if (!ArrayType.IsArray) throw Error.TypeNotIsArray();
            _serializerType = ArrayType;
            _serializerName = this._serializerType.FullName;
            baseType = this._serializerType.GetElementType();
            serializer = TypeSerializationHelper.GetTypeSerializer(baseType);
        }
 

    }

#else

    /// <summary>
    /// 数组类型序列化器
    /// </summary>
    internal class ArrayTypeSerialize<T> : TypeSerializationBase<T> where T : class
    {
        private static Type baseType;
        static ITypeSerializer serializer;
        static IArrayDeserialize _arrayDeserialize = null;
        static IUnsafeArrayDeserialize _unsafeArrayDeserialize = null;


        //字节数组序列化类型
        static readonly Type byteType = typeof(byte[]);

        public override void Serialize(T obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override T Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        }

        public ArrayTypeSerialize()
        {
            var ArrayType = typeof(T);
            if (!ArrayType.IsArray) throw Error.TypeNotIsArray();
            _serializerType = ArrayType;
            _serializerName = this._serializerType.FullName;
            baseType = this._serializerType.GetElementType();
            serializer = TypeSerializationHelper.GetTypeSerializer(baseType);
            if (baseType.IsClass)
            {
                _arrayDeserialize = Activator.CreateInstance(typeof(ArrayDeserializeCls<>).MakeGenericType(baseType)) as IArrayDeserialize;
                _unsafeArrayDeserialize = Activator.CreateInstance(typeof(UnsafeArrayDeserializeCls<>).MakeGenericType(baseType)) as IUnsafeArrayDeserialize;
            }
            else
            {
                _unsafeArrayDeserialize = Activator.CreateInstance(typeof(UnsafeArrayDeserializeStuct<>).MakeGenericType(baseType)) as IUnsafeArrayDeserialize;
                _arrayDeserialize = Activator.CreateInstance(typeof(ArrayDeserializeStuct<>).MakeGenericType(baseType)) as IArrayDeserialize;
            }
        }

        public static void Write(T obj, System.IO.Stream stream)
        {
            Array ary = obj as Array;
            int length = ary == null ? 0 : ary.Length;
            if (length < 254)
            {
                BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (byte)length);
            }
            else if (length <= ushort.MaxValue)
            {
                BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (byte)254);
                BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (ushort)length);
            }
            else
            {
                BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (byte)255);
                BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, (ushort)length);
            }
            if (length > 0)
            {
                foreach (var r in ary)
                {
                    serializer.Serialize(r, stream);
                }
            }
        }
        public static T Read(System.IO.Stream stream)
        {
            var length = stream.ReadByte();
            if (length == 254)
            {
                ushort t;
                SakerCore.Serialization.BigEndian.BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out t);
                length = t;
            }
            if (length == 255)
            {
                SakerCore.Serialization.BigEndian.BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out length);
            }

            var type = _arrayDeserialize;
            var array = type.ReadArray(stream, length, serializer);
            return array as T;

        }


        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, T> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<T, System.IO.Stream> writer = Write;
            return writer.Method;
        }


        public override unsafe T UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeRead(stream, pos, length);
        }
        public unsafe static T UnsafeRead(byte* stream, int* pos, int length)
        {


            byte* readstart = stream + *pos;

            byte by;
            BigEndianPrimitiveTypeSerializer. Instance.ReadValue(stream, pos, length, out by);
            int bytelength;
            switch (by)
            {
                case 254:
                    {
                        ushort t;
                        BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, pos, length, out t);
                        bytelength = t;
                        break;
                    }
                case 255:
                    {
                        BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, pos, length, out bytelength);
                        break;
                    }
                default:
                    {
                        bytelength = by;
                        break;
                    }
            }

            object result = _unsafeArrayDeserialize.ReadArray(stream, pos, length, bytelength, serializer);

            return result as T;
        }
        public unsafe override System.Reflection.MethodInfo GetUnsafeReaderMethod()
        {
            delUnsafeReaderMethod reader = UnsafeRead;
            return reader.Method;
        }


    }

    class ArrayDeserializeCls<BTCls> : IArrayDeserialize where BTCls : class
    {
        public object ReadArray(System.IO.Stream stream, int length, ITypeSerializer serializer)
        {
            int index = 0;
            var array = new BTCls[length];
            while (index < length)
            {
                var obj = serializer.Deserialize(stream);

                array[index++] = obj as BTCls;
            }
            return array;
        }
    }
    class ArrayDeserializeStuct<BTCls> : IArrayDeserialize
    {
        public object ReadArray(System.IO.Stream stream, int length, ITypeSerializer serializer)
        {
            int index = 0;
            var array = new BTCls[length];
            while (index < length)
            {
                var obj = serializer.Deserialize(stream);

                array[index++] = (BTCls)obj;
            }
            return array;
        }
    }


    interface IArrayDeserialize
    {
        object ReadArray(System.IO.Stream stream, int length, ITypeSerializer serializer);
    }

    interface IUnsafeArrayDeserialize
    {
        unsafe object ReadArray(byte* stream, int* pos, int length, int count, ITypeSerializer serializer);
    }
    unsafe class UnsafeArrayDeserializeCls<BTCls> : IUnsafeArrayDeserialize where BTCls : class
    {
        public object ReadArray(byte* stream, int* pos, int length, int count, ITypeSerializer serializer)
        {
            int index = 0;
            var array = new BTCls[count];
            while (index < array.Length)
            {
                var obj = serializer.UnsafeDeserialize(stream, pos, length);
                array[index++] = obj as BTCls;
            }
            return array;
        }
    }
    unsafe class UnsafeArrayDeserializeStuct<BTCls> : IUnsafeArrayDeserialize
    {
        public object ReadArray(byte* stream, int* pos, int length, int count, ITypeSerializer serializer)
        {
            int index = 0;
            var array = new BTCls[count];
            while (index < array.Length)
            {
                var obj = serializer.UnsafeDeserialize(stream, pos, length);
                array[index++] = (BTCls)obj;
            }
            return array;
        }
    }







#endif

}
