/***************************************************************************
 * 
 * 创建时间：   2016/6/24 12:05:43
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   文件操作帮助类
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SakerCore.Extension;

namespace SakerCore.IO
{
    /// <summary>
    /// 文件操作帮助类
    /// </summary>
    public static partial  class FileHelper
    {
        static readonly Encoding defaultEncoding = Encoding.UTF8;
        static readonly byte[] EmprtBytes = new byte[0];

        public static void CheckAndCreateDir(string path)
        {
            path = Path.GetFullPath(path);
            //检查路径中的文件夹是否存在，如果不存在则创建
            var dir = Path.GetDirectoryName(path);
            //检查文件夹是否存在，如果不存在则创建文件夹
            if (!Directory.Exists(dir))
            {
                //文件夹不存在则创建文件夹
                Directory.CreateDirectory(dir);
            }
        }
        private static bool TryCheckExistsFile(string path)
        {
            return File.Exists(path);
        }
        private static bool CheckExistsFile(string path)
        {
            return File.Exists(path);
        }

        static string _programBaseDir = null;
        static string _processName = null;
        /// <summary>
        /// 进程基础文件夹路径
        /// </summary>
        public static string ProcessBaseDir
        {
            get
            {
                return _programBaseDir ?? (_programBaseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
            }
        }
        /// <summary>
        /// 当前程序的进程名称
        /// </summary>
        public static string ProcessName
        {
            get
            {
                return _processName ?? (_processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            }
        }



        /// <summary>
        /// 获取当前目录中与指定搜索模式匹配并使用某个值确定是否在子目录中搜索的目录的数组。
        /// </summary> 
        public static string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!System.IO.Directory.Exists(path)) return new string[0];
            return Directory.GetDirectories(path, searchPattern, searchOption);
        }
        /// <summary>
        ///  返回指定目录中文件的名称，该目录与指定搜索模式匹配并使用某个值确定是否在子目录中搜索。
        /// </summary> 
        public static string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!System.IO.Directory.Exists(path)) return new string[0];
            return Directory.GetFiles(path, searchPattern, searchOption);
        }



        #region 扩展辅助文件操作方法


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void Copy(string source, string target)
        {
            if (!TryCheckExistsFile(source)) return;
            var dir = System.IO.Path.GetDirectoryName(target);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            System.IO.File.Copy(source, target, true);
        }



        /// <summary>
        /// 创建一个新文件，在其中写入指定的字符串，然后关闭文件。如果目标文件已存在，则覆盖该文件。
        /// </summary>
        /// <param name="path">写入文件的文件路径</param>
        /// <param name="context">写入的文件内容</param>
        /// <param name="encoding"></param>
        public static void WriteAllText(string path, string context, Encoding encoding) => WriteAllBytes(path, encoding.GetBytes(context));

        /// <summary>
        /// 创建一个新文件，在其中写入指定的字符串，然后关闭文件。如果目标文件已存在，则覆盖该文件。
        /// </summary>
        /// <param name="path">写入文件的文件路径</param>
        /// <param name="context">写入的文件内容</param>
        public static void WriteAllText(string path, string context) => WriteAllText(path, context, defaultEncoding);

        /// <summary>
        /// 打开一个文本文件，读取文件的所有行，然后关闭该文件。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadAllText(string path) => ReadAllText(path, defaultEncoding);

        /// <summary>
        ///  打开一个文件，使用指定的编码读取文件的所有行，然后关闭该文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadAllText(string path, Encoding encoding) => encoding.GetString(ReadAllBytes(path));

        /// <summary>
        ///  打开一个文件，使用指定的编码读取文件的所有行，然后关闭该文件。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] ReadAllLines(string path) => ReadAllLines(path, defaultEncoding);

        /// <summary>
        /// 将指定的字符串追加到文件中，如果文件还不存在则创建该文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        public static void AppendAllText(string path, string contents) => AppendAllText(path, contents, defaultEncoding);
        /// <summary>
        /// 将指定的字符串追加到文件中，如果文件还不存在则创建该文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        /// <param name="encoding"></param>
        public static void AppendAllText(string path, string contents, Encoding encoding) => AppendAllBytes(path, encoding.GetBytes(contents));


        #endregion

        #region 文件基础读写操作

        /// <summary>
        /// 打开一个文件，将文件的内容读入一个字节数组，然后关闭该文件。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(string path)
        {
            if (!CheckExistsFile(path)) return EmprtBytes;
            using (var fs = Open(path))
            {
                fs.Position = 0;

                using (var ms = new System.IO.MemoryStream())
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }
        /// <summary>
        /// 创建一个新文件，在其中写入指定的字节数组，然后关闭该文件。如果目标文件已存在，则覆盖该文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static void WriteAllBytes(string path, byte[] data)
        {
            CheckAndCreateDir(path);

            using (var fs = OpenWriterOrCreate(path))
            {
                fs.SetLength(0);
                fs.WriteBytes(data);
                fs.Flush();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static void AppendAllBytes(string path, byte[] data)
        {
            CheckAndCreateDir(path);

            using (var fs = OpenWriterOrCreate(path))
            {
                fs.Position = fs.Length;
                fs.WriteBytes(data);
                fs.Flush();
            }

        }
        /// <summary>
        ///  打开一个文件，使用指定的编码读取文件的所有行，然后关闭该文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private static string[] ReadAllLines(string path, Encoding encoding)
        {
            List<string> f = new List<string>();
            if (!CheckExistsFile(path)) return new string[0];
            using (var fs = Open(path))
            {
                using (var fr = fs.GetStreamReader(encoding))
                {
                    string l;
                    while ((l = fr.ReadLine()) != null)
                    {
                        f.Add(l);
                    }
                    return f.ToArray();
                }
            }
        }

        #endregion

        #region 创建文件的基础流操作 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Stream OpenWriterOrCreate(string file)
        {
            FileInfo fInfo = new FileInfo(file);
            if (!fInfo.Directory.Exists)
            {
                fInfo.Directory.Create();
            }
            return new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Stream AppendOrCreate(string file)
        {
            FileInfo fInfo = new FileInfo(file);
            if (!fInfo.Directory.Exists)
            {
                fInfo.Directory.Create();
            }
            return new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Stream Open(string file)
        {
            return new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Stream Create(string file)
        {
            FileInfo fInfo = new FileInfo(file);
            if (!fInfo.Directory.Exists)
            {
                fInfo.Directory.Create();
            }
            return new FileStream(file, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        }


        #endregion



        /// <summary>
        /// 获取指定文件相对于基础路径的相对路径
        /// </summary>
        /// <param name="basedir">需要计算的基础路径</param>
        /// <param name="filePath">需要计算的文件路径</param>
        /// <returns></returns>
        public static string GetRelativePath(string basedir, string filePath)
        {
            if (string.IsNullOrEmpty(basedir))
            {
                return filePath;
            }
            //判断当前的目录是否是共享目录
            bool isunc = filePath.StartsWith("\\\\");

            //获取文件的绝对路径
            basedir = Path.GetFullPath(basedir);
            if (!Directory.Exists(basedir))
            {
                basedir = Path.GetDirectoryName(basedir);
            }
            basedir = basedir.TrimEnd('\\');
            basedir = basedir.TrimStart('\\');
            filePath = Path.GetFullPath(filePath);
            filePath = filePath.TrimEnd('\\');
            filePath = filePath.TrimStart('\\');
            //找到两个文件共用的同用文件夹根目录
            var basedirPathArray = basedir.Split('\\');
            var filePathPathArray = filePath.Split('\\');

            int s = basedirPathArray.Length > filePathPathArray.Length ? filePathPathArray.Length : basedirPathArray.Length;
            int index = 0;
            for (; index < s; index++)
            {
                if (string.Equals(basedirPathArray[index], filePathPathArray[index], StringComparison.OrdinalIgnoreCase)) continue;
                else
                    break;
            }
            StringBuilder strb = new StringBuilder();
            //由 basedirPathArray 计算 ../
            if (index > 0)
            {
                //判断索引是否大于零，表示当前的目录是否包含同级目录。寻找相对路径
                for (int i = index; i < basedirPathArray.Length; i++)
                {
                    strb.Append("..\\");
                }
            }
            else if (isunc)
            {
                strb.Append("\\\\");
            }
            for (int i = index; i < filePathPathArray.Length; i++)
            {
                strb.Append($"{filePathPathArray[i]}\\");
            }
            var result = strb.ToString().TrimEnd('\\');
            //返回计算结果
            return result;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="call"></param>
        public static void WriteAllBytesAsync(string path, byte[] data, Action<System.Exception> call)
        {
            try
            {
                CheckAndCreateDir(path);
                //异步写入文件
                var filestream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                filestream.SetLength(0);
                filestream.BeginWrite(data, 0, data.Length, (iar) =>
                {
                    if (!iar.IsCompleted) return;
                    System.Exception run_ex = null;
                    try
                    {
                        filestream.EndWrite(iar);
                        filestream.Flush();
                    }
                    catch (System.Exception ex)
                    {
                        run_ex = ex;
                    }
                    finally
                    {
                        filestream?.Dispose();
                    }
                    try
                    {
                        call(run_ex);
                    }
                    catch (System.Exception ex)
                    {
                        SystemErrorProvide.OnSystemErrorHandleEvent(null, ex);
                    }
                }, null);
            }
            catch (System.Exception ex)
            {
                call(ex);
            }
        }
        /// <summary>
        /// 读取一个文件并将其缓存到内存流中
        /// </summary>
        /// <param name="path">需要读取的文件路径</param>
        /// <returns>返回读取到的文件流，如果读取失败则返回 null 值</returns>
        public static MemoryStream ReadFileToMemoryStream(string path)
        {
            if (!CheckExistsFile(path)) return null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var fs = Open(path))
                    {
                        fs.CopyTo(ms);
                    }
                    return ms;
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 开启一个异步操作来读取一个文件到内存流中
        /// </summary>
        /// <param name="path">需要读取的文件</param>
        /// <param name="call">读取失败的回调方法</param>
        public static void ReadFileToMemoryStreamAsync(string path, Action<MemoryStream> call)
        {
            if (call == null) return;

            SakerCore.Threading.ThreadPoolProviderManager.QueueUserWorkItem(() =>
            {
                call(ReadFileToMemoryStream(path));
            });

        }


        ///// <summary>
        ///// 将文件删除进回收站而不直接删除
        ///// </summary>
        ///// <param name="fullName"></param>
        //public static bool DeleteFileToRecycle(string fullName)
        //{
        //    try
        //    {
        //        if (CheckFileIsOpen(fullName)) return false;
        //        //为何不始用File.Delete()，是因为该方法不经过回收站，直接删除文件
        //        //要删除至回收站，可使用VisualBasic删除文件，需引用Microsoft.VisualBasic
        //        //删除确认对话框是根据电脑系统-回收站-显示删除确认对话框   是否打勾 自动添加的
        //        //为何不使用c#的File.Delete()方法？？？因为该方法是直接删除，而不是放入回收站
        //        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(fullName,
        //        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
        //        Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
        //        Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);

        //        return true;
        //    }
        //    catch //(Exception ex)
        //    {
        //        return false;
        //    }

        //}


        /************************/

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool CheckFileIsOpen(string fullName)
        {

            try
            {
                if (!File.Exists(fullName)) return false;

                using (var fs = System.IO.File.OpenWrite(fullName))
                {
                }
                return false;
            }
            catch //(Exception ex)
            {
                return true;
            }

        }


    }

}