using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions
{
    public static class CreateMatchLobby
    {
        [FunctionName("CreateMatchLobby")]
        public static async Task<MatchLobby> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(ConnectionStringSetting = "PlayFabTicTacToeCosmosDB")] DocumentClient cosmosDBClient,
            [CosmosDB(
                databaseName: Constants.DatabaseName,
                collectionName: Constants.MatchLobbyTableName,
                ConnectionStringSetting = "PlayFabTicTacToeCosmosDB")]
                IAsyncCollector<MatchLobby> matchlobbyCollection)
        {
            var context = await FunctionContext<CreateMatchLobbyRequest>.Create(req);
            var args = context.FunctionArgument;
            var creatorId = context.CallerEntityProfile.Entity.Id;

            return await MatchLobbyUtil.CreateMatchLobby(args.MatchLobbyId, args.Locked, creatorId, args.NetworkId, matchlobbyCollection, cosmosDBClient);
        }
    }
}
