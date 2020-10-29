using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models.Requests;
using Assets.Scripts.Models.Responses;
using PlayFab;
using TicTacToe.Handlers;
using TicTacToe.Helpers;
using TicTacToe.Models;
using TicTacToe.Models.Requests;
using TicTacToe.Models.Responses;
using UnityEngine;

public class MatchLobbyHandler : AzureFunctionRequestHandler
{
    public MatchLobby MatchLobby { get; private set; }

    public ResponseWrapper<JoinMatchLobbyResponse> JoinMatchLobbyResponse { get; private set; }

    public ResponseWrapper<SetMatchLobbyLockStateResponse> SetMatchLobbyLockStateResponse { get; private set; }

    public List<MatchLobby> MatchLobbyList { get; private set; }

    public MatchLobbyHandler(PlayerInfo Player) : base(Player) { }

    public IEnumerator JoinMatchLobby(string id, string invitationCode = null)
    {
        ExecutionCompleted = false;

        var request = GetExecuteFunctionRequest(
            Constants.JOIN_MATCH_LOBBY,
            new JoinMatchLobbyRequest { MatchLobbyId = id, InvitationCode = invitationCode });

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            (result) =>
            {
                JoinMatchLobbyResponse = result?.FunctionResult != null ? JsonUtility.FromJson<ResponseWrapper<JoinMatchLobbyResponse>>(result.FunctionResult.ToString()) : null;
                ExecutionCompleted = true;
            },
            (error) =>
            {
                JoinMatchLobbyResponse = null;
                ExecutionCompleted = true;
                Debug.Log($"MatchLobby join request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
            }
        );

        yield return WaitForExecution();
    }

    public IEnumerator CreateMatchLobby(string matchLobbyId, string networkId, bool locked)
    {
        ExecutionCompleted = false;

        var request = GetExecuteFunctionRequest(
            Constants.CREATE_MATCH_LOBBY,
            new CreateMatchLobbyRequest
            {
                MatchLobbyId = matchLobbyId,
                NetworkId = networkId,
                Locked = locked
            });

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            (result) =>
            {
                MatchLobby = result?.FunctionResult != null ? JsonUtility.FromJson<MatchLobby>(result.FunctionResult.ToString()) : null;
                ExecutionCompleted = true;
            },
            (error) =>
            {
                MatchLobby = null;
                ExecutionCompleted = true;
                Debug.Log($"MatchLobby creation request failed. Message: ${error.ErrorMessage}, Code: ${error.HttpCode}");
            }
        );

        yield return WaitForExecution();
    }

    public IEnumerator GetMatchLobbyList(string filter = "")
    {
        ExecutionCompleted = false;

        var request = GetExecuteFunctionRequest(
            Constants.SEARCH_MATCH_LOBBIES_FUNCTION_NAME,
            new SearchMatchLobbiesRequest { SearchTerm = filter });

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            (result) =>
            {
                var resultList = JsonHelper.FromJson<MatchLobby>(result.FunctionResult.ToString());
                MatchLobbyList = resultList.ToList();

                ExecutionCompleted = true;
            },
            (error) =>
            {
                MatchLobbyList = null;
                ExecutionCompleted = true;
                Debug.Log($"GetMatchLobbyList request failed. Message: ${error.ErrorMessage}, Code: ${error.HttpCode}");
            }
        );

        yield return WaitForExecution();
    }

    public IEnumerator DeleteMatchLobby(string matchLobbyId)
    {
        ExecutionCompleted = false;

        var request = GetExecuteFunctionRequest(
            Constants.DELETE_MATCH_LOBBY_FUNCTION_NAME,
            new DeleteMatchLobbyInfoRequest
            {
                Id = matchLobbyId,
                MatchLobbyId = matchLobbyId
            });

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            (result) =>
            {
                ExecutionCompleted = true;
            },
            (error) =>
            {
                ExecutionCompleted = true;
                Debug.Log($"DeleteMatchLobby request failed. Message: ${error.ErrorMessage}, Code: ${error.HttpCode}");
            }
        );

        yield return WaitForExecution();
    }

    public IEnumerator LeaveMatchLobby(string matchLobbyId)
    {
        ExecutionCompleted = false;

        var request = GetExecuteFunctionRequest(
            Constants.LEAVE_MATCH_LOBBY,
            new LeaveMatchLobbyRequest { MatchLobbyId = matchLobbyId });

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            (result) =>
            {
                MatchLobby = result?.FunctionResult != null ? JsonUtility.FromJson<MatchLobby>(result.FunctionResult.ToString()) : null;
                ExecutionCompleted = true;
            },
            (error) =>
            {
                MatchLobby = null;
                ExecutionCompleted = true;
                Debug.Log($"MatchLobby Leave request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
            }
        );

        yield return WaitForExecution();
    }

    public IEnumerator SetMatchLobbyLockState(string matchLobbyId, bool lockState)
    {
        ExecutionCompleted = false;

        var request = GetExecuteFunctionRequest(
            Constants.SET_MATCH_LOBBY_LOCK_STATE,
            new SetMatchLobbyLockStateRequest
            {
                MatchLobbyId = matchLobbyId,
                Locked = lockState
            }
        );

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            (result) =>
            {
                SetMatchLobbyLockStateResponse = result?.FunctionResult != null ? JsonUtility.FromJson<ResponseWrapper<SetMatchLobbyLockStateResponse>>(result.FunctionResult.ToString()) : null;
                ExecutionCompleted = true;
            },
            (error) =>
            {
                SetMatchLobbyLockStateResponse = null;
                ExecutionCompleted = true;
                Debug.Log($"MatchLobby Lobby locked state setting has failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
            }
        );

        yield return WaitForExecution();
    }
}
