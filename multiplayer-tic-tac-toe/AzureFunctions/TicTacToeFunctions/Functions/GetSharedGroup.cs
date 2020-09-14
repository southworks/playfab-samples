using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions
{
    public static class GetSharedGroup
    {
        [FunctionName("GetSharedGroup")]
        public static async Task<TicTacToeSharedGroupData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
        {
            var context = await FunctionContext<GetSharedGroupRequest>.Create(req);
            return await SharedGroupDataUtil.GetAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId);
        }
    }
}
