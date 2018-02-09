/***************************************************************************
 * 
 * 创建时间：   2016/6/12 10:48:25
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   为网站数据传输提供参数管理及其简单的验证服务
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace SakerCore.Web
{
    /// <summary>
    /// 为网站数据传输提供参数管理及其简单的验证服务
    /// </summary> 
    public class WebParamData : SortedDictionary<string, string>, IDictionary<string, string>, IWebParamData
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DateTime UTCBegin = new DateTime(1970, 1, 1);
        /// <summary>
        /// 
        /// </summary>
        public static readonly DateTime LocalUTCBegin = TimeZone.CurrentTimeZone.ToLocalTime(UTCBegin);
        const string default_ts_key = "timestamp";
        const string default_sg_key = "sign";
        const string versionKey = "api_version";
        const int CurentVersion = 1;
        public static readonly IWebParamData Empty = new WebParamData();

        /// <summary>
        /// 指定签名接口版本号
        /// </summary>
        public virtual int Version
        {
            get
            {
                int version;
                int.TryParse(this[versionKey], out version);
                return version;
            }
            set
            {
                this[versionKey] = value + "";
            }
        }

        /// <summary>
        /// 采用Sha1加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        static string Sha1Encrypt(string context, Encoding encoding)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = System.Security.Cryptography.SHA1.Create().ComputeHash(encoding.GetBytes(context));
            return BitConverter.ToString(resultData).Replace("-", "");
        }
        /// <summary>
        /// 采用MD5加密协议加密字符串
        /// </summary>
        /// <param name="context">待加密的文本</param>
        /// <param name="encoding">加密文本的编码格式</param>
        /// <returns></returns>
        static string MD5Encrypt(string context, Encoding encoding)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = System.Security.Cryptography.MD5.Create().ComputeHash(encoding.GetBytes(context));
            return BitConverter.ToString(resultData).Replace("-", "");
        }
        /// <summary>
        /// 采用Sha256加密协议加密字符串
        /// </summary>
        /// <param name="context"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        static string Sha256Encrypt(string context, Encoding encoding)
        {
            if (context == null) throw new ArgumentNullException();
            //计算加密结果
            var resultData = System.Security.Cryptography.SHA256.Create().ComputeHash(encoding.GetBytes(context));
            return BitConverter.ToString(resultData).Replace("-", "");
        }

        /// <summary>
        /// 获取或设置与指定的键相关联的值
        /// </summary>
        /// <param name="key">需要参数的键</param>
        /// <returns>返回指定键的值，如果获取不到则为null</returns>
        public new string this[string key]
        {
            get
            {
                if (key == null) return null;
                string val;
                if (base.TryGetValue(key, out val))
                {
                    return val;
                }
                return null;
            }
            set { base[key] = value; }
        }

        /// <summary>
        /// 初始化一个默认实例
        /// </summary>
        public WebParamData() : base(WebParamDataKeyIComparerHelper.IgnoreCaseComparer)
        {

        }
        /// <summary>
        /// 初始化一个新实例
        /// </summary>
        /// <param name="ignoreCase">表示当前是否忽略大小写</param>
        public WebParamData(bool ignoreCase) : base((ignoreCase ? WebParamDataKeyIComparerHelper.IgnoreCaseComparer : WebParamDataKeyIComparerHelper.CaseComparer)) { }

        #region 数据输出方法


        /// <summary>
        /// 强键值对的数据转换为连接URL参数拼接参数格式
        /// </summary> 
        /// <returns></returns>
        public virtual string ToUrl()
        {
            StringBuilder strb = new StringBuilder();
            foreach (var d in this)
            {
                if (string.IsNullOrEmpty(d.Key)) continue;
                if (string.IsNullOrEmpty(d.Value)) continue;
                strb.Append(string.Format("&{0}={1}", d.Key.Trim(), WebHelper.UrlEncode(d.Value.Trim())));
            }
            if (strb.Length > 0)
            {
                strb = strb.Remove(0, 1);
            }
            return strb.ToString();
        }
        /// <summary>
        /// 强键值对的数据转换为连接URL参数拼接参数格式
        /// </summary> 
        /// <param name="func"></param>
        /// <returns></returns>
        public virtual string ToUrl(CheckUrlKeyCall func)
        {
            System.Text.StringBuilder strb = new System.Text.StringBuilder();
            foreach (var d in this)
            {
                if (string.IsNullOrEmpty(d.Key)) continue;
                if (string.IsNullOrEmpty(d.Value)) continue;
                if (!func(d.Key)) continue;
                strb.Append(string.Format("&{0}={1}", d.Key.Trim(), WebHelper.UrlEncode(d.Value.Trim())));
            }
            if (strb.Length > 0)
            {
                strb = strb.Remove(0, 1);
            }

            return strb.ToString();
        }
        /// <summary>
        /// 强键值对的数据转换为连接URL参数拼接参数,并不对参数值进行url编码
        /// </summary> 
        /// <returns></returns>
        public virtual string ToUrlNoEncode()
        {
            System.Text.StringBuilder strb = new System.Text.StringBuilder();
            foreach (var d in this)
            {
                if (string.IsNullOrEmpty(d.Key)) continue;
                if (string.IsNullOrEmpty(d.Value)) continue;
                strb.Append(string.Format("&{0}={1}", d.Key.Trim(), d.Value.Trim()));
            }
            if (strb.Length > 0)
            {
                strb = strb.Remove(0, 1);
            }

            return strb.ToString();
        }
        /// <summary>
        /// 将键值对的数据转换为连接URL参数拼接参数,并不对参数值进行url编码
        /// </summary> 
        /// <param name="func"></param>
        /// <returns></returns>
        public virtual string ToUrlNoEncode(CheckUrlKeyCall func)
        {
            StringBuilder strb = new StringBuilder();
            foreach (var d in this)
            {
                if (string.IsNullOrEmpty(d.Key)) continue;
                if (string.IsNullOrEmpty(d.Value)) continue;
                if (!func(d.Key)) continue;
                strb.Append(string.Format("&{0}={1}", d.Key.Trim(), d.Value.Trim()));
            }
            if (strb.Length > 0)
            {
                strb = strb.Remove(0, 1);
            }

            return strb.ToString();
        }
        /// <summary>
        /// 将数据装换为json数据格式
        /// </summary>
        /// <returns></returns>
        public virtual string ToJson()
        {
            return SakerCore.Serialization.Json.JsonHelper.ToZipJson(this);
        }
        /// <summary>
        /// 将数据转换为XML数据格式
        /// </summary>
        /// <returns></returns>
        public virtual string ToXml()
        {
            //数据为空时直接输出
            if (0 == this.Count)
            {
                return "<xml></xml>";
            }
            StringBuilder strb = new StringBuilder();
            strb.Append(@"<xml>");
            foreach (var pair in this)
            {
                if (string.IsNullOrEmpty(pair.Key)) continue;
                if (string.IsNullOrEmpty(pair.Value)) continue;
                strb.Append(string.Format("<{0}><![CDATA[{1}]]></{0}>", pair.Key, pair.Value));
            }
            strb.Append(@"</xml>");
            return strb.ToString();
        }
        /// <summary>
        /// 将数据转换为指定的字符边界的参数数据
        /// </summary>
        /// <param name="func"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        public virtual string ToParam(CheckUrlKeyCall func, char ch = '\"')
        {
            StringBuilder strb = new StringBuilder();
            foreach (var d in this)
            {
                if (string.IsNullOrEmpty(d.Key)) continue;
                if (string.IsNullOrEmpty(d.Value)) continue;
                if (!func(d.Key)) continue;
                strb.Append(string.Format("&{0}={1}{2}{1}", d.Key.Trim(), ch, d.Value.Trim()));
            }
            if (strb.Length > 0)
            {
                strb = strb.Remove(0, 1);
            }

            return strb.ToString();

        }
        /// <summary>
        /// 获取表单数据自动提交的文本内容
        /// </summary>
        /// <returns></returns>
        public virtual string ToPostHtml()
        {
            return ToPostHtml("");
        }
        /// <summary>
        /// 获取表单数据自动提交的文本内容
        /// </summary>
        /// <returns></returns>
        public virtual string ToPostHtml(string action)
        {
            return ToPostHtml(action, (r) => true);
        }
        /// <summary>
        /// 获取表单数据自动提交的文本内容
        /// </summary>
        /// <returns></returns>
        public virtual string ToPostHtml(CheckUrlKeyCall func)
        {
            return ToPostHtml("", func);
        }
        /// <summary>
        /// 获取表单数据自动提交的文本内容
        /// </summary>
        /// <returns></returns>
        public virtual string ToPostHtml(string action, CheckUrlKeyCall func)
        {
            System.Text.StringBuilder strb = new StringBuilder();

            strb.Append(@"<!DOCTYPE html><html><body><form id=""f1"" method=""post""");
            if (!string.IsNullOrEmpty(action))
            {
                strb.Append(@" action=""" + action + @"""");
            }
            strb.Append(">");

            //以下写入表单数据
            foreach (var d in this)
            {
                if (string.IsNullOrEmpty(d.Key)) continue;
                if (string.IsNullOrEmpty(d.Value)) continue;
                if (!func(d.Key)) continue;

                strb.Append($@"<input type=""hidden"" name=""{d.Key}"" value=""{System.Web.HttpUtility.HtmlEncode(d.Value)}"" />");
            }
            //写入自动提交数据脚本
            strb.Append(@"<script>f1.submit();</script></form></body></html>");

            //返回输出结果
            return strb.ToString();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string ToAllUrl()
        {
            System.Text.StringBuilder strb = new System.Text.StringBuilder();
            foreach (var d in this)
            {
                if (string.IsNullOrEmpty(d.Key)) continue;
                strb.Append(string.Format("&{0}={1}", d.Key.Trim(), WebHelper.UrlEncode(d.Value.Trim())));
            }
            if (strb.Length > 0)
            {
                strb = strb.Remove(0, 1);
            }

            return strb.ToString();
        }

        #endregion

        #region 数据反转换

        //数据反转换
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static WebParamData FromXml(string xml)
        {
            var m_values = new WebParamData();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
                XmlNodeList nodes = xmlNode.ChildNodes;
                foreach (XmlNode xn in nodes)
                {
                    XmlElement xe = (XmlElement)xn;
                    m_values[xe.Name] = xe.InnerText;
                }
                return m_values;
            }
            catch //(Exception ex)
            {
                return m_values;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static WebParamData FromJson(string json)
        {
            throw new NotImplementedException();
            //if (string.IsNullOrEmpty(json)) return new WebParamData();
            //try
            //{
            //    var J = Newtonsoft.Json.Linq.JObject.Parse(json);
            //    return J.ToObject<WebParamData>();
            //}
            //catch (System.Exception)
            //{
            //    return new WebParamData();
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static WebParamData FromUrl(string url)
        {
            var param = new WebParamData();
            try
            {
                int l = string.IsNullOrEmpty(url) ? 0 : url.Length;
                int i = 0;

                unsafe
                {
                    fixed (char* s = url)
                    {
                        while (i < l)
                        {
                            int si = i;
                            int ti = -1;

                            while (i < l)
                            {
                                char ch = s[i];

                                if (ch == '=')
                                {
                                    if (ti < 0)
                                        ti = i;
                                }
                                else if (ch == '&')
                                {
                                    break;
                                }
                                i++;
                            }
                            string name = null;
                            string value = null;

                            if (ti >= 0)
                            {
                                name = url.Substring(si, ti - si);
                                value = url.Substring(ti + 1, i - ti - 1);
                            }
                            else
                            {
                                value = url.Substring(si, i - si);
                                i++;
                                continue;
                            }
                            if (!string.IsNullOrEmpty(name) && !(string.IsNullOrEmpty(value)))
                                param.Add(name, WebHelper.UrlDecode(value));
                            i++;
                        }
                    }
                }
                return param;
            }
            catch (System.Exception)
            {

                return param;
            }
        }
        #endregion

        /// <summary>
        /// 计算数据的签名结果
        /// </summary>
        /// <param name="secrets">签名密钥参数组，在应对不同的数据签名时提供不同的数据签名参数</param>
        /// <param name="addsign">指示是否直接添加签名结果到参数集合内 将覆盖键 sign 的值。如果该值是 true 如果没有手动指定版本号的话会刷新版本号</param>
        /// <returns>返回数据签名结果</returns>
        public virtual string MarkSign(bool addsign, params string[] secrets)
        {
            //如果需要自动添加添加签名，且签名不
            if (addsign && !this.ContainsKey(versionKey))
                Version = CurentVersion;
            switch (Version)
            {
                case 0:
                    return MarkSignV0(addsign, secrets);
                case 1:
                    return MarkSignV1(addsign, secrets);
                default:
                    return "";
            }
        }

        private string MarkSignV1(bool addsign, string[] secrets)
        {
            /*
            
            说明：关于签名我们约定使用 sign_type 字段来指示数据该使用何种签名方式，目前支持 SHA1,MD5,SHA256 三种签名方式
                  其中 sign_type 字段的值必须全部是大写，并且不包含空格等其他字符
                  默认使用SHA1加密方式对数据生成签名
            
            */
            if (secrets.Length < 2) return "";

            var secret = secrets[0];
            var url = secrets[1];
            var sign = "";
            var stringA = this.ToUrlNoEncode(p => p != default_sg_key);
            string unsignstr = $"{url}?{stringA}&{secret}";
            switch (WebSignType)
            {
                case SignType.MD5:
                    sign = MD5Encrypt(unsignstr, Encoding.UTF8); break;
                default:
                    sign = Sha1Encrypt(unsignstr, Encoding.UTF8); break;
            }
            if (addsign)
                this[default_sg_key] = sign;
            return sign;
        }

        string MarkSignV0(bool addsign, params string[] secrets)
        {

            /*
            
            说明：关于签名我们约定使用 sign_type 字段来指示数据该使用何种签名方式，目前支持 SHA1,MD5,SHA256 三种签名方式
                  其中 sign_type 字段的值必须全部是大写，并且不包含空格等其他字符
                  默认使用SHA1加密方式对数据生成签名
            
            */
            var secret = secrets[0];
            var sign = "";
            switch (WebSignType)
            {
                case SignType.MD5:
                    sign = MD5Encrypt(this.ToUrlNoEncode(p => p != default_sg_key) + secret, Encoding.UTF8); break;
                default:
                    sign = Sha1Encrypt(this.ToUrlNoEncode(p => p != default_sg_key) + secret, Encoding.UTF8); break;
            }
            if (addsign)
                this[default_sg_key] = sign;
            return sign;
        }













        /// <summary>
        /// 数据验签方法验证签名的正确性
        /// </summary>
        /// <param name="secrets">签名密钥参数组，在应对不同的数据签名时提供不同的数据签名参数</param>
        /// <returns>返回签名验证结果，如果验证通过则为 true 否则为 false</returns>
        public virtual bool VerifySign(params string[] secrets)
        {
            //获取回传数据的签名
            var sign = this[default_sg_key];
            if (string.IsNullOrEmpty(sign)) return false;
            var j_sign = this.MarkSign(false, secrets);

            return string.Equals(sign, j_sign, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 获取或者设置当前协议的加密类别 目前支持 SHA1,MD5,SHA256 三种签名方式
        /// </summary>
        public virtual SignType WebSignType
        {
            get
            {
                var sign_type = this["sign_type"];
                switch (sign_type)
                {
                    case "MD5":
                        return SignType.MD5;
                    case "RSA2":
                        return SignType.RSA2;
                    case "RSA":
                        return SignType.RSA;
                    default:
                        return SignType.SHA1;
                }
            }
            set
            {
                switch (value)
                {
                    case SignType.MD5: this["sign_type"] = "MD5"; break;
                    case SignType.SHA1: this["sign_type"] = "SHA1"; break;
                    case SignType.RSA: this["sign_type"] = "RSA"; break;
                    case SignType.RSA2: this["sign_type"] = "RSA2"; break;
                    default: this["sign_type"] = "SHA1"; break;
                }
            }
        }
        /// <summary>
        /// 检查当前请求是否已经超时
        /// </summary>
        /// <param name="timespan">验证超时阈值（单位：秒）</param>
        /// <returns>返回一个布尔值表示当前的请求是否超时，如果已超时返回 true 否则 返回 false</returns>
        public virtual bool CheckTimeOut(int timespan)
        {
            var timestampStr = this[default_ts_key];
            long time;
            if (!long.TryParse(timestampStr, out time))
            {
                return true;
            }
            DateTime dtStart = LocalUTCBegin.AddSeconds(time);
            //获取当前时间戳表示的时间信息
            var now = DateTime.Now;
            return Math.Abs((now - dtStart).TotalSeconds) >= timespan;
        }
        /// <summary>
        /// 输出一个新的 WebParam 对象;
        /// </summary>
        /// <param name="ignoreCase">指示新对象是否不区分大小写</param>
        /// <returns></returns>
        public virtual IWebParamData ToNewWebParamData(bool ignoreCase)
        {
            var new_o = new WebParamData(ignoreCase);

            foreach (var s in this)
            {
                new_o[s.Key] = s.Value;
            }

            return new_o;
        }
        /// <summary>
        /// 删除时间戳
        /// </summary>
        public virtual IWebParamData RemoveTimestamp()
        {
            this.Remove(default_ts_key);
            return this;
        }
        /// <summary>
        /// 删除签名
        /// </summary>
        public virtual IWebParamData RemoveSign()
        {
            this.Remove(default_sg_key);
            return this;
        }
        /// <summary>
        /// 标记时间戳
        /// </summary>
        public virtual IWebParamData MarkTimestamp()
        {
            this[default_ts_key] = ((long)(DateTime.Now - LocalUTCBegin).TotalSeconds).ToString();
            return this;
        }

        #region WebParamDataKeyIComparerHelper

        //字符串类的比较算法，不区分大小写
        static class WebParamDataKeyIComparerHelper
        {
            public static IComparer<string> IgnoreCaseComparer = new IgnoreCaseComparerHelper();
            public static IComparer<string> CaseComparer = new ComparerHelper();
            class IgnoreCaseComparerHelper : IComparer<string>
            {
                public int Compare(string x, string y) { return string.Compare(x, y, true); }
            }
            class ComparerHelper : IComparer<string>
            {
                public int Compare(string x, string y)
                {
                    unsafe
                    {
                        int x_len = x.Length;
                        int y_len = x.Length;

                        var c_len = x_len < y_len ? x_len : y_len;

                        fixed (char* x_pos = x)
                        fixed (char* y_pos = y)
                        {
                            for (int i = 0; i < c_len; i++)
                            {
                                if (x_pos[i] == y_pos[i]) continue;

                                if (x_pos[i] < y_pos[i]) return -1;
                                return 1;
                            }
                        }

                        if (x_len == y_len) return 0;

                        if (x_len < y_len) return -1;
                        return 1;



                    }





                }
            }
        }
        #endregion


        /*********** 以下是一些内部调用类库，不公开。优化约定去掉一些索引检查之类的 ********************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IWebParamData AddKeyValue(string key, string value)
        {
            this[key] = value;
            return this;
        }

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate bool CheckUrlKeyCall(string key);
}
namespace SakerCore.Web
{
    /// <summary>
    /// 定义一个接口为网站数据传输提供参数管理及其简单的验证服务必须实现的方法
    /// </summary>
    public interface IWebParamData : IDictionary<string, string>
    {
        /// <summary>
        /// 指定签名接口版本号
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// 获取或设置与指定的键相关联的值
        /// </summary>
        /// <param name="key">需要参数的键</param>
        /// <returns>返回指定键的值，如果获取不到则为null</returns>
        new string this[string key] { get; set; }
        /// <summary>
        /// 计算数据的签名结果
        /// </summary>
        /// <param name="secrets">签名密钥参数组，在应对不同的数据签名时提供不同的数据签名参数</param>
        /// <param name="addsign">指示是否直接添加签名结果到参数集合内 将覆盖键 sign 的值</param>
        /// <returns>返回数据签名结果</returns>
        string MarkSign(bool addsign, params string[] secrets);
        /// <summary>
        /// 将数据装换为json数据格式
        /// </summary>
        /// <returns></returns>
        string ToJson();
        /// <summary>
        /// 将数据转换为指定的字符边界的参数数据
        /// </summary>
        /// <param name="func"></param>
        /// <param name="ch"></param>
        /// <returns></returns>
        string ToParam(CheckUrlKeyCall func, char ch = '"');
        /// <summary>
        /// 获取表单数据自动提交的文本内容
        /// </summary>
        /// <returns></returns>
        string ToPostHtml();
        /// <summary>
        /// 获取表单数据自动提交的文本内容
        /// </summary>
        /// <returns></returns>
        string ToPostHtml(string action);
        /// <summary>
        /// 获取表单数据自动提交的文本内容
        /// </summary>
        /// <returns></returns>
        string ToPostHtml(CheckUrlKeyCall func);
        /// <summary>
        /// 获取表单数据自动提交的文本内容
        /// </summary>
        /// <returns></returns>
        string ToPostHtml(string action, CheckUrlKeyCall func);
        /// <summary>
        /// 强键值对的数据转换为连接URL参数拼接参数格式
        /// </summary> 
        /// <returns></returns>
        string ToUrl();
        /// <summary>
        /// 强键值对的数据转换为连接URL参数拼接参数格式
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        string ToUrl(CheckUrlKeyCall func);
        /// <summary>
        /// 强键值对的数据转换为连接URL参数拼接参数,并不对参数值进行url编码
        /// </summary> 
        /// <returns></returns>
        string ToUrlNoEncode();
        /// <summary>
        /// 强键值对的数据转换为连接URL参数拼接参数,并不对参数值进行url编码
        /// </summary> 
        /// <returns></returns>
        string ToUrlNoEncode(CheckUrlKeyCall func);
        /// <summary>
        /// 将数据转换为XML数据格式
        /// </summary>
        /// <returns></returns>
        string ToXml();
        /// <summary>
        /// 数据验签方法验证签名的正确性
        /// </summary>
        /// <param name="secrets">签名密钥参数组，在应对不同的数据签名时提供不同的数据签名参数</param>
        /// <returns>返回签名验证结果，如果验证通过则为 true 否则为 false</returns>
        bool VerifySign(params string[] secrets);
        /// <summary>
        /// 获取或者设置当前协议的加密类别
        /// </summary>
        SignType WebSignType { get; set; }
        /// <summary>
        /// 输出一个新的 WebParam 对象;
        /// </summary>
        /// <param name="ignoreCase">指示新对象是否不区分大小写</param>
        /// <returns></returns>
        IWebParamData ToNewWebParamData(bool ignoreCase);
        /// <summary>
        /// 检查当前请求是否已经超时
        /// </summary>
        /// <param name="timespan">验证超时阈值</param>
        /// <returns>返回一个布尔值表示当前的请求是否超时，如果已超时返回 true 否则 返回 false</returns>
        bool CheckTimeOut(int timespan);
        /// <summary>
        /// 标记时间戳
        /// </summary>
        IWebParamData MarkTimestamp();
        /// <summary>
        /// 删除时间戳参数
        /// </summary>
        IWebParamData RemoveTimestamp();
        /// <summary>
        /// 删除签名参数
        /// </summary>
        IWebParamData RemoveSign();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IWebParamData AddKeyValue(string key, string value);

    }
    /// <summary>
    /// 签名类别
    /// </summary>
    public enum SignType
    {
        /// <summary>
        /// MD5 加密方式
        /// </summary>
        MD5,
        /// <summary>
        /// SHA1 加密方式
        /// </summary>
        SHA1,
        /// <summary>
        /// 
        /// </summary>
        RSA2,
        /// <summary>
        /// 
        /// </summary>
        RSA
    }
}


namespace SakerCore.Web
{
    /// <summary>
    /// 网站操作辅助类
    /// </summary>
    public partial class WebHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string UrlEncode(string str, Encoding e)
        {
            return System.Web.HttpUtility.UrlEncode(str, e);
        }
        /// <summary>
        /// 执行URL编码
        /// </summary>
        /// <param name="str">需要编码的字符串</param> 
        /// <returns>返回编码后的字符串结果</returns>
        public static string UrlEncode(string str)
        {
            return System.Web.HttpUtility.UrlEncode(str, Encoding.UTF8);
        }
        /// <summary>
        /// 将编码后的字符串结果解密
        /// </summary>
        /// <param name="str">待解密的编码字符串信息</param>
        /// <param name="e">编码时使用字符串编码信息</param>
        /// <returns>返回解码后的字符串信息</returns>
        public static string UrlDecode(string str, Encoding e)
        {
            return System.Web.HttpUtility.UrlDecode(str, e);
        }
        /// <summary>
        /// 执行URl解码
        /// </summary>
        /// <param name="str">需要解码的字符串</param> 
        /// <returns>返回解码后的字符串信息</returns>
        public static string UrlDecode(string str)
        {
            return System.Web.HttpUtility.UrlDecode(str, Encoding.UTF8);
        }

    }
}