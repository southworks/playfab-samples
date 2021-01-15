using System;
using System.Collections.Generic;
using System.Linq;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;
using Newtonsoft.Json;

namespace FantasySoccer.Tests.FakeServices
{
    public static class DataHelper
    {
        public static List<FutbolPlayer> GetDummyPlayers(int startersQuantity, int subsQuantity, int lastIdUsed = 0, int futbolTeamID = 1)
        {
            var players = new List<FutbolPlayer>();
            if (startersQuantity > 0)
            {
                var starters = Enumerable.Range(1, startersQuantity)
                       .Select(r => new FutbolPlayer
                       {
                           ID = (lastIdUsed + r).ToString(),
                           InventoryId = (lastIdUsed + r).ToString(),
                           FutbolTeamID = futbolTeamID.ToString(),
                           Name = "Darrell",
                           LastName = "Gusikowski",
                           Birthdate = DateTime.Now,
                           Price = 4500,
                           Position = Position.Forward,
                           IsStarter = true,
                       })
                       .ToList();
                players.AddRange(starters);
            }

            if (subsQuantity > 0)
            {
                var subs = Enumerable.Range(1, subsQuantity)
                       .Select(r => new FutbolPlayer
                       {
                           ID = (lastIdUsed + startersQuantity + r).ToString(),
                           InventoryId = (lastIdUsed + startersQuantity + r).ToString(),
                           FutbolTeamID = futbolTeamID.ToString(),
                           Name = "Kenneth",
                           LastName = "Johnston",
                           Birthdate = DateTime.Now,
                           Price = 4500,
                           Position = Position.Forward,
                           IsStarter = false,
                       })
                       .ToList();
                players.AddRange(subs);
            }

            return players;
        }
      
        public static List<PlayFabItem> GetDummyStoreByTeams(int playerByTeam, string currency, int teams = 1)
        {
            var store = new List<PlayFabItem>();
            if (playerByTeam > 0)
            {
                for (var teamId = 1; teamId <= teams; teamId++)
                {
                    var players = GetDummyPlayers(playerByTeam, 0, (teamId - 1) * playerByTeam, teamId);

                    store.AddRange(players.Select(p => new PlayFabItem
                    {
                        ItemId = p.ID,
                        DisplayName = p.GetFullName(),
                        Currency = currency,
                        PriceStore = p.Price,
                        CustomData = JsonConvert.SerializeObject(p),
                    }));
                }
            }

            return store;
        }

        public static List<PlayFabItemInventory> GetDummyInventory(int startersQuantity, int subsQuantity, string currency)
        {
            var inventory = new List<PlayFabItemInventory>();
            if (startersQuantity > 0 || subsQuantity > 0)
            {
                var players = GetDummyPlayers(startersQuantity, subsQuantity);

                inventory.AddRange(players.Select(p => new PlayFabItemInventory
                {
                    ItemId = p.ID,
                    ItemInstanceId = p.InventoryId,
                    PurchaseDate = DateTime.Now,
                    DisplayName = p.GetFullName(),
                    Currency = currency,
                    PriceStore = p.Price,
                    CustomDataStore = JsonConvert.SerializeObject(p),
                    CustomDataInventory = new CustomDataInventory
                    {
                        IsStarter = p.IsStarter
                    }
                }));
            }

            return inventory;
        }

        public static List<UserTransaction> GetDummyUserTransactions(int quantity, string playFabId)
        {
            var operationTypesValues = OperationTypes.GetValues(typeof(OperationTypes));
            var random = new Random();

            var userTransactions = new List<UserTransaction>();
            if (quantity > 0)
            {
                userTransactions.AddRange(Enumerable.Range(1, quantity)
                    .Select(r => new UserTransaction
                    {
                        ID = r.ToString(),
                        UserPlayFabID = playFabId,
                        InvolvedFutbolPlayerID = r.ToString(),
                        OperationDate = DateTime.Now,
                        OperationType = (OperationTypes)random.Next(0, 2),
                    })
                    .ToList());
            }
            return userTransactions;
        }

        public static List<Tournament> GetDummyTournaments(int quantity)
        {
            var tournaments = new List<Tournament>();

            if (quantity > 0)
            {
                tournaments.AddRange(Enumerable.Range(1, quantity)
                    .Select(r => new Tournament
                    {
                        ID = r.ToString(),
                        Name = "Lorem",
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now,
                        CurrentRound = 1
                    })
                    .ToList());
            }
            return tournaments;
        }

        public static List<PlayFab.ClientModels.StatisticValue> GetDummyStatisticsForTournament(string tournamentId, int quantity)
        {
            var statistics = new List<PlayFab.ClientModels.StatisticValue>();

            if (quantity > 0)
            {
                statistics.AddRange(Enumerable.Range(1, quantity)
                    .Select(r => new PlayFab.ClientModels.StatisticValue
                    {
                        StatisticName = $"r-{r.ToString()}-{tournamentId}",
                        Value = r * 10,
                        Version = 1
                    })
                    .ToList());
            }
            return statistics;
        }

        public static List<Match> GetDummyMatches(int quantity, string tournamentId, int round)
        {
            var tournaments = new List<Match>();

            if (quantity > 0)
            {
                tournaments.AddRange(Enumerable.Range(1, quantity)
                    .Select(r => new Match
                    {
                        ID = r.ToString(),
                        LocalFutbolTeamID = "1",
                        VisitorFutbolTeamID = "2",
                        Round = round,
                        TournamentId = tournamentId,
                        LocalGoals = 0,
                        VisitorGoals = 0
                    })
                    .ToList());
            }
            return tournaments;
        }
    }
}
