namespace HistoricalMLBAllStars.Models
{
    public class Pitcher : IPlayer
    {
        public string Position { get; set; }
        public int PosNum { get; set; } // numerical representation of the position
        public string SortedPosNum { get; set; } // when players have multiple positions, this will determine how they are sorted
        public string Name { get; set; }
        public double War { get; set; } // wins above replacement
        public double Era { get; set; } // earned run average
        public double Whip { get; set; } // walks and hits per inning pitched
        public int Wins { get; set; }
        public int Saves { get; set; }
        public int Strikeouts { get; set; }
        public int RankPosition { get; set; }
        public string SearchID { get; set; }
        public string Role { get; set; } // rotation, bullpen, honorable mentions
    }
}