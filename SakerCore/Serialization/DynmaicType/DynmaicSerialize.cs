

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SakerCore.Serialization.DynmaicType
{

    #region DynmaicClassSerialize
    /************这里使用服务器动态编译的方法实现类型的序列化和反序列化************/

    /// <summary>
    /// 表示一个类型的动态编排序列化器
    /// </summary>
    public class DynmaicClassSerialize<T> : TypeSerializationBase<T> where T : class
    {
        /// <summary>
        /// 定义一个委托，这个委托表示当前序列化器的序列化方法的信息
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        public delegate void Writer(T obj, Stream stream);
        /// <summary>
        /// 定义一个委托，这个委托表示当前反序列化器的序列化方法的信息
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public delegate T Reader(Stream stream);

        Writer WriterHandle;
        Reader ReaderHandle;
        delUnsafeReaderMethod UnsafeReaderHandle;

        /// <summary>
        /// 创建动态序列化器
        /// </summary>
        /// <param name="type"></param>
        public DynmaicClassSerialize()
        {
            var type = typeof(T);
            if (CheckTypeLoopRefer(type))
            {
                throw new System.Exception($"检查到序列化的对象中包含有循环引用。错误类型 ：{type.FullName}");
            }
            this._serializerType = type;
            var serializerAttr = type.GetTypeSerializeAttr();
            this._serializerName = serializerAttr.TypeCode.IsEmpty(type.FullName);

            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null) throw Error.NonEmptyCtor(type);

            var r = GenerateReadMethod(type);
            var w = GenerateWriteMethod(type);
            var ur = GenerateUnsafeReadMethod(type);

            this.ReaderInfo = r;
            this.WriterInfo = w;
            this.UnsafeReaderInfo = ur;


            WriterHandle = (Writer)w.CreateDelegate(typeof(Writer));
            ReaderHandle = (Reader)r.CreateDelegate(typeof(Reader));
            UnsafeReaderHandle = ur?.CreateDelegate(typeof(delUnsafeReaderMethod)) as delUnsafeReaderMethod;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public override T Deserialize(Stream stream)
        {
            return ReaderHandle(stream);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        public override void Serialize(T obj, Stream stream)
        {
            WriterHandle(obj, stream);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pos"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public override unsafe T UnsafeDeserialize(byte* stream, int* pos, int length)
        {
            return UnsafeReaderHandle(stream, pos, length);
        }

        #region 核心方法（创建动态方法。）

        private static DynamicMethod GenerateReadMethod(Type type)
        {
            #region 测试用代码（生成程序集再反编译为 IL 或 C# 代码查看逻辑是否正确）

            //var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(type.Name + "Serializer"), AssemblyBuilderAccess.RunAndSave);
            //var moduleBuilder = asmBuilder.DefineDynamicModule(type.Name + "Serializer.dll");
            //var typeBuilder = moduleBuilder.DefineType(type.Name + "Serializer", TypeAttributes.Public);

            //var method = typeBuilder.DefineMethod("Read",
            //    MethodAttributes.Public | MethodAttributes.Static,
            //    CallingConventions.Standard,
            //    type,
            //    new Type[] { typeof(Stream) }
            //    );

            #endregion


            var PacketMemberInfos = GetPacketMemberInfos(type);


            //创建动态方法
            var method = new DynamicMethod("Read",
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                type,
                new Type[] {
                    typeof(Stream)},
                type,
                true
                );

            //指示生成的属性方法IL指令对象
            ILGenerator il = method.GetILGenerator();

            Type memberType = null;
            ITypeSerializer memberSerializer = null;
            MethodInfo readMethod = null;
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;


            #region IL实例

            /*
                    .maxstack 2
                    .locals init (
                    [0] class UyiSerlizerTest.TestClass,
                    [1] bool,
                    [2] class UyiSerlizerTest.TestClass
                    )
 
                    IL_0001: ldarg.0
                    IL_0002: call bool class [Uyi.Serialization]Uyi.Serialization.DynmaicType.DynmaicClassSerialize`1<class UyiSerlizerTest.TestClass>::CheckReadObject(class [mscorlib]System.IO.Stream)
                    IL_0007: ldc.i4.0
                    IL_0008: ceq
                    IL_000a: stloc.1
                    IL_000b: ldloc.1
                    IL_000c: brfalse.s IL_0012

                    IL_000e: ldnull
                    IL_000f: stloc.2
                    IL_0010: br.s IL_0034

                    IL_0012: newobj instance void UyiSerlizerTest.TestClass::.ctor()
                    IL_0017: stloc.0
                    IL_0018: ldloc.0

                    IL_0019: ldarg.0
                    IL_001a: call int32 [Uyi.Serialization]Uyi.Serialization.BaseType.Int32TypeSerialize::Read(class [mscorlib]System.IO.Stream)
                    IL_001f: stfld int32 UyiSerlizerTest.TestClass::ID
                    IL_0024: ldloc.0

                    IL_0025: ldarg.0
                    IL_0026: call int32 [Uyi.Serialization]Uyi.Serialization.BaseType.Int32TypeSerialize::Read(class [mscorlib]System.IO.Stream)
                    IL_002b: stfld int32 UyiSerlizerTest.TestClass::ID2
                    IL_0030: ldloc.0

                    IL_0031: stloc.2
                    IL_0032: br.s IL_0034

                    IL_0034: ldloc.2
                    IL_0035: ret
              

                */
            #endregion

            //验证类型是否具有默认的初始化方法
            var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor == null)
                throw Error.NonEmptyCtor(type);


            var retValue = il.DeclareLocal(type); // 声明局部变量（变量类型为需返回的消息类型）。


#if CanSerializeNullObject
            /****插入指令验证对象是否可以反序列化，不是 null 对象则序列化，是 null 则返回 null ****/

            var bl = il.DeclareLocal(typeof(bool)); // 声明局部变量（变量类型为需返回的消息类型）。

            //获取验证方法
            var checkReadMethod = typeof(DynmaicClassSerialize<>).MakeGenericType(type)
                .GetMethod(nameof(CheckReadObject), BindingFlags.Public | BindingFlags.Static);
            if (checkReadMethod == null)
                throw new System.Exception($"没找到指定类型【{type.FullName}】的【{nameof(CheckWriterObject)}】,无法完成对象反序列化的验证过程");

            var labelFalse = il.DefineLabel();

            //加载第一个参数以便后续调用
            il.Emit(OpCodes.Ldarg_0);
            //调用验证方法
            il.Emit(OpCodes.Call, checkReadMethod);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);                   //插入判断比较的IL指令
            il.Emit(OpCodes.Stloc, bl);
            il.Emit(OpCodes.Ldloc, bl);
            il.Emit(OpCodes.Brfalse_S, labelFalse); //判断为false 跳转到 labelFalse 标签位置指令
            il.Emit(OpCodes.Ldnull);                //将空引用（O 类型）推送到计算堆栈上。 
            il.Emit(OpCodes.Stloc, retValue);       // 将已实例化并已完成字段或属性值赋值的消息对象压入计算堆栈中。
            il.Emit(OpCodes.Ldloc, retValue);       // 将已实例化并已完成字段或属性值赋值的消息对象压入计算堆栈中。
            il.Emit(OpCodes.Ret);                   // 函数返回。 

            //插入指令标签 labelFalse
            il.MarkLabel(labelFalse);
#endif

            //调用类型的默认初始化方法来创建新的实例
            il.Emit(OpCodes.Newobj, defaultConstructor);
            //将类型初始化的的对象实例存储到指定的变量中
            il.Emit(OpCodes.Stloc, retValue);

            foreach (var pmi in PacketMemberInfos)
            {
                readMethod = null;
                memberType = (pmi.Member is FieldInfo) ? ((FieldInfo)pmi.Member).FieldType : ((PropertyInfo)pmi.Member).PropertyType;

                if (memberType.IsEnum)
                    memberType = typeof(byte);

                // 使用全局序列化器。
                memberSerializer = TypeSerializerFactory.GetOrCreateTypeSerializer(memberType);

                if (null == memberSerializer)
                    throw new System.Exception(string.Format("系统未找到类型 {0} 的序列化器，无法反序列化。1", memberType.FullName));

                readMethod = memberSerializer.ReaderInfo;
                if (null == readMethod)
                    throw new System.Exception(string.Format("系统未找到类型 {0} 的序列化器，无法反序列化。2", memberType.FullName));

                // 将刚刚创建的对象压入到计算堆栈中（后进先出）供后续方法调用。（在栈底）     
                il.Emit(OpCodes.Ldloc, retValue);
                il.Emit(OpCodes.Ldarg_0); // 将第 1 个参数 stream 的值压入到计算堆栈中供后续方法调用。（在栈顶）

                il.Emit(OpCodes.Call, readMethod); // 调用反序列化方法，方法签名为：public static T Read<T>(Stream)。
                if (pmi.Member is PropertyInfo) // 成员是属性，调用 Set 方法。
                    il.Emit(OpCodes.Callvirt, type.GetProperty(pmi.Member.Name, bindingFlags).GetSetMethod(true));
                else // 成员是字段，直接赋值。
                    il.Emit(OpCodes.Stfld, type.GetField(pmi.Member.Name, bindingFlags));
            }
            il.Emit(OpCodes.Ldloc, retValue); // 将已实例化并已完成字段或属性值赋值的消息对象压入计算堆栈中。
            il.Emit(OpCodes.Ret); // 函数返回。

            #region 测试用代码（生成程序集） 

            //var t = typeBuilder.CreateType();
            //asmBuilder.Save(type.Name + "Serializer.dll");

            #endregion

            return method;
        }
        private static DynamicMethod GenerateWriteMethod(Type type)
        {
            //var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(type.Name + "Serializer"), AssemblyBuilderAccess.RunAndSave);
            //var moduleBuilder = asmBuilder.DefineDynamicModule(type.Name + "Serializer.dll");
            //var typeBuilder = moduleBuilder.DefineType(type.Name + "Serializer", TypeAttributes.Public);
            //var method = typeBuilder.DefineMethod("Writer",
            //    MethodAttributes.Public | MethodAttributes.Static,
            //    CallingConventions.Standard,
            //    null,
            //    new Type[] { type, typeof(Stream) }
            //    );


            var PacketMemberInfos = GetPacketMemberInfos(type);

            var method = new DynamicMethod("Write",
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
               null,
                new Type[] { type, typeof(Stream) },
                type,
                true
                );


            #region IL 示例

            /*
                    // 用 C# 编写测试类型的序列化功能后，反编译为如下 IL 代码可供参考。
               
                        IL_0001: ldarg.0
                        IL_0002: ldarg.1
                        IL_0003: call bool UyiSerlizerTest.TestClass::CheckObjectWriter(class UyiSerlizerTest.TestClass, class [mscorlib]System.IO.Stream)
                    
                        IL_0008: ldc.i4.0
                        IL_0009: ceq
                        IL_000b: stloc.0
                        IL_000c: ldloc.0
                        IL_000d: brfalse.s IL_0011

                        IL_000f: br.s IL_001e

                        IL_0011: ldarg.0
                        IL_0012: ldfld int32 UyiSerlizerTest.TestClass::ID
                        IL_0017: ldarg.1
                        IL_0018: call void [Uyi.Serialization]Uyi.Serialization.BaseType.Int32TypeSerialize::Write(int32, class [mscorlib]System.IO.Stream)
                        IL_001d: nop

                        IL_001e: ret
                  
             */



            #endregion

            ILGenerator il = method.GetILGenerator();

            Type memberType = null;
            ITypeSerializer memberSerializer = null;
            MethodInfo writeMethod = null;
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;




#if CanSerializeNullObject      //表示是否可以序列化NULL对象
            #region 当序列化器支持序列化null对象时的处理逻辑

            var checkWrtertMethod = typeof(DynmaicClassSerialize<>).MakeGenericType(type)
                .GetMethod(nameof(CheckWriterObject), BindingFlags.Public | BindingFlags.Static);
            if (checkWrtertMethod == null)
                throw new System.Exception($"没找到指定类型【{type.FullName}】的【{nameof(CheckWriterObject)}】,无法完成对象序列化的验证过程");

            var bl = il.DeclareLocal(typeof(bool));
            var labelTrue = il.DefineLabel();
            var labelFalse = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0); // 将第 1 个参数 value 压入到计算堆栈中（后进先出）供后续方法调用。
            il.Emit(OpCodes.Ldarg_1); // 将第 2 个参数 stream 压入到计算堆栈中（后进先出）供后续方法调用。

            //调用对象是否为null的验证方法
            il.Emit(OpCodes.Call, checkWrtertMethod);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);   //插入比较指令
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Brtrue_S, labelFalse);
            il.Emit(OpCodes.Br_S, labelTrue);
            il.MarkLabel(labelFalse);//定义判断为false的跳转标签
            //验证为false时直接返回
            il.Emit(OpCodes.Ret);
            il.MarkLabel(labelTrue);//定义判断为true的跳转标签 

            #endregion
