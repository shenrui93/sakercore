using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SakerCore.Extension
{
    /// <summary>
    /// 根类型扩展，方法会扩展到所有类型上
    /// </summary>
    public static class _Object
    {
        static bool CheckObjectBefore(object obj)
        {
            if (obj == null) return false;
            return !Convert.IsDBNull(obj);
        }
        static long TryGetInt64Value(this object obj)
        {
            if (!CheckObjectBefore(obj)) return 0; 
            return Convert.ToInt64(obj);
        }





        /************* 基础数据类型转换 *************/

        /// <summary>
        /// 将对象转换为 16 位无符号整数
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ushort ConvertToUInt16(this object obj)
        {
            return (ushort)(obj.TryGetInt64Value());
        }
        /// <summary>
        /// 将对象转换为 32 位无符号整数
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static uint ConvertToUInt32(this object obj)
        {
            return (uint)(obj.TryGetInt64Value());
        }
        /// <summary>
        /// 将对象转换为 64 位无符号整数
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ulong ConvertToUInt64(this object obj)
        {
            return (ulong)(obj.TryGetInt64Value());
        }
        /// <summary>
        /// 将对象转换为 16 位有符号整数
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static short ConvertToInt16(this object obj)
        {
            return (short)(obj.TryGetInt64Value());
        }
        /// <summary>
        /// 将对象转换为 32 位有符号整数
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int ConvertToInt32(this object obj)
        {
            return (int)obj.TryGetInt64Value();
        }
        /// <summary>
        /// 将对象转换为 64 位有符号整数
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static long ConvertToInt64(this object obj)
        {
            return obj.TryGetInt64Value();
        }
        /// <summary>
        /// 将指定对象的值转换为等效的十进制数。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte ConvertToByte(this object obj)
        {
            return (byte)obj.TryGetInt64Value();
        }

        /// <summary>
        /// 将对象转换为时间格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(this object obj)
        {
            if (!CheckObjectBefore(obj)) return DateTime.MinValue;
            return Convert.ToDateTime(obj);
        }
        /// <summary>
        /// 将对象转换为时间格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TimeSpan ConvertToTimeSpan(this object obj)
        {
            if (!CheckObjectBefore(obj)) return TimeSpan.Zero;
            return (TimeSpan)obj;
        }
        /// <summary>
        /// 将指定对象的值转换为双精度浮点数。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double ConvertToDouble(this object obj)
        {
            if (!CheckObjectBefore(obj)) return 0;
            return Convert.ToDouble(obj);
        }
        /// <summary>
        /// 将指定对象的值转换为单精度浮点数。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static float ConvertToFloat(this object obj)
        {
            if (!CheckObjectBefore(obj)) return 0;
            return Convert.ToSingle(obj);
        }
        /// <summary>
        /// 将指定对象的值转换为等效的十进制数。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static decimal ConvertToDecimal(this object obj)
        {
            if (!CheckObjectBefore(obj)) return 0;
            return Convert.ToDecimal(obj);
        }
        /// <summary>
        /// 获取对象的类型名称
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string TypeName(this object obj)
        {
            if (obj == null)
                return "Object is NULL";
            return obj.GetType().Name;
        }
        /// <summary>
        /// 返回对象类型全名
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string TypeFullName(this object obj)
        {
            if (obj == null)
                return "Object is NULL";
            return obj.GetType().FullName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ToJson(this object o)
        {
            if (o == null) return "";
            return SakerCore.Serialization.Json.JsonHelper.ToZipJson(o);
        }
         
    }
}
