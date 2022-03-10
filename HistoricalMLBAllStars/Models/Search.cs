using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HistoricalMLBAllStars.Models
{
    public class Search
    {
        [Key]
        public string ID { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public string LeagueOrTeam { get; set; }
    }
}
