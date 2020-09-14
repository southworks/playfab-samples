// Copyright (C) Microsoft Corporation. All rights reserved.

using System;

namespace TicTacToe.Models.Responses
{
    [Serializable]
    public class MakeMultiplayerMoveResponse
    {
        public GameState gameState;

        public MakePlayerMoveResult playerMoveResult;
    }
}