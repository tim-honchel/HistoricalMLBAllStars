using Microsoft.EntityFrameworkCore;

namespace HistoricalMLBAllStars.Models
{
    public class PlayerContext : DbContext // Entity Framework context for accessing player database
    {
        public PlayerContext()
        {
        }
        public PlayerContext(DbContextOptions<PlayerContext> options): base(options)
        {

        }
        public DbSet<Player> players { get; set; }
    }
}
