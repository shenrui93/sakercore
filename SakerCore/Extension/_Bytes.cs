using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SakerCore.Extension
{
    /// <summary>
    /// 字节数组扩展类
    /// </summary>
    public static class _Bytes
    {
        static readonly byte[] EmptyArray = { };

        /// <summary>
        /// 反转字节数组元素
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] BytesReverse(this byte[] data)
        {

            if (data == null)
                return EmptyArray;
            if (data.Length <= 0)
                return EmptyArray;
            var newdata = new byte[data.Length];
            Buffer.BlockCopy(data, 0, newdata, 0, data.Length);
            Array.Reverse(newdata);
            return newdata;
        }

        /// <summary>
        /// 连接两个字节数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] BytesConcat(this byte[] data, byte[] array)
        {

            if (data == null) throw new ArgumentNullException();
            if (array == null) throw new ArgumentNullException();
            if (array.Length == 0) return data;
            if (data.Length == 0) return array;
            int length;
            length = array.Length + data.Length;

            var outArray = new byte[length];
            System.Buffer.BlockCopy(data, 0, outArray, 0, data.Length);
            System.Buffer.BlockCopy(array, 0, outArray, data.Length, array.Length);


            return outArray;
        }

        /// <summary>
        /// 在流中写入字节数据
        /// </summary>
        /// <param name="stream">需要写入到的流对象</param>
        /// <param name="data">需要写入的数据</param>
        public static void WriteBytes(this System.IO.Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 将字节数组转换为Base64字符串表示形式
        /// </summary>
        /// <param name="data">需要转换的字节数组</param>
        /// <returns>返回字节数组的 base64 字符串表示形式</returns>
        public static string ToBase64String(this byte[] data)
        {
            return Serialization.Base64Serialzier.ToBase64String(data);
        }
        
        
        
        
        
         
    }
}
