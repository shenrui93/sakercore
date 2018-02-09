using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SakerCore.Serialization
{
    /// <summary>
    /// 类型序列化器基类，提供类型序列化的基础方法实现
    /// </summary>
    public abstract class TypeSerializationBase<T> : ITypeSerializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pos"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected unsafe delegate T delUnsafeReaderMethod(byte* stream,int* pos, int length);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="stream"></param>
        /// <param name="pos"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected unsafe delegate TR delUnsafeReaderMethod<TR>(byte* stream,int* pos, int length);


        private MethodInfo _writerInfo;
        private MethodInfo _readerInfo;
        private MethodInfo _unsafeReaderInfo;
        private BindingFlags methodFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;


        /// <summary>
        /// 当前序列化器的序列化方法的
        /// </summary>
        public virtual MethodInfo WriterInfo
        {
            get
            {
                return _writerInfo ?? (_writerInfo = (GetWriterMethod()));
            }
            set
            {
                this._writerInfo = value;
            }
        }
        /// <summary>
        /// 当前序列化器的反序列化方法信息
        /// </summary>
        public virtual MethodInfo ReaderInfo
        {
            get
            {
                return _readerInfo ?? (_readerInfo = (GetReaderMethod()));
            }
            set
            {
                this._readerInfo = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual MethodInfo UnsafeReaderInfo
        {
            get
            {
                return _unsafeReaderInfo ?? (_unsafeReaderInfo = (GetUnsafeReaderMethod()));
            }
            set
            {
                _unsafeReaderInfo = value;
            }
        }





        /// <summary>
        /// 表示需要序列化的类型对象
        /// </summary>
        protected Type _serializerType;
        /// <summary>
        /// 类型序列化器名称，未指定默认使用类型GUID全名
        /// </summary>
        protected string _serializerName;


        /// <summary>
        /// 将对象进行序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public abstract void Serialize(T obj, Stream stream);
        /// <summary>
        /// 将对象反序列化为对象
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public abstract T Deserialize(Stream stream);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pos"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public unsafe virtual T UnsafeDeserialize(byte* stream,int* pos, int length)
        {
            return default(T);
        }


        /// <summary>
        /// 创建一个新的对象实例
        /// </summary>
        /// <returns></returns>
        public virtual object CreateNewObject()
        {
            return new object();
        }
        /// <summary>
        /// 当前序列化器的支持的序列化类型
        /// </summary>
        public virtual Type SerializerType
        {
            get { return this._serializerType; }
        }
        /// <summary>
        /// 当前序列化的序列化器的名称
        /// </summary>
        public virtual string SerializerName
        {
            get { return this._serializerName; }
        }


        /// <summary>
        /// 将当前的序列化输出为字符串的表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("类型：{0} 的动态序列化器", this._serializerType.ToString());
        }

        void ITypeSerializer.Serialize(object obj, Stream stream)
        {
            this.Serialize((T)obj, stream);
        }
        object ITypeSerializer.Deserialize(Stream stream)
        {
            return this.Deserialize(stream);
        }
        /// <summary>
        /// 非安全模式操作序列化
        /// </summary>
        /// <param name="stream">操作序列化的 byte 指针</param>
        /// <param name="length">当前序列化器数据长度</param>
        /// <param name="pos">读取游标</param>
        /// <returns></returns>
        unsafe object ITypeSerializer.UnsafeDeserialize(byte* stream,int* pos, int length)
        {
            return this.UnsafeDeserialize(stream,pos, length);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual MethodInfo GetReaderMethod()
        {
            var thistype = GetType();
            return GetType().GetMethod("Read"
                , methodFlag, null, CallingConventions.Any
                , new Type[] { typeof(Stream) }, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual MethodInfo GetUnsafeReaderMethod()
        { 
            return GetType().GetMethod("UnsafeRead");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual MethodInfo GetWriterMethod()
        {
            return GetType().GetMethod("Write");
        }
    }



}
