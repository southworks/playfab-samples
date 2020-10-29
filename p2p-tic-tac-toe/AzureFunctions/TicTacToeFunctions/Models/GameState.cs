namespace TicTacToeFunctions.Models
{
    public class GameState
    {
        public int[] BoardState { get; set; }

        public string CurrentPlayerId { get; set; }

        public int Winner { get; set; }
    }
}