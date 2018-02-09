/***************************************************************************
 * 
 * 创建时间：   2017/8/29 10:29:23
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供 Type 操作扩展
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.Extension
{
    /// <summary>
    /// 提供 Type 操作扩展
    /// </summary>
    public static class _Type
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool IsSubInterface(this Type type,Type interfaceType)
        {
            foreach(var t in type.GetInterfaces())
            {
                if (t == interfaceType) return true;
            }
            return false;
        }
    }
}
