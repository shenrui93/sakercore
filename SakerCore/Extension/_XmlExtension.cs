/***************************************************************************
 * 
 * 创建时间：   2017/9/8 12:20:11
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供Xml操作扩展
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SakerCore.Extension
{
    /// <summary>
    /// 提供Xml操作扩展
    /// </summary>
    public static class _XmlExtension
    {
        /// <summary>
        /// 设置属性
        /// </summary>
        public static void SetAttribute(this XmlAttributeCollection attrcoll,string name,string value,XmlDocument xmldoc)
        {
            var attr = xmldoc.CreateAttribute(name);
            attr.Value = value;
            attrcoll.SetNamedItem(attr);
        }


    }
}
