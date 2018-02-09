using System;


namespace SakerCore.Serialization
{
    /// <summary>
    /// <para>序列化成员属性标识特性标签。为每个序列化成员属性指定其在整个数据块中的编排顺序号或上一个紧邻的成员名称。
    /// 成员序列化编号在整个类型的序列化中必须是唯一的</para>
    /// 现在支持不指定编排顺序码，如果不指定则按照字段的成员名称按照字典序编排解析序列化顺序
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PacketMemberAttribute : Attribute, IComparable<PacketMemberAttribute>
    {
        private string m_order = "";
        /// <summary>
        /// 获取成员在报文中的存储顺序。
        /// </summary>
        public string Order
        {
            get
            {
                return this.m_order;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("排序字段不允许设置为空");
                this.m_order = value;
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public PacketMemberAttribute()
        {
        }
        /// <summary>
        /// 指定成员在对象格式化输出的数据块中的顺序来初始化类<c>PacketMemberAttribute</c>的一个新实例。
        /// </summary>
        /// <param name="order">
        /// 顺序号，从 1 开始的整数。用于指定成员在对象序列化后的数据块中的排列顺序。
        /// 注意：同一类型中各需序列化的成员（假如有基类的也标记为序列化的成员则同样包含该成员）的顺序号不能相同。
        /// </param>
        public PacketMemberAttribute(int order)
        { 
            this.m_order = order.ToString();
        }
        /// <summary>
        /// 指定成员在对象格式化输出的数据块中的顺序来初始化类 <see cref="PacketMemberAttribute"/> 的一个新实例。
        /// </summary>
        /// <param name="order">
        /// 顺序排序名称用于指定成员在对象序列化后的数据块中的排列顺序。
        /// 注意：同一类型中各需序列化的成员（假如有基类的也标记为序列化的成员则同样包含该成员）的顺序号不能相同。
        /// </param>
        public PacketMemberAttribute(string order)
        { 
            this.m_order = order;
        }
        int IComparable<PacketMemberAttribute>.CompareTo(PacketMemberAttribute otherAttribute)
        {
            if (otherAttribute != null)
            {
                return this.Order.CompareTo(otherAttribute.Order);
            }
            throw new ArgumentNullException("otherAttribute", "要比较的 PacketMemberAttribute 不能为空。");
        }
    }
}
