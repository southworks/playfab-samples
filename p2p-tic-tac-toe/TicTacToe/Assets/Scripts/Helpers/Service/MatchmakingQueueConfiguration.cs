using PlayFab.MultiplayerModels;

namespace TicTacToe.Models.Helpers
{
    public abstract class MatchmakingQueueConfiguration : IMatchmakingQueueConfiguration
    {
        public string QueueName { get; protected set; }

        public int GiveUpAfterSeconds { get; protected set; }

        public bool EscapeObject { get; protected set; }

        public bool ReturnMemberAttributes { get; protected set; }

        public IMatchmakingHandler MatchmakingHandler { get; protected set; }

        public QueueTypes QueueType { get; protected set; }

        public void ChangeQueueConfiguration(QueueTypes queueType)
        {
            MatchmakingHandler.QueueConfiguration = MatchmakingQueueHelper.CreateQueue(queueType, MatchmakingHandler);
        }

        public abstract MatchmakingPlayerAttributes GetMatchmakingPlayerAttributes(params string[] stringParams);
    }
}