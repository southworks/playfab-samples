// Copyright (C) Microsoft Corporation. All rights reserved.

namespace TicTacToeFunctions.Models.Requests
{
    public class MakeMultiplayerMoveRequest
    {
        public string SharedGroupId { get; set; }

        public string PlayerId { get; set; }

        public TicTacToeMove PlayerMove { get; set; }
    }
}