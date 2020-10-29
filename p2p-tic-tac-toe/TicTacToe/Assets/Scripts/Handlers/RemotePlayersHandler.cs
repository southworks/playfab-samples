using System.Collections.Generic;
using System.Linq;
using PlayFab.Party;
using TicTacToe.Models;

namespace TicTacToe.Handlers
{
    public class RemotePlayersHandler
    {
        public List<RemotePlayer> players { get; private set; }

        public RemotePlayersHandler(PlayFabMultiplayerManager multiplayerManager)
        {
            players = multiplayerManager.RemotePlayers?.Select(p => GetRemotePlayer(p)).ToList() ?? new List<RemotePlayer>(); ;
        }

        public void AddPlayer(PlayFabPlayer player)
        {
            var playerId = player?.EntityKey?.Id ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(playerId) && !players.Exists(p => p.playFabPlayer.EntityKey.Id == playerId))
            {
                players.Add(GetRemotePlayer(player));
            }
        }

        public void RemovePlayer(PlayFabPlayer player)
        {
            var playerId = player?.EntityKey?.Id ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(playerId))
            {
                var result = players.Find(p => p.playFabPlayer.EntityKey.Id == playerId);

                players.Remove(result);
            }
        }

        private RemotePlayer GetRemotePlayer(PlayFabPlayer player)
        {
            // This method will be improved later for adding useful information as the 
            // player NAME
            return new RemotePlayer { playFabPlayer = player };
        }
    }
}
