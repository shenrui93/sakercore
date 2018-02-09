/***************************************************************************
 * 
 * 创建时间：   2017/1/22 11:16:04
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供一个集合来缓存服务服务对象，该类是线程安全的
 * 
 * *************************************************************************/



#define LockSim

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SakerCore.Tools;

namespace SakerCore.Web
{
    /// <summary>
    /// 提供一个集合来缓存服务服务对象，该类是线程安全的
    /// </summary>
    public class ServerCacheManager<TKey, TValue>
    {
        private const int _default_expries_time = (1000 * 60 * 60 * 2);

        private static readonly TValue defaultValue = default(TValue);
        private static readonly TKey defaultKey = default(TKey);
        //线程同步锁定
#if LockSim
        private ReaderWriterLockSlim root = new ReaderWriterLockSlim();
#else
        private ReaderWriterLock root = new ReaderWriterLock();
#endif
        //提供对象缓存管理支持的对象
        private Dictionary<TKey, CacheItem> _collection = new Dictionary<TKey, CacheItem>();
        private long defaultExpriesTime;
        private bool CheckTagExpires(CacheItem item, out TValue val)
        {
            if (item == null)
            {
                val = defaultValue;
                return false;
            }
            var now = DateTime.Now;
            if (item.Expires >= now)
            {
                val = item.Tag;
                if (val.Equals(defaultValue))
                {
                    InternalUpgradeRemoveCache(item);
                    return false;
                }
                return true;
            }
            InternalUpgradeRemoveCache(item);
            val = defaultValue;
            return false; 
        }

        /// <summary>
        /// 构建一个新的缓存器
        /// </summary>
        public ServerCacheManager()
        {
            defaultExpriesTime = _default_expries_time;
        }
        /// <summary>
        /// 构建一个新的缓存器（并指定默认的过期时间）
        /// <param name="expriesTime">过期时间（单位：毫秒）</param>
        /// </summary>
        public ServerCacheManager(long expriesTime)
        {
            if (expriesTime <= 0) throw new System.Exception("默认的缓存时间 expries_time 不能小于零");
            defaultExpriesTime = expriesTime;
        }


        /// <summary>
        /// 发送删除数据的任务
        /// </summary>
        /// <param name="item"></param>
        private void InternalRemoveCache(CacheItem item)
        {
            if (item == null) return;
#if LockSim
            root.EnterWriteLock();
#else
            root.AcquireWriterLock(-1);
#endif
            try
            {
                CacheItem val;
                _collection.TryGetValue(item.Key, out val);
                if (item != val) return;
                _collection.Remove(item.Key);
            }
            finally
            {
#if LockSim
                root.ExitWriteLock();
#else
                root.ReleaseWriterLock();
#endif
            }
        }
        /// <summary>
        /// 发送删除数据的任务
        /// </summary>
        /// <param name="item"></param>
        private void InternalUpgradeRemoveCache(CacheItem item)
        {



#if LockSim
            root.EnterWriteLock();
#else
            var lockCookies = root.UpgradeToWriterLock(-1);

#endif
            try
            {
                CacheItem val;
                _collection.TryGetValue(item.Key, out val);
                if (item != val) return;
                _collection.Remove(item.Key);
            }
            finally
            {
#if LockSim
                root.ExitWriteLock();
#else 
                root.DowngradeFromWriterLock(ref lockCookies);
#endif
            }
        }
        /// <summary>
        /// 添加对象，内部调用
        /// </summary>
        private void InternalAddOrUpdateValue(TKey key, TValue value, DateTime expires)
        {

#if LockSim
            root.EnterWriteLock();
#else
            root.AcquireWriterLock(-1);
#endif
            try
            {
                _collection[key] = new CacheItem()
                {
                    Tag = value,
                    Key = key,
                    Expires = expires
                };
            }
            finally
            {

#if LockSim
                root.ExitWriteLock();
#else
                root.ReleaseWriterLock();
#endif
            }
        }
        /// <summary>
        /// 发送删除数据的任务
        /// </summary>
        /// <param name="key"></param>
        private void RemoveCache(TKey key)
        {
            if (key == null) return;
            CacheItem item;
            if (!_collection.TryGetValue(key, out item))
            {
                return;
            }
            InternalRemoveCache(item);
        }






