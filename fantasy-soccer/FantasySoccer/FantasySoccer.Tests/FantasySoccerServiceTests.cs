using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasySoccer.Core.Configuration;
using FantasySoccer.Core.Services;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;
using FantasySoccer.Tests.FakeServices;
using PlayFab.AdminModels;
using Xunit;


namespace FantasySoccer.Tests
{
    public class FantasySoccerServiceTests
    {
        private readonly IFantasySoccerService fantasySoccerService;
        private readonly FakeCosmosDBService fakeCosmosDBService;
        private readonly FakePlayFabService fakePlayFabService;

        public FantasySoccerServiceTests()
        {
            fakeCosmosDBService = new FakeCosmosDBService();
            fakePlayFabService = new FakePlayFabService();
            fantasySoccerService = new FantasySoccerService(fakeCosmosDBService.stub.Object, fakePlayFabService.stub.Object);
        }

        [Fact]
        public async Task CalculateUserRoundScore_CalculationOfUserPerformanceInTheRound_UserPlayerRoundPerformancesHasBeenRegistered()
        {
            //arrange
            const int ROUND = 1;
            const string TOURNAMENT_ID = "1";
            const string SCORE_ENTRY = "r-1-1";

            fakePlayFabService.InventoryUsingAdminAPI = new List<ItemInstance> {
                new ItemInstance
                {
                    ItemId = "1",
                    ItemInstanceId = "1",
                    PurchaseDate = new System.DateTime(2020, 1, 5, 13, 1, 0),
                    CatalogVersion = "futbolplayers",
                    DisplayName = "Darrell Gusikowski",
                    UnitCurrency = fakePlayFabService.Currency,
                    UnitPrice = 1000,
                    CustomData = new Dictionary<string, string>
                    {
                        { "IsStarter" , "false" }
                    }
                }
            };

            fakeCosmosDBService.MatchFutbolPlayerPerformances = new List<MatchFutbolPlayerPerformance>
            {
                new MatchFutbolPlayerPerformance {
                    ID = "1",
                    FutbolPlayerID = "1",
                    MatchID = "2",
                    Goals = 1,
                    Faults = 1,
                    YellowCards = 1,
                    RedCards = 1,
                    Saves = 1,
                    OwnGoals = 1,
                    PlayedMinutes = 1,
                    Score = 1,
                    Round = 1,
                    TournamentId = "1"
                },
                new MatchFutbolPlayerPerformance {
                    ID = "2",
                    FutbolPlayerID = "2",
                    MatchID = "1",
                    Goals = 1,
                    Faults = 1,
                    YellowCards = 1,
                    RedCards = 1,
                    Saves = 1,
                    OwnGoals = 1,
                    PlayedMinutes = 1,
                    Score = 1,
                    Round = 1,
                    TournamentId = "1"
                }
            };

            //act
            await fantasySoccerService.CalculateUserRoundScore(fakePlayFabService.PlayFabId, TOURNAMENT_ID, ROUND);
            var actualScore = fakePlayFabService.Statistics.FirstOrDefault(s => s.StatisticName == SCORE_ENTRY).Value;

            //assert
            Assert.Equal(2, actualScore);
        }

        [Fact]
        public async Task BuyFutbolPlayerAsync_BuyAlreadyExistingFutbolPlayer_ReturnsException()
        {
            //arrange
            fakePlayFabService.Inventory = new List<PlayFabItemInventory> {
                new PlayFabItemInventory
                {
                    ItemId = "1",
                    ItemInstanceId = "1",
                    PurchaseDate = new System.DateTime(2020, 1, 5, 13, 1, 0),
                    DisplayName = "Kenneth Johnston",
                    Currency = fakePlayFabService.Currency,
                    PriceStore = 4000,
                    CustomDataStore= "{\"FutbolTeamID\":\"1\",\"Name\":\"Kenneth\",\"LastName\":\"Johnston\",\"Birthdate\":\"2020-02-24T20:07:42.8397804-03:00\",\"Price\":4000,\"Position\":1,\"IsStarter\":false,\"id\":\"1\"}",
                    CustomDataInventory = new CustomDataInventory
                    {
                        IsStarter = false
                    }
                }
            };

            //act
            async Task actual()
            {
                await fantasySoccerService.BuyFutbolPlayerAsync("1",4000);
            }

            //assert
            await Assert.ThrowsAsync<Exception>(actual);
        }

