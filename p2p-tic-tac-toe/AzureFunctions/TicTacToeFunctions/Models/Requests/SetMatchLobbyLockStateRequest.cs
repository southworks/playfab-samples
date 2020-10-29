namespace TicTacToeFunctions.Models.Requests
{
    public class SetMatchLobbyLockStateRequest
    {
        public string MatchLobbyId { get; set; }

        public bool Locked { get; set; }
    }
}