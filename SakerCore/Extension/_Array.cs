using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SakerCore.Extension
{
    /// <summary>
    /// 数组扩展类
    /// </summary>
    public static class _Array
    {

        /// <summary>
        /// 连接两个对象数组序列
        /// </summary>
        public static T[] ArrayConcat<T>(this T[] arraySrc, T[] array)
        {
            if (arraySrc == null) return array;
            if (array == null) return arraySrc;
            if (array.Length == 0) return arraySrc;
            if (arraySrc.Length == 0) return array;
            int length;
            length = array.Length + arraySrc.Length;

            var outArray = new T[length];

            Array.ConstrainedCopy(arraySrc, 0, outArray, 0, arraySrc.Length);
            Array.ConstrainedCopy(array, 0, outArray, arraySrc.Length, array.Length);

            return outArray;
        }
        /// <summary>
        /// 根据条件删除数组元素
        /// </summary>
        public static T[] ArrayRemoveWhere<T>(this T[] arraySrc, Func<T, bool> func)
        {
            return arraySrc.Where(p => !func(p)).ToArray();
        }
        /// <summary>
        /// 从指定索引位置开始删除一定数量的数组元素
        /// </summary>
        public static T[] ArrayRemove<T>(this T[] arraySrc, int index, int count)
        {
            if (arraySrc.Length < (index + count)) throw new IndexOutOfRangeException();

            var newLength = arraySrc.Length - count;

            T[] newarrary = new T[newLength];


            Array.Copy(arraySrc, newarrary, index);
            Array.Copy(arraySrc, index + count, newarrary, index, newLength - index);

            return newarrary;
        }
        /// <summary>
        /// 数组元素组合函数，将字符串使用指定字符分割组合一条字符串
        /// </summary>
        public static string MapString<T>(this T[] array, char ch)
        {
            StringBuilder str = new StringBuilder();

            array.ForEach(p => { str.Append(ch + p.ToString()); });

            if (str.Length > 0)
                str.Remove(0, 1);
            return str.ToString();
        }
        /// <summary>
        /// 数组元素组合函数，将字符串使用指定字符分割组合一条字符串
        /// </summary>
        public static string MapString<T>(this T[] array, string ch)
        {
            StringBuilder str = new StringBuilder();

            array.ForEach(p => { str.Append(ch + p.ToString()); });

            if (str.Length > 0 && !string.IsNullOrEmpty(ch))
                str.Remove(0, ch.Length);
            return str.ToString();
        }
    }
}