        [Fact]
        public async Task BuyFutbolPlayerAsync_BuyFutbolPlayerDecreaseBudget_ReturnsUpdatedBudget()
        {
            //arrange
            fakePlayFabService.TitleData = new Dictionary<string, string>
            {
                {PlayFabConstants.CurrentTournamentId, "1" }
            };
            fakePlayFabService.Store = new List<PlayFabItem>
            {
                new PlayFabItem
                {
                    ItemId = "2",
                    DisplayName = "Juan Perez",
                    Currency = fakePlayFabService.Currency,
                    PriceStore = 5000,
                    CustomData = "{\"FutbolTeamID\":\"2\",\"Name\":\"Juan\",\"LastName\":\"Perez\",\"Birthdate\":\"2020-02-24T20:07:42.8397804-03:00\",\"Price\":5000,\"Position\":1,\"IsStarter\":false,\"id\":\"2\"}"
                }
            };
            fakePlayFabService.Inventory = new List<PlayFabItemInventory> {
                new PlayFabItemInventory
                {
                    ItemId = "1",
                    ItemInstanceId = "1",
                    PurchaseDate = new System.DateTime(2020, 1, 5, 13, 1, 0),
                    DisplayName = "Kenneth Johnston",
                    Currency = fakePlayFabService.Currency,
                    PriceStore = 4000,
                    CustomDataStore= "{\"FutbolTeamID\":\"1\",\"Name\":\"Kenneth\",\"LastName\":\"Johnston\",\"Birthdate\":\"2020-02-24T20:07:42.8397804-03:00\",\"Price\":4000,\"Position\":1,\"IsStarter\":false,\"id\":\"1\"}",
                    CustomDataInventory = new CustomDataInventory
                    {
                        IsStarter = false
                    }
                }
            };
            fakePlayFabService.Budget = 10000;

            //act
            var result = await fantasySoccerService.BuyFutbolPlayerAsync("2", 5000);

            //assert
            Assert.Equal("5000", result.Substring(3));
        }

        [Fact]
        public async Task UpdateUserTeamAsync_UserTeamHasLessThan11Starters_ReturnsException()
        {
            //arrange
            var userTeam = new UserTeam
            {
                ID = "1",
                UserPlayFabID = fakePlayFabService.PlayFabId,
                MatchdayScores = new Dictionary<int, int> { },
                Players = DataHelper.GetDummyPlayers(1, 1)
            };

            //act
            async Task actual()
            {
                await fantasySoccerService.UpdateUserTeamAsync(userTeam);
            }

            //assert
            await Assert.ThrowsAsync<Exception>(actual);
        }

