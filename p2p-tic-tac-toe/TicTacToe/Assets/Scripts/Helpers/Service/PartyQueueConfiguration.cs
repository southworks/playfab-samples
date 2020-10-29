using PlayFab.MultiplayerModels;

namespace TicTacToe.Models.Helpers
{
    public class PartyQueueConfiguration : MatchmakingQueueConfiguration
    {
        public PartyQueueConfiguration(IMatchmakingHandler matchmakingHandler)
        {
            QueueName = Constants.PARTY_MATCHMAKING_QUEUE_NAME;
            GiveUpAfterSeconds = Constants.GIVE_UP_AFTER_SECONDS;
            EscapeObject = false;
            ReturnMemberAttributes = true;
            MatchmakingHandler = matchmakingHandler;
            QueueType = QueueTypes.Party;
        }

        public override MatchmakingPlayerAttributes GetMatchmakingPlayerAttributes(params string[] stringParams)
        {
            return new MatchmakingPlayerAttributes
            {
                DataObject = new PartyTicketAttributes
                {
                    PreviousMatchId = stringParams?[0] ?? "",
                    NetworkId = stringParams?[1] ?? ""
                }
            };
        }
    }
}
