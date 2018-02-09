/***************************************************************************
 * 
 * 创建时间：   2016/7/19 18:41:08
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供基础流写操作
 * 
 * *************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Threading;
//using System.Diagnostics.Contracts;

namespace SakerCore.IO
{
    /// <summary>
    /// 提供基础流写操作
    /// </summary>
    public class StreamWriter : IDisposable
    {

        Stream baseStream;
        NetworkStream ms = new NetworkStream();
        Encoding baseEncoding;
        long _isDisposed = 0;
        long _isClose = 0;

        /// <summary>
        /// 用指定的编码及默认缓冲区大小，为指定的流初始化 <see cref="StreamWriter"/> 类的新实例。
        /// 不会再流的开头添加编码信息
        /// </summary>
        /// <param name="stream">要写入的流</param>
        /// <param name="encoding">要使用的字符编码</param>
        public StreamWriter(Stream stream, Encoding encoding)
        {
            baseStream = stream;
            baseEncoding = encoding;
        }
        /// <summary>
        /// 用默认的UTF-8编码及默认缓冲区大小，为指定的流初始化 <see cref="StreamWriter"/> 类的新实例。
        /// 不会再流的开头添加编码信息
        /// </summary>
        /// <param name="stream"></param>
        public StreamWriter(Stream stream) : this(stream, Encoding.UTF8)
        {

        }
        /// <summary>
        /// 关闭操作对象
        /// </summary>
        public virtual void Close()
        {
            if (Interlocked.CompareExchange(ref _isClose, 1, 0) != 0) return;
            Flush();
            ms.Close();
        }
        /// <summary>
        /// 清空缓冲区，并将缓冲区的对象写到基础对象中
        /// </summary>
        public virtual void Flush()
        {
            if (Interlocked.Read(ref _isClose) != 0) return;
            ms.CopyTo(baseStream);
        }
        /// <summary>
        /// 将指定的字符串内容写入到响应流中
        /// </summary>
        /// <param name="value">需要写入的字符串内容</param>
        public virtual void Write(string value)
        {
            if (Interlocked.Read(ref _isClose) != 0) return;
            var data = baseEncoding.GetBytes(value);
            ms.Write(data, 0, data.Length);
        }
        /// <summary>
        /// 将指定的字符串内容写入到响应流中，并在其字符串尾部追加换行符
        /// </summary>
        /// <param name="value">需要写入的字符串内容</param>
        public virtual void WriteLine(string value)
        {
            this.Write(value + Environment.NewLine);
        }
        /// <summary>
        /// 释放其对象
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            Close();
            ms.Dispose();
        }
    }
     

}
