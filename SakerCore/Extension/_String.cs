using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SakerCore.Extension
{
    /// <summary>
    /// 字符串类型扩展
    /// </summary>
    public static class _String
    {

        /// <summary>
        /// 将字符串格式化为 16 位无符号整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ushort ParseToUInt16(this string str)
        {
            return ushort.Parse(str);
        }
        /// <summary>
        /// 将字符串格式化为 32 位无符号整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static uint ParseToUInt32(this string str)
        {
            return uint.Parse(str);
        }
        /// <summary>
        ///  将字符串格式化为 64 位无符号整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ulong ParseToUInt64(this string str)
        {
            return ulong.Parse(str);
        }
        /// <summary>
        /// 将字符串格式化为 16 位有符号整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static short ParseToInt16(this string str)
        {
            return short.Parse(str);
        }
        /// <summary>
        /// 将字符串格式化为 32 位有符号整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ParseToInt32(this string str)
        {
            return int.Parse(str);
        }
        /// <summary>
        /// 将字符串格式化为 64 位有符号整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long ParseToInt64(this string str)
        {
            return long.Parse(str);
        }
        /// <summary>
        /// 将字符串格式化为时间格式的对象
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime ParseToDateTime(this string str)
        {
            DateTime datetime;
            if (DateTime.TryParse(str, out datetime))
                return datetime;
            throw new FormatException();
        }
        /// <summary>
        /// 反转字符串顺序
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public unsafe static string Reverse(this string str)
        {
            int length = str.Length;
            if (length <= 0)
                return string.Empty;
            char[] reverchars;
            Array.Reverse((reverchars = str.ToCharArray()));
            return new string(reverchars);
        }
        /// <summary>
        /// 从当前 System.String 对象尾部移除指定字符串。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static string TrimEnd(this string str, string arg)
        {
            if (string.IsNullOrEmpty(str)) return "";
            if (string.IsNullOrEmpty(arg)) return str;
            if (!str.EndsWith(arg)) return str;
            return str.Substring(0, str.Length - arg.Length);
        }
        /// <summary>
        /// 从当前 System.String 对象开始移除指定字符串。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static string TrimStart(this string str, string arg)
        {
            if (string.IsNullOrEmpty(str)) return "";
            if (string.IsNullOrEmpty(arg)) return str;
            if (!str.StartsWith(arg)) return str;
            return str.Substring(arg.Length - 1);
        }
        /// <summary>
        /// 将当前字符串的所有字符编码为一个字节序列
        /// </summary>
        /// <param name="str">需要编码的字符串</param>
        /// <returns>返回编码后的字节数组</returns>
        public static byte[] GetBytes(this string str)
        {
            return GetBytes(str, Encoding.UTF8);
        }
        /// <summary>
        /// 将当前字符串的所有字符编码为一个字节序列。
        /// </summary>
        /// <param name="str">需要编码的字符串</param>
        /// <param name="encode">使用的字符串编码</param>
        /// <returns>返回编码后的字节数组</returns>
        public static byte[] GetBytes(this string str, Encoding encode)
        {
            return encode.GetBytes(str);
        }
        /// <summary>
        /// 计算当前字符串的字符进行编码时所产生的字节数。
        /// </summary>
        /// <param name="str">需要计算编码字节数的字符串</param>
        /// <returns>对指定字符进行编码后生成的字节数</returns>
        public static int GetByteCount(this string str)
        {
            return GetByteCount(str, Encoding.UTF8);
        }
        /// <summary>
        /// 计算当前字符串的字符进行编码时所产生的字节数。
        /// </summary>
        /// <param name="str">需要计算编码字节数的字符串</param>
        /// <param name="encode">使用的字符串编码</param>
        /// <returns>对指定字符进行编码后生成的字节数</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetByteCount(this string str, Encoding encode)
        {
            return encode.GetByteCount(str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static string[] Split(this string str, string sp)
        {
            if (string.IsNullOrEmpty(sp)) return new string[0];
            if (string.IsNullOrEmpty(str)) return new string[0];
            unsafe
            {
                List<string> ls = new List<string>();

                var strLength = str.Length;
                var spLength = sp.Length;

                int index = 0, len = 0;

                char start_ch = sp[0];



                fixed (char* ch = str)
                fixed (char* spch = sp)
                {
                    for (int i = 0; i < strLength;)
                    {
                        if (start_ch == ch[i])
                        {
                            for (int c = 1; c < spLength;)
                            {
                                if (spch[c] != ch[i + c])
                                {
                                    i += c;
                                    goto continue1;
                                }
                                c++;
                            }
                            index = len;
                            len = i - len;
                            if (len > 0)
                                ls.Add(str.Substring(index, len));
                            i += spLength;
                            len = i;
                            continue1:
                            continue;
                        }
                        i++;
                    }
                }

                index = len;
                len = strLength - len;
                if (len > 0)
                    ls.Add(str.Substring(index, len));
                return ls.ToArray();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string str, Encoding encode)
        {
           return encode.GetBytes(str);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string str)
        {
           return Encoding.UTF8.GetBytes(str);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUrlEncode(this string str)
        {
            return SakerCore.Web.WebHelper.UrlEncode(str);
        }

        public static string GetUrlHostAndPath(this string uri)
        { 
            Uri u;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out u))
            {
                return null;
            };
            return u.GetHostAndPath();
        }
    }
}
