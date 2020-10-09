using System.Collections;
using TicTacToe.Handlers;
using TicTacToe.Helpers.Game;
using TicTacToe.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TicTacToe
{
    public class Game : MonoBehaviour
    {
        #region UI Objects
        public Button LeaveMatchButton;
        public Text WinnerStatusText;
        public Text GameStatusText;

        public Text CurrentMatchLobbyNameTxt;
        public Text CurrentMatchLobbyIdTxt;
        #endregion

        #region Game Objects
        public Board Board;
        #endregion

        #region Game State Properties

        public bool GameOver { get; private set; }

        public bool Leaving { get; private set; } = false;

        public PlayerInfo CurrentPlayer { get; private set; }

        public TicTacToeSharedGroupData CurrentData
        {

            get => ApplicationModel.CurrentSharedGroupData;

            private set => ApplicationModel.CurrentSharedGroupData = value;
        }

        #endregion

        #region Common

        void Start()
        {
            CurrentData = ApplicationModel.CurrentSharedGroupData;
            LeaveMatchButton.onClick.AddListener(OnCLickLeaveMatch);
            CurrentPlayer = ApplicationModel.CurrentPlayer;

            if (ApplicationModel.CurrentMatchLobby != null)
            {
                CurrentMatchLobbyNameTxt.text = $"Match Lobby: {ApplicationModel.CurrentMatchLobby.matchLobbyId}";
            }

            StartCoroutine(StartMatchLoop());
        }

        private void UpdateGameStatus(string statusText)
        {
            GameStatusText.GetComponent<Text>().enabled = true;
            GameStatusText.text = statusText;
        }

        private IEnumerator TriggerGameOver(bool shouldDeleteSharedGroup)
        {
            if (shouldDeleteSharedGroup && !Leaving)
            {
                var sharedGroupHandler = new SharedGroupHandler(CurrentPlayer);
                yield return StartCoroutine(sharedGroupHandler.Delete(CurrentData.sharedGroupId));
            }

            UpdateGameStatus(Constants.GAME_OVER);
        }

        private IEnumerator SetGameWinner(string playerWinnerId)
        {
            var gameStatusHandler = new GameStatusHandler(CurrentPlayer, CurrentData.sharedGroupId);
            yield return StartCoroutine(gameStatusHandler.SetWinner(playerWinnerId));
        }

        private IEnumerator GetGameStatus()
        {
            var gameStatusHandler = new GameStatusHandler(CurrentPlayer, CurrentData.sharedGroupId);

            yield return StartCoroutine(gameStatusHandler.Get());
            yield return ApplicationModel.CurrentGameState = gameStatusHandler.GameState;

            Board.RenderBoard(ApplicationModel.CurrentGameState.boardState);
            CheckForWinner();
        }

        private void OnCLickLeaveMatch()
        {
            Leaving = true;
            if (!GameOver && ApplicationModel.CurrentGameState != null && CurrentData.gameState.winner == 0)
            {
                var playerWinnerId = CurrentData.match.playerOneId == CurrentPlayer.PlayFabId ? CurrentData.match.playerTwoId : CurrentData.match.playerOneId;
                StartCoroutine(SetGameWinner(playerWinnerId));
                ApplicationModel.CurrentSharedGroupData = new TicTacToeSharedGroupData();
            }

            SceneManager.LoadScene("Lobby");
        }

        #endregion Common

        #region Multiplayer Loop

        private IEnumerator StartMatchLoop()
        {
            yield return StartCoroutine(StartMatch());

            while (true)
            {
                yield return StartCoroutine(GetGameStatus());

                if (GameOver)
                {
                    yield return TriggerGameOver(!IsMyTurn());
                    break;
                }

                if (!IsMyTurn())
                {
                    yield return new WaitForSeconds(Constants.RETRY_GET_MATCH_INFO_AFTER_SECONDS);
                    continue;
                }

                // Let the player make a move
                yield return StartCoroutine(MakeMove());
            }
        }

        private IEnumerator StartMatch()
        {
            var startMatchHandler = new MatchHandler(CurrentPlayer, ApplicationModel.CurrentSharedGroupData.sharedGroupId);
            yield return startMatchHandler.StartMatch();
            ApplicationModel.CurrentSharedGroupData.gameState = startMatchHandler.GameState;
            CurrentData = ApplicationModel.CurrentSharedGroupData;
        }

        private IEnumerator MakeMove()
        {
            while (true)
            {
                UpdateGameStatus(Constants.PLAYER_MOVE_WAIT);

                // Let the player make a move
                yield return StartCoroutine(Board.WaitForNextMove());

                UpdateGameStatus(Constants.PLAYER_MOVE_PROCESSING);

                // Get and execute the player move
                var move = Board.GetAndResetMove();

                var gameStatusHandler = new GameStatusHandler(CurrentPlayer, CurrentData.sharedGroupId);
                yield return StartCoroutine(gameStatusHandler.MakeMove(move));

                // If valid move place token and end player move
                if (gameStatusHandler.MultiplayerMoveResponse.playerMoveResult.valid)
                {
                    UpdateGameStatus(Constants.PLAYER_MOVE_COMPLETED);

                    WinnerStatusText.GetComponent<Text>().enabled = false;
                    Board.RenderBoard(ApplicationModel.CurrentGameState.boardState);

                    yield break;
                }
                // Otherwise ask for another move
                else
                {
                    UpdateGameStatus(Constants.PLAYER_MOVE_INVALID);
                }
            }
        }

        private bool IsMyTurn()
        {
            if (ApplicationModel.CurrentGameState == null || CurrentPlayer == null)
            {
                return false;
            }

            return ApplicationModel.CurrentGameState.currentPlayerId == CurrentPlayer.PlayFabId;
        }

        #endregion Multiplayer Loop

        #region Winner check

        private void CheckForWinner()
        {
            UpdateGameStatus(Constants.GAME_WIN_CHECK_STARTED);

            if ((ApplicationModel.CurrentGameState?.boardState?.Length ?? 0) > 0)
            {
                var data = WinCheckUtil.Check(ApplicationModel.CurrentGameState);
                ProcessWinCheckResult(data);
            }

            UpdateGameStatus(Constants.GAME_WIN_CHECK_COMPLETED);
        }

        private void ProcessWinCheckResult(WinCheckResult result)
        {
            switch (result.winner)
            {
                case GameWinnerType.NONE:
                    break;
                case GameWinnerType.DRAW:
                    WinnerStatusText.text = "DRAW!";
                    break;
                case GameWinnerType.PLAYER_ONE:
                    StartCoroutine(SetGameWinner(ApplicationModel.CurrentMatch.playerOneId));
                    WinnerStatusText.text = $"Player {(int)result.winner} Wins!";
                    break;
                case GameWinnerType.PLAYER_TWO:
                    StartCoroutine(SetGameWinner(ApplicationModel.CurrentMatch.playerTwoId));
                    WinnerStatusText.text = $"Player {(int)result.winner} Wins!";
                    break;
            }

            if (result.winner != GameWinnerType.NONE)
            {
                WinnerStatusText.GetComponent<Text>().enabled = true;
                GameOver = true;
            }
        }

        #endregion Winner check
    }
}
