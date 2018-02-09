/***************************************************************************
 * 
 * 创建时间：   2016/2/27 13:29:23
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.Serialization
{
    /// <summary>
    /// 表示一个消息体没有自定义的序列化器，请自动生成序列化器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NonCustomH5SerializerAttribute : Attribute
    {
    }
}
