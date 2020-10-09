using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Linq;
using TicTacToe.Handlers;
using TicTacToe.Helpers.Service;
using TicTacToe.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TicTacToe
{
    public class Lobby : MonoBehaviour
    {
        #region Lobby Game Objects

        public GameObject LobbyPnl;
        public InputField MatchLobbyNameInput;
        public Button SearchMatchLobbyBtn;
        public Button ExitBtn;
        public Text GameStatusTxt;
        public Text MatchLobbyText;
        public Button QuickMatchBtn;
        public Button ManageMatchLobbyBtn;

        #endregion Lobby Game Objects

        #region Fields

        public MatchmakingHandler singlePlayerMatchmakingHandler;
        private bool LookingForMatch;
        private bool IsLobbyCreated;

        #endregion Fields

        #region Unity Events

        void Start()
        {
            // TODO: When the appropriate place has been developed move this login process there
            if (ApplicationModel.CurrentPlayer == null)
            {
                var loginHandler = new LoginHandler();
                loginHandler.Login(OnPlayerLogin, OnLoginFail);
            }

            if (!string.IsNullOrWhiteSpace(ApplicationModel.CurrentSharedGroupData?.sharedGroupId))
            {
                var sharedGroupHandler = new SharedGroupHandler(ApplicationModel.CurrentPlayer);
                StartCoroutine(sharedGroupHandler.Delete(ApplicationModel.CurrentSharedGroupData.sharedGroupId));
            }

            ApplicationModel.Reset();
            LookingForMatch = false;
            ManageMatchLobbyBtn.onClick.AddListener(ManageMatchLobbyBtnOnClick);
            ExitBtn.onClick.AddListener(OnClickExitBtn);
            QuickMatchBtn.onClick.AddListener(OnClickQuickMatch);
            SearchMatchLobbyBtn.onClick.AddListener(OnMatchLobbySearchClick);
        }

        void Update()
        {
            if (ApplicationModel.JoinToMatchLobby)
            {
                StartCoroutine(JoinMatchLobby());
            }

            if (ApplicationModel.CurrentMatchLobby != null)
            {
                MatchLobbyText.text = $"Connected to match lobby: {ApplicationModel.CurrentMatchLobby.matchLobbyId}";
            }
        }

        #endregion Unity Events

        #region Common

        private IEnumerator CancelMatchmaking()
        {
            UpdateGameStatus(Constants.TICKET_CANCEL_STARTED);
            LookingForMatch = false;
            yield return StartCoroutine(singlePlayerMatchmakingHandler.CancelAllMatchmakingTicketsForPlayer());
            UpdateGameStatus(Constants.TICKET_CANEL);
        }

        private void CreateMatchmakingHandler(bool isQuickPlay)
        {
            if (singlePlayerMatchmakingHandler != null) return;

            var queueConfiguration = new MatchmakingQueueConfiguration
            {
                GiveUpAfterSeconds = Constants.GIVE_UP_AFTER_SECONDS,
                QuickPlayQueueName = Constants.QUICK_MATCHMAKING_QUEUE_NAME,
                MatchLobbyQueueName = Constants.LOBBY_MATCHMAKING_QUEUE_NAME,
                IsQuickPlay = isQuickPlay
            };

            singlePlayerMatchmakingHandler = new MatchmakingHandler(ApplicationModel.CurrentPlayer, queueConfiguration);
        }

        private IEnumerator CreateSinglePlayerMatch()
        {
            yield return StartCoroutine(CancelMatchmaking());

            this.LookingForMatch = true;
            UpdateGameStatus(Constants.TICKET_SEARCH);

            // Create the matchmaking Ticket.
            yield return StartCoroutine(singlePlayerMatchmakingHandler.CreateMatchmakingTicket("skill"));
            yield return StartCoroutine(ProcessTicket());
        }

        private IEnumerator ProcessTicket()
        {
            // We request the matchmaking Ticket's status for checking if there is a match or not.
            yield return StartCoroutine(singlePlayerMatchmakingHandler.EnsureGetMatchmakingTicketStatus());
            var ticketStatus = singlePlayerMatchmakingHandler.MatchmakingTicketStatus;

            // Finally, we get the match's info
            if (ticketStatus != null && ticketStatus.Status == MatchmakingTicketStatusEnum.Matched)
            {
                yield return StartCoroutine(singlePlayerMatchmakingHandler.GetMatchInfo());

                var matchInfo = singlePlayerMatchmakingHandler.MatchResult;
                InitializeSharedGroupData(match: PrepareQuickMatch(matchInfo));
                UpdateGameStatus(Constants.TICKET_MATCH);
                yield return StartMatch(shouldCreateMatch: true);
            }
            else
            {
                LookingForMatch = false;
                UpdateGameStatus(Constants.TICKET_TIMEDOUT_OR_CANCELLED);
                RestartUIToQuickMatch();
            }
        }

        private void InitializeSharedGroupData(GameState gameState = null, Match match = null, MatchLobby matchLobby = null)
        {
            if (ApplicationModel.CurrentSharedGroupData == null)
            {
                ApplicationModel.CurrentSharedGroupData = new TicTacToeSharedGroupData();
            }

            if (gameState != null)
            {
                ApplicationModel.CurrentGameState = gameState;
            }

            if (match != null)
            {
                ApplicationModel.CurrentMatch = match;
            }

            if (matchLobby != null)
            {
                ApplicationModel.CurrentMatchLobby = matchLobby;
            }
        }

        #endregion Common

        #region Quick Match

        private void UpdateGameStatus(string statusText)
        {
            GameStatusTxt.GetComponent<Text>().enabled = true;
            GameStatusTxt.text = statusText;
        }

        private void RestartUIToQuickMatch()
        {
            QuickMatchBtn.GetComponentInChildren<Text>().text = Constants.BUTTON_QUICKMATCH_QUICKMATCHED_CANCELED;
            MatchLobbyNameInput.gameObject.SetActive(true);
            MatchLobbyText.gameObject.SetActive(true);
            SearchMatchLobbyBtn.gameObject.SetActive(true);
            ManageMatchLobbyBtn.gameObject.SetActive(true);
        }

        private void OnClickQuickMatch()
        {
            ApplicationModel.IsMultiplayer = true;

            try
            {
                CreateMatchmakingHandler(isQuickPlay: true);

                if (LookingForMatch)
                {
                    StartCoroutine(CancelMatchmaking());
                    RestartUIToQuickMatch();
                }
                else
                {
                    StartCoroutine(CreateSinglePlayerMatch());
                    QuickMatchBtn.GetComponentInChildren<Text>().text = Constants.BUTTON_QUICKMATCH_QUICKMATCHED_STARTED;
                    MatchLobbyNameInput.gameObject.SetActive(false);
                    MatchLobbyText.gameObject.SetActive(false);
                    SearchMatchLobbyBtn.gameObject.SetActive(false);
                    ManageMatchLobbyBtn.gameObject.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error on OnClickQuickMatch: {ex.Message}");
            }
        }

        private Match PrepareQuickMatch(GetMatchResult matchResult)
        {
            var match = new Match();
            match.playerOneId = GetFirstPlayerToPlay(matchResult.Members[0].Entity.Id, matchResult.Members[1].Entity.Id);
            match.playerTwoId = matchResult.Members.FirstOrDefault(m => m.Entity.Id != match.playerOneId).Entity.Id;
            return match;
        }

        private string GetFirstPlayerToPlay(string playerOneId, string playerTwoId)
        {
            return string.Compare(playerOneId, playerTwoId, ignoreCase: true) < 0 ?
                playerOneId : playerTwoId;
        }

        private IEnumerator StartMatch(bool shouldCreateMatch)
        {
            var sharedGroupHandler = new SharedGroupHandler(ApplicationModel.CurrentPlayer);
            LookingForMatch = true;

            if (shouldCreateMatch)
            {
                yield return StartCoroutine(CreateMatch(sharedGroupHandler));
            }

            var gameCanStart = false;

            while (!gameCanStart && LookingForMatch && !string.IsNullOrWhiteSpace(ApplicationModel.CurrentSharedGroupData?.sharedGroupId))
            {
                yield return sharedGroupHandler.Get(ApplicationModel.CurrentSharedGroupData.sharedGroupId);

                gameCanStart = !string.IsNullOrWhiteSpace(sharedGroupHandler.SharedGroupData?.match?.playerOneId)
                            && !string.IsNullOrWhiteSpace(sharedGroupHandler.SharedGroupData?.match?.playerTwoId)
                            && !string.IsNullOrWhiteSpace(sharedGroupHandler.SharedGroupData?.sharedGroupId);

                yield return new WaitForSeconds(Constants.RETRY_GET_LOBBY_INFO_AFTER_SECONDS);
            }

            if (gameCanStart)
            {
                ApplicationModel.CurrentSharedGroupData = sharedGroupHandler.SharedGroupData;
                SceneManager.LoadScene("Game");
            }
            else
            {
                ApplicationModel.Reset();
                UpdateGameStatus(Constants.COULD_NOT_START_GAME);
            }
        }

        private IEnumerator CreateMatch(SharedGroupHandler sharedGroupHandler)
        {
            var isPlayerOne = ApplicationModel.CurrentPlayer.Entity.Id == ApplicationModel.CurrentMatch.playerOneId;

            ApplicationModel.CurrentSharedGroupData.sharedGroupId = $"{ApplicationModel.CurrentMatch.playerOneId}-{ApplicationModel.CurrentMatch.playerTwoId}";

            Debug.Log(ApplicationModel.CurrentSharedGroupData.sharedGroupId);

            if (isPlayerOne)
            {
                yield return sharedGroupHandler.Create(ApplicationModel.CurrentSharedGroupData.sharedGroupId);
            }
            else
            {
                var matchHandler = new MatchHandler(ApplicationModel.CurrentPlayer, ApplicationModel.CurrentSharedGroupData.sharedGroupId);

                while (string.IsNullOrWhiteSpace(matchHandler.TicTacToeSharedGroupData?.match?.playerTwoId) && LookingForMatch)
                {
                    Debug.Log("Joining to match");
                    yield return matchHandler.JoinMatch();
                    yield return new WaitForSeconds(Constants.RETRY_GET_LOBBY_INFO_AFTER_SECONDS);
                }
            }
        }

        #endregion Quick Match

        #region Match Lobby

        private IEnumerator CreateGroup(string groupName)
        {
            ApplicationModel.ConnectedToLobby = false;
            var sharedGroupHandler = new SharedGroupHandler(ApplicationModel.CurrentPlayer);
            yield return StartCoroutine(sharedGroupHandler.Create(groupName));

            if (!string.IsNullOrWhiteSpace(sharedGroupHandler.SharedGroupId))
            {
                var matchLobbyHandler = new MatchLobbyHandler(ApplicationModel.CurrentPlayer);
                yield return StartCoroutine(matchLobbyHandler.CreateMatchLobby(sharedGroupHandler.SharedGroupId));
                ApplicationModel.CurrentSharedGroupData = matchLobbyHandler.TicTacToeSharedGroupData;
                ApplicationModel.ConnectedToLobby = ApplicationModel.CurrentMatch.playerOneId == ApplicationModel.CurrentPlayer.PlayFabId;
            }
        }

        private void OnClickExitBtn()
        {
            if (LookingForMatch)
            {
                StartCoroutine(CancelMatchmaking());
            }

            Application.Quit();
        }

        private IEnumerator JoinMatchLobby()
        {
            ApplicationModel.JoinToMatchLobby = false;
            yield return StartCoroutine(ConnectMatchLobby(ApplicationModel.CurrentMatchLobbyToJoin.matchLobbyId));

            if (ApplicationModel.CurrentMatchLobby == null)
            {
                UpdateGameStatus(Constants.MATCH_LOBBY_JOIN_ERROR);
            }
            else
            {
                UpdateUIMatchLobbyConnection();
                yield return StartCoroutine(StartMatch(shouldCreateMatch: false));
            }
        }

        private IEnumerator ConnectMatchLobby(string matchLobbyId)
        {
            ApplicationModel.ConnectedToLobby = false;
            var matchLobbyHandler = new MatchLobbyHandler(ApplicationModel.CurrentPlayer);
            yield return matchLobbyHandler.JoinMatchLobby(matchLobbyId);
            ApplicationModel.CurrentSharedGroupData = matchLobbyHandler.TicTacToeSharedGroupData ?? new TicTacToeSharedGroupData();

            if (ApplicationModel.CurrentMatchLobby == null)
            {
                UpdateGameStatus(Constants.MATCH_LOBBY_JOIN_ERROR);
            }
            else
            {
                ApplicationModel.ConnectedToLobby = ApplicationModel.CurrentMatch.playerTwoId == ApplicationModel.CurrentPlayer.PlayFabId;
            }
        }

        private void ManageMatchLobbyBtnOnClick()
        {
            if (IsLobbyCreated)
            {
                StartCoroutine(CloseMatchLobby());
                return;
            }

            if (string.IsNullOrWhiteSpace(MatchLobbyNameInput.text))
            {
                UpdateGameStatus(Constants.LOBBY_NAME_EMPTY);
                return;
            }

            StartCoroutine(CreateMatchLobby(MatchLobbyNameInput.text));
        }

        private IEnumerator CreateMatchLobby(string sessionName)
        {
            var error = Constants.CREATE_MATCH_LOBBY_ERROR;

            UpdateGameStatus(Constants.MATCH_LOBBY_CREATING + sessionName);

            try
            {
                IsLobbyCreated = true;
                yield return StartCoroutine(CreateGroup(sessionName));

                if (ApplicationModel.CurrentMatchLobby == null)
                {
                    error = Constants.MATCH_LOBBY_REPEATED;
                }
                else
                {
                    UpdateUIMatchLobbyConnection();
                    error = string.Empty;
                    yield return StartCoroutine(StartMatch(shouldCreateMatch: false));
                    yield return StartCoroutine(DeleteMatchLobby(ApplicationModel.CurrentMatchLobby.matchLobbyId));
                }
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(error))
                {
                    IsLobbyCreated = false;
                    UpdateGameStatus(error);
                }
            }
        }

        private void UpdateUIMatchLobbyConnection()
        {
            QuickMatchBtn.gameObject.SetActive(false);
            MatchLobbyNameInput.gameObject.SetActive(false);
            SearchMatchLobbyBtn.gameObject.SetActive(false);
            ManageMatchLobbyBtn.GetComponentInChildren<Text>().text = Constants.BTN_CLOSE_MATCH_LOBBY;
            UpdateGameStatus(Constants.MATCH_LOBBY_WAITING);
        }

        private IEnumerator CloseMatchLobby()
        {
            UpdateGameStatus(Constants.MATCH_LOBBY_CLOSING);
            IsLobbyCreated = false;

            if (ApplicationModel.CurrentMatchLobby != null)
            {
                yield return StartCoroutine(DeleteMatchLobby(ApplicationModel.CurrentMatchLobby.matchLobbyId));
                var sharedGroupHandler = new SharedGroupHandler(ApplicationModel.CurrentPlayer);
                yield return sharedGroupHandler.Delete(ApplicationModel.CurrentSharedGroupData.sharedGroupId);
            }

            ApplicationModel.CurrentMatchLobby = null;
            ApplicationModel.ConnectedToLobby = false;
            MatchLobbyText.text = string.Empty;
            QuickMatchBtn.gameObject.SetActive(true);
            MatchLobbyNameInput.gameObject.SetActive(true);
            SearchMatchLobbyBtn.gameObject.SetActive(true);
            ManageMatchLobbyBtn.GetComponentInChildren<Text>().text = Constants.BTN_CREATE_MATCH_LOBBY;
        }

        public IEnumerator DeleteMatchLobby(string matchLobbyId)
        {
            var matchLobbyHandler = new MatchLobbyHandler(ApplicationModel.CurrentPlayer);
            yield return matchLobbyHandler.DeleteMatchLobby(matchLobbyId);
            Debug.Log($"MatchLobby deleted: {matchLobbyId}");
            ApplicationModel.CurrentMatchLobby = null;
        }

        #endregion Match Lobby

        #region Search

        private void OnMatchLobbySearchClick()
        {
            UpdateGameStatus(Constants.MATCH_LOBBY_SEARCHING);
            StartCoroutine(SearchMatchLobby(MatchLobbyNameInput.text));
        }

        private IEnumerator SearchMatchLobby(string filter)
        {
            var matchLobbyHandler = new MatchLobbyHandler(ApplicationModel.CurrentPlayer);
            yield return matchLobbyHandler.GetMatchLobbyList(filter);

            ApplicationModel.MatchLobbyList = matchLobbyHandler.MatchLobbyList;
            ApplicationModel.MatchLobbyListHasChanged = true;
            UpdateGameStatus((ApplicationModel.MatchLobbyList?.Count ?? default) + Constants.MATCH_LOBBY_FOUND);
        }

        #endregion Search

        #region Login

        private void OnPlayerLogin(PlayerInfo playerInfo)
        {
            UpdateGameStatus(Constants.PLAYER_LOGIN_COMPLETED);
            ApplicationModel.CurrentPlayer = playerInfo;
        }

        private void OnLoginFail()
        {
            UpdateGameStatus(Constants.PLAYER_LOGIN_FAILED);
            throw new Exception("Failed to login.");
        }

        #endregion Login
    }
}