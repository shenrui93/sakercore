using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SakerCore.Threading;

namespace SakerCore.Web
{

    #region HttpHelper

    /// <summary>
    /// 
    /// </summary>
    public static partial class HttpHelper
    {

        /// <summary>
        /// 
        /// </summary>
        public enum ContextType
        {
            /// <summary>
            /// 
            /// </summary>
            Unknown,
            /// <summary>
            /// 
            /// </summary>
            UrlEncoded,
            /// <summary>
            /// 
            /// </summary>
            JSON,
        }






        private static void EmptyCall(HttpWebResponse obj)
        {
            try
            {
                IDisposable io = obj;
                io?.Dispose();
            }
            catch //(Exception)
            {
            }
        }
        private static void EmptyCall(string obj)
        {
        }

        #region 辅助方法

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="queryStream"></param>
        /// <param name="isFormData"></param>
        /// <param name="referer"></param>
        /// <param name="proxy"></param>
        /// <param name="certPath"></param>
        /// <param name="certPassword"></param>
        /// <returns></returns>
        public static HttpWebRequest BuildWebRequest(string url, Stream queryStream, ContextType isFormData, string referer, string proxy, string certPath = null, string certPassword = null)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "get";
            if (!string.IsNullOrEmpty(referer))
                req.Referer = referer;
            if (!string.IsNullOrEmpty(proxy))
                req.Proxy = new WebProxy(proxy);

            if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
            {
                X509Certificate2 cert;
                var ext = Path.GetExtension(certPath);
                {
                    switch (ext.ToLower())
                    {
                        case "hccert":
                            {
                                var base64 = IO.FileHelper.ReadAllText(certPath);
                                var bytes = Serialization.Base64Serialzier.FromBase64String(base64);
                                cert = new X509Certificate2();
                                cert.Import(bytes);
                                break;
                            }
                        default:
                            {
                                cert = new X509Certificate2(certPath, certPassword);
                                break;
                            }

                    }
                }

                req.ClientCertificates.Add(cert);
            }


            if (queryStream != null && queryStream.Length > 0)
            {
                req.Method = "post";
                req.ContentLength = queryStream.Length;

                InitContentType(isFormData, req);

                using (var reqStream = req.GetRequestStream())
                {
                    if (queryStream.CanSeek)
                        queryStream.Position = 0;
                    queryStream.CopyTo(reqStream);
                    reqStream.Flush();
                }

            }
            return req;
        }
        static void BuildWebRequestAsync(string url, Stream queryStream, ContextType isFormData, string referer, string proxy, string certPath, string certPassword, Action<HttpWebRequest> call)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "get";
            if (!string.IsNullOrEmpty(referer))
                req.Referer = referer;
            if (!string.IsNullOrEmpty(proxy))
                req.Proxy = new WebProxy(proxy);


            if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
            {
                X509Certificate cert = new X509Certificate(certPath, certPassword);
                req.ClientCertificates.Add(cert);
            }

