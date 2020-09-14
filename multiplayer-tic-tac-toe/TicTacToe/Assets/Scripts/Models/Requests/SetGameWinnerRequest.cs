// Copyright (C) Microsoft Corporation. All rights reserved.

namespace TicTacToe.Models.Requests
{
    public class SetGameWinnerRequest
    {
        public string SharedGroupId { get; set; }

        public string PlayerId { get; set; }
    }
}