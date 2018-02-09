/***************************************************************************
* 
* 创建时间：   2016/10/10 17:35:39
* 创建人员：   沈瑞
* CLR版本号：  4.0.30319.42000
* 备注信息：   为数据库的操作提供简单操作支持
* 
* *************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace SakerCore.Data
{
    /// <summary>
    /// 为数据库的操作提供简单操作支持
    /// </summary>
    public class DBHelper
    {
        private string connectionString;
        private IDBEntityProvide DBEntityProvide; 
        static DBHelper()
        { 
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="DBEntityProvide"></param>
        public DBHelper(string connectionString, IDBEntityProvide DBEntityProvide)
        {
            this.connectionString = connectionString;
            this.DBEntityProvide = DBEntityProvide;
        }

        /// <summary>
        /// 执行处理sql并返回执行的受影响行数
        /// </summary>
        /// <param name="sql">需要执行的sql文本</param>
        /// <param name="ds">执行后得到的数据表</param>
        /// <returns>返回受影响的行数</returns>
        public int RunSql(string sql, out DataSet ds)
        {
            using (IDbConnection conn = DBEntityProvide.GetNewDbConnection(connectionString))
            {
                IDbDataAdapter dpa = DBEntityProvide.GetNewDbDataAdapter();
                var comm = DBEntityProvide.BulidDBCommand(sql);
                dpa.SelectCommand = comm;
                comm.Connection = conn;
                ds = new DataSet();
                int linecount = dpa.Fill(ds);
                return linecount;
            }
        }
        /// <summary>
        /// 执行存储过程，返回ReturnValue
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public object RunProcedure(string storedProcName, IDataParameter[] parameters, out DataSet dataSet)
        {
            int tryCount = 1;
            while (true)
            {
                IDbDataAdapter sqlDA = DBEntityProvide.GetNewDbDataAdapter();
                try
                {
                    using (var connection = DBEntityProvide.GetNewDbConnection(connectionString))
                    {
                        dataSet = new DataSet();
                        if (connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                        }
                        sqlDA.SelectCommand = DBEntityProvide.BuildInitCommand(connection, storedProcName, parameters);
                        sqlDA.Fill(dataSet);
                        return DBEntityProvide.GetReturnValue(sqlDA.SelectCommand);
                    }
                }
                catch (System.Exception ex)
                {
                    if (--tryCount > 0)
                        continue;

                    var tex = new Exception("调用存储过程“" + storedProcName + "”出现异常！", ex);
                    SystemErrorProvide.OnSystemErrorHandleEvent(null, tex);
                    throw tex;
                }
                finally
                {
                    if (sqlDA.SelectCommand != null && sqlDA.SelectCommand.Parameters.Count >= 0)
                        sqlDA.SelectCommand.Parameters.Clear();
                }
            }
        } 

        /// <summary>
        /// 开启一个异步执行sql查询操作
        /// </summary>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param>
        /// <param name="maxtablecount"></param>
        /// <param name="callback"></param>  
        public void RunProcedureAsync(string storedProcName, IDataParameter[] parameters, int maxtablecount, Action<object, DataSet> callback)
        {
            System.Threading.ThreadPool.UnsafeQueueUserWorkItem(o =>
            {
                try
                {
                    DataSet ds;
                    var ret = RunProcedure(storedProcName, parameters, out ds);
                    callback(ret, ds);

                }
                catch (System.Exception ex)
                {
                    OnRunProcedureAsyncExpection(storedProcName, ex);
                }
            }, null);
        } 
        private void OnRunProcedureAsyncExpection(string storedProcName, System.Exception ex)
        {
            var tex = new System.Exception("调用存储过程“" + storedProcName + "”出现异常！", ex);
            SystemErrorProvide.OnSystemErrorHandleEvent(null, tex);
        } 
    } 
    /// <summary>
    /// 数据实体提供程序，提供基础的数据库操作
    /// </summary>
    public interface IDBEntityProvide
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="comm"></param>
        /// <param name="cb"></param>
        /// <param name="userObject"></param>
        /// <returns></returns>
        IAsyncResult BeginExecuteReader(IDbCommand comm, AsyncCallback cb, object userObject);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iar"></param>
        /// <param name="comm"></param>
        /// <returns></returns>
        IDataReader EndExecuteReader(IDbCommand comm, IAsyncResult iar);

        /// <summary>
        /// 初始化执行存储过程的存储过程命令行，返回comm
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="storedProcName"></param>
        /// <param name="parameters"></param> 
        /// <returns></returns>
        IDbCommand BuildInitCommand(IDbConnection connection, string storedProcName, IDataParameter[] parameters);
        /// <summary>
        /// 根据sql语句初始化数据库的查询命令对象
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        IDbCommand BulidDBCommand(string sql);
        /// <summary>
        /// 获取一个新的数据库连接对象
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        IDbConnection GetNewDbConnection(string connectionString);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IDbDataAdapter GetNewDbDataAdapter();
        /// <summary>
        /// 提供数据的获取
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        object GetParameterValue(object param);
        /// <summary>
        /// 获取本次查询的返回值
        /// </summary>
        /// <param name="selectCommand">查询信息对象</param>
        /// <returns></returns>
        object GetReturnValue(IDbCommand selectCommand);
    } 
}
