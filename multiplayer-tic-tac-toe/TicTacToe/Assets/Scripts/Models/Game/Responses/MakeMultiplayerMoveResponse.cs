using System;

namespace TicTacToe.Models.Game.Responses
{
    [Serializable]
    public class MakeMultiplayerMoveResponse
    {
        public GameState gameState;

        public MakePlayerMoveResult playerMoveResult;
    }
}