#endif
            foreach (var pmi in PacketMemberInfos)
            {
                writeMethod = null;

                memberType = (pmi.Member is FieldInfo) ? ((FieldInfo)pmi.Member).FieldType : ((PropertyInfo)pmi.Member).PropertyType;

                if (memberType.IsEnum)
                {
                    memberType = typeof(byte);
                }


                //获取成员类型的序列化器
                memberSerializer = TypeSerializerFactory.GetOrCreateTypeSerializer(memberType);

                if (null == memberSerializer)
                    throw new System.Exception(string.Format("系统未找到类型 {0} 的序列化器，无法反序列化。1", memberType.FullName));

                //序列化类型的序列化方法
                //   var writer = new Writer(memberSerializer.Serialize);
                writeMethod = memberSerializer.WriterInfo;

                if (writeMethod == null)
                    throw new System.Exception(string.Format("系统未找到类型 {0} 的序列化器，无法反序列化。2", memberType.FullName));


                il.Emit(OpCodes.Ldarg_0); // 将第 1 个参数 value 压入到计算堆栈中（后进先出）供后续方法调用。
                if (pmi.Member is PropertyInfo) // 成员是属性。获取属性值。
                    il.Emit(OpCodes.Callvirt, type.GetProperty(pmi.Member.Name, bindingFlags).GetGetMethod(true));
                else // 成员是字段。获取字段值。
                    il.Emit(OpCodes.Ldfld, type.GetField(pmi.Member.Name, bindingFlags));
                il.Emit(OpCodes.Ldarg_1); // 将第 2 个参数 stream 压入到计算堆栈中（后进先出）供后续方法调用。
                il.Emit(OpCodes.Call, writeMethod); // 调用序列化方法，方法签名为：public static void Write(object,Stream)。
            }

            il.Emit(OpCodes.Ret); // 直接返回。


            return method;
        }
        private DynamicMethod GenerateUnsafeReadMethod(Type type)
        {


            var PacketMemberInfos = GetPacketMemberInfos(type);


            //#region 测试用代码（生成程序集再反编译为 IL 或 C# 代码查看逻辑是否正确）

            //var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(type.Name + "Serializer"), AssemblyBuilderAccess.RunAndSave);
            //var moduleBuilder = asmBuilder.DefineDynamicModule(type.Name + "Serializer.dll");
            //var typeBuilder = moduleBuilder.DefineType(type.Name + "Serializer", TypeAttributes.Public);


            //var method = typeBuilder.DefineMethod("UnsafeRead",
            //    MethodAttributes.Public | MethodAttributes.Static,
            //    CallingConventions.Standard,
            //    type,
            //    new Type[] { typeof(byte*), typeof(int*), typeof(int) }
            //    );

            //#endregion 




            //创建动态方法
            var method = new DynamicMethod("UnsafeRead",
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                type,
                new Type[] { typeof(byte*), typeof(int*), typeof(int) },
                type,
                true
                );

            //指示生成的属性方法IL指令对象
            ILGenerator il = method.GetILGenerator();

            Type memberType = null;
            ITypeSerializer memberSerializer = null;
            MethodInfo readMethod = null;
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            //验证类型是否具有默认的初始化方法
            var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor == null)
                throw Error.NonEmptyCtor(type);


            var retValue = il.DeclareLocal(type); // 声明局部变量（变量类型为需返回的消息类型）。


