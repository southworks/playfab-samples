using System.Collections.Generic;
using FantasySoccer.Schema.Models.CosmosDB;

namespace FantasySoccer.Core.Services
{
    public class MatchSimulationResult
    {
        public Match Match { get; set; }
        public List<MatchFutbolPlayerPerformance> PlayersPerformance { get; set; }
    }
}