            if (queryStream != null && queryStream.Length > 0)
            {
                req.Method = "post";
                req.ContentLength = queryStream.Length;
                InitContentType(isFormData, req);

                req.BeginGetRequestStream(iar =>
                {
                    try
                    {
                        using (var reqStream = req.EndGetRequestStream(iar))
                        {

                            if (queryStream.CanSeek)
                                queryStream.Position = 0;
                            queryStream.CopyTo(reqStream);
                            reqStream.Flush();
                        }
                        call(req);
                    }
                    catch (System.Exception)
                    {
                        call(null);
                    }
                }, null);
                return;
            }
            call(req);
        }
        /// <summary>
        /// HTTP网页的内容的基础编码方式
        /// </summary>
        static readonly Encoding baseEncoding = Encoding.UTF8;
        private static void InitContentType(ContextType isFormData, HttpWebRequest req)
        {
            switch (isFormData)
            {
                case ContextType.UrlEncoded:
                    {
                        req.ContentType = "application/x-www-form-urlencoded";
                        break;
                    }
                case ContextType.JSON:
                    {
                        req.ContentType = "application/json";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <returns></returns>
        public static string GetStringFromResponse(this WebResponse httpResponse)
        {
            return GetStringFromResponse(httpResponse, baseEncoding);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetStringFromResponse(this WebResponse httpResponse, Encoding encoding)
        {
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
                            return encoding.GetString(ms.ToArray());
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
        }














        #endregion


        #region  HTTP * GET 



        /// <summary>
        /// 对Http地址发起get请求，并直接获取响应对象
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static HttpWebResponse GetResponse(string url, string referer = null, string proxy = null)
        {
            return (HttpWebResponse)BuildWebRequest(url, null, ContextType.Unknown, referer, proxy).GetResponse();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <param name="proxy"></param>
        /// <param name="call"></param>
        public static void GetResponseAsync(string url, string referer = null, string proxy = null, Action<HttpWebResponse> call = null)
        {
            try
            {

                if (call == null) call = EmptyCall;
                BuildWebRequestAsync(url, null, ContextType.Unknown, referer, proxy, null, null, req =>
                {
                    try
                    {
                        req.BeginGetResponse(iar =>
                        {
                            try
                            {
                                using (var res = (HttpWebResponse)req.EndGetResponse(iar))
                                {
                                    call(res);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                call(null);
                            }

                        }, null);
                    }
                    catch (System.Exception)
                    {
                        call(null);
                    }
                });
            }
            catch
            {
                call(null);
            }
        }

        /// <summary>
        /// 对Http地址发起get请求，并直接获取响应对象
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static string GetString(string url, string referer = null, string proxy = null)
        {
            try
            {
                return GetResponse(url, referer, proxy).GetStringFromResponse();
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <param name="proxy"></param>
        /// <param name="call"></param>
        public static void GetStringAsync(string url, string referer = null, string proxy = null, Action<string> call = null)
        {
            try
            {
                if (call == null) call = EmptyCall;
                GetResponseAsync(url, referer, proxy, res =>
                {
                    call(res.GetStringFromResponse());
                });
            }
            catch (System.Exception)
            {
                call(null);
            }
        }









        #endregion

        #region  HTTP * POST

        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static HttpWebResponse PostResponse(string url, Stream queryStream, ContextType isFormData = ContextType.Unknown, string referer = null, string proxy = null)
        {
            HttpWebResponse res = (HttpWebResponse)BuildWebRequest(url, queryStream, isFormData, referer, proxy).GetResponse();
            return res;
        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static HttpWebResponse PostResponse(string url, string queryString, ContextType isFormData = ContextType.Unknown, string referer = null, string proxy = null)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(baseEncoding.GetBytes(queryString), 0, baseEncoding.GetByteCount(queryString));
            return PostResponse(url, ms, isFormData, referer, proxy);
        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static void PostResponseAsync(string url, Stream queryStream, ContextType isFormData = ContextType.Unknown, string referer = null, string proxy = null
            , Action<HttpWebResponse> call = null)
        {
            try
            {
                if (call == null) call = EmptyCall;
                BuildWebRequestAsync(url, queryStream, isFormData, referer, proxy, null, null, req =>
                {
                    try
                    {
                        req.BeginGetResponse(iar =>
                        {
                            try
                            {
                                using (var res = req.EndGetResponse(iar) as HttpWebResponse)
                                {
                                    call(res);
                                }
                            }
                            catch (System.Exception)
                            {
                                call(null);
                            };
                        }, null);
                    }
                    catch (System.Exception)
                    {
                        call(null);
                    }
                });
            }
            catch (System.Exception)
            {
                call(null);
            }
        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static void PostResponseAsync(string url, string queryString, ContextType isFormData = ContextType.Unknown, string referer = null, string proxy = null
            , Action<HttpWebResponse> call = null)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(baseEncoding.GetBytes(queryString), 0, baseEncoding.GetByteCount(queryString));
                PostResponseAsync(url, ms, isFormData, referer, proxy, call);
            }
            catch (System.Exception)
            {
                call(null);
            }
        }

        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static string PostString(string url, Stream queryStream, ContextType isFormData = ContextType.Unknown, string referer = null, string proxy = null)
        {
            try
            {
                return PostResponse(url, queryStream, isFormData, referer, proxy).GetStringFromResponse();
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static string PostString(string url, string queryString, ContextType isFormData = ContextType.Unknown, string referer = null, string proxy = null)
        {
            try
            {
                return PostResponse(url, queryString, isFormData, referer, proxy).GetStringFromResponse();
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static void PostStringAsync(string url, Stream queryStream, ContextType isFormData = ContextType.Unknown, string referer = null, string proxy = null
            , Action<string> call = null)
        {
            try
            {
                if (call == null) call = EmptyCall;
                PostResponseAsync(url, queryStream, isFormData, referer, proxy, res =>
                {
                    call(res.GetStringFromResponse());
                });
            }
            catch (System.Exception)
            {
                call(null);
            }
        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static void PostStringAsync(string url, string queryString, ContextType isFormData = ContextType.Unknown, string referer = null, string proxy = null
            , Action<string> call = null)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(baseEncoding.GetBytes(queryString), 0, baseEncoding.GetByteCount(queryString));
                PostStringAsync(url, ms, isFormData, referer, proxy, call);
            }
            catch (System.Exception)
            {
                call(null);
            }
        }




















        #endregion

        #region  HTTP * POST_FROMDATA

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="_query"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public static HttpWebResponse PostFromData(string url, IWebParamData _query, PostFromDataParam<PostFileData> files, IWebParamData header = null)
        {
            //写入请求流 
            var req = BuilderRequest(url, _query, files, r =>
            {
                if (header != null)
                {
                    foreach (var kv in header)
                    {
                        r.Headers[kv.Key] = kv.Value;
                    }
                }
            });
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="_query"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public static string PostFromDataString(string url, IWebParamData _query, PostFromDataParam<PostFileData> files, IWebParamData header = null)
        {
            try
            {
                var response = PostFromData(url, _query, files, header);
                return GetStringFromResponse(response);
            }
            catch //(Exception ex)
            {
                return "";
            }
        }






        #endregion

        #region 文件上传相关

        /*********************** Begin Method *************************/

        /// <summary>
        /// 一个url发起异步的http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="_query"></param>
        /// <param name="files"></param>
        /// <param name="callback"></param>
        static void BeginPostFromData(string url, IWebParamData _query, PostFromDataParam<PostFileData> files, Action<HttpWebResponse, System.Exception> callback)
        {
            ThreadPoolProviderManager.QueueUserWorkItem(() =>
            {
                try
                {
                    var req = BuilderRequest(url, _query, files, null);
                    using (var res = (HttpWebResponse)req.GetResponse())
                    {
                        callback?.Invoke(res, null);
                    }
                }
                catch (System.Exception ex)
                {
                    callback?.Invoke(null, ex);
                }

            });

        }
        /// <summary>
        /// 一个url发起异步的http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="_query"></param>
        /// <param name="files"></param>
        /// <param name="callback"></param>
        public static void BeginPostFromDataString(string url, IWebParamData _query, PostFromDataParam<PostFileData> files, Action<string, System.Exception> callback)
        {
            BeginPostFromData(url, _query, files, (res, e) =>
            {
                Process_GetHttpResponseString(res, e, callback);
            });
        }
        private static HttpWebRequest BuilderRequest(string url, IWebParamData _query, PostFromDataParam<PostFileData> files, Action<HttpWebRequest> BeforSendHeader)
        {

            byte[] _newlinebytes = baseEncoding.GetBytes("\r\n");
            var _newlinebytescount = _newlinebytes.Length;

            using (var ms = new MemoryStream())
            { 
                #region 文件流


                //写入请求头参数信息，query
                StringBuilder _queryStringbuffer = new StringBuilder();

                foreach (var p in _query)
                {
                    _queryStringbuffer.Length = 0;

                    _queryStringbuffer.Append("--");
                    _queryStringbuffer.Append(ConstString.BOUNDARY);
                    _queryStringbuffer.Append("\r\n");
                    _queryStringbuffer.Append("Content-Disposition: form-data; name=\"" + p.Key + "\"\r\n\r\n");
                    _queryStringbuffer.Append(p.Value);
                    _queryStringbuffer.Append("\r\n");

                    //写入请求流
                    var data = baseEncoding.GetBytes(_queryStringbuffer.ToString());
                    ms.Write(data, 0, data.Length);
                }

                if (files != null)
                {

                    #region 写文件


                    //写文件
                    foreach (var f in files)
                    {
                        var file = f.Value;
                        if (!file.IsHasFile) continue;


                        _queryStringbuffer.Length = 0;

                        _queryStringbuffer.Append("--");
                        _queryStringbuffer.Append(ConstString.BOUNDARY);
                        _queryStringbuffer.Append("\r\n");
                        _queryStringbuffer.Append("Content-Disposition:form-data;name=\"" + f.Key + "\";filename=\"" + file.FileName + "\"\r\n");
                        _queryStringbuffer.Append("Content-Type:application/octet-stream\r\n\r\n");


                        //写入请求流
                        var data = baseEncoding.GetBytes(_queryStringbuffer.ToString());
                        ms.Write(data, 0, data.Length);

                        file.WriteTo(ms);

                        //写入更新的换行符
                        ms.Write(_newlinebytes, 0, _newlinebytescount);

                    };

                    #endregion

                }

                #endregion

                //写入结束标记

                //byte[] end_data = ("--" +   BOUNDARY + "--\r\n").getBytes();//数据结束标志
                byte[] end_data = baseEncoding.GetBytes($"--" + ConstString.BOUNDARY + "--\r\n");//数据结束标志

                ms.Write(end_data, 0, end_data.Length);



                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "post";
                req.ContentLength = ms.Length;
                req.ContentType = ConstString.MULTIPART_FORM_DATAContextType;

                BeforSendHeader?.Invoke(req);


                using (var req_stream = req.GetRequestStream())
                {
                    if (ms.CanSeek)
                        ms.Position = 0;
                    ms.CopyTo(req_stream);
                    req_stream.Flush();

                    return req;

                }
            }
        }


        private static void Process_HttpPostAsync(HttpWebRequest req, Stream ms, Action<HttpWebResponse, System.Exception> callback)
        {
            try
            {
                req.BeginGetRequestStream(r =>
                {
                    if (!r.IsCompleted) return;
                    try
                    {
                        using (var req_stream = req.EndGetRequestStream(r))
                        {
                            if (ms.CanSeek)
                                ms.Position = 0;
                            ms.CopyTo(req_stream);
                            Process_HttpAsync(req, callback);

                        }
                    }
                    catch (System.Exception ex)
                    {
                        callback?.Invoke(null, ex);
                    }
                }, null);
            }
            catch (System.Exception ex)
            {
                callback?.Invoke(null, ex);
            }
        }
        private static void Process_HttpAsync(HttpWebRequest req, Action<HttpWebResponse, System.Exception> callback)
        {
            try
            {
                req.BeginGetResponse(r =>
                {
                    if (!r.IsCompleted) return;
                    try
                    {
                        using (HttpWebResponse res = (HttpWebResponse)req.EndGetResponse(r))
                        {
                            callback?.Invoke(res, null);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        callback?.Invoke(null, ex);
                    }
                }, null);
            }
            catch (System.Exception ex)
            {
                callback?.Invoke(null, ex);
            }
        }
        private static void Process_GetHttpResponseString(HttpWebResponse res, System.Exception ex, Action<string, System.Exception> callback)
        {
            if (ex != null)
            {
                callback?.Invoke("", ex);
                return;
            }
            using (var stream = res.GetResponseStream())
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    var res_str = Encoding.UTF8.GetString(ms.ToArray());
                    callback?.Invoke(res_str, ex);
                }
            }

        }


        #endregion

    }


    #endregion

    #region PostFileData


    /// <summary>
    /// 文件上传的表单数据
    /// </summary>
    public class PostFileData
    {

        public delegate void CallProgressBar(object sender, double cur, double total);

        private bool _hasFile = false;
        private Stream _fileData;
        private string _fileName;
        private long position = 0;
        private long count = 0;

        /// <summary>
        /// 
        /// </summary>
        public CallProgressBar ProgressBarEvent { get; set; } = (a, b, c) => { };


        #region 构造方法


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public PostFileData(string filePath)
        {
            if (!File.Exists(filePath)) return;

            _hasFile = true;

            var ms = new MemoryStream(File.ReadAllBytes(filePath));

            Initializer(Path.GetFileName(filePath), ms, 0, ms.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="context"></param>
        public PostFileData(string filename, string context) : this(filename, context, Encoding.UTF8)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="context"></param>
        /// <param name="encoding"></param>
        public PostFileData(string filename, string context, Encoding encoding)
           : this(filename, encoding.GetBytes(context))
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="filedata"></param>
        public PostFileData(string filename, byte[] filedata)
        {
            if (filedata == null) return;
            Initializer(filename, new MemoryStream(filedata), 0, filedata.Length);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="stream"></param>
        public PostFileData(string filename, Stream stream)
            : this(filename, stream, 0, stream.Length)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="stream"></param>
        /// <param name="position"></param>
        /// <param name="count"></param>
        public PostFileData(string filename, Stream stream, long position, long count)
        {
            Initializer(filename, stream, position, count);
        }

        private void Initializer(string filename, Stream stream, long position, long count)
        {
            if (!stream.CanSeek)
            {
                throw new System.Exception("此流不支持查找操作");
            }
            this.position = position;
            this.count = count;
            this._fileData = stream;
            _fileName = filename;
            _hasFile = true;
        }


        #endregion


        private static byte[] GetBytesByStream(Stream stream)
        {
            if (!stream.CanRead) return null;
            byte[] data = new byte[512];
            int readcount = 0;
            using (var ms = new MemoryStream())
            {
                while ((readcount = stream.Read(data, 0, 512)) > 0)
                {
                    ms.Write(data, 0, readcount);
                }
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ms"></param>
        public void WriteTo(Stream ms)
        {
            //var curPosion = this._fileData.Position;

            this._fileData.Position = this.position;
            long total = 0;
            int len = (int)count;
            byte[] buffer = new byte[1024];
            len = len > 1024 ? 1024 : len;
            if (count <= 0) goto wEnd;

            while ((len = this._fileData.Read(buffer, 0, len)) > 0)
            {
                ms.Write(buffer, 0, len);
                ms.Flush();
                total += len;
                ProgressBarEvent(this, total, count);
                var tmp = count - total;
                if (tmp <= 0) goto wEnd;

                if (tmp > 1024)
                {
                    len = 1024;
                    continue;
                }
                len = (int)tmp;
            }

        wEnd:
            return;
        }


        /// <summary>
        ///  文件的名称
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
        }
        /// <summary>
        /// 文件流内容数据
        /// </summary>
        public Stream FileData { get { return _fileData; } }
        /// <summary>
        /// 是否存在文件信息
        /// </summary>
        public bool IsHasFile
        {
            get
            {
                return _hasFile;
            }
        }
    }


    #endregion

    #region CertPost
    public static partial class HttpHelper
    {

        static HttpHelper()
        {
            ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;// new RemoteCertificateValidationCallback(CheckValidationResult);
        }
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }


        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary>
        /// <param name="url">请求的Url</param>
        /// <param name="queryStream"></param>
        /// <param name="certFileName"></param>
        /// <param name="certPassword"></param>
        /// <param name="isFormData">表示当前提交的参数是不是表单提交的参数，如果是则会修改文档类型。参数需要进行url编码</param>
        /// <returns></returns>
        public static HttpWebResponse CertPostResponse(string url, string certFileName, string certPassword, Stream queryStream, bool isFormData)
        {
            HttpWebRequest req = BuildWebRequest(url, queryStream, ContextType.Unknown, null, null, certFileName, certPassword);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            return res;
        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static void CertPostResponseAsync(string url, string certFileName, string certPassword, Stream queryStream, bool isFormData, Action<HttpWebResponse> call)
        {
            if (call == null) call = EmptyCall;
            BuildWebRequestAsync(url, queryStream, ContextType.Unknown, null, null, certFileName, certPassword, req =>
            {
                req.BeginGetResponse(iar =>
                {
                    try
                    {
                        call((HttpWebResponse)req.EndGetResponse(iar));
                    }
                    catch
                    {
                        call(null);
                    }

                }, null);
            });


        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary>
        /// <param name="url">请求的Url</param>
        /// <param name="queryString"></param>
        /// <param name="certFileName"></param>
        /// <param name="certPassword"></param>
        /// <param name="callback"></param>
        /// <param name="isFormData">表示当前提交的参数是不是表单提交的参数，如果是则会修改文档类型。参数需要进行url编码</param>
        /// <returns></returns>
        public static HttpWebResponse CertPostResponse(string url, string certFileName, string certPassword, string queryString, bool isFormData)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(baseEncoding.GetBytes(queryString), 0, baseEncoding.GetByteCount(queryString));
            return CertPostResponse(url, certFileName, certPassword, ms, isFormData);
        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static void CertPostResponseAsync(string url, string certFileName, string certPassword, string queryString, bool isFormData, Action<HttpWebResponse> call)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(baseEncoding.GetBytes(queryString), 0, baseEncoding.GetByteCount(queryString));
            CertPostResponseAsync(url, certFileName, certPassword, ms, isFormData, call);
        }














        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary>
        /// <param name="url">请求的Url</param>
        /// <param name="queryStream"></param>
        /// <param name="certFileName"></param>
        /// <param name="certPassword"></param>
        /// <param name="isFormData">表示当前提交的参数是不是表单提交的参数，如果是则会修改文档类型。参数需要进行url编码</param>
        /// <returns></returns>
        public static string CertPostString(string url, string certFileName, string certPassword, Stream queryStream, bool isFormData)
        {
            return CertPostResponse(url, certFileName, certPassword, queryStream, isFormData).GetStringFromResponse();
        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static void CertPostStringAsync(string url, string certFileName, string certPassword, Stream queryStream, bool isFormData, Action<string> call)
        {
            if (call == null) call = EmptyCall;
            CertPostResponseAsync(url, certFileName, certPassword, queryStream, isFormData, res =>
            {
                call(res.GetStringFromResponse());
            });
        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary>
        /// <param name="url">请求的Url</param>
        /// <param name="queryString"></param>
        /// <param name="certFileName"></param>
        /// <param name="certPassword"></param>
        /// <param name="callback"></param>
        /// <param name="isFormData">表示当前提交的参数是不是表单提交的参数，如果是则会修改文档类型。参数需要进行url编码</param>
        /// <returns></returns>
        public static string CertPostString(string url, string certFileName, string certPassword, string queryString, bool isFormData)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(baseEncoding.GetBytes(queryString), 0, baseEncoding.GetByteCount(queryString));
            return CertPostString(url, certFileName, certPassword, ms, isFormData);
        }
        /// <summary>
        /// 对Http地址发起post请求，并直接获取响应对象
        /// </summary> 
        /// <returns></returns>
        public static void CertPostStringAsync(string url, string certFileName, string certPassword, string queryString, bool isFormData, Action<string> call)
        {

            MemoryStream ms = new MemoryStream();
            ms.Write(baseEncoding.GetBytes(queryString), 0, baseEncoding.GetByteCount(queryString));
            CertPostStringAsync(url, certFileName, certPassword, ms, isFormData, call);
        }












    }
    #endregion


}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class PostFromDataParam<T> : Dictionary<string, T>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public new T this[string key]
    {
        get
        {
            T val;
            base.TryGetValue(key, out val);
            return val;
        }
        set
        {
            base[key] = value;
        }
    }
}
static class ConstString
{

    internal const string urlencoded = "application/x-www-form-urlencoded";
    internal const string otc_stream = "application/otc-stream";
    internal readonly static string BOUNDARY = $"----------------{Guid.NewGuid().ToString("n").Substring(0, 12)}";
    internal const string MULTIPART_FORM_DATA = "multipart/form-data";

    internal readonly static string MULTIPART_FORM_DATAContextType = MULTIPART_FORM_DATA + ";boundary=" + BOUNDARY;
}


