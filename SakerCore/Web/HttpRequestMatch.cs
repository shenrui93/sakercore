/***************************************************************************
 * 
 * 创建时间：   2017/1/23 18:41:53
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供 Http 请求头解析类
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.Web
{

    /*************************************

        ================= 请求头参数参考 ===================
 
GET /index.html?p=1 HTTP/1.1
Host: xzmj.game1.8hb.cc
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0 
Accept-Language: zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3
Accept-Encoding: gzip, deflate
Connection: keep-alive
Cache-Control: no-cache
Postman-Token: ddbaa919-292b-a30d-3908-ad0ad055e3a1

---------------------------------body-----------------------

         


---------------------------------body-----------------------
*/

    /// <summary>
    /// 提供 Http 请求头解析类
    /// </summary>
    public class HttpRequestMatch
    {
        /// <summary>
        /// 获取当前请求的谓词
        /// </summary>
        public string Method { get; private set; }
        /// <summary>
        /// 请求的基础路径
        /// </summary>
        public string Path { get; private set; }
        /// <summary>
        /// 获取请求的查询参数
        /// </summary>
        public IWebParamData QueryString { get; private set; }
        /// <summary>
        /// 获取请求头参数
        /// </summary>
        public IWebParamData Header { get; private set; }
        /// <summary>
        /// 获取当前的HTTP请求的版本号
        /// </summary>
        public string HttpVersion { get; private set; }

        private HttpRequestMatch()
        {

        } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerBody"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool TryParse(string headerBody, out HttpRequestMatch param)
        {
            param = null;
            var tempParam = new HttpRequestMatch();
            //请求头数据的读取类
            StringReader reader = new StringReader(headerBody);
            StringBuilder tempSb = new StringBuilder();
            if (!TryParsePathInfo(reader, tempParam, tempSb))
            {
                return false;
            }
            if (!TryParseHeaderInfo(reader, tempParam, tempSb))
            {
                return false;
            }
            param = tempParam;
            return true;
        }

        private static bool TryParseHeaderInfo(StringReader reader, HttpRequestMatch temp_param, StringBuilder temp_sb)
        { 
            //读取的字符缓存
            int ch, linecount = 0;
            //提供请求头数据缓存
            var webParam = new WebParamData();
            temp_param.Header = webParam;

            string key, value;

            //开始读取请求头
            while (true)
            {
                ch = reader.Read();
                //遇到文档末尾结束
                if (ch == -1) return true;
                //遇到回车符继续
                if (ch == '\r') continue;

                //遇到换行符
                if (ch == '\n')
                {
                    linecount++;
                    if (linecount >= 2)
                    {
                        //遇到空行结束
                        return true;
                    }
                    continue;
                }
                //换行符计数清零
                linecount = 0;

                #region 解析key

                temp_sb.Length = 0;
                while (true)
                {
                    temp_sb.Append((char)ch);
                    ch = reader.Read();
                    //意外遇到文档末尾结束
                    if (ch == -1 || ch == '\n') return false;
                    //遇到回车符继续
                    if (ch == '\r') continue;

                    if (ch == ':')
                    {
                        //遇到分隔符,检查下一个字符是否是空格
                        ch = reader.Peek();
                        if (ch != ' ')
                        {
                            //不是空格继续
                            continue;
                        }

                        //是空格，获取键值结束,推进读取游标
                        reader.Read();

                        key = temp_sb.ToString();

                        break;
                    }
                }

                #endregion

                value = reader.ReadLine();
                linecount = 1;
                webParam[key] = value;

                continue;
            }
        }
        private static bool TryParsePathInfo(StringReader reader, HttpRequestMatch temp_param, StringBuilder temp_sb)
        {
            temp_sb.Length = 0;
            int ch;
            //开始解析请求头谓词
            while (true)
            {
                ch = reader.Read();
                //回车符继续
                if (ch == '\r') continue;
                //遇到意外的换行符或者文档末尾，结束解析
                if (ch == -1 || ch == '\n') return false;
                //遇到空格结束
                if (ch == ' ') break;

                temp_sb.Append((char)ch);
            }
            temp_param.Method = temp_sb.ToString();


            bool hasquerystring = false;

            //开始解析请求头路径
            temp_sb.Length = 0;
            while (true)
            {
                ch = reader.Read();
                //回车符继续
                if (ch == '\r') continue;
                //遇到意外的换行符或者文档末尾，结束解析
                if (ch == -1 || ch == '\n') return false;
                //遇到空格结束
                if (ch == ' ') break;
                if (ch == '?')
                {
                    //路径中包含查询字符串
                    hasquerystring = true;
                    break;
                }

                temp_sb.Append((char)ch);
            }
            temp_param.Path = temp_sb.ToString();

            if (hasquerystring)
            {
                //路径中包含查询字符串，解析查询参数
                temp_sb.Length = 0;
                while (true)
                {
                    ch = reader.Read();
                    //回车符继续
                    if (ch == '\r') continue;

                    //遇到意外的换行符或者文档末尾，结束解析
                    if (ch == -1 || ch == '\n') return false;
                    //遇到空格结束
                    if (ch == ' ') break;

                    temp_sb.Append((char)ch);
                }
                //解析查询字符串信息
                temp_param.QueryString = WebParamData.FromUrl(temp_sb.ToString());
            }
            //解析请求头HTTP版本信息
            temp_sb.Length = 0;
            while (true)
            {
                ch = reader.Read();
                //回车符继续
                if (ch == '\r') continue;
                //遇到意外字符
                if (ch == ' ') return false;
                //换行符结束
                if (ch == '\n') break;

                temp_sb.Append((char)ch);
            }
            temp_param.HttpVersion = temp_sb.ToString();




            return true;
        }
    }
}
