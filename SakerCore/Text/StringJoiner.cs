/***************************************************************************
 * 
 * 创建时间：   2016/7/14 12:16:31
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   包含一个封装 StringBuilder 的字符串连接处理类，（当前类的所有操作运算符都是引用传递操作）
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SakerCore.Text
{
    /// <summary>
    /// 包含一个封装 StringBuilder 的字符串连接处理类，（当前类的所有操作运算符都是引用传递操作）
    /// </summary>
    public class StringJoiner
    {
        StringBuilder sb;
        /// <summary>
        /// 初始化 <see cref="StringJoiner"/> 类的新实例。
        /// </summary>
        public StringJoiner()
        {
            sb = new StringBuilder();
        }
        /// <summary>
        /// 使用指定的字符串初始化 Uyi.Text.StringJoiner 类的新实例。
        /// </summary>
        public StringJoiner(string value)
        {
            //初始化
            sb = new StringBuilder(value);
        }


        /// <summary>
        /// 包含一个隐示转换，即将 String 类型 转换为 StringJoiner
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator StringJoiner(string value)
        {
            var join = new StringJoiner(value);
            return join;
        }
        /// <summary>
        /// 包含一个隐示转换，即将 StringJoiner 类型 转换为 String
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string (StringJoiner value)
        {
            return value?.sb.ToString();
        }


        #region   + 操作运算符重载 

        /**************************        + 操作运算符重载         *************************/

        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, string value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, bool value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, byte value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, sbyte value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, short value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, ushort value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, int value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, uint value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, long value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, ulong value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, float value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, double value)
        {
            j.sb.Append(value);
            return j;
        }
        /// <summary>
        /// 
        /// </summary> 
        public static StringJoiner operator +(StringJoiner j, decimal value)
        {
            j.sb.Append(value);
            return j;
        }


        #endregion

        
        /// <summary>
        /// 返回当前的字符串值
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            return sb.ToString();
        }
    }
}
