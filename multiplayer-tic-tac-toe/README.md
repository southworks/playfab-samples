# PlayFab - Multiplayer Tic-Tac-Toe

## Index

- [Summary](#summary)
- [Turning the base sample into a multiplayer game](#turning-the-base-sample-into-a-multiplayer-game)
  - [Automatic matchmaking](#automatic-matchmaking)
  - [Game flow and matchmaking detailed documentation](#game-flow-and-matchmaking-detailed-documentation)
- [Support Match Lobby](#support-match-lobby)
  - [Match Lobby feature detailed documentation](#match-lobby-feature-detailed-documentation)

## Summary

This sample demonstrates how to configure a multiplayer Tic-Tac-Toe game using [Unity][unity-main-page], [PlayFab][playfab-main-page], and [Azure Services][azure-main-page].

This project also has a set of specific instructions on how to implement these features:

- Turning a single player game into multiplayer.
- Supporting Match Lobbies in a multiplayer game.

This sample used [Tic-Tac-Toe PlayFab sample][tic-tac-toe-base-sample] as base. This sample consists of a tic-tac-toe game where a human player plays against an AI, and shows how to utilize PlayFab's Cloud Script Azure integration.

## Turning the base sample into a multiplayer game

Since the base sample had only one human player, it was able to store all the game data on PlayFab Player’s Custom Data.

---

![Original Move flow][original-make-move-flow]

---

To support a multiplayer scenario, it's necessary to need to change where the game state will be stored so all players can access it at any time.

Players will now have all their data in a PlayFab Shared Group Data instance, which both players will take turns to update.

---

![New Move Flow][new-make-move-flow]

---

When the player is on their turn, they will be able to make a move.
When it is not their turn, they will keep polling the game state until it says it is.

Every time the player gets the game state, the game validates if there is a winner and has to end the match.

When a player makes a move that ends the game move (a move that sets a winner, or sets the match as a draw), it informs to the other player that the game has ended. When the second player is informed that the match has ended, a request will be sent to delete the shared group data.

This means that the TTL of a shared group data instance is the duration of a match.

As a double check, if the player still has the shared group data in the game’s context when leaving the match, it sends a request to delete the Shared Group Data (SGD) instance.

### Automatic matchmaking

With two player mode implemented, we had to implement a way to match two players (P1 and P2) together.

Our automatic matchmaking (*Quick Match*) takes advantage of [PlayFab's Matchmaking service][playfab-matchmaking-doc]. The flow works like this:

- P1 issues a matchmaking ticket to PlayFab's Matchmaking feature.
  - P1 keeps polling the ticket state until finding a match.
- P2 Two issues a matchmaking ticket to PlayFab's Matchmaking feature.
  - P2 keeps polling the ticket state until finding a match.
- PlayFab matches P1 and P2 together.
- P1 starts a flow to trigger the SGD, using the following id: “{playerOneId}-{playerTwoId}”.
- P2 starts a flow to join to the SGD, using the same id as the P1: “{playerOneId}-{playerTwoId}”.
  - If P2 fails to join to the SGD, it will keep retrying in case the requests were sent before SGD is created.
- Once both players are part of the SGD, the game will start.

### Game flow and matchmaking detailed documentation

For more detailed information regarding on how multiplayer was added to the Tic-Tac-Toe game, or how the automatic matchmaking works, you can check the following documents:

- [Game flow documentation][game-flow-documentation]
- [Quick Match documentation][quick-play-document]

## Support Match Lobby

Once the game is ready to receive the players, we can add the possibility to create, list and join Match Lobbies, allowing players to manually match with each other.

Match Lobby management involves four layers: the Unity game, PlayFab service, Azure Function, and Cosmos DB.

---

![Match Lobby architecture][match-lobby-high-level-architecture]

---

In this scenario, Match Lobbies will be linked to a *Shared Group Data* instance since they will share the same ID. For example, if the lobby’s name is “Awesome lobby” the SGD ID will also be “Awesome lobby”.

The major limitation here is that PlayFab does not yet have a way to list entities like Groups or Shared Group Data instances, so we had to store Match Lobbies outside of PlayFab.

In this sample, every time a player creates a Match Lobby, it will add the Match Lobby data into a Cosmos DB storage instance. Other players will be able to search through a list of available lobbies and select one to join to.

---

<p align="center">
  <img src="./document-assets/match-lobby-list-preview.png" />
</p>

---

The player that created the Match Lobby will poll the its state until another player has joined.

When another player joins a Match Lobby, it will update the SGD with the player’s data, and the game will start automatically.

Match Lobby data will be deleted from Cosmos DB as soon as the match starts. If a third player tries to create a Match Lobby with the same name, PlayFab will not allow it, since there cannot be two Shared Group Data instances with the same ID.

### Match Lobby feature detailed documentation

For more detailed information regarding the Match Lobby feature, you can check the following documents:

- [Create a Match Lobby][create-match-lobby]
- [Search a Match Lobby][search-match-lobby]
- [Join to a Match Lobby][join-match-lobby]

<!-- URLS -->

[azure-main-page]: https://azure.microsoft.com/
[playfab-main-page]: https://playfab.com/
[playfab-matchmaking-doc]: https://docs.microsoft.com/gaming/playfab/features/multiplayer/matchmaking/
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
