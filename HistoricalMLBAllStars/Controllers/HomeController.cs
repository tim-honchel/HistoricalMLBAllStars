using HistoricalMLBAllStars.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace HistoricalMLBAllStars.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        HttpClient client = new HttpClient();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var newSearch = new Search();
            newSearch.YearDropdown = new List<int> { };
            for (int year = 1900; year <= 2021; year++)
            {
                newSearch.YearDropdown.Add(year);
            }
            newSearch.TeamDropdown = new List<string> { "All Teams", "American League", "National League", "Arizona Diamondbacks", "Atlanta Braves", "Baltimore Orioles", "Boston Red Sox", "Chicago Cubs", "Chicago White Sox", "Cincinnati Reds", "Cleveland Guardians", "Colorado Rockies", "Detroit Tigers", "Houston Astros", "Kansas City Royals", "Los Angeles Angels", "Los Angeles Dodgers", "Miami Marlins", "Milwaukee Brewers", "Minnesota Twins", "New York Mets", "New York Yankees", "Oakland Athletics", "Philadelphia Phillies", "Pittsburgh Pirates", "San Diego Padres", "San Francisco Giants", "Seattle Mariners", "St.Louis Cardinals", "Tampa Bay Rays", "Texas Rangers", "Toronto Blue Jays", "Washington Nationals" };
            return View(newSearch);
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Loading(Search newSearch)
        {
            SetSearchInfo(newSearch); // sets global variables with search information
            return View(newSearch);
        }
        public IActionResult Roster(Search newSearch)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var htmlDictionary = RequestPages(newSearch); // asynchronously get HTML from 11 FanGraphs pages 
            List<IPlayer> players = PrepPlayers(htmlDictionary);
            stopwatch.Stop();
            SearchInfo.Time = stopwatch.Elapsed.Seconds;
            return View(players);
        }
            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public void SetSearchInfo(Search newSearch)
        {
            SearchInfo.StartYear = newSearch.StartYear;
            SearchInfo.EndYear = newSearch.EndYear;
            SearchInfo.LeagueOrTeam = newSearch.LeagueOrTeam;
            Dictionary<string, string> teams = new Dictionary<string, string>()
            {
                {"Arizona Diamondbacks", "15"}, { "Atlanta Braves", "16"}, { "Baltimore Orioles", "2"}, { "Boston Red Sox","3"}, { "Chicago Cubs","17"}, { "Chicago White Sox","4"}, { "Cincinnati Reds","18"}, { "Cleveland Guardians","5"}, { "Colorado Rockies","19"}, { "Detroit Tigers","6"}, { "Houston Astros","21"}, { "Kansas City Royals","7"}, { "Los Angeles Angels","1"}, { "Los Angeles Dodgers","22"}, { "Miami Marlins","20"}, { "Milwaukee Brewers","23"}, { "Minnesota Twins","8"}, { "New York Mets","25"}, { "New York Yankees","9"}, { "Oakland Athletics","10"}, { "Philadelphia Philles","26"}, { "Pittsburgh Pirates","27"}, { "San Diego Padres","29"}, { "Seattle Mariners","11"}, { "San Francisco Giants","30"}, { "St. Louis Cardinals","28"}, { "Tampa Bay Rays","12"}, { "Texas Rangers","13"}, { "Toronto Blue Jays","14"}, { "Washington Nationals","24"}
            }
                ;
            if (newSearch.LeagueOrTeam == "All Teams" || newSearch.LeagueOrTeam == "All")
            {
                SearchInfo.Display = "in baseball";
                SearchInfo.StatAdjustment = 0;
                SearchInfo.League = "all";
                SearchInfo.Team = "0";
            }
            else if (newSearch.LeagueOrTeam == "American League" || newSearch.LeagueOrTeam == "National League")
            {
                SearchInfo.Display = $"in the {newSearch.LeagueOrTeam}";
                SearchInfo.StatAdjustment = 0;
                SearchInfo.League = newSearch.LeagueOrTeam == "American League" ? "al" : "nl";
                SearchInfo.Team = "0";
            }
            else
            {
                SearchInfo.Display = $"for the {newSearch.LeagueOrTeam}";
                SearchInfo.StatAdjustment = 1;
                SearchInfo.League = "all";
                SearchInfo.Team = teams[newSearch.LeagueOrTeam];
            }
        }
        
        public Dictionary<string,string> RequestPages(Search newSearch)
        {
            var minIP = Math.Min(10 * (newSearch.EndYear - newSearch.StartYear + 1), 100);
            var minPA = Math.Min(20 * (newSearch.EndYear - newSearch.StartYear + 1), 200);

            var urlSP = $"https://www.fangraphs.com/leaders.aspx?pos=all&stats=sta&lg={SearchInfo.League}&qual={minIP}&type=c,59,6,42,4,11,24,13&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_10&sort={3-SearchInfo.StatAdjustment},d";
            var urlRP = $"https://www.fangraphs.com/leaders.aspx?pos=all&stats=rel&lg={SearchInfo.League}&qual={minIP}&type=c,59,6,42,4,11,24,13&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_7&sort={3-SearchInfo.StatAdjustment},d";
            var urlC = $"https://www.fangraphs.com/leaders.aspx?pos=c&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var url1B = $"https://www.fangraphs.com/leaders.aspx?pos=1b&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var url2B = $"https://www.fangraphs.com/leaders.aspx?pos=2b&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var url3B = $"https://www.fangraphs.com/leaders.aspx?pos=3b&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlSS = $"https://www.fangraphs.com/leaders.aspx?pos=ss&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlLF = $"https://www.fangraphs.com/leaders.aspx?pos=lf&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlCF = $"https://www.fangraphs.com/leaders.aspx?pos=cf&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlRF = $"https://www.fangraphs.com/leaders.aspx?pos=rf&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlDH = $"https://www.fangraphs.com/leaders.aspx?pos=dh&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_2&sort={3-SearchInfo.StatAdjustment},d";

            string htmlSP = "unassigned";
            string htmlRP = "unassigned";
            string htmlC = "unassigned";
            string html1B = "unassigned";
            string html2B = "unassigned";
            string html3B = "unassigned";
            string htmlSS = "unassigned";
            string htmlLF = "unassigned";
            string htmlCF = "unassigned";
            string htmlRF = "unassigned";
            string htmlDH = "unassigned";

            Task taskSP = Task.Run(() => { htmlSP = GetHTML(urlSP).Result; });
            Task taskRP = Task.Run(() => { htmlRP = GetHTML(urlRP).Result; });
            Task taskC = Task.Run(() => { htmlC = GetHTML(urlC).Result; });
            Task task1B = Task.Run(() => { html1B = GetHTML(url1B).Result; });
            Task task2B = Task.Run(() => { html2B = GetHTML(url2B).Result; });
            Task task3B = Task.Run(() => { html3B = GetHTML(url3B).Result; });
            Task taskSS = Task.Run(() => { htmlSS = GetHTML(urlSS).Result; });
            Task taskLF = Task.Run(() => { htmlLF = GetHTML(urlLF).Result; });
            Task taskCF = Task.Run(() => { htmlCF = GetHTML(urlCF).Result; });
            Task taskRF = Task.Run(() => { htmlRF = GetHTML(urlRF).Result; });
            Task taskDH = Task.Run(() => { htmlDH = GetHTML(urlDH).Result; });

            var htmlDictionary = new Dictionary<string, string>();
            htmlDictionary["SP"] = htmlSP;
            htmlDictionary["RP"] = htmlRP;
            htmlDictionary["C"] = htmlC;
            htmlDictionary["1B"] = html1B;
            htmlDictionary["2B"] = html2B;
            htmlDictionary["3B"] = html3B;
            htmlDictionary["SS"] = htmlSS;
            htmlDictionary["LF"] = htmlLF;
            htmlDictionary["CF"] = htmlCF;
            htmlDictionary["RF"] = htmlRF;
            htmlDictionary["DH"] = htmlDH;

            return htmlDictionary;
        }
        public async Task<string> GetHTML(string url)
        {
            var html = client.GetStringAsync(url).Result;
            return html;
        }
        public List<IPlayer> PrepPlayers(Dictionary<string,string> htmlDictionary)
        {
            List<IPlayer> starters = GeneratePitchers(htmlDictionary["SP"], "SP");
            List<IPlayer> relievers = GeneratePitchers(htmlDictionary["RP"], "RP");
            List<IPlayer> catchers = GenerateBatters(htmlDictionary["C"], "C");
            List<IPlayer> firstBasemen = GenerateBatters(htmlDictionary["1B"], "1B");
            List<IPlayer> secondBasemen = GenerateBatters(htmlDictionary["2B"], "2B");
            List<IPlayer> thirdBasemen = GenerateBatters(htmlDictionary["3B"], "3B");
            List<IPlayer> shortstops = GenerateBatters(htmlDictionary["SS"], "SS");
            List<IPlayer> leftFielders = GenerateBatters(htmlDictionary["LF"], "LF");
            List<IPlayer> centerFielders = GenerateBatters(htmlDictionary["CF"], "CF");
            List<IPlayer> rightFielders = GenerateBatters(htmlDictionary["RF"], "RF");
            List<IPlayer> designatedHitters = GenerateBatters(htmlDictionary["DH"], "DH");

            List<IPlayer> players = new List<IPlayer>(starters.Count + relievers.Count + catchers.Count + firstBasemen.Count + secondBasemen.Count + thirdBasemen.Count + shortstops.Count + leftFielders.Count + centerFielders.Count + rightFielders.Count + designatedHitters.Count);
            players.AddRange(starters);
            players.AddRange(relievers);
            players.AddRange(catchers);
            players.AddRange(firstBasemen);
            players.AddRange(secondBasemen);
            players.AddRange(thirdBasemen);
            players.AddRange(shortstops);
            players.AddRange(leftFielders);
            players.AddRange(centerFielders);
            players.AddRange(rightFielders);
            players.AddRange(designatedHitters);

            return players;
        }
        public List<IPlayer> GeneratePitchers(string html, string position)
        {

            List<IPlayer> pitchers = new List<IPlayer>();
            var pitcher = new Pitcher();
            pitcher.Name = "Test";
            pitcher.Position = position;
            pitcher.Role = "rotation";
            pitchers.Add(pitcher);
            return pitchers;
        }
        public List<IPlayer> GenerateBatters(string html, string position)
        {

            List<IPlayer> batters = new List<IPlayer>();
            var batter = new Batter();
            batter.Name = "Test";
            batter.Position = position;
            batter.Role = "starter";
            batters.Add(batter);
            return batters;
        }
    }
}
