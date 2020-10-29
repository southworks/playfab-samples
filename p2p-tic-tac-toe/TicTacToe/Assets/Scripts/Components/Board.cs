using System.Collections;
using System.Collections.Generic;
using TicTacToe.Models.Helpers;
using TicTacToe.Models.Requests;
using UnityEngine;

namespace TicTacToe
{
    public class Board : MonoBehaviour
    {
        public bool Enabled { get; set; }

        public TokenRow[] Rows = new TokenRow[3];
        public GameObject XPrefab;
        public GameObject OPrefab;

        public GameObject board;

        public TicTacToeMove LatestPlayerMove { get; private set; }

        //todo: Refactor this to manage rendering in a nicer way
        private readonly IDictionary<int, KeyValuePair<int, int>> CellsDictionary = new Dictionary<int, KeyValuePair<int, int>>
        {
            { 0 , new KeyValuePair<int,int>(0, 0) },
            { 1 , new KeyValuePair<int,int>(0, 1) },
            { 2 , new KeyValuePair<int,int>(0, 2) },
            { 3 , new KeyValuePair<int,int>(1, 0) },
            { 4 , new KeyValuePair<int,int>(1, 1) },
            { 5 , new KeyValuePair<int,int>(1, 2) },
            { 6 , new KeyValuePair<int,int>(2, 0) },
            { 7 , new KeyValuePair<int,int>(2, 1) },
            { 8 , new KeyValuePair<int,int>(2, 2) }
        };

        private void Start()
        {
            SetEnabled(false);
        }

        public void SetActivate(bool value)
        {
            board.SetActive(value);
        }

        public IEnumerator WaitForNextMove()
        {
            // Enable the board            
            SetEnabled(true);

            while (true)
            {
                for (int i = 0; i < Rows.Length; i++)
                {
                    // Query the row for the next move on it
                    StartCoroutine(Rows[i].GetNextMove());

                    // On the first row that gets clicked
                    if (Rows[i].ColumnClicked != -1)
                    {
                        // Get the move that was requested
                        LatestPlayerMove = new TicTacToeMove
                        {
                            row = i,
                            col = Rows[i].ColumnClicked
                        };

                        // Reset the row
                        Rows[i].ColumnClicked = -1;

                        // Disable the board
                        SetEnabled(false);
                        yield break;
                    }
                }

                yield return null;
            }
        }

        public void RenderBoard(int[] boardState)
        {
            var index = 0;

            foreach (var boardCell in boardState)
            {
                switch (boardCell)
                {
                    case (int)OccupantType.PLAYER_ONE:
                        this.PlacePlayerToken(CellsDictionary[index].Key, CellsDictionary[index].Value);
                        break;
                    case (int)OccupantType.PLAYER_TWO:
                        this.PlaceAIToken(CellsDictionary[index].Key, CellsDictionary[index].Value);
                        break;
                    default:
                        break;
                }
                index++;
            }
        }

        public TicTacToeMove GetAndResetMove()
        {
            var move = LatestPlayerMove;
            LatestPlayerMove = null;
            return move;
        }

        public void PlacePlayerToken(int row, int col) => TryPlaceToken(row, col, XPrefab);

        public void PlaceAIToken(int row, int col) => TryPlaceToken(row, col, OPrefab);

        public void Reset()
        {
            SetEnabled(true);

            foreach (var row in Rows)
            {
                foreach (var token in row.Columns)
                {
                    token.Reset();
                }
            }
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;

            foreach (var row in Rows)
            {
                foreach (var token in row.Columns)
                {
                    token.IsEnabled = enabled;
                }
            }
        }

        private void TryPlaceToken(int row, int col, GameObject gameObject)
        {
            if (Rows[row].Columns[col].Token == null)
                Rows[row].Columns[col].PlaceToken(gameObject);
        }
    }
}
