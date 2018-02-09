using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SakerCore.Serialization
{
    /// <summary>
    /// 服务器序列化操作支持类
    /// </summary>
    public class BinarySerializer
    {
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] Serialize(object message)
        {
            // 对象运行时类型
            var type = message.GetType();
            //获取对象序列化器
            var serialize = TypeSerializationHelper.GetTypeSerializer(type);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            serialize.Serialize(message, ms);
            //返回序列化的字节数组
            return ms.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ushort GetTypeCode(Type type)
        {
            ushort v;
            if (TypeSerializerFactory.TryGetSerializerCode(type, out v))
            {
                return v;
            }
            throw Error.NotFindSerializer(type.FullName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeCode"></param>
        /// <returns></returns>
        public static bool CanDeserialize(int typeCode)
        {
            return TypeSerializationHelper.CanDeserialize(typeCode);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static bool CanDeserialize(int typeCode,out ITypeSerializer serializer)
        {
            return TypeSerializerFactory.TryGetSerializer(typeCode,out serializer);
        }
        /// <summary>
        /// 进行对象的反序列化操作
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="typeCode"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static object Deserialize(System.IO.MemoryStream ms, int typeCode ,ITypeSerializer serializer)
        {  
            return serializer?.Deserialize(ms);
        }


        /// <summary>
        /// 进行对象的反序列化操作
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="serializer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public unsafe static object UnsafeDeserialize(byte* stream, int length, ITypeSerializer serializer)
        {  
            unsafe
            {
                int _pos = 0;
                int* pos = &_pos;
                return serializer?.UnsafeDeserialize(stream, pos, length);
            };

        }




        /// <summary>
        /// 获取对象序列化器
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ITypeSerializer GetTypeSerializer(object obj)
        {
            try
            {
                var serialize = TypeSerializationHelper.GetTypeSerializer(obj.GetType());
                return serialize;
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// 获取指定类型的对象序列化器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ITypeSerializer GetTypeSerializer(Type type)
        {
            try
            {
                var serialize = TypeSerializationHelper.GetTypeSerializer(type);
                return serialize;
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeCode"></param>
        /// <returns></returns>
        public static Type GetSerializerType(int typeCode)
        {
            var serialize = TypeSerializationHelper.GetTypeSerializer(typeCode.ToString());
            return serialize?.SerializerType;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static class DebugHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void RuningLog(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            msg = $"SerializerDebug==>>【{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}】 {msg} <<==SerializerDebug";
            if (LogMessage != null)
            {
                LogMessage(null, new DebugEventArgs() { Log = msg });
            }
        }
        /// <summary>
        /// 日志信息的通知事件
        /// </summary>
        public static event EventHandler<DebugEventArgs> LogMessage;
    }
    /// <summary>
    /// 
    /// </summary>
    public class DebugEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public string Log { get; internal set; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class SerializationErrorEvent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        internal static void OnErrorEvent(System.Exception ex)
        {
            SystemErrorProvide.OnSystemErrorHandleEvent(null, ex);
        }
    }
}