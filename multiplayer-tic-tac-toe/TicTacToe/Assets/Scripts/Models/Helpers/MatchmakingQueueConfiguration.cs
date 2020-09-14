// Copyright (C) Microsoft Corporation. All rights reserved.

namespace TicTacToe.Models.Helpers
{
    public class MatchmakingQueueConfiguration
    {
        public string QuickPlayQueueName { get; set; }

        public string MatchLobbyQueueName { get; set; }

        public int GiveUpAfterSeconds { get; set; }

        public bool IsQuickPlay { get; set; }

        public string QueueName => IsQuickPlay ? QuickPlayQueueName : MatchLobbyQueueName;
    }
}