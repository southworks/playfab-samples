using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Service.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions.Service
{
    public class JoinMatchLobby
    {
        [FunctionName("JoinMatchLobby")]
        public static async Task<TicTacToeSharedGroupData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(
                databaseName: Constants.DATABASE_NAME,
                collectionName: Constants.MATCH_LOBBY_TABLE_NAME,
                ConnectionStringSetting = "PlayFabTicTacToeCosmosDB")]
                DocumentClient documentClient
            )
        {
            var context = await FunctionContext<JoinMatchLobbyRequest>.Create(req);
            var matchLobbyId = context.FunctionArgument.MatchLobbyId;
            var playerTwo = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            return await MatchLobbyUtil.JoinMatchLobby(context.AuthenticationContext, matchLobbyId, playerTwo, documentClient);
        }
    }
}
