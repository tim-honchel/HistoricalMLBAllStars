using System.ComponentModel.DataAnnotations;

namespace HistoricalMLBAllStars.Models
{
    public class Player
    {
        [Key]
        public int PlayerID { get; set; }
        public string Position { get; set; }
        public int PosNum { get; set; } // numerical representation of the position
        public string Name { get; set; }
        public string Link { get; set; } // link to player's FanGraphs page
        public double War { get; set; } // wins above replacement
        public double Era { get; set; } // earned run average
        public double Whip { get; set; } // walks and hits per inning pitched
        public int Wins { get; set; }
        public int Saves { get; set; }
        public int Strikeouts { get; set; }
        public int Innings { get; set; }
        public double Games { get; set; }
        public double GamesStarted { get; set; }
        public double Avg { get; set; } // batting average
        public double Obp { get; set; } // on base percentage
        public double Slg { get; set; } // slugging percentage
        public int Runs { get; set; }
        public int Hits { get; set; }
        public int HomeRuns { get; set; }
        public int RBI { get; set; }
        public int Steals { get; set; }
        public int PlateAppearances { get; set; }
        public int DWar { get; set; } // contribution to WAR from defense
        public int RankPosition { get; set; }
        public string SearchID { get; set; }
        public string Role { get; set; } // rotation, bullpen, honorable mentions
        public string Duplicate { get; set; }
        
    }
}

