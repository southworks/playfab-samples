# Join to the Match Lobby

## Index

- [Summary][summary]
- [Prerequisites][prerequisites]
- [Architecture][architecture]
- [Implementation][implementation]
  - [Unity Game: Starts the connection process][unity-game-starts-the-connection-process]
  - [Azure Function App: JoinMatchLobby function][azure-function-app-joinmatchlobby-function]

## Summary

This sample demonstrates how to implement the Join Match Lobby feature. It allows a player to connect to an existing Match Lobby in order to play against another player.

## Prerequisites

Before configuring this project, first ensure the following prerequisites have been completed:

- Read and complete the [PlayFab configuration][playfab-config-readme].
- Read and complete the [Azure Function configuration][azure-function-config-readme].
- Read and complete the [Cosmos DB configuration][cosmos-db-config-readme].
- Read the [Search Match Lobby][search-match-lobby-readme] implementation guide.

## Architecture

Before starting explaining how this feature works, lets see how the game was implemented:

---

![High Level Architecture](./document-assets/high-level-architecture.png)

---

## Implementation

The implementation of the Match Lobby creation feature consists of the following steps:

---

![Join Match Diagram](./document-assets/images/diagrams/join-diagram.png)

---

### Unity Game: Starts the connection process

The Unity Game is the first layer involved, once the game has [retrieved the Lobbies list][search-match-lobby-readme] the player clicks the `Join` button of the desired Match Lobby.

---

<p align="center">
  <img src="./document-assets/images/match-lobby-result.png" />
</p>

---

This button executes the [`JoinMatchLobby`][match-lobby-handler] method of the `MatchLobbyHandler` that triggers the [`JoinMatchLobby`][join-match-lobby] Azure Function using the [PlayFab SDK][playfab-sdk].

### Azure Function App: JoinMatchLobby function

The Azure Function [`JoinMatchLobby`][join-match-lobby] is responsible for:

- [Retrieving][retrieving-the-shared-group-data] the Shared Group Data by its identifier.
- [Adding][add-member-to-shared-group-data] the P2 as member of the shared group.
- [Updating][update-the-shared-group-data] the Shared Group Data:
  - Updates the current availability of  the `MatchLobby` property to zero.
  - Sets the current player ID as the `PlayerTwoId` of the `Match` property.
- [Updating][insert-match-lobby-into-cosmos-db] the new `MatchLobby` in Cosmos DB.

To add P2 as a member of the Shared Group the [function sends a request][add-member-to-shared-group-data] to the [`/Server/AddSharedGroupMembers`][add-shared-group-members-endpoint] endpoint through the PlayFab's SDK.

> NOTE: The function must use the `MasterPlayerAccountId` - which is [retrieved from its context][retrieve-the-master-player-account-id-from-the-function-context] - as P2 identifier.

To update the Shared Group Data the [function sends a request][update-the-shared-group-data] to the [`/Server/UpdateSharedGroupData`][update-shared-group-data-endpoint] endpoint through the PlayFab's SDK.

Next, the current availability of the `MatchLobby` is decreased and its new state is [updated in Cosmos DB][insert-match-lobby-into-cosmos-db].

Finally, the Shared Group Data is returned to the Game which starts the [Start Match][start-match-readme] process.

<!-- Index Links -->
[summary]: #summary
[prerequisites]: #prerequisites
[architecture]: #architecture
[implementation]: #implementation
[unity-game-starts-the-connection-process]: #unity-game-starts-the-connection-process
[azure-function-app-joinmatchlobby-function]: #azure-function-app-joinmatchlobby-function

<!-- READMEs -->
[search-match-lobby-readme]: ./search-match-lobby.md
[playfab-config-readme]: ./TicTacToe/README.md
[azure-function-config-readme]: ./AzureFunctions/README.md
[cosmos-db-config-readme]: ./AzureFunctions/cosmos-db-configuration.md
[start-match-readme]: ./start-match.md

<!-- AZURE FUNCTIONS -->
[join-match-lobby]: ./AzureFunctions/TicTacToeFunctions/Functions/JoinMatchLobby.cs
[retrieving-the-shared-group-data]: ./AzureFunctions/TicTacToeFunctions/Util/SharedGroupDataUtil.cs#L66
[add-member-to-shared-group-data]: ./AzureFunctions/TicTacToeFunctions/Util/SharedGroupDataUtil.cs#L28
[update-the-shared-group-data]: ./AzureFunctions/TicTacToeFunctions/Util/SharedGroupDataUtil.cs#L41
[insert-match-lobby-into-cosmos-db]: ./AzureFunctions/TicTacToeFunctions/Util/MatchlobbyUtil.cs#L34
[retrieve-the-master-player-account-id-from-the-function-context]: ./AzureFunctions/TicTacToeFunctions/Functions/JoinMatchLobby.cs#L28

<!-- Game -->
[match-lobby-handler]: ./TicTacToe/Assets/Scripts/Handlers/MatchlobbyHandler.cs#L21

<!-- PlayFab References -->
[playfab-sdk]: https://github.com/PlayFab/CSharpSDK
[add-shared-group-members-endpoint]: https://docs.microsoft.com/rest/api/playfab/server/shared-group-data/addsharedgroupmembers?view=playfab-rest
[update-shared-group-data-endpoint]: https://docs.microsoft.com/rest/api/playfab/server/shared-group-data/updatesharedgroupdata?view=playfab-rest
