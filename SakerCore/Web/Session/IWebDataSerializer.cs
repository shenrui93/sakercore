/***************************************************************************
 * 
 * 创建时间：   2017/8/25 9:22:00
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   为对象基础验证和序列化操作定义操作接口
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.Web.Session
{
    /// <summary>
    /// 为对象基础验证和序列化操作定义操作接口
    /// </summary>
    public interface IWebDataSerializer
    {
        /// <summary>
        /// 序列化当前对象
        /// </summary>
        /// <param name="secret">提供序列化密钥</param>
        /// <returns></returns>
        string Serializer(string secret);
        /// <summary>
        /// 验证密钥签名的正确性
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        bool VerifySign(string secret);
        /// <summary>
        /// 验证当前的请求是否过期
        /// </summary>
        /// <param name="ex_time"></param>
        /// <returns></returns>
        bool CheckTimeOut(int ex_time); 
        /// <summary>
        /// 版本号
        /// </summary>
        byte Version { get; set; }

    }
    /// <summary>
    /// 
    /// </summary>
    public interface IDataEncryption
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] Decode(byte[] data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] Encode(byte[] data);

    }
}
