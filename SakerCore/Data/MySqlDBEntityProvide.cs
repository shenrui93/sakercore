using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SakerCore.Data
{

    /// <summary>
    /// 为 Mysql 数据库提供驱动处理程序
    /// </summary>
    public class MySqlDBEntityProvide : IDBEntityProvide
    {

        const string returnstr = "?returnvalue";
        static MySqlConnection _instance;
        public IDbCommand BuildInitCommand(IDbConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            MySqlConnection sqlconn = connection as MySqlConnection;

            if (sqlconn == null)
            {
                throw new ArgumentException("传入的参数 connection 无效");
            }

            MySqlCommand command = BuildQueryCommand(sqlconn, storedProcName, parameters);
            return command;
        }
        private MySqlCommand BuildQueryCommand(MySqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            MySqlCommand command = new MySqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            foreach (MySqlParameter parameter in parameters)
            {
                if (parameter != null)
                {
                    // 检查未分配值的输出参数,将其分配以DBNull.Value.
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }
            return command;
        }

        public IDbCommand BulidDBCommand(string sql)
        {
            return new MySqlCommand(sql);
        }
        public IDbConnection GetNewDbConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
        public IDbDataAdapter GetNewDbDataAdapter()
        {
            return new MySqlDataAdapter();
        }
        public object GetParameterValue(object param)
        {
            return (param as MySqlParameter)?.Value ?? -1;
        }

        public object GetReturnValue(IDbCommand selectCommand)
        {
            var parameters = selectCommand.Parameters;
            var hasreturnValue = false;
            foreach (var r in parameters)
            {
                if (parameters.Contains(returnstr) && ((MySqlParameter)parameters[returnstr]).Direction == ParameterDirection.Output)
                {
                    hasreturnValue = true;
                    break;
                }
            }

            if (hasreturnValue)
                return this.GetParameterValue(selectCommand.Parameters[returnstr]);
            else
                return 0;
        }


        public IAsyncResult BeginExecuteReader(IDbCommand comm, AsyncCallback cb, object userObject)
        {
            throw new NotSupportedException();
        }
        public IDataReader EndExecuteReader(IDbCommand comm, IAsyncResult iar)
        {
            throw new NotSupportedException();
        }
    }

}
