using System.Collections.Generic;
using System.Threading.Tasks;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Core.Services
{
    public interface ISimulationService
    {
        Task<List<MatchSimulationResult>> SimulateTournamentRound(string tournamentId, int round);

        MatchSimulationResult SimulateMatch(Match match, List<FutbolPlayer> localTeam, List<FutbolPlayer> visitorTeam);
    }
}
