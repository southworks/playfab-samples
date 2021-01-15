using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Models.ViewModels
{
    public class MarketPlaceItems
    {
        public FutbolPlayer FutbolPlayer{ get; set; }
        
        public string FutbolTeamName { get; set; }

        public bool UserTeamContainsPlayer { get; set; }
    }
}
