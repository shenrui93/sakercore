using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SakerCore.Serialization
{

    /// <summary>
    /// 指示类型支持序列化的特性标签。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class PacketContractAttribute : Attribute
    {
        /// <summary>
        /// 获取指定类型的唯一代码。如果未指定，则采用该类型的 GUID。
        /// </summary>
        public string TypeCode
        {
            get;
            internal set;
        }
        /// <summary>
        /// 初始化类<c>PacketContractAttribute</c>的一个新实例。
        /// </summary>
        public PacketContractAttribute()
        {
            this.TypeCode = string.Empty;
        }
        /// <summary>
        /// 指定类型代码来初始化<c>PacketContractAttribute</c>的一个新实例。
        /// </summary>
        /// <param name="typeCode">
        /// 类型代码。用于标识当前指定序列化的类型。必须确保唯一。
        /// 如果不指定或指定为负数，则将采用类型的 GUID。
        /// </param>
        public PacketContractAttribute(int typeCode)
        {
            if (typeCode >= 0)
            {
                this.TypeCode = typeCode.ToString();
            }
        }
        /// <summary>
        /// 指定类型代码来初始化<c>PacketContractAttribute</c>的一个新实例。
        /// </summary>
        /// <param name="typeCode">
        /// 类型代码。用于标识当前指定序列化的类型。必须确保唯一。
        /// 如果不指定或指定为空字符串，则将采用类型的 GUID。
        /// </param>
        public PacketContractAttribute(string typeCode)
        {
            this.TypeCode = typeCode;
        }
    }
}
