using Faceit_Stats_Provider.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using MatchType = Faceit_Stats_Provider.Models.MatchType;
using Microsoft.Extensions.FileSystemGlobbing;
using Faceit_Stats_Provider.Classes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using static Faceit_Stats_Provider.Models.PlayerStats;
using System.Net.Sockets;
using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Faceit_Stats_Provider.Controllers
{
    public class PlayerStatsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly Random _random;
        static string playerid = "";
        private readonly ILogger<HttpClientManager> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;
        private readonly GetTotalEloRetrievesCountFromRedis _getTotalEloRetrievesCountFromRedis;
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly ILoadMoreMatches _loadMoreMatchesService;
        private readonly IFetchMaxElo _fetchMaxEloService;
        private readonly IRazorViewEngine _razorViewEngine;


        public PlayerStatsController(ILogger<HttpClientManager> logger, IHttpClientFactory clientFactory, IMemoryCache cache, IConfiguration configuration, IConnectionMultiplexer redis, GetTotalEloRetrievesCountFromRedis getTotalEloRetrievesCountFromRedis, IConnectionMultiplexer multiplexer, ILoadMoreMatches loadMoreMatchesService, IFetchMaxElo fetchMaxEloService, IRazorViewEngine razorViewEngine)
        {
            _random = new Random();
            _clientFactory = clientFactory;
            _memoryCache = cache;
            _logger = logger;
            _configuration = configuration;
            _redis = redis;
            _getTotalEloRetrievesCountFromRedis = getTotalEloRetrievesCountFromRedis;
            _multiplexer = multiplexer;
            _loadMoreMatchesService = loadMoreMatchesService;
            _razorViewEngine = razorViewEngine;
            _fetchMaxEloService = fetchMaxEloService;
        }

        public async Task<ActionResult> PlayerStats(string nickname)
        {
            var client = _clientFactory.CreateClient("Faceit");
            var client2 = _clientFactory.CreateClient("FaceitV1");

            PlayerStats.Rootobject playerinf;
            MatchHistory.Rootobject matchhistory;
            List<MatchStats.Round> matchstats = new List<MatchStats.Round>();
            OverallPlayerStats.Rootobject overallplayerstats;
            List<EloDiff.Root> eloDiff;
            List<EloDiff.Root> allhistory;
            List<MatchType.Rootobject> matchtype;
            NicknameBySteamId.Rootobject NicknameBySteamID;
            NicknameBySteamId.Rootobject NicknameByFaceitID;

            long RedisEloRetrievesCount = 0;
            string errorString;
            string game = "cs2";


            if (!string.IsNullOrEmpty(nickname) && (nickname.Contains("https://steamcommunity.com/profiles/") || nickname.Contains("https://steamcommunity.com/id/")))
            {
                try
                {
                    string SteamID = "";

                    if (nickname.Contains("https://steamcommunity.com/profiles/"))
                    {
                        SteamID = ExtractSteamId(nickname);
                    } else if (nickname.Contains("https://steamcommunity.com/id/")) 
                    {
                        var SteamCustomID = ExtractIdFromUrl(nickname);

                        SteamID = await ResolveVanityURL(SteamCustomID);

                    }

                    var cacheKey = $"nickname_{SteamID}";

                    if (!_memoryCache.TryGetValue(cacheKey, out NicknameBySteamID))
                    {
                        try
                        {
                            NicknameBySteamID = await client.GetFromJsonAsync<NicknameBySteamId.Rootobject>($"v4/players?game_player_id={SteamID}&game=cs2");

                        }
                        catch {
                            NicknameBySteamID = await client.GetFromJsonAsync<NicknameBySteamId.Rootobject>($"v4/players?game_player_id={SteamID}&game=csgo");
                        }

                        if (NicknameBySteamID is not null)
                        {
                            _memoryCache.Set(cacheKey, NicknameBySteamID, TimeSpan.FromMinutes(3));
                            nickname = NicknameBySteamID.nickname;
                        }
                    }
                    else
                    {
                        nickname = NicknameBySteamID.nickname;
                    }
                }
                catch
                {
                    return View("~/Views/PlayerNotFoundBySteamID/PlayerNotFoundBySteamID.cshtml");
                }
            }

            if (!string.IsNullOrEmpty(nickname) && !nickname.Contains("https://steamcommunity.com/profiles/") && IsValidUUID(nickname))
            {
                try
                {
                    var FaceitID = nickname;
                    var cacheKey = $"nickname_{FaceitID}";

                    if (!_memoryCache.TryGetValue(cacheKey, out NicknameByFaceitID))
                    {
                        try
                        {
                            NicknameByFaceitID = await client.GetFromJsonAsync<NicknameBySteamId.Rootobject>($"v4/players/{FaceitID}");
                        }
                        catch
                        {
                            return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
                        }

                        if (NicknameByFaceitID is not null)
                        {

                            _memoryCache.Set(cacheKey, NicknameByFaceitID, TimeSpan.FromMinutes(3));
                            nickname = NicknameByFaceitID.nickname;
                        }
                    }
                    else
                    {
                        nickname = NicknameByFaceitID.nickname;
                    }
                }
                catch
                {
                    return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
                }
            }

            try
            {
                Stopwatch z = new Stopwatch();
                z.Start();
                if (!_memoryCache.TryGetValue(nickname, out playerinf))
                {
                    playerinf = await client.GetFromJsonAsync<PlayerStats.Rootobject>($"v4/players?nickname={nickname}");
                    _memoryCache.Set(nickname, playerinf, TimeSpan.FromMinutes(3));
                    playerid = playerinf.player_id;
                }

                var matchhistoryTask = client.GetFromJsonAsync<MatchHistory.Rootobject>(
                    $"v4/players/{playerinf.player_id}/history?game=cs2&from=120&offset=0&limit=20");


                var MatchhHistoryTaskResult = await matchhistoryTask;

                if (MatchhHistoryTaskResult.items.Count() == 0)
                {
                    matchhistoryTask = client.GetFromJsonAsync<MatchHistory.Rootobject>(
                        $"v4/players/{playerinf.player_id}/history?game=csgo&from=120&offset=0&limit=20");
                    game = "csgo";

                }

                var overallplayerstatsTask = client.GetFromJsonAsync<OverallPlayerStats.Rootobject>(
                    $"v4/players/{playerinf.player_id}/stats/cs2");

                try
                {
                    var overallplayerstatsTaskResult = await overallplayerstatsTask;

                    if (overallplayerstatsTaskResult.segments.Count() == 0)
                    {

                        throw new Exception("No cs2 Matches");
                    }
                }
                catch
                {

                    overallplayerstatsTask = client.GetFromJsonAsync<OverallPlayerStats.Rootobject>(
                       $"v4/players/{playerinf.player_id}/stats/csgo");

                }

                var eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                    $"v1/stats/time/users/{playerinf.player_id}/games/cs2?page=0&size=31");

                var eloDiffTaskResult = await eloDiffTask;

                if (eloDiffTaskResult.Count() == 0)
                {
                    eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                       $"v1/stats/time/users/{playerinf.player_id}/games/csgo?page=0&size=31");
                }

                matchhistory = matchhistoryTask.Result!;
                overallplayerstats = overallplayerstatsTask.Result!;
                eloDiff = eloDiffTask.Result!;

                var matchstatsCacheKey = $"{nickname}_matchstats";

                if (!_memoryCache.TryGetValue(matchstatsCacheKey, out List<MatchStats.Round> cachedMatchStats))
                {

                    try
                    {
                        var task = matchhistory.items.Select(async match =>
                        {

                            try
                            {
                                // Fetch data from v4/matches/{match.match_id}
                                var matchResponse = await client.GetAsync($"v4/matches/{match.match_id}");
                                matchResponse.EnsureSuccessStatusCode();


                                var matchData = await matchResponse.Content.ReadFromJsonAsync<MatchType.Rootobject>();
                                var calculateElo = matchData?.calculate_elo ?? false;

                                // Fetch data from v4/matches/{match.match_id}/stats
                                var statsResponse = await client.GetAsync($"v4/matches/{match.match_id}/stats");
                                statsResponse.EnsureSuccessStatusCode();

                                var matchStats = await statsResponse.Content.ReadFromJsonAsync<MatchStats.Rootobject>();

                                // Set the calculate_elo property based on the fetched data
                                if (matchStats != null)
                                {
                                    foreach (var round in matchStats.rounds)
                                    {
                                        round.calculate_elo = calculateElo;
                                        round.competition_name = matchData?.competition_name;
                                        round.match_id = matchData?.match_id;
                                        round.finished_at = matchData.finished_at;
                                    }
                                }

                                return matchStats;

                            }

                            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                            {
                                Console.WriteLine($"Match ID {match.match_id} not found. Skipping.");

                                return new MatchStats.Rootobject
                                {
                                    rounds = new MatchStats.Round[1]
                                    {
                                            new MatchStats.Round
                                            {
                                                competition_id ="",
                                                competition_name = match.competition_name,
                                                best_of = "Walkover",
                                                game_id = "",
                                                game_mode = "",
                                                match_id = match.match_id,
                                                match_round = "",
                                                played = "",
                                                round_stats = null,
                                                teams = null,
                                                elo = ""
                                            }
                                    }
                                };
                            }

                        }).ToList();

                        var results = await Task.WhenAll(task);

                        int offset = 20;

                        int WalkoverCount = results.Count(matchStats =>
                            matchStats?.rounds.Any(round => round?.best_of == "Walkover") ?? false);

                        // Fetch additional non-Walkover matches
                        if (WalkoverCount > 0)
                        {
                            List<MatchStats.Round> AdditionalMatchesList = new List<MatchStats.Round>();

                            var AdditionalMatches = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                                $"v4/players/{playerinf.player_id}/history?game=cs2&from=120&offset={offset}&limit=10");

                            if (AdditionalMatches != null)
                            {

                                var AddMatches = AdditionalMatches.items.Select(async match =>
                                {
                                    try
                                    {
                                        // Fetch data from v4/matches/{match.match_id}
                                        var matchResponse = await client.GetAsync($"v4/matches/{match.match_id}");
                                        matchResponse.EnsureSuccessStatusCode();


                                        var matchData = await matchResponse.Content.ReadFromJsonAsync<MatchType.Rootobject>();
                                        var calculateElo = matchData?.calculate_elo ?? false;

                                        // Fetch data from v4/matches/{match.match_id}/stats
                                        var statsResponse = await client.GetAsync($"v4/matches/{match.match_id}/stats");
                                        statsResponse.EnsureSuccessStatusCode();

                                        var matchStats = await statsResponse.Content.ReadFromJsonAsync<MatchStats.Rootobject>();

                                        // Set the calculate_elo property based on the fetched data
                                        if (matchStats != null)
                                        {
                                            foreach (var round in matchStats.rounds)
                                            {
                                                round.calculate_elo = calculateElo;
                                                round.competition_name = matchData?.competition_name;
                                                round.match_id = matchData?.match_id;
                                                round.finished_at = matchData.finished_at;
                                            }
                                        }

                                        return matchStats;
                                    }

                                    catch
                                    {
                                        return null;
                                    }

                                }).ToList();

                                var SecondResults = await Task.WhenAll(AddMatches);

                                var FinalAdditionList = SecondResults
                                    .Where(matchStats => matchStats != null)
                                    .SelectMany(matchStats => matchStats.rounds)
                                    .Where(round => round?.best_of != "Walkover")
                                    .Take(WalkoverCount);

                                // Create a new MatchStats.Rootobject to encapsulate FinalAdditionList
                                var finalAdditionStats = new MatchStats.Rootobject
                                {
                                    rounds = FinalAdditionList.ToArray() // Assuming rounds is an array property in MatchStats.Rootobject
                                };

                                // Add finalAdditionStats to the results list
                                results = results.Concat(new[] { finalAdditionStats }).ToArray();

                            }
                        }


                        matchstats.AddRange(results.Where(x => x is not null).SelectMany(x => x!.rounds));
                    }
                    catch
                    {
                        Console.WriteLine("Blad");
                    }
                }
                else
                {
                    matchstats = cachedMatchStats;
                }

                errorString = null;

                z.Stop();
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(z.ElapsedMilliseconds);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                errorString = $"Error: {ex.Message}";
                playerinf = null;
                matchhistory = null;
                overallplayerstats = null;
                matchstats = null;
                eloDiff = null;
                allhistory = null;
            }

            if (playerinf is null)
            {
                return RedirectToAction("PlayerNotFound");
            }

            var ConnectionStatus = new PlayerStats
            {
                RedisEloRetrievesCount = RedisEloRetrievesCount,
                OverallPlayerStatsInfo = overallplayerstats,
                Last20MatchesStats = matchstats,
                MatchHistory = matchhistory,
                Playerinfo = playerinf,
                EloDiff = eloDiff,
                currentModel = eloDiff,
                Game = game,
                ErrorMessage = errorString,
            };

            ViewData["PlayerStats"] = false;

            return View(ConnectionStatus);
        }

        public IActionResult PlayerNotFound()
        {
            return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> LoadMoreMatches([FromBody] LoadMoreMatchesRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request data is null.");
            }

            _logger.LogInformation($"Received CsGoSwap HAGSADGHSDGAUIDSOU: {request.CsGoSwap}");

            // Use request.currentModel directly
            var viewModel = await _loadMoreMatchesService.LoadMoreMatches(
                request.nickname,
                request.offset,
                request.playerID,
                request.isOffsetModificated,
                request.QuantityOfEloRetrieves,
                request.currentModel,
                request.currentPage,
                request.CsGoSwap,
                request.Game
            ) ;

            // Render the partial view to a string
            var partialViewHtml = RenderPartialViewToString("MatchListPartial", viewModel);

            _logger.LogInformation($"Received game HAGSADGHSDGAUIDSOU: {viewModel.Game}");
            _logger.LogInformation($"Received CsGoSwap2222 HAGSADGHSDGAUIDSOU: {viewModel.CsGoSwap}");

            // Return JSON with both the HTML and the new EloDiff data
            return Json(new
            {
                partialViewHtml = partialViewHtml,
                newEloDiff = viewModel.EloDiff,
                currentPage = viewModel.currentPage,
                game = viewModel.Game,
                csGoSwap = viewModel.CsGoSwap
            });
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new System.IO.StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View {viewName} not found.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                return sw.GetStringBuilder().ToString();
            }
        }


        [HttpGet("/FetchMaxElo")]
        public async Task<IActionResult> FetchMaxElo(string playerId)
        {
            try
            {
                var highestEloData = await _fetchMaxEloService.FetchMaxEloAsync(playerId);
                return Ok(highestEloData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        public string ExtractSteamId(string url)
        {
            string pattern = @"https:\/\/steamcommunity\.com\/profiles\/(\d+)";

            Regex regex = new Regex(pattern);

            Match match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }


        public string ExtractIdFromUrl(string url)
        {
            string pattern = @"https:\/\/steamcommunity\.com\/id\/([^\/]+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        private bool IsValidUUID(string uuid)
        {
            // Regular expression to match a v4 UUID format
            var regex = new Regex(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[4][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$");
            return regex.IsMatch(uuid);
        }

        public async Task<string> ResolveVanityURL(string vanityId)
        {
            var apiKey = _configuration["SteamAPIKey"]; 
            var client = _clientFactory.CreateClient();  
            var response = await client.GetFromJsonAsync<ResponeFromSteamForCustomSteamID.Rootobject>($"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1/?key={apiKey}&vanityurl={vanityId}");

            if (response != null && response.response.success == 1)
            {
                return response.response.steamid;
            }

            return null;
        }

    }

}
