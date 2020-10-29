namespace TicTacToe.Models.Helpers
{
    public enum PartyNetworkMessageEnum
    {
        GameState = 0,
        PlayersReady = 1,
        Move = 2,
        MatchAbandonment = 3,
        MatchLobbyCancelled = 4,
        PlayerKicked = 5
    }
}
