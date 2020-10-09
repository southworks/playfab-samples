using System.Collections.Generic;

namespace TicTacToe.Models
{
    public class ApplicationModel
    {
        public static TicTacToeSharedGroupData CurrentSharedGroupData = new TicTacToeSharedGroupData();
        public static PlayerInfo CurrentPlayer;
        public static bool IsMultiplayer;
        public static List<MatchLobby> MatchLobbyList = null;
        public static bool MatchLobbyListHasChanged = false;
        public static bool ConnectedToLobby = false;
        public static bool JoinToMatchLobby = false;
        public static MatchLobby CurrentMatchLobbyToJoin;

        public static Match CurrentMatch
        {
            get => CurrentSharedGroupData?.match ?? null;

            set => CurrentSharedGroupData.match = value;
        }

        public static GameState CurrentGameState
        {
            get => CurrentSharedGroupData?.gameState ?? null;

            set => CurrentSharedGroupData.gameState = value;
        }

        public static MatchLobby CurrentMatchLobby
        {
            get => CurrentSharedGroupData?.matchLobby ?? null;

            set => CurrentSharedGroupData.matchLobby = value;
        }

        public static void Reset()
        {
            MatchLobbyList = null;
            IsMultiplayer = false;
            ConnectedToLobby = false;
            JoinToMatchLobby = false;
            MatchLobbyListHasChanged = false;
            CurrentSharedGroupData = new TicTacToeSharedGroupData();
        }
    }
}
