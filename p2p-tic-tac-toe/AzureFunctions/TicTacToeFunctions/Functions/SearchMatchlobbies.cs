using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using TicTacToeFunctions.Models.Requests;
using TicTacToeFunctions.Models.Responses;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions
{
    public static class SearchMatchLobbies
    {
        [FunctionName("SearchMatchLobbies")]
        public static async Task<Wrapper<MatchLobbyDTO>> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(ConnectionStringSetting = "PlayFabTicTacToeCosmosDB")] DocumentClient cosmosDBClient)
        {
            var context = await FunctionContext<SearchMatchLobbiesRequest>.Create(req);
            var lobbyListRequest = context.FunctionArgument;

            var result = await MatchLobbyUtil.GetMatchLobbiesDTOAsync(
                cosmosDBClient,
                ExpressionUtils.GetSearchMatchLobbiesExpression(lobbyListRequest.SearchTerm));

            return new Wrapper<MatchLobbyDTO>
            {
                Items = result
            };
        }
    }
}
