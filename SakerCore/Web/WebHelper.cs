/***************************************************************************
 * 
 * 创建时间：   2016/6/7 18:02:19
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   为网站的操作提供一些杂项方法
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace SakerCore.Web
{
    /// <summary>
    /// 为网站的操作提供一些杂项方法
    /// </summary>
    public partial class WebHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="title"></param>
        /// <param name="size"></param>
        /// <param name="maxszie"></param>
        /// <param name="minsize"></param>
        /// <param name="getvalue">获取数据的方法</param>
        /// <returns></returns>
        public static string WriterTableInfo(System.Data.DataTable dt, string title, int[] size, int maxszie = 0, int minsize = 0, GetValueMethod getvalue = null)
        {
            getvalue = getvalue ?? GetValue;
            StringBuilder strb = new StringBuilder();

            size = size ?? new int[0];

            //写入网页标题
            strb.Append($@"<!DOCTYPE html><html><head>
<title>{title}</title>
<style>
div{{text-align:center}}
table{{border-collapse:collapse;border-spacing:0;border-left:1px solid #888;border-top:1px solid #888;background:#efefef;width:100%;margin:0 auto;
{(minsize <= 0 ? "" : $"min-width:{minsize}px;")}{(maxszie <= 0 ? "" : $"max-width:{maxszie}px;")}
}}
table tr:first-child{{text-align:center}}
th,td{{border-right:1px solid #888;border-bottom:1px solid #888;padding:5px 15px;word-break:break-all}}
th{{font-weight:bold;background:#ccc}}
</style>
</head><body>");
            //写标题
            strb.Append($"<div><h2>{title}</h2></div>");

            //开始写表
            strb.Append($"<table>");
            //写入列标题
            strb.Append($"<tr>");
            int index = 0;
            foreach (System.Data.DataColumn dcol in dt.Columns)
            {
                strb.Append($"<td {(size.Length < index + 1 ? "" : $"style=\"width:{size[index++]}px\"")}>{dcol.ColumnName}</td>");
            }
            strb.Append($"</tr>");


            var cols = dt.Columns;
            //写入数据行
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                strb.Append($"<tr>");
                foreach (System.Data.DataColumn col in cols)
                {
                    strb.Append($"<td>{getvalue(dr[col.ColumnName].ToString(), col)}</td>");
                }
                strb.Append($"</tr>");
            }
            strb.Append($"</table>");
            strb.Append($@"</body></html>");


            return strb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dts"></param>
        /// <param name="title"></param>
        /// <param name="getvalue">获取数据的方法</param>
        /// <returns></returns>
        public static string WriterTableArray(System.Data.DataTable[] dts, string title, GetValueMethod getvalue = null)
        {
            getvalue = getvalue ?? GetValue;

            StringBuilder strb = new StringBuilder();


            //写入网页标题
            strb.Append($@"<!DOCTYPE html><html><head>
<title>{title}</title>
<style>
div{{text-align:center}}
table{{border-collapse:collapse;border-spacing:0;border-left:1px solid #888;border-top:1px solid #888;background:#efefef;width:100%;margin:0 auto;
}}
table tr:first-child{{text-align:center}}
th,td{{border-right:1px solid #888;border-bottom:1px solid #888;padding:5px 15px;word-break:break-all}}
th{{font-weight:bold;background:#ccc}}
</style>
</head><body>");
            //写标题
            strb.Append($"<div><h2>{title}</h2></div>");


            foreach (var dt in dts)
            {
                _WriterSingleTable(dt, strb, getvalue);
                strb.Append("<br/><br/><br/>");
            }

            strb.Append($@"</body></html>");
            return strb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="getvalue">获取数据的方法</param>
        /// <returns></returns>
        public static string WriterSingleTable(DataTable dt, GetValueMethod getvalue = null)
        {
            getvalue = getvalue ?? GetValue;
            StringBuilder strb = new StringBuilder();

            //开始写表
            strb.Append($"<table>");
            //写入列标题
            strb.Append($"<tr>");
            foreach (System.Data.DataColumn dcol in dt.Columns)
            {
                strb.Append($"<td >{dcol.ColumnName}</td>");
            }
            strb.Append($"</tr>");

            var cols = dt.Columns;
            //写入数据行
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                strb.Append($"<tr>");
                foreach (System.Data.DataColumn col in cols)
                {
                    strb.Append($"<td>{getvalue(dr[col.ColumnName].ToString(), col)}</td>");
                }
                strb.Append($"</tr>");
            }
            strb.Append($"</table>");

            return strb.ToString();
        }


        static string GetValue(string value, DataColumn dr)
        {
            if (string.IsNullOrEmpty(value)) return "";
            value = value.Trim();
            //if (maxlen == 0) return value.Trim();
            //if (value.Length >= maxlen)
            //    return $@"<span title=""{value}"">{value.Substring(0, maxlen)}...</span>";
            return value;
        }
        private static void _WriterSingleTable(DataTable dt, StringBuilder strb, GetValueMethod getvalue = null)
        {
            strb.Append($"<div><h4>{dt.TableName}</h4></div>");
            //开始写表
            strb.Append($"<table>");
            //写入列标题
            strb.Append($"<tr>");
            foreach (System.Data.DataColumn dcol in dt.Columns)
            {
                strb.Append($"<td >{dcol.ColumnName}</td>");
            }
            strb.Append($"</tr>");

            var cols = dt.Columns;
            //写入数据行
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                strb.Append($"<tr>");
                foreach (System.Data.DataColumn col in cols)
                {
                    strb.Append($"<td>{getvalue(dr[col.ColumnName].ToString(), col)}</td>");
                }
                strb.Append($"</tr>");
            }
            strb.Append($"</table>");
        }

        /// <summary>
        /// URL安全的字符串编码
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string UrlSafeBase64Encode(byte[] s, bool fb = false)
        {
            return Convert.ToBase64String(s).Replace('+', '-')
                .Replace('/', '_');
        }
        /// <summary>
        /// URL安全的字符串解码
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] UrlSafeBase64Decode(string s)
        {
            return SakerCore.Serialization.Base64Serialzier.FromBase64String(s);
        }

        /// <summary>
        /// 将参数合并到Url连接中
        /// </summary>
        /// <param name="url">需要合并参数的url</param>
        /// <param name="param">需要合并的参数</param>
        /// <param name="isCover">指定使用覆盖模式，如果需要用合并参数覆盖原url参数则为 true。如果使用原url参数覆盖合并参数则为 false </param>
        /// <returns></returns>
        public static string MergeUrlQueryString(string url, IWebParamData param, bool isCover = false)
        {
            if (param == null) return url;
            if (string.IsNullOrEmpty(url)) return "?" + param.ToUrl();
            var wen_index = url.IndexOf('?');
            if (wen_index < 0)
            {
                return url + "?" + param.ToUrl();
            }
            if (wen_index + 1 >= url.Length) return url + param.ToUrl();

            var path = url.Substring(0, wen_index);
            var qstr = url.Substring(wen_index + 1);

            var q_p = WebParamData.FromUrl(qstr);
            if (isCover)
            {
                //用合并参数覆盖原url参数
                foreach (var r in param)
                {
                    q_p[r.Key] = r.Value;
                }
                return $"{path}?{q_p.ToUrl()}";
            }
            else
            {
                //用原url参数覆盖合并参数 
                foreach (var r in q_p)
                {
                    param[r.Key] = r.Value;
                }

                return $"{path}?{param.ToUrl()}";
            }

        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="col"></param>
    public delegate string GetValueMethod(string value, DataColumn col);
}
