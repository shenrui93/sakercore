using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SakerCore.Serialization
{
    internal static class Error
    {
        /// <summary>
        /// 类型不是一个数组类型，不能使用数组类型初始化器初始化
        /// </summary>
        /// <returns></returns>
        internal static Exception TypeNotIsArray()
        {
            return new System.Exception("类型不是一个数组类型，不能使用数组类型序列化器初始化");
        }
        internal static Exception NotFindSerializer(string key)
        {
            return new System.Exception("未找到指定名称的类型序列化器对象，查找名称:" + key);
        }
        internal static Exception ConflictSerializer(string key, Type type1, Type type2)
        {
            var msg =
                string.Format("注册名称：{0}的序列化器发生类型冲突，注册类型：{1},冲突类型：{2}"
                , key, type1.FullName, type2.FullName);
            return new System.Exception(msg);

        }
        internal static Exception CanNotReadOrWriteProperty()
        {
            return new System.Exception("指定属性不支持读写操作！");
        }
        internal static Exception NotHasSerializeAttrType(Type type)
        {
            return new System.Exception("指定类型：" + type.FullName + " 不支持序列化！未为该类型添加支持序列化特性标签");

        }
        internal static Exception NonEmptyCtor(Type type)
        {
            return new System.Exception("无法创建类型：" + type.FullName + " 的动态序列化器，该类未发现或无法访问无参数构造函数");
        }

#if Version35

        internal static Exception RepeatOrder(
            DynmaicType.DynmaicClassSerialize serialize
            , DynmaicType.IFieldSerializer before
            , DynmaicType.IFieldSerializer cur)
        {
            return new Exception(string.Format("类型： {0}  包含相同成员ID： {1}  字段1： {2}  ,字段2： {3}"
                    , serialize.SerializerType.FullName
                    , before.Order,
                    before.ToString(),
                    cur.ToString()
                ));
        } 
#else

        internal static Exception RepeatOrder(
           string serializeTName
            , DynmaicType.TypeSerlizaMemberInfo before
            , DynmaicType.TypeSerlizaMemberInfo cur)
        {
            return new System.Exception(string.Format("类型： {0}  包含相同成员ID： {1}  字段1： {2}  ,字段2： {3}"
                    , serializeTName
                    , before.Attribute.Order,
                    before.Member.ToString(),
                    cur.Member.ToString()
                ));
        }
#endif



        /// <summary>
        /// 没有找到指定类型的序列化器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Exception NotFindSerializerMethod(Type type)
        {
            throw new System.Exception($"没有找到指定类型{type.ToString()}的序列化器");
        }
    }

}