#if CanSerializeNullObject
            /****插入指令验证对象是否可以反序列化，不是 null 对象则序列化，是 null 则返回 null ****/
            var bl = il.DeclareLocal(typeof(bool)); // 声明局部变量（变量类型为需返回的消息类型）。

            //获取验证方法
            var checkReadMethod = typeof(DynmaicClassSerialize<>).MakeGenericType(type)
                .GetMethod(nameof(UnsafeCheckReadObject), BindingFlags.Public | BindingFlags.Static);
            if (checkReadMethod == null)
                throw new System.Exception($"没找到指定类型【{type.FullName}】的【{nameof(UnsafeCheckReadObject)}】,无法完成对象反序列化的验证过程");

            var labelFalse = il.DefineLabel();

            //加载第一个参数以便后续调用
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            //调用验证方法
            il.Emit(OpCodes.Call, checkReadMethod);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);                   //插入判断比较的IL指令
            il.Emit(OpCodes.Stloc, bl);
            il.Emit(OpCodes.Ldloc, bl);
            il.Emit(OpCodes.Brfalse_S, labelFalse); //判断为false 跳转到 labelFalse 标签位置指令
            il.Emit(OpCodes.Ldnull);                //将空引用（O 类型）推送到计算堆栈上。 
            il.Emit(OpCodes.Stloc, retValue);       // 将已实例化并已完成字段或属性值赋值的消息对象压入计算堆栈中。
            il.Emit(OpCodes.Ldloc, retValue);       // 将已实例化并已完成字段或属性值赋值的消息对象压入计算堆栈中。
            il.Emit(OpCodes.Ret);                   // 函数返回。 

            //插入指令标签 labelFalse
            il.MarkLabel(labelFalse);
