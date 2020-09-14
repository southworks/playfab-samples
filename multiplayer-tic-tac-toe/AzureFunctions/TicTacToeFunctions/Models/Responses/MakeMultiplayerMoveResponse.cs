// Copyright (C) Microsoft Corporation. All rights reserved.

namespace TicTacToeFunctions.Models.Responses
{
    public class MakeMultiplayerMoveResponse
    {
        public GameState GameState { get; set; }

        public MakePlayerMoveResponse PlayerMoveResult { get; set; }
    }
}