# Game Flow

## Index
- [Summary][summary]
- [Architecture][architecture]
- [Pre-requisites][pre-requisites]
- [Implementation][implementation]
	- [Brief diagram description][brief-diagram-description]
	- [Unity Game][unity-game]
	- [Azure Function: Start Match][azure-function-start-match]
	- [Azure Function: Get Game Status][azure-function-get-game-status]
	- [Azure Function: Make Multiplayer Move][azure-function-make-multiplayer-move]


## Summary
This sample demonstrates how the multiplayer Tic-Tac-Toe Game was implemented using PlayFab's services.

## Architecture
Before starting explaining how this feature works, lets see how the game was implemented:

---
![alt-text][architecture-01]

---

## Pre-requisites
* Read and complete the [PlayFab configuration][playfab-config-readme].
* Read and complete the [Azure Function configuration][azure-function-config-readme].
* Read and complete the [Cosmos DB configuration][cosmos-db-config-readme].

## Implementation
---
![alt-text][game-flow-diagram-01]

---

#### Brief diagram description

In the previous diagram you can see how we've implemented the *Game Flow*. We can highlight three major parts:
- ***Part 01 - Steps 1 to 2***: In this part the Tic-Tac-Toe Game starts the *Game Flow* by creating a request to the [StartMatch][start-match-azf] Azure Function, which will initialize the *Game State*.
- ***Part 02 - Steps 3 to 6***: Here the Game will retrieve the *Game State* data using the [GetGameStatus][get-game-status-azf] Azure Function. After getting this data, the game will check if this is the current Player's turn for doing a move (*Step 5*), and in case it is, it will check if the latest performed move is an END GAME move (*Step 6*).
- ***Part 03 - Steps 7 to 13***: After checking that the latest move wasn't an END GAME move, the game will ask for a Player move. Once it's done, the Tic-Tac-Toe Game, using the [MakeMultiplayerMove][make-multiplayer-move-azf] Azure Function, will validate the move (*Step 9*) and, in case it is valid, will store it in the Game State (*step 10*). Lastly, after the Azure Function execution, the game will check the movement validation, and in case it was an invalid one, it will return to *step 7*.

### Unity Game
All the Game Flow logic is implemented in the [Game][game-class-file] file. The main logic is contained in the [StartMatchLoop][game-class-file-start-match-loop] method, which is implemented in this way:

1. ***Start Match***: the first thing we do is calling the [StartMatch][game-class-file-start-match] method, which uses the [StartMatch][match-handler-start-match] method from the [MatchHandler][match-handler]. All these methods allow us to execute the [StartMatch][start-match-azf] Azure Function, which initializes the *Game State*.
1. ***Loop***: the second part of the *StartMatchLoop* consists in a *while* loop where we run the next actions:
    1. ***Get Game Status***: here we call the [GetGameStatus][game-class-file-get-game-status] method, which uses the [Get][game-status-handler-get] method from the [GameStatusHandler][game-status-handler] for retrieving the current *Game State* information, which will be used in further steps. Here we also call the [CheckForWinner][game-class-file-check-for-winner] method, which uses the retrieved *Game State* information for checking if the latest Player move is and End Game move or not. In case it is, the game will conclude.
    1. ***Make Move***: after checking if it's the current player's turn (check it [here][game-class-file-my-turn]), we run the [MakeMove][game-class-make-move] method, which request the Player to perform a move in the Tic-Tac-Toe Board and sends it to the [MakeMultiplayerMove][make-multiplayer-move-azf] Azure Function for storing it in the *Game State*. For this, it uses the [MakeMove][game-status-handler-make-move] method from the [GameStatusHandler][game-status-handler] class.


### Azure Function: Start Match
The [StartMatch][start-match-azf] Azure Function initializes the *Game Status* data (a representation of the Tic-Tac-Toe Board, with the respective players moves), and stores it in a PlayFab's *Shared Group*. This is an important step in our flow as this data (and the related *Shared Group*) will be used all along the current Game (i.e.: if the Game is concluded or cancelled, the data will be deleted), allowing us to share the different moves and states between both players.

This is the current implementation of the [StartMatch][start-match-azf] Azure Function:

```csharp
[FunctionName("StartMatch")]
public static async Task<GameState> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
{
    var context = await FunctionContext<StartMatchRequest>.Create(req);
    // (1)
    return await GameStateUtil.InitializeAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId);
}
```

In the previous Azure Function we've used the next method:

`(1)` In the [InitializeAsync][game-state-util-initialize] method from the [GameStateUtil][game-state-util] we perform all the necessary tasks for creating and storing the *Game State* in a *Shared Group*.

### Azure Function: Get Game Status
The [GetGameStatus][get-game-status-azf] Azure Function allow us to get the current game's state from an specific *Shared Group*. For doing so, it's necessary to provide the *Shared Group ID* (which is a property from the [GetGameStatusRequest][get-game-status-request] request class).

This is the current implementation:

```csharp
[FunctionName("GetGameStatus")]
public static async Task<GameState> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
    [CosmosDB(ConnectionStringSetting = "<your-cosmos-db-connection-string>")] DocumentClient cosmosDBClient
)
{
    var context = await FunctionContext<GetGameStatusRequest>.Create(req);
    // (1)
    return await GameStateUtil.GetAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId);
}
```

