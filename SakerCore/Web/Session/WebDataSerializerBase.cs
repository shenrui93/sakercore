/***************************************************************************
 * 
 * 创建时间：   2017/8/15 13:10:59
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供对象基础验证和序列化操作
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using static SakerCore.Security.Encryption;
using SakerCore.Serialization;
using SakerCore.Extension;

namespace SakerCore.Web.Session
{
    /// <summary>
    /// 提供对象基础验证和序列化操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [SakerCore.Serialization.PacketContract]
    public abstract class WebDataSerializerBase<T> : IWebDataSerializer, IDataEncryption
        where T : class, IDataEncryption, new()
    {

        static IDataEncryption instance = new T();

        /// <summary>
        /// 比较字节数组
        /// </summary>
        /// <param name="b1">字节数组1</param>
        /// <param name="b2">字节数组2</param>
        /// <returns>如果两个数组相同，返回0；如果数组1小于数组2，返回小于0的值；如果数组1大于数组2，返回大于0的值。</returns>
        static bool MemoryCompare(byte[] b1, byte[] b2)
        {
            if (b1 == null || b2 == null) return false;

            var len1 = b1.Length;
            var len2 = b2.Length;
            if (len1 != len2) return false;

            for (int i = 0; i < len1; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }


        /// <summary>
        /// 指定类型 T 的类型序列化器
        /// </summary>
        static ITypeSerializer serializer;

        static WebDataSerializerBase()
        {
            serializer = SakerCore.Serialization.BinarySerializer.GetTypeSerializer(typeof(T));
            if (serializer == null)
                throw new System.Exception("未找到类型序列化器信息");
        }

        /// <summary>
        /// 将Base64字符串反序列化成对象
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static T GetSerializerData(string base64String)
        {
            try
            {
                var data = Base64Serialzier.FromBase64String(base64String);
                data = instance.Decode(data);
                using (var stream = new MemoryStream(data))
                {
                    return serializer.Deserialize(stream) as T;
                }
            }
            catch //(Exception ex)
            {
            }
            return null;
        }
        /// <summary>
        /// 把对象序列化成字符串的表示形式
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetBase64String(object value)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(value, stream);
                    var data = stream.ToArray();
                    data = instance.Encode(data);
                    return Base64Serialzier.ToBase64String(data);
                }
            }
            catch
            {
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public abstract byte[] MarkSign(string secret);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public virtual bool VerifySign(string secret)
        {
            return MemoryCompare(this.Sign, MarkSign(secret));
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void MarkTimestamp()
        {
            this.Timestamp = (uint)DateTime.Now.GetTimestamp();
        }
        /// <summary>
        /// 检查当前请求是否已经超时
        /// </summary>
        /// <param name="timespan">验证超时阈值（单位：秒）</param>
        /// <returns>返回一个布尔值表示当前的请求是否超时，如果已超时返回 true 否则 返回 false</returns>
        public virtual bool CheckTimeOut(int timespan)
        {
            DateTime dtStart = WebParamData.LocalUTCBegin.AddSeconds(this.Timestamp);
            //获取当前时间戳表示的时间信息
            var now = DateTime.Now;
            return Math.Abs((now - dtStart).TotalSeconds) >= timespan;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public virtual string Serializer(string secret)
        {
            this.MarkTimestamp();
            this.Sign = this.MarkSign(secret);
            return GetBase64String(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] IDataEncryption.Decode(byte[] data)
        {
            return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] IDataEncryption.Encode(byte[] data)
        {
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        [PacketMember(1023)]
        public uint Timestamp;
        /// <summary>
        /// 
        /// </summary>
        [PacketMember(1024)]
        public byte[] Sign;

        /// <summary>
        /// 请求数据的版本
        /// </summary>
        public virtual byte Version { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class WebDataSerializerProviderBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session_key"></param>
        /// <param name="base64Str"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        protected static bool GetDataVersion(string session_key, out string base64Str, out byte version)
        {
            base64Str = null;
            version = 0;

            if (session_key.Length < 3) return false;

            char sp = session_key[1];
            char v = session_key[0];
            if (sp != '$')
            {
                base64Str = session_key;
                return true;
            }

            base64Str = session_key.Substring(2);

            if (v >= '0' && v <= '9')
            {
                version = (byte)(v - '0');
                return true;
            }

            return false;



        }

    }
}
