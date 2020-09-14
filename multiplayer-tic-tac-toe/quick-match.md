# Quick Match

## Index

* [Summary][summary]
* [Architecture][architecture]
* [Pre Requisites][pre-requisites]
* [Implementation][implementation]
  * [Player one flow][player-one-flow]
    * [Brief Diagram Description][brief-diagram-description-player-one]
    * [Unity Game][unity-game-player-one]
      * [Matchmaking Ticket Creation][matchmaking-ticket-creation-player-one]
      * [Getting Matchmaking Ticket Status][getting-the-matchmaking-ticket-status-player-one]
      * [Creating Shared Group][creating-shared-group]
    * [Azure Function: Create Shared Group][azure-function-app-createsharedgroup-function]
    * [Azure Function: Get Shared Group][azure-function-app-getsharedgroup-function-player-one]
  * [Player two flow][player-two-flow]
    * [Brief Diagram Description][brief-diagram-description-player-two]
    * [Unity Game][unity-game-player-two]
      * [Matchmaking Ticket Creation][matchmaking-ticket-creation-player-two]
      * [Getting Matchmaking Ticket Status][getting-the-matchmaking-ticket-status-player-two]
      * [Joining to Shared Group][joining-to-the-share-group]
    * [Azure Function: Join Match][azure-function-app-join-match-function]
    * [Azure Function: Get Shared Group][azure-function-app-getsharedgroup-function-player-two]

## Summary

This sample demonstrates how to implement a ***Quick Match*** using PlayFab's services. This feature will allow the players to play a Match instantly, without the need of looking for an specific Match.

## Architecture

Before starting explaining how this feature works, lets see how the game was implemented.

---

![alt-text][architecture-01]

---

Regarding the ***Quick Match*** feature, we can add the next information:

* *Tic-Tac-Toe game*: this is implemented in [this project][unity-game-project]. Here we've the *Quick Match option*, where we can start a *Match*.
* *Tic-Tac-Toe Function App*: we've implemented this [here][azure-function-project].

## Pre-requisites

* Read and complete the [PlayFab configuration][playfab-config-readme].
* Read and complete the [Azure Function configuration][azure-function-config-readme].
* Read and complete the [Cosmos DB configuration][cosmos-db-config-readme].

## Implementation

### Player One Flow

---

![alt-text][quick-match-player-01]

---

#### Brief diagram description

The *Player One's Quick Match* implementation could be described as a three major parts process:

1. The first part consists in the *Step 01 to Step 03*. Here the *Player One* creates a [*Matchmaking Ticket*][playfab-matchmaking-documentation] for starting a *Match* against a random Player, and then polls the PlayFab's services for getting the Ticket status, until a Match has been assigned.
1. The second part includes the *Steps 04 to 09*. In this part, the *Tic-Tac-Toe* Game, using the [*CreateSharedGroup*][azf-create-shared-group] Azure Function, creates a [*Shared Group*][playfab-shared-group-documentation] using the PlayFab's Services, in order to have a storage where the Game's Data will be stored later. After creating it, the Azure Function returns the *Shared Group's Data* to the *Tic-Tac-Toe* Game for using it in further steps.
1. The last part consists in the *Step 10*, where the *Tic-Tac-Toe* Game starts the [Start Match process][start-match-flow].

#### Unity Game

##### Matchmaking Ticket Creation

The process starts when the *Player One* press the *Quick match* button.

---

<p align="center">
  <img src="./document-assets/images/search-match-lobby-02.png" />
</p>

---

Then, we use the [Matchmaking Handler][ttt-matchmaking-handler] for creating a [Matchmaking Ticket][playfab-matchmaking-documentation].

In this we use the *PlayFab's API* for creating the *Matchmatking Ticket* ([link][playfab-api-create-matchmaking-ticket]), setting all the necessary request's properties using the *QueueConfiguration* object we set when creating the current MatchmakingHandler class (check this [here][ttt-lobby-file-create-matchmaking-handler]).

