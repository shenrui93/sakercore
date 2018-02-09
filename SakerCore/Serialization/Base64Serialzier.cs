/***************************************************************************
 * 
 * 创建时间：   2017/3/2 17:03:54
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   提供字节数组到字符串的编解码实现，使用的是base64的编解码实现，但并不符合base64编解码的标准
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Text;

namespace SakerCore.Serialization
{
    /// <summary>
    /// 提供字节数组到字符串的编解码实现，使用的是base64的编解码实现，但并不符合base64编解码的标准
    /// </summary>
    public class Base64Serialzier
    {
        //一个空的字节数组，这个字段是只读的
        private readonly byte[] EmptyBytes = new byte[0];

        const string base64Table = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        const uint intA = 'A';
        const uint inta = 'a';
        const uint int0 = '0';
        const uint intPlus = '-';
        const uint intSlash = '_';
        const uint intAtoZ = ('Z' - 'A');  // = ('z' - 'a')
        const uint int0to9 = ('9' - '0');

        /**********************************************************************/


        #region 编码


        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [SecurityCritical]  // auto-generated
        public static string ToBase64String(byte[] data)
        {
            int datalen = data.Length;

            if (datalen == 0) return string.Empty;

            int strLen = datalen / 3 * 4;
            int lengthmod3 = datalen % 3;
            if (lengthmod3 != 0)
            {
                strLen += lengthmod3 + 1;
            }
            char[] out_chs = new char[strLen];
            UnsafeToBase64String(data, datalen - lengthmod3, lengthmod3, out_chs);
            return new string(out_chs);

        }
        [SecurityCritical]  // auto-generated
        private unsafe static void UnsafeToBase64String(byte[] inData, int calcdatalen, int lengthmod3, char[] outchs)
        {
            int j = 0;
            int i = 0;
            fixed (char* base64 = base64Table)
            fixed (byte* data = inData)
            fixed (char* outChars = outchs)
            {
                for (; i < calcdatalen; i += 3)
                {
                    outChars[j] = base64[(data[i] & 0xfc) >> 2];
                    outChars[j + 1] = base64[((data[i] & 0x03) << 4) | ((data[i + 1] & 0xf0) >> 4)];
                    outChars[j + 2] = base64[((data[i + 1] & 0x0f) << 2) | ((data[i + 2] & 0xc0) >> 6)];
                    outChars[j + 3] = base64[(data[i + 2] & 0x3f)];
                    j += 4;
                }
                i = calcdatalen;

                switch (lengthmod3)
                {
                    case 2: //One character padding needed
                        outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                        outChars[j + 1] = base64[((inData[i] & 0x03) << 4) | ((inData[i + 1] & 0xf0) >> 4)];
                        outChars[j + 2] = base64[(inData[i + 1] & 0x0f) << 2];
                        //outChars[j + 3] = base64[64]; //Pad
                        //j += 4;
                        break;
                    case 1: // Two character padding needed
                        outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                        outChars[j + 1] = base64[(inData[i] & 0x03) << 4];
                        //outChars[j + 2] = base64[64]; //Pad
                        //outChars[j + 3] = base64[64]; //Pad
                        //j += 4;
                        break;
                }
            }
        }

        #endregion

        #region 解码

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public static byte[] FromBase64String(string s)
        {

            if (s == null) throw new NullReferenceException();

            unsafe
            {
                fixed (char* sPtr = s)
                {
                    return FromBase64CharPtr(sPtr, s.Length);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputPtr"></param>
        /// <param name="inputLength"></param>
        /// <returns></returns>
        private static unsafe byte[] FromBase64CharPtr(char* inputPtr, int inputLength)
        {
            int padding = 0;

            int resultLength;
            var in_p = inputLength % 4;
            switch (in_p)
            {
                case 1: throw new FormatException("输入字符串格式不正确");
                case 2: padding = 1; break;
                case 3: padding = 2; break;
                default: padding = 0; break;
            }
            resultLength = (inputLength / 4) * 3 + padding;

            byte[] decodedBytes = new byte[resultLength];

            fixed (byte* decodedBytesPtr = decodedBytes)
                FromBase64_Decode(inputPtr, inputLength, decodedBytesPtr, resultLength, padding);
            //返回序列化完成的字节数组
            return decodedBytes;


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startInputPtr"></param>
        /// <param name="inputLength"></param>
        /// <param name="startDestPtr"></param>
        /// <param name="destLength"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        [SecurityCritical]
        private static unsafe void FromBase64_Decode(char* startInputPtr, int inputLength, byte* startDestPtr, int destLength, int padding)
        {

            //输入输出端的开始指针位置
            char* inputPtr = startInputPtr;
            byte* destPtr = startDestPtr;

            //输入输出端的结束指针位置
            char* endInputPtr = inputPtr + inputLength;
            byte* endDestPtr = destPtr + destLength;

            // 当前的解码值
            uint currCode;
            //当前块代码
            uint currBlockCodes = 0x000000FFu;

            unchecked
            {
                #region 普通解码循环


                while (true)
                {
                    if (inputPtr >= endInputPtr)
                    {
                        //如果解析的编码已经到达结尾则结束解码 跳转到指定位置标记
                        goto _AllInputConsumed;
                    }
                    //获取当前输入端指针的值
                    currCode = (*inputPtr);
                    //输入端指针递增
                    inputPtr++;
                    if (currCode - intA <= intAtoZ)
                        currCode -= intA;               //如果当前的字符落在 A-Z字母之间之间减去字母 A 的 ansii 码值 参照编码时字符位置
                    else if (currCode - inta <= intAtoZ)
                        currCode -= (inta - 26u);       //如果当前的字符落在 a-z 字母之间之间减去字母 a 的 ansii 码值 参照编码时字符位置 再减去26个大写字母的位置
                    else if (currCode - int0 <= int0to9)
                        currCode -= (int0 - 52u);       //如果当前的字符落在 0-9 数字之间之间减去字符 0 的 ansii 码值 参照编码时字符位置 再减去所有字母的位置             
                    else
                    {
                        //两个特殊字符的处理 -_ (连接符和下划线)
                        switch (currCode)
                        {
                            case intPlus:
                                currCode = 62u;     //对应字符编码 '-'
                                break;
                            case intSlash:
                                currCode = 63u;     //对应字符编码 '_'
                                break;
                            //这里要特别注意，由于我们不是标准的Base64字符编码格式，所以遇到其他意外字符都算是错误
                            //这里参照 base64Table 常量的字符位置编码
                            default:
                                throw new FormatException("输入字符串格式不正确！");
                        }
                    }
                    //由于base64字符编码的特性即 3*8 = 4*6 = 24; 解码时按照6位数值解码
                    currBlockCodes = (currBlockCodes << 6) | currCode;

                    //判断当前的计算值是否已经解析够24位解码值，如果够则进行数值转换
                    if ((currBlockCodes & 0x80000000u) != 0u)
                    {
                        if ((int)(endDestPtr - destPtr) < 3)
                            return;      //这层判断其实是多余的，如果判断为真，则表示已经出现异常，且可能会损坏堆栈信息
                        //执行解码逻辑
                        *(destPtr) = (byte)(currBlockCodes >> 16);
                        *(destPtr + 1) = (byte)(currBlockCodes >> 8);
                        *(destPtr + 2) = (byte)(currBlockCodes);
                        destPtr += 3;
                        //解码完成重新复制循环执行下一个解析
                        currBlockCodes = 0x000000FFu;
                    }

                }
                #endregion
            }

            _AllInputConsumed:

            switch (padding)
            {
                case 2:
                    {
                        //剩余两个字节直接解析 
                        *(destPtr) = (byte)(currBlockCodes >> 10);
                        *(destPtr + 1) = (byte)(currBlockCodes >> 2);
                        //destPtr += 2;
                        break;
                    }
                case 1:
                    {
                        //剩余一个字节直接解析
                        *(destPtr) = (byte)(currBlockCodes >> 4);
                        //destPtr += 1;
                        break;
                    }
            }
        }

        #endregion


    }
}
