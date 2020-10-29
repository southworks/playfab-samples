using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using TicTacToeFunctions.Models.Exceptions;
using TicTacToeFunctions.Models.Requests;
using TicTacToeFunctions.Models.Responses;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions
{
    public class SetMatchLobbyLockState
    {
        [FunctionName("SetMatchLobbyLockState")]
        public static async Task<ResponseWrapper<SetMatchLobbyLockStateResponse>> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(ConnectionStringSetting = "PlayFabTicTacToeCosmosDB")] DocumentClient cosmosDBClient)
        {
            var context = await FunctionContext<SetMatchLobbyLockStateRequest>.Create(req);
            var matchLobbyId = context.FunctionArgument.MatchLobbyId;
            var locked = context.FunctionArgument.Locked;
            var creatorId = context.CallerEntityProfile.Entity.Id;

            try
            {
                var matchLobby = await MatchLobbyUtil.SetMatchLobbyLockState(matchLobbyId, locked, creatorId, cosmosDBClient);

                return new ResponseWrapper<SetMatchLobbyLockStateResponse> { StatusCode = StatusCode.OK, Response = new SetMatchLobbyLockStateResponse { MatchLobby = matchLobby } };
            }
            catch (TicTacToeException exception)
            {
                return TicTacToeExceptionUtil.GetEmptyResponseWrapperFromException<SetMatchLobbyLockStateResponse>(exception);
            }
        }
    }
}