##### Getting the Matchmaking Ticket Status

The next steps consists in a process where we poll the ticket status using the [EnsureGetMatchmakingTicketStatus][ttt-matchmaking-handler-ensure-get-matchmaking-ticket-status] method from the [MatchmakingHandler][ttt-matchmaking-handler] class. This method uses the [GetMatchmakingTicketStatus][ttt-matchmaking-handler-get-matchmaking-ticket-status] method from the same class, where we call the [GetMatchmakingTicket][playfab-api-get-matchmaking-ticket-status] PlayFab API's method for getting the Ticket's information, where the *Status* is included.

Something important to mention here is that the [GetMatchmakingTicket][playfab-api-get-matchmaking-ticket-status] API's method has a *time-polling constraint* of 10 polls per minute (i.e., *one every 6 seconds*). Due to this, we have added [this condition][ttt-matchmaking-handler-ensure-get-matchmaking-ticket-status-constraint] in the [EnsureGetMatchmakingTicketStatus][ttt-matchmaking-handler-ensure-get-matchmaking-ticket-status] method. You can read more about this [here][playfab-api-get-status-notes].

##### Creating Shared Group

Once we've ensured there is a Match, the Tic-Tac-Toe Game determines if we're the First player to play (Player One) using [this method][ttt-lobby-file-is-first-player-to-play]. In case it's true, we'll be in charge of creating the *Shared Group* the Game will be using later for storing its information. For doing so, we'll be using the [Create][shared-group-handler-create] method from the [Shared Group Handler][shared-group-handler] class. This method allows us to perform a request to the [CreateSharedGroup][azf-create-shared-group] Azure Function, which purpose we'll be explaining later.

This is the implementation of the [Create][shared-group-handler-create] method:

```csharp
public IEnumerator Create(string sharedGroupId)
{
    var request = GetExecuteFunctionRequest(
        "CreateSharedGroup",
        new CreateSharedGroupRequest { SharedGroupId = sharedGroupId });

    PlayFabCloudScriptAPI.ExecuteFunction(request,
        (result) =>
        {
            // handle OnSuccess code.
        },
        (error) =>
        {
            // handle OnError code.
        }
    );

    yield return WaitForExecution();
}
```

> NOTE: In this method we're using the [GetExecuteFunctionRequest][request-handler-class-get-exec-function-request] from a custom [Request Handler][request-handler-class] class for creating the request we'll be sending to our Azure Function.

#### Azure Function App: CreateSharedGroup Function

One of the Azure Functions we use in this process is the [CreateSharedGroup][azf-create-shared-group] Azure Function, which allows us to create a *Shared Group* in the PlayFab's service, add the current Player as a *Shared Group* member, and update and return the *Shared Group* data.

This is the current Function's implementation:

```csharp
[FunctionName("CreateSharedGroup")]
public static async Task<TicTacToeSharedGroupData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
{
    var context = await FunctionContext<CreateSharedGroupRequest>.Create(req);

    var playerOne = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

    // (1)
    var sgdResponse = await SharedGroupDataUtil.CreateAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId);

    // (2)
    await SharedGroupDataUtil.AddMembersAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId, new List<string> { playerOne });

    var sharedGroupData = new TicTacToeSharedGroupData
    {
        SharedGroupId = sgdResponse.SharedGroupId,
        Match = new Match { PlayerOneId = playerOne }
    };

    // (3)
    return await SharedGroupDataUtil.UpdateAsync(context.AuthenticationContext, sharedGroupData);
}
```

In this Azure Function we use the next PlayFab's API methods, which we've implemented in the [SharedGroupDataUtil][shared-group-data-util] class:

`(1)` [Create Shared Group][playfab-api-create-shared-group], implemented [here][shared-group-data-util-create].

`(2)` [Add member to Shared Group][playfab-api-add-member-to-shared-group], implemented [here][shared-group-data-util-add-member].

`(3)` [Update Shared Group][playfab-api-update-shared-group], implemented [here][shared-group-data-util-update].

