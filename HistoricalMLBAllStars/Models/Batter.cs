namespace HistoricalMLBAllStars.Models
{
    public class Batter : IPlayer
    {
        public string Position { get; set; }
        public int PosNum { get; set; } // numerical representation of the position
        public string SortedPosNum { get; set; } // when players have multiple positions, this will determine how they are sorted
        public string Name { get; set; }
        public double War { get; set; } // wins above replacement
        public double Avg { get; set; } // batting average
        public double Obp { get; set; } // on base percentage
        public double Slg { get; set; } // slugging percentage
        public int Runs { get; set; }
        public int Hits { get; set; }
        public int HomeRuns { get; set; }
        public int Steals { get; set; }
        public int PlateAppearances { get; set; }
        public double DWar { get; set; } // contribution to WAR from defense
        public int RankPosition { get; set; }
        public string SearchID { get; set; }
        public string Role { get; set; } // starters, reserves, honorable mentions
    }
}

