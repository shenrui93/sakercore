using System;
using System.IO;
using System.Text;

namespace SakerCore.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAppConfigBase
    {
        /// <summary>
        /// 
        /// </summary>
        void Init();
        /// <summary>
        /// 
        /// </summary>
        void Save();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="HostType"></typeparam>
    [Serializable]
    public abstract class AppConfigBase<HostType> : IAppConfigBase
        where HostType : class, IAppConfigBase, new()
    {
        static string configfilename;

        static AppConfigBase()
        {
            InitConfigFileName();
        }

        private static void InitConfigFileName()
        {
            var type = typeof(HostType);

            var atrs = type.GetCustomAttributes(typeof(AppConfigPathInfo), true);
            if (atrs == null || atrs.Length <= 0)
            {
                InitConfigFileNameForProcessName();
                return;
            }

            var config_attr = atrs[0] as AppConfigPathInfo;
            if(config_attr == null)
            {
                InitConfigFileNameForProcessName();
                return;
            }
            if (string.IsNullOrEmpty(config_attr.configName))
            {
                InitConfigFileNameForProcessName();
                return;
            }


            //使用配置的文件名称来配置文件的配置路径文件名称 
            configfilename = config_attr.configName;

        }
        private static void InitConfigFileNameForProcessName()
        {
            configfilename = SakerCore.IO.FileHelper. ProcessName;
        }



        /// <summary>
        /// 
        /// </summary>
        public virtual void Save()
        {
            var json = SakerCore.Serialization.Json.JsonHelper.ToFormatJson(this);
            var path = ConfigPath;
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static HostType Load()
        {
            var path = ConfigPath;
            var s = Serialization.Json.JsonHelper.ToObjectFromJsonFile<HostType>(path);
            return s;
        }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string ConfigPath
        {
            get
            {
                return $"{ SakerCore.IO.FileHelper.ProcessBaseDir}/{configfilename}.apconf";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public AppConfigBase()
        {
            LoadConfig();
        }
        private static HostType sc;
        /// <summary>
        /// 单例模式，获取系统配置实例
        /// </summary>
        /// <returns></returns>
        public static HostType Instance
        {
            get
            {
                if (sc == null)
                {
                    sc = Load();
                    if (sc == null)
                    {
                        sc = new HostType();
                    }
                    sc.Init();
                    sc.Save();
                }
                return sc;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void Init();
        /// <summary>
        /// 
        /// </summary>
        public virtual void LoadConfig()
        {

        }

    }
    /// <summary>
    /// 指示服务器的配置文件名称，默认使用进程名称标识文件名称
    /// </summary>
    public class AppConfigPathInfo : Attribute
    {
        internal string configName;
        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="filename"></param>
        public AppConfigPathInfo(string filename)
        {
            configName = filename;
        }
    }
}
