using System;
using TicTacToe.Models;
using TicTacToe.Models.Helpers;
using TicTacToe.Models.Responses;

namespace TicTacToe.Utils
{
    public static class WinCheckUtil
    {
        public static WinCheckResult Check(GameState gameState)
        {
            switch (gameState.winner)
            {
                case (int)GameWinnerType.PLAYER_ONE:
                    return new WinCheckResult
                    {
                        winner = (GameWinnerType)OccupantType.PLAYER_ONE
                    };
                case (int)GameWinnerType.PLAYER_TWO:
                    return new WinCheckResult
                    {
                        winner = (GameWinnerType)OccupantType.PLAYER_TWO
                    };
            }

            var data = gameState.boardState;
            int[,] state2D = ArrayUtil.Make2DArray(data, 3, 3);
            // For all but NONE occupant types, check to see if there is a winner in the game at the moment
            foreach (var occupantType in (OccupantType[])Enum.GetValues(typeof(OccupantType)))
            {
                if (occupantType == OccupantType.NONE)
                    continue;

                if (CheckRowWin(occupantType, state2D)
                    || CheckColWin(occupantType, state2D)
                    || CheckDiagWin(occupantType, state2D))
                {
                    return new WinCheckResult
                    {
                        winner = (GameWinnerType)occupantType
                    };
                }
            }

            // Check for a draw, otherwise game is not over yet: no winner
            if (CheckDraw(state2D))
            {
                return new WinCheckResult
                {
                    winner = GameWinnerType.DRAW
                };
            }

            return new WinCheckResult
            {
                winner = GameWinnerType.NONE
            };
        }

        private static bool CheckDraw(int[,] state)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    // There's an unoccupied space so game cannot be draw
                    if (state[i, j] == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool CheckRowWin(OccupantType occupantType, int[,] state)
        {
            // Given an occupant type, check all rows to see if that occupant type has won any row
            for (int i = 0; i < 3; i++)
            {
                if (state[i, 0] == (int)occupantType
                    && state[i, 1] == (int)occupantType
                    && state[i, 2] == (int)occupantType)
                    return true;
            }
            return false;
        }

        private static bool CheckColWin(OccupantType occupantType, int[,] state)
        {
            // Given an occupant type, check all column to see if that occupant type has won any column
            for (int i = 0; i < 3; i++)
            {
                if (state[0, i] == (int)occupantType
                    && state[1, i] == (int)occupantType
                    && state[2, i] == (int)occupantType)
                    return true;
            }
            return false;
        }

        private static bool CheckDiagWin(OccupantType occupantType, int[,] state)
        {
            // Given an occupant type, check both diagonals  to see if that occupant type has won any diagonal
            if (state[0, 0] == (int)occupantType
                && state[1, 1] == (int)occupantType
                && state[2, 2] == (int)occupantType)
                return true;
            if (state[2, 0] == (int)occupantType
                && state[1, 1] == (int)occupantType
                && state[0, 2] == (int)occupantType)
                return true;
            return false;
        }
    }

    public class ArrayUtil
    {
        public static T[,] Make2DArray<T>(T[] input, int height, int width)
        {
            T[,] output = new T[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    output[i, j] = input[i * width + j];
                }
            }
            return output;
        }
    }
}