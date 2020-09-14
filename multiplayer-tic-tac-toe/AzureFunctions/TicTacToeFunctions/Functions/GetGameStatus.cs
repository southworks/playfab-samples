// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.Plugins.CloudScript;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions
{
    public static class GetGameStatus
    {
        [FunctionName("GetGameStatus")]
        public static async Task<GameState> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(ConnectionStringSetting = "PlayFabTicTacToeCosmosDB")] DocumentClient cosmosDBClient,
            ILogger log)
        {
            var context = await FunctionContext<GetGameStatusRequest>.Create(req);
            return await GameStateUtil.GetAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId);
        }
    }
}
