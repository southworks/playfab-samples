# DataSeeder

## Index

- [Summary][summary]
- [Pre-requisites][pre-requisites]
- [Configuration][configuration]
  - [Appsettings file][appsettings-file]
  - [CLI arguments][cli-arguments]

## Summary

The DataSeeder is an additional application for the [Fantasy Soccer][fantasy-soccer-app-dir] app which allows us to fill with mocked data a set of AZURE resources and a PlayFab title.

## Pre-requisites

In order to run this application, it's necessary to run the [provisioning script][provisioning-script-path] for the [Fantasy Soccer][fantasy-soccer-app-dir] app.

It's also recommended to follow up this guides:

- Create an Azure Active Directory Tenant ([link][azure-ad-tenant]).
- Create a PlayFab account ([link][playfab-account]).
- Create a PlayFab game ([link][playfab-game]).
- Create a PlayFab Title ([link][playfab-title]).

## Configuration

We have two possible ways for configuring this application:

### Appsettings file

The first possible way is using the [appsettings.json][appsettings-json-file] file. This formatting way is ideal for running the application in a local environment, as we only have to set the next variables in the file:

- `CosmosDBConfig`
  - `EndpointUri`: A CosmosDB URI.
  - `PrimaryKey`: A CosmosDB Primary Key.
- `PlayFabConfiguration`
  - `TitleId`: A PlayFab Title ID.
  - `DeveloperSecretKey`: A PlayFab Title's Developer Secret Key.
  - `CatalogName`: An existing PlayFab Title's Catalog name.
  - `StoreName`: An existing PlayFab Title's Store name.
  - `Currency`: An existing PlayFab Title's Currency name
- `TournamentConfig`
  - `FutbolTeamsAmount`: amount of football teams the application will fake.
  - `IsHomeAway`: determines if the tournament to fake will be a home/away or not, i.e., if two teams will be facing twice or only once in the tournament.
  - `TeamStartersAmount`: amount of starters players for each team.
  - `TeamSubsAmount`: amount of substitutes players for each team.
  - `TournamentsAmount`: amount of tournament to fake.
- `UserDataConfig`
  - `UserTeamsAmount`: max amount of teams that will be faked for the players.
- `DatabaseName`: current CosmosDB name.

### CLI Arguments

The other alternative we have to configure this project is using a set of command line arguments. For using this, we've to use the `dotnet run` command in a CLI, and then use the next arguments:

- `--endpointUri`: A CosmosDB URI.
- `--primaryKey`: A CosmosDB Primary Key.
- `--titleId`: A PlayFab Title ID.
- `--developerSecretKey`: A PlayFab Title's Developer Secret Key.
- `--catalogName`: An existing PlayFab Title's Catalog name.
- `--storeName`: An existing PlayFab Title's Store name.
- `--currency`: An existing PlayFab Title's Currency name
- TournamentConfig.
- `--futbolTeamsAmount`: amount of football teams the application will fake.
- `--isHomeAway`: determines if the tournament to fake will be a home/away or not, i.e., if two teams will be facing twice or only once in the tournament.
- `--teamStartersAmount`: amount of starters players for each team.
- `--teamSubsAmount`: amount of substitutes players for each team.
- `--tournamentsAmount`: amount of tournament to fake.
- `--userTeamsAmount`: max amount of teams that will be faked for the players.
- `--databaseName`: current CosmosDB name.

[summary]:#Summary
[pre-requisites]:#pre---requisites
[configuration]:#configuration
[appsettings-file]:#appsettings-file
[cli-arguments]:#cli-arguments

[fantasy-soccer-app-dir]: ./FantasySoccer
[provisioning-script-path]: ./tools/provisioning-script/provisioning-script.ps1
[appsettings-json-file]: ./tools/SourceCode/DataSeeder/DataSeeder/appsettings.json

[azure-ad-tenant]: https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-access-create-new-tenant

[playfab-account]: https://docs.microsoft.com/gaming/playfab/gamemanager/quickstart#create-a-playfab-account
[playfab-game]: https://docs.microsoft.com/gaming/playfab/gamemanager/quickstart#create-your-first-game
[playfab-title]: https://docs.microsoft.com/gaming/playfab/gamemanager/quickstart#your-studios-and-titles
