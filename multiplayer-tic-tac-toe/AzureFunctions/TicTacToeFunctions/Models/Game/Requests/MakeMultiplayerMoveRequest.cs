namespace TicTacToeFunctions.Models.Game.Requests
{
    public class MakeMultiplayerMoveRequest
    {
        public string SharedGroupId { get; set; }

        public string PlayerId { get; set; }

        public TicTacToeMove PlayerMove { get; set; }
    }
}