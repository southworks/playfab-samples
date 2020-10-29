using System;
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
    public class LeaveMatchLobby
    {
        [FunctionName("LeaveMatchLobby")]
        public static async Task<MatchLobby> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(ConnectionStringSetting = "PlayFabTicTacToeCosmosDB")] DocumentClient cosmosDBClient)
        {
            var context = await FunctionContext<LeaveMatchLobbyRequest>.Create(req);
            var matchLobbyId = context.FunctionArgument.MatchLobbyId;

            var matchLobby = await MatchLobbyUtil.GetMatchLobbyAsync(
                cosmosDBClient,
                (document) => document.MatchLobbyId == matchLobbyId);

            if (matchLobby == null)
            {
                throw new Exception("Match Lobby not found");
            }

            return await MatchLobbyUtil.LeaveMatchLobby(matchLobby, cosmosDBClient);
        }
    }
}
