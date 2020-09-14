// Copyright (C) Microsoft Corporation. All rights reserved.

namespace TicTacToeFunctions.Models
{
    public class MatchLobby
    {
        public string id { get => MatchLobbyId; }

        public string MatchLobbyId { get; set; }

        public int CurrentAvailability { get; set; }
    }
}