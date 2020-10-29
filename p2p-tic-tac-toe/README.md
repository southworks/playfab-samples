# PlayFab - P2P Multiplayer Tic-Tac-Toe

## Index

- [Summary][index-summary]
- [Turning the base sample into a multiplayer game][index-turning-the-base-sample-into-a-multiplayer-game]
- [Automatic matchmaking][index-automatic-matchmaking]
- [Support Match Lobby][index-support-match-lobby]

## Summary

This sample demonstrates how to configure a multiplayer Tic-Tac-Toe game using [Unity][unity-main-page], [PlayFab][playfab-main-page], and [Azure Services][azure-main-page].

This project also has a set of specifics instructions on how to implement these features:

- How to support Match Lobbies in a multiplayer game.
- How to turn a single player game into multiplayer.
- How to use [PlayFab Party][playfab-party-docs] to manage communication between players.

This sample is based on the [Tic-Tac-Toe PlayFab sample][tic-tac-toe-base-sample], which consists on a tic-tac-toe game where the player plays against an AI, and shows how to utilize PlayFab's Cloud Script Azure integration.

## Turning the base sample into a multiplayer game

Since the base sample had only one human player, it was able to store all the game data on PlayFab Player’s Custom Data.

---

![Original Move flow][original-make-move-flow]

---

To support a multiplayer scenario, first we had to implement a way for players to find each other. To allow these we added the [Quick Match][index-automatic-matchmaking] and the [Match Lobby][index-support-match-lobby] features.

With this solved, we decided to use [PlayFab Party][playfab-party-docs] to manage the communication between the players. **PlayFab Party** adds low-latency chat and data communication to your game in a way that's flexible, inclusive, and secured. Party is ideal for multiplayer implementations where a cloud-hosted dedicated server is not desired.

In this scenario matched players will have two possible roles:

- The **Host**. The player responsible of creating the [PlayFab Party network][playfab-party-network-docs].
  - Also referred as Player 1 (P1).
- The **Guest**. The player that joins the PlayFab Party network.
  - Also referred as Player 2 (P2).

---

![New Move Flow][new-make-move-flow]

---

Once players are connected in the same network, they will be able to communicate to each other directly using custom [data messages][playfab-party-data-messages-docs].

This will allow them to share when a movement was made or if the other player left before the match was over.

For more detailed information regarding on how the game flow works, you can check the [Game Flow documentation][game-flow-documentation].

## Automatic matchmaking

Now that the game is ready to receive two players, we had to implement a way to match players together and start a match.

Our automatic matchmaking (*Quick Match*) takes advantage of [PlayFab's Matchmaking service][playfab-matchmaking-doc], and its flow works like this:

- Player 1 issues a matchmaking ticket to PlayFab Matchmaking service.
  - P1 keeps polling the ticket state until finding a match.
- Player 2 issues a matchmaking ticket to PlayFab Matchmaking service.
  - P2 keeps polling the ticket state until finding a match.
- PlayFab matches P1 and P2 together. This matching is done randomly by the PlayFab services.
- P1 creates the PlayFab Party network that will connect both players.
- P1 and P2 issues another matchmaking ticket to queue that has a [string equality rule][playfab-mm-queue-configurations-types].
  - P1 injects the NetworkId to the its ticket so P2 can use it to join.
  - The second queue will match players that have an attribute with the same ID (the previous Match ID).
- Once matched again P2 joins to the network.
- P1 gets notified that P2 has joined, and it sends an event to notify that both players are ready to play.
- Finally, this notification triggers the start of the game for both players.

For more detailed information regarding on how the Automatic matchmaking works, you can check the [Quick Match document][quick-play-document].

## Support Match Lobby

Once the game is ready to receive the players, we can add the possibility to create, list and join to match lobbies, allowing known players to manually match to play together.

The Match Lobby management involves four layers, the Unity game, PlayFab service, Azure Function, and a Cosmos DB.

---

![Match Lobby architecture][match-lobby-high-level-architecture]

---

On our scenario, match lobbies will be linked to a PlayFab Party Network.

The major limitation here is that PlayFab does not have (yet) a way to list entities like Groups or Networks, so we had to add an extra step for this: storing match lobbies in an non-PlayFab storage.

In this sample, every time a player creates a match lobby, it will add the match lobby data into a Cosmos DB storage. Then, other players will be able to search through a list of available lobbies and select one to join to.

---

<p align="center">
  <img src="./document-assets/match-lobby-list-preview.png" />
</p>

---

The player that created the match lobby will also create the Network.
The player joining the match lobby will be also joining to the Network.

The host player will be able to:

- Start the game.
- Kick players.
- Lock the match lobby.
  - Players will be only available to join if they know the invitation code.
- Close the match lobby.

Match lobby data will be deleted from Cosmos DB as soon as the match starts.
Also, if a third player tries to join to a lobby that is full, it will be prompted with an error stating that the lobby is full or it doesn't exists anymore.

For more detailed information regarding the Match Lobbies feature, you can check the following documents:

- [Create a Match Lobby][create-match-lobby]
- [Search a Match Lobby][search-match-lobby]
- [Join to a match lobby][join-match-lobby]

<!-- URLS -->
[azure-main-page]: https://azure.microsoft.com/
[playfab-main-page]: https://playfab.com/
[playfab-matchmaking-doc]: https://docs.microsoft.com/gaming/playfab/features/multiplayer/matchmaking/
[playfab-mm-queue-configurations-types]: https://docs.microsoft.com/gaming/playfab/features/multiplayer/matchmaking/config-queues#standard-rule-types
[playfab-party-data-messages-docs]:https://docs.microsoft.com/gaming/playfab/features/multiplayer/networking/party-unity-plugin-quickstart#sending-and-receiving-data-messages
[playfab-party-docs]: https://docs.microsoft.com/gaming/playfab/features/multiplayer/networking/
[playfab-party-network-docs]: https://docs.microsoft.com/gaming/playfab/features/multiplayer/networking/concepts-objects#network
[tic-tac-toe-base-sample]: https://github.com/PlayFab/PlayFab-Samples/tree/master/Samples/Unity/TicTacToe
[unity-main-page]: https://unity.com/

<!-- Internal documents -->
[create-match-lobby]: ./create-match-lobby.md
[join-match-lobby]: ./join-to-the-match-lobby.md
[search-match-lobby]: ./search-match-lobby.md
[quick-play-document]: ./quick-match.md
[game-flow-documentation]: ./game-flow.md

<!-- IMAGES -->
[match-lobby-high-level-architecture]: ./document-assets/high-level-architecture.png
[new-make-move-flow]: ./document-assets/new-make-move-flow.png
[original-make-move-flow]: ./document-assets/original-make-move-flow.png

<!-- Index -->
[index-summary]: #summary
[index-turning-the-base-sample-into-a-multiplayer-game]: #turning-the-base-sample-into-a-multiplayer-game
[index-automatic-matchmaking]: #automatic-matchmaking
[index-support-match-lobby]: #support-match-lobby