#endif

            //调用类型的默认初始化方法来创建新的实例
            il.Emit(OpCodes.Newobj, defaultConstructor);
            //将类型初始化的的对象实例存储到指定的变量中
            il.Emit(OpCodes.Stloc, retValue);

            foreach (var pmi in PacketMemberInfos)
            {
                readMethod = null;
                memberType = (pmi.Member is FieldInfo) ? ((FieldInfo)pmi.Member).FieldType : ((PropertyInfo)pmi.Member).PropertyType;

                if (memberType.IsEnum)
                    memberType = typeof(byte);   //如果是枚举类型使用byte类型的序列化器

                // 使用全局序列化器。
                memberSerializer = TypeSerializerFactory.GetOrCreateTypeSerializer(memberType);

                if (null == memberSerializer)
                    throw new System.Exception(string.Format("系统未找到类型 {0} 的序列化器，无法反序列化。1", memberType.FullName));

                readMethod = memberSerializer.UnsafeReaderInfo;
                if (null == readMethod)
                    throw new System.Exception(string.Format("系统未找到类型 {0} 的序列化器，无法反序列化。2", memberType.FullName));

                // 将刚刚创建的对象压入到计算堆栈中（后进先出）供后续方法调用。（在栈底）     
                il.Emit(OpCodes.Ldloc, retValue);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);               //向计算机堆栈压入输入参数

                il.Emit(OpCodes.Call, readMethod); // 调用反序列化方法
                if (pmi.Member is PropertyInfo) // 成员是属性，调用 Set 方法。
                    il.Emit(OpCodes.Callvirt, type.GetProperty(pmi.Member.Name, bindingFlags).GetSetMethod(true));
                else // 成员是字段，直接赋值。
                    il.Emit(OpCodes.Stfld, type.GetField(pmi.Member.Name, bindingFlags));
            }
            il.Emit(OpCodes.Ldloc, retValue); // 将已实例化并已完成字段或属性值赋值的消息对象压入计算堆栈中。
            il.Emit(OpCodes.Ret); // 函数返回。


            #region 测试用代码（生成程序集） 

            //var t = typeBuilder.CreateType();
            //asmBuilder.Save($@"{type.Name}Serializer.dll");
            //return null;
            #endregion
            return method;
        }

        #endregion

        /// <summary>
        /// 反射获取类型的类型成员信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<TypeSerlizaMemberInfo> GetPacketMemberInfos(Type type)
        {

            var field = type.GetFields();
            var pros = type.GetProperties();

            List<TypeSerlizaMemberInfo> _listMember = new List<TypeSerlizaMemberInfo>();
            foreach (var r in field)
            {
                if (!r.IsCanSerializeProperty()) continue;
                _listMember.Add(new TypeSerlizaMemberInfo()
                {
                    Attribute = r.GetMemberSerializeAttr(),
                    Member = r
                });
            }
            foreach (var r in pros)
            {
                if (!r.IsCanSerializeProperty()) continue;
                if (!r.CanRead) continue;
                if (!r.CanWrite) continue;
                _listMember.Add(new TypeSerlizaMemberInfo()
                {
                    Attribute = r.GetMemberSerializeAttr(),
                    Member = r
                });
            }


            //获取一个属性在继承链条中的继承深度
            //var shendu = GetPacketMemberInfosJCSD(r,)

            var _serializerList = _listMember.OrderBy(p => p.Attribute.Order);
            string befindex = "";
            TypeSerlizaMemberInfo before = null;
            foreach (var cur in _serializerList)
            {
                if (cur.Attribute.Order == befindex) throw Error.RepeatOrder(typeof(T).FullName, before, cur);
                befindex = cur.Attribute.Order;
                before = cur;
            }

            return _serializerList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool CheckWriterObject(T obj, Stream stream)
        {
            byte by = (byte)(obj == null ? 0 : 1);
            stream.WriteByte(by);
            if (by == 0)
                return false;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool CheckReadObject(Stream stream)
        {
            var by = stream.ReadByte();
            if (by == 0) return false;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pos"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public unsafe static bool UnsafeCheckReadObject(byte* stream, int* pos, int length)
        { 
            *pos += 1;
            if (*pos >= length) return false;
            var by = stream[(*pos) - 1];
            return by == 1;
        }

        /// <summary>
        /// 检查序列化的类中是否有循环引用的对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool CheckTypeLoopRefer(Type type)
        {
            //检查一个类中是否包含循环引用字段信息
            Stack<Type> stacktype = new Stack<Type>();
            return CheckTypeLoopReferHelper(type, stacktype);
        }
        private static bool CheckTypeLoopReferHelper(Type type, Stack<Type> stacktype)
        {

            //检查类型是否是基础类型，如果是直接返回
            if (type.IsValueType) return false;
            if (type.IsArray)
            {
                return CheckTypeLoopReferHelper(type.GetElementType(), stacktype);
            }
            //在当前栈压入当前类型信息
            if (CheckHasTypeInfoInStack(type, stacktype))
            {
                return true;
            }
            stacktype.Push(type);

            //找到当前类型的所有字段信息
            var allfields = type.GetFields().Where(p => p.IsDefined(typeof(PacketMemberAttribute), true));
            foreach (var f in allfields)
            {
                if (CheckTypeLoopReferHelper(f.FieldType, stacktype))
                {
                    stacktype.Pop();
                    return true;
                }
            }
            //找到当前类型的所有字段信息
            var allpros = type.GetProperties().Where(p => p.CanRead && p.CanWrite && p.IsDefined(typeof(PacketMemberAttribute), true)
            );
            foreach (var f in allpros)
            {
                if (CheckTypeLoopReferHelper(f.PropertyType, stacktype))
                {
                    stacktype.Pop();
                    return true;
                }
            }

            stacktype.Pop();
            return false;
        }
        private static bool CheckHasTypeInfoInStack(Type type, Stack<Type> stacktype)
        {
            //遍历栈判断当前类型的对象是否存在
            foreach (var r in stacktype)
            {
                if (r.Equals(type))
                    return true;
            }
            return false;
        }
    }

    internal class TypeSerlizaMemberInfo
    {
        internal PacketMemberAttribute Attribute;
        internal MemberInfo Member;
    }

    #endregion

    #region CustomSerializer

    /// <summary>
    /// 对象的自定义序列化器
    /// </summary>
    internal class CustomSerializer<T> : TypeSerializationBase<T> where T : ICustomSerializer, new()
    {

        public CustomSerializer()
        {
            var type = typeof(T);
            this._serializerType = type;
            var serializerAttr = type.GetTypeSerializeAttr();
            this._serializerName = serializerAttr.TypeCode.IsEmpty(type.FullName);
        }


        public static void Write(T obj, Stream stream)
        {
            if (obj == null)
            {
                stream.WriteByte(0);
                return;
            }
            stream.WriteByte(1);
            obj.Serialize(stream);
        }
        public static T Read(Stream stream)
        {
            var v = stream.ReadByte();
            if (v != 1) return default(T);

            var obj = new T();
            obj.Deserialize(stream);
            return obj;
        }


        public override T Deserialize(Stream stream)
        {
            return Read(stream);
        }
        public override void Serialize(T obj, Stream stream)
        {
            Write(obj, stream);
        }
    }

    #endregion

}
