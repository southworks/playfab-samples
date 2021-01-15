using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSeeder.Configuration;
using DataSeeder.Configurer;
using FantasySoccer.Core.Services;
using FantasySoccerDataSeeder.Services.Fakers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace DataSeeder
{
    public class Program
    {
        private static DataSeederConfig dataSeederConfig;

        public static async Task Main(string[] args)
        {
            InitConfiguration(args);
            await FakeDataAsync();
        }

        public static async Task FakeDataAsync()
        {
            #region Fake

            Console.Write("Mocking data... ");

            var futbolPlayerFaker = new FutbolPlayerFakerService(dataSeederConfig.TournamentConfig);
            var futbolPlayers = futbolPlayerFaker.GenerateFutbolPlayers();

            var tournamentFaker = new TournamentFakerService(dataSeederConfig.TournamentConfig, futbolPlayers);
            tournamentFaker.GenerateTournaments();
            var tournament = tournamentFaker.Tournament;
            var matches = tournamentFaker.Matches;

            var teams = tournament.FutbolTeams;
            var playersForTeam = dataSeederConfig.TournamentConfig.TeamStartersAmount + dataSeederConfig.TournamentConfig.TeamSubsAmount;
            var teamId = 1;
            var countPlayers = 0;

            foreach (var player in futbolPlayers)
            {
                player.FutbolTeamID = teamId.ToString();

                countPlayers++;
                if (countPlayers == playersForTeam)
                {
                    countPlayers = 0;
                    teamId++;
                }
            }

            var userDataFaker = new UserDataFakerService(dataSeederConfig.UserDataConfig, futbolPlayers);
            var userTeamData = userDataFaker.GenerateUserTeams();
            var userTransactionData = userDataFaker.GenerateUserTransactions();

            Console.WriteLine("Mocking data... Done");

            #endregion

            #region Populate CosmosDB

            Console.Write("Connecting to PlayFab and Cosmos DB... ");
            const int batchSize = 50;

            var playFabClient = new PlayFabService(dataSeederConfig.ConfigPlayFab, new MemoryCache(new MemoryCacheOptions()));

            var cosmosClient = new FantasySoccerDataSeeder.Services.CosmosDBService(dataSeederConfig.ConfigCosmosDB);
            await cosmosClient.CreateDatabaseIfNotExistsAsync(CosmosDBConstants.DatabaseName);

            Console.WriteLine("Done");

            Console.Write("Inserting Tornament into CosmosDB... ");

            try
            {
                await cosmosClient.CreateContainerAsync(CosmosDBConstants.DatabaseName, CosmosDBConstants.TournamentContainerName, CosmosDBConstants.PartitionKey);
                var tournamentContainer = cosmosClient.GetContainer(CosmosDBConstants.DatabaseName, CosmosDBConstants.TournamentContainerName);

                var tournamentString = JsonConvert.SerializeObject(tournament);
                var stream = new MemoryStream(Encoding.ASCII.GetBytes(tournamentString));

                await cosmosClient.AddItemStreamAsync(tournamentContainer, stream, tournament.ID);

                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION when inserting Tournament: {ex.Message}");
            }

            try
            {
                Console.Write("Inserting FutbolPlayers into CosmosDB... ");

                //-CosmosDB
                await cosmosClient.CreateContainerAsync(CosmosDBConstants.DatabaseName, CosmosDBConstants.FutbolPlayerContainer, CosmosDBConstants.PartitionKey);
                var futbolPlayersContainer = cosmosClient.GetContainer(CosmosDBConstants.DatabaseName, CosmosDBConstants.FutbolPlayerContainer);

                var totalPlayers = futbolPlayers.Count;
                var iPlayers = 0;
                var batchPlayers = batchSize;

                while (iPlayers < totalPlayers)
                {
                    var playersToSend = futbolPlayers.Skip(iPlayers).Take(batchPlayers).ToList();
                    await cosmosClient.AddBulkItemsAsync(futbolPlayersContainer, playersToSend);
                    iPlayers += batchPlayers;
                }

                Console.WriteLine("Done");

                Console.Write("Inserting FutbolPlayers into PlayFab... ");

                //-PlayFab
                await playFabClient.SetCatalogItems(futbolPlayers);
                await playFabClient.SetStoreItems(futbolPlayers);
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION when inserting Futbol Players: {ex.Message}");
            }

            try
            {
                Console.Write("Inserting FutbolTeams into CosmosDB... ");

                await cosmosClient.CreateContainerAsync(CosmosDBConstants.DatabaseName, CosmosDBConstants.FutbolTeamContainer, CosmosDBConstants.PartitionKey);
                var futbolTeamsContainer = cosmosClient.GetContainer(CosmosDBConstants.DatabaseName, CosmosDBConstants.FutbolTeamContainer);

                var totalTeams = teams.Count;
                var iTeams = 0;
                var batchTeams = batchSize;

                while (iTeams < totalTeams)
                {
                    var teamsToSend = teams.Skip(iTeams).Take(batchTeams).ToList();
                    await cosmosClient.AddBulkItemsAsync(futbolTeamsContainer, teamsToSend);
                    iTeams += batchTeams;
                }
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION when inserting Futbol Teams: {ex.Message}");
            }

            try
            {
                Console.Write("Inserting Matches into CosmosDB... ");
                await cosmosClient.CreateContainerAsync(CosmosDBConstants.DatabaseName, CosmosDBConstants.MatchContainer, CosmosDBConstants.PartitionKey);
                var matchesContainer = cosmosClient.GetContainer(CosmosDBConstants.DatabaseName, CosmosDBConstants.MatchContainer);

                var totalMatches = matches.Count;
                var iMatches = 0;
                var batchMatches = batchSize;

                while (iMatches < totalMatches)
                {
                    var matchesToSend = matches.Skip(iMatches).Take(batchMatches).ToList();
                    await cosmosClient.AddBulkItemsAsync(matchesContainer, matchesToSend);
                    iMatches += batchMatches;
                }

                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION when inserting Matches: {ex.Message}");
            }

            #endregion

            #region Populate PlayFab Title

            try
            {
                Console.Write("Inserting UserTeam into PlayFab... ");

                //-PlayFab
                //Clean inventory
                var inventory = await playFabClient.GetUserInventoryUsingAdminAPI(dataSeederConfig.PlayFabId);
                await playFabClient.RevokeInventoryItems(dataSeederConfig.PlayFabId, inventory);

                var userTeam = userTeamData.FirstOrDefault();
                var starters = userTeam.Players.Take(dataSeederConfig.TournamentConfig.TeamStartersAmount).ToList();
                var subs = userTeam.Players.Skip(dataSeederConfig.TournamentConfig.TeamStartersAmount).Take(dataSeederConfig.TournamentConfig.TeamSubsAmount).ToList();

                //Starters
                var grantedStarters = await playFabClient.GrantItemstoUser(dataSeederConfig.PlayFabId, starters);

                var starterData = new Dictionary<string, string> { { "IsStarter", "true" } };

                foreach (var item in grantedStarters)
                {
                    await playFabClient.UpdateUserInventoryItemCustomData(dataSeederConfig.PlayFabId, item.ItemInstanceId, starterData);
                }

                //Substitutes
                var grantedSubstitutes = await playFabClient.GrantItemstoUser(dataSeederConfig.PlayFabId, subs);

                var substituteData = new Dictionary<string, string> { { "IsStarter", "false" } };

                foreach (var item in grantedSubstitutes)
                {
                    await playFabClient.UpdateUserInventoryItemCustomData(dataSeederConfig.PlayFabId, item.ItemInstanceId, substituteData);
                }

                //User statistics
                var statistics = new Dictionary<string, int> { { "TournamentScore", 2100 } };
                await playFabClient.UpdatePlayerStatistics(dataSeederConfig.PlayFabId, statistics);

                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION when inserting User Teams: {ex.Message}");
            }

            try
            {
                Console.Write("Inserting UserTransactions into Cosmos DB... ");
                await cosmosClient.CreateContainerAsync(CosmosDBConstants.DatabaseName, CosmosDBConstants.UserTransactionContainer, CosmosDBConstants.PartitionKey);
                var userTransactionC = cosmosClient.GetContainer(CosmosDBConstants.DatabaseName, CosmosDBConstants.UserTransactionContainer);

                var totalUserTransactions = userTransactionData.Count;
                var iUserTransactions = 0;
                var batchUserTransactions = batchSize;

                while (iUserTransactions < totalUserTransactions)
                {
                    var userTransactionsToSend = userTransactionData.Skip(iUserTransactions).Take(batchUserTransactions).ToList();
                    await cosmosClient.AddBulkItemsAsync(userTransactionC, userTransactionsToSend);
                    iUserTransactions += batchUserTransactions;
                }
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION when inserting User Transactions: {ex.Message}");
            }

            try
            {
                Console.Write("Generating MatchPlayerPerformance container into Cosmos DB... ");
                await cosmosClient.CreateContainerAsync(CosmosDBConstants.DatabaseName, CosmosDBConstants.MatchPlayerPerformanceContainerId, CosmosDBConstants.PartitionKey);
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION when generating MatchPlayerPerformance Container: {ex.Message}");
            }

            #endregion
        }

        private static void InitConfiguration(string[] args)
        {
            dataSeederConfig = args.Count() > 0 ? new CLIArgumentsConfigurer(args).Configure() :
                new AppSettingsConfigurer().Configure();

            CosmosDBConstants.DatabaseName = dataSeederConfig.DatabaseName;
        }
    }
}
