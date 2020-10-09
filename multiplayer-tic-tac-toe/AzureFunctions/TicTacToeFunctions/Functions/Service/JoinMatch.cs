using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Service.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions.Service
{
    /// <disclaimer>
    /// This function is meant to be used in a process where only two players are matched to play against, using
    /// the PlayFab's Matchmaking feature, where only one of these players will be calling it. In case you need
    /// a similar solution but for a situation where more than two players will (or could) be calling this function,
    /// we strongly recommend to check the JoinMatchLobby function of this project, as there we manage some situations
    /// that could be happening in those cases, as, for example, a race condition situation.
    /// </disclaimer>
    public static class JoinMatch
    {
        [FunctionName("JoinMatch")]
        public static async Task<TicTacToeSharedGroupData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
        {
            var context = await FunctionContext<JoinMatchRequest>.Create(req);
            var playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            await SharedGroupDataUtil.AddMembersAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId, new List<string> { playerId });

            return await MatchUtil.AddMember(context.AuthenticationContext, context.FunctionArgument.SharedGroupId, playerId);
        }
    }
}