        /// <summary>
        /// 获取或者写入一个缓存对象信息（如果写入则使用默认的写入缓存时间写入）
        /// </summary>
        /// <param name="key">获取缓存信息的键</param>
        /// <returns>返回获取的值，如果获取失败返回默认值</returns>
        public TValue this[TKey key]
        {
            get
            {
                return ReadValue(key);
            }
            set
            {
                WriteValue(key, value, defaultExpriesTime);
            }
        }
        /// <summary>
        /// 获取一个缓存对象信息
        /// </summary>
        /// <param name="key">获取缓存信息的键</param>
        /// <returns>返回获取的值，如果获取失败返回默认值</returns>
        public TValue ReadValue(TKey key)
        {
            TValue value = defaultValue;
            TryReadValue(key, out value);
            return value;
        }
        /// <summary>
        /// 尝试获取一个键值
        /// </summary>
        /// <param name="key">获取缓存信息的键</param>
        /// <param name="value">返回获取到的值</param>
        /// <returns>如果成功获取返回 true,如果失败返回 false</returns>
        public bool TryReadValue(TKey key, out TValue value)
        {
            //获取一个读取锁 
#if LockSim
            root.EnterUpgradeableReadLock();
#else
            root.AcquireWriterLock(-1);
#endif
            try
            {
                //尝试获取缓存对象信息
                CacheItem item;
                if (!_collection.TryGetValue(key, out item))
                {
                    //获取失败
                    value = defaultValue;
                    return false;
                }
                //检查当前的缓存信息是否已经过期，如果已经过期则返回获取失败
                TValue val;
                if (!CheckTagExpires(item, out val))
                {
                    //当前的缓存对象信息已经过期
                    value = defaultValue;
                    return false;
                }
                //获取对象成功，直接返回
                value = val;
                return true;
            }
            finally
            {
#if LockSim
                root.ExitUpgradeableReadLock();
#else
                root.ReleaseWriterLock();
#endif
            }
        }
        /// <summary>
        /// 添加或者更新对象
        /// </summary>
        /// <param name="key">需要添加或者更新</param>
        /// <param name="value">需要更新的内容</param>
        /// <param name="expires">对象过期时间</param>
        public void WriteValue(TKey key, TValue value, DateTime expires)
        {
            if (key.Equals(defaultKey)) return;
            if (value.Equals(defaultKey) || DateTime.Now >= expires)
            {
                RemoveCache(key);
                return;
            }
            InternalAddOrUpdateValue(key, value, expires);

        }
        /// <summary>
        /// 添加或者更新对象
        /// </summary>
        /// <param name="key">需要添加或者更新</param>
        /// <param name="value">需要更新的内容</param>
        /// <param name="expires">对象过期时间（单位：毫秒）</param>
        public void WriteValue(TKey key, TValue value, long expires)
        {
            if (key.Equals(defaultKey)) return;
            if (value.Equals(defaultKey) || expires <= 0)
            {
                RemoveCache(key);
                return;
            }
            var expires_time = DateTime.Now.AddMilliseconds(expires);
            InternalAddOrUpdateValue(key, value, expires_time);

        }
        /// <summary>
        /// 添加或者更新对象
        /// </summary>
        /// <param name="key">需要添加或者更新</param>
        /// <param name="value">需要更新的内容</param>
        /// <param name="expires">对象过期时间</param>
        public void WriteValue(TKey key, TValue value, TimeSpan expires)
        {
            if (key.Equals(defaultKey)) return;
            if (value.Equals(defaultKey) || expires.TotalSeconds <= 0)
            {
                RemoveCache(key);
                return;
            }
            var expires_time = DateTime.Now.AddSeconds(expires.TotalSeconds);
            InternalAddOrUpdateValue(key, value, expires_time);
        }
        /// <summary>
        /// 写入一个对象
        /// </summary>
        /// <param name="key">要写入的键</param>
        /// <param name="value">要写入的值</param>
        public void WriteValue(TKey key, TValue value)
        {
            WriteValue(key, value, defaultExpriesTime);
        }


        /// <summary>
        /// 提供对象对象缓存所需要的基础对象支持
        /// </summary> 
        class CacheItem
        {
            /// <summary>
            /// 缓存对象的键
            /// </summary>
            public TKey Key;
            /// <summary>
            /// 需要缓存的对象信息
            /// </summary>
            public TValue Tag;
            /// <summary>
            /// 对象的过期时间
            /// </summary>
            public DateTime Expires;
        }
    }
}
