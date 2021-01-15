using System.Threading.Tasks;
using FantasySoccer.Core.Services;
using FantasySoccer.Tests.FakeServices;
using Xunit;

namespace FantasySoccer.Tests
{
    public class SimulationServiceTests
    {
        private readonly IFantasySoccerService fantasySoccerService;
        private readonly SimulationService simulationService;
        private readonly FakeCosmosDBService fakeCosmosDBService;
        private readonly FakePlayFabService fakePlayFabService;

        public SimulationServiceTests()
        {
            fakeCosmosDBService = new FakeCosmosDBService();
            fakePlayFabService = new FakePlayFabService();
            fantasySoccerService = new FantasySoccerService(fakeCosmosDBService.stub.Object, fakePlayFabService.stub.Object);
            simulationService = new SimulationService(fantasySoccerService);
        }

        [Theory]
        [InlineData("1", 1)]
        public async Task SimulateTournamentRound_ThereIsAMatchWithTwoTeams_PerformancesGeneratedSuccessfully(string tournamentId, int round)
        {
            //Arrange
            var expectedPerformances = 22;
            fakeCosmosDBService.Matches = DataHelper.GetDummyMatches(1, tournamentId, round);
            fakePlayFabService.Store = DataHelper.GetDummyStoreByTeams(16, fakePlayFabService.Currency, 2);

            //Act
            var result = await simulationService.SimulateTournamentRound(tournamentId, round);

            //Assert
            Assert.Equal(expectedPerformances, result[0].PlayersPerformance.Count);
        }
    }
}
