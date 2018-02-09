using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

namespace SakerCore.Extension
{
    /// <summary>
    /// 集合类型扩展
    /// </summary>
    public static class _IEnumerable
    {
        /// <summary>
        /// 返回集合中第一个匹配查询的索引，没有匹配返回 -1
        /// </summary>
        public static int IndexOfWhereFirist<T>(this IEnumerable<T> ienum, Func<T, bool> func)
        {
            int index = 0;

            foreach (var r in ienum)
            {
                if (func(r))
                {
                    return index;
                }
                index++;
                continue;
            }
            return -1;
        }
        /// <summary>
        /// 返回集合中匹配查询的所有索引的数组，没有匹配返回空数组
        /// </summary>
        public static int[] IndexOfWhereAll<T>(this IEnumerable<T> ienum, Func<T, bool> func)
        {
            int index = 0;
            List<int> index_list = new List<int>();

            foreach (var r in ienum)
            {
                if (func(r))
                {
                    index_list.Add(index);
                }
                index++;
                continue;
            }
            return index_list.ToArray();
        }
        /// <summary>
        /// 返回指定元素的指定字段是否存在指定的集合字典中的一个集合
        /// </summary>
        public static IEnumerable<T> ExistIn<T, OutResult>(this IEnumerable<T> ele, Func<T, OutResult> fun, IEnumerable<OutResult> refele)
        {
            return ele.Where(p =>
            {
                return refele.Contains(fun(p));
            });
        } 
        /// <summary>
        /// 在所有迭代元素上调用指定方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ele"></param>
        /// <param name="Fuc"></param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> ele, Action<T> Fuc)
        {
            if (ele == null) return ele;
            foreach (var r in ele)
            {
                Fuc(r);
            }
            return ele;
        }
        /// <summary>
        /// 将传入的集合进行分段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ele">元素集合</param>
        /// <param name="count">每段最大数量</param>
        /// <returns></returns>
        public static IEnumerable<T[]> Subsection<T>(this IEnumerable<T> ele, int count)
        {

            if (count <= 0) throw new System.Exception("数组的分段每段数量必须大于零");
            var srcArray = ele.ToArray();
            int length = srcArray.Length;
            //计算数据将被分多少段
            var duanshu = (int)Math.Ceiling((float)length / count);
            var outarray = new T[duanshu][];

            int index = 0;
            int di = 0;
            while (index < length)
            {
                var sucount = length - index > count ? count : length - index;
                outarray[di] = new T[sucount];
                Array.Copy(srcArray, index, outarray[di], 0, sucount);
                index += sucount;
                di++;
            }
            return outarray;
        }


    }
}
