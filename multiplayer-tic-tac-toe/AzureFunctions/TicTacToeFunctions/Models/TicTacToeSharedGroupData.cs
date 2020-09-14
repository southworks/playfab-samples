// Copyright (C) Microsoft Corporation. All rights reserved.

namespace TicTacToeFunctions.Models
{
    public class TicTacToeSharedGroupData
    {
        public string SharedGroupId { get; set; }

        public GameState GameState { get; set; }

        public Match Match { get; set; }

        public MatchLobby MatchLobby { get; set; }
    }
}