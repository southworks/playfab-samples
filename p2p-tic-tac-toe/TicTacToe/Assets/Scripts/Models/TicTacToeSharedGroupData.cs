using System;

namespace TicTacToe.Models
{
    [Serializable]
    public class TicTacToeSharedGroupData
    {
        public string sharedGroupId;

        public GameState gameState;

        public Match match;

        public MatchLobby matchLobby;
    }
}