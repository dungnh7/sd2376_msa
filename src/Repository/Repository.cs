using Dapper;
using MotoFacts.Context;
using System.Data;

namespace MotoFacts.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IDbConnection _connection;
        public Repository(IDapperContext context)
        {
            _connection = context.GetConnection();
        }

        public async Task Create(T entity, string query)
        {
            await _connection.ExecuteAsync(query, entity);
        }

        public async Task Delete(T entity, int id)
        {
            string query = "DELETE FROM " + typeof(T).Name + " WHERE Id = @Id";
            await _connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<T>> ExecuteQuery(string query, object? parameters = null)
        {
            return await _connection.QueryAsync<T>(query, parameters);
        }

        public async Task<IEnumerable<T>> GetAlls()
        {
            var sql = "SELECT * FROM " + typeof(T).Name;
            return await _connection.QueryAsync<T>(sql);
        }

        public async Task<T> GetById(int id)
        {
            string query = "SELECT * FROM " + typeof(T).Name + " WHERE id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id });
        }

        public async Task Update(T entity, string query)
        {
            await _connection.ExecuteAsync(query, entity);
        }
    }
}
