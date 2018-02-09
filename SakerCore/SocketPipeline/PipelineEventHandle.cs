using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uyi.Net
{

    /// <summary>
    /// 服务器管道事件参数委托
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="pipeline"></param>
    /// <param name="e"></param>
    public delegate void PipelineEventHandle<TEventArgs>(Uyi.Net.IPipeline pipeline, TEventArgs e) where TEventArgs : EventArgs;

}
