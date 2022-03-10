using Microsoft.EntityFrameworkCore;

namespace HistoricalMLBAllStars.Models
{
    public class SearchContext : DbContext
    {
        public SearchContext()
        {
        }
        public SearchContext(DbContextOptions<SearchContext> options) : base(options)
        {

        }
        public DbSet<Search> searches { get; set; }
    }
}
