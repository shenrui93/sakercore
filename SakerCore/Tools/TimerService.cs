

using System;
using System.Collections.Generic;
using System.Threading;
//using Uyi.Extension;
//using System.Linq;
//using System.Collections.Concurrent;

namespace SakerCore.Tools
{
    /// <summary>
    /// 定时器服务提供程序
    /// </summary>
    public static class TimerService
    {

        const string SystemTimeChanged = "1";
        const string TimeTicked = "2";

        static int _checkTimer = 0;


        internal static int _serverBaseTimeInterval = 1000;

        /// <summary>
        /// 
        /// </summary>
        public static int ServerBaseTimeInterval => _serverBaseTimeInterval;

        //服务器定时器,提供基础的服务器计时服务，这是所有计时服务的根
        internal static Timer ServerTimer;
        //static object root = new object();

        static ReaderWriterLock rw_root = new ReaderWriterLock();

        static MessageQueue<string> _tickTaskQueue = new MessageQueue<string>();

        static TimerService()
        {
            _tickTaskQueue.MessageComing = _tickTaskQueue_MessageComing;
            ServerTimer = new Timer(ServerTimer_Elapsed, null, 1000, 1000);
            //SystemEvents.TimeChanged += SystemEvents_TimeChanged;
        }

        #region 定时器核心管理方法




        //这是服务器的计时器管理集合
        static Dictionary<long, ServerTimerBase> _serverTimerList = new Dictionary<long, ServerTimerBase>();
        static List<ServerTimerBase> l = new List<ServerTimerBase>();
        private static List<ServerTimerBase> ToList()
        {
            rw_root.AcquireReaderLock(-1);
            try
            {
                l.Clear();
                foreach (var r in _serverTimerList)
                {
                    if (r.Value == null) continue;
                    l.Add(r.Value);
                }
                return l;
            }
            finally
            {
                rw_root.ReleaseReaderLock();
            }

        }
        private static bool ServerTryAdd(long skey, ServerTimerBase timer)
        {
            rw_root.AcquireWriterLock(-1);
            try
            {
                {
                    if (_serverTimerList.ContainsKey(skey)) return false;
                    _serverTimerList.Add(skey, timer);
                    return true;
                }
            }
            finally
            {
                rw_root.ReleaseWriterLock();
            }
        }
        private static bool ServerTryRemove(long key, out ServerTimerBase v)
        {
            rw_root.AcquireWriterLock(-1);
            try
            {
                _serverTimerList.TryGetValue(key, out v);
                return _serverTimerList.Remove(key);

            }
            finally
            {
                rw_root.ReleaseWriterLock();
            }
        }
        /// <summary>
        /// 获取当前定时的数量
        /// </summary>
        public static int Count => _serverTimerList.Count;





        #endregion

        private static void _tickTaskQueue_MessageComing(IMessageQueue<string> sender, MessageEventArgs<string> e)
        {
            var msg = e.Message;
            switch (msg)
            {
                case SystemTimeChanged:       //系统时间修改时发生的处理方法
                    {
                        var l = ToList();
                        l.ForEach(p =>
                        { 
                            try
                            {
                                p.SystemEventsTimeChangedInitializer();
                            }
                            catch //(Exception)
                            {
                            }
                        });
                    }
                    break;
                case TimeTicked:           //服务器一个计时周期完成的处理方法
                    {
                        _checkTimer = 1;
                        try
                        {
                            var l = ToList();
                            l.ForEach(p =>
                            { 
                                var now = DateTime.Now;
                                p.Tick(now);
                            });
                        }
                        finally
                        {
                            _checkTimer = 0;
                        }
                    }
                    break;
            }

        }
        //提供服务器计时器生成服务
        static long ServerTimerID = long.MinValue;
        static long GetNewTimerKey()
        {
            long newid = long.MinValue;
            do
            {
                newid = Interlocked.Increment(ref ServerTimerID);
            }
            while (newid == long.MinValue);
            return newid;
        }
        static void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            //系统时间修改时发生的处理方法
            //_tickTaskQueue.Enqueue(SystemTimeChanged);
        }
        static void ServerTimer_Elapsed(object state)
        {
            if (_checkTimer == 1) return;
            _tickTaskQueue.Enqueue(TimeTicked);
        }


