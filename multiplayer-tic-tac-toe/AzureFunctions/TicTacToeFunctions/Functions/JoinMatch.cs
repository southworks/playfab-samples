using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions
{
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
