using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uyi.Net
{
    class AsyncConnectGameServerResult : IAsyncResult
    {
        internal IGamePipeline asyncPipeline;
        internal bool isCompleted = false;
        internal object userState;
        internal bool result = false;


        public object AsyncState
        {
            get { return userState; }
        }

        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { throw new NotSupportedException(); }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return isCompleted; }
        }
    }


}
