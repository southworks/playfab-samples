using PlayFab.MultiplayerModels;
using TicTacToe.Models;

namespace TicTacToe.Helpers
{
    public class MatchmakingHelper
    {
        public static MatchmakingPlayer GetMatchmakingPlayer(PlayerInfo player, MatchmakingPlayerAttributes attributes)
        {
            return new MatchmakingPlayer
            {
                Attributes = attributes,
                Entity = new EntityKey
                {
                    Id = player.Entity.Id,
                    Type = player.Entity.Type,
                },
            };
        }
    }
}
