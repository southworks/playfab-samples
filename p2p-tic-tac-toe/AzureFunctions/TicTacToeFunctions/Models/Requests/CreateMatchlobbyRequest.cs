namespace TicTacToeFunctions.Models.Requests
{
    public class CreateMatchLobbyRequest
    {
        public string MatchLobbyId { get; set; }

        public bool Locked { get; set; }

        public string NetworkId { get; set; }
    }
}
