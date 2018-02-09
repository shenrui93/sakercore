/***************************************************************************
 * 
 * 创建时间：   2017/1/6 20:16:23
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供DateTime类型的方法扩展
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.Extension
{
    /// <summary>
    /// 提供DateTime类型的方法扩展
    /// </summary>
    public static class _DateTime
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTimestamp(this DateTime dt)
        {
            return (long)(dt - Web.WebParamData.LocalUTCBegin).TotalSeconds;
        }
    }
}
