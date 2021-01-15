using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Core.Services
{
    public class SimulationService: ISimulationService
    {
        private readonly IFantasySoccerService fantasySoccerService;
        private static readonly Random random = new Random();

        public SimulationService(IFantasySoccerService fantasySoccerService)
        {
            this.fantasySoccerService = fantasySoccerService;
        }

        public async Task<List<MatchSimulationResult>> SimulateTournamentRound(string tournamentId, int round)
        {
            var matches = await fantasySoccerService.GetMatches(tournamentId, round);
            var matchSimulationResults = new List<MatchSimulationResult>();

            var futbolPlayers = await fantasySoccerService.GetFutbolPlayersStoreAsync();

            var futbolPlayersGroupByTeam = futbolPlayers.PaginatedItems.ToLookup(fp => fp.FutbolTeamID, fp => fp);

            foreach (var match in matches)
            {
                var localTeam = futbolPlayersGroupByTeam[match.LocalFutbolTeamID];
                var visitorTeam = futbolPlayersGroupByTeam[match.VisitorFutbolTeamID];
                matchSimulationResults.Add(SimulateMatch(match, localTeam.ToList(), visitorTeam.ToList()));
            }

            return matchSimulationResults;
        }

        public MatchSimulationResult SimulateMatch(Match match, List<FutbolPlayer> localTeam, List<FutbolPlayer> visitorTeam)
        {
            localTeam = localTeam.Take(SimulatorConstants.PlayersOnTheField).OrderBy(fp => (int)fp.Position).ToList();
            visitorTeam = visitorTeam.Take(SimulatorConstants.PlayersOnTheField).OrderBy(fp => (int)fp.Position).ToList();

            SetGoalsForMatch(match, localTeam, visitorTeam);

            var simulationResult = new MatchSimulationResult
            {
                Match = match,
                PlayersPerformance = MatchSimulation.SimulateMatch(match, localTeam, visitorTeam)
            };

            return simulationResult;
        }

        private static void SetGoalsForMatch(Match match, List<FutbolPlayer> localTeam, List<FutbolPlayer> visitorTeam)
        {
            var localWins = localTeam.Sum(p => p.GeneralStats.Wins - p.GeneralStats.Losses);
            var VisitorWins = visitorTeam.Sum(p => p.GeneralStats.Wins - p.GeneralStats.Losses);

            var probabilitiesOfWinning = new List<string>();
            switch (localWins - VisitorWins)
            {
                case int n when (n > 0):
                    probabilitiesOfWinning.AddRange(Enumerable.Repeat("local", SimulatorConstants.ProbabilityOfWinningForFavorite).ToList());
                    probabilitiesOfWinning.AddRange(Enumerable.Repeat("visitor", SimulatorConstants.ProbabilityOfWinningForLessFavorite).ToList());
                    break;
                case int n when (n < 0):
                    probabilitiesOfWinning.AddRange(Enumerable.Repeat("Local", SimulatorConstants.ProbabilityOfWinningForLessFavorite).ToList());
                    probabilitiesOfWinning.AddRange(Enumerable.Repeat("visitor", SimulatorConstants.ProbabilityOfWinningForFavorite).ToList());
                    break;
                default:
                    probabilitiesOfWinning.AddRange(Enumerable.Repeat("Local", SimulatorConstants.ProbabilityOfWinningEqual).ToList());
                    probabilitiesOfWinning.AddRange(Enumerable.Repeat("visitor", SimulatorConstants.ProbabilityOfWinningEqual).ToList());
                    break;
            }

            var winner = probabilitiesOfWinning[random.Next(probabilitiesOfWinning.Count)];

            var goalResultA = random.Next(0, 5);
            var goalResultB = random.Next(0, 5);

            if (winner == "local")
            {
                match.LocalGoals = Math.Max(goalResultA, goalResultB);
                match.VisitorGoals = Math.Min(goalResultA, goalResultB);
            }
            else
            {
                match.LocalGoals = Math.Min(goalResultA, goalResultB);
                match.VisitorGoals = Math.Max(goalResultA, goalResultB);
            }
        }
    }
}