#### Azure Function App: GetSharedGroup Function

The other function we're using in this process is the [GetSharedGroup][azf-get-shared-group] Azure Function, which allows us to retrieve the information stored in the *Shared Group*.

This is the current implementation:

```csharp
[FunctionName("GetSharedGroup")]
public static async Task<TicTacToeSharedGroupData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
{
    var context = await FunctionContext<GetSharedGroupRequest>.Create(req);

    // (1)
    return await SharedGroupDataUtil.GetAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId);
}
```

In this Azure Function we use the next PlayFab's API methods, which we've implemented in the [SharedGroupDataUtil][shared-group-data-util] class:

`(1)` [Get Shared Group][playfab-api-get-shared-group], implemented [here][shared-group-data-util-get].

### Player Two Flow

---

![alt-text][quick-match-player-02]

---

#### Brief diagram description

The *Player Two's Quick Match* implementation could be described as a three major parts process:

1. The first part consists in the *Step 01 to Step 03*. Here the *Player Two* creates a [*Matchmaking Ticket*][playfab-matchmaking-documentation] for starting a *Match* against a random Player, and then polls the PlayFab's services for getting the Ticket status, until a Match has been assigned.
1. The second part includes the *Steps 04 to 07*. In this part, the *Tic-Tac-Toe* Game, using the [*JoinMatch*][azf-join-match] Azure Function, adds the Player to the current [Match][azf-model-Match] as the *Player Two*, and returns an updated [TicTacToeSharedGroupData][azf-model-ttt-shared-group-data] object for using in further steps.
1. The last part consists in the *Step 08*, where the *Tic-Tac-Toe* Game starts the [Start Match process][start-match-flow].

#### Unity Game

##### Matchmaking Ticket Creation

This is implemented in the same way as for the *Player One* process. You can check it [here][matchmaking-ticket-creation]

##### Getting the Matchmaking Ticket Status

This is implemented in the same way as for the *Player One* process. You can check it [here][get-matchmaking-ticket-status].

##### Joining to the Share Group

