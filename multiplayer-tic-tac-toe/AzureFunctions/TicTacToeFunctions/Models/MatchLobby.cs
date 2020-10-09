using TicTacToeFunctions.Models.Service.Interfaces;

namespace TicTacToeFunctions.Models
{
    public class MatchLobby : ICustomDocument
    {
        public string id { get => MatchLobbyId; }

        public string MatchLobbyId { get; set; }

        public int CurrentAvailability { get; set; }

        public string _etag { get; set; }
    }
}