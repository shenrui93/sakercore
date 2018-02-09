/***************************************************************************
 * 
 * 创建时间：   2017/4/23 14:43:58
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   指示数组存储的大小端特性
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore
{
    /// <summary>
    /// 指示数组存储的大小端特性
    /// </summary>
    public enum OperationEndianOption
    {
        /// <summary>
        /// 大端存储
        /// </summary>
        BigEndian,
        /// <summary>
        /// 小段存储
        /// </summary>
        LittleEndian,
    }
}
