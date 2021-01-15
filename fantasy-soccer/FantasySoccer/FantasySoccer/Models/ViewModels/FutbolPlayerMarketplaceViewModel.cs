using System.Linq;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Models.ViewModels
{
    public class FutbolPlayerMarketplaceViewModel
    {
        public UserTeam UserTeam { get; set; }

        public FutbolPlayer Player { get; set; }

        public FutbolTeam Team { get; set; }

        public bool UserTeamContainsPlayer(FutbolPlayer player) => UserTeam?.Players != null &&  UserTeam.Players.Any(teamPlayer => teamPlayer.ID == player.ID);
    }
}
