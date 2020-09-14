// Copyright (C) Microsoft Corporation. All rights reserved.

namespace TicTacToe.Models.Requests
{
    public class MakeMultiplayerMoverRequest
    {
        public string SharedGroupId { get; set; }

        public string PlayerId { get; set; }

        public TicTacToeMove PlayerMove { get; set; }
    }
}