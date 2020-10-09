namespace TicTacToeFunctions.Models.Game.Responses
{
    public class MakeMultiplayerMoveResponse
    {
        public GameState GameState { get; set; }

        public MakePlayerMoveResponse PlayerMoveResult { get; set; }
    }
}