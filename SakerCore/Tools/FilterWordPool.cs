/***************************************************************************
 * 
 * 创建时间：   2016/12/29 9:31:06
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   非法关键词过滤(自动忽略汉字数字字母间的其他字符)
 * 
 * *************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SakerCore.Tools
{

    /// <summary>
    /// 非法关键词过滤(自动忽略汉字数字字母间的其他字符)
    /// </summary>
    class FilterWord : IDisposable
    {
        ~FilterWord()
        {
            Dispose();
        }


        public FilterWord() { }
        public FilterWord(string dictionaryPath)
        {
            this.dictionaryPath = dictionaryPath;
            LoadDictionary();
        }

        private string dictionaryPath = string.Empty;
        /// <summary>
        /// 词库路径
        /// </summary>
        public string DictionaryPath
        {
            get { return dictionaryPath; }
            set { dictionaryPath = value; }
        }
        /// <summary>
        /// 内存词典
        /// </summary>
        private WordGroup[] MEMORYLEXICON = new WordGroup[(int)char.MaxValue];

        private string sourctText = string.Empty;
        /// <summary>
        /// 检测源游标
        /// </summary>
        int cursor = 0;
        /// <summary>
        /// 匹配成功后偏移量
        /// </summary>
        int wordlenght = 0;
        /// <summary>
        /// 检测词游标
        /// </summary>
        int nextCursor = 0;
        List<string> illegalWords = new List<string>();

        /// <summary>
        /// 检测到的非法词集
        /// </summary>
        public List<string> IllegalWords
        {
            get { return illegalWords; }
        }
        /// <summary>
        /// 检测源
        /// </summary>
        public string SourctText
        {
            get { return sourctText; }
            set { sourctText = value; }
        }

        /// <summary>
        /// 判断是否是中文
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private bool isCHS(char character)
        {
            //  中文表意字符的范围 4E00-9FA5
            int charVal = (int)character;
            return (charVal >= 0x4e00 && charVal <= 0x9fa5);
        }
        /// <summary>
        /// 判断是否是数字
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private bool isNum(char character)
        {
            int charVal = (int)character;
            return (charVal >= 48 && charVal <= 57);
        }
        /// <summary>
        /// 判断是否是字母
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private bool isAlphabet(char character)
        {
            return false;
            //int charVal = (int)character;
            //return ((charVal >= 97 && charVal <= 122) || (charVal >= 65 && charVal <= 90));
        }
        /// <summary>
        /// 转半角小写的函数(DBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        ///<remarks>
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>
        private string ToDBC(string input)
        {
            unsafe
            {
                var len = input.Length;
                if (len == 0) return string.Empty;
                string result = SakerCore.ToolsMethods.FastAllocateString(len);
                fixed (char* c = input)
                fixed (char* r = result)
                {
                    for (int i = 0; i < len; i++)
                    {
                        if (c[i] == 12288)
                        {
                            r[i] = (char)32;
                            continue;
                        }
                        if (c[i] > 65280 && c[i] < 65375)
                            r[i] = (char)(c[i] - 65248);
                        else
                            r[i] = c[i];
                    }
                }
                return result.ToLower();
            }
        }


        #region 加载敏感词库

        /// <summary>
        /// 加载内存词库
        /// </summary>
        private void LoadDictionary()
        {
            if (DictionaryPath != string.Empty)
            {
                //一个字词集合列表
                List<string> wordList = new List<string>();
                //清理内存缓存字词组
                Array.Clear(MEMORYLEXICON, 0, MEMORYLEXICON.Length);
                //加载词库文件，一行一个
                string[] words = GetDictionaryContext();
                foreach (string word in words)
                {
                    string key = this.ToDBC(word);
                    wordList.Add(key);
                    //将简体字转换为繁体字词
                    //wordList.Add(Microsoft.VisualBasic.Strings.StrConv(key, Microsoft.VisualBasic.VbStrConv.TraditionalChinese, 0));
                }
                Comparison<string> cmp = delegate (string key1, string key2)
                { 
                    return key2.CompareTo(key1);
                };
                //排序后去重
                wordList.Sort(cmp);
                for (int i = wordList.Count - 1; i > 0; i--)
                {
                    if (wordList[i].ToString() == wordList[i - 1].ToString())
                    {
                        wordList.RemoveAt(i);
                    }
                }
                foreach (var word in wordList)
                {
                    WordGroup group = MEMORYLEXICON[(int)word[0]];
                    if (group == null)
                    {
                        group = new WordGroup();
                        MEMORYLEXICON[(int)word[0]] = group;

                    }
                    group.Add(word.Substring(1));
                }
            }
        }
        private string[] GetDictionaryContext()
        {
            string dictionaryPath = DictionaryPath;
            //如果文件不存在，返回指定的
            if (!System.IO.File.Exists(dictionaryPath))
            {
                createFile(dictionaryPath);
                return new string[0];
            }

            //文件存在，加载敏感词控制的配置文件 
            var temp = File.ReadAllLines(dictionaryPath);
            List<string> l = new List<string>();
            foreach (var r in temp)
            {
                var t = r.Trim();
                if (string.IsNullOrEmpty(t)) continue;
                if (t.StartsWith("#")) continue;

                t = t.Trim().ToLower();
                if (string.IsNullOrEmpty(t)) continue;

                l.Add(t);

            }

            return l.ToArray();

        }
        private readonly Encoding baseEncoder = System.Text.Encoding.UTF8;
        private void createFile(string dictionaryPath)
        {
            string context = $@"
#创建时间：{DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss")}
#
#备注说明：这是平台使用敏感词控制的配置文件，所有限制的词汇均在这里配置
#          系统会逐个检测每个敏感词在输入词汇中是否出现，并返回结果
#          当前可以使用 # 输入注释

";
            System.IO.File.Delete(dictionaryPath);
            System.IO.File.WriteAllText(dictionaryPath, context, baseEncoder);

        }



        #endregion


        /// <summary>
        /// 检测
        /// </summary>
        /// <param name="blackWord"></param>
        /// <returns></returns>
        private bool Check(string blackWord)
        {
            wordlenght = 0;
            //检测源下一位游标
            nextCursor = cursor + 1;
            bool found = false;
            //遍历词的每一位做匹配
            for (int i = 0; i < blackWord.Length; i++)
            {
                //特殊字符偏移游标
                int offset = 0;
                if (nextCursor >= sourctText.Length)
                {
                    break;
                }
                else
                {
                    //检测下位字符如果不是汉字 数字 字符 偏移量加1
                    for (int y = nextCursor; y < sourctText.Length; y++)
                    {

                        if (!isCHS(sourctText[y]) && !isNum(sourctText[y]) && !isAlphabet(sourctText[y]))
                        {
                            offset++;
                            //避让特殊字符，下位游标如果>=字符串长度 跳出
                            if (nextCursor + offset >= sourctText.Length) break;
                            wordlenght++;

                        }
                        else break;
                    }

                    if ((int)blackWord[i] == (int)sourctText[nextCursor + offset])
                    {
                        found = true;
                    }
                    else
                    {
                        found = false;
                        break;
                    }


                }
                nextCursor = nextCursor + 1 + offset;
                wordlenght++;


            }
            return found;
        }
        /// <summary>
        /// 查找并替换
        /// </summary>
        /// <param name="replaceChar"></param>
        public string Filter(char replaceChar)
        {
            if (sourctText != string.Empty)
            {
                char[] tempString = sourctText.ToCharArray();

                var dbcarray = ToDBC(SourctText);
                int index;
                for (int i = 0; i < SourctText.Length; i++)
                {
                    if (dbcarray.Length <= i) continue;
                    index = (int)dbcarray[i];
                    WordGroup group = MEMORYLEXICON[index];
                    if (group != null)
                    {
                        for (int z = 0; z < group.Count; z++)
                        {
                            string word = group.GetWord(z);
                            if (word.Length == 0 || Check(word))
                            {
                                string blackword = string.Empty;
                                for (int pos = 0; pos < wordlenght + 1; pos++)
                                {
                                    blackword += tempString[pos + cursor].ToString();
                                    tempString[pos + cursor] = replaceChar;

                                }
                                illegalWords.Add(blackword);
                                cursor = cursor + wordlenght;
                                i = i + wordlenght;

                            }
                        }
                    }
                    cursor++;
                }
                return new string(tempString);
            }
            else
            {
                return string.Empty;
            }

        }
        /// <summary>
        /// 指示当前对象是不是对象池管理对象
        /// </summary>
        public bool IsPoolObject { get; internal set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public FilterWordPool PoolManager { get; internal set; }

        /// <summary>
        /// 对象释放
        /// </summary>
        public void Dispose()
        {
            if (System.Threading.Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            if (IsPoolObject)
            {
                PoolManager?.Push(this);
                GC.ReRegisterForFinalize(this);
            }
        }

        int _isDisposed = 0;

        #region WordGroup


        /// <summary>
        /// 具有相同首字符的词组集合
        /// </summary>
        class WordGroup
        {
            /// <summary>
            /// 集合
            /// </summary>
            private List<string> groupList;

            public WordGroup()
            {
                groupList = new List<string>();
            }

            /// <summary>
            /// 添加词
            /// </summary>
            /// <param name="word"></param>
            public void Add(string word)
            {
                groupList.Add(word);
            }

            /// <summary>
            /// 获取总数
            /// </summary>
            /// <returns></returns>
            public int Count
            {
                get
                {
                    return groupList.Count;
                }
            }

            /// <summary>
            /// 根据下标获取词
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string GetWord(int index)
            {
                return groupList[index];
            }
        }


        #endregion

        internal void Reset()
        {
            sourctText = string.Empty;
            cursor = 0;
            wordlenght = 0;
            nextCursor = 0;
            illegalWords.Clear();
            _isDisposed = 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FilterWordPool : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        ~FilterWordPool()
        {
            Dispose();
        }

        static SakerCore.Tools.MessageQueueMultiple<AsyncFilterWordTask> _task = new MessageQueueMultiple<AsyncFilterWordTask>();
        static FilterWordPool()
        {
            _task.MessageComing = _task_MessageComing;
        }
        private static void _task_MessageComing(object sender, MessageEventArgs<AsyncFilterWordTask> e)
        {
            var task = e.Message;
            if (task == null) return;
            task.ProcessTask();
        }


        /// <summary>
        /// 初始化操作
        /// </summary>
        /// <param name="path"></param>
        public FilterWordPool(string path)
        {
            this.path = path;
        }
        Semaphore root = new Semaphore(100, 100);
        ConcurrentQueue<FilterWord> _query = new ConcurrentQueue<FilterWord>();
        private readonly string path;

        /// <summary>
        /// 词典文件路径
        /// </summary>
        public string Path
        {
            get
            {
                return path;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourctText"></param>
        /// <param name="replaceChar"></param>
        /// <returns></returns>
        public string Filter(string sourctText, char replaceChar)
        {
            root.WaitOne();
            try
            {
                FilterWord f;
                _query.TryDequeue(out f);
                if (f == null)
                {
                    f = new FilterWord(Path);
                    f.IsPoolObject = true;
                    f.PoolManager = this;
                }

                f.Reset();
                f.SourctText = sourctText;

                var result = f.Filter(replaceChar);
                f.Dispose();
                return result;
            }
            catch (System.Exception ex)
            {
                SakerCore.SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
                return "";
            }
            finally
            {
                root.Release();
            }
        }
        /// <summary>
        /// 执行一个异步的敏感词过滤操作
        /// </summary>
        /// <param name="sourctText"></param>
        /// <param name="replaceChar"></param>
        /// <param name="callback"></param>
        public void FilterAsync(string sourctText, char replaceChar, Action<string> callback)
        {
            _task.Enqueue(new AsyncFilterWordTask()
            {
                host = this,
                sourctText = sourctText,
                replaceChar = replaceChar,
                callback = callback
            });
        }


        long _isDispose = 0;
        /// <summary>
        /// 对象释放
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDispose, 1, 0) != 0) return;
            if (_query != null)
            {
                while (!_query.IsEmpty)
                {
                    FilterWord f;
                    if (!_query.TryDequeue(out f)) break;
                    if (f == null) continue;
                    f.IsPoolObject = false;
                    f.PoolManager = null;
                    f.Dispose();
                }
            }
            _query = null;
        }
        internal void Push(FilterWord filterWord)
        {
            if (Interlocked.Read(ref _isDispose) != 0)
            {
                FreeObject(filterWord);
                return;
            }
            var q = _query;
            q?.Enqueue(filterWord);
            if (Interlocked.Read(ref _isDispose) != 0 && q != null)
            {
                while (!q.IsEmpty)
                {
                    FilterWord f;
                    if (!q.TryDequeue(out f)) break;
                    FreeObject(f);
                }
            }
        }
        private void FreeObject(FilterWord filterWord)
        {
            if (filterWord == null) return;
            filterWord.IsPoolObject = false;
            filterWord.PoolManager = null;
            filterWord.Dispose();
        }

        private class AsyncFilterWordTask
        {
            internal Action<string> callback;
            internal FilterWordPool host;
            internal char replaceChar;
            internal string sourctText;

            internal void ProcessTask()
            {
                try
                {
                    if (host == null) return;
                    if (callback == null) return;
                    if (string.IsNullOrEmpty(sourctText)) callback("");
                    var result = host.Filter(sourctText, replaceChar);
                    callback(result);
                }
                catch (System.Exception ex)
                {
                    SakerCore.SystemErrorProvide.OnSystemErrorHandleEvent(this, ex);
                }
            }
        }
    }

}
