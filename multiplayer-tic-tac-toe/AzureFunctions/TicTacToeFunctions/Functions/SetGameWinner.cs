// Copyright (C) Microsoft Corporation. All rights reserved.

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
    public static class SetGameWinner
    {
        [FunctionName("SetGameWinner")]
        public static async Task<GameState> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            var context = await FunctionContext<SetGameWinnerRequest>.Create(req);
            var gameWinnerRequest = context.FunctionArgument;
            return await GameStateUtil.UpdateWinnerAsync(context.AuthenticationContext, gameWinnerRequest.PlayerId, gameWinnerRequest.SharedGroupId);
        }
    }
}
