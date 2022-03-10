using Microsoft.EntityFrameworkCore;
using System.Data.Entity.Infrastructure;

namespace HistoricalMLBAllStars.Models
{
    public class SearchContext : DbContext // Entity Framework context for accessing player database
    {
        public int? CommandTimeout { get; set; }
        public SearchContext()
        {
            
        }
        public SearchContext(DbContextOptions<SearchContext> options) : base(options)
        {
            CommandTimeout = 1;
        }
        public DbSet<Search> searches { get; set; }
        
    }
}
