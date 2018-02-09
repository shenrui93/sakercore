using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SakerCore.Serialization
{
    /// <summary>
    ///   消息体常量定义，表示这个类的常量是所有消息定义的主副命令码
    /// </summary>

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PackCommonIDAttribute : Attribute
    {

    }
}
