using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data
{
    public class DbContext : IDisposable
    {
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction;

        public DbContext(SqlConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.CreateConnection();
        }

        public SqlConnection Connection => _connection;
        public SqlTransaction Transaction => _transaction;

        public async Task OpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await OpenAsync();
            _transaction = (SqlTransaction)await _connection.BeginTransactionAsync();
        }

        public Task CommitAsync()
        {
            _transaction?.Commit();
            _transaction = null;
            return Task.CompletedTask;
        }

        public Task RollbackAsync()
        {
            _transaction?.Rollback();
            _transaction = null;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }

}
