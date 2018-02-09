/***************************************************************************
 * 
 * 创建时间：   2017/3/4 12:13:21
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   为运行时提供相关功能的杂项方法
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace SakerCore
{
    /// <summary>
    /// 为运行时提供相关功能的杂项方法
    /// </summary>
    public class ToolsMethods
    {
        const int inta_A = 'a' - 'A';
        const int intA = 'A';
        const int intZ = 'Z';


        static ToolsMethods()
        {
        }

        /// <summary>
        /// 从指定的指针位置开始复制一系列字节到指定的缓冲区
        /// </summary>
        /// <param name="src">源数据的指针</param>
        /// <param name="buffer">接收拷贝数据的缓冲区</param>
        /// <param name="startIndex">数据的开始位置</param>
        /// <param name="count">拷贝的数量</param>
        public unsafe static void MemoryCopy(void* src, byte[] buffer, int startIndex, int count)
        {
            System.Runtime.InteropServices.Marshal.Copy(new IntPtr(src), buffer, startIndex, count);
        }

        /// <summary>
        /// 命令行参数解析器
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string[] SplitCmd(string cmd)
        {
            var reader = new StringReader(cmd);

            int ch = '\0';

            List<string> data = new List<string>();
            int index = 0;
            int pos = 0;
            int count = 0;

            bool isY = false;

            while (true)
            {
                index++;
                ch = reader.Read();
                if (ch == -1) break;
                if (ch == ' ')
                {
                    if (isY) continue;
                    if (doSubstrProcess(data, cmd, ref pos, ref count, ref index)) break;
                    continue;
                }
                if (ch == '"' && isY)
                {
                    isY = false;
                    if (doSubstrProcess(data, cmd, ref pos, ref count, ref index)) break;
                    continue;
                }
                if (ch == '"')
                {
                    pos = index;
                    isY = true;
                    continue;
                }
                count++;
            }
            doSubstrProcess(data, cmd, ref pos, ref count, ref index);
            return data.ToArray();
        }

        internal static string FastAllocateString(int strLen)
        {
            return new string(new char[strLen]);
        }

        private static bool doSubstrProcess(List<string> data, string con, ref int pos, ref int count, ref int index)
        {
            if (count == 0)
            {
                pos = index;
                return false;
            }

            data.Add(con.Substring(pos, count));
            pos = index;
            count = 0;
            return false;
        }




        /// <summary>
        /// 创建指定类型的默认构造器方法，Emit操作
        /// </summary>
        /// <typeparam name="T">委托类型</typeparam>
        /// <param name="type">需要创建的目标</param>
        /// <param name="currentType">引用目标类型</param>
        /// <returns></returns>
        public static T GeneratNewobjMethod<T>(Type type, Type currentType)
        {
            var defaultCtor = type.GetConstructor(Type.EmptyTypes);
            if (defaultCtor == null)
                throw new System.Exception($"类型【{type.FullName}】不包含默认的构造方法 new()");

            var method = new DynamicMethod("GetNewObject"
                , type
                , null
                , currentType
                , true);



            //获取il编辑器
            var il = method.GetILGenerator();

            //变量声明
            var ret = il.DeclareLocal(type);

            il.Emit(OpCodes.Newobj, defaultCtor);
            il.Emit(OpCodes.Stloc, ret);
            il.Emit(OpCodes.Ldloc, ret);
            il.Emit(OpCodes.Ret);
            object o = method.CreateDelegate(typeof(T));
            return (T)o;

        }






        /// <summary>
        /// 比较输入字符串与模式的是否匹配，将使用指针根据每个字符进行比较
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="pattern">模式，允许使用的通配符：*。* 代表零或多个任意字符</param>
        /// <param name="matchs">匹配数据的结果</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static unsafe bool WildMatchByPtr(string input, string pattern, out string[] matchs)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pattern))
            {
                matchs = null;
                return false;
            }
            matchs = null;
            List<string> l = new List<string>();
            StringBuilder b = new StringBuilder();

            bool matched = false;
            fixed (char* p_wild = pattern)
            fixed (char* p_str = input)
            {
                char* wild = p_wild;    //通配符字符串寻址
                char* str = p_str;      //输入字符串寻址
                char* cp = null;
                char* mp = null;

                while ((*str) != 0 && (*wild != '*'))
                {
                    if ((*wild != *str))
                    {
                        return matched;
                    }
                    wild++;
                    str++;
                }

                while (*str != 0)
                {
                    if (*wild == '*')
                    {
                        InitMath(b, l);
                        if (0 == (*++wild))
                        {
                            b.Append(new string(str));
                            l.Add(b.ToString());
                            matchs = l.ToArray();
                            //如果*后面没有其它模式符，则判定匹配
                            matched = true;
                            return matched;
                        }
                        mp = wild;
                        cp = str;
                    }
                    else if ((*wild == *str))
                    {
                        wild++;
                        str++;
                    }
                    else
                    {
                        //模式串未到结尾，而输入字串已经走到结尾，判定不匹配
                        wild = mp;//冻结，固定在不匹配的模式字符上
                        b.Append(*cp);
                        str = ++cp;
                    }
                }

                //修正模式串
                while (*wild == '*')
                {
                    wild++;
                }
                var result = (*wild) == 0 ? true : false;
                if (result)
                {
                    InitMath(b, l);
                    matchs = l.ToArray();
                }
                return result;
            }
        }
        private static void InitMath(StringBuilder b, List<string> l)
        {
            if (b.Length == 0) return;
            l.Add(b.ToString());
            b.Length = 0;
        }


        /// <summary>
        /// 将字节大小转换为数据可识别字符串形式
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string ToSizeString(long? size)
        {
            if (size == null) return "";

            var t = (double)size.Value;

            if (t < 1024) return $"{t.ToString("0")}";
            if (t < 1024 * 1024)
            {
                t = t / 1024;
                return $"{t.ToString("0.00")}K";
            }
            if (t < 1024 * 1024 * 1024)
            {
                t = t / 1024 / 1024;
                return $"{t.ToString("0.00")}M";
            }
            t = t / 1024 / 1024 / 1024;



            return $"{t.ToString("0.00")}G";

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string CompileCss2Js(string str, string id)
        {
            str = str.Replace(id, "");
            var data = str.Split('}');
            StringBuilder strb = new StringBuilder();
            foreach (var r in data)
            {
                var i = r.IndexOf('{');
                if (i < 0) continue;

                var selectstr = r.Substring(0, i).Trim();
                var cssstr = r.Substring(i + 1)?.Trim();
                if (string.IsNullOrEmpty(cssstr)) continue;


                strb.Append("$(dom).");

                if (string.IsNullOrEmpty(selectstr))
                {
                    strb.Append("css({");
                }
                else
                {
                    strb.Append($@"find('{selectstr}').css({{");
                }
                int insert_count = 0;
                foreach (var csskvitem in cssstr.Split(';'))
                {
                    var csskvitemstr = csskvitem?.Trim();
                    if (string.IsNullOrEmpty(csskvitemstr))
                    {
                        continue;
                    }
                    var csskv = csskvitemstr.Split(':');

                    if (csskv.Length < 2) continue;
                    var csskey = csskv[0]?.Trim();
                    var cssvalue = csskv[1]?.Trim();

                    if (string.IsNullOrEmpty(csskey)) continue;
                    if (string.IsNullOrEmpty(cssvalue)) continue;
                    insert_count++;

                    float s;
                    if (float.TryParse(cssvalue, out s))
                    {
                        strb.Append($@"""{csskey}"":{cssvalue},");
                    }
                    else
                    {
                        strb.Append($@"""{csskey}"":""{cssvalue}"",");
                    }
                }
                if (insert_count > 0)
                {
                    strb.Remove(strb.Length - 1, 1);
                }
                strb.AppendLine("});");
            }
            return strb.ToString();
        }



    }
}