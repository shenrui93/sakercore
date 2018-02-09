/***************************************************************************
 * 
 * 创建时间：   2016/3/4 11:21:37
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

using System;

namespace SakerCore.Serialization
{
    /// <summary>
    /// 标记类型序列化信息，该类型不应该序列化为h5对象信息
    /// </summary>
    public class NonH5SerializerAttribute:Attribute
    {
        /// <summary>
        /// 类NonH5Serializer的默认构造函数
        /// </summary>
        public NonH5SerializerAttribute()
        {
            //在这里实现对象的初始化操作
        }
    }
}
