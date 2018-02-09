/*************************************************************************
 * 
 * 创建时间： 2015年9月19日 20:10:09
 * 
 * 创建人员：沈瑞
 * 
 * 备注：
 * 
 * *************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace SakerCore.Serialization
{
    /// <summary>
    /// 定义一个接口，提供类型序列化和反序列化功能
    /// </summary>
    public interface ITypeSerializer
    {
        /// <summary>
        /// 将对象序列化为字节流
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        void Serialize(object obj, Stream stream);

        /// <summary>
        /// 将字节流反序列化为对象
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        object Deserialize(Stream stream);
        /// <summary>
        /// 非安全模式操作序列化
        /// </summary>
        /// <param name="stream">操作序列化的 byte 指针</param>
        /// <param name="length">当前序列化器数据长度</param>
        /// <param name="pos">读取游标</param>
        /// <returns></returns>
        unsafe object UnsafeDeserialize(byte* stream,int* pos, int length);

        /// <summary>
        /// 该序列花器支持序列化的类型对象
        /// </summary>
        Type SerializerType { get; }
        /// <summary>
        /// 获取该序列化器的名称
        /// </summary>
        string SerializerName { get; }
        /// <summary>
        /// 动态编译时序列化器的序列化操作方法信息
        /// </summary>
        System.Reflection.MethodInfo WriterInfo { get; set; }
        /// <summary>
        ///  动态编译时反序列化器的序列化操作方法信息
        /// </summary>
        System.Reflection.MethodInfo ReaderInfo { get; set; }
        /// <summary>
        ///  动态编译时反序列化器的序列化操作方法信息
        /// </summary>
        System.Reflection.MethodInfo UnsafeReaderInfo { get; set; }


    }

    /// <summary>
    /// 为对象的序列化提供自定义的类型序列化器
    /// </summary>
    public interface ICustomSerializer
    {
        /// <summary>
        /// 为对象执行序列化操作
        /// </summary>
        /// <param name="stream"></param>
        void Serialize(Stream stream);
        /// <summary>
        /// 为对象执行反序列化操作
        /// </summary>
        /// <param name="stream"></param>
        void Deserialize(Stream stream);
    }
}
