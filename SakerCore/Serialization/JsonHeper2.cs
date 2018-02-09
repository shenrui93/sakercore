/***************************************************************************
 * 
 * 创建时间：   2016/5/25 13:09:42
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SakerCore.Serialization
{

    namespace Json
    {
        /// <summary>
        /// 类JsonHelper的注释信息
        /// </summary>
        public class JsonHelper
        {
            ///// <summary>
            ///// 格式化json的设置信息
            ///// </summary>
            //public static readonly JsonSerializerSettings FormatJsonSetting = new JsonSerializerSettings()
            //{
            //    Formatting = Formatting.Indented, 
            //};



            /// <summary>
            /// 将对象转换为json表示格式
            /// </summary>
            /// <param name="obj">需要转换的对象</param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public static string ToZipJson(object obj)
            {
                throw new NotImplementedException();

                //return JsonConvert.SerializeObject(obj);
            }
            /// <summary>
            /// 将对象转换为json表示格式
            /// </summary>
            /// <param name="obj">需要转换的对象</param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public static string ToFormatJson(object obj)
            { 
                throw new NotImplementedException();
                //var serializer = new JsonSerializer();
                //StringWriter textWriter = new StringWriter();
                //var s = JsonConvert.SerializeObject(obj, FormatJsonSetting);
                //var jsonWriter = new JsonTextWriter(textWriter)
                //{
                //    Formatting = Formatting.Indented,
                //    Indentation = 4,
                //    IndentChar =' '
                //};
                //serializer.Serialize(jsonWriter, obj);

                //return textWriter.ToString();
            }

            /// <summary>
            /// 将对象转换为json表示格式
            /// </summary>
            /// <param name="obj">需要转换的对象</param>
            /// <param name="path">表示文件存储的路径</param>
            /// <returns></returns> 
            public static void ToZipJsonFile(object obj,string path)
            {
                var json = ToZipJson(obj);
                InternalWriterFile(json, path);

             }

            /// <summary>
            /// 将对象转换为json表示格式
            /// </summary>
            /// <param name="obj">需要转换的对象</param>
            /// <param name="path">文件存储的路径</param>
            /// <returns></returns>
            public static void ToFormatJsonFile (object obj,string path)
            {
                var json = ToFormatJson(obj); 
                InternalWriterFile(json, path);
            }
            /// <summary>
            /// 将json格式对象转换为指定 T 类型的对象,如果转换失败，返回 T 类型的默认值
            /// </summary>
            /// <typeparam name="T">需要转换的类型对象</typeparam>
            /// <param name="json">需要转换的json字符串</param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public static T ToObjectFromJson<T>(string json)
            {
                throw new NotImplementedException();
                //try
                //{
                //    return JsonConvert.DeserializeObject<T>(json);
                //}
                //catch
                //{
                //    return default(T);
                //}
            }
            /// <summary>
            /// 将json格式对象转换为指定 T 类型的对象,如果转换失败，返回 T 类型的默认值
            /// </summary>
            /// <typeparam name="T">需要转换的类型对象</typeparam>
            /// <param name="path">需要转换的json信息存储文件的路径</param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public static T ToObjectFromJsonFile<T>(string path)
            {
                throw new NotImplementedException();
                //try
                //{
                //    var json = InternalReadFile(path);
                //    if (string.IsNullOrEmpty(json))
                //        return default(T);
                //    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                //}
                //catch //(Exception ex)
                //{
                //    return default(T);
                //}
            }

            private static string InternalReadFile(string path)
            { 
                if (!File.Exists(path)) return null;

                return File.ReadAllText(path); 
            }
            private static void InternalWriterFile(string json, string path)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(path, json, Encoding.UTF8);
            }


        }
    }
}
