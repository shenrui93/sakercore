/***************************************************************************
 * 
 * 创建时间：   2016/4/14 13:18:56
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   服务器系统事件通知委托
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic; 
using System.IO; 
using System.Text; 

namespace SakerCore
{
    /// <summary>
    /// 服务器系统事件通知委托
    /// </summary>
    public delegate void EventHandle(object sender, object e);
    /// <summary>
    /// Uyi服务器系统事件通知委托
    /// </summary>
    /// <typeparam name="T">时间参数的对象类型信息</typeparam>
    /// <param name="sender">引发事件的对象</param>
    /// <param name="e">携带的事件参数</param>
    public delegate void EventHandle<T>(object sender, T e);
    /// <summary>
    /// Uyi服务器系统事件通知委托
    /// </summary>
    /// <typeparam name="TSender">事件发送对象的类型</typeparam>
    /// <typeparam name="TEventArg">事件参数的对象类型信息</typeparam>
    /// <param name="sender">引发事件的对象</param>
    /// <param name="e">携带的事件参数</param>
    public delegate void EventHandle<TSender,TEventArg>(TSender sender, TEventArg e);
}
