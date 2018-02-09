/***************************************************************************
 * 
 * 创建时间：   2016/11/23 12:05:04
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   定义一个线程池任务调度器必须实现的接口
 * 
 * *************************************************************************/

using System;

namespace SakerCore.Threading
{
    /// <summary>
    /// 定义一个线程池任务调度器必须实现的接口
    /// </summary>
    public interface IThreadPoolProvider:IDisposable
    {
        /// <summary>
        /// 向线程池中发送工作任务
        /// </summary>
        /// <param name="runMethod">需要执行的任务方法委托</param>
        /// <returns>如果将任务成功的排入线程池任务队列则为 true 否则引发异常</returns>
        bool QueueUserWorkItem(Action runMethod);
    }
}
