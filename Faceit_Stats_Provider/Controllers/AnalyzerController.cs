using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Models;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Controllers
{
    public class AnalyzerController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AnalyzerController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return View("~/Views/Analyzer/AnalyzerSearch.cshtml");
        }

        [HttpGet]
        public async Task<ActionResult> Analyze(string roomId)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                return BadRequest("URL parameter is required.");
            }

            var client = _clientFactory.CreateClient("Faceit");

            string RoomID = ExtractRoomIdFromUrl(roomId);

            AnalyzerMatchPlayers.Rootobject players;
            var getPlayerStatsTasks = new List<Task<AnalyzerPlayerStats.Rootobject>>();
            var getPlayerMatchHistoryTasks = new List<(string playerId, Task<AnalyzerMatchHistory.Rootobject>)>();
            var getPlayerMatchStatsTasks = new List<(string playerId, Task<AnalyzerMatchStats.Rootobject>)>();

            try
            {
                players = await client.GetFromJsonAsync<AnalyzerMatchPlayers.Rootobject>($"v4/matches/{RoomID}");

                foreach (var item in players.teams.faction1.roster)
                {
                    getPlayerStatsTasks.Add(client.GetFromJsonAsync<AnalyzerPlayerStats.Rootobject>($"v4/players/{item.player_id}/stats/cs2"));
                    getPlayerMatchHistoryTasks.Add((item.player_id, client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=cs2&from=120&offset=0&limit=20")));
                }

                foreach (var item in players.teams.faction2.roster)
                {
                    getPlayerStatsTasks.Add(client.GetFromJsonAsync<AnalyzerPlayerStats.Rootobject>($"v4/players/{item.player_id}/stats/cs2"));
                    getPlayerMatchHistoryTasks.Add((item.player_id, client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=cs2&from=120&offset=0&limit=20")));
                }

                // Await all tasks concurrently
                await Task.WhenAll(getPlayerStatsTasks);
                await Task.WhenAll(getPlayerMatchHistoryTasks.Select(t => t.Item2));

                // Retrieve results from tasks
                var playerStats = getPlayerStatsTasks.Select(t => t.Result).ToList();
                var playerMatchHistory = getPlayerMatchHistoryTasks.Select(t => (t.playerId, t.Item2.Result)).ToList();

                foreach (var (playerId, playerHistory) in playerMatchHistory)
                {
                    foreach (var matchItem in playerHistory.items)
                    {
                        getPlayerMatchStatsTasks.Add((playerId, client.GetFromJsonAsync<AnalyzerMatchStats.Rootobject>($"v4/matches/{matchItem.match_id}/stats")));
                    }
                }


                var playerMatchStatsResults = await Task.WhenAll(getPlayerMatchStatsTasks.Select(task => HandleHttpRequestAsync(task.Item2).ContinueWith(t => (task.playerId, Result: t.Result))));
                var playerMatchStats = playerMatchStatsResults.Where(result => result.Result != null).ToList();


                var viewModel = new AnalyzerViewModel
                {
                    RoomId = RoomID,
                    Players = players,
                    PlayerStats = playerStats,
                    PlayerMatchStats = playerMatchStats
                };

                return View("~/Views/Analyzer/Analyze.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string ExtractRoomIdFromUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                string[] segments = uri.Segments;
                return segments.Length > 0 ? segments[segments.Length - 1].Trim('/') : null;
            }
            return null;
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
    }
}
