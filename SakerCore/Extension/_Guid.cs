/***************************************************************************
 * 
 * 创建时间：   2017/1/7 11:57:47
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供GUID类型的方法扩展
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
    /// 提供GUID类型的方法扩展
    /// </summary>
    public static class _Guid
    {
        /// <summary>
        /// 将一个GUID值转换为一个简短的区分大小的字符串信息
        /// </summary>
        /// <param name="guid">需要转换的GUID</param>
        /// <returns></returns>
        public static string ToSimpleString(this Guid guid)
        {
            var data = Serialization.Base64Serialzier.ToBase64String(guid.ToByteArray());
            return data;
        }


    }
}
