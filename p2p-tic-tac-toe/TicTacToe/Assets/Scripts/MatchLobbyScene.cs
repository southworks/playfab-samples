using System;
using System.Collections;
using Assets.Scripts.Helpers;
using Assets.Scripts.Models.Responses;
using PlayFab.Party;
using TicTacToe.Handlers;
using TicTacToe.Helpers;
using TicTacToe.Models;
using TicTacToe.Models.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TicTacToe
{
    public class MatchLobbyScene : MonoBehaviour
    {
        #region Game Object

        public Button StartMatchBtn;
        public Button LeaveMatchBtn;
        public Button CancelMatchBtn;
        public Text MatchLobbyTitleTxt;
        public Text GameStatusTxt;
        public InputField InvitationCodeField;
        public GameObject LockedStateTgl;

        #endregion

        #region Fields

        private MatchLobby MatchLobby;
        private PlayerInfo CurrentPlayer;
        private PartyNetworkHandler PartyNetworkHandler;
        private MatchLobbyHandler MatchLobbyHandler;
        private Toggle ToggleController;
        private bool playerLeft = false;
        private bool playerJoined = false;
        private bool runLeaveMatchProcess = false;

        public MatchLobbyPlayerListPopulate matchLobbyPlayerListPopulate;

        #endregion

        #region Unity Events

        private void Awake()
        {
            matchLobbyPlayerListPopulate = FindObjectOfType(typeof(MatchLobbyPlayerListPopulate)) as MatchLobbyPlayerListPopulate;
        }

        // Start is called before the first frame update
        void Start()
        {
            SetGameObjectListeners();

            if (ApplicationModel.CurrentMatchLobby != null)
            {
                MatchLobby = ApplicationModel.CurrentMatchLobby;
                MatchLobbyTitleTxt.text = $"Match Lobby: { ApplicationModel.CurrentMatchLobby.matchLobbyId }";
            }

            if (ApplicationModel.CurrentPlayer != null)
            {
                CurrentPlayer = ApplicationModel.CurrentPlayer;
            }

            if (PartyNetworkHandler == null)
            {
                PartyNetworkHandler = new PartyNetworkHandler();
                SetPartyNetworkHandlerListeners();
            }

            MatchLobbyHandler = new MatchLobbyHandler(ApplicationModel.CurrentPlayer);

            InitializeScene();
            InvitationCodeField.text = string.Empty;
        }

        // Update is called once per frame
        void Update()
        {
            // this should be handled here and not in an event listener (another thread) as this method make changes in the UI
            if (playerLeft)
            {
                playerLeft = false;
                ProcessPlayerLeft();
            }

            // this should be handled here and not in an event listener (another thread) as this method make changes in the UI
            if (playerJoined)
            {
                playerJoined = false;
                ProcessPlayerJoined();
            }

            // this should be handled here and not in an event listener (another thread) as this method make changes in the UI
            if (runLeaveMatchProcess)
            {
                runLeaveMatchProcess = true;
                LeaveMatchProcess();
            }

            if (ApplicationModel.CurrentMatchLobby.locked && string.IsNullOrEmpty(InvitationCodeField.text))
            {
                InvitationCodeField.text = $"Invitation Code: {PartyNetworkHandler?.InvitationId}";
            }
        }

        #endregion

        #region Scene management methods

        private void InitializeScene()
        {
            var isNetworkCreator = IsPartyNetworkCreator();

            // the start and cancel match buttons will be only available for the Player One (Network Creator)
            StartMatchBtn.gameObject.SetActive(isNetworkCreator);
            CancelMatchBtn.gameObject.SetActive(isNetworkCreator);

            // the leave match button will be only available for the Player Two
            LeaveMatchBtn.gameObject.SetActive(!isNetworkCreator);

            EnableDisableStartMatch();

            // at the beggining of the scene, we won't show any status.
            GUIHelper.UpdateGUIMessage(GameStatusTxt, string.Empty);
        }

        private void EnableDisableStartMatch()
        {
            StartMatchBtn.enabled = StartMatchConditionsMet();
        }

        private void StartMatch()
        {
            if (StartMatchConditionsMet())
            {
                if (ApplicationModel.CurrentMatchLobby != null && ApplicationModel.IsHost)
                {
                    var matchLobbyHandler = new MatchLobbyHandler(ApplicationModel.CurrentPlayer);
                    StartCoroutine(matchLobbyHandler.DeleteMatchLobby(ApplicationModel.CurrentMatchLobby.matchLobbyId));
                }

                CreateCurrentMatch();
                SceneManager.LoadScene("Game");
            }
        }

        #endregion

        #region Game Scene Logic's methods

        public bool StartMatchConditionsMet()
        {
            // if we don't have any player connected to the Network, we won't be able to start the match
            // NOTE: the "+1" is for considering the current players, as the remotePlayers doesn't consider it.
            if ((PartyNetworkHandler?.RemotePlayers?.Count ?? 0) + 1 != Constants.NUMBER_OF_PLAYERS)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Game Object Listeners

        private void SetGameObjectListeners()
        {
            StartMatchBtn.onClick.AddListener(StartMatchBtnOnClick);
            CancelMatchBtn.onClick.AddListener(CancelMatchBtnOnClick);
            LeaveMatchBtn.onClick.AddListener(LeaveMatchBtnOnClick);
            ToggleController = LockedStateTgl.GetComponent<Toggle>();
            ToggleController.OnValueChanged += LockedStateToggleOnChange;
        }

        /// <summary>
        /// This method could only be executed by the Match Lobby creator.
        /// </summary>
        private void StartMatchBtnOnClick()
        {
            // Match Lobby creator notifies the rest of the players that they can start the game.
            SendPlayersReadyMessage();
            StartMatch();
        }

        /// <summary>
        /// This method could only be executed by the Match Lobby creator.
        /// </summary>
        private void CancelMatchBtnOnClick()
        {
            // we notify to the lobby players that the match is cancelled.
            SendMatchLobbyCancelledMessage();

            // then, we delete the current Match Lobby
            StartCoroutine(DeleteMatchLobby());

            StartCoroutine(PartyNetworkHandler.LeaveNetwork());

            // Return to Lobby search scene
            SceneManager.LoadScene("Lobby");
        }

        /// <summary>
        /// This method could only be executed by the Match Lobby "joiners".
        /// </summary>
        private void LeaveMatchBtnOnClick()
        {
            LeaveMatchProcess();
        }

        private void LeaveMatchProcess()
        {
            // we make the Player two leave the Lobby.
            StartCoroutine(LeaveMatchLobby());

            // NOTE: it's not necessary to send a notification over the Network about the player leaving the lobby,
            // as this will be handled by the "RemotePlayerLeftListener" method.
            StartCoroutine(LeaveNetwork());

            // Return to Lobby search scene
            SceneManager.LoadScene("Lobby");
        }

        private void LockedStateToggleOnChange(bool lockState)
        {
            StartCoroutine(SetMatchLobbyLockState(MatchLobby.matchLobbyId, lockState));
        }

        private IEnumerator SetMatchLobbyLockState(string matchLobbyId, bool lockState)
        {
            ToggleController.SetEnabled(false);
            yield return MatchLobbyHandler.SetMatchLobbyLockState(matchLobbyId, lockState);

            if (MatchLobbyHandler.SetMatchLobbyLockStateResponse == null || MatchLobbyHandler.SetMatchLobbyLockStateResponse.statusCode != StatusCode.OK)
            {
                GUIHelper.UpdateGUIMessageWithStatusCode(GameStatusTxt, MatchLobbyHandler.SetMatchLobbyLockStateResponse.statusCode);
            }
            else
            {
                ApplicationModel.CurrentMatchLobby = MatchLobbyHandler.SetMatchLobbyLockStateResponse.response.matchLobby;
                GUIHelper.UpdateGUIMessage(
                    GameStatusTxt,
                    ApplicationModel.CurrentMatchLobby.locked ?
                        Constants.SET_MATCH_LOBBY_LOCK_STATE_OK_ON
                        : Constants.SET_MATCH_LOBBY_LOCK_STATE_OK_OFF
                );

                if (!ApplicationModel.CurrentMatchLobby.locked)
                {
                    InvitationCodeField.text = string.Empty;
                }
            }

            ToggleController.SetEnabled(true);
        }

        #endregion

        #region Party Network methods

        private void SendPlayersReadyMessage()
        {
            SendMessage(PartyNetworkMessageEnum.PlayersReady, "Players ready to play.");
        }

        private void SendMatchLobbyCancelledMessage()
        {
            SendMessage(PartyNetworkMessageEnum.MatchLobbyCancelled, "Match Lobby cancelled.");
        }

        private void SendPlayerKickedMessage(string playerEntityId)
        {
            SendMessage(PartyNetworkMessageEnum.PlayerKicked, playerEntityId);
        }

        private void SendMessage(PartyNetworkMessageEnum type, string messageData)
        {
            PartyNetworkHandler.SendDataMessage(
                new PartyNetworkMessageWrapper<string>
                {
                    MessageType = type,
                    MessageData = messageData
                }
            );
        }

        private bool IsPartyNetworkCreator()
        {
            return !string.IsNullOrWhiteSpace(MatchLobby.creatorId) && MatchLobby.creatorId == CurrentPlayer.Entity.Id;
        }

        private IEnumerator LeaveNetwork()
        {
            yield return PartyNetworkHandler.LeaveNetwork();
        }

        /// <summary>
        /// This method should be executed by the players joined to the lobby. It will be triggered after
        /// receiving a "Players ready" message in the Party Network (see "OnDataMessageNoCopyReceivedListener").
        /// This happens after the Match Lobby creator press the Start Match button.
        /// </summary>
        private void ProcessPlayersReadyMessage()
        {
            if (!IsPartyNetworkCreator())
            {
                StartMatch();
            }
        }

        /// <summary>
        /// This method should be executed by the players joined to the lobby. It will be triggered after
        /// receiving a "Match Lobby cancelled" message in the Party Network
        /// (see "OnDataMessageNoCopyReceivedListener"). This happens after the Match Lobby creator press the
        /// Cancel match button.
        /// </summary>
        private void ProcessMatchLobbyCancelledMessage()
        {
            if (!IsPartyNetworkCreator())
            {
                PartyNetworkHandler.LeaveNetwork();
                SceneManager.LoadScene("Lobby");
            }
        }

        private void ProcessPlayerKickedMessage(string playerEntityId)
        {
            if (!IsPartyNetworkCreator() && ApplicationModel.CurrentPlayer.Entity.Id == playerEntityId)
            {
                runLeaveMatchProcess = true;
            }
        }

        #endregion

        #region Party Network Listener

        private void SetPartyNetworkHandlerListeners()
        {
            PartyNetworkHandler.AddOnRemotePlayerJoinedListener(OnRemotePlayerJoinedListener);
            PartyNetworkHandler.AddOnRemotePlayerLeftListener(OnRemotePlayerLeftListener);
            PartyNetworkHandler.AddOnDataMessageNoCopyReceivedListener(OnDataMessageNoCopyReceivedListener);
        }

        public void OnRemotePlayerJoinedListener(object sender, PlayFabPlayer player)
        {
            if (ApplicationModel.RemotePlayersHandler != null)
            {
                ApplicationModel.RemotePlayersHandler.AddPlayer(player);
            }

            ApplicationModel.JoinedPlayerId = player.EntityKey.Id;
            playerJoined = true;

            EnsureGetMatchLobbyPlayerListPopulate()?.RenderList(ApplicationModel.RemotePlayersHandler.players);
        }

        public void OnRemotePlayerLeftListener(object sender, PlayFabPlayer player)
        {
            if (ApplicationModel.RemotePlayersHandler != null)
            {
                ApplicationModel.RemotePlayersHandler.RemovePlayer(player);
            }

            ApplicationModel.JoinedPlayerId = string.Empty;
            playerLeft = true;

            EnsureGetMatchLobbyPlayerListPopulate()?.RenderList(ApplicationModel.RemotePlayersHandler.players);
        }

        private void OnDataMessageNoCopyReceivedListener(object sender, PlayFabPlayer from, IntPtr buffer, uint bufferSize)
        {
            var parsedData = PartyNetworkMessageHelper.GetParsedDataFromBuffer<PartyNetworkMessageWrapper<string>>(buffer, bufferSize);

            switch (parsedData.MessageType)
            {
                case PartyNetworkMessageEnum.PlayersReady:
                    ProcessPlayersReadyMessage();
                    break;
                case PartyNetworkMessageEnum.MatchLobbyCancelled:
                    ProcessMatchLobbyCancelledMessage();
                    break;
                case PartyNetworkMessageEnum.PlayerKicked:
                    ProcessPlayerKickedMessage(parsedData.MessageData);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Match Lobby Management methods

        private IEnumerator DeleteMatchLobby()
        {
            var matchLobbyHandler = new MatchLobbyHandler(CurrentPlayer);
            yield return matchLobbyHandler.DeleteMatchLobby(MatchLobby.matchLobbyId);
        }

        private IEnumerator LeaveMatchLobby()
        {
            var matchLobbyHandler = new MatchLobbyHandler(CurrentPlayer);
            yield return matchLobbyHandler.LeaveMatchLobby(MatchLobby.matchLobbyId);
        }

        #endregion

        #region Player Join/Left methods

        public void ProcessPlayerJoined()
        {
            EnableDisableStartMatch();
        }

        public void ProcessPlayerLeft()
        {
            EnableDisableStartMatch();
        }

        #endregion

        #region Inter-script methods

        private MatchLobbyPlayerListPopulate EnsureGetMatchLobbyPlayerListPopulate()
        {
            if (matchLobbyPlayerListPopulate == null)
            {
                matchLobbyPlayerListPopulate = FindObjectOfType(typeof(MatchLobbyPlayerListPopulate)) as MatchLobbyPlayerListPopulate;
            }

            return matchLobbyPlayerListPopulate;
        }

        #endregion

        #region Common

        /// <summary>
        /// This is called from the MatchLobbyPlayerListPopulate Script, after we click on the kick button
        /// </summary>
        /// <param name="playerEntityId"></param>
        public void OnKickButtonClick(string playerEntityId)
        {
            SendPlayerKickedMessage(playerEntityId);
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

        #endregion
    }
}
