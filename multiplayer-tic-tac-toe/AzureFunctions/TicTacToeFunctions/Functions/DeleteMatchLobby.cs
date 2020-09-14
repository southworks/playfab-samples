// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PlayFab.Plugins.CloudScript;
using System.Net.Http;
using System.Threading.Tasks;
using TicTacToeFunctions.Models.Requests;
using TicTacToeFunctions.Util;

namespace TicTacToeFunctions.Functions
{
    public static class DeleteMatchLobby
    {
        [FunctionName("DeleteMatchLobby")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(ConnectionStringSetting = "PlayFabTicTacToeCosmosDB")] DocumentClient cosmosDBClient
        )
        {
            var context = await FunctionContext<DeleteMatchLobbyInfoRequest>.Create(req);
            var matchLobbyRequest = context.FunctionArgument;

            Settings.TrySetSecretKey(context.ApiSettings);
            Settings.TrySetCloudName(context.ApiSettings);

            await MatchLobbyUtil.DeleteMatchLobbyFromDDBBAsync(cosmosDBClient, matchLobbyRequest.Id, matchLobbyRequest.MatchLobbyId);

            return new OkObjectResult(matchLobbyRequest);
        }
    }
}
