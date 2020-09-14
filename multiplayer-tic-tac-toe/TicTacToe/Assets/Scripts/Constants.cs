// Copyright (C) Microsoft Corporation. All rights reserved.

public class Constants
{
    // FOR DEMO PURPOSES ONLY
    // It is strongly adviced to move this data out of the game client in a real production environment

    // API settings
    public const string TITLE_ID = "";
    public const bool COMPRESS_API_DATA = false;
    public const string PLAYFAB_PLAYER_CUSTOM_ID = "PLAYFAB_PLAYER_CUSTOM_ID";

    // Button caption
    public const string BUTTON_QUICKMATCH_QUICKMATCHED_CANCELED = "Quick match";
    public const string BUTTON_QUICKMATCH_QUICKMATCHED_STARTED = "Cancel match";

    // Function names
    public const string CREATE_MATCH_LOBBY = "CreateMatchLobby";
    public const string CREATE_SHARED_GROUP = "CreateSharedGroup";
    public const string DELETE_MATCH_LOBBY_FUNCTION_NAME = "DeleteMatchLobby";
    public const string DELETE_SHARED_GROUP_FUNCTION_NAME = "DeleteSharedGroup";
    public const string GET_GAME_STATUS_FUNCTION_NAME = "GetGameStatus";
    public const string GET_SHARED_GROUP = "GetSharedGroup";
    public const string JOIN_MATCH_FUNCTION_NAME = "JoinMatch";
    public const string JOIN_MATCH_LOBBY = "JoinMatchLobby";
    public const string MULTIPLAYER_MOVE_FUNCTION_NAME = "MakeMultiplayerMove";
    public const string SEARCH_MATCH_LOBBIES_FUNCTION_NAME = "SearchMatchLobbies";
    public const string START_MATCH_FUNCTION_NAME = "StartMatch";
    public const string SET_WINNER_GAME_FUNCTION_NAME = "SetGameWinner";

    // Status messages
    public const string CREATE_MATCH_LOBBY_ERROR = "There was an error trying to create a match lobby. Please try again later";
    public const string GAME_OVER = "Game over";
    public const string GAME_WIN_CHECK_COMPLETED = "Win check complete";
    public const string GAME_WIN_CHECK_STARTED = "Checking for winner";
    public const string LOBBY_NAME_EMPTY = "Please insert a name for the Lobby";
    public const string MATCH_LOBBY_CREATING = "Creating match lobby: ";
    public const string MATCH_LOBBY_CLOSING = "Closing match lobby...";
    public const string MATCH_LOBBY_FOUND = " lobbies found";
    public const string MATCH_LOBBY_JOIN_ERROR = "Match lobby is full or it doesn't exists anymore";
    public const string MATCH_LOBBY_REPEATED = "There is a lobby with the same name";
    public const string MATCH_LOBBY_SEARCHING = "Searching for lobbies ...";
    public const string MATCH_LOBBY_WAITING = "Waiting for player...";
    public const string PLAYER_LOGIN_COMPLETED = "Login successful";
    public const string PLAYER_LOGIN_FAILED = "Login failed";
    public const string PLAYER_MOVE_COMPLETED = "Player move complete";
    public const string PLAYER_MOVE_INVALID = "Invalid player move";
    public const string PLAYER_MOVE_PROCESSING = "Executing player move";
    public const string PLAYER_MOVE_WAIT = "Waiting for player move";
    public const string COULD_NOT_START_GAME = "Couldn't start the game, try again later";
    public const string TICKET_SEARCH = "Searching for opponent...";
    public const string TICKET_MATCH = "Opponent found. Starting game...";
    public const string TICKET_CANEL = "Matchmaking stopped";
    public const string TICKET_CANCEL_STARTED = "Stopping matchmaking...";
    public const string TICKET_TIMEDOUT_OR_CANCELLED = "Opponent not found or match was cancelled";

    // Buttons
    public const string BTN_CLOSE_MATCH_LOBBY = "Close match lobby";
    public const string BTN_CREATE_MATCH_LOBBY = "Create match lobby";

    // Matchmaking Queue
    public const string QUICK_MATCHMAKING_QUEUE_NAME = "TicTacToeQuickMatch";
    public const string LOBBY_MATCHMAKING_QUEUE_NAME = "LobbyQueue";

    // It's recommended to set it to 120 as default.
    public const int GIVE_UP_AFTER_SECONDS = 120;
    // It's recommended to retry getting Ticket status every 6 seconds (max 10 times per minute).
    public const float RETRY_GET_TICKET_STATUS_AFTER_SECONDS = 6;
    public const float RETRY_GET_LOBBY_INFO_AFTER_SECONDS = 1;
    public const float RETRY_GET_MATCH_INFO_AFTER_SECONDS = 1;

    public const string MATCH_LOBBY_LIST_CONTENT_CONTAINER_NAME = "MatchLobbyListContent";
}
