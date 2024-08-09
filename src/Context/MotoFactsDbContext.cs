using Microsoft.EntityFrameworkCore;

namespace MotoFacts
{
    public class MotoFactsDbContext : DbContext
    {
        public MotoFactsDbContext(DbContextOptions<MotoFactsDbContext> options) : base(options)
        {
        }

        public DbSet<products> products { get; set; }
    }
}