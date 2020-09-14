// Copyright (C) Microsoft Corporation. All rights reserved.

using System;

namespace TicTacToe.Models.Requests
{
    [Serializable]
    public class DeleteMatchLobbyInfoRequest
    {
        public string Id;
        public string MatchLobbyId;
    }
}