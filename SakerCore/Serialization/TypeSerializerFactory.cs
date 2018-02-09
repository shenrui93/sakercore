using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SakerCore.Extension;
using SakerCore.Serialization.BaseType;
using SakerCore.Serialization.DynmaicType;

namespace SakerCore.Serialization
{
    /// <summary>
    /// 类型序列化器工厂
    /// </summary>
    internal static class TypeSerializerFactory
    {


        static TypeSerializerFactory()
        {
            //root = new System.Threading.ReaderWriterLockSlim();
            TypeSerializerInitializer();
        }
        internal static void TypeSerializerInitializer()
        {
            RegisterBaseTypeSerializer();

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                CreateTypeSerializerFromAssembly(assembly);
            }
        }
        static string formatErrorString(System.Exception ex)
        {
            StringBuilder strb = new StringBuilder();
            if (ex != null)
            {
                strb.AppendLine(ex.Message);
                strb.AppendLine(formatErrorString(ex.InnerException));
            }
            return strb.ToString();
        }

        //线程同步根
        //static System.Threading.ReaderWriterLockSlim root;
        private static Dictionary<Type, ITypeSerializer> TypeAndSerializer = new Dictionary<Type, ITypeSerializer>();
        private static Dictionary<string, ITypeSerializer> NameAndSerializer = new Dictionary<string, ITypeSerializer>();
        private static Dictionary<int, ITypeSerializer> CodeAndSerializer = new Dictionary<int, ITypeSerializer>();
        private static Dictionary<Type, ushort> TypeAndSerializerCode = new Dictionary<Type, ushort>();

        static void AddSerializer(string key, ITypeSerializer v)
        {
            if (NameAndSerializer.ContainsKey(key))
            {
                if (!v.SerializerType.Equals(NameAndSerializer[key].SerializerType))
                {
                    throw Error.ConflictSerializer(key, v.SerializerType, NameAndSerializer[key].SerializerType);
                }
                return;
            }
            NameAndSerializer.Add(key, v);

            ushort typecode;
            if (ushort.TryParse(key, out typecode))
            {
                TypeAndSerializerCode[v.SerializerType] = typecode;
                CodeAndSerializer[typecode] = v;
            }



        }
        static void AddSerializer(Type key, ITypeSerializer v)
        {
            TypeAndSerializer[key] = v;
        }



