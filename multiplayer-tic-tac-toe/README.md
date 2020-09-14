# PlayFab - Multiplayer Tic-Tac-Toe

## Index

- [Summary](#summary)
- [Turning the base sample into a multiplayer game](#turning-the-base-sample-into-a-multiplayer-game)
  - [Automatic matchmaking](#automatic-matchmaking)
  - [Game flow and matchmaking detailed documentation](#game-flow-and-matchmaking-detailed-documentation)
- [Support Match Lobby](#support-match-lobby)
  - [Match lobby feature detailed documentation](#match-lobby-feature-detailed-documentation)

## Summary

This sample demonstrates how to configure a multiplayer Tic-Tac-Toe game using [Unity][unity-main-page], [PlayFab][playfab-main-page], and [Azure Services][azure-main-page].

This project also has a set of specifics instructions on how to implement these features:

- How to turn a single player game into multiplayer.
- How to support Match Lobbies in a multiplayer game.

This sample used [Tic-Tac-Toe PlayFab sample][tic-tac-toe-base-sample] as base, which consists on a tic-tac-toe game where the player plays against an AI, and shows how to utilize PlayFab's Cloud Script Azure integration.

## Turning the base sample into a multiplayer game

Since the base sample had only one human player, it was able to store all the game data on PlayFab Player’s Custom Data.

---

![Original Move flow][original-make-move-flow]

---

To support a multiplayer scenario, we need to change where the game state will be stored so all the players could access it at any time.

Players will now have all their data in a PlayFab Shared Group Data instance, that both players will take turns to update.

---

![New Move Flow][new-make-move-flow]

---

When the player is on its turn, he will be able to make a move, but when it is not his turn, he will keep polling the game state until it says it is.

Every time the player gets the game state, the game validates if there is a winner and has to end the match.

When a player makes an end game move (a move that sets a winner, or sets the match as a draw), it informs to the other player that the game has ended. When the second player gets informed that the match has ended, it will send a request to delete the shared group data.

This means that the TTL of a shared group data instance is the duration of a match.

As a double check, if the player still has the shared group data in the game’s context when leaving the match, it sends a request to delete the SGD instance.

### Automatic matchmaking

Now that the game is ready to receive two players, we had to implement a way to match players together and start a match.

Our automatic matchmaking (*Quick Match*) takes advantage of [PlayFab's Matchmaking service][playfab-matchmaking-doc], and its flow works like this:

- Player One issues a matchmaking ticket to PlayFab Matchmaking service.
  - P1 keeps polling the ticket state until finding a match.
- Player Two issues a matchmaking ticket to PlayFab Matchmaking service.
  - P2 keeps polling the ticket state until finding a match.
- PlayFab matches P1 and P2 together.
- P1 starts a flow to trigger the Share Group Data creation, using the following id: “{playerOneId}-{playerTwoId}”.
- P2 starts a flow to join to the Share Group Data.
  - If P2 fails to join to the Share Group Data, it will keep retrying in case his requests are sent before SGD is created.
- Once both players are part of the SGD, the game will start.

### Game flow and matchmaking detailed documentation

For more detailed information regarding on how multiplayer was added to the Tic-Tac-Toe game, or how the Automatic matchmaking works, you can check the following documents:

- [Game flow documentation][game-flow-documentation]
- [Quick Match documentation][quick-play-document]

## Support Match Lobby

Once the game is ready to receive the players, we can add the possibility to create, list and join to match lobbies, allowing known players to manually match to play together.

The Match Lobby management involves four layers, the Unity game, PlayFab service, Azure Function, and a Cosmos DB.

---

![Match Lobby architecture][match-lobby-high-level-architecture]

---

On our scenario, match lobbies will be linked to an *Shared Group Data* instance since they will share the same id. For example, if the lobby’s name is “Awesome lobby” the SGD id will be “Awesome lobby” too.

The major limitation here is that PlayFab does not have (yet) a way to list entities like Groups or Shared Group Data instances, so we had to add an extra step for this: storing match lobbies in an non-PlayFab storage.

In this sample, every time a player creates a match lobby, it will add the match lobby data into a Cosmos DB storage. Then, other players will be able to search through a list of available lobbies and select one to join to.

---

<p align="center">
  <img src="./document-assets/match-lobby-list-preview.png" />
</p>

---

The player that created the lobby will poll the match lobby state until it checks a player has joined.

When another player joins to a lobby, it will update the SGD with the player’s data, and the game will start automatically.

Match lobby data will be deleted from Cosmos DB as soon as the match starts. If a third player tries to create a lobby with the same name, PlayFab will not allow it, since there cannot be two shared group data instances with the same id.

### Match lobby feature detailed documentation

For more detailed information regarding the Match Lobbies feature, you can check the following documents:

- [Create a Match Lobby][create-match-lobby]
- [Search a Match Lobby][search-match-lobby]
- [Join to a match lobby][join-match-lobby]

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
