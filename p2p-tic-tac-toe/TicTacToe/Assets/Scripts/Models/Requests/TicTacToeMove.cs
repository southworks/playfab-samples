using System;

namespace TicTacToe.Models.Requests
{
    [Serializable]
    public class TicTacToeMove
    {
        public int row;
        public int col;

        public bool Valid
        {
            get
            {
                return row != -1 && col != -1;
            }
        }
    }
}