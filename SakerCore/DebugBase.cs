/***************************************************************************
 * 
 * 创建时间：   2017/12/8 19:17:53
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供基础的日志操作基类
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SakerCore.Extension;
using SakerCore.Tools;

namespace SakerCore
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DebugBase<T> : IUyiDebug
        where T : class, IUyiDebug, new()
    {
        static T Instance => _instance ?? (_instance = new T());


        IMessageQueue<DebugMsg> _writer_log_queue = new MessageQueue<DebugMsg>();
        string lastlogpath;
        Stream _logStream;
        private static T _instance;

        public DebugBase()
        {
            _writer_log_queue.MessageComing = Writer_log_queue_MessageComing;
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract string ServerName { get; }

        /// <summary>
        /// 
        /// </summary>
        string LogFilesName(DateTime now)
        {
            var hour = now.Hour.ToString().PadLeft(2, '0');
            var minute = (now.Minute / 5 * 5).ToString().PadLeft(2, '0');
            return $"{IO.FileHelper.ProcessBaseDir}/Log/{DateTime.Now.ToString("yyyy-MM-dd")}/{ServerName}-{hour}.log";

        }
        Stream LogStream(DateTime now)
        {
            var fn = LogFilesName(now);
            if (_logStream == null || lastlogpath != fn)
            {
                _logStream?.Dispose();
                lastlogpath = fn;
                _logStream = SakerCore.IO.FileHelper.AppendOrCreate(lastlogpath);
            }
            return _logStream;
        }
        private void Writer_log_queue_MessageComing(IMessageQueue<DebugMsg> sender, MessageEventArgs<DebugMsg> e)
        {
            var m = e.Message;
            if (m == null) return;

            try
            {
                var now = m.DateTime;

                var stream = LogStream(now);
                if (stream == null) return;

                stream.WriteBytes($"[{now.ToString("yyyy-MM-dd HH:mm:ss")}]  {m.Message}{Environment.NewLine}".GetBytes());
                stream.Flush();
            }
            catch (System.Exception)
            {
            }
            finally
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteError(string msg)
        {
            Instance.OnShowMsg(msg, ShowMsgColor.Red);
            Instance.OnWriteLog(msg);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteWarnning(string msg)
        {
            Instance.OnShowMsg(msg, ShowMsgColor.Yellow);
            Instance.OnWriteLog(msg);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteMessage(string msg)
        {
            Instance.OnShowMsg(msg, ShowMsgColor.Blue);
            Instance.OnWriteLog(msg);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteText(string msg)
        {
            Instance.OnShowMsg(msg, ShowMsgColor.White);
            Instance.OnWriteLog(msg);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetErrorFormatString(System.Exception ex)
        {

            if (ex == null)
                throw new NullReferenceException();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("*********************异常文本*********************");
            sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());

            sb.AppendLine("【异常类型】：" + ex.GetType().Name);
            sb.AppendLine("【异常信息】：" + ex.Message);
            sb.AppendLine("【堆栈调用】：" + ex.StackTrace);

            sb.AppendLine("******************************************************************");

            if (ex.InnerException != null)
            {
                sb.AppendLine(GetErrorFormatString(ex.InnerException));
            }

            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteCilentLog(string message)
        { 
            Instance.OnWriteLog(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLog(string message)
        { 
            Instance.OnWriteLog(message);
        }

 
        public static void PrintError(System.Exception err, Stream stream = null, string fname = null, string[] emils = null) => Instance.OnPrintError(err, stream, fname, emils);
        public static void PrintError(System.Exception exception)
        {
            PrintError(exception, null);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="color"></param>
        public abstract void OnPrintError(System.Exception err, System.IO.Stream stream = null, string fname = null, string[] emils = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="color"></param>
        public abstract void OnShowMsg(string msg, ShowMsgColor color);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public virtual void OnWriteLog(string msg)
        {
            var o = GetPoolObj();
            o.DateTime = DateTime.Now;
            o.Message = msg;
            _writer_log_queue.Enqueue(o);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _writer_log_queue.Clear();
            _logStream?.Dispose();
            _logStream = null;
        }



        class DebugMsg
        {
            public DateTime DateTime;
            public string Message;
        }


        DebugMsg GetPoolObj()
        {
            return new DebugMsg();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IUyiDebug : IDisposable
    {
        void OnShowMsg(string msg, ShowMsgColor color);
        void OnWriteLog(string msg);
        void OnPrintError(System.Exception err, System.IO.Stream stream = null, string fname = null, string[] emils = null);
    }


    /// <summary>
    /// 
    /// </summary>
    public enum ShowMsgColor
    {
        Red,
        Yellow,
        Blue,
        White
    }
}
