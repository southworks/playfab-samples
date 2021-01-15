# SOUTHWORKS - PlayFab Samples

This repository contains complete working samples that demonstrate best-practice usage patterns for [PlayFab features][playfab-website].

## Samples

| Sample | PlayFab features |
| - | - |
| [Fantasy Soccer Web Application][fantasy-soccer] | OpenID/Playfab Login, Economy, Inventory management, Automation |
| [PlayFab Azure B2C integration using custom policies][aadb2c-custom-policy-api-login] | Custom ID login/registration |
| [PlayFab Azure B2C integration using ASP.NET Core Identity][aadb2c-net-identity] | Login, MSIdentity |
| [Multiplayer Tic-Tac-Toe](#multiplayer-tic-tac-toe) | Matchmaking Queues, Shared Group, Login |
| [P2P Multiplayer Tic-Tac-Toe](#p2p-multiplayer-tic-tac-toe) | Matchmaking Queues, PlayFab Party, Login |

### Multiplayer Tic-Tac-Toe

The [Multiplayer Tic-Tac-Toe][multiplayer-tic-tac-toe] sample demonstrates how to configure a multiplayer Tic-Tac-Toe game using [Unity][unity-main-page], [PlayFab][playfab-website], [Azure Services][azure-main-page] and [Cosmos DB][cosmos-db-doc].

This project also has a set of specifics instructions on how to implement these features:

- How to turn a single player game into multiplayer.
- How to support Match Lobbies in a multiplayer game.
  - This includes an integration with [Cosmos DB][cosmos-db-doc] to support Match Lobby listing.

> **As [PlayFab's documentation][sdg-disclamer] states, Shared Group Data should not be used by groups larger than a dozen or so players, at most**.

#### PlayFab Features

- **Matchmaking Queues**. Used to match random players together.
- **Shared Group**. Used to store the game state, and for manual matchmaking through Match Lobbies.
- **Basic Login**. Used to identify each game instance as a different user.

### P2P Multiplayer Tic-Tac-Toe

The [P2P Multiplayer Tic-Tac-Toe][p2p-tic-tac-toe] sample demonstrates how to configure a multiplayer Tic-Tac-Toe game using [Unity][unity-main-page], [PlayFab][playfab-website], [Azure Services][azure-main-page] and [Cosmos DB][cosmos-db-doc].

This project also has a set of specifics instructions on how to implement these features:

- How to support Match Lobbies in a multiplayer game.
- How to turn a single player game into multiplayer.
- How to use PlayFab Party to manage communication between players.

#### PlayFab Features

- **Matchmaking Queues**. Used to match random players together.
- **PlayFab Party**. Used to manage most communications between players during the game.
- **Basic Login**. Used to identify each game instance as a different user.

## Feedback

Got ideas for samples that you would like to see developed? Let us know at [playfab-sw@southworks.com](mailto:playfab-sw@southworks.com).

## License

This repository is covered under the [MIT License](./LICENSE).

<!-- Internal links -->
[multiplayer-tic-tac-toe]: ./multiplayer-tic-tac-toe
[p2p-tic-tac-toe]: ./p2p-tic-tac-toe
[aadb2c-custom-policy-api-login]: ./aadb2c-custom-policy-api-login/
[aadb2c-net-identity]: ./playfab-login-with-b2c
[fantasy-soccer]: ./fantasy-soccer

<!-- External links -->
[azure-main-page]: https://azure.microsoft.com/
[cosmos-db-doc]: https://docs.microsoft.com/azure/cosmos-db/introduction
[playfab-website]: https://playfab.com/
[unity-main-page]: https://unity.com/
[sdg-disclamer]: https://docs.microsoft.com/gaming/playfab/features/social/groups/using-shared-group-data
