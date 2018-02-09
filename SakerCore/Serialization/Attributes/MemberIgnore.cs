/***************************************************************************
 * 
 * 创建时间：   2017/5/25 11:01:30
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.Serialization.Attributes
{
    /// <summary>
    /// 指定一个字段在序列化操作过程中忽略字段行为
    /// </summary>
    internal class MemberIgnore
    {
        /// <summary>
        /// 类MemberIgnore的默认构造函数
        /// </summary>
        public MemberIgnore()
        {
            //在这里实现对象的初始化操作
        }
    }
}
