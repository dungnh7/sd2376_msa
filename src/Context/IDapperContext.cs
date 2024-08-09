using System.Data;

namespace MotoFacts.Context
{
    public interface IDapperContext
    {
        IDbConnection GetConnection();
    }
}
