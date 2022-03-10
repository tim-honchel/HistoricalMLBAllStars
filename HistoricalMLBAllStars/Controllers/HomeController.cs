using HistoricalMLBAllStars.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;

namespace HistoricalMLBAllStars.Controllers
{
    public class HomeController : Controller
    {
        private readonly PlayerContext _playerContext;
        private readonly SearchContext _searchContext;
        private readonly ILogger<HomeController> _logger;
        HttpClient client = new HttpClient();

        public HomeController(PlayerContext playerContext, SearchContext searchContext)
        {
            _playerContext = playerContext; // Entity Framework context for querying player table
            _searchContext = searchContext; // EF context for search table
        }

        public IActionResult Index()
        {
            SearchInfo.Message = ""; // resets static variables
            SearchInfo.stopwatch.Reset();
            SearchInfo.InDatabase = false;
            var newSearch = new Search();
            SearchInfo.YearDropdown = new List<int> { };
            for (int year = 1900; year <= 2021; year++)
            {
                SearchInfo.YearDropdown.Add(year); // creates dropdown list of years
            }
            SearchInfo.TeamDropdown = new List<string> { "All Teams", "American League", "National League", "Arizona Diamondbacks", "Atlanta Braves", "Baltimore Orioles", "Boston Red Sox", "Chicago Cubs", "Chicago White Sox", "Cincinnati Reds", "Cleveland Guardians", "Colorado Rockies", "Detroit Tigers", "Houston Astros", "Kansas City Royals", "Los Angeles Angels", "Los Angeles Dodgers", "Miami Marlins", "Milwaukee Brewers", "Minnesota Twins", "New York Mets", "New York Yankees", "Oakland Athletics", "Philadelphia Phillies", "Pittsburgh Pirates", "San Diego Padres", "San Francisco Giants", "Seattle Mariners", "St.Louis Cardinals", "Tampa Bay Rays", "Texas Rangers", "Toronto Blue Jays", "Washington Nationals" };
            return View(newSearch);
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Preloading(Search newSearch)
        {
            SearchInfo.stopwatch.Start(); // starts a stopwatch to track how the long the search takes
            SetSearchInfo(newSearch); // sets global variables with search information
            newSearch.ID = $"{newSearch.StartYear}{newSearch.EndYear}{newSearch.LeagueOrTeam}".Replace(" ", ""); // creates a unique primary key for the searchID
            SearchInfo.SearchID = newSearch.ID;
            return View(newSearch); // a temporary loading screen
        }
        public IActionResult Loading(Search newSearch)
        {
            SearchInfo.Message = $"{SearchInfo.Message} Searched database index.";
            try
            {
                SearchInfo.InDatabase = _searchContext.searches.Any(m => m.ID == SearchInfo.SearchID); // records whether the search already exists in the database
                if (SearchInfo.InDatabase == true)
                {
                    SearchInfo.Message = $"{SearchInfo.Message} Index found.";
                }
                else
                {
                    SearchInfo.Message = $"{SearchInfo.Message} Index not found.";
                }
            }
            catch (Exception)
            {
                SearchInfo.Message = "Database index search failed."; // records if there is a connection timeout so the program knows not to attempt querying the database again
            }

            return View(newSearch); // the same loading screen
        }
        public IActionResult Roster() // called if there were no results in the database
        {
            SearchInfo.Message = $"{SearchInfo.Message} Searched FanGraphs.";
            List<Player> players = new List<Player>();
            try
            {
                var htmlDictionary = RequestPages(); // asynchronously get HTML from 11 FanGraphs pages
                players = PrepPlayers(htmlDictionary); // scrapes the HTML into player objects
                SearchInfo.Message = $"{SearchInfo.Message} FanGraphs connection succeeded.";
            }
            catch (Exception)
            {
                SearchInfo.Message = $"{SearchInfo.Message} FanGraphs connection failed.";
                return RedirectToAction("Error"); // shows error page if FanGraphs request failed
            }
            CheckDuplicates(players); // consolidates any players who appeared at multiple positions
            ChooseRoster(players); // selects the top 25 players, plus 10 honorable mentions
            Search newSearch = new Search() 
            { 
                ID = SearchInfo.SearchID,
                StartYear = SearchInfo.StartYear,
                EndYear = SearchInfo.EndYear,
                LeagueOrTeam = SearchInfo.LeagueOrTeam
            };
            if (SearchInfo.Message.Contains("Database index search failed.") == true)
            {
                SearchInfo.Message = $"{SearchInfo.Message} Database save not attempted.";
            }
            else // writes results to the database, as long as the initial database query did not time out
            {
                try
                {
                    _searchContext.Add(newSearch);
                    _playerContext.AddRange(players);
                    _searchContext.SaveChanges();
                    _playerContext.SaveChanges();
                    SearchInfo.Message = $"{SearchInfo.Message} Database save succeeded.";
                }
                catch (Exception)
                {
                    SearchInfo.Message = $"{SearchInfo.Message} Database save failed."; // if the attempt times out, the results are not saved
                }
            }
            SearchInfo.stopwatch.Stop(); // stops the stopwatch to get the total search time
            SearchInfo.Time = SearchInfo.stopwatch.Elapsed.Seconds;
            return View(players.OrderBy(x => x.PosNum).ThenByDescending(x => x.War).ToList()); // orders the player list by position, then WAR
        }
        public IActionResult QuickRoster() // called if there were results in the database
        {
            SearchInfo.Message = $"{SearchInfo.Message} Searched player database.";
            List<Player> players = new List<Player>();
            try
            {
                players = _playerContext.players.Where(m => m.SearchID == SearchInfo.SearchID).ToList(); // retrieves matching players from database
                SearchInfo.Message = $"{SearchInfo.Message} Database search succeeded.";
            }
            catch
            {
                SearchInfo.Message = $"{SearchInfo.Message} Database search failed.";
                return RedirectToAction("Roster"); // if the database connection times out, searches FanGraphs instead
            }
            SearchInfo.stopwatch.Stop(); // stops the stopwatch to get the total search time
            SearchInfo.Time = SearchInfo.stopwatch.Elapsed.Seconds;
            return View(players.OrderBy(x => x.PosNum).ThenByDescending(x => x.War).ToList()); // orders the player list by position, then WAR
        }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); // error page
        }
        public void SetSearchInfo(Search newSearch)
        {
            SearchInfo.StartYear = newSearch.StartYear;
            SearchInfo.EndYear = newSearch.EndYear;
            SearchInfo.LeagueOrTeam = newSearch.LeagueOrTeam;
            Dictionary<string, string> teams = new Dictionary<string, string>()
            {
                {"Arizona Diamondbacks", "15"}, { "Atlanta Braves", "16"}, { "Baltimore Orioles", "2"}, { "Boston Red Sox","3"}, { "Chicago Cubs","17"}, { "Chicago White Sox","4"}, { "Cincinnati Reds","18"}, { "Cleveland Guardians","5"}, { "Colorado Rockies","19"}, { "Detroit Tigers","6"}, { "Houston Astros","21"}, { "Kansas City Royals","7"}, { "Los Angeles Angels","1"}, { "Los Angeles Dodgers","22"}, { "Miami Marlins","20"}, { "Milwaukee Brewers","23"}, { "Minnesota Twins","8"}, { "New York Mets","25"}, { "New York Yankees","9"}, { "Oakland Athletics","10"}, { "Philadelphia Philles","26"}, { "Pittsburgh Pirates","27"}, { "San Diego Padres","29"}, { "Seattle Mariners","11"}, { "San Francisco Giants","30"}, { "St. Louis Cardinals","28"}, { "Tampa Bay Rays","12"}, { "Texas Rangers","13"}, { "Toronto Blue Jays","14"}, { "Washington Nationals","24"}
            } // each team has a corresponding number for FanGraphs searches
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
                SearchInfo.StatAdjustment = 1; // when searching a single team, it's helpful to adjust search parameters to include more or fewer players, depending on the scenario
                SearchInfo.League = "all";
                SearchInfo.Team = teams[newSearch.LeagueOrTeam];
            }
        }
        
        public Dictionary<string,string> RequestPages()
        {
            var minIP = Math.Min(10 * (SearchInfo.EndYear - SearchInfo.StartYear + 1), 100); // sets minimum innings pitched to exclude unqualified pitchers
            var minPA = Math.Min(20 * (SearchInfo.EndYear - SearchInfo.StartYear + 1), 200); // sets minimum plate appearances to exclude unquialified batters

            // interpolated URLs for FanGraphs
            var urlSP = $"https://www.fangraphs.com/leaders.aspx?pos=all&stats=pit&lg={SearchInfo.League}&qual={minIP}&type=c,59,6,42,4,11,24,13,7,8&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_10&sort={3-SearchInfo.StatAdjustment},d";
            var urlRP = $"https://www.fangraphs.com/leaders.aspx?pos=all&stats=pit&lg={SearchInfo.League}&qual=0&type=c,59,6,42,4,11,24,13,7,8&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{40-20*SearchInfo.StatAdjustment}&sort={10-SearchInfo.StatAdjustment},d";
            var urlC = $"https://www.fangraphs.com/leaders.aspx?pos=c&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var url1B = $"https://www.fangraphs.com/leaders.aspx?pos=1b&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var url2B = $"https://www.fangraphs.com/leaders.aspx?pos=2b&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var url3B = $"https://www.fangraphs.com/leaders.aspx?pos=3b&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlSS = $"https://www.fangraphs.com/leaders.aspx?pos=ss&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlLF = $"https://www.fangraphs.com/leaders.aspx?pos=lf&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlCF = $"https://www.fangraphs.com/leaders.aspx?pos=cf&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlRF = $"https://www.fangraphs.com/leaders.aspx?pos=rf&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_{3+SearchInfo.StatAdjustment}&sort={3-SearchInfo.StatAdjustment},d";
            var urlDH = $"https://www.fangraphs.com/leaders.aspx?pos=dh&stats=bat&lg={SearchInfo.League}&qual={minPA}&type=c,58,23,37,38,7,12,11,13,21,6,203,199&season={SearchInfo.EndYear}&month=0&season1={SearchInfo.StartYear}&ind=0&team={SearchInfo.Team}&rost=0&age=0&filter=&players=0&startdate={SearchInfo.StartYear}-01-01&enddate={SearchInfo.EndYear}-12-31&page=1_2&sort={3-SearchInfo.StatAdjustment},d";

            // will store HTML from the URLs
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

            // asynchronous tasks open the URLs and grab the HTML
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
            Task.WhenAll(taskSP, taskRP, taskC, task1B, task2B, task3B, taskSS, taskLF, taskCF, taskRF, taskDH).Wait();

            // saves the HTMLs into a dictionary for later use
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
        public List<Player> PrepPlayers(Dictionary<string,string> htmlDictionary)
        {
            // generates list of players for each position
            List<Player> starters = GeneratePitchers(htmlDictionary["SP"], "SP", 10);
            List<Player> relievers = GeneratePitchers(htmlDictionary["RP"], "RP", 40 - 20 * SearchInfo.StatAdjustment);
            List<Player> catchers = GenerateBatters(htmlDictionary["C"], "C", 3 + SearchInfo.StatAdjustment);
            List<Player> firstBasemen = GenerateBatters(htmlDictionary["1B"], "1B", 3 + SearchInfo.StatAdjustment);
            List<Player> secondBasemen = GenerateBatters(htmlDictionary["2B"], "2B", 3 + SearchInfo.StatAdjustment);
            List<Player> thirdBasemen = GenerateBatters(htmlDictionary["3B"], "3B", 3 + SearchInfo.StatAdjustment);
            List<Player> shortstops = GenerateBatters(htmlDictionary["SS"], "SS", 3 + SearchInfo.StatAdjustment);
            List<Player> leftFielders = GenerateBatters(htmlDictionary["LF"], "LF", 3 + SearchInfo.StatAdjustment);
            List<Player> centerFielders = GenerateBatters(htmlDictionary["CF"], "CF", 3 + SearchInfo.StatAdjustment);
            List<Player> rightFielders = GenerateBatters(htmlDictionary["RF"], "RF", 3 + SearchInfo.StatAdjustment);
            List<Player> designatedHitters = new List<Player>();
            // National League teams usually do not have a DH, so a search would cause an error
            if (SearchInfo.League == "al" || (SearchInfo.League == "all" && SearchInfo.Team == "0") || (Convert.ToInt32(SearchInfo.Team) <= 14 && Convert.ToInt32(SearchInfo.Team) > 0) || Convert.ToInt32(SearchInfo.Team) == 21)
            {
                designatedHitters = GenerateBatters(htmlDictionary["DH"], "DH", 2);
            }
            List<Player> players = new List<Player>(starters.Count + relievers.Count + catchers.Count + firstBasemen.Count + secondBasemen.Count + thirdBasemen.Count + shortstops.Count + leftFielders.Count + centerFielders.Count + rightFielders.Count + designatedHitters.Count);
            // combines positions into a single player list
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
        public List<Player> GeneratePitchers(string html, string position, int numRows)
        {
            List<Player> pitchers = new List<Player>();
            Dictionary<string, int> positions = new Dictionary<string, int>()
            {
                { "SP", 0 }, { "RP", 1 }
            }; // assigns numbers to different positions

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html); // loads FanGraphs HTML into HTTP Agility Pack parser
            for (int rowNum = 0; rowNum < numRows; rowNum++) // for each desired player
            {
                var relevantHTML = htmlDoc.DocumentNode.SelectSingleNode($"//tr[@id='LeaderBoard1_dg1_ctl00__{rowNum}']"); // isolates the correct section of HTML
                if (relevantHTML != null) // sometimes the list doesn't have as many players as the program requests
                {
                    var rawRow = relevantHTML.OuterHtml; // converts the HTML to string
                    var rawRowSplit = rawRow.Split("</td>"); // splits the string into columns for different stats
                    var columnNum = 0;
                    var pitcher = new Player(); // creates a player object
                    pitcher.Position = position;
                    foreach (string rawColumn in rawRowSplit) // cycles through each column, parses the desired info, and stores it to a variable
                    {
                        if (columnNum == 0 || (columnNum == 2 && SearchInfo.StatAdjustment == 0) || columnNum == 12)
                        {

                        }
                        if (columnNum == 1)
                        {
                            pitcher.Name = rawColumn.Split(">")[2].Split("<")[0];
                            pitcher.Link = $"https://fangraphs.com/{rawColumn.Split(">")[1].Split('"')[1]}";
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 3)
                        {
                            pitcher.War = Math.Round(Convert.ToDouble(rawColumn.Split(">")[1]), 1);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 4)
                        {
                            pitcher.Era = Math.Round(Convert.ToDouble(rawColumn.Split(">")[1]), 2);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 5)
                        {
                            pitcher.Whip = Math.Round(Convert.ToDouble(rawColumn.Split(">")[1]), 2);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 6)
                        {
                            pitcher.Wins = Convert.ToInt32(rawColumn.Split(">")[1]);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 7)
                        {
                            pitcher.Saves = Convert.ToInt32(rawColumn.Split(">")[1]);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 8)
                        {
                            pitcher.Strikeouts = Convert.ToInt32(rawColumn.Split(">")[1]);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 9)
                        {
                            pitcher.Innings = Convert.ToInt32(Math.Floor(Convert.ToDouble(rawColumn.Split(">")[1])));
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 10)
                        {
                            pitcher.Games = Convert.ToDouble((rawColumn.Split(">")[1]));
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 11)
                        {
                            pitcher.GamesStarted = Convert.ToDouble((rawColumn.Split(">")[1]));
                        }
                        columnNum++;
                    }
                    pitcher.Position = position;
                    pitcher.PosNum = positions[position];

                    // decides whether a pitcher is a starter, reliever, or both
                    if ((pitcher.GamesStarted / pitcher.Games) <= 0.9 && (pitcher.GamesStarted / pitcher.Games) >= 0.5)
                    {
                        pitcher.Position = "SP/RP";
                        pitcher.PosNum = 0;
                    }
                    else if ((pitcher.GamesStarted / pitcher.Games) < 0.5 && (pitcher.GamesStarted / pitcher.Games) >= 0.1)
                    {
                        pitcher.Position = "RP/SP";
                        pitcher.PosNum = 1;
                    }
                    else if (pitcher.GamesStarted / pitcher.Games > 0.9)
                    {
                        pitcher.Position = "SP";
                        pitcher.PosNum = 0;
                    }
                    else
                    {
                        pitcher.Position = "RP";
                        pitcher.PosNum = 1;
                    }
                    pitcher.Role = "mention pitcher";
                    pitcher.SearchID = SearchInfo.SearchID;
                    pitchers.Add(pitcher);
                }
            }


            return pitchers;
        }
        public List<Player> GenerateBatters(string html, string position, int numRows) // see comments from GeneratePitchers method
        {
            List<Player> batters = new List<Player>();

            Dictionary<string, int> positions = new Dictionary<string, int>()
            {
                { "C", 2 }, { "1B", 3 }, { "2B", 4 }, { "3B", 5 }, { "SS", 6 }, { "LF", 7 }, { "CF", 8 }, { "RF", 9 }, { "DH", 10 }
            };

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            for (int rowNum = 0; rowNum < numRows; rowNum++)
            {
                var relevantHTML = htmlDoc.DocumentNode.SelectSingleNode($"//tr[@id='LeaderBoard1_dg1_ctl00__{rowNum}']");
                if (relevantHTML != null)
                {
                    var rawRow = relevantHTML.OuterHtml;
                    var rawRowSplit = rawRow.Split("</td>");
                    var columnNum = 0;
                    var batter = new Player();
                    batter.Position = position;
                    batter.RankPosition = rowNum + 1;
                    foreach (string rawColumn in rawRowSplit)
                    {
                        if (columnNum == 0 || (columnNum == 2 && SearchInfo.StatAdjustment == 0) || columnNum + SearchInfo.StatAdjustment == 13 || columnNum + SearchInfo.StatAdjustment == 15)
                        {

                        }
                        if (columnNum == 1)
                        {
                            batter.Name = rawColumn.Split(">")[2].Split("<")[0];
                            batter.Link = $"https://fangraphs.com/{rawColumn.Split(">")[1].Split('"')[1]}";
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 3)
                        {
                            batter.War = Math.Round(Convert.ToDouble(rawColumn.Split(">")[1]), 1);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 4)
                        {
                            batter.Avg = Math.Round(Convert.ToDouble(rawColumn.Split(">")[1]), 3);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 5)
                        {
                            batter.Obp = Math.Round(Convert.ToDouble(rawColumn.Split(">")[1]), 3);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 6)
                        {
                            batter.Slg = Math.Round(Convert.ToDouble(rawColumn.Split(">")[1]), 3);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 7)
                        {
                            batter.Hits = Convert.ToInt32(rawColumn.Split(">")[1]);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 8)
                        {
                            batter.Runs = Convert.ToInt32(rawColumn.Split(">")[1]);
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 9)
                        {
                            batter.HomeRuns = Convert.ToInt32(Math.Floor(Convert.ToDouble(rawColumn.Split(">")[1])));
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 10)
                        {
                            batter.RBI = Convert.ToInt32(Math.Floor(Convert.ToDouble(rawColumn.Split(">")[1])));
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 11)
                        {
                            batter.Steals = Convert.ToInt32(Math.Floor(Convert.ToDouble(rawColumn.Split(">")[1])));
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 12)
                        {
                            batter.PlateAppearances = Convert.ToInt32(Math.Floor(Convert.ToDouble(rawColumn.Split(">")[1])));
                        }
                        if (columnNum + SearchInfo.StatAdjustment == 14)
                        {
                            batter.DWar = Convert.ToInt32(Convert.ToDouble(rawColumn.Split(">")[1]) / 10);
                        }
                        columnNum++;
                    }
                    batter.Position = position;
                    batter.PosNum = positions[position];
                    batter.Role = "mention batter";
                    batter.SearchID = SearchInfo.SearchID;
                    batters.Add(batter);
                }
            }


            return batters;
        }
        public void CheckDuplicates (List<Player> players)
        {
            foreach (Player player1 in players)
            {
                foreach (Player player2 in players) // compares each player to every other player
                {
                    if (player1.Name == player2.Name && player1 != player2 && player1.Duplicate != "true" && player2.Duplicate != "true") // if they are the same player and haven't already been marked for removal
                    {
                        ChooseDuplicateToRemove(player1, player2, players);
                    }
                }
            }
            List<Player> playersToRemove = players.FindAll(x => x.Duplicate == "true"); // removes all duplicate players
            foreach (Player player in playersToRemove)
            {
                    players.Remove(player);
            }
        }
        public void ChooseDuplicateToRemove(Player player1, Player player2, List<Player> players)
        {
            if (player2.RankPosition == 1 && player1.RankPosition > 1 && player1.PosNum >= 2 && player2.PosNum != 10) // if one instance is ranked #1 at their position, the other is deleted
            {
                player2.Position = $"{player2.Position}/{player1.Position}";
                player1.Duplicate = "true";
            }
            else if (player2.RankPosition == 1 && player1.RankPosition == 1 && player2.PosNum >= 2 && player2.PosNum != 10) // if both instances are ranked #1, chooses the position with the worse #2 ranked player
            {
                var nextBestPosition1 = players.Find(x => x.PosNum == player1.PosNum && x.RankPosition == 2).War;
                var nextBestPosition2 = players.Find(x => x.PosNum == player2.PosNum && x.RankPosition == 2).War;
                if (nextBestPosition1 > nextBestPosition2)
                {
                    player2.Position = $"{player2.Position}/{player1.Position}";
                    player1.Duplicate = "true";
                }
                else
                {
                    player1.Position = $"{player1.Position}/{player2.Position}";
                    player2.Duplicate = "true";
                }
            }
            else if (player2.PosNum == 2 && player2.RankPosition == 2 && player1.RankPosition != 1) // #2 catcher is given priority
            {
                player2.Position = $"{player2.Position}/{player1.Position}";
                player1.Duplicate = "true";
            }
            else if (player1.PosNum == 10) // DH has the lowest priority
            {
                player2.Position = $"{player2.Position}/{player1.Position}";
                player1.Duplicate = "true";
            }
            else if (player1.PosNum == 3 && player2.PosNum != 10) // 1B has the second lowest priority
            {
                player2.Position = $"{player2.Position}/{player1.Position}";
                player1.Duplicate = "true";
            }
            else if (player2.PosNum + player1.PosNum <= 2) // for pitchers, it doesn't matter which is kept
            {
                player2.Duplicate = "true";
            }
            else
            {
                player1.Position = $"{player1.Position}/{player2.Position}";
                player2.Duplicate = "true";
            }
        }
        public void ChooseRoster(List<Player> players)
        {
            Dictionary<int, int> positionCount = new Dictionary<int, int>()
            {
                {0, 0 }, {1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 }, { 7, 0 }, { 8, 0 }, { 9, 0 }, { 10, 0 }
            }; // counts the number selected for each position
            Dictionary<string, int> rosterCount = new Dictionary<string, int>()
            {
                {"rotation", players.FindAll(x => x.Role == "rotation").Count() }, {"bullpen", players.FindAll(x => x.Role == "bullpen").Count() }, {"mention pitcher", 0}, { "starters", 0 }, { "reserves", 0 }, { "backup C", 0 }, { "backup IF", 0 }, { "backup OF", 0 }, { "DH", 0 }, { "flex reserve", 0 }, {"mention batter", 0}
            }; // counts the number for various roles
            var sortedPlayers = players.OrderByDescending(x => x.War); // sorts all players by WAR

            foreach (Player player in sortedPlayers) // cycles through each player, best to worst
            {
                if (player.Role == "mention batter") // if batter
                {
                    if (rosterCount["starters"] < 9) // looks for starters if 9 haven't been selected yet
                    {
                        if (player.PosNum >= 2 && player.PosNum < 10 && positionCount[player.PosNum] == 0)
                        {
                            player.Role = "starter";
                            positionCount[player.PosNum]++;
                            rosterCount["starters"]++;
                        }
                        else if (player.PosNum == 10 && positionCount[10] == 0)
                        {
                            player.Role = "starter";
                            player.PosNum = 10;
                            positionCount[10]++;
                            rosterCount["DH"]++;
                            rosterCount["starters"]++;
                        }
                        else if (player.PosNum >= 2 && positionCount[10] == 0)
                        {
                            player.Role = "starter";
                            if (player.Position.Contains("DH") == false)
                            {
                                player.Position = $"{player.Position}(DH)";
                            }
                            else if (player.Position.Contains("/DH") == true)
                            {
                                player.Position = $"DH/{player.Position.Substring(0,player.Position.Length-3)}";
                            }
                            player.PosNum = 10;
                            positionCount[10]++;
                            rosterCount["starters"]++;
                        }
                    }
                    if (rosterCount["reserves"] < 4 && player.Role == "mention batter") // looks for reserves if 4 haven't been selected yet
                    {
                        if (player.PosNum == 2 && rosterCount["backup C"] == 0)
                        {
                            player.Role = "reserve";
                            rosterCount["backup C"]++;
                            rosterCount["reserves"]++;
                        }
                        else if ((player.PosNum == 4 || player.PosNum == 5 || player.PosNum == 6) && rosterCount["backup IF"] == 0)
                        {
                            player.Role = "reserve";
                            rosterCount["backup IF"]++;
                            rosterCount["reserves"]++;
                        }
                        else if ((player.PosNum == 7 || player.PosNum == 8 || player.PosNum == 9) && rosterCount["backup OF"] == 0)
                        {
                            player.Role = "reserve";
                            rosterCount["backup OF"]++;
                            rosterCount["reserves"]++;
                        }
                        else if (player.PosNum >= 2 && player.PosNum < 10 && rosterCount["flex reserve"] == 0)
                        {
                            player.Role = "reserve";
                            rosterCount["flex reserve"]++;
                            rosterCount["reserves"]++;
                        }
                        else if (player.PosNum == 10 && rosterCount["DH"] == 0 && rosterCount["flex reserve"] == 0)
                        {
                            player.Role = "reserve";
                            rosterCount["flex reserve"]++;
                            rosterCount["DH"]++;
                            rosterCount["reserves"]++;
                        }
                    }
                }
                else if (player.Role == "mention pitcher") // if pitcher
                {
                    if (player.PosNum == 0 && rosterCount["rotation"] < 5) // looks for starters if fewer than 5 have been selected
                    {
                        player.Role = "rotation";
                        positionCount[player.PosNum]++;
                        rosterCount["rotation"]++;
                    }
                    else if (rosterCount["bullpen"] < 7 && positionCount[player.PosNum] < 8) // looks for relievers if fewer than 7 have been selected
                    {
                        player.Role = "bullpen";
                        positionCount[player.PosNum]++;
                        player.PosNum = 1;
                        rosterCount["bullpen"]++;
                    }
                }
                if (player.Role == "mention batter" || player.Role == "mention pitcher") // chooses best 10 remaining as honorable mentions
                {
                    rosterCount[player.Role]++;
                    if (rosterCount[player.Role] > 5)
                    {
                        player.Duplicate = "true";
                    }
                }
            }
            List<Player> playersToRemove = players.FindAll(x => x.Duplicate == "true");
            foreach (Player player in playersToRemove) // removes players who weren't selected
            {
                players.Remove(player);
            }
        }
    }
}