The next step consists in joining into an existing *Match*. For doing this, we are using the [JoinMatch][match-handler-join-match] method from the [MatchHandler][match-handler] class. This allows us to perform request calls to the [JoinMatch][azf-join-match] Azure Function (which we'll explaining later).

#### Azure Function App: Join Match Function

With [this][azf-join-match] function we are able to Join the *Player Two* to an existing *Shared Group*.

```csharp
[FunctionName("JoinMatch")]
public static async Task<TicTacToeSharedGroupData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
{
    var context = await FunctionContext<JoinMatchRequest>.Create(req);
    var playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

    // (1)
    await SharedGroupDataUtil.AddMembersAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId, new List<string> { playerId });

    return // TicTacToeSharedGroupData
}
```

In this Azure Function we use the next PlayFab's API methods, which we've implemented in the [SharedGroupDataUtil][shared-group-data-util] class:

`(1)` [Add member to Shared Group][playfab-api-get-shared-group], implemented [here][shared-group-data-util-get].

#### Azure Function App: GetSharedGroup Function

The other function we're using in this process is the [GetSharedGroup][azf-get-shared-group] Azure Function, which allows us to retrieve the information stored in the *Shared Group*.

This is the current implementation:

```csharp
[FunctionName("GetSharedGroup")]
public static async Task<TicTacToeSharedGroupData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req)
{
    var context = await FunctionContext<GetSharedGroupRequest>.Create(req);

    // (1)
    return await SharedGroupDataUtil.GetAsync(context.AuthenticationContext, context.FunctionArgument.SharedGroupId);
}
```

In this Azure Function we use the next PlayFab's API methods, which we've implemented in the [SharedGroupDataUtil][shared-group-data-util] class:

`(1)` [Get Shared Group][playfab-api-get-shared-group], implemented [here][shared-group-data-util-get].

<!-- READMEs -->
[playfab-config-readme]: ./TicTacToe/README.md
[azure-function-config-readme]: ./AzureFunctions/README.md
[cosmos-db-config-readme]: ./AzureFunctions/cosmos-db-configuration.md

<!-- IMAGES -->
[architecture-01]: ./document-assets/high-level-architecture.png

[quick-match-player-01]: ./document-assets/images/diagrams/quick-match-diagram-01.png
[quick-match-player-02]: ./document-assets/images/diagrams/quick-match-diagram-02.png

<!-- Projects -->
[unity-game-project]: .
[azure-function-project]: ./AzureFunctions

[playfab-shared-group-documentation]: https://docs.microsoft.com/gaming/playfab/features/social/groups/using-shared-group-data
[playfab-matchmaking-documentation]: https://docs.microsoft.com/gaming/playfab/features/multiplayer/matchmaking/
[playfab-matchmaking-quickstart]: https://docs.microsoft.com/gaming/playfab/features/multiplayer/matchmaking/quickstart

<!-- AZURE FUNCTIONS -->
[azf-create-shared-group]: ./AzureFunctions/TicTacToeFunctions/Functions/CreateSharedGroup.cs
[azf-get-shared-group]: ./AzureFunctions/TicTacToeFunctions/Functions/GetSharedGroup.cs
[azf-join-match]: ./AzureFunctions/TicTacToeFunctions/Functions/JoinMatch.cs

[azf-model-ttt-shared-group-data]: ./AzureFunctions/TicTacToeFunctions/Models/TicTacToeSharedGroupData.cs
[azf-model-ttt-shared-group-data-match]: ./AzureFunctions/TicTacToeFunctions/Models/TicTacToeSharedGroupData.cs#L11
[azf-model-Match]: ./AzureFunctions/TicTacToeFunctions/Models/Match.cs

<!-- TicTacToe Lobby -->
[ttt-lobby-file]: ./TicTacToe/Assets/Scripts/Lobby.cs
[ttt-lobby-file-process-ticket]: ./TicTacToe/Assets/Scripts/Lobby.cs#L98
[ttt-lobby-file-create-matchmaking-handler]: ./TicTacToe/Assets/Scripts/Lobby.cs#L71
[ttt-lobby-file-is-first-player-to-play]: ./TicTacToe/Assets/Scripts/Lobby.cs#L207

<!-- Matchmaking Handler -->
[ttt-matchmaking-handler]: ./TicTacToe/Assets/Scripts/Handlers/MatchmakingHandler.cs
[ttt-matchmaking-handler-create-matchmaking-ticket]: ./TicTacToe/Assets/Scripts/Handlers/MatchmakingHandler.cs#L26
[ttt-matchmaking-handler-get-matchmaking-ticket-status]: ./TicTacToe/Assets/Scripts/Handlers/MatchmakingHandler.cs#L61
[ttt-matchmaking-handler-ensure-get-matchmaking-ticket-status]: ./TicTacToe/Assets/Scripts/Handlers/MatchmakingHandler.cs#L120
[ttt-matchmaking-handler-ensure-get-matchmaking-ticket-status-constraint]: ./TicTacToe/Assets/Scripts/Handlers/MatchmakingHandler.cs#L134

<!-- Shared Group Handler -->

[shared-group-handler]: ./TicTacToe/Assets/Scripts/Handlers/SharedGroupHandler.cs
[shared-group-handler-create]: ./TicTacToe/Assets/Scripts/Handlers/SharedGroupHandler.cs#L16

<!-- Match Handler -->

[match-handler]: ./TicTacToe/Assets/Scripts/Handlers/MatchHandler.cs
[match-handler-join-match]: ./TicTacToe/Assets/Scripts/Handlers/MatchHandler.cs#L47

<!-- Request Handler -->
[request-handler-class]: ./TicTacToe/Assets/Scripts/Handlers/RequestHandler.cs
[request-handler-class-get-exec-function-request]: ./TicTacToe/Assets/Scripts/Handlers/RequestHandler.cs#L33

<!-- PlayFab API - Matchmaking -->
[playfab-api-create-matchmaking-ticket]: https://docs.microsoft.com/rest/api/playfab/multiplayer/matchmaking/creatematchmakingticket?view=playfab-rest
[playfab-api-get-matchmaking-ticket-status]: https://docs.microsoft.com/rest/api/playfab/multiplayer/matchmaking/getmatchmakingticket?view=playfab-rest
[playfab-api-get-status-notes]: https://docs.microsoft.com/gaming/playfab/features/multiplayer/matchmaking/quickstart#check-the-status-of-the-matchmaking-ticket

<!-- PlayFab API - Shared Group -->
[playfab-api-create-shared-group]: https://docs.microsoft.com/rest/api/playfab/server/shared-group-data/createsharedgroup?view=playfab-rest
[playfab-api-get-shared-group]: https://docs.microsoft.com/rest/api/playfab/server/shared-group-data/getsharedgroupdata?view=playfab-rest
[playfab-api-update-shared-group]: https://docs.microsoft.com/rest/api/playfab/server/shared-group-data/updatesharedgroupdata?view=playfab-rest
[playfab-api-add-member-to-shared-group]: https://docs.microsoft.com/rest/api/playfab/server/shared-group-data/addsharedgroupmembers?view=playfab-rest

<!-- Shared Group - Data Util -->
[shared-group-data-util]: ./AzureFunctions/TicTacToeFunctions/Util/SharedGroupDataUtil.cs
[shared-group-data-util-create]: ./AzureFunctions/TicTacToeFunctions/Util/SharedGroupDataUtil.cs#L15
[shared-group-data-util-add-member]: ./AzureFunctions/TicTacToeFunctions/Util/SharedGroupDataUtil.cs#L28
[shared-group-data-util-update]: ./AzureFunctions/TicTacToeFunctions/Util/SharedGroupDataUtil.cs#L41
[shared-group-data-util-get]: ./AzureFunctions/TicTacToeFunctions/Util/SharedGroupDataUtil.cs#L66

<!-- PlayFab References -->

[playfab-cloudscript-azure-function-feature]: https://docs.microsoft.com/gaming/playfab/features/automation/cloudscript-af/
[playfab-cloudscript-azure-function-tutorial]: https://docs.microsoft.com/gaming/playfab/features/automation/cloudscript-af/quickstart#using-and-calling-cloudscript-using-azure-functions-from-your-playfab-title

<!-- Index Links -->
[summary]: #summary
[architecture]: #architecture
[pre-requisites]: #pre-requisites
[implementation]: #implementation
[player-one-flow]: #player-one-flow
[player-two-flow]: #player-two-flow
[matchmaking-ticket-creation]: #matchmaking-ticket-creation
[get-matchmaking-ticket-status]: #getting-the-matchmaking-ticket-status
[brief-diagram-description-player-one]: #brief-diagram-description
[brief-diagram-description-player-two]: #brief-diagram-description-1
[unity-game-player-one]: #unity-game
[unity-game-player-two]: #unity-game-1
[matchmaking-ticket-creation-player-one]: #matchmaking-ticket-creation
[matchmaking-ticket-creation-player-two]: #matchmaking-ticket-creation-1
[getting-the-matchmaking-ticket-status-player-one]: #getting-the-matchmaking-ticket-status
[getting-the-matchmaking-ticket-status-player-two]: #getting-the-matchmaking-ticket-status-1
[creating-shared-group]: #creating-shared-group
[azure-function-app-createsharedgroup-function]: #azure-function-app-createSharedGroup-function
[azure-function-app-getsharedgroup-function-player-one]: #azure-function-app-getsharedgroup-function
[joining-to-the-share-group]: #joining-to-the-share-group
[azure-function-app-join-match-function]: #azure-function-app-join-match-function
[azure-function-app-getsharedgroup-function-player-two]: #azure-function-app-getsharedgroup-function-1
