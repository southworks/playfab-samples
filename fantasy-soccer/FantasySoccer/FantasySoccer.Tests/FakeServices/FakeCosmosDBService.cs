using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasySoccer.Core.Configuration;
using FantasySoccer.Core.Services;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.CosmosDB;
using Microsoft.Azure.Cosmos;
using Moq;

namespace FantasySoccer.Tests.FakeServices
{
    public class FakeCosmosDBService
    {
        public List<FutbolTeam> FutbolTeams { get; set; }
        public List<Schema.Models.CosmosDB.Match> Matches { get; set; } = new List<Schema.Models.CosmosDB.Match>();
        public List<MatchFutbolPlayerPerformance> MatchFutbolPlayerPerformances { get; set; } = new List<MatchFutbolPlayerPerformance>();
        public List<Tournament> Tournaments { get; set; } = new List<Tournament>();
        public List<UserTransaction> UserTransactions { get; set; } = new List<UserTransaction>();

        public readonly Mock<ICosmosDBService> stub;

        public FakeCosmosDBService()
        {
            stub = new Mock<ICosmosDBService>();
            var mockContainer = new Mock<Container>();

            InitInstances();

            stub.Setup(x => x.CreateContainerAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .Returns((string containerId, string partitionId) => Task.CompletedTask);

            stub.Setup(x => x.DeleteContainerAsync(It.IsAny<Container>()))
                  .Returns((Container c) => clearList(c.Id));

            stub.Setup(x => x.GetContainer(It.IsAny<string>()))
                    .Callback((string id) => mockContainer.SetupGet(x => x.Id).Returns(id))
                    .Returns(mockContainer.Object);

            stub.Setup(x => x.AddItemsAsync(It.IsAny<Container>(), It.IsAny<Tournament>(), It.IsAny<string>(), It.IsAny<ItemRequestOptions>()))
                  .Returns((Container c, Tournament item, string partitionKey, ItemRequestOptions option) => AddTournament(item));
            stub.Setup(x => x.AddItemsAsync(It.IsAny<Container>(), It.IsAny<UserTransaction>(), It.IsAny<string>(), It.IsAny<ItemRequestOptions>()))
                   .Returns((Container c, UserTransaction item, string partitionKey, ItemRequestOptions option) => AddUserTransaction(item));

            stub.Setup(x => x.UpdateItemAsync(It.IsAny<Container>(), It.IsAny<GeneralModel>(), It.IsAny<ItemRequestOptions>()))
                  .Returns((Container c, Tournament item, ItemRequestOptions option) => UpdateTournament(item));

            stub.Setup(x => x.UpdateItemsAsync(It.IsAny<Container>(), It.IsAny<List<Schema.Models.CosmosDB.Match>>()))
                  .Returns((Container c, List<Schema.Models.CosmosDB.Match> items) => UpdateMatches(items));

            stub.Setup(x => x.GetItemById<Tournament>(It.IsAny<Container>(), It.IsAny<string>()))
                    .ReturnsAsync((Container c, string id) => Tournaments.FirstOrDefault(t => t.ID == id));

            stub.Setup(x => x.AddBulkItemsAsync(It.IsAny<Container>(), It.IsAny<List<FutbolTeam>>()))
                  .Returns((Container c, List<FutbolTeam> items) => AddFutbolTeams(items));
            stub.Setup(x => x.AddBulkItemsAsync(It.IsAny<Container>(), It.IsAny<List<Schema.Models.CosmosDB.Match>>()))
                   .Returns((Container c, List<Schema.Models.CosmosDB.Match> items) => AddMatches(items));
            stub.Setup(x => x.AddBulkItemsAsync(It.IsAny<Container>(), It.IsAny<List<MatchFutbolPlayerPerformance>>()))
                   .Returns((Container c, List<MatchFutbolPlayerPerformance> items) => AddMatchFutbolPlayerPerformances(items));

            stub.Setup(x => x.GetItemsAsync<FutbolTeam>(It.IsAny<Container>(), It.IsAny<string>()))
                    .ReturnsAsync((Container c, string q) => FutbolTeams);
            stub.Setup(x => x.GetItemsAsync<Schema.Models.CosmosDB.Match>(It.IsAny<Container>(), It.IsAny<string>()))
                   .ReturnsAsync((Container c, string q) => Matches);
            stub.Setup(x => x.GetItemsAsync<MatchFutbolPlayerPerformance>(It.IsAny<Container>(), It.IsAny<string>()))
                    .ReturnsAsync((Container c, string q) => MatchFutbolPlayerPerformances);
            stub.Setup(x => x.GetItemsAsync<Tournament>(It.IsAny<Container>(), It.IsAny<string>()))
                    .ReturnsAsync((Container c, string q) => Tournaments);
            stub.Setup(x => x.GetItemsAsync<UserTransaction>(It.IsAny<Container>(), It.IsAny<string>()))
                    .ReturnsAsync((Container c, string q) => UserTransactions);
            stub.Setup(x => x.GetItemsAsync<Dictionary<string, int>>(It.IsAny<Container>(), It.IsAny<string>()))
                    .ReturnsAsync((Container c, string q) => GetCalculatedData());
        }

        private void InitInstances()
        {
            FutbolTeams = new List<FutbolTeam>
            {
                new FutbolTeam {
                    ID = "1",
                    Name = "Lorem",
                    FutbolPlayersId = new int[0]
                },
                new FutbolTeam {
                    ID = "2",
                    Name = "Ipsum",
                    FutbolPlayersId = new int[0]
                }
            };         
        }

        private Task clearList(string containerName)
        {
            switch (containerName)
            {
                case CosmosDBConstants.FutbolTeamId:
                    FutbolTeams.Clear();
                    break;
                case CosmosDBConstants.MatchContainerId:
                    Matches.Clear();
                    break;
                case CosmosDBConstants.MatchPlayerPerformanceContainerId:
                    MatchFutbolPlayerPerformances.Clear();
                    break;
                case CosmosDBConstants.TournamentContainerId:
                    Tournaments.Clear();
                    break;
                case CosmosDBConstants.UserTransactionId:
                    UserTransactions.Clear();
                    break;
            }
            return Task.CompletedTask;
        }

        private List<Dictionary<string, int>> GetCalculatedData()
        {
            return new List<Dictionary<string, int>>
            {
                new Dictionary<string, int>()
                {
                    { "NumberOfRounds", Matches.Any() ? Matches.Max(m => m.Round) : 0},
                    { "CurrentId", Tournaments.Any() ? Tournaments.Max(t => int.Parse(t.ID)) : 0 }
                }
            };
        }

        private Task AddFutbolTeams(List<FutbolTeam> teams)
        {
            FutbolTeams.AddRange(teams);
            return Task.CompletedTask;
        }

        private Task AddMatches(List<Schema.Models.CosmosDB.Match> matches)
        {
            Matches.AddRange(matches);
            return Task.CompletedTask;
        }

        private Task AddMatchFutbolPlayerPerformances(List<MatchFutbolPlayerPerformance> performances)
        {
            MatchFutbolPlayerPerformances.AddRange(performances);
            return Task.CompletedTask;
        }

        private Task AddTournament(Tournament tournament)
        {
            Tournaments.Add(tournament);
            return Task.CompletedTask;
        }

        private Task AddUserTransaction(UserTransaction userTransaction)
        {
            UserTransactions.Add(userTransaction);
            return Task.CompletedTask;
        }

        private Task UpdateMatches(List<Schema.Models.CosmosDB.Match> matches)
        {
            Matches = Matches.GroupJoin(matches, 
                M => M.ID, 
                m => m.ID,
                (M, m) => new
                {
                    Original = M,
                    Changed = m
                })
                .SelectMany(temp => temp.Changed.DefaultIfEmpty(),
                    (x, y) => y ?? x.Original)
                .ToList();

            return Task.CompletedTask;
        }

        private Task UpdateTournament(Tournament tournament)
        {
            var index = Tournaments.FindIndex(t => t.ID == tournament.ID);
            if (index != -1)
            {
                Tournaments[index] = tournament;
            }
            return Task.CompletedTask;
        }
    }
}
