using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace SakerCore.Serialization
{
    /// <summary>
    /// 类型序列化帮助类，实现基本类型的一些扩展方法
    /// </summary>
    internal static class TypeSerializationHelper
    {
        /// <summary>
        /// 获取类型序列化器
        /// </summary>
        /// <param name="type">需要序列化的类型</param>
        /// <returns></returns>
        public static ITypeSerializer GetTypeSerializer(Type type)
        {
            return TypeSerializerFactory.GetOrCreateTypeSerializer(type);
        }
        /// <summary>
        /// 获取类型序列化器
        /// </summary>
        /// <param name="key">需要获取序列化器的名称</param>
        /// <returns></returns>
        public static ITypeSerializer GetTypeSerializer(string key)
        {
            ITypeSerializer serializer;
            if (TypeSerializerFactory.TryGetSerializer(key, out serializer))
                return serializer;
            throw Error.NotFindSerializer(key);

        }

        /// <summary>
        /// 判断指定类型是否支持序列化
        /// </summary>
        /// <param name="type">判断的类型</param>
        /// <returns></returns>
        public static bool IsCanSerializeType(this Type type)
        {
            var attr = type.GetCustomAttributes(false);
            if (type.IsAbstract) return false;
            foreach (var r in attr)
            {
                if (r is PacketContractAttribute)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 判断类型成员是否支持序列化
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static bool IsCanSerializeProperty(this MemberInfo member)
        {
            var attr = member.GetCustomAttributes(false);

            foreach (var r in attr)
            {
                if (r is PacketMemberAttribute)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 获取类型的序列化特性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PacketContractAttribute GetTypeSerializeAttr(this MemberInfo type)
        {
            return type.GetCustomAttribute<PacketContractAttribute>(false);
        }
        /// <summary>
        /// 获取类型成员的序列化特性
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static PacketMemberAttribute GetMemberSerializeAttr(this MemberInfo member)
        {
            var name = member.Name;
            var decname = member.DeclaringType.FullName;


            var atr = member.GetCustomAttribute<PacketMemberAttribute>(false);
            if (string.IsNullOrEmpty(atr.Order))
            {
                atr.Order = name;
            }
            atr.Order = decname + atr.Order;
            return atr;
        }
        /// <summary>
        /// 对成员对象搜索指定类型的特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this MemberInfo member, bool inherit) where T : System.Attribute
        {
            var attr = member.GetCustomAttributes(typeof(T), inherit);
            return attr.FirstOrDefault() as T;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rr"></param>
        /// <param name="defaul"></param>
        /// <returns></returns>
        public static string IsEmpty(this string rr, string defaul)
        {
            if (string.IsNullOrEmpty(rr)) return defaul;
            return rr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsHasCustomSerializer(this Type type)
        {
#if DEBUG


#endif
            //var faces = type.GetInterfaces();

            //foreach (var f in faces)
            //{
            //    if (f.Equals(typeof(ICustomSerializer)))
            //    {
            //        if (type.IsDefined(typeof(PacketNonCustomSerializerAttribute), false)) return false;
            //        return true;
            //    }
            //}
            return false;
        }
        /// <summary>
        /// 检查指定编码类型是否支持反序列化
        /// </summary>
        /// <param name="typeCode"></param>
        /// <returns></returns>
        public static bool CanDeserialize(int typeCode)
        {
            ITypeSerializer serializer;
            return TypeSerializerFactory.TryGetSerializer(typeCode.ToString(), out serializer);

        }
    }
}
