using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using FantasySoccer.Authorization;
using FantasySoccer.Core.Models.Functions;
using FantasySoccer.Core.Services;
using FantasySoccer.Core.Services.Exceptions;
using FantasySoccer.Extensions;
using FantasySoccer.Models;
using FantasySoccer.Models.Responses;
using FantasySoccer.Models.ViewModels;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Controllers
{
    public class AdminController: Controller
    {
        private readonly IFantasySoccerService fantasySoccerService;
        private readonly ISimulationService simulationService;
        private readonly JsonSerializerOptions serializerOptions;
        
        public AdminController(IFantasySoccerService fantasySoccerService, ISimulationService simulationService)
        {
            this.fantasySoccerService = fantasySoccerService;
            this.simulationService = simulationService;
            serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        [CustomAuthorize]
        public IActionResult Index()
        {
            ViewData["Title"]="Admininstrator Module";
            return View();
        }

        [CustomAuthorize]
        public async Task<IActionResult> Simulation()
        {
            ViewData["Title"] = "Simulation Module";

            var tournament = await fantasySoccerService.GetCurrentTournament();
            var numberOfRounds = await fantasySoccerService.GetNumberOfRoundsForATournament(tournament.ID);

            var viewModel = new SimulationViewModel
            {
                TournamentId = tournament.ID,
                TournamentName = tournament.Name,
                CurrentRound = tournament.CurrentRound,
                Rounds = Enumerable.Range(1, numberOfRounds)
                    .Select(r => new SelectListItem 
                        {
                            Text = r.ToString(),
                            Value = r.ToString(),
                            Selected = tournament.CurrentRound == r
                        }
                    )
            };

            return View("Simulation", viewModel);
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<ResponseWrapper<SimulationResponse>> SimulateRound([FromBody] SimulateRoundRequest request)
        {
            var tournament = await fantasySoccerService.GetTournament(request.TournamentId);
            var matchSimulationResults = await simulationService.SimulateTournamentRound(tournament.ID, request.Round);

            var playersPerformances = new List<MatchFutbolPlayerPerformance>();
            var matches = new List<Match>();
            matchSimulationResults.ForEach(msr => 
            {
                playersPerformances.AddRange(msr.PlayersPerformance);
                matches.Add(msr.Match);
            });

            await fantasySoccerService.UpdateMatchesAsync(matches);

            await fantasySoccerService.AddMatchFutbolPlayersPerformancesAsync(playersPerformances);
            
            tournament.CurrentRound++;
            await fantasySoccerService.UpdateTournament(tournament);

            return new ResponseWrapper<SimulationResponse>
            {
                StatusCode = Models.Responses.StatusCode.OK,
                Response = new SimulationResponse
                {
                    TournamentId = request.TournamentId,
                    TournamentName = tournament.Name,
                    CurrentRound = tournament.CurrentRound,
                    Matches = matches
                }
            };
        }
       
        [CustomAuthorize]
        public async Task<IActionResult> TournamentManagement()
        {
            if (TempData.ContainsKey("ViewModel"))
            {
                return View("TournamentManagement", TempData.Get<TournamentManagementViewModel>("ViewModel"));
            }

            var tournament = await fantasySoccerService.GetCurrentTournament();
            var tournamentEdit = new TournamentManagementViewModel
            {
                Id = tournament.ID,
                Name = tournament.Name,
                EndDate = tournament.EndDate,
                StartDate = tournament.StartDate,
                Result = TempData.Get<BaseResponseWrapper>("Result")
            };
            return View("TournamentManagement", tournamentEdit);
        }

        [CustomAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TournamentManagement([FromForm] TournamentManagementViewModel editedTournament)
        {
            if (!ModelState.IsValid)
            {
                editedTournament.Result = new BaseResponseWrapper
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = "The tournament team data sent is not valid"
                };
            }

            try
            {
                var tournament = await fantasySoccerService.GetCurrentTournament();
                tournament.ID = editedTournament.Id;
                tournament.Name = editedTournament.Name;
                tournament.StartDate = editedTournament.StartDate;
                tournament.EndDate = editedTournament.EndDate;

                await fantasySoccerService.UpdateTournament(tournament);

                editedTournament.Result = new BaseResponseWrapper
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Message = "The tournament team has been updated successfully"
                };
            }
            catch (Exception exception)
            {
                var statusCode = exception is TournamentStartDateMustBeOlderThanEndDateException ? Models.Responses.StatusCode.BadRequest : Models.Responses.StatusCode.InternalServerError;
                editedTournament.Result = new BaseResponseWrapper
                {
                    StatusCode = statusCode,
                    Message = exception.Message
                };
            }

            TempData.Set("ViewModel", editedTournament);
            return RedirectToAction(nameof(TournamentManagement));
        }

        [CustomAuthorize]
        public IActionResult CreateTournament()
        {
            if (TempData.ContainsKey("ViewModel"))
            {
                return View("TournamentManagement", TempData.Get<TournamentManagementViewModel>("ViewModel"));
            }

            var viewModel = new TournamentManagementViewModel { 
                StartDate = DateTime.Now,
                EndDate = DateTime.Now
            };

            return View("TournamentManagement", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize]
        public async Task<IActionResult> CreateTournament([FromForm] TournamentManagementViewModel tournament)
        {
            if (!ModelState.IsValid)
            {
                tournament.Result = new BaseResponseWrapper
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = "The tournament team data sent is not valid"
                };
                TempData.Set("ViewModel", tournament);

                return RedirectToAction(nameof(CreateTournament));
            }

            try
            {
                await fantasySoccerService.AddTournamentAsync(new Tournament 
                {
                    Name = tournament.Name,
                    StartDate = tournament.StartDate,
                    EndDate = tournament.EndDate
                });

                tournament.Result = new BaseResponseWrapper
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Message = "The tournament team has been created successfully"
                };
                TempData.Set("Result", tournament.Result);

                return RedirectToAction(nameof(TournamentManagement));
            }
            catch (Exception exception)
            {
                var statusCode = exception is TournamentStartDateMustBeOlderThanEndDateException ? Models.Responses.StatusCode.BadRequest : Models.Responses.StatusCode.InternalServerError;
                tournament.Result = new BaseResponseWrapper
                {
                    StatusCode = statusCode,
                    Message = exception.Message
                };
                TempData.Set("ViewModel", tournament);
                
                return RedirectToAction(nameof(CreateTournament));
            }
        }

        [CustomAuthorize]
        public async Task<IActionResult> MatchDayScheduling()
        {
            ViewData["Title"] = "Matchday Scheduling";
            
            var tournament = await fantasySoccerService.GetCurrentTournament();
            var numberOfRounds = await fantasySoccerService.GetNumberOfRoundsForATournament(tournament.ID);
            var matches = await fantasySoccerService.GetMatches(tournament.ID, tournament.CurrentRound);

            var viewModel = new CurrentRoundViewModel
            {
                TournamentId = tournament.ID,
                TournamentName = tournament.Name,
                CurrentRound = tournament.CurrentRound,
                NumberOfRounds = numberOfRounds,
                Matches = matches
            };

            return View(viewModel);
        }

        [CustomAuthorize]
        public async Task<ResponseWrapper<List<Match>>> GetMatchesByRound([FromQuery] SimulateRoundRequest request)
        {
            try
            {
                var matches = await fantasySoccerService.GetMatches(request.TournamentId, request.Round);

                return new ResponseWrapper<List<Match>>
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Response = matches
                };
            }
            catch
            {
                return new ResponseWrapper<List<Match>>
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = "Couldn't get the round data. \nTry again later"
                };
            }
        }

        [CustomAuthorize]
        public IActionResult DataManagement()
        {
            ViewData["Title"]="Data Management";
            return View();
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<BaseResponseWrapper> UploadTeams([FromForm] FileUpload objFile)
        {
            try
            {
                var result = GetStringFromFile(objFile);

                var data = JsonSerializer.Deserialize<List<FutbolTeam>>(result.ToString(), serializerOptions);

                await fantasySoccerService.OverwriteFutbolTeamsAsync(data);

                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Message = "Teams updated."
                };
            }
            catch
            {
                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = "Couldn't update teams. \nTry again later"
                };
            }
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<BaseResponseWrapper> UploadPlayers([FromForm] FileUpload objFile)
        {
            try
            {
                var result = GetStringFromFile(objFile);

                var data = JsonSerializer.Deserialize<List<FutbolPlayer>>(result.ToString(), serializerOptions);

                await fantasySoccerService.OverwriteFutbolPlayersAsync(data);

                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Message = "Players updated."
                };
            }
            catch
            {
                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = "Couldn't update players. \nTry again later"
                };
            }
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<BaseResponseWrapper> UploadMatchDays([FromForm] FileUpload objFile)
        {
            try
            {
                var result = GetStringFromFile(objFile);

                var data = JsonSerializer.Deserialize<List<Match>>(result.ToString(), serializerOptions);

                await fantasySoccerService.OverwriteMatchesAsync(data);

                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Message = "Matches updated."
                };
            }
            catch
            {
                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = "Couldn't update matches. \nTry again later"
                };
            }
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<ActionResult> DownloadTeams()
        {
            var teams = await fantasySoccerService.GetFutbolTeamsAsync();
            
            var data = JsonSerializer.Serialize(teams, serializerOptions);

            var bytes = Encoding.UTF8.GetBytes(data);

            return new FileContentResult(bytes, "application/json")
            {
                FileDownloadName = "teams.json"
            };
        }
        
        [HttpPost]
        [CustomAuthorize]
        public async Task<ActionResult> DownloadPlayers()
        {
            var players = await fantasySoccerService.GetFutbolPlayersStoreAsync();

            var data = JsonSerializer.Serialize(players.PaginatedItems, serializerOptions);

            var bytes = Encoding.UTF8.GetBytes(data);

            return new FileContentResult(bytes, "application/json")
            {
                FileDownloadName = "players.json"
            };
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<ActionResult> DownloadMatchDay()
        {
            var currentTournament = await fantasySoccerService.GetCurrentTournament();
            var matches = await fantasySoccerService.GetMatches(currentTournament.ID, null);

            var data = JsonSerializer.Serialize(matches, serializerOptions);

            var bytes = Encoding.UTF8.GetBytes(data);

            return new FileContentResult(bytes, "application/json")
            {
                FileDownloadName = "matches.json"
            };
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<BaseResponseWrapper> CleanTeams()
        {
            try
            { 
                await fantasySoccerService.CleanFutbolTeamsAsync();
                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Message = "Teams cleaned."
                };
            }
            catch
            {
                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = "Couldn't clean teams. \nTry again later."
                };
            }
        }

        private static StringBuilder GetStringFromFile(FileUpload objFile)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(objFile.file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadLine());
            }

            return result;
        }
    }
}