        static void CreateTypeSerializerFromAssembly(Assembly assembly)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    try
                    {
                        TryCreateTypeSerializer(type);
                    }
                    catch (System.Exception ex)
                    {
                        SerializationErrorEvent.OnErrorEvent(ex);
#if TRACE
                        throw ex;
#endif
                    }
                }
            }
            catch// ()
            {
#if TRACE
                throw  ;
#endif
            }
        }
        static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            CreateTypeSerializerFromAssembly(args.LoadedAssembly);
        }
        private static void AddTypeSerializer(ITypeSerializer serializer)
        {
            AddSerializer(serializer.SerializerName, serializer);
            AddSerializer(serializer.SerializerType, serializer);
        }
        private static void RegisterBaseTypeSerializer()
        {
            //注册基础类型序列化器

            AddTypeSerializer(new Int16TypeSerialize());
            AddTypeSerializer(new Int32TypeSerialize());
            AddTypeSerializer(new Int64TypeSerialize());
            AddTypeSerializer(new UInt16TypeSerialize());
            AddTypeSerializer(new UInt32TypeSerialize());
            AddTypeSerializer(new UInt64TypeSerialize());

            AddTypeSerializer(new ByteTypeSerialize());
            AddTypeSerializer(new SByteTypeSerialize());
            AddTypeSerializer(new ByteArrayTypeSerialize());

            AddTypeSerializer(new DateTimeTypeSerialize());
            AddTypeSerializer(new TimeSpanTypeSerialize());

            AddTypeSerializer(new DoubleTypeSerialize());
            AddTypeSerializer(new FloatTypeSerialize());
            AddTypeSerializer(new DecimalTypeSerialize());

            AddTypeSerializer(new StringTypeSerialize());
            AddTypeSerializer(new BooleanTypeSerialize());
            AddTypeSerializer(new CharTypeSerialize());
            AddTypeSerializer(new GUIDTypeSerialize());
        }


        internal static ITypeSerializer GetOrCreateTypeSerializer(Type type)
        {
            ITypeSerializer serializer;


            if (TryGetSerializer(type, out serializer))
            {
                return serializer;
            }
            return CreateTypeSerializer(type);

        }


        internal static bool TryGetSerializerCode(Type type, out ushort v)
        {
            return TypeAndSerializerCode.TryGetValue(type, out v);
        }
        internal static bool TryGetSerializer(Type type, out ITypeSerializer v)
        {
            return TypeAndSerializer.TryGetValue(type, out v);
        }
        internal static bool TryGetSerializer(string key, out ITypeSerializer v)
        {
            return NameAndSerializer.TryGetValue(key, out v);
        }
        internal static bool TryGetSerializer(int key, out ITypeSerializer v)
        {
            return CodeAndSerializer.TryGetValue(key, out v);
        }





        static ITypeSerializer CreateTypeSerializer(Type type)
        {

#if DEBUG
            DebugHelper.RuningLog($"开始创建 Type：{type.FullName}的序列化器");
#endif
            ITypeSerializer serializer = null;
            if (type.IsArray)
            {
#if DEBUG
                //    DebugHelper.RuningLog($"类型是数组类型，创建数组类型的动态序列化器");
#endif
#if Version35
                serializer = new ArrayTypeSerialize(type) as ITypeSerializer;
#else
                serializer = Activator.CreateInstance(typeof(ArrayTypeSerialize<>)
                           .MakeGenericType(type)) as ITypeSerializer;
#endif
            }
            else if (type.IsEnum)
            {
#if DEBUG
                // DebugHelper.RuningLog($"类型是枚举类型，直接使用枚举类型的序列化器");
#endif
                return EnumTypeSerialize.Instance;
            }
            else if (type.IsClass)
            {
#if DEBUG
                //   DebugHelper.RuningLog($"类型是普通类型，动态创建类型的序列化器");
#endif
                //检查类型是否支持序列化
                if (!type.IsCanSerializeType()) throw Error.NotHasSerializeAttrType(type);

                //如果类型实现了自定义的序列化器，初始化自定义的序列化器
                if (type.IsSubInterface(typeof(ICustomSerializer)))
                {

                    //创建类型动态序列化器
                    serializer = Activator.CreateInstance(typeof(CustomSerializer<>)
                        .MakeGenericType(type)) as ITypeSerializer;
                }
                else
                {
                    //创建类型动态序列化器
                    serializer = Activator.CreateInstance(typeof(DynmaicClassSerialize<>)
                        .MakeGenericType(type)) as ITypeSerializer;

                }
            }
            else
            {
                //检查类型是否支持序列化
                if (!type.IsCanSerializeType()) throw Error.NotHasSerializeAttrType(type);
                throw new System.Exception("目前平台的序列化器不支持序列化结构体类型（struct）类型");
            }
            //缓存创建的类型序列化器
            AddTypeSerializer(serializer);
            return serializer;
        }
        static void TryCreateTypeSerializer(Type type)
        {
            try
            {
                if (!type.IsCanSerializeType()) return;
                ITypeSerializer serializer;
                if (TryGetSerializer(type, out serializer)) return;
                CreateTypeSerializer(type);
            }
            catch (System.Exception ex)
            {
                SakerCore.SystemErrorProvide.OnSystemErrorHandleEvent(null, new System.Exception($"处理序列化类型：{type.FullName} 出现异常", ex));
#if TRACE
                throw ex; 
#endif
            }
        }

    }
}
