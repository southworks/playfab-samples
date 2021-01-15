using System.Collections.Generic;
using FantasySoccer.Schema.Models.CosmosDB;

namespace FantasySoccer.Models.Responses
{
    public class SimulationResponse
    {
        public string TournamentId { get; set; }
        public string TournamentName { get; set; }
        public int CurrentRound { get; set; }
        public IEnumerable<Match> Matches { get; set; }
    }
}