In the previous Azure Function we've used the next method:

`(1)` With the [GetAsync][game-state-util-get] method from the [GameStateUtil][game-state-util] utility class we are able to retrieve the *Game State* from an specified *Shared Group*.

### Azure Function: Make Multiplayer Move
After a player has made a move, the [MakeMultiplayerMove][make-multiplayer-move-azf] Azure Functions is executed to determine if that movement is valid or not and, in case it is valid, update the current *Game State*.

It's implemented in the next way:

```csharp
[FunctionName("MakeMultiplayerMove")]
public static async Task<MakeMultiplayerMoveResponse> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req
)
{
    var context = await FunctionContext<MakeMultiplayerMoveRequest>.Create(req);
    var playerMoveRequest = context.FunctionArgument;

    // (1)
    var updatedData = await GameStateUtil.MoveUpdateAsync(context.AuthenticationContext, playerMoveRequest.PlayerId, playerMoveRequest.PlayerMove, playerMoveRequest.SharedGroupId);

    // (2)
    return new MakeMultiplayerMoveResponse
    {
        GameState = updatedData,
        PlayerMoveResult = new MakePlayerMoveResponse
        {
            Valid = updatedData != null
        }
    };
}
```

Some notes about the previous implementation:

`(1)` Both the validation and update of the current *Game State* is performed in the [MoveUpdateAsync][game-state-util-move-update] method from the [GameStateUtil][game-state-util] utility class.

`(2)` The [MakeMultiplayerMoveResponse][make-multiplayer-move-response] contains both the *Game State* and the *validation result* ([PlayerMoveResult][make-multiplayer-move-response-player-move-result] property).

<!-- IMAGES -->
[architecture-01]: ./document-assets/high-level-architecture.png

[game-flow-diagram-01]: ./document-assets/images/diagrams/game-flow-diagram-01.png

<!-- READMEs -->
[playfab-config-readme]: ./TicTacToe/README.md
[azure-function-config-readme]: ./AzureFunctions/README.md
[cosmos-db-config-readme]: ./AzureFunctions/cosmos-db-configuration.md


<!-- Azure Functions -->
[start-match-azf]: ./AzureFunctions/TicTacToeFunctions/Functions/StartMatch.cs
[get-game-status-azf]: ./AzureFunctions/TicTacToeFunctions/Functions/GetGameStatus.cs
[make-multiplayer-move-azf]: ./AzureFunctions/TicTacToeFunctions/Functions/MakeMultiplayerMove.cs


<!-- Game Class-->
[game-class-file]: ./TicTacToe/Assets/Scripts/Game.cs
[game-class-file-start-match-loop]: ./TicTacToe/Assets/Scripts/Game.cs#L116
[game-class-file-start-match]: ./TicTacToe/Assets/Scripts/Game.cs#L141
[game-class-file-get-game-status]: ./TicTacToe/Assets/Scripts/Game.cs#L90
[game-class-file-my-turn]: ./TicTacToe/Assets/Scripts/Game.cs#L192
[game-class-make-move]: ./TicTacToe/Assets/Scripts/Game.cs#L149
[game-class-file-check-for-winner]: ./TicTacToe/Assets/Scripts/Game.cs#L206


<!-- Handlers -->
[match-handler]: ./TicTacToe/Assets/Scripts/Handlers/MatchHandler.cs
[match-handler-start-match]: ./TicTacToe/Assets/Scripts/Handlers/MatchHandler.cs#L24

[game-status-handler]: ./TicTacToe/Assets/Scripts/Handlers/GameStatusHandler.cs
[game-status-handler-get]: ./TicTacToe/Assets/Scripts/Handlers/GameStatusHandler.cs#L25
[game-status-handler-make-move]: ./TicTacToe/Assets/Scripts/Handlers/GameStatusHandler.cs#L46


<!-- Utils -->
[game-state-util]: ./AzureFunctions/TicTacToeFunctions/Util/GameStateUtil.cs
[game-state-util-initialize]: ./AzureFunctions/TicTacToeFunctions/Util/GameStateUtil.cs#L32
[game-state-util-get]: ./AzureFunctions/TicTacToeFunctions/Util/GameStateUtil.cs#L12
[game-state-util-move-update]: ./AzureFunctions/TicTacToeFunctions/Util/GameStateUtil.cs#L18

<!-- AZF Request and Responses -->
[get-game-status-request]: ./AzureFunctions/TicTacToeFunctions/Models/Requests/GetGameStatusRequest.cs
[make-multiplayer-move-response]: ./AzureFunctions/TicTacToeFunctions/Models/Responses/MakeMultiplayerMoveResponse.cs
[make-multiplayer-move-response-player-move-result]: ./AzureFunctions/TicTacToeFunctions/Models/Responses/MakeMultiplayerMoveResponse.cs#L9


<!-- Index -->
[summary]: #summary
[architecture]: #architecture
[pre-requisites]: #pre-requisites
[implementation]: #implementation
[brief-diagram-description]: #brief-diagram-description
[unity-game]: #unity-game
[azure-function-start-match]: #azure-function-start-match
[azure-function-get-game-status]: #azure-function-get-game-status
[azure-function-make-multiplayer-move]: #azure-function-make-multiplayer-move
