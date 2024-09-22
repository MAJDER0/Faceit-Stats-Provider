using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MatchType = Faceit_Stats_Provider.Models.MatchType;

namespace Faceit_Stats_Provider.Services
{
    public class LoadMoreMatchesService : ILoadMoreMatches
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoadMoreMatchesService> _logger;

        public LoadMoreMatchesService(IHttpClientFactory clientFactory, IMemoryCache memoryCache, IConfiguration configuration, ILogger<LoadMoreMatchesService> logger)
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<MatchHistoryWithStatsViewModel> LoadMoreMatches(string nickname, int offset, string playerID, bool isOffsetModificated, int QuantityOfEloRetrieves = 10, List<EloDiff.Root> currentModel = null,int currentPage=0, int CsGoSwap = 0,string Game = "cs2")
        {

            int limit = 10;
            int page = 0;
            string game = Game;
            string StaticPlayerid = "";

            if (offset >= 100)
            {
                page = (offset / 100);
            }

            if (offset % 2 == 0)
            {
                limit = LookForBiggestDivider(offset);
            }

            static int LookForBiggestDivider(int number)
            {
                for (int divisor = Math.Min(number / 2, 14); divisor >= 2; divisor--)
                {
                    if (number % divisor == 0)
                    {
                        return divisor;
                    }
                }
                return 2;
            }

            try
            {
                var client = _clientFactory.CreateClient("Faceit");
                var client2 = _clientFactory.CreateClient("FaceitV1");

                if (!_memoryCache.TryGetValue(nickname, out StaticPlayerid))
                {
                    var x = await client.GetFromJsonAsync<PlayerStats.Rootobject>($"v4/players?nickname={nickname}");
                    _memoryCache.Set(nickname, x.nickname, TimeSpan.FromMinutes(5));
                    StaticPlayerid = x.nickname;
                }

                var playerinf = await client.GetFromJsonAsync<PlayerStats.Rootobject>($"v4/players?nickname={nickname}");

                var matchhistory = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                    $"v4/players/{playerID}/history?game={game}&from=1200&offset={offset}&limit={limit}");

                if (matchhistory?.items == null || matchhistory.items.Length == 0)
                {
                    game = "csgo";
                    if (CsGoSwap==0)
                    {
                        offset = 0;
                        limit = 10;
                        CsGoSwap = 1;
                    }
                    matchhistory = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                        $"v4/players/{playerID}/history?game={game}&from=1200&offset={offset}&limit={limit}");
                }

                if (isOffsetModificated)
                {
                    var additionalMatch = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                        $"v4/players/{playerID}/history?game={game}&from=1200&offset={offset}&limit={limit}");

                    if (additionalMatch?.items != null && additionalMatch.items.Any())
                    {
                        matchhistory.items = matchhistory.items.Concat(additionalMatch.items).ToArray();
                    }
                }

                if (matchhistory?.items != null && matchhistory.items.Length > 0)
                {
                    var matchStatsList = new List<MatchStats.Rootobject>();

                    var matchStatsTasks = matchhistory.items.Select(async match =>
                    {
                        try
                        {
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
                                competition_id = "",
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
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"Error fetching MatchStats for match {match.match_id}: {innerEx.Message}");
                            throw;
                        }
                    }).ToList();

                    var eloDiffTasks = new List<Task<List<EloDiff.Root>>>();
                    Task<List<EloDiff.Root>>? eloDiffTask = Task.FromResult(new List<EloDiff.Root>());

                    Task<List<EloDiff.Root>>? eloDiffTaskCheck = null;
                    Task<List<EloDiff.Root>>? eloDiffTaskCs2Check = null;

                    List<EloDiff.Root> eloDiffTaskResult = new List<EloDiff.Root>();

                    // If the game is not "csgo", fetch data for "csgo" on page 0
                    if (game != "csgo")
                    {
                        // Fetch "csgo" data as well, if game is not "csgo"
                        try
                        {
                            eloDiffTaskCs2Check = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                                $"v1/stats/time/users/{playerID}/games/cs2?page={page + 1}&size=100");
                        }
                        catch {
                            eloDiffTaskCs2Check = null;
                        }
                    }

                    if (currentPage != page || eloDiffTaskCs2Check is null || eloDiffTaskCs2Check.Result.Count() == 0)
                    {
                        if (currentPage != page)
                        {
                            // Fetch eloDiffTask for the specified page and game
                            eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                               $"v1/stats/time/users/{playerID}/games/{game}?page={page}&size=100");

                            currentPage = page;
                        }

                        // Await the result of eloDiffTask to access its list
                        if (eloDiffTask is not null)
                        {
                            eloDiffTaskResult = await eloDiffTask;
                        }

                        // Only add from eloDiffTaskCheck if it is not null and if there is data to add
                        if (eloDiffTaskCs2Check is not null && eloDiffTaskCs2Check.Result.Count() == 0)
                        {
                            eloDiffTaskCheck = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                                $"v1/stats/time/users/{playerID}/games/csgo?page=0&size=100");

                            currentPage = -1;
                            // Await the eloDiffTaskCheck task and add its result to eloDiffTaskResult
                            var csgoResult = await eloDiffTaskCheck;
                            eloDiffTaskResult.AddRange(csgoResult);
                        }

                        // Add the original eloDiffTask to the list of tasks
                        eloDiffTasks.Add(eloDiffTask);

                        // Change console color to DarkMagenta
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;

                        // Print "DONE! DONE!" 40 times
                        for (int i = 0; i < 40; i++)
                        {
                            Console.WriteLine("DONE! DONE!");
                        }

                        // Reset console color back to default
                        Console.ResetColor();
                    }

                    // Await all eloDiff tasks concurrently
                    var eloDiffResults = await Task.WhenAll(eloDiffTasks);

                    // Initialize eloDiff with currentModel if provided, otherwise with an empty list
                    var eloDiff = currentModel ?? new List<EloDiff.Root>();

                    // Combine results from all eloDiff tasks
                    foreach (var eloDiffList in eloDiffResults)
                    {
                        if (eloDiffList != null)
                        {
                            eloDiff.AddRange(eloDiffList); // Concatenate the new results to currentModel
                        }
                    }

                    // Await match stats tasks
                    var matchStatsResults = await Task.WhenAll(matchStatsTasks);
                    matchStatsList.AddRange(matchStatsResults);

                    var viewModel = new MatchHistoryWithStatsViewModel
                    {
                        Playerinfo = playerinf,
                        MatchHistoryItems = matchhistory.items.ToList(),
                        MatchStats = matchStatsList,
                        EloDiff = eloDiff, // Updated EloDiff list
                        currentPage = currentPage, // Updated EloDiff list
                        Game = game,
                        CsGoSwap = CsGoSwap
                    };

                    return viewModel;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching more matches: {ex.Message}");
            }

            return new MatchHistoryWithStatsViewModel();
        }

    }
}
