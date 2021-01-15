using System.Net.Http;
using System.Threading.Tasks;
using FantasySoccer.Core.Models.Functions;
using FantasySoccer.Core.Services;
using FantasySoccer.Functions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace FantasySoccer.Functions
{
    public static class UserTeamRoundScoreCalculation
    {
        [FunctionName("UserTeamRoundScoreCalculation")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
        {
            // We couldn't find a functional model to represent
            // the request from a Scheduled Task
            var context = JsonConvert.DeserializeObject<UserTeamRoundScoreCalculationRequestContext>(await req.Content.ReadAsStringAsync());

            var fantasySoccer = new FantasySoccerService(
                new CosmosDBService(ConfigurationConstants.CosmosDbConfig), 
                new PlayFabService(ConfigurationConstants.PlayFabConfiguration, new MemoryCache(new MemoryCacheOptions())));

            await fantasySoccer.CalculateUserRoundScore(context.PlayerProfile.PlayerId.Value, context.FunctionArgument.TournamentId, context.FunctionArgument.Round);

            return new OkResult();
        }
    }
}
