using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.Plugins.CloudScript;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models.Game.Requests;
using TicTacToeFunctions.Models.Game.Responses;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions.Game
{
    public static class MakeMultiplayerMove
    {
        [FunctionName("MakeMultiplayerMove")]
        public static async Task<MakeMultiplayerMoveResponse> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            var context = await FunctionContext<MakeMultiplayerMoveRequest>.Create(req);
            var playerMoveRequest = context.FunctionArgument;

            var updatedData = await GameStateUtil.MoveUpdateAsync(context.AuthenticationContext, playerMoveRequest.PlayerId, playerMoveRequest.PlayerMove, playerMoveRequest.SharedGroupId);

            return new MakeMultiplayerMoveResponse
            {
                GameState = updatedData,
                PlayerMoveResult = new MakePlayerMoveResponse { Valid = updatedData != null }
            };
        }
    }
}
