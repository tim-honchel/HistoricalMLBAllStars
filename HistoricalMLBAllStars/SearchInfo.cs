using System.Collections.Generic;
using System.Diagnostics;
using HistoricalMLBAllStars.Models;

namespace HistoricalMLBAllStars
{
    public static class SearchInfo
    {
        public static int StartYear { get; set; }
        public static int EndYear { get; set; }
        public static string LeagueOrTeam { get; set; }
        public static string Display { get; set; }
        public static int StatAdjustment { get; set; }
        public static string League { get; set; }
        public static string Team { get; set; }
        public static double Time { get; set; }
        public static string SearchID { get; set; }
        public static bool InDatabase { get; set; }
        public static List<int> YearDropdown;
        public static List<string> TeamDropdown;
        public static Stopwatch stopwatch = new Stopwatch();
        public static string Message { get; set; }
    }
}
