using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasySoccer.Authorization;
using FantasySoccer.Core.Services;
using FantasySoccer.Models.Responses;
using FantasySoccer.Models.ViewModels;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Controllers
{
    public class MarketplaceController: Controller
    {
        private readonly IFantasySoccerService fantasySoccerService;

        public MarketplaceController(IFantasySoccerService fantasySoccerService)
        {
            this.fantasySoccerService = fantasySoccerService;
        }

        [CustomAuthorize]
        public async Task<ViewResult> Index(int? page, string? searchedFirstNamePlayer, string? searchedLastNamePlayer)
        {
            ViewBag.Title = "Marketplace";
            var pageSize = 20;
            var pageNumber = page ?? 1;
            ViewBag.FirstName = searchedFirstNamePlayer;
            ViewBag.LastName = searchedLastNamePlayer;

            var (marketPlaceItems, totalFutbolPlayer) = await GetMarketPlaceItems(pageSize, pageNumber, searchedFirstNamePlayer, searchedLastNamePlayer);

            return View(new MarketplaceViewModel
            {
                CurrentPage = pageNumber,
                FutbolPlayers = marketPlaceItems,
                TotalPages = CalculateTotalPages(totalFutbolPlayer, pageSize)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ViewResult> Index([FromForm] string searchedFirstNamePlayer, string searchedLastNamePlayer)
        {
            ViewBag.Title = "Marketplace";
            ViewBag.FirstName = searchedFirstNamePlayer;
            ViewBag.LastName = searchedLastNamePlayer;
            var page = 1;
            var pageSize = 20;

            var (marketPlaceItems, totalFutbolPlayer) = await GetMarketPlaceItems(pageSize, page, searchedFirstNamePlayer, searchedLastNamePlayer);

            return View("Index", new MarketplaceViewModel
            {
                CurrentPage = page,
                FutbolPlayers = marketPlaceItems,
                TotalPages = CalculateTotalPages(totalFutbolPlayer, pageSize)
            });
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<ResponseWrapper<string>> Buy(FutbolPlayer futbolPlayer)
        {
            try
            {
                var budget = await fantasySoccerService.BuyFutbolPlayerAsync(futbolPlayer.ID, (int)futbolPlayer.Price);

                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Response = budget,
                    Message = $"{futbolPlayer.GetFullName()} was added to your team.",
                };
            }
            catch
            {
                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = "Couldn't buy the player. \nTry again later"
                };
            }
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<ResponseWrapper<string>> Sell(string id)
        {
            try
            {
                var budget = await fantasySoccerService.SellFutbolPlayerAsync(id);

                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Response = budget,
                    Message = "The sale has been registered successfully"
                };
            }
            catch
            {
                return new ResponseWrapper<string>
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = "Couldn't sell the player. \nTry again later"
                };
            }
        }

        private async Task<(List<MarketPlaceItems>, int)> GetMarketPlaceItems(int pageSize, int page, string searchedFirstNamePlayer = "", string searchedLastNamePlayer = "")
        {
            Func<FutbolPlayer, bool> filterPlayer = null;

            if (!string.IsNullOrWhiteSpace(searchedFirstNamePlayer) || !string.IsNullOrWhiteSpace(searchedLastNamePlayer))
            {
                filterPlayer = futbolPlayer => futbolPlayer.Name.ToLower().Contains(searchedFirstNamePlayer?.ToLower() ?? " ") || futbolPlayer.LastName.ToLower().Contains(searchedLastNamePlayer?.ToLower() ?? " ");
            }

            var skip = (page - 1) * pageSize;

            var futbolPlayerPaginated = await fantasySoccerService.GetFutbolPlayersStoreAsync(pageSize, skip, filterPlayer);

            var teams = await fantasySoccerService.GetFutbolTeamsAsync();
            var userTeam = await fantasySoccerService.GetUserTeamAsync();

            var marketPlaceItems = futbolPlayerPaginated.PaginatedItems.Join(teams,
                                 p => p.FutbolTeamID,
                                 t => t.ID,
                                 (p, t) => new MarketPlaceItems
                                 {
                                     FutbolPlayer = p,
                                     FutbolTeamName = t.Name,
                                     UserTeamContainsPlayer = userTeam.Players != null && userTeam.Players.Any(teamPlayer => teamPlayer.ID == p.ID)
                                 }).ToList();

            return (marketPlaceItems, futbolPlayerPaginated.TotalItems);
        }

        private int CalculateTotalPages(int total, int pageSize)
        {
            return (int)Math.Ceiling((double)total / (double)pageSize);
        }
    }
}
