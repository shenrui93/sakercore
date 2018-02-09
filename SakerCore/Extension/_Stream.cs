/***************************************************************************
 * 
 * 创建时间：   2017/1/5 11:21:08
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供 Stream 操作扩展
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
    /// 提供 Stream 操作扩展
    /// </summary>
    public static class _Stream
    {
        private static readonly Encoding defaultEncoding = Encoding.UTF8;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fs"></param>
        //拷贝数据流
        public static void CopyTo(this Stream stream, Stream fs)
        {
            if (!stream.CanRead) return;            //源字节流对象不支持读取
            if (!fs.CanWrite) return;                //目标字节流不支持写入

            byte[] buffer = new byte[512];
            while (true)
            {
                var read = stream.Read(buffer, 0, 512);
                if (read <= 0) break;
                fs.Write(buffer, 0, read);
            }
        }
        /// <summary>
        /// 开启一个异步拷贝
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fs"></param>
        /// <param name="callback"></param>
        public static void CopyToAsync(this Stream stream, Stream fs, Action callback)
        {
            if (callback == null)
                callback = DefaultValueManager.EmptyMethod;
            if (!stream.CanRead)
            {
                callback();
                return;            //源字节流对象不支持读取
            }
            if (!fs.CanWrite)
            {

                callback();
                return;                //目标字节流不支持写入
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static System.IO.StreamReader GetStreamReader(this Stream stream,Encoding encoding = null)
        {
            if(encoding == null)
            {
                encoding = defaultEncoding;
            }
            return new StreamReader(stream);
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static System.IO.StreamWriter GetStreamWriter(this Stream stream,Encoding encoding = null)
        {
            if(encoding == null)
            {
                encoding = defaultEncoding;
            }
            return new StreamWriter(stream,encoding);
        }

    }
}
