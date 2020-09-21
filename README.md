# SOUTHWORKS - PlayFab Samples

This repository contains complete working samples that demonstrate best-practice usage patterns for [PlayFab features][playfab-website].

## Samples

| Sample | PlayFab features |
| - | - |
|[Multiplayer Tic-Tac-Toe](#multiplayer-tic-tac-toe) | Matchmaking Queues, Shared Group, Login |

### Multiplayer Tic-Tac-Toe

The [Multiplayer Tic-Tac-Toe][multiplayer-tic-tac-toe] sample demonstrates how to configure a multiplayer Tic-Tac-Toe game using [Unity][unity-main-page], [PlayFab][playfab-website], [Azure Services][azure-main-page] and [Cosmos DB][cosmos-db-doc].

This project also has a set of specifics instructions on how to implement these features:

- How to turn a single player game into multiplayer.
- How to support Match Lobbies in a multiplayer game.
  - This includes an integration with [Cosmos DB][cosmos-db-doc] to support Match Lobby listing.

#### PlayFab Features

- **Matchmaking Queues**. Used to match random players together.
- **Shared Group**. Used to store the game state, and for manual matchmaking through Match Lobbies.
- **Basic Login**. Used to identify each game instance as a different user.

## Feedback

Got ideas for samples that you would like to see developed? Let us know at [playfab-sw@southworks.com](mailto:playfab-sw@southworks.com).

## License

This repository is covered under the [MIT License](./LICENSE).

<!-- Internal links -->
[multiplayer-tic-tac-toe]: ./multiplayer-tic-tac-toe

<!-- External links -->
[azure-main-page]: https://azure.microsoft.com/
[cosmos-db-doc]: https://docs.microsoft.com/azure/cosmos-db/introduction
[playfab-website]: https://playfab.com/
[unity-main-page]: https://unity.com/
