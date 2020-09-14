using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.Plugins.CloudScript;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions
{
    public class CreateSharedGroup
    {
        [FunctionName("CreateSharedGroup")]
        public static async Task<TicTacToeSharedGroupData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            var context = await FunctionContext<CreateSharedGroupRequest>.Create(req);

            // We have to use the MasterPlayerAccountId instead of the TitlePlayerAccountId to avoid errors during its addition as member of SGD
            var playerOne = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            var sgdResponse = await SharedGroupDataUtil.CreateAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId);

            await SharedGroupDataUtil.AddMembersAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId, new List<string> { playerOne });

            var sharedGroupData = new TicTacToeSharedGroupData
            {
                SharedGroupId = sgdResponse.SharedGroupId,
                Match = new Match { PlayerOneId = playerOne }
            };

            return await SharedGroupDataUtil.UpdateAsync(context.AuthenticationContext, sharedGroupData);
        }
    }
}