        /// <summary>
        /// 注册定时器,将定时器添加到定时器集合以便宿主提供计时服务
        /// </summary>
        /// <param name="timer"></param>
        internal static void RegistrationTimer(ServerTimerBase timer)
        {
            while (true)
            {
                var skey = GetNewTimerKey();
                if (!ServerTryAdd(skey, timer))
                    continue;
                timer._id = skey;
                break;
            }
        }
        /// <summary>
        /// 销毁定时器
        /// </summary>
        /// <param name="key"></param>
        internal static void DestroyTimer(long key)
        {
            ServerTimerBase v;
            ServerTryRemove(key, out v);
        }



        /// <summary>
        /// 创建一个基础的服务器定时器实例
        /// </summary>
        /// <param name="callback">定时器回调执行的函数</param>
        /// <param name="timeInterval">定时器时间间隔</param>
        /// <returns></returns>
        public static IServerTimerBase CreateServerTimer(TimerCallBackDelegate callback, int timeInterval)
        {
            var timer = new ServerTimerInterval()
            {
                tickfunc = callback,
                TimeInterval = timeInterval
            };
            return timer;
        }
        /// <summary>
        /// 创建一个基础的服务器定时器实例,这个定时器会在每日指定的时间到达时执行方法
        /// </summary>
        /// <param name="callback">需要定时执行的方法</param>
        /// <param name="time">事件执行的定时时间定时时间</param>
        /// <returns></returns>
        public static IServerTimerBase CreateServerTimer(TimerCallBackDelegate callback, TimeSpan time)
        {
            var timer = new ServerTimerTimeSpan()
            {
                tickfunc = callback,
                tick = time
            };
            return timer;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="timeInterval"></param>
        /// <param name="targer"></param>
        public static IServerTimerBase WaitRunHandle(Action callBack, int timeInterval, IInvokeHandle targer)
        {
            var tg = targer;

            var timer = CreateServerTimer(t =>
            {
                t.Dispose();
                if (tg != null)
                {
                    tg.Invoke(() =>
                    {
                        callBack();
                    });
                }
                else
                {
                    callBack();
                }
                return false;
            }, timeInterval);
            timer.Restart();
            return timer;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="timeInterval"></param>
        /// <returns></returns>
        public static IServerTimerBase WaitRunHandle(Action callBack, int timeInterval)
        {
            return WaitRunHandle(callBack, timeInterval, null);
        }
        /// <summary>
        /// 设置服务器定时器最小基本时间间隔，以毫秒为单位
        /// 请慎重设置其时间间隔参数，所设置的时间作为定时器动作最小时间间隔
        /// </summary>
        /// <param name="baseInterval">需要调整的时间间隔数值</param>
        public static void SetTimerServiceInterval(int baseInterval)
        {
            _serverBaseTimeInterval = baseInterval;
            ServerTimer.Change(0, baseInterval);
        }

    }

    #region ServerTimerInterval
    class ServerTimerInterval : ServerTimerBase
    {
    }

    #endregion

    #region ServerTimerTimeSpan

    class ServerTimerTimeSpan : ServerTimerInterval
    {
        public TimeSpan tick = new TimeSpan(0, 0, 0);
        protected override void Next(bool init = false)
        {
            var now = DateTime.Now;
            if (init)
            {
                base.tickTime = now.Date.Add(tick);
            }
            while (now > base.tickTime)
            {
                base.tickTime = base.tickTime.AddDays(1);
            }
        }
    }

    #endregion


    #region ServerTimerBase -- 为计时器服务对象提供抽象基类
    /// <summary>
    /// 为计时器服务对象提供抽象基类
    /// </summary>
    abstract class ServerTimerBase : IServerTimerBase
    {

        #region 字段定义

        //指示当前计时器对象是否处于运行状态
        long _isRun = 0;
        long running = 0;
        int timeInterval = 1;
        //定时器需要执行的计时方法
        internal TimerCallBackDelegate tickfunc;
        internal long _id;

        #endregion

        #region 内部使用方法


        /// <summary>
        /// 计时 Tick方法，由基础计时服务程序调用
        /// </summary>
        internal void Tick(DateTime now)
        {
            if (IsRuning)
            {
                if (Interlocked.CompareExchange(ref running, 1, 0) != 0) return;
                while (TimeEnd(now))
                {
                    this.Next();
                    try
                    {
                        TimerTick();
                    }
                    catch (System.Exception ex)
                    {
                        this.Stop();
                        SystemErrorProvide.OnSystemErrorHandleEvent(null, ex);
                    }
                }
                Interlocked.CompareExchange(ref running, 0, 1);
            }
        }
        /// <summary>
        /// 系统时间修改触发的初始化方法
        /// </summary>
        internal void SystemEventsTimeChangedInitializer()
        {
            Next(true);
        }

        protected DateTime tickTime;
        /// <summary>
        /// 当计时器时间到达时计算下一个计时周期的触发时间
        /// </summary>
        /// <param name="init">是否初始化计时服务，如果是初始化计时服务当以当前时间初始化计时对象服务</param>
        protected virtual void Next(bool init = false)
        {
            if (init)
            {
                var now = DateTime.Now;
                tickTime = now.AddMilliseconds(TimerService.ServerBaseTimeInterval * TimeInterval);
            }
            else
            {
                tickTime = tickTime.AddMilliseconds(TimerService.ServerBaseTimeInterval * TimeInterval);
            }
        }
        /// <summary>
        /// 计算计时服务的对象时间是否已经到达本次计时周期
        /// </summary>
        /// <returns></returns>
        protected bool TimeEnd(DateTime now)
        {
            // var now = DateTime.Now;
            return tickTime <= now;
        }

        #endregion

        #region 公开方法 

        public virtual int TimeInterval
        {
            get { return timeInterval; }
            set
            {
                if (value < 1)
                    value = 1;
                timeInterval = value;
            }
        }
        /// <summary>
        /// 服务器定时器ID，全局唯一
        /// </summary>
        public virtual long ID
        {
            get
            {
                return Interlocked.Read(ref _id);
            }
        }
        /// <summary>
        /// 表示定时器是否正在运行
        /// </summary>
        public virtual bool IsRuning
        {
            get
            {
                return Interlocked.CompareExchange(ref _isRun, 1, 1) == 1;
            }
        }
        /// <summary>
        /// 停止并销毁定时器
        /// </summary>
        /// <returns></returns>
        public virtual void Stop()
        {
            long id = this.ID;
            if (Interlocked.CompareExchange(ref _isRun, 0, 1) == 1)
            {
                TimerService.DestroyTimer(id);
                _id = long.MinValue;
            }
        }
        /// <summary>
        /// 启动定时器
        /// </summary>
        /// <returns></returns>
        public virtual void Start()
        {
            if (Interlocked.CompareExchange(ref _isRun, 1, 0) == 0)
            {
                this.Next(true);
                TimerService.RegistrationTimer(this);
            }
        }
        /// <summary>
        /// 重新启动定时器
        /// </summary>
        public virtual void Restart()
        {
            Start();
            this.Next(true);
        }
        /// <summary>
        /// 
        /// </summary>
        public void TimerTick()
        {
            if (tickfunc != null)
            {
                if (!tickfunc(this))
                    this.Stop();
            }
        }
        /// <summary>
        /// 销毁计时器对象
        /// </summary>
        public void Dispose()
        {
            this.Stop();
        }

        #endregion

    }
    #endregion

    #region IServerTimerBase  -- 定义一个接口，该接口提供对服务器定时器创建必须实现的方法

    /// <summary>
    /// 定义一个接口，该接口提供对服务器定时器创建必须实现的方法
    /// </summary>
    public interface IServerTimerBase : IDisposable
    {
        /// <summary>
        /// 计时器的ID
        /// </summary>
        long ID { get; }
        /// <summary>
        /// 表示当前计时器是否处于运行状态
        /// </summary>
        bool IsRuning { get; }
        /// <summary>
        /// 计时器的时间间隔
        /// </summary>
        int TimeInterval { get; set; }
        /// <summary>
        /// 重启计时器
        /// </summary>
        void Restart();
        /// <summary>
        /// 开始计时器
        /// </summary>
        void Start();
        /// <summary>
        /// 停止计时器
        /// </summary>
        void Stop();
        /// <summary>
        /// 计时器执行方法
        /// </summary>
        void TimerTick();

    }

    #endregion

    /// <summary>
    /// 定义一个委托，表示计时器回调方法
    /// </summary>
    /// <param name="timer">当前触发事件的计时器对象</param>
    /// <returns>返回一个布尔值表示是否要立即停止当前计时的处理 如果需要立即停止则为 true 否则为 false </returns>
    public delegate bool TimerCallBackDelegate(IServerTimerBase timer);

    /// <summary>
    /// 定义一个接口，实现异步操作回调处理逻辑，消息同步
    /// </summary>
    public interface IInvokeHandle
    {
        /// <summary>
        /// 回调处理执行逻辑,一般的这些方法的实现是为了将方法的执行封送回消息队列和UI线程
        /// </summary>
        /// <param name="action">需要封送执行的方法对象</param>
        int Invoke(Action action);
    }
}
