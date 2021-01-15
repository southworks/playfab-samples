using PlayFab.ServerModels;

namespace FantasySoccer.Core.Models.Functions
{
    public class UserTeamRoundScoreCalculationRequestContext
    {
        public dynamic PlayerProfile { get; set; }

        public UserTeamRoundScoreCalculationRequest FunctionArgument { get; set; }
    }
}
