using PlayFab;
using System.Collections;
using TicTacToe.Models;
using TicTacToe.Models.Requests;
using UnityEngine;

namespace TicTacToe.Handlers
{
    public class SharedGroupHandler : RequestHandler
    {
        public string SharedGroupId;

        public TicTacToeSharedGroupData SharedGroupData { get; private set; }

        public SharedGroupHandler(PlayerInfo Player) : base(Player) { }

        public IEnumerator Create(string sharedGroupId)
        {
            ExecutionCompleted = false;
            SharedGroupData = null;

            var request = GetExecuteFunctionRequest(
                Constants.CREATE_SHARED_GROUP,
                new CreateSharedGroupRequest { SharedGroupId = sharedGroupId });

            PlayFabCloudScriptAPI.ExecuteFunction(request,
                (result) =>
                {
                    var response = result?.FunctionResult != null ? JsonUtility.FromJson<TicTacToeSharedGroupData>(result.FunctionResult.ToString()) : null;
                    SharedGroupId = response?.sharedGroupId;
                    ExecutionCompleted = true;
                },
                (error) =>
                {
                    SharedGroupId = null;
                    ExecutionCompleted = true;
                    Debug.Log($"Shared Group creation request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator Get(string sharedGroupId)
        {
            ExecutionCompleted = false;
            SharedGroupData = null;

            var request = GetExecuteFunctionRequest(
                Constants.GET_SHARED_GROUP,
                new GetSharedGroupRequest { SharedGroupId = sharedGroupId });

            PlayFabCloudScriptAPI.ExecuteFunction(request,
                (result) =>
                {
                    SharedGroupData = result?.FunctionResult != null ? JsonUtility.FromJson<TicTacToeSharedGroupData>(result.FunctionResult.ToString()) : null;
                    ExecutionCompleted = true;
                },
                (error) =>
                {
                    SharedGroupData = null;
                    ExecutionCompleted = true;
                    Debug.Log($"Shared Group get request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator Delete(string sharedGroupId)
        {
            ExecutionCompleted = false;
            Debug.Log($"DeleteShcaredGroupHandler request '{sharedGroupId}'");

            var request = GetExecuteFunctionRequest(
                Constants.DELETE_SHARED_GROUP_FUNCTION_NAME,
                new DeleteSharedGroupRequest { SharedGroupId = sharedGroupId });

            PlayFabCloudScriptAPI.ExecuteFunction(request,
                (result) =>
                {
                    Debug.Log("Deleted");
                    ExecutionCompleted = true;
                },
                (error) =>
                {
                    Debug.Log($"DeleteSharedGroupHandler request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
                }
           );

            yield return WaitForExecution();
        }
    }
}
