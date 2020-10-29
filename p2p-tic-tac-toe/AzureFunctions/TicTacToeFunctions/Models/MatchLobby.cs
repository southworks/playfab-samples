using Newtonsoft.Json;

namespace TicTacToeFunctions.Models
{
    public class MatchLobby : ICustomDocument
    {
        [JsonProperty("id")]
        public string Id { get => MatchLobbyId; }

        [JsonProperty("_etag")]
        public string ETag { get; set; }

        public string MatchLobbyId { get; set; }

        public int CurrentAvailability { get; set; }

        public string NetworkId { get; set; }

        public string CreatorId { get; set; }

        public bool Locked { get; set; }
    }
}