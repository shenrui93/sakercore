/***************************************************************************
 * 
 * 创建时间：   2018/1/9 10:05:17
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供基于 Http的操作方法操作类
 * 
 * *************************************************************************/

#define TASK






using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Uyi.Web.Http
{
    /// <summary>
    /// 提供基于 Http的操作方法操作类
    /// </summary>
    public class HttpOperation : IDisposable
    {
        const string MethodGET = "get";
        const string MethodPOST = "post";

        private HttpWebRequest request;
        private string method = MethodGET;
        private string json = null;
        private Dictionary<string, string> query = new Dictionary<string, string>();
        private Dictionary<string, string> cookies = new Dictionary<string, string>();
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        private System.Collections.Concurrent.ConcurrentDictionary<string, FileUpload> filedict
            = new System.Collections.Concurrent.ConcurrentDictionary<string, FileUpload>();
        private Uri proxy;
        private string url;
        private X509Certificate2 cert;



        string Method
        {
            get
            {
                if (!string.IsNullOrEmpty(json)) return MethodPOST;
                if (filedict.Count > 0) return MethodPOST;
                return method;
            }
        }
        string BUrl
        {
            get
            {
                if (string.IsNullOrEmpty(url)) return null;
                if (url.IndexOf('?') > 0)
                {
                    return url + "&" + this.query.ToUrl();
                }
                else
                {
                    return url + "?" + this.query.ToUrl();
                }
            }
        }
        string FormBoundaryType
        {
            get
            {
                return "WebKitFormBoundary7MA4YWxkTrZu0gW";
            }
        }


        /// <summary>
        /// 获取发起一个Get请求的操作对象
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public static HttpOperation Get(string url)
        {
            return new HttpOperation(MethodGET, url);
        }
        /// <summary>
        /// 获取发起一个Post请求的操作对象
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpOperation Post(string url)
        {
            return new HttpOperation(MethodPOST, url);
        }



        #region 公开操作类

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpOperation SetHeader(string key, long value)
        {
            if (key.IsEmpty()) return this;
            return this.SetHeader(key, value.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpOperation SetHeader(string key, string value)
        {
            if (key.IsEmpty()) return this;
            if (value.IsEmpty())
            {
                headers.Remove(key);
                return this;
            }
            headers[key] = value;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpOperation SetCookies(string key, long value)
        {
            return this.SetHeader(key, value.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpOperation SetCookies(string key, string value)
        {
            if (key.IsEmpty()) return this;
            if (value.IsEmpty())
            {
                cookies.Remove(key);
                return this;
            }
            cookies[key] = value;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpOperation SetParam(string key, long value)
        {
            if (key.IsEmpty()) return this;
            query[key] = value.ToString();
            return this;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpOperation SetParam(string key, string value)
        {
            if (key.IsEmpty()) return this;
            if (value.IsEmpty())
            {
                query.Remove(key);
                return this;
            }
            query[key] = value;
            return this;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public HttpOperation SetParam(string key, string name, Stream files)
        {
            if (key.IsEmpty()) return this;
            if (files == null || !files.CanRead || files.Length <= 0)
            {
                FileUpload t;
                filedict.TryRemove(key, out t);
                t?.Dispose();
                return this;
            }
            var fu = new FileUpload()
            {
                fs = files,
                key = key,
                name = name
            };
            filedict.AddOrUpdate(key, fu, (k, v) =>
            {
                if (files != v?.fs)
                {
                    v?.Dispose();
                }
                return fu;
            });
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public HttpOperation SetParam(string key, string name, byte[] files)
        {
            if (files == null || files.Length <= 0) return this;

            var ms = new System.IO.MemoryStream(files);
            ms.Position = 0;
            return SetParam(key, name, ms);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public HttpOperation SetParam(string key, string name, string filePath)
        {
            var ms = SakerCore.IO.FileHelper.Open(filePath);
            return SetParam(key, name, ms);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public HttpOperation SetParam(SakerCore.Web.IWebParamData param)
        {
            string key, value;

            foreach(var item in param)
            {
                key = item.Key;
                value = item.Value;

                if (key.IsEmpty()) continue;
                if (value.IsEmpty())
                {
                    query.Remove(key);
                    continue;
                }
                query[key] = value;
            } 
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpOperation SetProxy(string url)
        {
            Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out proxy);
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpOperation SetJson(string value)
        {
            this.json = value;
            return this;
        }

        #endregion

        #region 请求发送的封装方法 

        private void BuildRequest(Action<Exception> callback)
        {
            if (!json.IsEmpty())
            {
                BuildPostJsonRequest(callback);
                return;
            }
            if (filedict.Count > 0)
            {
                BuildPostFileRequest(callback);
                return;
            }
            var m = Method;
            if (m == MethodGET)
            {
                request = (HttpWebRequest)WebRequest.Create(BUrl);
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(url);
            }
            request.Method = m;
            request.ContentType = "application/x-www-form-urlencoded";
            //设置请求头
            BuildHeader();
            SendRequestData(callback);
        }
        private void BuildPostFileRequest(Action<Exception> callback)
        {
            request = (HttpWebRequest)WebRequest.Create(BUrl);
            request.Method = Method;
            request.ContentType = $"multipart/form-data; boundary={FormBoundaryType}";
            BuildHeader();
            SendRequestData(callback);
        }
        private void BuildPostJsonRequest(Action<Exception> callback)
        {
            request = (HttpWebRequest)WebRequest.Create(BUrl);
            request.Method = Method;
            request.ContentType = "application/json";
            BuildHeader();
            SendRequestData(callback);
        }
        private void SendRequestData(Stream requestStream, Stream ms, Action<Exception> callback)
        {
            using (ms)
            {
                byte[] buffer = new byte[512];
                int count = 0;
                while (true)
                {
                    count = ms.Read(buffer, 0, 512);
                    if (count <= 0) break;
                    requestStream.Write(buffer, 0, count);
                    requestStream.Flush();
                }

            }
            callback(null);
        }
        private void SendRequestData(Action<Exception> callback)
        {
            Stream ms = BuildRequestStream();
            if (ms == null || ms.Length <= 0 || !ms.CanRead)
            {
                callback(null);
                return;
            }
            try
            {
                request.ContentLength = ms.Length;
                request.BeginGetRequestStream(iar =>
                {
                    try
                    {
                        if (iar.IsCompleted)
                        {
                            var requestStream = request.EndGetRequestStream(iar);
                            SendRequestData(requestStream, ms, callback);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        callback(ex);
                    }

                }, null);
            }
            catch (Exception ex)
            {
                callback(ex);
            }
        }
        private Stream BuildRequestStream()
        {
            var m = Method;
            if (m == MethodGET)
                return null;
            var ms = new MemoryStream();
            if (!string.IsNullOrEmpty(json))
            {
                //写Json
                var buffer = Encoding.UTF8.GetBytes(json);
                ms.Write(buffer, 0, buffer.Length);
                ms.Position = 0;
                return ms;
            }
            if (filedict.Count <= 0)
            {
                //写Url参数
                var buffer = Encoding.UTF8.GetBytes(query.ToUrl());
                ms.Write(buffer, 0, buffer.Length);
                ms.Position = 0;
                return ms;
            }
            bool mark = true;
            var fdStr = ("--" + this.FormBoundaryType + "\r\n");
            var fdBytes = Encoding.UTF8.GetBytes(fdStr);


            foreach (var q in query)
            {
                if (mark)
                {
                    ms.WriteBytes(fdBytes);
                    mark = false;
                }
                ms.WriteUtf8String($@"Content-Disposition: form-data; name=""{q.Key}""

{q.Value}
");
                ms.WriteBytes(fdBytes);

            }
            foreach (var q in filedict)
            {
                if (mark)
                {
                    ms.WriteBytes(fdBytes);
                    mark = false;
                }
                var fileItem = q.Value;
                var fname = fileItem.name;
                ms.WriteUtf8String($@"Content-Disposition: form-data; name=""{q.Key}""{(string.IsNullOrEmpty(fname) ? "" : $@" ;filename=""{fname}""")}
Content-Type: application/octet-stream

");

                ms.WriteStream(fileItem.fs);
                fileItem.Dispose();
                ms.WriteUtf8String("\r\n");
                ms.WriteBytes(fdBytes);
            }
            ms.Flush();
            ms.Position = 0;
            return ms;
        }
        private void BuildHeader()
        {
            var h = request.Headers;
            foreach (var item in headers)
            {
                h[item.Key] = item.Value;
            }
            BuildProxy();
            BuildCert();
        }
        private void BuildCert()
        {
            if (cert == null) return;
            request.ClientCertificates.Add(cert);
        }
        private void BuildProxy()
        {
            if (proxy != null)
            {
                request.Proxy = new WebProxy(proxy);
            }
        }

        #endregion


        #region 基础异步操作模块

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        public IAsyncResult BeginGetResponse(AsyncCallback callback, object userState)
        {
            //创建一个异步操作结果对象信息
            HttpOperationAsyncResult r = new HttpOperationAsyncResult()
            {
                AsyncState = userState,
                _callback = callback
            };
            BuildRequest(ex =>
            {
                Destory();
                if (ex != null)
                {
                    r._ex = ex;
                    r.OnComplete();
                    return;
                }
                try
                {
                    request.BeginGetResponse(iar =>
                    {
                        try
                        {
                            if (iar.IsCompleted)
                            {
                                //操作完成
                                var res = (HttpWebResponse)request.EndGetResponse(iar);
                                r._res = res;
                                r.OnComplete();
                                return;
                            }
                        }
                        catch (WebException we)
                        {
                            r._res = we.Response as HttpWebResponse;
                            r.OnComplete();
                            return;
                        }
                        catch (Exception we)
                        {
                            r._ex = we;
                            r.OnComplete();
                            return;
                        }
                    }, null);
                }
                catch (Exception ex2)
                {
                    r._ex = ex2;
                    r.OnComplete();
                    return;
                }

            });
            return r;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iar"></param>
        /// <returns></returns>
        public HttpWebResponse EndGetResponse(IAsyncResult iar)
        {
            var r = iar as HttpOperationAsyncResult;
            if (r == null)
            {
                throw new ArgumentException("无效的异步操作状态信息");
            }
            if (r._ex != null)
            {
                throw new Exception("异步操作出现异常", r._ex);
            }
            return r._res;

        }


        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        public IAsyncResult BeginGetResponseString(AsyncCallback callback, object userState)
        {
            return BeginGetResponse(callback, userState);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iar"></param>
        /// <returns></returns>
        public string EndGetResponseString(IAsyncResult iar)
        {
            return EndGetResponseString(iar, Encoding.UTF8);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iar"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public string EndGetResponseString(IAsyncResult iar, Encoding encode)
        {
            var httpResponse = EndGetResponse(iar);
            try
            {
                if (httpResponse == null) return "";
                using (httpResponse)
                {

                    using (var stream = httpResponse.GetResponseStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            return encode.GetString(ms.ToArray());
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task<HttpWebResponse> GetResponseAsync()
        {
            var t = new System.Threading.Tasks.TaskCompletionSource<HttpWebResponse>();
            BeginGetResponse(iar =>
            {
                try
                {
                    if (iar.IsCompleted)
                    {
                        t.TrySetResult(this.EndGetResponse(iar));
                    }
                }
                catch (Exception ex)
                {
                    t.TrySetException(ex);
                }

            }, null);

            return t.Task;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task<string> GetResponseStringAsync()
        {
            var t = new System.Threading.Tasks.TaskCompletionSource<string>();
            BeginGetResponseString(iar =>
            {
                try
                {
                    if (iar.IsCompleted)
                    {
                        t.TrySetResult(this.EndGetResponseString(iar));
                    }
                }
                catch (Exception ex)
                {
                    t.TrySetException(ex);
                }

            }, null);

            return t.Task;
        }


        class HttpOperationAsyncResult : IAsyncResult
        {
            private bool _isCompleted;
            public object AsyncState
            {
                get;
                set;
            }
            public WaitHandle AsyncWaitHandle => null;
            public bool CompletedSynchronously => false;
            public bool IsCompleted => _isCompleted;
            public Exception _ex;
            public HttpWebResponse _res;
            internal AsyncCallback _callback;

            internal void OnComplete()
            {
                _isCompleted = true;
                _callback(this);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Destory();
        }

        void Destory()
        {
            foreach (var r in filedict)
            {
                r.Value?.Dispose();
            }
            filedict?.Clear();

        }














        private HttpOperation(string method, string url)
        {
            this.method = method;
            this.url = url;

        }

        class FileUpload : IDisposable
        {
            public string key;
            public string name;
            public Stream fs;

            public void Dispose()
            {
                fs?.Dispose();
            }
        }


    }

    static class __
    {
        public static void WriteBytes(this Stream stream, byte[] buffer)
        {
            if (buffer == null || buffer.Length <= 0) return;

            stream.Write(buffer, 0, buffer.Length);
        }
        public static void WriteUtf8String(this Stream stream, string value)
        {
            if (value == null || value.Length == 0) return;
            var b = Encoding.UTF8.GetBytes(value);

            stream.Write(b, 0, b.Length);
        }
        public static void WriteStream(this Stream stream, Stream value)
        {
            if (value == null || value.Length == 0) return;
            var buffer = new byte[512];
            int count;
            while (true)
            {
                count = value.Read(buffer, 0, 512);
                if (count <= 0) break;
                stream.Write(buffer, 0, count);
            }
        }
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
        public static string ToUrl(this Dictionary<string, string> value)
        {
            if (value == null) return "";

            StringBuilder strb = new StringBuilder();
            foreach (var d in value)
            {
                if (string.IsNullOrEmpty(d.Key)) continue;
                if (string.IsNullOrEmpty(d.Value)) continue;
                strb.Append(string.Format("&{0}={1}", d.Key.Trim(), System.Web.HttpUtility.UrlEncode(d.Value.Trim())));
            }
            if (strb.Length > 0)
            {
                strb = strb.Remove(0, 1);
            }
            return strb.ToString();
        }
    }

}
