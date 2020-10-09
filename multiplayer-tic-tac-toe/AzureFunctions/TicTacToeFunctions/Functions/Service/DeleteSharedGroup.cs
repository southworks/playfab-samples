using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models.Service.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions.Service
{
    public static class DeleteSharedGroup
    {
        [FunctionName("DeleteSharedGroup")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req
        )
        {
            var context = await FunctionContext<DeleteSharedGroupRequest>.Create(req);
            var request = context.FunctionArgument;

            await SharedGroupDataUtil.DeleteAsync(context.AuthenticationContext, request.SharedGroupId);
        }
    }
}
