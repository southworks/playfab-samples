namespace TicTacToeFunctions.Util
{
    public static class Constants
    {
        public const string PlayfabDevSecretKey = "PLAYFAB_DEV_SECRET_KEY";
        public const string PlayFabTitleId = "PLAYFAB_TITLE_ID";
        public const string PlayFabCloudName = "PLAYFAB_CLOUD_NAME";
        public const string DatabaseName = "PlayFabTicTacToe";
        public const string MatchLobbyTableName = "MatchLobby";
        public const string ExceptionLobbyNotFound = "Lobby not found";
        public const string ExceptionLobbyIsFull = "Lobby is full";
        public const string ExceptionRequesterIsCreator = "The player two is already the lobby player one";
        public const string ExceptionMissingInvitationCode = "Lobby locked. Invitation code not sent";
        public const string ExceptionInvalidInvitationCode = "Lobby locked. Invalid invitation code";
        public const string ExceptionOnlyCreatorCanLockLobby = "Only the lobby creator can modify the lock state";
    }
}
