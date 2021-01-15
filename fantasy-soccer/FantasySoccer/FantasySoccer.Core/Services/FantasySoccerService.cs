using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasySoccer.Core.Configuration;
using FantasySoccer.Core.Services.Exceptions;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Core.Services
{
    public class FantasySoccerService: IFantasySoccerService
    {
        private readonly ICosmosDBService cosmosDBService;
        private readonly IPlayFabService playFabService;

        public FantasySoccerService(
            ICosmosDBService cosmosDBService, 
            IPlayFabService playFabService)
        {
            this.cosmosDBService = cosmosDBService;
            this.playFabService = playFabService;
        }

        public async Task<UserPlayerStatistics> GetUserTeamStatisticAsync()
        {
            var tournament = await GetCurrentTournament();
            var currentRound = tournament?.CurrentRound ?? default(int);
            
            if (currentRound < 1)
            {
                throw new Exception("Tournament has not started");
            }
            
            var listStatistic = GetStatisticsName(tournament.ID, tournament.CurrentRound);
            var userStatistic = await playFabService.GetUserStatisticAsync(listStatistic);

            if(userStatistic == null || !userStatistic.Any())
            {
                return null;
            }

            var score = 0;
            var bestRound = new UserPlayerStatisticItem
            {
                Order = 10,
                Description = "Best round",
                Round = 0,
                Score = -10000,
            };
            var worstRound = new UserPlayerStatisticItem
            {
                Order = 20,
                Description = "Worst round",
                Round = 0,
                Score = 10000,
            };

            userStatistic.ForEach(userStatistic =>
            {
                score += userStatistic.Value;
                if (bestRound.Score < userStatistic.Value)
                {
                    bestRound.Score = userStatistic.Value;
                    bestRound.Round = ConvertStatisticNameToRoundNumber(userStatistic.StatisticName);
                }
                if (worstRound.Score > userStatistic.Value)
                {
                    worstRound.Score = userStatistic.Value;
                    worstRound.Round = ConvertStatisticNameToRoundNumber(userStatistic.StatisticName);
                }
            });

            var userPlayerStatistics = new UserPlayerStatistics
            {
                StatisticItems = new List<UserPlayerStatisticItem>
                {
                    new UserPlayerStatisticItem
                    {
                        Description = "Current round",
                        Round = tournament.CurrentRound,
                        Order = 1,
                        Score = score,
                    },
                    new UserPlayerStatisticItem
                    {
                        Description = "Average round score",
                        Round = 0,
                        Order = 100,
                        Score = score/tournament.CurrentRound,
                    },
                    bestRound,
                    worstRound
                }
            };

            return userPlayerStatistics;
        }

        public async Task<List<FutbolPlayer>> GetFutbolPlayerByFutbolTeamIdAsync(string futbolTeamId, string visitorFutbolTeamId = null)
        {
            var items = await playFabService.GetStoreItems();
            var futbolPlayers = new List<FutbolPlayer>();
            
            items.ForEach(item =>
            {
                var futbolPlayer = JsonConvert.DeserializeObject<FutbolPlayer>(item.CustomData);

                if (futbolPlayer.FutbolTeamID == futbolTeamId || futbolPlayer.FutbolTeamID == visitorFutbolTeamId)
                {
                    futbolPlayers.Add(futbolPlayer);
                }
            });
            return futbolPlayers;
        }

        public async Task<List<FutbolTeam>> GetFutbolTeamsAsync()
        {
            var container = cosmosDBService.GetContainer(CosmosDBConstants.FutbolTeamId);
            var teams = await cosmosDBService.GetItemsAsync<FutbolTeam>(container);
            var players = await GetFutbolPlayersStoreAsync();

            return teams.Select(t => new FutbolTeam { 
                                    ID = t.ID,
                                    Name = t.Name,
                                    FutbolPlayersId = t.FutbolPlayersId,
                                    Players = players.FindAll(p => p.FutbolTeamID == t.ID)
            }).ToList();
        }

        public async Task<List<FutbolPlayer>> GetFutbolPlayersStoreAsync()
        {
            var items = await playFabService.GetStoreItems();
            var players = new List<FutbolPlayer>();

            foreach (var item in items)
            {
                var player = JsonConvert.DeserializeObject<FutbolPlayer>(item.CustomData);
                players.Add(player);
            }
            return players;
        }

        public async Task<PaginatedItem<FutbolPlayer>> GetFutbolPlayersStoreAsync(int? size = null, int? skip = null, Func<FutbolPlayer, bool> filter = null)
            => await playFabService.GetStoreItems(size, skip, filter);

        public async Task<UserTeam> GetUserTeamAsync()
        {
            try
            {
                var result = await playFabService.GetUserInventory();
                
                var players = new List<FutbolPlayer>();

                foreach (var item in result.Inventory)
                {
                    var player = JsonConvert.DeserializeObject<FutbolPlayer>(item.CustomDataStore);
                    player.ID = item.ItemId;
                    player.InventoryId = item.ItemInstanceId;
                    player.IsStarter = item.CustomDataInventory.IsStarter;
                    players.Add(player);
                }

                return new UserTeam
                {
                    UserPlayFabID = playFabService.GetPlayFabId(),
                    MatchdayScores = new Dictionary<int, int> { },
                    Players = players
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<Tournament> GetTournament(string id)
        {
            var container = cosmosDBService.GetContainer(CosmosDBConstants.TournamentContainerId);

            return await cosmosDBService.GetItemById<Tournament>(container, id);
        }

        public async Task<Tournament> GetCurrentTournament()
        {
            var container = cosmosDBService.GetContainer(CosmosDBConstants.TournamentContainerId);
            var query = "SELECT MAX(StringToNumber(c.id)) CurrentId FROM c";
            var queryResult = (await cosmosDBService.GetItemsAsync<Dictionary<string, int>>(container, query)).FirstOrDefault();
            var currentTournamentId = 0;
            queryResult?.TryGetValue("CurrentId", out currentTournamentId);

            return await GetTournament(currentTournamentId.ToString());
        }

        public async Task<List<Match>> GetMatches(string tournamentId, int? round)
        {
            var container = cosmosDBService.GetContainer(CosmosDBConstants.MatchContainerId);
            var query = $"SELECT * FROM c WHERE c.TournamentId = '{tournamentId}'";
            if (round != null)
            {
                query += $" AND c.Round = {round}";
            }
            
            var teams = await GetFutbolTeamsAsync();
            var matches = await cosmosDBService.GetItemsAsync<Match>(container, query);
            matches.ForEach(m =>
            {
                m.LocalFutbolTeamName = teams.FirstOrDefault(t => t.ID == m.LocalFutbolTeamID)?.Name;
                m.VisitorFutbolTeamName = teams.FirstOrDefault(t => t.ID == m.VisitorFutbolTeamID)?.Name;
            });

            return matches;
        }

        public async Task<int> GetNumberOfRoundsForATournament(string tournamentId)
        {
            var container = cosmosDBService.GetContainer(CosmosDBConstants.MatchContainerId);
            var query = $"SELECT MAX(c.Round) NumberOfRounds FROM c WHERE c.TournamentId = \"{tournamentId}\"";

            var result = (await cosmosDBService.GetItemsAsync<Dictionary<string, int>>(container, query)).FirstOrDefault();
            return result.ContainsKey("NumberOfRounds") ? result["NumberOfRounds"] : default;
        }
        
        public async Task<List<UserTransaction>> GetUserTransactionsAsync()
        {
            var userTransactionContainer = cosmosDBService.GetContainer(CosmosDBConstants.UserTransactionId);
            var query = $"SELECT * FROM c WHERE c.UserPlayFabID = '{playFabService.GetPlayFabId()}' ORDER BY c.OperationDate DESC";
            var userTransactions = await cosmosDBService.GetItemsAsync<UserTransaction> (userTransactionContainer, query);
            
            var players = await GetFutbolPlayersStoreAsync();

            userTransactions.ForEach(transaction =>
                transaction.InvolvedFutbolPlayerFullName = players.FirstOrDefault(p => p.ID == transaction.InvolvedFutbolPlayerID).GetFullName());

            return userTransactions;
        }

        public async Task UpdateUserTeamAsync(UserTeam userTeam)
        {
            if (userTeam.Players.Where(player => player.IsStarter).Count() != FantasySoccerConstants.NumberOfStarters)
            {
                throw new Exception($"You need to have {FantasySoccerConstants.NumberOfStarters} starters.");
            }

            foreach (var player in userTeam.Players)
            {
                await playFabService.UpdateUserInventoryItemCustomData(
                    playFabService.GetPlayFabId(), 
                    player.InventoryId, 
                    new Dictionary<string, string> { { "IsStarter", player.IsStarter.ToString().ToLower() } });
            }
        }

        public async Task OverwriteFutbolTeamsAsync(List<FutbolTeam> futbolTeams)
        {
            var players = new List<FutbolPlayer> { };
            futbolTeams.ForEach(t => players.AddRange(t.Players));

            await OverwriteFutbolPlayersAsync(players);

            var futbolTeamContainer = await CleanContainerAsync(CosmosDBConstants.FutbolTeamId);
            await cosmosDBService.AddBulkItemsAsync(futbolTeamContainer, futbolTeams);
        }
        
        public async Task OverwriteMatchesAsync(List<Match> matches)
        {            
            var matchContainer = await CleanContainerAsync(CosmosDBConstants.MatchContainerId);
            await cosmosDBService.AddBulkItemsAsync(matchContainer, matches);
        }

        public async Task AddMatchesAsync(List<Match> matches)
        {
            var matchContainer = cosmosDBService.GetContainer(CosmosDBConstants.MatchContainerId);
            await cosmosDBService.AddBulkItemsAsync(matchContainer, matches);
        }

        public async Task OverwriteFutbolPlayersAsync(List<FutbolPlayer> futbolPlayers)
        {           
            await playFabService.SetCatalogItems(futbolPlayers);
            await playFabService.SetStoreItems(futbolPlayers);            
        }

        public async Task UpdateMatchesAsync(List<Match> matches)
        {
            var matchContainer = cosmosDBService.GetContainer(CosmosDBConstants.MatchContainerId);
            await cosmosDBService.UpdateItemsAsync(matchContainer, matches);
        }

        public async Task CleanFutbolTeamsAsync()
        {
            await CleanContainerAsync(CosmosDBConstants.FutbolTeamId);
        }

        public async Task<string> SellFutbolPlayerAsync(string itemInstanceId)
        {
            var item = await playFabService.GetItemInventory(itemInstanceId, OriginItemEnum.Inventory);

            if (item == null)
            {
                throw new Exception("Player not found.");
            }
            
            await playFabService.RevokeInventoryItem(playFabService.GetPlayFabId(), itemInstanceId);
            var budget = await playFabService.AddUserVirtualCurrency(playFabService.GetPlayFabId(), (int)item.PriceStore);

            await AddUserTransaction(item.ItemId, OperationTypes.Sell);

            return budget;
        }

        public async Task<string> BuyFutbolPlayerAsync(string itemId, int price)
        {
            var item = await playFabService.GetItemInventory(itemId, OriginItemEnum.Store);

            if (item != null)
            {
                throw new Exception("The player is already on your team.");
            }

            var purchaseItemResponse = await playFabService.PurchaseItem(itemId, price);
            await playFabService.UpdateUserInventoryItemCustomData(playFabService.GetPlayFabId(), purchaseItemResponse.ItemInstanceId, new Dictionary<string, string> { { "IsStarter", "false" } });

            await AddUserTransaction(itemId, OperationTypes.Buy);

            return purchaseItemResponse.Budget;
        }

        public async Task<List<MatchFutbolPlayerPerformance>> AddMatchFutbolPlayersPerformancesAsync(List<MatchFutbolPlayerPerformance> performances)
        {
            var container = cosmosDBService.GetContainer(CosmosDBConstants.MatchPlayerPerformanceContainerId);
            await cosmosDBService.AddBulkItemsAsync(container, performances);

            return performances;
        }

        public async Task CalculateUserRoundScore(string currentPlayerId, string tournamentId, int round)
        {
            var userInventory = await playFabService.GetUserInventoryUsingAdminAPI(currentPlayerId);

            if(string.IsNullOrWhiteSpace(tournamentId) || round < 1)
            {
                throw new ArgumentException();
            }

            if (userInventory.Count == 0)
            {
                // User has no team configured
                return;
            }

            var starters = userInventory.Where(item => 
                item.CustomData != null &&
                item.CustomData.ContainsKey("IsStarter") && 
                item.CustomData["IsStarter"] == false.ToString())
                .ToList();

            var query = $"SELECT * FROM c where c.FutbolPlayerID in ({string.Join(",", userInventory.Select(i => $"\"{i.ItemId}\""))}) and c.TournamentId = \"{tournamentId}\" and c.Round = {round}";

            var performances = await cosmosDBService.GetItemsAsync<MatchFutbolPlayerPerformance>(cosmosDBService.GetContainer(CosmosDBConstants.MatchPlayerPerformanceContainerId), query);
            var roundScore = performances.Any() ? performances.Sum(performance => performance.Score) : -1;

            await playFabService.UpdatePlayerStatistics(
                currentPlayerId,
                new Dictionary<string, int> {
                    { $"r-{round}-{tournamentId}", roundScore },
                    { "TournamentScore", roundScore }
                });
        }

        public async Task UpdateTournament(Tournament tournament)
        {
            if (tournament.StartDate > tournament.EndDate)
            {
                throw new TournamentStartDateMustBeOlderThanEndDateException("End date must be newer than start date");
            }

            var container = cosmosDBService.GetContainer(CosmosDBConstants.TournamentContainerId);
            await cosmosDBService.UpdateItemAsync(container, tournament);
        }

        public async Task AddTournamentAsync(Tournament tournament)
        {
            if (tournament.StartDate > tournament.EndDate)
            {
                throw new TournamentStartDateMustBeOlderThanEndDateException("End date must be newer than start date");
            }

            var titleData = await playFabService.GetTitleData(new List<string> { PlayFabConstants.CurrentTournamentId });
            var currentTournamentId = titleData.ContainsKey(PlayFabConstants.CurrentTournamentId) ? titleData[PlayFabConstants.CurrentTournamentId] : "0";
            tournament.ID = (int.Parse(currentTournamentId) + 1).ToString();
            tournament.CurrentRound = 1;

            var container = cosmosDBService.GetContainer(CosmosDBConstants.TournamentContainerId);
            await cosmosDBService.AddItemsAsync(container, tournament, tournament.ID);
            await playFabService.SetTitleData(PlayFabConstants.CurrentTournamentId, tournament.ID);

            await AddMatchesForTournament(tournament.ID);
        }

        private async Task AddMatchesForTournament(string tournamentId)
        {
            var teams = await GetFutbolTeamsAsync();
            var matches = GenerateMatches(tournamentId, teams);

            await AddMatchesAsync(matches);
        }

        private List<Match> GenerateMatches(string tournamentId, List<FutbolTeam> teams, bool isHomeAway = false)
        {
            var totalRounds = teams.Count - 1;
            var halfTeamsAmount = teams.Count / 2;
            var tempTeams = teams.ToList();
            
            var matches = new List<Match> { };

            for (var round = 0; round < totalRounds; round++)
            {
                for (var i = 0; i < halfTeamsAmount; i++)
                {
                    matches.Add(new Match {
                        ID = Guid.NewGuid().ToString(),
                        TournamentId = tournamentId,
                        LocalFutbolTeamID = tempTeams[i].ID,
                        VisitorFutbolTeamID = tempTeams[teams.Count - 1 - i].ID,
                        Round = round + 1
                    });

                    if (isHomeAway)
                    {
                        matches.Add(new Match
                        {
                            LocalFutbolTeamID = tempTeams[teams.Count - 1 - i].ID,
                            VisitorFutbolTeamID = tempTeams[i].ID,
                            Round = round + 1 + (totalRounds / 2)
                        });
                    }
                }
                SwapTeams(tempTeams);
            }
            return matches;

            void SwapTeams(List<FutbolTeam> teams)
            {
                var item = teams[^1];
                teams.RemoveAt(teams.Count - 1);
                teams.Insert(1, item);
            }
        }

        private int ConvertStatisticNameToRoundNumber(string statisticName)
        {
            return Convert.ToInt32(statisticName[2..statisticName.IndexOf("-", 2)]);
        }

        private List<string> GetStatisticsName(string tournamentId, int currentRound)
        {
            var statisticsName = new List<string>();

            for (var roundNumber = 1; roundNumber <= currentRound; roundNumber++)
            {
                statisticsName.Add($"r-{roundNumber}-{tournamentId}");
            }
            return statisticsName;
        }

        private async Task<Container> CleanContainerAsync(string containerId)
        {
            var futbolPlayerContainer = cosmosDBService.GetContainer(containerId);
            await cosmosDBService.DeleteContainerAsync(futbolPlayerContainer);
            await cosmosDBService.CreateContainerAsync(containerId, CosmosDBConstants.PartitionKey);
            return futbolPlayerContainer;
        }

        private async Task AddUserTransaction(string futbolPlayerID, OperationTypes operation)
        {
            var UserTransactionContainer = cosmosDBService.GetContainer(CosmosDBConstants.UserTransactionId);
            var titleData = await playFabService.GetTitleData(new List<string> { PlayFabConstants.CurrentTournamentId });
            var currentTournamentId = titleData.ContainsKey(PlayFabConstants.CurrentTournamentId) ? titleData[PlayFabConstants.CurrentTournamentId] : "0";
            var userTransaction = new UserTransaction
            {
                ID = currentTournamentId + "-" + Guid.NewGuid().ToString(),
                UserPlayFabID = playFabService.GetPlayFabId(),
                InvolvedFutbolPlayerID = futbolPlayerID,
                OperationDate = DateTime.Now,
                OperationType = operation
            };
            await cosmosDBService.AddItemsAsync(UserTransactionContainer, userTransaction, userTransaction.ID);
        }
    }
}
