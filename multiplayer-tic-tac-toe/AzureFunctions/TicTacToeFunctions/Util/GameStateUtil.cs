// Copyright (C) Microsoft Corporation. All rights reserved.

using PlayFab;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Helpers;

namespace TicTacToeFunctions.Util
{
    public class GameStateUtil
    {
        public static async Task<GameState> GetAsync(PlayFabAuthenticationContext context, string SharedGroupId)
        {
            var sharedGroupData = await SharedGroupDataUtil.GetAsync(context, SharedGroupId);
            return sharedGroupData.GameState;
        }

        public static async Task<GameState> MoveUpdateAsync(PlayFabAuthenticationContext context, string playerId, TicTacToeMove playerMove, string SharedGroupId)
        {
            var sharedGroupData = await SharedGroupDataUtil.GetAsync(context, SharedGroupId);
            var newGameState = UpdateAsync(sharedGroupData.Match, sharedGroupData.GameState, playerId, playerMove);

            if (newGameState != null)
            {
                sharedGroupData.GameState = newGameState;
                await SharedGroupDataUtil.UpdateAsync(context, sharedGroupData);
            }

            return newGameState;
        }

        public static async Task<GameState> InitializeAsync(PlayFabAuthenticationContext context, string SharedGroupId)
        {
            var sharedGroupData = await SharedGroupDataUtil.GetAsync(context, SharedGroupId);

            sharedGroupData.GameState = new GameState
            {
                BoardState = new int[9],
                CurrentPlayerId = sharedGroupData.Match.PlayerOneId,
                Winner = (int)OccupantType.NONE
            };

            await SharedGroupDataUtil.UpdateAsync(context, sharedGroupData);
            return sharedGroupData.GameState;
        }

        public static async Task<GameState> UpdateWinnerAsync(PlayFabAuthenticationContext context, string playerId, string SharedGroupId)
        {
            var sharedGroupData = await SharedGroupDataUtil.GetAsync(context, SharedGroupId);
            sharedGroupData.GameState.Winner = sharedGroupData.Match.PlayerOneId == playerId ?
                (int)OccupantType.PLAYER_ONE : (int)OccupantType.PLAYER_TWO;

            await SharedGroupDataUtil.UpdateAsync(context, sharedGroupData);
            return sharedGroupData.GameState;
        }

        private static GameState UpdateAsync(Match match, GameState gameState, string playerId, TicTacToeMove move)
        {
            var occupantPlayer = playerId == match.PlayerOneId ? OccupantType.PLAYER_ONE : OccupantType.PLAYER_TWO;
            var playerMovePosition = move.row * 3 + move.col;

            if (gameState.BoardState[playerMovePosition] == (int)OccupantType.NONE)
            {
                gameState.BoardState[playerMovePosition] = (int)occupantPlayer;

                // Pass the turn to the other player
                switch (occupantPlayer)
                {
                    case OccupantType.PLAYER_ONE:
                        gameState.CurrentPlayerId = match.PlayerTwoId;
                        break;
                    case OccupantType.PLAYER_TWO:
                        gameState.CurrentPlayerId = match.PlayerOneId;
                        break;
                }

                return gameState;
            }
            else
            {
                return null;
            }
        }
    }
}