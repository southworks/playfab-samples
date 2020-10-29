using System.Collections.Generic;
using PlayFab.MultiplayerModels;
using PlayFab.Party;
using TicTacToe.Handlers;

namespace TicTacToe.Models
{
    public class ApplicationModel
    {
        public static PlayerInfo CurrentPlayer;
        public static Match CurrentMatch;
        public static string NetworkCreatorId = string.Empty;
        public static string JoinedPlayerId = string.Empty;
        public static List<MatchLobby> MatchLobbyList = null;
        public static bool MatchLobbyListHasChanged = false;
        public static bool JoinToMatchLobby = false;
        public static bool StartMatch = false;
        public static bool NewTurnToUpdate = false;
        public static GameState CurrentGameState = null;
        public static MatchLobby CurrentMatchLobbyToJoin = null;
        public static MatchLobby CurrentMatchLobby = null;
        public static GetMatchResult CurrentMatchResult = null;
        public static bool IsQuickMatchFlow = false;

        public static bool MatchLobbyJoinedPlayerListHasChanged = false;
        public static RemotePlayersHandler RemotePlayersHandler = null;


        public static bool IsHost
        {
            get => NetworkCreatorId == CurrentPlayer.Entity.Id;
        }

        public static bool IsMyTurn
        {
            get => CurrentPlayer.Entity.Id == CurrentGameState.currentPlayerId;
        }

        public static void CreateRemotePlayersHandler(PlayFabMultiplayerManager multiplayerManager)
        {
            RemotePlayersHandler = new RemotePlayersHandler(multiplayerManager);
        }

        public static void Reset()
        {
            CurrentMatch = null;
            NetworkCreatorId = string.Empty;
            MatchLobbyList = null;
            MatchLobbyListHasChanged = false;
            JoinToMatchLobby = false;
            StartMatch = false;
            NewTurnToUpdate = false;
            CurrentGameState = null;
            CurrentMatchLobbyToJoin = null;
            CurrentMatchResult = null;
            CurrentMatchLobby = null;
            JoinedPlayerId = string.Empty;
            IsQuickMatchFlow = false;
            MatchLobbyJoinedPlayerListHasChanged = false;
        }
    }
}
