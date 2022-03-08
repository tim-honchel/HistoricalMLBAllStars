namespace HistoricalMLBAllStars
{
    public interface IPlayer
    {
        public string Position { get; set; }
        public int PosNum { get; set; } // numerical representation of the position
        public string SortedPosNum { get; set; } // when players have multiple positions, this will determine how they are sorted
        public string Name { get; set; }
        public double War { get; set; } // wins above replacement
        public string Role { get; set; } // rotation, bullpen, starters, reserves, honorable mentions
    }
}
