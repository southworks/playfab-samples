// Copyright (C) Microsoft Corporation. All rights reserved.

using PlayFab;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TicTacToe.Handlers;
using TicTacToe.Helpers;
using TicTacToe.Models;
using TicTacToe.Models.Requests;
using UnityEngine;

public class MatchLobbyHandler : RequestHandler
{
    public TicTacToeSharedGroupData TicTacToeSharedGroupData { get; private set; }

    public List<MatchLobby> MatchLobbyList { get; private set; }

    public MatchLobbyHandler(PlayerInfo Player) : base(Player) { }

    public IEnumerator JoinMatchLobby(string id)
    {
        ExecutionCompleted = false;

        var request = GetExecuteFunctionRequest(
            Constants.JOIN_MATCH_LOBBY,
            new JoinMatchLobbyRequest { MatchLobbyId = id });

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            (result) =>
            {
                TicTacToeSharedGroupData = result?.FunctionResult != null ? JsonUtility.FromJson<TicTacToeSharedGroupData>(result.FunctionResult.ToString()) : null;
                ExecutionCompleted = true;
            },
            (error) =>
            {
                TicTacToeSharedGroupData = null;
                ExecutionCompleted = true;
                Debug.Log($"MatchLobby join request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
            }
        );

        yield return WaitForExecution();
    }

    public IEnumerator CreateMatchLobby(string groupName)
    {
        ExecutionCompleted = false;

        var request = GetExecuteFunctionRequest(
            Constants.CREATE_MATCH_LOBBY,
            new CreateMatchLobbyRequest { SharedGroupId = groupName });

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            (result) =>
            {
                TicTacToeSharedGroupData = result?.FunctionResult != null ? JsonUtility.FromJson<TicTacToeSharedGroupData>(result.FunctionResult.ToString()) : null;
                ExecutionCompleted = true;
            },
            (error) =>
            {
                TicTacToeSharedGroupData = null;
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
}
