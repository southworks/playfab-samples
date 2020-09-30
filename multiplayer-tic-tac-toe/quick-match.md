# Quick Match

## Index

- [Summary](#summary)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Implementation](#implementation)
- [Player One Flow](#player-one-flow)
  - [Unity Game](#unity-game---p1)
    - [Matchmaking Ticket Creation](#matchmaking-ticket-creation---p1)
    - [Getting the Matchmaking Ticket Status](#getting-the-matchmaking-ticket-status---p1)
    - [Creating Shared Group](#creating-shared-group---p1)
  - [Azure Function App: CreateSharedGroup Function](#azure-function-app--createsharedgroup-function)
  - [Azure Function App: GetSharedGroup Function](#azure-function-app--getsharedgroup-function)
- [Player Two Flow](#player-two-flow)
  - [Unity Game](#unity-game---p2)
    - [Matchmaking Ticket Creation](#matchmaking-ticket-creation---p2)
    - [Getting the Matchmaking Ticket Status](#getting-the-matchmaking-ticket-status---p2)
    - [Joining to the Share Group](#joining-to-the-share-group---p2)
  - [Azure Function App: Join Match Function](#azure-function-app--join-match-function)

## Summary

This sample demonstrates how to implement a ***Quick Match*** using PlayFab's services. This feature will allow the players to play a Match instantly, without the need of looking for a specific Match.

## Architecture

Before starting explaining how this feature works, lets see how the game was implemented.

---

![alt-text][architecture-01]

---

Regarding the ***Quick Match*** feature, we can add the next information:

- Tic-Tac-Toe game: this is implemented in [this project][unity-game-project]. Here we've the *Quick Match* button, where players can start a match.
- Tic-Tac-Toe Function App: we've implemented this [here][azure-function-project].

## Prerequisites

Before configuring this project, first ensure the following prerequisites have been completed:

- Read and complete the [PlayFab configuration][playfab-config-readme].
- Read and complete the [Azure Function configuration][azure-function-config-readme].
- Read and complete the [Cosmos DB configuration][cosmos-db-config-readme].

## Implementation

### Player One Flow

---

![alt-text][quick-match-player-01]

---

The *P1's Quick Match* implementation is a three part process:

1. The first part consists of *Step 01 to Step 03*. Here the P1 creates a [*Matchmaking Ticket*][playfab-matchmaking-documentation] for starting a match against a random player, and then polls the PlayFab's services to get the Ticket status until a match has been assigned.
1. The second part includes *Steps 04 to 09*. The *Tic-Tac-Toe* game, using the [*CreateSharedGroup*][azf-create-shared-group] Azure Function, creates a [Shared Group][playfab-shared-group-documentation] using the PlayFab's Services, in order to have a storage where the Game's Data will be stored later. After creating it, the Azure Function returns the *Shared Group Data* to the Tic-Tac-Toe game for using it in further steps.
1. The last part consists in the *Step 10*, where the *Tic-Tac-Toe* game starts the [Start Match process][start-match-flow].

#### Unity Game - P1

##### Matchmaking Ticket Creation - P1

The process starts when the P1 press the `Quick match` button.

---

<p align="center">
  <img src="./document-assets/images/search-match-lobby-02.png" />
</p>

---

Then, we use the [Matchmaking Handler][ttt-matchmaking-handler] for creating a [Matchmaking Ticket][playfab-matchmaking-documentation].

In this step, we use the PlayFab's API for creating the Matchmaking Ticket ([link][playfab-api-create-matchmaking-ticket]), setting all the necessary request's properties using the *QueueConfiguration* object we set when creating the current MatchmakingHandler class (check this [here][ttt-lobby-file-create-matchmaking-handler]).

##### Getting the Matchmaking Ticket Status - P1

In the next step, we poll the ticket status using the [EnsureGetMatchmakingTicketStatus][ttt-matchmaking-handler-ensure-get-matchmaking-ticket-status] method from the [MatchmakingHandler][ttt-matchmaking-handler] class. This method uses the [GetMatchmakingTicketStatus][ttt-matchmaking-handler-get-matchmaking-ticket-status] method from the same class, where we call the [GetMatchmakingTicket][playfab-api-get-matchmaking-ticket-status] PlayFab API's method for getting the Ticket's information, where the *Ticket Status* is included.

Something important to mention here is that the [GetMatchmakingTicket][playfab-api-get-matchmaking-ticket-status] API's method has a *time-polling constraint* of 10 polls per minute (i.e., *one every 6 seconds*). Due to this, we have added [this condition][ttt-matchmaking-handler-ensure-get-matchmaking-ticket-status-constraint] in the [EnsureGetMatchmakingTicketStatus][ttt-matchmaking-handler-ensure-get-matchmaking-ticket-status] method. You can read more about this [here][playfab-api-get-status-notes].

##### Creating Shared Group - P1

Once we've ensured there is a match, the Tic-Tac-Toe game determines if we're the first player to play (P1) using [this method][ttt-lobby-file-is-first-player-to-play]. In case it's true, we'll be in charge of creating the Shared Group the game will be using later for storing its information. For doing so, we'll be using the [Create][shared-group-handler-create] method from the [Shared Group Handler][shared-group-handler] class. This method allows us to perform a request to the [CreateSharedGroup][azf-create-shared-group] Azure Function, which purpose is explained [here][azure-function-app-createsharedgroup-function].

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

One of the Azure Functions we use in this process is the [CreateSharedGroup][azf-create-shared-group] Azure Function, which allows us to create a Shared Group in the PlayFab's service, add the current player as a Shared Group member, and update and return the Shared Group Data.

This is the implementation of the [CreateSharedGroup][azf-create-shared-group] Azure Function:

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

The other function we're using in this process is the [GetSharedGroup][azf-get-shared-group] Azure Function, which allows us to retrieve the data stored in the Shared Group instance.

This is the implementation of the [GetSharedGroup][azf-get-shared-group] Azure Function:

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

The *P2 Quick Match* follows a three part process:

1. The first part consists in the *Step 01 to Step 03*. Here, P2 creates a [*Matchmaking Ticket*][playfab-matchmaking-documentation] for starting a match against a random player, and then polls the PlayFab's services to get the Ticket's status, until a match has been assigned.
1. The second part includes the *Steps 04 to 07*. In this part, the *Tic-Tac-Toe* game triggers the [*JoinMatch*][azf-join-match] Azure Function to add the player to the current [match][azf-model-Match] as P2, and returns an updated [TicTacToeSharedGroupData][azf-model-ttt-shared-group-data] object for use in further steps.
1. The last part consists in the *Step 08*, where the *Tic-Tac-Toe* game starts the [Start Match process][start-match-flow].

#### Unity Game - P2

##### Matchmaking Ticket Creation - P2

This is implemented in the same way as the P1 process. You can check it [here][matchmaking-ticket-creation---p1]

##### Getting the Matchmaking Ticket Status - P2

This is implemented in the same way as for the P1 process. You can check it [here][getting-the-matchmaking-ticket-status---p1].

##### Joining to the Share Group - P2

The next step consists in joining into an existing Match. For doing this, we are using the [JoinMatch][match-handler-join-match] method from the [MatchHandler][match-handler] class. This allows us to perform request calls to the [JoinMatch][azf-join-match] Azure Function (which we'll explain later).

#### Azure Function App: Join Match Function

With [this][azf-join-match] function we are able to join the P2 to an existing Shared Group.

This is the implementation of the [JoinMatch][azf-join-match] Azure Function:

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

<!-- READMEs -->
[playfab-config-readme]: ./TicTacToe/README.md
[azure-function-config-readme]: ./AzureFunctions/README.md
[cosmos-db-config-readme]: ./AzureFunctions/cosmos-db-configuration.md
[start-match-flow]: ./start-match.md

<!-- IMAGES -->
[architecture-01]: ./document-assets/high-level-architecture.png

[quick-match-player-01]: ./document-assets/images/diagrams/quick-match-diagram-01.png
[quick-match-player-02]: ./document-assets/images/diagrams/quick-match-diagram-02.png

<!-- Projects -->
[unity-game-project]: ./TicTacToe/
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
[prerequisites]: #prerequisites
[implementation]: #implementation
[player-one-flow]: #player-one-flow
[unity-game---p1]: #unity-game---p1
[matchmaking-ticket-creation---p1]: #matchmaking-ticket-creation---p1
[getting-the-matchmaking-ticket-status---p1]: #getting-the-matchmaking-ticket-status---p1
[azure-function-app-createsharedgroup-function]: #azure-function-app-createSharedGroup-function
[azure-function-app-getsharedgroup-function]: #azure-function-app-getsharedgroup-function
[player-two-flow]: #player-two-flow
[unity-game---p2]: #unity-game---p2
[matchmaking-ticket-creation---p2]: #matchmaking-ticket-creation---p2
[getting-the-matchmaking-ticket-status---p2]: #getting-the-matchmaking-ticket-status---p2
[joining-to-the-share-group---p2]: #joining-to-the-share-group---p2
[azure-function-app--join-match-function]: #azure-function-app--join-match-function
