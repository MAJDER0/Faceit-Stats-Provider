using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Models;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Faceit_Stats_Provider.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
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

            AnalyzerMatchPlayers.Rootobject players;

            List<AnalyzerPlayerStats.Rootobject> playerStats = new List<AnalyzerPlayerStats.Rootobject>();

            string RoomID = ExtractRoomIdFromUrl(roomId);

            try
            {
                 players = await client.GetFromJsonAsync<AnalyzerMatchPlayers.Rootobject>(
                     $"v4/matches/{RoomID}");

                foreach (var item in players.teams.faction1.roster)
                {
                    playerStats.Add(await client.GetFromJsonAsync<AnalyzerPlayerStats.Rootobject>($"v4/players/{item.player_id}/stats/cs2"));
                }

                foreach (var item in players.teams.faction2.roster)
                {
                    playerStats.Add(await client.GetFromJsonAsync<AnalyzerPlayerStats.Rootobject>($"v4/players/{item.player_id}/stats/cs2"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            var viewModel = new AnalyzerViewModel
            {
                RoomId = RoomID,
                Players = players ,
                PlayerStats = playerStats
            };

            return View("~/Views/Analyzer/Analyze.cshtml", viewModel);
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
    }
}
