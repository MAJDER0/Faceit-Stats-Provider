using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Models;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using Faceit_Stats_Provider.Services;

namespace Faceit_Stats_Provider.Controllers
{

    public class AnalyzerController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IGetMatchDetails _getMatchDetailsService;
        private readonly IPlayerStatistics _playerStatisticsService;
        private readonly IOnlyCsGoStats _onlyCsGoStatsService;
        private readonly IToggleIncludeCsGoStats _toggleIncludeCsGoStatsService;
        private readonly ITogglePlayer _togglePlayerService;

        public AnalyzerController(
            IHttpClientFactory clientFactory,
            IMemoryCache memoryCache,
            IGetMatchDetails getMatchDetailsService,
            IPlayerStatistics playerStatisticsService,
            IOnlyCsGoStats onlyCsGoStatsService,
            IToggleIncludeCsGoStats toggleIncludeCsGoStatsService,
            ITogglePlayer togglePlayerService)
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _getMatchDetailsService = getMatchDetailsService;
            _playerStatisticsService = playerStatisticsService;
            _onlyCsGoStatsService = onlyCsGoStatsService;
            _toggleIncludeCsGoStatsService = toggleIncludeCsGoStatsService;
            _togglePlayerService = togglePlayerService;
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
            string RoomID = UtilityForAnalyzer.ExtractRoomIdFromUrl(roomId);

            AnalyzerMatchPlayers.Rootobject players;
            var getPlayerStatsTasks = new List<Task<AnalyzerPlayerStats.Rootobject>>();
            var getPlayerStatsForCsGoTasks = new List<Task<AnalyzerPlayerStatsForCsgo.Rootobject>>();
            var getPlayerMatchHistoryTasks = new List<(string playerId, Task<AnalyzerMatchHistory.Rootobject>)>();
            var getPlayerMatchStatsTasks = new List<(string playerId, Task<AnalyzerMatchStats.Rootobject>)>();

            try
            {
                var matchDetails = await _getMatchDetailsService.GetMatchDetailsAsync(RoomID);

                players = matchDetails.Players;
                getPlayerStatsTasks = matchDetails.GetPlayerStatsTasks;
                getPlayerStatsForCsGoTasks = matchDetails.GetPlayerStatsForCsGoTasks;
                getPlayerMatchHistoryTasks = matchDetails.GetPlayerMatchHistoryTasks;

                // Await all tasks concurrently
                await Task.WhenAll(getPlayerStatsTasks);
                await Task.WhenAll(getPlayerStatsForCsGoTasks);
                await Task.WhenAll(getPlayerMatchHistoryTasks.Select(t => t.Item2));

                // Retrieve results from tasks
                var playerStats = getPlayerStatsTasks.Select(t => t.Result).ToList();
                var playerStatsForCsGo = getPlayerStatsForCsGoTasks.Select(t => t.Result).ToList();

                var playerMatchHistory = getPlayerMatchHistoryTasks.Select(t => (t.playerId, t.Item2.Result)).ToList();

                var (playerMatchStats, combinedPlayerStats) = await _playerStatisticsService.ProcessPlayerStatisticsAsync(
                 playerMatchHistory,
                 playerStats,
                 playerStatsForCsGo);


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
        public async Task<IActionResult> OnlyCsGoStats()
        {
            string jsonString = await ReadRequestBody.ReadRequestBodyAsync(Request);

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

            try
            {
                var partialViewModel = await _onlyCsGoStatsService.ProcessOnlyCsGoStatsAsync(toggleRequest);
                return PartialView("_StatisticsPartial", partialViewModel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("/ToggleIncludeCsGoStats")]
        public async Task<IActionResult> ToggleIncludeCsGoStats()
        {

            string jsonString = await ReadRequestBody.ReadRequestBodyAsync(Request);

            var toggleRequest = JsonConvert.DeserializeObject<ToggleIncludeCsGoStatsRequest>(jsonString);

            if (toggleRequest == null)
            {
                return BadRequest("Request is null");
            }

            // Validate the request data
            if (string.IsNullOrEmpty(toggleRequest.RoomId) || toggleRequest.Players == null || toggleRequest.PlayerMatchStats == null)
            {
                return BadRequest("Invalid request data");
            }

            try
            {
                var partialViewModel = await _toggleIncludeCsGoStatsService.ProcessToggleIncludeCsGoStatsAsync(toggleRequest);
                return PartialView("_StatisticsPartial", partialViewModel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> TogglePlayer()
        {

            string jsonString = await ReadRequestBody.ReadRequestBodyAsync(Request);

            var model = JsonConvert.DeserializeObject<ExcludePlayerModel>(jsonString);

            if (model == null || model.Players == null || model.PlayerStats == null || model.PlayerMatchStats == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var partialViewModel = await _togglePlayerService.ProcessTogglePlayerAsync(model);
                return PartialView("_StatisticsPartial", partialViewModel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log exception
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
