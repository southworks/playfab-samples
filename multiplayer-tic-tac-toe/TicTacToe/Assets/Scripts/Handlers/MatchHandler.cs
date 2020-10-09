using PlayFab;
using System.Collections;
using TicTacToe.Models;
using TicTacToe.Models.Game.Requests;
using UnityEngine;

namespace TicTacToe.Handlers
{
    public class MatchHandler : RequestHandler
    {
        private readonly string sharedGroupId;

        public GameState GameState { get; set; }

        public TicTacToeSharedGroupData TicTacToeSharedGroupData { get; set; }

        public MatchHandler(PlayerInfo playerInfo, string sharedGroupId) : base(playerInfo)
        {
            this.sharedGroupId = sharedGroupId;
        }

        public IEnumerator StartMatch()
        {
            ExecutionCompleted = false;

            var request = GetExecuteFunctionRequest(
                Constants.START_MATCH_FUNCTION_NAME,
                new StartMatchRequest { SharedGroupId = sharedGroupId });

            PlayFabCloudScriptAPI.ExecuteFunction(request,
                (result) =>
                {
                    ExecutionCompleted = true;
                    GameState = JsonUtility.FromJson<GameState>(result.FunctionResult.ToString());
                },
                (error) =>
                {
                    Debug.Log($"StartMatch request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator JoinMatch()
        {
            ExecutionCompleted = false;

            var request = GetExecuteFunctionRequest(
                Constants.JOIN_MATCH_FUNCTION_NAME,
                new StartMatchRequest { SharedGroupId = sharedGroupId });

            PlayFabCloudScriptAPI.ExecuteFunction(request,
                (result) =>
                {
                    ExecutionCompleted = true;
                    TicTacToeSharedGroupData = JsonUtility.FromJson<TicTacToeSharedGroupData>(result.FunctionResult.ToString());
                },
                (error) =>
                {
                    TicTacToeSharedGroupData = null;
                    ExecutionCompleted = true;
                    Debug.Log($"StartMatch request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
                }
            );

            yield return WaitForExecution();
        }
    }
}