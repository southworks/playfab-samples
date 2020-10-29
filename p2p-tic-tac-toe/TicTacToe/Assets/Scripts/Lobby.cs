using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Helpers;
using PlayFab.MultiplayerModels;
using PlayFab.Party;
using TicTacToe.Handlers;
using TicTacToe.Helpers;
using TicTacToe.Models;
using TicTacToe.Models.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StatusCode = Assets.Scripts.Models.Responses.StatusCode;

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

        public InvitationCodeModal invitationCodeModal;
        public MatchLobbyPlayerListPopulate matchLobbyPlayerListPopulate;

        #endregion Lobby Game Objects

        #region Fields

        public MatchmakingHandler matchmakingHandler;
        private bool LookingForMatch;
        private bool IsLobbyCreated;
        private PartyNetworkHandler partyNetworkHandler;
        #endregion Fields

        #region Unity Events

        private void Awake()
        {
            matchLobbyPlayerListPopulate = FindObjectOfType(typeof(MatchLobbyPlayerListPopulate)) as MatchLobbyPlayerListPopulate;
        }

        void Start()
        {
            if (ApplicationModel.CurrentPlayer == null)
            {
                var loginHandler = new LoginHandler();
                loginHandler.Login(OnPlayerLogin, OnLoginFail);
            }

            if (partyNetworkHandler == null)
            {
                partyNetworkHandler = new PartyNetworkHandler();
                SetPartyNetworkHandlerListeners();
            }

            partyNetworkHandler.LeaveNetwork();

            ApplicationModel.Reset();
            LookingForMatch = false;
            ManageMatchLobbyBtn.onClick.AddListener(ManageMatchLobbyBtnOnClick);
            ExitBtn.onClick.AddListener(OnClickExitBtn);
            QuickMatchBtn.onClick.AddListener(OnClickQuickMatch);
            SearchMatchLobbyBtn.onClick.AddListener(OnMatchLobbySearchClick);

            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private void SceneManager_sceneUnloaded(Scene _)
        {
            partyNetworkHandler.OnDataMessageNoCopyReceived -= OnDataMessageNoCopyReceivedListener;
        }

        void Update()
        {
            if (ApplicationModel.JoinToMatchLobby)
            {
                StartCoroutine(JoinMatchLobby());
            }

            if (ApplicationModel.StartMatch)
            {
                ApplicationModel.StartMatch = false;
                StartMatch();
            }
        }

        #endregion Unity Events

        #region Common

        private void UpdateGameStatus(string statusText)
        {
            GameStatusTxt.GetComponent<Text>().enabled = true;
            GameStatusTxt.text = statusText;
        }

        private void CreateCurrentMatch()
        {
            ApplicationModel.CurrentMatch = new Match
            {
                playerOneId = ApplicationModel.NetworkCreatorId,
                playerTwoId = ApplicationModel.NetworkCreatorId == ApplicationModel.CurrentPlayer.Entity.Id ?
                                ApplicationModel.JoinedPlayerId :
                                ApplicationModel.CurrentPlayer.Entity.Id
            };
        }

        private void OnClickExitBtn()
        {
            if (LookingForMatch)
            {
                StartCoroutine(CancelMatchmaking());
            }

            Application.Quit();
        }

        #endregion Common

        #region Quick Match

        private IEnumerator CancelMatchmaking()
        {
            UpdateGameStatus(Constants.TICKET_CANCEL_STARTED);
            LookingForMatch = false;

            // ensure that tickets from all the possible queues are cancelled
            foreach (var queueType in Enum.GetValues(typeof(QueueTypes)).Cast<QueueTypes>().ToList())
            {
                matchmakingHandler.ChangeQueueConfiguration(queueType);
                yield return StartCoroutine(matchmakingHandler.CancelPlayerTickets());
            }

            UpdateGameStatus(Constants.TICKET_CANEL);
        }

        private void CreateMatchmakingHandler()
        {
            if (matchmakingHandler != null) return;

            matchmakingHandler = new MatchmakingHandler(ApplicationModel.CurrentPlayer);
        }

        private void OnClickQuickMatch()
        {
            try
            {
                CreateMatchmakingHandler();

                if (LookingForMatch)
                {
                    StartCoroutine(CancelMatchmaking());
                    QuickMatchBtn.GetComponentInChildren<Text>().text = Constants.BUTTON_QUICKMATCH_QUICKMATCHED_CANCELED;
                    MatchLobbyNameInput.gameObject.SetActive(true);
                    MatchLobbyText.gameObject.SetActive(true);
                    SearchMatchLobbyBtn.gameObject.SetActive(true);
                    ManageMatchLobbyBtn.gameObject.SetActive(true);
                }
                else
                {
                    StartCoroutine(CreateQuickMatch());
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

        private IEnumerator CreateQuickMatch()
        {
            yield return StartCoroutine(CancelMatchmaking());

            LookingForMatch = true;
            UpdateGameStatus(Constants.TICKET_SEARCH);

            // Create the matchmaking Ticket.
            matchmakingHandler.ChangeQueueConfiguration(QueueTypes.QuickMatch);
            ApplicationModel.IsQuickMatchFlow = true;
            yield return StartCoroutine(matchmakingHandler.CreateTicket(attribute: "skill"));
            yield return StartCoroutine(ProcessTicket());
        }

        private IEnumerator ProcessTicket(bool isPartyQuickMatch = false)
        {
            matchmakingHandler.ChangeQueueConfiguration(isPartyQuickMatch ? QueueTypes.Party : QueueTypes.QuickMatch);

            // We request the matchmaking Ticket's status for checking if there is a match or not.
            yield return StartCoroutine(matchmakingHandler.EnsureGetTicketStatus());
            var ticketStatus = matchmakingHandler.MatchmakingTicketStatus;

            // Finally, we get the match's info
            if (ticketStatus != null && ticketStatus.Status == MatchmakingTicketStatusEnum.Matched)
            {
                yield return StartCoroutine(matchmakingHandler.GetMatch());

                var matchInfo = matchmakingHandler.MatchResult;

                if (isPartyQuickMatch)
                {
                    UpdateGameStatus(Constants.TICKET_MATCH);
                    yield return StartCoroutine(ProcessPartyQuickMatch(matchInfo));
                }
                else
                {
                    UpdateGameStatus(Constants.CREATING_NETWORK);
                    yield return StartCoroutine(PreparePartyQuickMatch(matchInfo));
                }
            }
            else
            {
                LookingForMatch = false;
                UpdateGameStatus(Constants.TICKET_TIMEDOUT_OR_CANCELLED);
            }
        }

        private IEnumerator PreparePartyQuickMatch(GetMatchResult match)
        {
            var (playerOne, _) = GetPlayersFromMatch(match);
            var networkId = string.Empty;

            // The PlayerOne is the Party Network creator
            if (ApplicationModel.CurrentPlayer.Entity.Id == playerOne.Entity.Id)
            {
                // We ensure that player one isn't connected to any network
                if (!string.IsNullOrWhiteSpace(partyNetworkHandler.NetworkId))
                {
                    yield return partyNetworkHandler.LeaveNetwork();
                }

                yield return StartCoroutine(partyNetworkHandler.CreateAndJoinToNetwork());

                networkId = partyNetworkHandler.NetworkId;
                ApplicationModel.NetworkCreatorId = ApplicationModel.CurrentPlayer.Entity.Id;
            }

            matchmakingHandler.ChangeQueueConfiguration(QueueTypes.Party);
            // player one sends the networkId; player two sends an empty string.
            yield return StartCoroutine(matchmakingHandler.CreateTicket(attribute: match.MatchId, networkId: networkId));
            yield return StartCoroutine(ProcessTicket(isPartyQuickMatch: true));
        }

        private IEnumerator ProcessPartyQuickMatch(GetMatchResult match)
        {
            ApplicationModel.CurrentMatchResult = match;
            var (playerOne, playerTwo) = GetPlayersFromMatch(match);

            if (playerTwo.Entity.Id == ApplicationModel.CurrentPlayer.Entity.Id)
            {
                // We ensure that player two isn't connected to any network
                if (!string.IsNullOrWhiteSpace(partyNetworkHandler.NetworkId))
                {
                    yield return partyNetworkHandler.LeaveNetwork();
                }

                var networkId = GetNetworkIdFromMatch(match);
                ApplicationModel.NetworkCreatorId = playerOne.Entity.Id;
                yield return StartCoroutine(partyNetworkHandler.JoinNetwork(networkId));
            }

            yield return null;
        }

        private (MatchmakingPlayerWithTeamAssignment playerOne, MatchmakingPlayerWithTeamAssignment playerTwo) GetPlayersFromMatch(GetMatchResult match)
        {
            var orderedPlayers = match?.Members?.OrderBy(member => member.Entity.Id).ToList() ?? new List<MatchmakingPlayerWithTeamAssignment>();
            var playerOne = orderedPlayers?.ElementAtOrDefault(0) ?? null;
            var playerTwo = orderedPlayers?.ElementAtOrDefault(1) ?? null;

            return (playerOne, playerTwo);
        }

        private string GetNetworkIdFromMatch(GetMatchResult match)
        {
            var (playerOne, playerTwo) = GetPlayersFromMatch(match);

            var playerOneData = JsonUtility.FromJson<PartyTicketAttributes>(playerOne?.Attributes?.DataObject.ToString() ?? "{}");
            var playerTwoData = JsonUtility.FromJson<PartyTicketAttributes>(playerTwo?.Attributes?.DataObject.ToString() ?? "{}");

            return !string.IsNullOrWhiteSpace(playerOneData?.NetworkId) ? playerOneData?.NetworkId : (playerTwoData?.NetworkId);
        }

        private void StartMatch()
        {
            // note: remote players property doesn't consider the current player.
            var gameCanStart = (partyNetworkHandler.RemotePlayers.Count + 1) == Constants.NUMBER_OF_PLAYERS;

            if (gameCanStart)
            {
                CreateCurrentMatch();
                SceneManager.LoadScene("Game");
            }
            else
            {
                ApplicationModel.Reset();
                UpdateGameStatus(Constants.COULD_NOT_START_GAME);
            }
        }

        #endregion Quick Match

        #region Match Lobby

        private IEnumerator TryCreateMatchLobby(string sessionName)
        {
            var error = Constants.CREATE_MATCH_LOBBY_ERROR;

            UpdateGameStatus(Constants.MATCH_LOBBY_CREATING + sessionName);

            try
            {
                IsLobbyCreated = true;
                yield return StartCoroutine(CreateMatchLobby(sessionName, true));

                if (ApplicationModel.CurrentMatchLobby == null)
                {
                    error = Constants.MATCH_LOBBY_REPEATED;
                }
                else
                {
                    ApplicationModel.CreateRemotePlayersHandler(PlayFabMultiplayerManager.Get());
                    SceneManager.LoadScene("MatchLobby");
                    error = string.Empty;
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

        private IEnumerator CreateMatchLobby(string groupName, bool locked)
        {
            yield return StartCoroutine(partyNetworkHandler.CreateAndJoinToNetwork());
            ApplicationModel.NetworkCreatorId = ApplicationModel.CurrentPlayer.Entity.Id;

            if (string.IsNullOrWhiteSpace(partyNetworkHandler.NetworkId))
            {
                yield return null;
            }

            var matchLobbyHandler = new MatchLobbyHandler(ApplicationModel.CurrentPlayer);
            yield return StartCoroutine(matchLobbyHandler.CreateMatchLobby(groupName, partyNetworkHandler.NetworkId, locked));

            ApplicationModel.CurrentMatchLobby = matchLobbyHandler.MatchLobby;
        }

        private IEnumerator JoinMatchLobby()
        {
            ApplicationModel.JoinToMatchLobby = false;
            var cancel = false;

            if (ApplicationModel.CurrentMatchLobbyToJoin.locked)
            {
                InvitationCodeModal.InvitationCodeModalResult modalResult = null;
                yield return AskForInvitationCode((result) => { modalResult = result; });
                cancel = modalResult.OptionSelected != InvitationCodeModal.OptionSelected.OK;

                if (!cancel)
                {
                    yield return StartCoroutine(ConnectMatchLobby(ApplicationModel.CurrentMatchLobbyToJoin.matchLobbyId, invitationCodeModal.Result.InvitationCode));
                }
            }
            else
            {
                yield return StartCoroutine(ConnectMatchLobby(ApplicationModel.CurrentMatchLobbyToJoin.matchLobbyId));
            }

            if (!cancel && ApplicationModel.CurrentMatchLobby != null)
            {
                // HACK: set that the lobby players list has changed in order to trigger the players renderization
                ApplicationModel.CreateRemotePlayersHandler(PlayFabMultiplayerManager.Get());
                ApplicationModel.MatchLobbyJoinedPlayerListHasChanged = true;
                SceneManager.LoadScene("MatchLobby");
            }
        }

        private IEnumerator ConnectMatchLobby(string matchLobbyId, string invitationCode = null)
        {
            var matchLobbyHandler = new MatchLobbyHandler(ApplicationModel.CurrentPlayer);
            yield return matchLobbyHandler.JoinMatchLobby(matchLobbyId, invitationCode);

            var joinResponse = matchLobbyHandler.JoinMatchLobbyResponse;
            GUIHelper.UpdateGUIMessageWithStatusCode(GameStatusTxt, joinResponse.statusCode);

            if (joinResponse.statusCode == StatusCode.OK)
            {
                ApplicationModel.CurrentMatchLobby = ApplicationModel.CurrentMatchLobbyToJoin;
                ApplicationModel.NetworkCreatorId = ApplicationModel.CurrentMatchLobby.creatorId;
                yield return partyNetworkHandler.JoinNetwork(joinResponse.response.networkId);
            }
            else
            {
                if (joinResponse.statusCode == StatusCode.NotInvitationCodeIncluded)
                {
                    InvitationCodeModal.InvitationCodeModalResult modalResult = null;
                    yield return AskForInvitationCode((result) => { modalResult = result; });

                    if (modalResult.OptionSelected == InvitationCodeModal.OptionSelected.OK && !string.IsNullOrWhiteSpace(modalResult.InvitationCode))
                    {
                        yield return ConnectMatchLobby(matchLobbyId, modalResult.InvitationCode);
                    }
                }
            }
        }

        private IEnumerator CloseMatchLobby()
        {
            UpdateGameStatus(Constants.MATCH_LOBBY_CLOSING);
            IsLobbyCreated = false;

            if (ApplicationModel.CurrentMatchLobby != null)
            {
                yield return StartCoroutine(DeleteMatchLobbyAndLeaveNetwork(ApplicationModel.CurrentMatchLobby.matchLobbyId));
            }

            ApplicationModel.CurrentMatchLobby = null;
            MatchLobbyText.text = string.Empty;
            QuickMatchBtn.gameObject.SetActive(true);
            MatchLobbyNameInput.gameObject.SetActive(true);
            SearchMatchLobbyBtn.gameObject.SetActive(true);
            ManageMatchLobbyBtn.GetComponentInChildren<Text>().text = Constants.BTN_CREATE_MATCH_LOBBY;
        }

        private IEnumerator AskForInvitationCode(Action<InvitationCodeModal.InvitationCodeModalResult> resultHandler)
        {
            SetInteractableLobbyPanelElements(false);
            yield return CoroutineHelper.Run(invitationCodeModal.Show(), resultHandler);
            SetInteractableLobbyPanelElements(true);
        }

        private void SetInteractableLobbyPanelElements(bool value)
        {
            var selectableElements = LobbyPnl.GetComponentsInChildren<Selectable>(false);

            foreach (var element in selectableElements)
            {
                element.interactable = value;
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

            StartCoroutine(TryCreateMatchLobby(MatchLobbyNameInput.text));
        }

        public IEnumerator DeleteMatchLobbyAndLeaveNetwork(string matchLobbyId)
        {
            var matchLobbyHandler = new MatchLobbyHandler(ApplicationModel.CurrentPlayer);
            yield return matchLobbyHandler.DeleteMatchLobby(matchLobbyId);
            yield return partyNetworkHandler.LeaveNetwork();
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
            ApplicationModel.CurrentPlayer = playerInfo;
        }

        private void OnLoginFail()
        {
            UpdateGameStatus(Constants.PLAYER_LOGIN_FAILED);
            throw new Exception("Failed to login.");
        }

        #endregion Login

        #region PartyNetworkListeners

        //TODO: listeners might be removed for the Match Lobby part (check if Quick Match uses them).
        // we might need a variable in ApplicationModel (don't forget to reset it) for differencing a 
        // match lobby from a quick-match for this case.
        private void SetPartyNetworkHandlerListeners()
        {
            partyNetworkHandler.AddOnRemotePlayerJoinedListener(OnRemotePlayerJoinedListener);
            partyNetworkHandler.AddOnDataMessageNoCopyReceivedListener(OnDataMessageNoCopyReceivedListener);
        }

        private void OnRemotePlayerJoinedListener(object sender, PlayFabPlayer player)
        {
            // this listener should only work for Quick Match flow
            if (!ApplicationModel.IsQuickMatchFlow)
            {
                return;
            }

            var remotePlayers = partyNetworkHandler.RemotePlayers;

            // note: remote players property doesn't consider the current player.
            if (IsPartyNetworkCreator() && (remotePlayers.Count + 1) == Constants.NUMBER_OF_PLAYERS)
            {
                ApplicationModel.JoinedPlayerId = player.EntityKey.Id;
                ApplicationModel.StartMatch = true;

                partyNetworkHandler.SendDataMessage(
                    new PartyNetworkMessageWrapper<string>
                    {
                        MessageType = PartyNetworkMessageEnum.PlayersReady,
                        MessageData = "Players ready to play."
                    }
                );
            }
        }

        private void OnDataMessageNoCopyReceivedListener(object sender, PlayFabPlayer from, IntPtr buffer, uint bufferSize)
        {
            // this listener should only work for Quick Match flow
            if (!ApplicationModel.IsQuickMatchFlow)
            {
                return;
            }

            var parsedData = PartyNetworkMessageHelper.GetParsedDataFromBuffer<PartyNetworkMessageWrapper<string>>(buffer, bufferSize);
            Debug.Log($"OnDataMessageNoCopyReceivedListener: {parsedData.MessageType}");

            if (parsedData.MessageType == PartyNetworkMessageEnum.PlayersReady)
            {
                ApplicationModel.StartMatch = true;
            }
        }

        private bool IsPartyNetworkCreator()
        {
            var creatorId = ApplicationModel.CurrentMatchLobby?.creatorId ?? string.Empty;

            return !string.IsNullOrWhiteSpace(creatorId) ?
                    creatorId == ApplicationModel.CurrentPlayer.Entity.Id :
                    partyNetworkHandler.isNetworkCreator;
        }

        #endregion
    }
}