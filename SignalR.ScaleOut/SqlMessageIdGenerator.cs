using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SignalR.ScaleOut
{
    public class SqlMessageIdGenerator : IMessageIdGenerator
    {
        private readonly string _connectionString;

        private const string _getMessageIdSql = "INSERT INTO {TableName} (EventKey, Created)" +
                                                "VALUES (@EventKey, GETDATE()) " +
                                                "SELECT @@IDENTITY";

        public SqlMessageIdGenerator(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            MessageTableName = "[dbo].[SignalRMessages]";

            _connectionString = connectionString;
        }

        public string MessageTableName { get; set; }

        private SqlConnection CreateAndOpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public Task<long> GenerateMessageId(string key)
        {
            var connection = CreateAndOpenConnection();
            var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);
            var cmd = new SqlCommand(_getMessageIdSql.Replace("{TableName}", MessageTableName), connection, transaction);
            cmd.Parameters.AddWithValue("EventKey", key);
            return cmd.ExecuteScalarAsync<long>()
                .ContinueWith(idTask =>
                              {
                                  // We purposely don't commit the transaction, we just wanted the ID anyway, not the record
                                  connection.Close();
                                  return idTask;
                              })
                .Unwrap();
        }
    }
}