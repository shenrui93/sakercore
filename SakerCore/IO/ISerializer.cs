/***************************************************************************
 * 
 * 创建时间：   2017/4/21 14:25:36
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   实现自定义的序列化操作
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.IO
{
    /// <summary>
    /// 实现自定义的序列化操作
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// 将对象进行序列化
        /// </summary>
        /// <param name="operation"></param>
        void Serializer(IBinnaryOperation operation);
        /// <summary>
        /// 将对象进行反序列化
        /// </summary>
        /// <param name="operation"></param>
        void Deserializer(IBinnaryOperation operation);
    }
    ///// <summary>
    ///// 序列化操作基类
    ///// </summary>
    //public abstract class TypeSerializerOperationBase : ISerializer
    //{ 
    //    /// <summary>
    //    /// 反序列化
    //    /// </summary>
    //    /// <param name="stream"></param>
    //    public virtual void Deserializer(IUyiBinnaryOperation stream)
    //    {
            




    //    }
    //    /// <summary>
    //    /// 序列化
    //    /// </summary>
    //    /// <param name="stream"></param>
    //    public virtual void Serializer(IUyiBinnaryOperation stream)
    //    {

    //    }
    //}





}
