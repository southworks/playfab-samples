using System;

namespace TicTacToe.Models
{
    [Serializable]
    public class MatchLobby
    {
        public string matchLobbyId;

        public string creatorId;

        public bool locked;
    }
}