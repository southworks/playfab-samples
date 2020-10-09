using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Game.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions
{
    public static class StartMatch
    {
        [FunctionName("StartMatch")]
        public static async Task<GameState> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
        {
            var context = await FunctionContext<StartMatchRequest>.Create(req);
            return await GameStateUtil.InitializeAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId);
        }
    }
}
