# Tic-Tac-Toe Azure Functions

## Index
* [Description][description]
    * [Azure Functions list][az-function-list]
* [Configuration][configuration]
    * [Pre-requisites][pre-requisites]
    * [Configuration steps][configuration-steps]
        * [Retrieve Azure Functions URLs][get-az-function-urls]
* [Technologies][technologies]

## Description
This project has a set of [Azure Functions][azure-functions-main-page] used for managing the *Tic-Tac-Toe* sample's logic.

### Azure Function List
These are the current functions and their responsibilities:

| Function | Responsibility | Link |
| --- | --- | --- |
| *CreateMatchLobby* | Allows us to create a *Match Lobby*, where two players will face each other. | [Link][CreateMatchLobby-AZF-source-code] |
| *CreateSharedGroup* | Creates a PlayFab's *Shared Group*, where the Match information will be stored. | [Link][CreateSharedGroup-AZF-source-code] |
| *DeleteMatchLobby* | Allows us to delete an existing *Match Lobby* from Cosmos DB. | [Link][DeleteMatchLobby-AZF-source-code] |
| *DeleteSharedGroup* | Deletes an existing *Shared Group* | [Link][DeleteSharedGroup-AZF-Source-code] |
| *GetGameStatus* | Returns the game's status. | [Link][GetGameStatus-AZF-source-code] |
| *GetSharedGroup* | Returns the information stored in an existing *Shared Group*, given its Id. | [Link][GetSharedGroup-AZF-source-code] |
| *JoinMatch* | Adds a player into an existing Match. This is used when playing a *Quick match*. | [Link][JoinMatch-AZF-source-code] |
| *JoinMatchLobby* | Adds an player to an specific *Match Lobby*. | [Link][JoinMatchLobby-AZF-source-code] |
| *MakeMultiplayerMove* | Process game's moves in a multiplayer game. | [Link][MakeMultiplayerMove-AZF-source-code] |
| *SearchMatchLobbies* | Performs a *Match Lobby* search, based in their names and Ids. | [Link][SearchMatchlobbies-AZF-source-code] |
| *SetGameWinner* | Sets a player as the winner of a match. | [Link][SetGameWinner-AZF-source-code] |
| *StartMatch* | Starts a multiplayer match. | [Link][StartMatch-AZF-source-code] |

## Configuration
### Pre-requisites
Before configuring this project, first ensure you have completed the next pre-requisites:
* Clone this project in your computer (`git clone https://github.com/southworks/playfab.git`).
* Create an [Azure account][azure-account], or [sign-in][azure-sign-in-page] into your current account.
* Create a [PlayFab account][playfab-account-create] ([tutorial][playfab-account-create-tutorial]), or [sing-in][playfab-account-login] into your current account.
* Create a new [PlayFab Title][playfab-title-create-tutorial] in your [PlayFab account][playfab-account-login].
* Download [Visual Studio][visual-studio-download] or [Visual Studio Code][visual-studio-code-download].
* Read and complete the [Azure Cosmos DB configuration][cosmos-db-readme].

