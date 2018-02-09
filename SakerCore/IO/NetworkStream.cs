/***************************************************************************
 * 
 * 创建时间：   2016/4/14 14:16:56
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供一个只进的读写流。该类是线程安全的
 * 
 * *************************************************************************/

using System;
using System.IO;

namespace SakerCore.IO
{
    /// <summary>
    /// 提供一个只进的读写流。该类是线程安全的
    /// </summary>
    public class NetworkStream : Stream
    {
        const int MaxBufferCount = 1024 * 2;
        const int MaxBufferCat = 1024 * 4;
        int _isDisposed = 0;

        static readonly byte[] EmptyBytes = new byte[0];
        /// <summary>
        /// 
        /// </summary>
        public NetworkStream()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public NetworkStream(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            this.Write(data, 0, data.Length);
        }

        byte[] _buffer = new byte[256];
        int cat = 256;
        int count = 0;
        object root = new object();

        private void WriteInternal(byte[] buffer, int offset, int count)
        {
            lock (root)
            {
                var newcount = this.count + count;
                if (newcount > cat)
                {
                    while (newcount > cat)
                    {
                        cat *= 2;
                    }
                    var newBuffer = new byte[cat];
                    InternalBlockCopy(_buffer, 0, newBuffer, 0, this.count);
                    this._buffer = newBuffer;
                }
                InternalBlockCopy(buffer, offset, this._buffer, this.count, count);
                this.count = newcount;
            }
        }
        private int ReadInternal(byte[] buffer, int offset, int count)
        {
            lock (root)
            {
                if (this.count == 0) return 0;
                var relCount = this.count > count + offset ? count : this.count - offset;
                InternalBlockCopy(this._buffer, 0, buffer, offset, relCount);
                var newcount = this.count - relCount;
                if (newcount != 0)
                {
                    InternalBlockCopy(this._buffer, relCount, this._buffer, 0, newcount);
                }
                this.count = newcount;
                InitBuffer();
                return relCount;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public new void CopyTo(Stream stream)
        {
            if (!stream.CanWrite) return;


            byte[] buffer = new byte[512];
            while (true)
            {
                var read = this.Read(buffer, 0, 512);
                if (read <= 0) break;
                stream.Write(buffer, 0, read);
            }
        }

        private int ReadOnlyInternal(byte[] buffer, int offset, int count)
        {
            lock (root)
            {
                if (this.count == 0) return 0;
                var relCount = this.count > count + offset ? count : this.count - offset;
                InternalBlockCopy(this._buffer, offset, buffer, 0, relCount);
                return relCount;
            }
        }
        private byte[] ReadBytesInternal(int offset, int count)
        {
            lock (root)
            {
                if (this.count == 0) return EmptyBytes;
                var relCount = this.count > count + offset ? count : this.count - offset;
                var buffer = new byte[relCount];
                InternalBlockCopy(this._buffer, 0, buffer, offset, relCount);
                var newcount = this.count - relCount;
                if (newcount != 0)
                {
                    InternalBlockCopy(this._buffer, relCount, this._buffer, 0, newcount);
                }
                this.count = newcount;
                InitBuffer();
                return buffer;
            }
        }
        private byte[] ReadOnlyBytesInternal(int offset, int count)
        {
            lock (root)
            {
                if (this.count == 0) return EmptyBytes;
                var relCount = this.count > count + offset ? count : this.count - offset;
                var buffer = new byte[relCount];
                InternalBlockCopy(this._buffer, offset, buffer, 0, relCount);
                return buffer;
            }
        }
        private byte[] ReadAllBytesInternal()
        {
            lock (root)
            {
                return ReadBytesInternal(0, this.count);
            }
        }
        private byte[] ReadOnlyAllBytesInternal()
        {
            lock (root)
            {
                return ReadOnlyBytesInternal(0, this.count);
            }
        }
        private void RemoveInternal(int count)
        {
            lock (root)
            {
                var newcount = this.count - count;
                if (newcount > 0)
                {
                    InternalBlockCopy(this._buffer, count, this._buffer, 0, newcount);
                }
                this.count = newcount < 0 ? 0 : newcount;
                InitBuffer();
            }
        }
        private void InitBuffer()
        {
            //重新调整缓冲区大小增加伸缩性
            if (this.cat >= MaxBufferCat && this.count <= MaxBufferCount)
            {
                this.cat = MaxBufferCount;
                var newbuffer = new byte[MaxBufferCount];
                if (this.count > 0)
                    InternalBlockCopy(this._buffer, 0, newbuffer, 0, this.count);
                this._buffer = newbuffer;
            }

        }
        /// <summary>
        /// 读取一个字节数据
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            lock (root)
            {
                if (this.count == 0) return -1;
                return base.ReadByte(); 
            }
        }

        #region Stream  相关成员
        /// <summary>
        /// 只是该流是否支持读取
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// 指示该流是否支持查找
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 指示该流是否支持写入
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// 获取该流的数据长度
        /// </summary>
        public override long Length
        {
            get
            {
                lock (root)
                {
                    return this.count;
                }
            }
        }
        /// <summary>
        /// 获取或者设置该流的游标位置
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// 将数据写入基础支持流并清空缓冲区
        /// </summary>
        public override void Flush()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 读取并删除读取的数据
        /// </summary> 
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadInternal(buffer, offset, count);
        }
        /// <summary>
        /// 设置当前流的游标位置
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="origin"></param>
        /// <returns>该流不支持该方法，调用该方法永远引发 <see cref="NotSupportedException"/> 异常</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// 设置数据流的长度，执行该操作一般会导致截断数据
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            lock (this.root)
            {
                this.count = (int)value;
                InitBuffer();
            }
        }
        /// <summary>
        /// 向流的末尾处追加数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteInternal(buffer, offset, count);
        }

        #endregion 

        /// <summary>
        /// 长度
        /// </summary>
        public int Count
        {
            get
            {
                lock (root)
                {
                    return this.count;
                }
            }
        }
        /// <summary>
        /// 获取指定索引处的字节数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index]
        {
            get
            {
                lock (root)
                {
                    return this._buffer[index];
                }
            }
        } 
        /// <summary>
        /// 读取但不删除数据
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadArray(int startIndex, int count)
        {
            return this.ReadOnlyBytesInternal(startIndex, count);
        }
        /// <summary>
        /// 读取并删除读取的数据
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadAndRemoveBytes(int count)
        {
            return this.ReadBytesInternal(0, count);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="count"></param>
        public void Remove(int count)
        {
            RemoveInternal(count);
        }
        /// <summary>
        /// 返回缓冲区内所有数据的一个数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return ReadOnlyAllBytesInternal();
        }  


        /// <summary>
        /// 返回缓冲区内所有数据的一个数组，并清空缓存区
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ToArray()
        {
            return ReadAllBytesInternal();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Write(byte[] data)
        { 
            Write(data, 0, data.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (System.Threading.Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
             
        }


        void InternalBlockCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
        }

    }


}
