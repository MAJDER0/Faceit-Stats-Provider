using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Models;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Controllers
{
    public class AnalyzerController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public AnalyzerController(IHttpClientFactory clientFactory, IMemoryCache memoryCache)
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            return View("~/Views/Analyzer/AnalyzerSearch.cshtml");
        }

        public IActionResult InvalidMatchRoomLink()
        {
            return View("~/Views/InvalidMatchRoomLink/InvalidMatchRoomLink.cshtml");
        }


        [HttpGet]
        public async Task<ActionResult> Analyze(string roomId)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                return RedirectToAction("InvalidMatchRoomLink", "Analyzer");
            }

            var client = _clientFactory.CreateClient("Faceit");
            string RoomID = ExtractRoomIdFromUrl(roomId);

            AnalyzerMatchPlayers.Rootobject players;
            var getPlayerStatsTasks = new List<Task<AnalyzerPlayerStats.Rootobject>>();
            var getPlayerStatsForCsGoTasks = new List<Task<AnalyzerPlayerStatsForCsgo.Rootobject>>();
            var getPlayerMatchHistoryTasks = new List<(string playerId, Task<AnalyzerMatchHistory.Rootobject>)>();
            var getPlayerMatchStatsTasks = new List<(string playerId, Task<AnalyzerMatchStats.Rootobject>)>();

            try
            {
                players = await GetOrAddToCacheAsync($"players_{RoomID}", () => client.GetFromJsonAsync<AnalyzerMatchPlayers.Rootobject>($"v4/matches/{RoomID}"));

                if (players.teams.faction1.roster is null || players.teams.faction2.roster is null)
                {
                    AnalyzerMatchPlayersOldMatch.Rootobject oldMatch = await client.GetFromJsonAsync<AnalyzerMatchPlayersOldMatch.Rootobject>($"v4/matches/{RoomID}");

                     players = CovertOldPlatformMatchForAnalyzer.ConvertOldMatchToNew(oldMatch);
                }

                foreach (var item in players.teams.faction1.roster)
                {
                    try
                    {
                        var cs2stats = await client.GetFromJsonAsync<AnalyzerPlayerStats.Rootobject>($"v4/players/{item.player_id}/stats/cs2");
                        if (cs2stats != null)
                        {
                            getPlayerStatsTasks.Add(Task.FromResult(cs2stats));
                        }
                    }
                    catch
                    {
                        getPlayerStatsTasks.Add(
                            Task.FromResult(new AnalyzerPlayerStats.Rootobject
                            {
                                player_id = item.player_id,
                                lifetime = null,
                                segments = null,
                                game_id = item.game_player_id,
                            })
                        );
                    }
                    try
                    {
                        getPlayerStatsForCsGoTasks.Add(GetOrAddToCacheAsync($"stats_csgo_{item.player_id}", () => client.GetFromJsonAsync<AnalyzerPlayerStatsForCsgo.Rootobject>($"v4/players/{item.player_id}/stats/csgo")));
                    }
                    catch (Exception ex)
                    {
                        // Log the error and handle it by adding a default task
                        Console.WriteLine($"Error fetching stats for player {item.player_id}: {ex.Message}");

                        // Add a default task with a fallback value to the task list
                        getPlayerStatsForCsGoTasks.Add(Task.FromResult(new AnalyzerPlayerStatsForCsgo.Rootobject
                        {
                            player_id = item.player_id,
                            game_id = item.game_player_id,
                            lifetime = null,
                            segments = null
                        }));
                    }

                    try
                    {
                        var cs2GameHistory = await client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=cs2&from=120&offset=0&limit=20");

                        if (cs2GameHistory.items.Count() == 0)
                        {
                            throw new Exception();
                        }

                        if (cs2GameHistory != null && cs2GameHistory.items != null)
                        {
                            getPlayerMatchHistoryTasks.Add((item.player_id, Task.FromResult(cs2GameHistory)));
                        }

                    }
                    catch
                    {
                        getPlayerMatchHistoryTasks.Add((item.player_id, GetOrAddToCacheAsync($"history_cs2_{item.player_id}", () => client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=csgo&from=120&offset=0&limit=20"))));

                    }
                }

                foreach (var item in players.teams.faction2.roster)
                {
                    try
                    {
                        var cs2stats = await client.GetFromJsonAsync<AnalyzerPlayerStats.Rootobject>($"v4/players/{item.player_id}/stats/cs2");
                        if (cs2stats != null)
                        {
                            getPlayerStatsTasks.Add(Task.FromResult(cs2stats));
                        }
                    }
                    catch
                    {
                        getPlayerStatsTasks.Add(
                            Task.FromResult(new AnalyzerPlayerStats.Rootobject
                            {
                                player_id = item.player_id,
                                lifetime = null,
                                segments = null,
                                game_id = item.game_player_id,
                            })
                        );
                    }

                    try
                    {
                        getPlayerStatsForCsGoTasks.Add(GetOrAddToCacheAsync($"stats_csgo_{item.player_id}", () => client.GetFromJsonAsync<AnalyzerPlayerStatsForCsgo.Rootobject>($"v4/players/{item.player_id}/stats/csgo")));
                    }
                    catch (Exception ex)
                    {
                        // Log the error and handle it by adding a default task
                        Console.WriteLine($"Error fetching stats for player {item.player_id}: {ex.Message}");

                        // Add a default task with a fallback value to the task list
                        getPlayerStatsForCsGoTasks.Add(Task.FromResult(new AnalyzerPlayerStatsForCsgo.Rootobject
                        {
                            player_id = item.player_id,
                            game_id = item.game_player_id,
                            lifetime = null,
                            segments = null
                        }));
                    }

                    try
                    {
                        var cs2GameHistory = await client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=cs2&from=120&offset=0&limit=20");

                        if (cs2GameHistory.items.Count() == 0) {
                            throw new Exception();
                        }

                        if (cs2GameHistory != null && cs2GameHistory.items != null)
                        {
                            getPlayerMatchHistoryTasks.Add((item.player_id, Task.FromResult(cs2GameHistory)));
                        }

                    }
                    catch
                    {
                        getPlayerMatchHistoryTasks.Add((item.player_id, GetOrAddToCacheAsync($"history_cs2_{item.player_id}", () => client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=csgo&from=120&offset=0&limit=20"))));

                    }

                }

                // Await all tasks concurrently
                await Task.WhenAll(getPlayerStatsTasks);
                await Task.WhenAll(getPlayerStatsForCsGoTasks);
                await Task.WhenAll(getPlayerMatchHistoryTasks.Select(t => t.Item2));

                // Retrieve results from tasks
                var playerStats = getPlayerStatsTasks.Select(t => t.Result).ToList();
                var playerStatsForCsGo = getPlayerStatsForCsGoTasks.Select(t => t.Result).ToList();

                var playerMatchHistory = getPlayerMatchHistoryTasks.Select(t => (t.playerId, t.Item2.Result)).ToList();

                foreach (var (playerId, playerHistory) in playerMatchHistory)
                {
                    foreach (var matchItem in playerHistory.items)
                    {
                        getPlayerMatchStatsTasks.Add((playerId, GetOrAddToCacheAsync($"match_stats_{matchItem.match_id}", () => client.GetFromJsonAsync<AnalyzerMatchStats.Rootobject>($"v4/matches/{matchItem.match_id}/stats"))));
                    }
                }

                var playerMatchStatsResults = await Task.WhenAll(getPlayerMatchStatsTasks.Select(task => HandleHttpRequestAsync(task.Item2).ContinueWith(t => (task.playerId, Result: t.Result))));
                var playerMatchStats = playerMatchStatsResults.Where(result => result.Result != null).ToList();

                var combinedPlayerStats = playerStats.Zip(playerStatsForCsGo, (cs2, csgo) =>
                {
                    var combinedSegments = new Dictionary<string, AnalyzerPlayerStatsCombined.Segment>();

                    if (cs2?.segments != null)
                    {
                        foreach (var cs2Segment in cs2.segments)
                        {
                            if (!cs2Segment.mode.ToLower().Contains("wingman"))
                            {
                                var normalizedLabel = NormalizeLabel(cs2Segment.label);
                                combinedSegments[normalizedLabel] = ConvertToCombinedSegment(cs2Segment);
                            }
                        }
                    }

                    if (csgo?.segments != null)
                    {
                        var csgoProperMaps = csgo.segments.Where(mode => mode.mode == "5v5").ToList();

                        foreach (var csgoSegment in csgoProperMaps)
                        {
                            if (!csgoSegment.mode.ToLower().Contains("wingman"))
                            {

                                var normalizedLabel = NormalizeLabel(csgoSegment.label);
                                if (combinedSegments.TryGetValue(normalizedLabel, out var existingSegment))
                                {
                                    combinedSegments[normalizedLabel] = CombineSegments(existingSegment, ConvertToCombinedSegment(csgoSegment));
                                }
                                else
                                {
                                    combinedSegments[normalizedLabel] = ConvertToCombinedSegment(csgoSegment);
                                }
                            }
                        }
                    }

                    return new AnalyzerPlayerStatsCombined.Rootobject
                    {
                        player_id = cs2?.player_id ?? csgo?.player_id,
                        game_id = cs2?.game_id ?? csgo?.game_id,
                        lifetime = new AnalyzerPlayerStatsCombined.Lifetime
                        {
                            Wins = cs2?.lifetime?.Wins ?? csgo?.lifetime?.Wins,
                            TotalHeadshots = cs2?.lifetime?.TotalHeadshots ?? csgo?.lifetime?.TotalHeadshots,
                            LongestWinStreak = cs2?.lifetime?.LongestWinStreak ?? csgo?.lifetime?.LongestWinStreak,
                            KDRatio = cs2?.lifetime?.KDRatio ?? csgo?.lifetime?.KDRatio,
                            Matches = cs2?.lifetime?.Matches ?? csgo?.lifetime?.Matches,
                            AverageHeadshots = cs2?.lifetime?.AverageHeadshots ?? csgo?.lifetime?.AverageHeadshots,
                            AverageKDRatio = cs2?.lifetime?.AverageKDRatio ?? csgo?.lifetime?.AverageKDRatio,
                            WinRate = cs2?.lifetime?.WinRate ?? csgo?.lifetime?.WinRate,
                            ExtensionData = cs2?.lifetime?.ExtensionData ?? csgo?.lifetime?.ExtensionData
                        },
                        segments = combinedSegments.Values.ToArray()
                    };
                }).ToList();


                // Create deep copy of the initial model
                var initialViewModel = new AnalyzerViewModel
                {
                    RoomId = RoomID,
                    Players = players,
                    PlayerStats = playerStats,
                    PlayerStatsForCsGo = playerStatsForCsGo,
                    PlayerMatchStats = playerMatchStats,
                    PlayerStatsCombinedViewModel = combinedPlayerStats,
                    IsIncludedCsGoStats = false
                };

                var initialModelCopy = JsonConvert.DeserializeObject<AnalyzerViewModel>(JsonConvert.SerializeObject(initialViewModel));

                var viewModel = new AnalyzerViewModel
                {
                    RoomId = RoomID,
                    Players = players,
                    PlayerStats = playerStats,
                    PlayerStatsForCsGo = playerStatsForCsGo,
                    PlayerMatchStats = playerMatchStats,
                    PlayerStatsCombinedViewModel = combinedPlayerStats,
                    IsIncludedCsGoStats = false,
                    InitialModelCopy = ModelMapper.ToExcludePlayerModel(initialModelCopy)
                };

                return View("~/Views/Analyzer/Analyze.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("InvalidMatchRoomLink", "Analyzer");
            }
        }

        [HttpPost("/OnlyCsGoStats")]
        public IActionResult OnlyCsGoStats([FromBody] object request)
        {
            // Convert the object to a JSON string
            var jsonString = request.ToString();

            // Deserialize the JSON string into your ToggleIncludeCsGoStatsRequest model
            var toggleRequest = JsonConvert.DeserializeObject<OnlyCsGoStatsRequest>(jsonString);

            if (toggleRequest == null)
            {
                return BadRequest("Request is null");
            }

            // Validate the request data
            if (string.IsNullOrEmpty(toggleRequest.RoomId) || toggleRequest.Players == null || toggleRequest.PlayerMatchStats == null)
            {
                return BadRequest("Invalid request data");
            }

            AnalyzerViewModel viewModel;

            var ConvertCsGoStatsAsPlayerStats = ConvertCsgoToAnalyzerPlayerStats(toggleRequest.PlayerStatsForCsGo);

            if (toggleRequest.IncludeCsGoStats == false && toggleRequest.CsGoStatsOnlyDisplayed == true) {

                viewModel = new AnalyzerViewModel
                {
                    RoomId = toggleRequest.RoomId,
                    Players = toggleRequest.Players,
                    PlayerStats = ConvertCsGoStatsAsPlayerStats,
                    PlayerStatsForCsGo = toggleRequest.PlayerStatsForCsGo,
                    PlayerStatsCombinedViewModel = toggleRequest.PlayerStatsCombinedViewModel,
                    PlayerMatchStats = toggleRequest.PlayerMatchStats
                          .Select(pms => (pms.playerId, pms.matchStats))
                          .ToList(),
                    InitialModelCopy = toggleRequest.InitialModelCopy,
                    IsIncludedCsGoStats = true,
                    CsGoStatsOnlyDisplayed = true,
                };
            }
            else
            {
                var initialModelCopy = toggleRequest.InitialModelCopy;
                viewModel = new AnalyzerViewModel
                {
                    RoomId = initialModelCopy.RoomId,
                    Players = initialModelCopy.Players,
                    PlayerStats = initialModelCopy.PlayerStats,
                    PlayerStatsForCsGo = initialModelCopy.PlayerStatsForCsGo,
                    PlayerStatsCombinedViewModel = initialModelCopy.PlayerStatsCombinedViewModel,
                    PlayerMatchStats = initialModelCopy.PlayerMatchStats
                        .Select(pms => (pms.playerId, pms.matchStats))
                        .ToList(),
                    InitialModelCopy = initialModelCopy.InitialModelCopy,
                    IsIncludedCsGoStats = false,
                    CsGoStatsOnlyDisplayed = false,
                };
            }

            // Apply the excluded players logic
            if (toggleRequest.ExcludedPlayers != null && toggleRequest.ExcludedPlayers.Any())
            {
                viewModel.Players.teams.faction1.roster = viewModel.Players.teams.faction1.roster.Where(p => !toggleRequest.ExcludedPlayers.Contains(p.player_id)).ToArray();
                viewModel.Players.teams.faction2.roster = viewModel.Players.teams.faction2.roster.Where(p => !toggleRequest.ExcludedPlayers.Contains(p.player_id)).ToArray();
                viewModel.PlayerStats = viewModel.PlayerStats.Where(ps => !toggleRequest.ExcludedPlayers.Contains(ps.player_id)).ToList();
                viewModel.PlayerMatchStats = viewModel.PlayerMatchStats.Where(pms => !toggleRequest.ExcludedPlayers.Contains(pms.playerId)).ToList();
            }

            var partialViewModel = new AnalyzerPartialViewModel
            {
                ModifiedViewModel = viewModel,
                OriginalViewModel = ModelMapper.ToAnalyzerViewModel(toggleRequest.InitialModelCopy) // Use mapping function
            };

            return PartialView("_StatisticsPartial", partialViewModel);

        }

            [HttpPost("/ToggleIncludeCsGoStats")]
        public IActionResult ToggleIncludeCsGoStats([FromBody] object request)
        {
            // Convert the object to a JSON string
            var jsonString = request.ToString();

            // Deserialize the JSON string into your ToggleIncludeCsGoStatsRequest model
            var toggleRequest = JsonConvert.DeserializeObject<ToggleIncludeCsGoStatsRequest>(jsonString);

            // Now you can use the toggleRequest object
            if (toggleRequest == null)
            {
                return BadRequest("Request is null");
            }

            // Validate the request data
            if (string.IsNullOrEmpty(toggleRequest.RoomId) || toggleRequest.Players == null || toggleRequest.PlayerMatchStats == null)
            {
                return BadRequest("Invalid request data");
            }

            AnalyzerViewModel viewModel;

            if (toggleRequest.IncludeCsGoStats)
            {
                var convertedCombinedStatsToMatchModel = ConvertCombinedToPlayerStats(toggleRequest.PlayerStatsCombinedViewModel);

                viewModel = new AnalyzerViewModel
                {
                    RoomId = toggleRequest.RoomId,
                    Players = toggleRequest.Players,
                    PlayerStats = convertedCombinedStatsToMatchModel,
                    PlayerStatsForCsGo = toggleRequest.PlayerStatsForCsGo,
                    PlayerStatsCombinedViewModel = toggleRequest.PlayerStatsCombinedViewModel,
                    PlayerMatchStats = toggleRequest.PlayerMatchStats
                        .Select(pms => (pms.playerId, pms.matchStats))
                        .ToList(),
                    InitialModelCopy = toggleRequest.InitialModelCopy,
                    IsIncludedCsGoStats = true,
                    CsGoStatsOnlyDisplayed = false
                };
            }
            else
            {
                var initialModelCopy = toggleRequest.InitialModelCopy;
                viewModel = new AnalyzerViewModel
                {
                    RoomId = initialModelCopy.RoomId,
                    Players = initialModelCopy.Players,
                    PlayerStats = initialModelCopy.PlayerStats,
                    PlayerStatsForCsGo = initialModelCopy.PlayerStatsForCsGo,
                    PlayerStatsCombinedViewModel = initialModelCopy.PlayerStatsCombinedViewModel,
                    PlayerMatchStats = initialModelCopy.PlayerMatchStats
                        .Select(pms => (pms.playerId, pms.matchStats))
                        .ToList(),
                    InitialModelCopy = initialModelCopy.InitialModelCopy,
                    IsIncludedCsGoStats = false,
                    CsGoStatsOnlyDisplayed = false,
                };
            }

            // Apply the excluded players logic
            if (toggleRequest.ExcludedPlayers != null && toggleRequest.ExcludedPlayers.Any())
            {
                viewModel.Players.teams.faction1.roster = viewModel.Players.teams.faction1.roster.Where(p => !toggleRequest.ExcludedPlayers.Contains(p.player_id)).ToArray();
                viewModel.Players.teams.faction2.roster = viewModel.Players.teams.faction2.roster.Where(p => !toggleRequest.ExcludedPlayers.Contains(p.player_id)).ToArray();
                viewModel.PlayerStats = viewModel.PlayerStats.Where(ps => !toggleRequest.ExcludedPlayers.Contains(ps.player_id)).ToList();
                viewModel.PlayerMatchStats = viewModel.PlayerMatchStats.Where(pms => !toggleRequest.ExcludedPlayers.Contains(pms.playerId)).ToList();
            }

            var partialViewModel = new AnalyzerPartialViewModel
            {
                ModifiedViewModel = viewModel,
                OriginalViewModel = ModelMapper.ToAnalyzerViewModel(toggleRequest.InitialModelCopy) // Use mapping function
            };

            // Return the updated partial view
            return PartialView("_StatisticsPartial", partialViewModel);
        }

        [HttpPost]
        public ActionResult TogglePlayer([FromBody] object data)
        {
            var jsonString = data.ToString();
            var model = JsonConvert.DeserializeObject<ExcludePlayerModel>(jsonString);

            if (model == null || model.Players == null || model.PlayerStats == null || model.PlayerMatchStats == null)
            {
                return BadRequest("Invalid data.");
            }

            var initialModelCopy = model.InitialModelCopy;
            var players = model.Players;
            var excludedPlayerIds = model.ExcludedPlayers;
            var includeCsGoStats = model.IncludeCsGoStats;
            var csGoStatsOnlyDisplayed = model.CsGoStatsOnlyDisplayed; // Ensure this is respected

            List<AnalyzerPlayerStats.Rootobject> playerStats;

            // Respect CS:GO stats-only mode and inclusion state
            if (includeCsGoStats == false && csGoStatsOnlyDisplayed == true)
            {
                // Use CS:GO stats when in CS:GO stats-only mode
                playerStats = ConvertCsgoToAnalyzerPlayerStats(model.PlayerStatsForCsGo);
            }
            else if (includeCsGoStats == true && csGoStatsOnlyDisplayed == false)
            {
                // Use combined stats (CS2 + CS:GO) when including CS:GO stats
                playerStats = ConvertCombinedToPlayerStats(model.PlayerStatsCombinedViewModel);
            }
            else
            {
                // Default to CS2 stats
                playerStats = model.PlayerStats;
            }

            // Filter out excluded players from playerMatchStats
            var playerMatchStats = model.PlayerMatchStats
                .Where(pms => !excludedPlayerIds.Contains(pms.playerId))
                .Select(pms => (pms.playerId, pms.matchStats))
                .ToList();

            if (excludedPlayerIds.Count > 0)
            {
                // Exclude players from the roster
                if (players.teams.faction1?.roster != null)
                {
                    players.teams.faction1.roster = players.teams.faction1.roster
                        .Where(p => !excludedPlayerIds.Contains(p.player_id))
                        .ToArray();
                }

                if (players.teams.faction2?.roster != null)
                {
                    players.teams.faction2.roster = players.teams.faction2.roster
                        .Where(p => !excludedPlayerIds.Contains(p.player_id))
                        .ToArray();
                }

                // Calculate updated statistics after exclusion
                var result = StatsHelper.CalculateNeededStatistics(players.teams.faction1.leader, players.teams.faction2.leader, players.teams.faction1.roster, players.teams.faction2.roster, playerStats, playerMatchStats);
                playerStats = result.Item8.Concat(result.Item9).ToList();
            }
            else
            {
                // When all players are included, ensure CS:GO stats remain displayed if required
                if (includeCsGoStats == false && csGoStatsOnlyDisplayed == true)
                {
                    playerStats = ConvertCsgoToAnalyzerPlayerStats(model.PlayerStatsForCsGo);
                }
            }

            var viewModel = new AnalyzerViewModel
            {
                RoomId = model.RoomId,
                Players = players,
                PlayerStats = playerStats,
                PlayerMatchStats = playerMatchStats,
                IsIncludedCsGoStats = includeCsGoStats,
                CsGoStatsOnlyDisplayed = csGoStatsOnlyDisplayed,
                InitialModelCopy = initialModelCopy
            };

            var partialViewModel = new AnalyzerPartialViewModel
            {
                ModifiedViewModel = viewModel,
                OriginalViewModel = ModelMapper.ToAnalyzerViewModel(initialModelCopy)
            };

            return PartialView("_StatisticsPartial", partialViewModel);
        }



        private string ExtractRoomIdFromUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                string[] segments = uri.Segments;
                // The room ID is always the last segment if there's no additional path, or the second-to-last if there is
                if (segments.Length >= 4 && segments[segments.Length - 2].Equals("room/", StringComparison.OrdinalIgnoreCase))
                {
                    return segments[segments.Length - 1].Trim('/');
                }
                else if (segments.Length > 4)
                {
                    return segments[segments.Length - 2].Trim('/');
                }
            }
            return null;
        }

        private async Task<T> GetOrAddToCacheAsync<T>(string cacheKey, Func<Task<T>> factory)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out T cacheEntry))
            {
                try
                {
                    cacheEntry = await factory(); // Call the API
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Log the specific request that failed with a 404 error
                    Console.WriteLine($"404 error for cache key: {cacheKey}. API endpoint not found.");
                    return default(T); // Return a default value in case of 404
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    Console.WriteLine($"Error occurred while fetching data for cache key: {cacheKey}. Message: {ex.Message}");
                    return default(T); // Return a default value in case of other errors
                }

                // If no exceptions, add the result to the cache
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheDuration
                };
                _memoryCache.Set(cacheKey, cacheEntry, cacheEntryOptions);
            }
            return cacheEntry;
        }


        private async Task<T> HandleHttpRequestAsync<T>(Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (HttpRequestException ex)
            {
                // Log the specific request that failed
                // For example, you can use a logging framework or output to console
                Console.WriteLine($"HTTP request error: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                // Log other exceptions
                Console.WriteLine($"General error: {ex.Message}");
                return default;
            }
        }

        private string NormalizeLabel(string label)
        {
            return label?.ToLowerInvariant().Replace("de_", "").Replace("_", "").Replace("-", "");
        }

        private AnalyzerPlayerStatsCombined.Segment CombineSegments(AnalyzerPlayerStatsCombined.Segment cs2Segment, AnalyzerPlayerStatsCombined.Segment csgoSegment)
        {
            int SafeParse(string value)
            {
                return int.TryParse(value, out int result) ? result : 0;
            }

            // Sum the relevant values
            int totalKills = SafeParse(cs2Segment.stats.Kills) + SafeParse(csgoSegment.stats.Kills);
            int totalMatches = SafeParse(cs2Segment.stats.Matches) + SafeParse(csgoSegment.stats.Matches);
            int totalRounds = SafeParse(cs2Segment.stats.Rounds) + SafeParse(csgoSegment.stats.Rounds);
            int totalDeaths = SafeParse(cs2Segment.stats.Deaths) + SafeParse(csgoSegment.stats.Deaths);
            int totalAssists = SafeParse(cs2Segment.stats.Assists) + SafeParse(csgoSegment.stats.Assists);
            int totalHeadshots = SafeParse(cs2Segment.stats.TotalHeadshots) + SafeParse(csgoSegment.stats.TotalHeadshots);
            int totalWins = SafeParse(cs2Segment.stats.Wins) + SafeParse(csgoSegment.stats.Wins);

            // Calculate derived statistics
            double kdratio = totalDeaths != 0 ? totalKills / (double)totalDeaths : 0;
            double krratio = totalRounds != 0 ? totalKills / (double)totalRounds : 0;
            double winRate = totalMatches != 0 ? (totalWins / (double)totalMatches) * 100 : 0;

            return new AnalyzerPlayerStatsCombined.Segment
            {
                label = cs2Segment.label ?? csgoSegment.label,
                img_small = cs2Segment.img_small ?? csgoSegment.img_small,
                img_regular = cs2Segment.img_regular ?? csgoSegment.img_regular,
                stats = new AnalyzerPlayerStatsCombined.Stats
                {
                    Kills = totalKills.ToString(),
                    AverageHeadshots = totalMatches != 0 ? (totalHeadshots / (double)totalMatches).ToString("F2") : "0",
                    Assists = totalAssists.ToString(),
                    AverageKills = totalMatches != 0 ? (totalKills / (double)totalMatches).ToString("F2") : "0",
                    HeadshotsperMatch = totalMatches != 0 ? (totalHeadshots / (double)totalMatches).ToString("F2") : "0",
                    AverageKRRatio = krratio.ToString("F2"),
                    AverageKDRatio = kdratio.ToString("F2"),
                    Matches = totalMatches.ToString(),
                    WinRate = winRate.ToString("F2"),
                    Rounds = totalRounds.ToString(),
                    TotalHeadshots = totalHeadshots.ToString(),
                    KRRatio = krratio.ToString("F2"),
                    Deaths = totalDeaths.ToString(),
                    KDRatio = kdratio.ToString("F2"),
                    AverageAssists = totalMatches != 0 ? (totalAssists / (double)totalMatches).ToString("F2") : "0",
                    Headshots = totalHeadshots.ToString(),
                    Wins = totalWins.ToString(),
                    AverageDeaths = totalMatches != 0 ? (totalDeaths / (double)totalMatches).ToString("F2") : "0",
                    ExtensionData = cs2Segment.stats.ExtensionData ?? csgoSegment.stats.ExtensionData
                },
                type = cs2Segment.type ?? csgoSegment.type,
                mode = cs2Segment.mode ?? csgoSegment.mode
            };
        }

        private AnalyzerPlayerStatsCombined.Segment ConvertToCombinedSegment(AnalyzerPlayerStats.Segment segment)
        {
            return new AnalyzerPlayerStatsCombined.Segment
            {
                label = segment.label,
                img_small = segment.img_small,
                img_regular = segment.img_regular,
                stats = new AnalyzerPlayerStatsCombined.Stats
                {
                    Kills = segment.stats.Kills,
                    AverageHeadshots = segment.stats.AverageHeadshots,
                    Assists = segment.stats.Assists,
                    AverageKills = segment.stats.AverageKills,
                    HeadshotsperMatch = segment.stats.HeadshotsperMatch,
                    AverageKRRatio = segment.stats.AverageKRRatio,
                    AverageKDRatio = segment.stats.AverageKDRatio,
                    Matches = segment.stats.Matches,
                    WinRate = segment.stats.WinRate,
                    Rounds = segment.stats.Rounds,
                    TotalHeadshots = segment.stats.TotalHeadshots,
                    KRRatio = segment.stats.KRRatio,
                    Deaths = segment.stats.Deaths,
                    KDRatio = segment.stats.KDRatio,
                    AverageAssists = segment.stats.AverageAssists,
                    Headshots = segment.stats.Headshots,
                    Wins = segment.stats.Wins,
                    AverageDeaths = segment.stats.AverageDeaths,
                    ExtensionData = segment.stats.ExtensionData
                },
                type = segment.type,
                mode = segment.mode
            };
        }

        private AnalyzerPlayerStatsCombined.Segment ConvertToCombinedSegment(AnalyzerPlayerStatsForCsgo.Segment segment)
        {
            return new AnalyzerPlayerStatsCombined.Segment
            {
                label = segment.label,
                img_small = segment.img_small,
                img_regular = segment.img_regular,
                stats = new AnalyzerPlayerStatsCombined.Stats
                {
                    Kills = segment.stats.Kills,
                    AverageHeadshots = segment.stats.AverageHeadshots,
                    Assists = segment.stats.Assists,
                    AverageKills = segment.stats.AverageKills,
                    HeadshotsperMatch = segment.stats.HeadshotsperMatch,
                    AverageKRRatio = segment.stats.AverageKRRatio,
                    AverageKDRatio = segment.stats.AverageKDRatio,
                    Matches = segment.stats.Matches,
                    WinRate = segment.stats.WinRate,
                    Rounds = segment.stats.Rounds,
                    TotalHeadshots = segment.stats.TotalHeadshots,
                    KRRatio = segment.stats.KRRatio,
                    Deaths = segment.stats.Deaths,
                    KDRatio = segment.stats.KDRatio,
                    AverageAssists = segment.stats.AverageAssists,
                    Headshots = segment.stats.Headshots,
                    Wins = segment.stats.Wins,
                    AverageDeaths = segment.stats.AverageDeaths,
                    ExtensionData = segment.stats.ExtensionData
                },
                type = segment.type,
                mode = segment.mode
            };
        }

        private List<AnalyzerPlayerStats.Rootobject> ConvertCombinedToPlayerStats(List<AnalyzerPlayerStatsCombined.Rootobject> combinedStats)
        {
            var playerStats = new List<AnalyzerPlayerStats.Rootobject>();

            foreach (var combined in combinedStats)
            {
                var playerStat = new AnalyzerPlayerStats.Rootobject
                {
                    player_id = combined.player_id,
                    game_id = combined.game_id,
                    lifetime = new AnalyzerPlayerStats.Lifetime
                    {
                        Wins = combined.lifetime.Wins?.Replace(",", "."),
                        TotalHeadshots = combined.lifetime.TotalHeadshots?.Replace(",", "."),
                        LongestWinStreak = combined.lifetime.LongestWinStreak?.Replace(",", "."),
                        KDRatio = combined.lifetime.KDRatio?.Replace(",", "."),
                        Matches = combined.lifetime.Matches?.Replace(",", "."),
                        AverageHeadshots = combined.lifetime.AverageHeadshots?.Replace(",", "."),
                        AverageKDRatio = combined.lifetime.AverageKDRatio?.Replace(",", "."),
                        WinRate = combined.lifetime.WinRate?.Replace(",", "."),
                        ExtensionData = combined.lifetime.ExtensionData
                    },
                    segments = combined.segments.Select(seg => new AnalyzerPlayerStats.Segment
                    {
                        label = seg.label,
                        img_small = seg.img_small,
                        img_regular = seg.img_regular,
                        stats = new AnalyzerPlayerStats.Stats
                        {
                            Kills = seg.stats.Kills?.Replace(",", "."),
                            AverageHeadshots = seg.stats.AverageHeadshots?.Replace(",", "."),
                            Assists = seg.stats.Assists?.Replace(",", "."),
                            AverageKills = seg.stats.AverageKills?.Replace(",", "."),
                            HeadshotsperMatch = seg.stats.HeadshotsperMatch?.Replace(",", "."),
                            AverageKRRatio = seg.stats.AverageKRRatio?.Replace(",", "."),
                            AverageKDRatio = seg.stats.AverageKDRatio?.Replace(",", "."),
                            Matches = seg.stats.Matches?.Replace(",", "."),
                            WinRate = seg.stats.WinRate?.Replace(",", "."),
                            Rounds = seg.stats.Rounds?.Replace(",", "."),
                            TotalHeadshots = seg.stats.TotalHeadshots?.Replace(",", "."),
                            KRRatio = seg.stats.KRRatio?.Replace(",", "."),
                            Deaths = seg.stats.Deaths?.Replace(",", "."),
                            KDRatio = seg.stats.KDRatio?.Replace(",", "."),
                            AverageAssists = seg.stats.AverageAssists?.Replace(",", "."),
                            Headshots = seg.stats.Headshots?.Replace(",", "."),
                            Wins = seg.stats.Wins?.Replace(",", "."),
                            AverageDeaths = seg.stats.AverageDeaths?.Replace(",", "."),
                            ExtensionData = seg.stats.ExtensionData
                        },
                        type = seg.type,
                        mode = seg.mode
                    }).ToArray()
                };

                playerStats.Add(playerStat);
            }

            return playerStats;
        }

        private List<AnalyzerPlayerStats.Rootobject> ConvertCsgoToAnalyzerPlayerStats(List<AnalyzerPlayerStatsForCsgo.Rootobject> csgoStatsList)
        {
            if (csgoStatsList == null || !csgoStatsList.Any()) return new List<AnalyzerPlayerStats.Rootobject>();

            return csgoStatsList
      .Where(csgoStats => csgoStats != null)  // Ensure csgoStats is not null
      .Select(csgoStats => new AnalyzerPlayerStats.Rootobject
      {
          player_id = csgoStats.player_id,
          game_id = csgoStats.game_id,
          lifetime = csgoStats.lifetime != null ? new AnalyzerPlayerStats.Lifetime
          {
              Wins = csgoStats.lifetime?.Wins,
              TotalHeadshots = csgoStats.lifetime?.TotalHeadshots,
              LongestWinStreak = csgoStats.lifetime?.LongestWinStreak,
              KDRatio = csgoStats.lifetime?.KDRatio,
              Matches = csgoStats.lifetime?.Matches,
              RecentResults = csgoStats.lifetime?.RecentResults,
              AverageHeadshots = csgoStats.lifetime?.AverageHeadshots,
              AverageKDRatio = csgoStats.lifetime?.AverageKDRatio,
              WinRate = csgoStats.lifetime?.WinRate,
              CurrentWinStreak = csgoStats.lifetime?.CurrentWinStreak,
              ExtensionData = csgoStats.lifetime?.ExtensionData
          } : null,  // Return null for lifetime if csgoStats.lifetime is null

          segments = csgoStats.segments?.Where(segment => segment != null)  // Ensure no null segments
              .Select(segment => new AnalyzerPlayerStats.Segment
              {
                  label = segment.label,
                  img_small = segment.img_small,
                  img_regular = segment.img_regular,
                  stats = segment.stats != null ? new AnalyzerPlayerStats.Stats
                  {
                      Kills = segment.stats.Kills,
                      AverageHeadshots = segment.stats.AverageHeadshots,
                      Assists = segment.stats.Assists,
                      AverageKills = segment.stats.AverageKills,
                      HeadshotsperMatch = segment.stats.HeadshotsperMatch,
                      AverageKRRatio = segment.stats.AverageKRRatio,
                      AverageKDRatio = segment.stats.AverageKDRatio,
                      AverageQuadroKills = segment.stats.AverageQuadroKills,
                      Matches = segment.stats.Matches,
                      WinRate = segment.stats.WinRate,
                      Rounds = segment.stats.Rounds,
                      TotalHeadshots = segment.stats.TotalHeadshots,
                      KRRatio = segment.stats.KRRatio,
                      Deaths = segment.stats.Deaths,
                      KDRatio = segment.stats.KDRatio,
                      AverageAssists = segment.stats.AverageAssists,
                      Headshots = segment.stats.Headshots,
                      Wins = segment.stats.Wins,
                      AverageDeaths = segment.stats.AverageDeaths,
                      ExtensionData = segment.stats.ExtensionData
                  } : null,  // Return null for stats if segment.stats is null
                  type = segment.type,
                  mode = segment.mode
              }).ToArray() ?? Array.Empty<AnalyzerPlayerStats.Segment>()  // Handle null segments array
      }).ToList();

        }
    }

}


