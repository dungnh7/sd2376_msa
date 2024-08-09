namespace MotoFacts.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAlls();
        Task<T> GetById(int id);
        Task Create(T entity,string query);
        Task Update(T entity,string query);
        Task Delete(T entity,int id);
        Task<IEnumerable<T>> ExecuteQuery(string query, object parameters = null);
    }
}
