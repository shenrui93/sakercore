using System;

namespace SakerCore.Serialization
{
    /// <summary>
    /// 指示类型如何支持序列化的特性标签,该类型不应该使用自定义类型序列化器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class PacketNonCustomSerializerAttribute : Attribute
    {
    }
}
