/***************************************************************************
 * 
 * 创建时间：   2017/10/19 13:13:09
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供一个双差异化优先级信号量
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SakerCore.Threading
{
    /// <summary>
    /// 提供一个双差异化优先级信号量
    /// </summary>
    public class DoublePrioritySemaphore
    { 
        //锁定
        private object root = new object(); 
        private int high_request = 0;
        private SpinWait spin = new SpinWait();
        private int lockedThreadId = -1;
        private int CurrentThreadId =>Thread.CurrentThread.ManagedThreadId ;


        /// <summary>
        /// 进入一个高优先级的线程同步等待
        /// </summary>
        public void EnterHighWait()
        {
            Interlocked.Increment(ref high_request);
            Monitor.Enter(root);

        }
        /// <summary>
        /// 退出一次高优先级的线程等待
        /// </summary>
        public void ExitHighWait()
        { 
            Interlocked.Decrement(ref high_request);
            Monitor.Exit(root);
        } 
        /// <summary>
        /// 进入一个低优先级的线程同步等待
        /// </summary>
        public void EnterLowWait()
        {
            //自选检查高优先级请求
            while (true)
            {
                if(high_request <= 0 || lockedThreadId == CurrentThreadId)
                {
                    break;
                }
                //执行一个自旋操作
                spin.SpinOnce();
                continue;
            }
            Monitor.Enter(root);
            lockedThreadId = CurrentThreadId;
        }
        /// <summary>
        /// 退出一个低优先级的线程同步等待
        /// </summary>
        public void ExitLowWait()
        { 
            Monitor.Exit(root);
        }
        
    }
}
