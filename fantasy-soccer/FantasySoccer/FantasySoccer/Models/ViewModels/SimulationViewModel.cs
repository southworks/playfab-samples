using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FantasySoccer.Models.ViewModels
{
    public class SimulationViewModel
    {
        public string TournamentId { get; set; }
        public string TournamentName { get; set; }
        public int CurrentRound { get; set; }
        public IEnumerable<SelectListItem> Rounds { get; set; }
    }
}
