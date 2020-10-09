using PlayFab;
using PlayFab.CloudScriptModels;
using System.Collections;
using TicTacToe.Models;
using UnityEngine;

namespace TicTacToe.Handlers
{
    public class RequestHandler
    {
        protected PlayerInfo Player { get; set; }

        protected PlayFabAuthenticationContext PlayFabAuthenticationContext { get; set; }

        protected bool ExecutionCompleted { get; set; }

        public RequestHandler(PlayerInfo player)
        {
            Player = player;
            PlayFabAuthenticationContext = new PlayFabAuthenticationContext
            {
                EntityToken = player.EntityToken
            };
        }

        protected IEnumerator WaitForExecution()
        {
            yield return new WaitUntil(() => { return ExecutionCompleted; });
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
