using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace PlayFabB2C.Functions
{
    public static class PlayFabSUSI
    {
        [FunctionName("PlayFabSUSI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(requestBody);

            // These settings should be set on the app settings
            PlayFabSettings.staticSettings.TitleId = "";
            PlayFabSettings.staticSettings.DeveloperSecretKey = "";

            var loginResult = await PlayFabClientAPI.LoginWithCustomIDAsync(new LoginWithCustomIDRequest
            {
                CreateAccount = true,
                CustomId = data.objectId,
                TitleId = PlayFabSettings.staticSettings.TitleId,
            });

            return new OkObjectResult(new 
            { 
                playFabId = loginResult.Result.PlayFabId,
                sessionTicket = loginResult.Result.SessionTicket
            });
        }
    }
}
