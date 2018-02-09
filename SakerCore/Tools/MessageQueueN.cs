

/*******************核心服务类******************************
 * 
 * 
 * 创建时间：2015年6月24日 11:57:12
 * 
 * 备注说明:1、该类提供一种一致的消息处理能力，重构 MessageQueue 实现逻辑；
 *          2、摒弃消息队列单线程结构，改用线程池模型处理
 *          3、此处采用系统的ThreadPool提供线程处理服务
 *          4、消息队列和线程不挂钩，线程池采用循环移动的方式在所有消息队列之间切换处理任务
 *          5、对于单个消息队列同一时间只允许有且最多一个线程访问和执行任务
 * 
 * ********************/


using System;
using System.Threading;
using SakerCore.Threading;
using System.Collections.Concurrent;

namespace SakerCore.Tools
{

    #region 单线程对列操作模型

    /// <summary>
    /// 该类提供一种一致的消息处理能力，重构 MessageQueue 实现逻辑；
    /// 该类是线程安全的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageQueue<T> : IMessageQueue<T> //where T : class
    {
        ConcurrentQueue<T> mq = new ConcurrentQueue<T>();

        const int timeWait = 0;
        const int MaxTryCount = 10;
        //处理消息超时时间最大秒数
        const int MaxProcessTime = 1;
        //状态维持对象
        private long _isDisposed = 0;
        private long _isRunning = 0;
        Thread currentThread;      //当前执行消息队列消息的处理线程
        MessageEventArgs<T> messageArg = new MessageEventArgs<T>();
        LongProcessMessage<T> longMessageArg = new LongProcessMessage<T>();
        /// <summary>
        /// 消息队列处理线程调度器，一般指定为线程池调度器
        /// </summary>
        private IThreadPoolProvider _threadProvider;
        /// <summary>
        /// 指示当前消息是否已经成功处理
        /// </summary>
        private bool IsProcessSuccess = true;
        /// <summary>
        /// 指示消息处理失败的重试时间，消息队列会被挂起
        /// </summary>
        private int tryCount = 0;
        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();


        /// <summary>
        /// 报告消息队列当前的消息处理失败，需要重新执行幂等逻辑，消息队列会重复通知处理当前消息，业务逻辑保证消息幂等处理逻辑的正确性
        /// </summary>
        /// <returns>操作成功的状态，如果操作成功则为true ,否则为false</returns>
        public bool SetFailRequestCurrentMesage()
        {
            if (Thread.CurrentThread.ManagedThreadId != this.currentThread.ManagedThreadId)
            {
                return false;
            }
            IsProcessSuccess = false;
            return true;
        }



        /// <summary>
        /// 实例化对象
        /// </summary>
        public MessageQueue()
            : this(false)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="haschecktimer"></param>
        public MessageQueue(bool haschecktimer)
            : this(haschecktimer, null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadProvider"></param>
        public MessageQueue(IThreadPoolProvider threadProvider)
            : this(false, threadProvider)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="haschecktimer"></param>
        /// <param name="threadProvider"></param>
        public MessageQueue(bool haschecktimer, IThreadPoolProvider threadProvider)
        {
            this._threadProvider = threadProvider ?? ThreadPoolProviderManager.Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        public EventHandler Disposed = (S, E) => { };
        /// <summary>
        /// 
        /// </summary>
        public EventHandle<IMessageQueue<T>, MessageEventArgs<T>> MessageComing { get; set; } = (S, E) => { };
        /// <summary>
        /// 处理时间过长的消息
        /// </summary>
        public EventHandle<IMessageQueue<T>, LongProcessMessage<T>> LongProcessMessage { get; set; } = (S, E) => { };

        /// <summary>
        /// 触发实例被释放前通知事件
        /// </summary>
        protected virtual void OnDisposing(object sender, EventArgs e)
        {
            Disposed(sender, e);
        }
        /// <summary>
        /// 触发新消息入队处理处理事件
        /// </summary> 
        /// <param name="e">所包含的事件参数对象</param>
        protected virtual void OnMessageComing(MessageEventArgs<T> e)
        {
            this.MessageComing(this, e);
        }


        //提供给线程池调用的方法
        void RunHandleProcess()
        {
            try
            {
                TryRunHandleProcess();
            }
            catch (System.Exception ex)
            {
                SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
            }
            finally
            {
                Interlocked.CompareExchange(ref _isRunning, 0, 1);
                if (!mq.IsEmpty)
                {
                    StartProcess();
                }
            }
        }

        private bool TryRunHandleProcess()
        {
            currentThread = Thread.CurrentThread;
            while (!mq.IsEmpty)
            {
                T msg;
                while (mq.TryPeek(out msg))
                {
                    IsProcessSuccess = true;
                    try
                    {
                        messageArg.Message = msg;
                        OnMessageComing(messageArg);
                        //sw.Stop();          //停止对消息处理时间的计数器
                        //if (sw.Elapsed.Seconds >= MaxProcessTime)
                        //{
                        //    this.longMessageArg.Message = msg;
                        //    this.longMessageArg.Elapsed = sw.Elapsed;
                        //    this.LongProcessMessage(this, this.longMessageArg);
                        //}
                    }
                    finally
                    {
                        if (IsProcessSuccess)
                        {
                            tryCount = 0;
                            mq.TryDequeue(out msg);
                        }
                        else
                        {
                            tryCount++;
                            if (tryCount >= MaxTryCount)
                            {
                                tryCount = 0;
                                mq.TryDequeue(out msg);
                            }
                            Thread.SpinWait(timeWait);
                        }
                    }
                }
            }
            return true;
        }
        private void StartProcess()
        {
            //启动消息队列的消息处理线程 
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0)
                return;
            this._threadProvider.QueueUserWorkItem(RunHandleProcess);
        }

        /// <summary>
        /// 将对象添加到集合结尾处
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            if (item == null) return;
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 1) != 0) return;
            mq.Enqueue(item);
            StartProcess();
        }
        /// <summary>
        /// 清理消息队列上的所有未处理消息
        /// </summary>
        public void Clear()
        {
            T msg;
            lock (this)
            {
                while (mq.TryDequeue(out msg)) ;
            }
        }
        /// <summary>
        /// 释放消息队列
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
                return;
            //var t = timer;
            //t?.Dispose();
            //timer = null;
            OnDisposing(this, EventArgs.Empty);
        }
        /// <summary>
        /// 重启消息队列的处理循环
        /// </summary>
        public void Restart()
        {
            if (!IsRunning)
                StartProcess();
            else
            {
                try
                {
                    var ct = currentThread;
                    try
                    {
                        ct?.Abort(this);
                    }
                    catch (System.Exception ex)
                    {
                        SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
                    }
                }
                catch (System.Exception ex)
                {
                    SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
                }

            }
        }


        /// <summary>
        /// 获取一个值,该值表示当前消息队列是否被线程池启动处理
        /// </summary>
        public bool IsRunning => Interlocked.Read(ref _isRunning) == 1;

        public int Count => mq.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageEventArgs<T> : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public T Message { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LongProcessMessage<T> : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public T Message { get; set; }
        /// <summary>
        /// 处理时间计数
        /// </summary>
        public TimeSpan Elapsed { get; set; }
    }



    #endregion

    #region MessageQueueMultiple


    /// <summary>
    /// 提供一种一致的消息处理能力，这个可以控制处理消息的线程数量，任务可以在多个线程中同步执行，一般消息互不干扰的情况下可以使用该类
    /// 该类是线程安全的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageQueueMultiple<T> : IMessageQueue<T> //where T : class
    {
        ConcurrentQueue<T> mq = new ConcurrentQueue<T>();
        //状态维持对象
        private long _isDisposed = 0;
        Semaphore _semaphore;
        IThreadPoolProvider _provider;

        #region 构造方法

        /// <summary>
        /// 实例化对象
        /// </summary>
        public MessageQueueMultiple()
            : this(Environment.ProcessorCount * 2)
        {
        }

        /// <summary>
        /// 实例化对象
        /// </summary>
        public MessageQueueMultiple(EventHandle<IMessageQueue<T>, MessageEventArgs<T>> call)
            : this(Environment.ProcessorCount * 2, call, null)
        {
        }

        /// <summary>
        /// 实例化对象
        /// </summary>
        public MessageQueueMultiple(int maxRunThreadCount) : this(maxRunThreadCount, null, null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxRunThreadCount"></param>
        /// <param name="call"></param>
        public MessageQueueMultiple(int maxRunThreadCount, EventHandle<IMessageQueue<T>, MessageEventArgs<T>> call) :
            this(maxRunThreadCount, call, null)
        {
        }

        /// <summary>
        /// 实例化对象
        /// </summary>
        public MessageQueueMultiple(int maxRunThreadCount, EventHandle<IMessageQueue<T>, MessageEventArgs<T>> call
            , IThreadPoolProvider provider)
        {
            if (provider == null)
            {
                provider = ThreadPoolProviderManager.Instance;
            }

            this._provider = provider;
            if (call != null)
            {
                MessageComing = call;
            }
            else
            {
                MessageComing = (S, E) => { };
            }
            _semaphore = new Semaphore(maxRunThreadCount, maxRunThreadCount);
        }


        #endregion

        /// <summary>
        /// 
        /// </summary>
        public EventHandler Disposed = (S, E) => { };
        /// <summary>
        /// 
        /// </summary>
        public EventHandle<IMessageQueue<T>, MessageEventArgs<T>> MessageComing { get; set; } = (s, e) => { };
        /// <summary>
        /// 处理时间过长的消息
        /// </summary>
        public EventHandle<IMessageQueue<T>, LongProcessMessage<T>> LongProcessMessage { get; set; } = (S, E) => { };

        public bool IsRunning => true;

        /// <summary>
        /// 触发实例被释放前通知事件
        /// </summary>
        protected virtual void OnDisposing(object sender, EventArgs e)
        {
            Disposed(sender, e);
        }
        /// <summary>
        /// 触发新消息入队处理处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">所包含的事件参数对象</param>
        protected virtual void OnMessageComing(object sender, MessageEventArgs<T> e)
        {
            this.MessageComing(this, e);
        }

        //提供给线程池调用的方法
        void RunHandleProcess()
        {
            try
            {
                TryRunHandleProcess();
            }
            catch (System.Exception ex)
            {
                SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
            }
            finally
            {
                _semaphore.Release();
                if (!mq.IsEmpty)
                {
                    StartProcess();
                }
            }
        }

        private void TryRunHandleProcess()
        {
            while (!mq.IsEmpty)
            {
                T msg;
                while (mq.TryDequeue(out msg))
                {
                    MessageEventArgs<T> messageArg = GetByPool();
                    messageArg.Message = msg;
                    OnMessageComing(this, messageArg);
                    RetPool(messageArg);
                }
            }
        }
        private void StartProcess()
        {
            //启动消息队列的消息处理线程 
            if (!_semaphore.WaitOne(0)) return;
            _provider.QueueUserWorkItem(RunHandleProcess);
        }

        /// <summary>
        /// 将对象添加到集合结尾处
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            if (item == null) return;
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 1) != 0) return;
            mq.Enqueue(item);
            StartProcess();
        }
        /// <summary>
        /// 清理消息队列上的所有未处理消息
        /// </summary>
        public void Clear()
        {
            T msg;
            while (mq.TryDequeue(out msg)) ;
        }
        /// <summary>
        /// 释放消息队列
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
                return;
            OnDisposing(this, EventArgs.Empty);
        }

        bool IMessageQueue<T>.SetFailRequestCurrentMesage()
        {
            return false;
        }
        void IMessageQueue<T>.Restart()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public int Count => mq.Count;





        private MessageEventArgs<T> GetByPool()
        {
            return new MessageEventArgs<T>();
        }
        private void RetPool(MessageEventArgs<T> messageArg)
        {
            //throw new NotImplementedException();
        }



    }


    #endregion

    #region MessageQueueDoublePriority

    /// <summary>
    /// 实现一个双优先级的消息队列，高级别的消息会优先得到处理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageQueueDoublePriority<T> : IMessageQueue<T> where T : IMessagePriority
    {

        /// <summary>
        /// 
        /// </summary>
        public EventHandle<IMessageQueue<T>, MessageEventArgs<T>> MessageComing { get; set; } = (s, e) => { };
        /// <summary>
        /// 处理时间过长的消息
        /// </summary>
        public EventHandle<IMessageQueue<T>, LongProcessMessage<T>> LongProcessMessage { get; set; } = (S, E) => { };
        System.Threading.SpinWait spin = new System.Threading.SpinWait();


        System.Collections.Concurrent.ConcurrentQueue<T> mq_high = new ConcurrentQueue<T>();
        ConcurrentQueue<T> mq_low = new ConcurrentQueue<T>();

        #region 初始化

        const int HignMq = 5;
        const int LowHignMq = 1;
        const int SpinRate = 10;

        const int timeWait = 0;
        const int MaxTryCount = 10;
        //状态维持对象
        private long _isDisposed = 0;
        private long _isRunning = 0;
        Thread currentThread;      //当前执行消息队列消息的处理线程
        MessageEventArgs<T> messageArg = new MessageEventArgs<T>();
        /// <summary>
        /// 消息队列处理线程调度器，一般指定为线程池调度器
        /// </summary>
        private IThreadPoolProvider _threadProvider;
        /// <summary>
        /// 指示当前消息是否已经成功处理
        /// </summary>
        private bool IsProcessSuccess = true;
        /// <summary>
        /// 指示消息处理失败的重试时间，消息队列会被挂起
        /// </summary>
        private int tryCount = 0;
        /// <summary>
        /// 实例化对象
        /// </summary>
        public MessageQueueDoublePriority()
            : this(false)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="haschecktimer"></param>
        public MessageQueueDoublePriority(bool haschecktimer)
            : this(haschecktimer, null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadProvider"></param>
        public MessageQueueDoublePriority(IThreadPoolProvider threadProvider)
            : this(false, threadProvider)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="haschecktimer"></param>
        /// <param name="threadProvider"></param>
        public MessageQueueDoublePriority(bool haschecktimer, IThreadPoolProvider threadProvider)
        {
            this._threadProvider = threadProvider ?? ThreadPoolProviderManager.Instance;
        }



        #endregion


        /// <summary>
        /// 触发新消息入队处理处理事件
        /// </summary> 
        /// <param name="e">所包含的事件参数对象</param>
        protected virtual void OnMessageComing(MessageEventArgs<T> e)
        {
            this.MessageComing(this, e);
        }


        //提供给线程池调用的方法
        void RunHandleProcess()
        {
            try
            {
                TryRunHandleProcess();
            }
            catch (System.Exception ex)
            {
                SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
            }
            finally
            {
                Interlocked.CompareExchange(ref _isRunning, 0, 1);
            }
        }
        private bool TryRunHandleProcess()
        {
            currentThread = Thread.CurrentThread;
            while (do_Process(mq_high, HignMq)) ;
            while (do_Process(mq_low, LowHignMq)) ;
            return true;
        }
        private bool do_Process(ConcurrentQueue<T> mq, int x)
        {
            spin.Reset();
            while (!mq.IsEmpty)
            {
                T msg;
                while (mq.TryPeek(out msg))
                {
                    IsProcessSuccess = true;
                    try
                    {
                        messageArg.Message = msg;
                        OnMessageComing(messageArg);
                    }
                    finally
                    {
                        if (IsProcessSuccess)
                        {
                            tryCount = 0;
                            mq.TryDequeue(out msg);
                        }
                        else
                        {
                            tryCount++;
                            if (tryCount >= MaxTryCount)
                            {
                                tryCount = 0;
                                mq.TryDequeue(out msg);
                            }
                        }
                    }
                    if (spin.Count >= SpinRate * x)
                    {
                        return false;
                    }
                    spin.SpinOnce();
                }
            }
            return false;
        }
        private void StartProcess()
        {
            //启动消息队列的消息处理线程 
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0)
                return;
            this._threadProvider.QueueUserWorkItem(RunHandleProcess);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            T v;
            while (!mq_high.IsEmpty)
            {
                mq_high.TryDequeue(out v);
            }
            while (!mq_low.IsEmpty)
            {
                mq_low.TryDequeue(out v);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            this.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {

            if (item == null) return;
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 1) != 0) return;
            switch (item.MPriority)
            {
                case MessagePriority.Hign:
                    mq_high.Enqueue(item);
                    break;
                default:
                    mq_low.Enqueue(item);
                    break;
            }
            StartProcess();



        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SetFailRequestCurrentMesage()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Restart()
        {
            if (!IsRunning)
                StartProcess();
            else
            {
                try
                {
                    var ct = currentThread;
                    try
                    {
                        ct?.Abort(this);
                    }
                    catch (System.Exception ex)
                    {
                        SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
                    }
                }
                catch (System.Exception ex)
                {
                    SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
                }

            }
        }


        /// <summary>
        /// 获取一个值,该值表示当前消息队列是否被线程池启动处理
        /// </summary>
        public bool IsRunning => Interlocked.Read(ref _isRunning) == 1;

        /// <summary>
        /// 
        /// </summary>
        public int Count => mq_high.Count + mq_low.Count;
    }

    #endregion

}
