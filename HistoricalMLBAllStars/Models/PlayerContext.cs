using Microsoft.EntityFrameworkCore;

namespace HistoricalMLBAllStars.Models
{
    public class PlayerContext : DbContext
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