        [Fact]
        public async Task GetMatches_GetMatchesForARound_ReturnTheTwoMatchesForTheRound()
        {
            //arrange
            const int ROUND = 1;
            const string TOURNAMENT_ID = "1";
            fakeCosmosDBService.Matches = DataHelper.GetDummyMatches(2, TOURNAMENT_ID, ROUND);

            //act
            var result = await fantasySoccerService.GetMatches(TOURNAMENT_ID, ROUND);

            //assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateUserTeamAsync_UserTeamHas11StartersAnd2Subs_UserTeamUpdatedSuccessfully()
        {
            //arrange
            var starters = 11;
            var subs = 2;
            var userTeam = new UserTeam
            {
                ID = "1",
                UserPlayFabID = fakePlayFabService.PlayFabId,
                MatchdayScores = new Dictionary<int, int> { },
                Players = DataHelper.GetDummyPlayers(starters, subs)
            };
            
            fakePlayFabService.Inventory = DataHelper.GetDummyInventory(starters, subs, fakePlayFabService.Currency);
            fakePlayFabService.Inventory[0].CustomDataInventory.IsStarter = false;
            fakePlayFabService.Inventory[11].CustomDataInventory.IsStarter = true;

            //act
            await fantasySoccerService.UpdateUserTeamAsync(userTeam);

            //assert
            Assert.True(fakePlayFabService.Inventory[0].CustomDataInventory.IsStarter);
            Assert.False(fakePlayFabService.Inventory[11].CustomDataInventory.IsStarter);
        }

        [Fact]
        public async Task GetUserTransactionsAsync_UserHaveTransactions_ReturnsTransactionsWithPlayerNames()
        {
            //arrange
            var expectedQuantity = 5;
            fakeCosmosDBService.UserTransactions = DataHelper.GetDummyUserTransactions(expectedQuantity, fakePlayFabService.Currency);
            fakePlayFabService.Store = DataHelper.GetDummyStoreByTeams(expectedQuantity, fakePlayFabService.Currency);
            
            //act
            var userTransactions = await fantasySoccerService.GetUserTransactionsAsync();
            var actualQuantity = userTransactions.Count;

            //assert
            Assert.Equal(expectedQuantity, actualQuantity);
            Assert.All(userTransactions, item => Assert.True(!string.IsNullOrWhiteSpace(item.InvolvedFutbolPlayerFullName)));
        }

        [Fact]
        public async Task GetUserTeamStatisticAsync_ThereAreNotTournaments_ReturnsException()
        {
            //act            
            async Task actual()
            {
                await fantasySoccerService.GetUserTeamStatisticAsync();
            }

            //assert
            await Assert.ThrowsAsync<Exception>(actual);           
        }

        [Fact]
        public async Task GetUserTeamStatisticAsync_ThereAreTournamentsButNotStatistics_ReturnsNull()
        {
            //arrange
            fakeCosmosDBService.Tournaments = DataHelper.GetDummyTournaments(1);

            //act            
            var userStatistics = await fantasySoccerService.GetUserTeamStatisticAsync();

            //assert
            Assert.Null(userStatistics);
        }

        [Fact]
        public async Task GetUserTeamStatisticAsync_ThereAreTournamentsAndStatistics_ReturnsPlayerStatistics()
        {
            //arrange
            var expectedQuantity = 4;
            fakeCosmosDBService.Tournaments = DataHelper.GetDummyTournaments(1);
            fakePlayFabService.Statistics = DataHelper.GetDummyStatisticsForTournament(fakeCosmosDBService.Tournaments[0].ID, 1);

            //act            
            var userStatistics = await fantasySoccerService.GetUserTeamStatisticAsync();
            var actualQuantity = userStatistics.StatisticItems.Count;

            //assert
            Assert.Equal(expectedQuantity, actualQuantity);
        }


        [Fact]
        public async Task GetUserTeamStatisticAsync_CurrentRoundIsTwoAndThereAreTwoStatistics_AverageCalculatedSuccessfully()
        {
            //arrange
            const string STATISTIC_NAME = "Average round score";
            var expectedAverage = 15;
            fakeCosmosDBService.Tournaments = DataHelper.GetDummyTournaments(1);
            fakeCosmosDBService.Tournaments[0].CurrentRound = 2;
            fakePlayFabService.Statistics = DataHelper.GetDummyStatisticsForTournament(fakeCosmosDBService.Tournaments[0].ID, 2);
            fakePlayFabService.Statistics[0].Value = 10;
            fakePlayFabService.Statistics[1].Value = 20;

            //act            
            var userStatistics = await fantasySoccerService.GetUserTeamStatisticAsync();
            var actualAverage = userStatistics.StatisticItems.FirstOrDefault(s => s.Description == STATISTIC_NAME).Score;

            //assert
            Assert.Equal(expectedAverage, actualAverage);
        }
    }
}
