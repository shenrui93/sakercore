using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SakerCore.Serialization
{
    /// <summary>
    /// 表示在输出时对象属性的输出代码文本，在H5中解析使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PacketCodeStringAttribute : Attribute
    {
        /// <summary>
        /// 输出的代码信息文本
        /// </summary>
        public string CodeStr
        {
            get
            {
                if (code_line == null || code_line.Length <= 0) return "";
                StringBuilder strb = new StringBuilder();
                int row = 1;
                foreach (var r in code_line)
                {
                    if (row >= code_line.Length)
                        strb.Append(this.SpaceStr + r);
                    else
                        strb.AppendLine(this.SpaceStr + r);
                    row++;
                }

                return strb.ToString();
            }
        }
        /// <summary>
        /// 序列化类型的字符串表示信息
        /// </summary>
        public string TypeStr { get; private set; }

        string[] code_line = new string[] { };
        /// <summary>
        /// 初始化一个序列化标签信息的对象
        /// </summary>
        /// <param name="typestr"></param>
        /// <param name="code_str"></param>
        public PacketCodeStringAttribute(string typestr, string code_str)
        {
            this.code_line = new string[] { code_str };
            this.TypeStr = typestr;
        }
        /// <summary>
        /// 初始化一个序列化标签信息的对象
        /// </summary>
        /// <param name="typestr"></param>
        /// <param name="code_line"></param>
        public PacketCodeStringAttribute(string typestr, string[] code_line)
        {
            this.code_line = code_line;
            this.TypeStr = typestr;
        }
        /// <summary>
        /// 
        /// </summary>
        public PacketCodeStringAttribute()
        {
            this.code_line =null;
            this.TypeStr = "";

        }

        /// <summary>
        /// 设置数据每行的补间空格字符
        /// </summary>
        public string SpaceStr { get; set; } = "";
    }
}
