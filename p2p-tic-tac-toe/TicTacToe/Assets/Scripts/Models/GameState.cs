using System;
using TicTacToe.Models.Helpers;

namespace TicTacToe.Models
{
    [Serializable]
    public class GameState
    {
        public int[] boardState;

        public string currentPlayerId;

        public int winner;

        public GameState(string currentPlayerId)
        {
            boardState = new int[9];
            this.currentPlayerId = currentPlayerId;
            winner = (int)GameWinnerType.NONE;
        }
    }
}