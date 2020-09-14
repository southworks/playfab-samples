// Copyright (C) Microsoft Corporation. All rights reserved.

using PlayFab;
using System.Collections;
using TicTacToe.Models;
using TicTacToe.Models.Requests;
using TicTacToe.Models.Responses;
using UnityEngine;

namespace TicTacToe.Handlers
{
    public class GameStatusHandler : RequestHandler
    {
        private readonly string sharedGroupId;

        public GameState GameState { get; private set; }

        public MakeMultiplayerMoveResponse MultiplayerMoveResponse { get; private set; }

        public GameStatusHandler(PlayerInfo player, string sharedGroupId) : base(player)
        {
            this.sharedGroupId = sharedGroupId;
        }

        public IEnumerator Get()
        {
            var request = GetExecuteFunctionRequest(
                Constants.GET_GAME_STATUS_FUNCTION_NAME,
                new GetGameStatusRequest { SharedGroupId = sharedGroupId });

            PlayFabCloudScriptAPI.ExecuteFunction(request,
               (result) =>
               {
                   ExecutionCompleted = true;
                   GameState = JsonUtility.FromJson<GameState>(result.FunctionResult.ToString());
               },
               (error) =>
               {
                   Debug.Log($"GetGameStatus request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
               }
           );

            yield return WaitForExecution();
        }

        public IEnumerator MakeMove(TicTacToeMove move)
        {
            var request = GetExecuteFunctionRequest(
                Constants.MULTIPLAYER_MOVE_FUNCTION_NAME,
                new MakeMultiplayerMoverRequest()
                {
                    SharedGroupId = sharedGroupId,
                    PlayerId = Player.PlayFabId,
                    PlayerMove = move
                });

            PlayFabCloudScriptAPI.ExecuteFunction(
                request,
                (result) =>
                {
                    ExecutionCompleted = true;
                    MultiplayerMoveResponse = JsonUtility.FromJson<MakeMultiplayerMoveResponse>(result.FunctionResult.ToString());
                },
                (error) =>
                {
                    Debug.Log($"Make multiplayer move request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator SetWinner(string playerId)
        {
            var request = GetExecuteFunctionRequest(
                Constants.SET_WINNER_GAME_FUNCTION_NAME,
                new SetGameWinnerRequest()
                {
                    SharedGroupId = sharedGroupId,
                    PlayerId = playerId
                });

            PlayFabCloudScriptAPI.ExecuteFunction(
                request,
                (result) =>
                {
                    ExecutionCompleted = true;
                    GameState = JsonUtility.FromJson<GameState>(result.FunctionResult.ToString());
                },
                (error) =>
                {
                    Debug.Log($"Set Game Winner request failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}");
                }
            );

            yield return WaitForExecution();
        }
    }
}