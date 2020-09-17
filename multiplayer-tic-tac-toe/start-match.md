# PlayFab - Multiplayer Tic-Tac-Toe - Start Match flow

## Index

- [Summary](#summary)
- [Implementation](#implementation)
  - [Create match](#create-match)
  - [Polling process](#polling-process)
  - [Polling result](#polling-result)

## Summary

This document explains how the Start Match flow was designed.

This flow is the step that links the `Lobby` scene with the `Game` scene, and makes sure players are ready to play the match.

There are 4 scenarios where players can trigger this logic:

- Player One (P1) clicking on the Quick Match button. ([here][quickmatch-sm-trigger])
- Player Two (P2) clicking on the Quick Match button. ([here][quickmatch-sm-trigger])
- P1 creating a Match Lobby. ([here][matchlobby-connect-sm-trigger])
- P2 joining a Match Lobby. ([here][matchlobby-join-sm-trigger])

## Implementation

The [StartMatch method][start-match-method] expects the `shouldCreateMatch` flag.
This flag is used to state if the match (and the related shared group data) was created before calling the method.

When two players are matched after clicking on the `Quick Play` button, the game will trigger the StartMatch with the `shouldCreateMatch` flag in true.
This triggers the Match Creation using the data initialized in the `Quick Play` flow.

Then, the game will start the polling process to see if both players are ready to start.

Finally, depending on the result of the polling the game will start, or it will trigger a reset routine.

### Create match

When the start match method triggers the [Create Match flow][create-match-method], it will run this process:

- Checks if the current player is the P1 or P2.
- Defines the ID of the new shared group data with the following format: `{PlayerOneId}-{PlayerTwoId}`
- If the player is P1, it will trigger the `CreateSharedGroup` Azure Function with the ID defined before.
- If the player is P2, it will trigger the `JoinMatch` Azure Function with the ID defined before.
  - P2 will keep trying to join until a match is created or the player cancels the matchmaking.

### Polling process

The game will not start until both players have joined to the match.

Each player will keep [polling][start-match-polling] the game state until:

- **The game can start**. There is a valid shared group data instance and both players set their IDs in the game context.
  - On each loop where the player gets the match state, it will update a flag to see if the game can be started.
- **The player cancels matchmaking**.
- **The user fails to get the match data**. In this case the shared group data gets deleted in any case.

### Polling result

Finally, the game checks if the game state polling process resulted in the game being able to start or not.

If the game can start, the game state data is set in the game's context.

If not, the game context has to be restarted, and the UI gets set as if the match was cancelled manually.

<!-- Internal link -->
[create-match-method]: ./TicTacToe/Assets/Scripts/Lobby.cs#L254
[matchlobby-connect-sm-trigger]: ./TicTacToe/Assets/Scripts/Lobby.cs#L379
[matchlobby-join-sm-trigger]: ./TicTacToe/Assets/Scripts/Lobby.cs#L322
[quickmatch-sm-trigger]: ./TicTacToe/Assets/Scripts/Lobby.cs#L130
[start-match-method]: ./TicTacToe/Assets/Scripts/Lobby.cs#L219
[start-match-polling]: ./TicTacToe/Assets/Scripts/Lobby.cs#L231
