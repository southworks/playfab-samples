using System.Collections;
using TicTacToe.Handlers;
using TicTacToe.Helpers;
using TicTacToe.Models;
using TicTacToe.Models.Helpers;
using TicTacToe.Models.Requests;
using TicTacToe.Models.Responses;
using TicTacToe.Utils;
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

        public PlayerInfo CurrentPlayer { get; private set; }

        private GameStateHandler gameStateHandler;

        #endregion

        #region Common

        void Start()
        {
            CurrentPlayer = ApplicationModel.CurrentPlayer;
            LeaveMatchButton.onClick.AddListener(OnClickLeaveMatch);

            if (ApplicationModel.CurrentMatchLobby != null)
            {
                CurrentMatchLobbyNameTxt.text = $"Match Lobby: {ApplicationModel.CurrentMatchLobby.matchLobbyId}";
            }

            gameStateHandler = new GameStateHandler();
            gameStateHandler.OnGameStateReceived += HandleOnGameStateReceived;

            if (ApplicationModel.IsHost)
            {
                gameStateHandler.OnMoveReceived += HandleOnMoveReceived;
            }

            gameStateHandler.OnMatchAbandonment += HandleOnMatchAbandonment;

            InitializeGame();
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
            UpdateGameStatus($"Local Player: {gameStateHandler.Manager.LocalPlayer.EntityKey.Id} - Remote Player: {gameStateHandler.Manager.RemotePlayers[0].EntityKey.Id}");
        }

        private void Update()
        {
            if (ApplicationModel.NewTurnToUpdate)
            {
                StartCoroutine(UpdateGameAtNewTurn());
            }

            if (GameOver && Board.Enabled)
            {
                Board.SetEnabled(false);
            }
        }

        private void SceneManager_sceneUnloaded(Scene _)
        {
            gameStateHandler.OnGameStateReceived -= HandleOnGameStateReceived;
            gameStateHandler.OnMatchAbandonment -= HandleOnMatchAbandonment;

            if (ApplicationModel.IsHost)
            {
                gameStateHandler.OnMoveReceived -= HandleOnMoveReceived;
            }

            gameStateHandler.Leave();
            ApplicationModel.Reset();
            SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
        }

        private void UpdateGameStatus(string statusText)
        {
            GameStatusText.GetComponent<Text>().enabled = true;
            GameStatusText.text = statusText;
        }

        private void OnClickLeaveMatch()
        {
            if (ApplicationModel.CurrentGameState.winner == (int)GameWinnerType.NONE)
            {
                gameStateHandler.OnMatchAbandonment -= HandleOnMatchAbandonment;
                gameStateHandler.SendMatchAbandonment();
            }

            SceneManager.LoadScene("Lobby");
        }

        #endregion Common

        #region Multiplayer Events

        private void HandleOnMatchAbandonment()
        {
            var gameState = ApplicationModel.CurrentGameState;
            gameState.winner = ApplicationModel.CurrentMatch.playerOneId == ApplicationModel.CurrentPlayer.Entity.Id
                ? (int)GameWinnerType.PLAYER_ONE : (int)GameWinnerType.PLAYER_TWO;
            gameStateHandler.SendGameState(gameState);
            ApplicationModel.NewTurnToUpdate = true;
        }

        private void HandleOnGameStateReceived(GameState gameState)
        {
            ApplicationModel.CurrentGameState = gameState;
            ApplicationModel.NewTurnToUpdate = true;
        }

        private void HandleOnMoveReceived(string playerId, TicTacToeMove move)
        {
            var gameState = AddMoveToGameState(playerId, move, ApplicationModel.CurrentGameState, ApplicationModel.CurrentMatch);

            if (gameState == null)
            {
                Debug.LogError($"Invalid move received from the player {playerId}");
                return;
            }

            ApplicationModel.CurrentGameState = gameState;
            gameStateHandler.SendGameState(gameState);
            ApplicationModel.NewTurnToUpdate = true;
        }

        #endregion

        #region Multiplayer Flow

        private void InitializeGame()
        {
            if (ApplicationModel.IsHost && ApplicationModel.CurrentGameState == null)
            {
                ApplicationModel.CurrentGameState = new GameState(ApplicationModel.NetworkCreatorId);
                gameStateHandler.SendGameState(ApplicationModel.CurrentGameState);
                ApplicationModel.NewTurnToUpdate = true;
            }
        }

        private IEnumerator UpdateGameAtNewTurn()
        {
            ApplicationModel.NewTurnToUpdate = false;
            Board.RenderBoard(ApplicationModel.CurrentGameState.boardState);

            var currentGameResult = (GameWinnerType)ApplicationModel.CurrentGameState.winner;

            if (currentGameResult != GameWinnerType.NONE)
            {
                UpdateUIWithGameResult(currentGameResult);
                yield return null;
            }

            if (ApplicationModel.IsMyTurn)
            {
                TicTacToeMove nextMove = null;
                yield return CoroutineHelper.Run<TicTacToeMove>(WaitAndGetNextMove(), (move) => { nextMove = move; });
                HandleNewLocalMove(nextMove);
                ApplicationModel.NewTurnToUpdate = ApplicationModel.IsHost;
            }
        }

        private IEnumerator WaitAndGetNextMove()
        {
            // Let the player make a move
            yield return StartCoroutine(Board.WaitForNextMove());
            // Get and send the player move
            var move = Board.GetAndResetMove();

            yield return move;
        }

        private void HandleNewLocalMove(TicTacToeMove move)
        {
            if (ApplicationModel.IsHost)
            {
                var newGameState = AddMoveToGameState(ApplicationModel.CurrentPlayer.Entity.Id, move, ApplicationModel.CurrentGameState, ApplicationModel.CurrentMatch);
                gameStateHandler.SendGameState(newGameState);
            }
            else
            {
                gameStateHandler.SendMove(move);
            }
        }

        private GameState AddMoveToGameState(string playerId, TicTacToeMove move, GameState gameState, Match match)
        {
            var occupantPlayer = playerId == match.playerOneId ? OccupantType.PLAYER_ONE : OccupantType.PLAYER_TWO;
            var playerMovePosition = move.row * 3 + move.col;

            if (gameState.boardState[playerMovePosition] != (int)OccupantType.NONE)
            {
                return null;
            }

            gameState.boardState[playerMovePosition] = (int)occupantPlayer;

            // Pass the turn to the other player
            switch (occupantPlayer)
            {
                case OccupantType.PLAYER_ONE:
                    gameState.currentPlayerId = match.playerTwoId;
                    break;
                case OccupantType.PLAYER_TWO:
                    gameState.currentPlayerId = match.playerOneId;
                    break;
            }

            gameState.winner = (int)GetCurrentGameResult(gameState).winner;

            return gameState;
        }

        #endregion Multiplayer Flow

        #region Winner check

        private WinCheckResult GetCurrentGameResult(GameState gameState)
        {
            var checkResult = new WinCheckResult
            {
                winner = GameWinnerType.NONE
            };

            if (gameState.boardState.Length > 0)
            {
                checkResult = WinCheckUtil.Check(gameState);
            }

            return checkResult;
        }

        private void UpdateUIWithGameResult(GameWinnerType gameResult)
        {
            switch (gameResult)
            {
                case GameWinnerType.NONE:
                    break;
                case GameWinnerType.DRAW:
                    WinnerStatusText.text = "DRAW!";
                    break;
                case GameWinnerType.PLAYER_ONE:
                    WinnerStatusText.text = $"Player {(int)gameResult} Wins!";
                    break;
                case GameWinnerType.PLAYER_TWO:
                    WinnerStatusText.text = $"Player {(int)gameResult} Wins!";
                    break;
            }

            if (gameResult != GameWinnerType.NONE)
            {
                WinnerStatusText.GetComponent<Text>().enabled = true;
                GameOver = true;
            }
        }

        #endregion Winner check
    }
}
