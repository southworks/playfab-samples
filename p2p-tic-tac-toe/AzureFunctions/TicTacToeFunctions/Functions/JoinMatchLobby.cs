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
    public class JoinMatchLobby
    {
        [FunctionName("JoinMatchLobby")]
        public static async Task<ResponseWrapper<JoinMatchLobbyResponse>> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(ConnectionStringSetting = "PlayFabTicTacToeCosmosDB")] DocumentClient cosmosDBClient)
        {
            var context = await FunctionContext<JoinMatchLobbyRequest>.Create(req);
            var matchLobbyId = context.FunctionArgument.MatchLobbyId;
            var invitationCode = context.FunctionArgument.InvitationCode;
            var playerTwo = context.CallerEntityProfile.Entity.Id;

            try
            {
                var matchLobby = await MatchLobbyUtil.JoinMatchLobby(matchLobbyId, playerTwo, cosmosDBClient, invitationCode);

                return new ResponseWrapper<JoinMatchLobbyResponse> { StatusCode = StatusCode.OK, Response = new JoinMatchLobbyResponse { NetworkId = matchLobby.NetworkId } };
            }
            catch (TicTacToeException exception)
            {
                return TicTacToeExceptionUtil.GetEmptyResponseWrapperFromException<JoinMatchLobbyResponse>(exception);
            }
        }
    }
}
