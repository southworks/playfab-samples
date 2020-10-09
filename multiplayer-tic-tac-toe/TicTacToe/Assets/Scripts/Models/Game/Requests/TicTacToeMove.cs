using System;

namespace TicTacToe.Models.Game.Requests
{
    [Serializable]

    public class TicTacToeMove
    {
        public int row { get; set; }
        public int col { get; set; }

        public bool Valid
        {
            get
            {
                return row != -1 && col != -1;
            }
        }
    }
}