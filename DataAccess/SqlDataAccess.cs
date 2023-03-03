using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace DataAccess
{
    public class SqlDataAccess : IDataAccess
    {
        private readonly IConfiguration _config;
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
            Console.WriteLine("sqldataaccess created");
        }

        public async Task<IEnumerable<U>> Query<T, U>(
            string storedProcedure,
            T parameters,
            string connectionId = "Default")
        {
            using IDbConnection con = new SqlConnection(_config.GetConnectionString(connectionId));

            return await con.QueryAsync<U>(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<U> QuerySingle<T, U>(
            string storedProcedure,
            T parameters,
            string connectionId = "Default")
        {
            using IDbConnection con = new SqlConnection(_config.GetConnectionString(connectionId));

            return await con.QuerySingleOrDefaultAsync<U>(storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> Execute<T>(
            string storedProcedure,
            T parameters,
            string connectionId = "Default")
        {
            using IDbConnection con = new SqlConnection(_config.GetConnectionString(connectionId));

            return await con.ExecuteAsync(storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public void StartTransaction(string connectionId = "Default")
        {
            _connection = new SqlConnection(_config.GetConnectionString(connectionId));
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _transaction?.Commit();
            _connection?.Close();
        }

        public void RollbackTransaction()
        {
            _transaction?.Rollback();
            _connection?.Close();
        }

        public async Task<IEnumerable<U>> QueryInTransaction<T, U>(
            string storedProcedure,
            T parameters)
        {
            return await _connection.QueryAsync<U>(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                transaction: _transaction);
        }

        public async Task<U> QuerySingleInTransaction<T, U>(
            string storedProcedure,
            T parameters)
        {
            return await _connection.QuerySingleOrDefaultAsync<U>(storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                transaction: _transaction);
        }

        public async Task<int> ExecuteInTransaction<T>(
            string storedProcedure,
            T parameters)
        {
            return await _connection.ExecuteAsync(storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                transaction: _transaction);
        }
    }
}
