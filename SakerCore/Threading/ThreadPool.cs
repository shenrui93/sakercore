/***************************************************************************
 * 
 * 创建时间：   2016/5/3 9:47:52
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   平台用线程池管理器
 * 
 * *************************************************************************/

using System;
using System.Threading;

namespace SakerCore.Threading
{
    /// <summary>
    /// 平台用线程池管理器
    /// </summary>
    public class ThreadPoolProviderManager : IThreadPoolProvider
    { 

        static ThreadPoolProviderManager()
        {
        }
        static IThreadPoolProvider _instance = new ThreadPoolProviderManager();
        /// <summary>
        /// 提供一个线程池调度器的单根实例
        /// </summary>
        public static IThreadPoolProvider Instance { get { return _instance; } }


        /// <summary>
        /// 将工作任务排入线程池队列
        /// </summary>
        /// <param name="runMethod"></param>
        public static void QueueUserWorkItem(Action runMethod)
        {
            _instance.QueueUserWorkItem(runMethod);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {

        }
        /// <summary>
        /// 将任务排入线程池队列以便执行
        /// </summary>
        /// <param name="runMethod"></param>
        /// <returns></returns>
        bool IThreadPoolProvider.QueueUserWorkItem(Action runMethod)
        {
            return ThreadPool.UnsafeQueueUserWorkItem((o) => { runMethod(); }, null);
        }
    }





}
