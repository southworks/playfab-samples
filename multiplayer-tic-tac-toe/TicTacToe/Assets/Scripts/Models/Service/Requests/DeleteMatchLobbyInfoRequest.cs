using System;

namespace TicTacToe.Models.Service.Requests
{
    [Serializable]
    public class DeleteMatchLobbyInfoRequest
    {
        public string Id;
        public string MatchLobbyId;
    }
}