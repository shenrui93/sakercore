/***************************************************************************
 * 
 * 创建时间：   2017/6/22 15:12:04
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供System.Data.DataTable类型的方法扩展
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace SakerCore.Extension
{
    /// <summary>
    /// 提供System.Data.DataTable类型的方法扩展
    /// </summary>
    public static class _DataTable
    {
        static readonly object[][] EmptyObjectArray = new object[][] { };
        static readonly object[] EmptyOneObjectArray = new object[] { };
        static readonly IDictionary<string, object>[] EmptyDictionaryArray = { };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="tb"></param>
        /// <returns></returns>
        public static object[][] GetDataTableArray(this System.Data.DataTable tb, long ts = 1000)
        {
            if (tb == null || tb.Rows.Count <= 0)
            {
                return EmptyObjectArray;
            }
            object[][] data = new object[tb.Rows.Count][];
            var rows = tb.Rows;
            for (int i = 0; i < tb.Rows.Count; i++)
            {
                data[i] = FormatData(rows[i].ItemArray,ts);
            }

            return data;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tb"></param>
        /// <returns></returns>
        public static object[] GetTopOneDataTableArray(this System.Data.DataTable tb, long ts = 1000)
        {
            if (tb == null || tb.Rows.Count <= 0)
            {
                return EmptyOneObjectArray;
            }
            return FormatData(tb.Rows[0].ItemArray,ts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static IDictionary<string, object>[] GetDataTableArrayHasColum(this System.Data.DataTable tb, long ts = 1000)
        {
            if (tb.Rows.Count <= 0) return EmptyDictionaryArray;

            var colums = tb.Columns.Cast<System.Data.DataColumn>().ToArray();

            var array = new aa[tb.Rows.Count];
            int index = 0;

            foreach (System.Data.DataRow dr in tb.Rows)
            {
                var a = new aa();
                colums.ForEach(p => a[p.ColumnName] = FormatData(dr[p.ColumnName],ts));
                array[index++] = a;
            }
            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static object GetTopOneDataTableObjectHasColum(this System.Data.DataTable tb, long ts = 1000)
        {
            if (tb.Rows.Count <= 0) return EmptyDictionaryArray;

            var colums = tb.Columns.Cast<System.Data.DataColumn>().ToArray();

            var array = new aa();

            if (tb.Rows.Count <= 0) return null;

            System.Data.DataRow dr = tb.Rows[0];
            {
                colums.ForEach(p => array[p.ColumnName] = FormatData(dr[p.ColumnName],ts));
            }
            return array;
        }


        class aa : Dictionary<string, object>, IDictionary<string, object>
        {
            public new object this[string index]
            {
                get
                {
                    object val;
                    base.TryGetValue(index, out val);
                    return val;
                }
                set
                {
                    base[index] = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object GetData(this System.Data.DataRow dr, string name)
        {
            if (!dr.Table.Columns.Contains(name))
                return DBNull.Value;
            return dr[name];
        }

        static object[] FormatData(object[] data, long ts)
        {
            if (data == null || data.Length <= 0) return EmptyObjectArray;


            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] is DateTime)
                {
                    data[i] = FormatData(data[i], ts);
                }
            }

            return data;

        }
        static object FormatData(object data,long ts)
        {
            if (data == null) return null;

            if (data is DateTime)
            {
                return ((DateTime)data).GetTimestamp() * ts;
            }

            return data;

        }
    }
}
