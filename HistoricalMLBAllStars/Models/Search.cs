using System.Collections.Generic;

namespace HistoricalMLBAllStars.Models
{
    public class Search
    {
        public string ID { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public string LeagueOrTeam { get; set; }
        public List<int> YearDropdown;
        public List<string> TeamDropdown;
        public string Display { get; set; }
    }
}
