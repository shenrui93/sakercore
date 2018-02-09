/***************************************************************************
 * 
 * 创建时间：   2016/12/6 12:15:46
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供一个异步操作Socket网络通讯的网络流
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SakerCore.Net
{
    /// <summary>
    /// 提供一个异步操作Socket网络通讯的网络流
    /// </summary>
    public sealed class SocketNetworkStream : Stream, IDisposable
    {
        private Socket baseSocket;      //指示基础通讯Socket对象
        int _isReceive = 0;             //指示当前对象是否处于接收状态
        int _isSend = 0;                //指示当前对象是否处于发送数据状态
        int _isDisposed = 0;            //指示当前对象是否已经被释放

        //发送数据的异步通讯对象
        private SocketAsyncEventArgsMetadata send;
        //接收数据的异步通讯对象
        private SocketAsyncEventArgsMetadata receive;


        SocketNetworkStream_AsyncResult writer_iar = new SocketNetworkStream_AsyncResult();
        SocketNetworkStream_AsyncResult read_iar = new SocketNetworkStream_AsyncResult();


        ManualResetEvent read_are = new ManualResetEvent(false);
        ManualResetEvent write_are = new ManualResetEvent(false);



        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="baseSocket"></param>
        public SocketNetworkStream(Socket baseSocket)
        {
            this.baseSocket = baseSocket;
            Initializer();
        }

        private void Initializer()
        {
            send = SocketAsyncEventArgsPool.GetNewAsyncEventArgs();
            receive = SocketAsyncEventArgsPool.GetNewAsyncEventArgs();
            //绑定异步套接字操作对象的Socket对象
            send.AcceptSocket = receive.AcceptSocket = this.baseSocket;

            receive.Completed += Receive_Completed;
            send.Completed += Send_Completed;


        }

        private void Send_Completed(object sender, SocketAsyncEventArgsMetadata e)
        {
            var iar = e.UserToken as SocketNetworkStream_AsyncResult;
            if (iar == null)
            {
                //出现意外的调用命令
                this.Dispose();
                return;
            }
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                iar.Result = e.BytesTransferred;
            }
            else
            {
                iar.Result = 0;
                iar.ex = new System.Exception("远程主机已经关闭连接");
            }
            Send_End();
            iar.CallComplete();
        }
        private void Receive_Completed(object sender, SocketAsyncEventArgsMetadata e)
        {
            var iar = e.UserToken as SocketNetworkStream_AsyncResult;
            if (iar == null)
            { 
                //出现意外的调用命令
                this.Dispose();
                return;
            }
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                iar.Result = e.BytesTransferred;
                StatisticsManage.AddReceiveBytes(iar.Result);
            }
            else
            { 
                iar.Result = 0;
                iar.ex = new System.Exception("远程主机已经关闭连接");
            }
            Receive_End();
            iar.CallComplete();
        }
        private void Receive_End()
        {
            Interlocked.CompareExchange(ref _isReceive, 0, 1);
        }
        private void Send_End()
        {
            Interlocked.CompareExchange(ref _isSend, 0, 1);
        }
        /// <summary>
        /// 指示当前流是否支持读取
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// 指示当前流是否支持查找
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 指示当前流是否支持写入
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// 获取当前流的长度
        /// </summary>
        public override long Length
        {
            get
            {
                throw new NotSupportedException("该流不支持查找");
            }
        }
        /// <summary>
        /// 获取或者设置当前流的游标
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotSupportedException("该流不支持查找");

            }

            set
            {
                throw new NotSupportedException("该流不支持查找");

            }
        }
        /// <summary>
        /// 完成数据写入，将数据发送到基础数据流
        /// </summary>
        public override void Flush()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                var iar = InternalBeginRead(buffer, offset, count, SynchronizationReadCallback, null,read_are.Reset); 
                read_are.WaitOne();
                return this.EndRead(iar);
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                var iar = InternalBeginWrite(buffer, offset, count, SynchronizationWriteCallback, null, write_are.Reset);
                write_are.WaitOne();
                this.EndWrite(iar);
            }
            catch
            {
                throw;
            }
        } 

        private void SynchronizationWriteCallback(IAsyncResult ar)
        {
            write_are.Set(); 
        }
        private void SynchronizationReadCallback(IAsyncResult ar)
        {
            read_are.Set();
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("该流不支持查找");

        }
        /// <summary>
        /// 设置当前数据流的数据流长度（该方法不支持）
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("该流不支持查找");

        }



        /// <summary>
        /// 开始一个异步读取操作
        /// </summary>
        /// <param name="buffer">需要将数据读入的数据缓冲区</param>
        /// <param name="offset">缓冲区的数据偏移量</param>
        /// <param name="count">尝试读取的数据长度</param>
        /// <param name="callback">操作完成的通知回调函数</param>
        /// <param name="state">用户操作对象</param> 
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return InternalBeginRead(buffer, offset, count, callback, state, SakerCore.DefaultValueManager.BoolEmptyCall);
        }
        /// <summary>
        /// 开始一个异步写入数据操作
        /// </summary>
        /// <param name="buffer">需要将数据读入的数据缓冲区</param>
        /// <param name="offset">缓冲区的数据偏移量</param>
        /// <param name="count">尝试读取的数据长度</param>
        /// <param name="callback">操作完成的通知回调函数</param>
        /// <param name="state">用户操作对象</param>
        /// <returns></returns>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return InternalBeginWrite(buffer, offset, count, callback, state, SakerCore.DefaultValueManager.BoolEmptyCall);
        }
        IAsyncResult InternalBeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state, Func<bool> beforcall)
        {
            if (Interlocked.CompareExchange(ref _isReceive, 1, 0) != 0)
            {
                throw new System.Exception("已经有一个数据接收的任务正在执行");
            }
            try
            {
                read_iar._asyncState = state;
                read_iar._cb = callback;
                receive.SetBuffer(buffer, offset, count);
                receive.UserToken = read_iar;
                if (!this.baseSocket.ReceiveAsync(receive))
                {
                    beforcall();
                    Receive_Completed(read_iar, receive);
                }
                else
                {
                    beforcall();
                }
                return read_iar; ;
            }
            catch
            {
                Receive_End();
                this.Dispose();
                throw;
            } 
        }
        IAsyncResult InternalBeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state, Func<bool> beforcall)
        { 
            if (Interlocked.CompareExchange(ref _isSend, 1, 0) != 0)
            {
                throw new System.Exception("已经有一个数据发送的任务正在执行");
            }
            try
            {
                writer_iar._asyncState = state;
                writer_iar._cb = callback;
                send.SetBuffer(buffer, offset, count);
                send.UserToken = writer_iar;
                if (!this.baseSocket.SendAsync(send))
                {
                    beforcall();
                    Send_Completed(writer_iar, send);
                }
                else
                { 
                    beforcall();
                }
                return writer_iar; ;
            }
            catch
            {
                Send_End();
                this.Dispose();
                throw;
            } 
        } 



        /// <summary>
        /// 结束一个异步读取操作
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public override int EndRead(IAsyncResult asyncResult)
        {
            var iar = asyncResult as SocketNetworkStream_AsyncResult;
            if (iar == null)
            {
                throw new System.Exception("");
            }
            //if (iar.ex != null) throw new Exception("调用的目标发生异常", iar.ex);
            return iar.Result;
        }
        /// <summary>
        /// 结束一个异步写入操作
        /// </summary>
        /// <param name="asyncResult"></param>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            var iar = asyncResult as SocketNetworkStream_AsyncResult;
            if (iar == null)
            {
                throw new System.Exception("指定的 iar 对象对于当前操作无效");
            }
            if (iar.ex != null) throw iar.ex;
        }

        /// <summary>
        /// 指示当前的流是否支持超时
        /// </summary>
        public override bool CanTimeout
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 获取当前网络流的基础通讯Socket
        /// </summary>
        public Socket BaseSocket
        {
            get
            {
                return baseSocket;
            }
        }
        /// <summary>
        /// 关闭当前流
        /// </summary>
        public override void Close()
        {
            this.Dispose(true);
        }
        /// <summary>
        /// 释放当前流占用的托管和非托管资源
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;

            this.baseSocket?.Close();
            this.send?.Dispose();
            this.receive?.Dispose(); 
        }

        #region SocketNetworkStream_AsyncResult

        class SocketNetworkStream_AsyncResult : IAsyncResult
        {
            public object _asyncState;
            public AsyncCallback _cb;
            public int Result;
            public System.Exception ex;
            private bool _isComplete = false;

            public object AsyncState => _asyncState;
            public WaitHandle AsyncWaitHandle => null;
            public bool CompletedSynchronously => false;
            public bool IsCompleted => _isComplete;

            public void CallComplete()
            {
                _isComplete = true;
                _cb?.Invoke(this);
            }
        }

        #endregion


    }
}
