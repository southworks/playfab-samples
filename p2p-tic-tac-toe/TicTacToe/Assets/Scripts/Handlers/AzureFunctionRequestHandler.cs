using PlayFab;
using PlayFab.CloudScriptModels;
using TicTacToe.Models;

namespace TicTacToe.Handlers
{
    public class AzureFunctionRequestHandler : RequestHandler
    {
        protected PlayerInfo Player { get; set; }

        protected PlayFabAuthenticationContext PlayFabAuthenticationContext { get; set; }

        public AzureFunctionRequestHandler(PlayerInfo player)
        {
            Player = player;
            PlayFabAuthenticationContext = new PlayFabAuthenticationContext
            {
                EntityToken = player.EntityToken
            };
        }

        protected ExecuteFunctionRequest GetExecuteFunctionRequest(string functionName, object functionParameter)
        {
            return new ExecuteFunctionRequest
            {
                FunctionName = functionName,
                FunctionParameter = functionParameter,
                AuthenticationContext = PlayFabAuthenticationContext
            };
        }
    }
}
