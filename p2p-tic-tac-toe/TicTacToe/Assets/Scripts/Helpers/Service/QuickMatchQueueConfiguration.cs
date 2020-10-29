
using PlayFab.MultiplayerModels;

namespace TicTacToe.Models.Helpers
{
    public class QuickMatchQueueConfiguration : MatchmakingQueueConfiguration
    {
        public QuickMatchQueueConfiguration(IMatchmakingHandler matchmakingHandler)
        {
            QueueName = Constants.QUICK_MATCHMAKING_QUEUE_NAME;
            GiveUpAfterSeconds = Constants.GIVE_UP_AFTER_SECONDS;
            EscapeObject = false;
            ReturnMemberAttributes = true;
            MatchmakingHandler = matchmakingHandler;
            QueueType = QueueTypes.QuickMatch;
        }

        public override MatchmakingPlayerAttributes GetMatchmakingPlayerAttributes(params string[] stringParams)
        {
            return new MatchmakingPlayerAttributes
            {
                DataObject = new { Skill = stringParams?[0] ?? "" }
            };
        }
    }
}
