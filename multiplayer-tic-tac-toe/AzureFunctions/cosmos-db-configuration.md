# Cosmos DB configuration

## Index

- [Description][description]
- [Configuration][configuration]
  - [Prerequisites][prerequisites]
  - [Configuration steps][configuration-steps]
    - [Retrieve Cosmos DB connection string][get-connection-string]
- [Technologies][technologies]

## Description

This guide contains the necessary steps to configure an [Azure Cosmos DB][azure-cosmos-db] instance, and how to integrate it to the [Azure Functions project][AZF-project].

## Configuration

### Prerequisites

Before configuring this project, first ensure the following prerequisites have been completed:

- Create an [Azure account][azure-account], or [sign-in][azure-sign-in-page] into your current account.

### Configuration steps

1. Sign in into the [Azure Portal][azure-sign-in-page].
1. Go to your resource group, and create an [Azure Cosmos DB account][azure-cosmos-db-account-create].
1. Create the `PlayFabTicTacToe` database (you can follow [this guide][azure-cosmos-db-add-database-container]).
1. Create the `MatchLobby` container in the just created database (you can follow [this guide][azure-cosmos-db-add-database-container]):

    > NOTE: set `/MatchLobbyId` as the [Partition Key][azure-cosmos-db-partitioning].

1. Configure [CORS][azure-cosmos-db-cors] from the [Azure Portal][azure-sign-in-page] following [this guide][azure-cosmos-db-cors-portal-config].

    > NOTE: The enable all possible domains, you can use the `*` wildcard.

### Retrieve Cosmos DB connection string

For retrieving the connection string of your Azure Cosmos DB account, follow the next steps:

1. Sign in into the [Azure Portal][azure-sign-in-page].
1. Go to your resource group, and select your Azure Cosmos DB account.
1. Once your Azure Cosmos DB account page has been loaded, go to `Settings/Keys`, and then copy the content of the `Primary Connection String` textbox.

    ---

    <p align="center">
      <img src="../document-assets/images/CosmosDB-connection-string.png" />
    </p>

    ---

## Technologies

- [Azure Services][azure-main-page]
- [Azure Cosmos DB][azure-cosmos-db] ([Documentation's main page][azure-cosmos-db-doc-page])

[description]: #description
[configuration]: #configuration
[prerequisites]: #prerequisites
[configuration-steps]: #configuration-steps
[get-connection-string]: #retrieve-cosmos-db-connection-string
[technologies]: #technologies

[AZF-project]: TicTacToeFunctions

[azure-main-page]: https://azure.microsoft.com/
[azure-account]: https://azure.microsoft.com/free/
[azure-sign-in-page]: https://azure.microsoft.com/account/
[azure-cosmos-db]: https://azure.microsoft.com/services/cosmos-db/
[azure-cosmos-db-doc-page]: https://docs.microsoft.com/azure/cosmos-db/
[azure-cosmos-db-account-create]: https://docs.microsoft.com/azure/cosmos-db/create-cosmosdb-resources-portal#create-an-azure-cosmos-db-account
[azure-cosmos-db-add-database-container]: https://docs.microsoft.com/azure/cosmos-db/create-cosmosdb-resources-portal#add-a-database-and-a-container
[azure-cosmos-db-partitioning]: https://docs.microsoft.com/azure/cosmos-db/partitioning-overview

[azure-cosmos-db-cors]: https://docs.microsoft.com/azure/cosmos-db/how-to-configure-cross-origin-resource-sharing
[azure-cosmos-db-cors-portal-config]: https://docs.microsoft.com/azure/cosmos-db/how-to-configure-cross-origin-resource-sharing#enable-cors-support-from-azure-portal
