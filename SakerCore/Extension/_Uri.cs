/***************************************************************************
 * 
 * 创建时间：   2017/8/30 9:55:16
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供 Uri 操作扩展
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
    /// 提供 Uri 操作扩展
    /// </summary>
    public static class _Uri
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetHostAndPath(this Uri uri)
        {
            return $"{uri.Host}{uri.AbsolutePath}";
        }

    }
}
