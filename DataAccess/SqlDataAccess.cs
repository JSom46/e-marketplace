using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace DataAccess
{
    public class SqlDataAccess : IDataAccess
    {
        private readonly IConfiguration _config;

        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
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
    }
}
