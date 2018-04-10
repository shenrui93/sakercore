using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SakerCore.Data
{

    public class MSSQLServerDBEntityProvide : IDBEntityProvide
    {
        public IDbCommand BuildInitCommand(IDbConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlConnection sqlconn = connection as SqlConnection;

            if (sqlconn == null)
            {
                throw new ArgumentException("传入的参数 connection 无效");
            }
            SqlCommand command = BuildQueryCommand(sqlconn, storedProcName, parameters);

            WriteDBTrace(command);

            command.Parameters.Add(new SqlParameter("ReturnValue",
                SqlDbType.Int, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }

        private void WriteDBTrace(SqlCommand command)
        {
            //StringBuilder strb = new StringBuilder();

            //strb.AppendLine(command.CommandText +" ");
            //foreach(SqlParameter r in command.Parameters)
            //{ 
            //    if(r.)
            //    strb.AppendLine($@"@{r.ParameterName} = {r.Value}");
            //}
            //strb.AppendLine()







        }

        private SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            foreach (SqlParameter parameter in parameters)
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
            return new System.Data.SqlClient.SqlCommand(sql);
        }
        public IDbConnection GetNewDbConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
        public IDbDataAdapter GetNewDbDataAdapter()
        {
            return new System.Data.SqlClient.SqlDataAdapter();
        }
        public object GetParameterValue(object param)
        {
            return (param as SqlParameter)?.Value ?? -1;
        }
        public object GetReturnValue(IDbCommand selectCommand)
        {
            var param = selectCommand.Parameters;
            if (!param.Contains("ReturnValue")) return 0;
            return ((SqlParameter)param["ReturnValue"]).Value;
        }
        public IAsyncResult BeginExecuteReader(IDbCommand comm, AsyncCallback cb, object userObject)
        {
            var sqlcomm = comm as SqlCommand;
            if (sqlcomm == null)
            {
                throw new NotSupportedException("提供了不支持的操作对象");
            }
            return sqlcomm.BeginExecuteReader(cb, userObject);

        }
        public IDataReader EndExecuteReader(IDbCommand comm, IAsyncResult iar)
        {
            var sqlcomm = comm as SqlCommand;
            if (sqlcomm == null)
            {
                throw new NotSupportedException("提供了不支持的操作对象");
            }
            return sqlcomm.EndExecuteReader(iar);

        }
    }

}