### Configuration steps
Follow up the next steps for configuring this project to run in an *Azure environment*:
1. First, retrieve the next information from your *PlayFab Title*, as we'll be using them later:
    1. Retrieve your [PlayFab's title ID][playfab-title-get-title-id].
    1. Retrieve your [Title's developer secret key][playfab-title-get-developer-secret-key].
1. Create an *Azure Function app* in your [Azure personal account][azure-sign-in-page] following [this guide][azure-function-app-create-portal].
1. Add the next *application settings* in your function app (you can follow up [this guide][azure-function-app-settings]):
    1. **PLAYFAB_TITLE_ID**: configure it with your [PlayFab's title ID][playfab-title-get-title-id].
    1. **PLAYFAB_DEV_SECRET_KEY**: configure it with your [Title's developer secret key][playfab-title-get-developer-secret-key].
    1. **PlayFabTicTacToeCosmosDB**: retrieve your [AZ CosmosDB connection string][cosmos-db-config].
1. Open the [TicTacToeFunctions project][AZF-project] using *Visual Studio* or *Visual Studio Code*.
1. Publish the [TicTacToeFunctions project][AZF-project] in your *Azure Function App*:
    1. If your are using *Visual Studio*, follow [this guide][azf-publish-from-vs].
    1. If your are using *Visual Studio Code*, follow [this guide][azf-publish-from-vs-code].
1. Enable [CORS][azure-function-app-cors] in your *Azure Function APP* from the Azure Portal (you can follow [this guide][azure-function-app-cors-portal-config]).

    > NOTE: The enable all possible domains, you can use the `*` wildcard.

#### Retrieve Azure Functions URLs
For getting your *Azure Functions URLs*, following the first steps of [this tutorial][azure-function-test-get-url].

## Technologies
* [Azure Services][azure-main-page]
* [Azure Functions][azure-functions-main-page] ([Documentation's main page][azure-function-documentation])
* [PlayFab][playfab-main-page]
* [Visual Studio][visual-studio-download]/[Visual Studio Code][visual-studio-code-download]


[description]: #description
[az-function-list]: #azure-function-list
[configuration]: #configuration
[pre-requisites]: #pre-requisites
[configuration-steps]: #configuration-steps
[get-az-function-urls]: #retrieve-azure-functions-urls
[technologies]: #technologies

[AZF-project]: TicTacToeFunctions
[CreateMatchLobby-AZF-source-code]: TicTacToeFunctions/Functions/CreateMatchLobby.cs
[CreateSharedGroup-AZF-Source-code]: TicTacToeFunctions/Functions/CreateSharedGroup.cs
[DeleteMatchLobby-AZF-source-code]: TicTacToeFunctions/Functions/DeleteMatchLobby.cs
[DeleteSharedGroup-AZF-Source-code]: TicTacToeFunctions/Functions/DeleteSharedGroup.cs
[GetGameStatus-AZF-source-code]: TicTacToeFunctions/Functions/GetGameStatus.cs
[GetMatchlobbyInfo-AZF-source-code]: TicTacToeFunctions/Functions/GetMatchlobbyInfo.cs
[GetSharedGroup-AZF-source-code]: TicTacToeFunctions/Functions/GetSharedGroup.cs
[JoinMatch-AZF-source-code]: TicTacToeFunctions/Functions/JoinMatch.cs
[JoinMatchLobby-AZF-source-code]: TicTacToeFunctions/Functions/JoinMatchLobby.cs
[MakeMultiplayerMove-AZF-source-code]: TicTacToeFunctions/Functions/MakeMultiplayerMove.cs
[RestartMultiplayerGame-AZF-source-code]: TicTacToeFunctions/Functions/RestartMultiplayerGame.cs
[SearchMatchlobbies-AZF-source-code]: TicTacToeFunctions/Functions/SearchMatchlobbies.cs
[SetGameWinner-AZF-source-code]: TicTacToeFunctions/Functions/SetGameWinner.cs
[StartMatch-AZF-source-code]: TicTacToeFunctions/Functions/StartMatch.cs

[azure-main-page]: https://azure.microsoft.com/
[azure-account]: https://azure.microsoft.com/free/
[azure-sign-in-page]: https://azure.microsoft.com/account/
[azure-functions-main-page]: https://azure.microsoft.com/services/functions/
[azure-function-documentation]: https://docs.microsoft.com/azure/azure-functions/
[azure-function-app-create-portal]: https://docs.microsoft.com/azure/azure-functions/functions-create-function-app-portal
[azure-function-app-settings]: https://docs.microsoft.com/azure/azure-functions/functions-how-to-use-azure-function-app-settings#settings
[azure-function-app-cors]: https://docs.microsoft.com/azure/azure-functions/functions-how-to-use-azure-function-app-settings#cors
[azure-function-app-cors-portal-config]: https://docs.microsoft.com/azure/azure-functions/functions-how-to-use-azure-function-app-settings#portal-1
[azure-function-test-get-url]: https://docs.microsoft.com/azure/azure-functions/functions-create-first-azure-function#test-the-function

[playfab-main-page]: https://playfab.com/
[playfab-account-create]: https://developer.playfab.com/en-US/sign-up
[playfab-account-create-tutorial]: https://docs.microsoft.com/gaming/playfab/gamemanager/pfab-account
[playfab-account-login]: https://developer.playfab.com/en-US/login
[playfab-title-create-tutorial]: https://docs.microsoft.com/gaming/playfab/gamemanager/quickstart#create-your-first-game
[playfab-title-get-title-id]: https://docs.microsoft.com/gaming/playfab/personas/developer#retrieving-your-titleid
[playfab-title-get-developer-secret-key]: https://docs.microsoft.com/gaming/playfab/gamemanager/secret-key-management

[cosmos-db-readme]: cosmos-db-configuration.md
[cosmos-db-config]: cosmos-db-configuration.md#retrieve-cosmos-db-connection-string

[visual-studio-download]: https://visualstudio.microsoft.com/downloads/
[visual-studio-code-download]: https://code.visualstudio.com/download

[azf-publish-from-vs]: https://docs.microsoft.com/azure/azure-functions/functions-develop-vs#publish-to-azure
[azf-publish-from-vs-code]: https://docs.microsoft.com/azure/azure-functions/functions-develop-vs-code?tabs=csharp#sign-in-to-azure
