using System.Collections.Generic;
using FantasySoccer.Schema.Models.CosmosDB;

namespace FantasySoccer.Models.ViewModels
{
    public class CurrentRoundViewModel : SimulationViewModel
    {
        public List<Match> Matches { get; set; }
        public int NumberOfRounds { get; set; }
    }
}
