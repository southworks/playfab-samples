using System;
using System.Collections.Generic;

namespace TicTacToe.Models.Helpers
{
    public class MatchmakingQueueHelper
    {
        private static readonly Lazy<Dictionary<QueueTypes, Func<IMatchmakingHandler, MatchmakingQueueConfiguration>>> queueConfigurationTypes =
            new Lazy<Dictionary<QueueTypes, Func<IMatchmakingHandler, MatchmakingQueueConfiguration>>>(() =>
                new Dictionary<QueueTypes, Func<IMatchmakingHandler, MatchmakingQueueConfiguration>>
                {
                    { QueueTypes.QuickMatch, (queueConfig) => new QuickMatchQueueConfiguration(queueConfig) },
                    { QueueTypes.Party, (queueConfig) => new PartyQueueConfiguration(queueConfig) },
                }
            );

        public static MatchmakingQueueConfiguration CreateQueue(QueueTypes type, IMatchmakingHandler matchmakingHandler)
        {
            queueConfigurationTypes.Value.TryGetValue(type, out var func);
            return func != null ? func(matchmakingHandler) : null;
        }
    }
}
