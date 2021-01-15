# Azure Pipelines

## Index

- [Summary][summary]
- [Pre-Requisites][pre-requisites]
- [Pipelines][pipelines]
  - [Provisioning][provisioning-pipeline]
  - [Delete Resources][delete-resources-pipeline]
  - [Data seeder][data-seeder-pipeline]
  - [Testing][testing-pipeline]
  - [Deployment][deployment-pipeline]
  - [Release][release-pipeline]
- [Azure Variable Group][azure-variable-groups]
  - [Data Seeder][fantasy-soccer-data-seeder-variable-group]
  - [Deployment][fantasy-soccer-deployment-variable-group]
  - [Provisioning][fantasy-soccer-provisioning-script-variable-group]
  - [Service Principal][fantasy-soccer-service-principal-variable-group]
  - [Testing][fantasy-soccer-testing-variable-group]
  - [Web App][fantasy-soccer-web-app-settings-variable-group]

## Summary

This document describes the main features of the different pipelines we have, and how to configure them for using in an automatized Pipelines Scenario.

## Pre-requisites

It's recommended to read these documentations:

- Azure Pipelines Documentation ([link][az-pipelines-doc])
- Azure Release Pipelines Documentation ([link][az-release-pipelines-doc])
- Azure Pipelines Variables Group ([link][az-pipelines-variable-groups-doc])
- Artifacts in Azure Pipelines ([link][az-pipelines-artifacts-doc])
- Publishing artifacts ([link][az-pipelines-publishing-artifacts-doc])

## Pipelines

### Provisioning Pipeline

[This pipeline][provisioning-pipeline-yml] allows us to create all the necessary Azure resources for running the *Fantasy Soccer* application, and to configure our PlayFab title. It consists in a two parts task, being the first part the one where we Login into Azure with a service principal account (as we will be creating new resources). The second part of this task is where we run the [provisioning script][provisioning-script], which creates the specified Azure resources, and configure our PlayFab title automatically.

This pipeline is configured for only running when executed manually. Also, it requires to configure the [provisioning script][fantasy-soccer-provisioning-script-variable-group] and [service principal][fantasy-soccer-service-principal-variable-group] Variable Groups.

### Delete Resources Pipeline

The [delete-resources][delete-resources-pipeline-yml] pipeline is a manually triggered pipeline which allows us to delete a set of defined Azure resources. For running it it's necessary to configure the [provisioning script][fantasy-soccer-provisioning-script-variable-group] and [service principal][fantasy-soccer-service-principal-variable-group] Variable Groups.

### Data Seeder Pipeline

The [Data Seeder][data-seeder-pipeline-yml] pipeline allows us to run the [data-seeder][data-seeder-app] application in order to fill with fake data an Azure CosmosDB. It consists in two tasks, being the first the one we use for login into Azure with a Service Principal application, and the second task runs the [data-seeder][data-seeder-app] app.

For running it, it's necessary to configure the next Variable Groups:

- [Provisioning Script Variable Group][fantasy-soccer-provisioning-script-variable-group]
- [Data Seeder Variable Group][fantasy-soccer-data-seeder-variable-group]
- [Web App Settings Variable Group][fantasy-soccer-web-app-settings-variable-group]
- [Service Principal Variable Group][fantasy-soccer-service-principal-variable-group]

### Testing Pipeline

The [Testing][testing-pipeline-yml] pipeline allows us to run the Fantasy Soccer's [Unit test][fantasy-soccer-unit-test-project] project in order to check if any of these tests fail or not.

This pipeline will run after a PR against our *master* branch has been created.

This PR only uses the [fantasy-soccer-testing-VG][fantasy-soccer-testing-variable-group] Variable Group.

### Deployment Pipeline

The [Deployment][deployment-pipeline-yml] Pipeline aims to generate the necessary assets (A.K.A. build artifacts) that will be used later for an Azure Release Pipeline in order to release a set of applications. In our case, this pipeline creates two assets:

- The Fantasy Soccer [Web App][fantasy-soccer-web-app] project
- The Fantasy Soccer [Azure Functions APP][fantasy-soccer-azf]

Both of these projects are zipped into a [build artifact][pipeline-artifacts-doc] which will be shared with the Azure Release Pipeline for future uses.

This Pipeline will be triggered after a PR against our *master* branch has been created.

This Pipeline only uses the [fantasy-soccer-deployment-VG][fantasy-soccer-deployment-variable-group] Variable Group.

### Release Pipeline

Finally, we've created a Release Pipeline which consumes the [Deployment Pipeline][deployment-pipeline]'s artifacts in order to publish them into their respective Azure resources.

This pipeline consists in one Job with three tasks:

- First, we deploy the current Fantasy Soccer's [Web APP][fantasy-soccer-web-app] into Azure.
- Secondly, using the [fantasy-soccer-web-app-settings][fantasy-soccer-web-app-settings-variable-group] we configure the just published Web App.
- Finally, we deploy the Fantasy Soccer [Azure Functions APP][fantasy-soccer-azf].

So far, this release pipeline could only be triggered manually, and it uses three variables groups:

- [fantasy-soccer-web-app-settings][fantasy-soccer-web-app-settings-variable-group] Variable Group
- [fantasy-soccer-deployment-VG][fantasy-soccer-deployment-variable-group] Variable Group
- [fantasy-soccer-provisioning-script-VG][fantasy-soccer-provisioning-script-variable-group] Variable Group

## Azure Variable Groups

### Fantasy Soccer Data Seeder Variable Group

The *fantasy-soccer-data-seeder-VG* is a Variable Group used for defining variables that will be used alongside the [data-seeder application][data-seeder-app]. It consists in the next variables:

- `futbol-teams-amount`: defines the amount of football a tournament will have.
- `is-home-away`: determines if the tournament is a home-away one or not, i.e., if two teams will be facing twice or only once per tournament.
- `playfab-catalog-name`: name of the PlayFab catalog we'll be using for storing data.
- `playfab-currency`: name of the PlayFab's currency we'll be using in the application.
- `playfab-store-name`: name of the PlayFab's store we'll be using with the application.
- `playFabId`: any player's PlayFab Id. Will be used for creating an starting catalog for the player.
- `project-folder-path`: path to the data-seeder app, referencing from the repository main folder.
- `project-name`: data-seeder app's name.
- `team-starters-amount`: amount of starter players per team, which the app will be faking.
- `team-subs-amount`: amount of substitutes players per team, which the app will be faking.
- `tournaments-amount`: amount of tournament to fake.
- `user-teams-amount`: amount of teams per user to fake.

### Fantasy Soccer Deployment Variable Group

The *fantasy-soccer-deployment-VG* is a Variable Group we'll use for the deployment pipelines. It consists in the next variables:

- `AZFAppName`: path to the Azure Function app we'll be deploying. The path should be created with the repository's root as path base.
- `dropArtifactName`: name of the drop artifact we create as a result of the deployment pipeline. More info about this kind of artifacts [here][pipeline-artifacts-doc].
- `fantasy-soccer-solution-path`: path to the web application we'll be deploying. The path should be created with the repository's root as path base.
- `storageAccountName`: name of the Azure Storage Account where the web app will be deployed.
- `WebAppProjectName`: web application's name.

### Fantasy Soccer Provisioning Script Variable Group

The *fantasy-soccer-provisioning-script-VG* Variable Group contains all the necessary variables we'll be using in the provisioning pipeline. It contains the next variables:

- `ClientId`: App's registration ID.
- `ClientSecret`: App's registration Secret key.
- `Environment`: Environment where we'll be deploying the application.
- `PlayFabSecretKey`: PlayFab Developer's Secret Key. More info in this [here][playfab-dev-secret-key].
- `PlayFabTitleId`: PlayFab Title's ID.
- `ProjectName`: Name of the project we'll be creating. This variable will define the Azure resources' names.
- `Region`: Region where we'll be creating the necessary Azure resources.

### Fantasy Soccer Service Principal Variable Group

The *fantasy-soccer-service-principal* variable group defines a set of variables that will allows us to Login into Azure with a Service Principal account:

- `AppId`: Service Principal APP's ID.
- `Secret`: Secret key we have created for our Service Principal's APP.
- `SpName`: Service Principal Connection name.
- `subscriptionName`: Subscription where we have our Service Principal APP.
- `TenantId`: Service Principal Tenant (Directory)'s ID.

### Fantasy Soccer Testing Variable Group

The *fantasy-soccer-testing-VG* variable group has the necessary variable we'll be using in a testing pipeline.
It contains:

- `project_path`: path to the application we'll be testing, considering as a base path the repository's root folder.

### Fantasy Soccer Web App Settings Variable Group

The *fantasy-soccer-web-app-settings* variable group has all the necessary variables we'll be using in the pipeline for configuring the Azure Web App settings:

- `AzureAD__CallbackPath`: Azure AD callback path.
- `AzureAD__ClientId`: Azure AD Client ID.
- `AzureAD__Domain`: Azure AD Domain.
- `AzureAD__Instance`: Azure AD Instance.
- `AzureAD__TenantId`: Azure AD Tenant ID.
- `CosmosDB__EndpointUri`: CosmosDB Endpoint URI.
- `CosmosDB__PrimaryKey`: CosmosDB Primary Key.
- `CosmosDB__DatabaseName`: CosmosDB Database's name.
- `PlayFab__AllUserSegmentId`: PlayFab All user's Segment ID.
- `PlayFab__CatalogName`: PlayFab Catalog's name.
- `PlayFab__ConnectionId`: PlayFab's Connection ID.
- `PlayFab__Currency`: PlayFab's Currency Name.
- `PlayFab__DeveloperSecretKey`: PlayFab's Developer Secret Key.
- `PlayFab__StoreName`: PlayFab's Store name.
- `PlayFab__TitleId`: PlayFab's Title ID.

[data-seeder-pipeline-yml]: ./tools/pipelines/fantasy-soccer-data-seeder.yml
[delete-resources-pipeline-yml]: ./tools/pipelines/fantasy-soccer-delete-resources.yml
[deployment-pipeline-yml]: ./tools/pipelines/fantasy-soccer-deployment.yml
[provisioning-pipeline-yml]: ./tools/pipelines/fantasy-soccer-provisioning.yml
[testing-pipeline-yml]: ./tools/pipelines/fantasy-soccer-testing.yml

[provisioning-script]: ./tools/provisioning-script/provisioning-script.ps1
[data-seeder-app]: ./tools/SourceCode/DataSeeder

[pipeline-artifacts-doc]: https://docs.microsoft.com/azure/devops/pipelines/artifacts/pipeline-artifacts?view=azure-devops&tabs=yaml
[playfab-dev-secret-key]: https://docs.microsoft.com/gaming/playfab/gamemanager/secret-key-management

[fantasy-soccer-unit-test-project]: ./FantasySoccer/FantasySoccer.Tests
[fantasy-soccer-web-app]: ./FantasySoccer/FantasySoccer
[fantasy-soccer-azf]: ./FantasySoccer/FantasySoccer.Functions

[az-pipelines-doc]: https://docs.microsoft.com/azure/devops/pipelines/?view=azure-devops&viewFallbackFrom=vs-2019
[az-release-pipelines-doc]: https://docs.microsoft.com/azure/devops/pipelines/release/?view=azure-devops
[az-pipelines-artifacts-doc]: https://docs.microsoft.com/azure/devops/pipelines/artifacts/artifacts-overview?view=azure-devops
[az-pipelines-publishing-artifacts-doc]: https://docs.microsoft.com/azure/devops/pipelines/artifacts/pipeline-artifacts?view=azure-devops&tabs=yaml#publishing-artifacts
[az-pipelines-variable-groups-doc]: https://docs.microsoft.com/azure/devops/pipelines/library/variable-groups?view=azure-devops&tabs=yaml

[summary]: #summary
[pre-requisites]: #pre-requisites

[pipelines]: #pipelines

[provisioning-pipeline]: #provisioning-pipeline
[delete-resources-pipeline]: #delete-resources-pipeline
[data-seeder-pipeline]: #data-seeder-pipeline
[testing-pipeline]: #testing-pipeline
[deployment-pipeline]: #deployment-pipeline
[release-pipeline]: #release-pipeline

[azure-variable-groups]: #azure-variable-groups

[fantasy-soccer-data-seeder-variable-group]: #fantasy-soccer-data-seeder-variable-group
[fantasy-soccer-deployment-variable-group]: #fantasy-soccer-deployment-variable-group
[fantasy-soccer-provisioning-script-variable-group]: #fantasy-soccer-provisioning-script-variable-group
[fantasy-soccer-service-principal-variable-group]: #fantasy-soccer-service-principal-variable-group
[fantasy-soccer-testing-variable-group]: #fantasy-soccer-testing-variable-group
[fantasy-soccer-web-app-settings-variable-group]: #fantasy-soccer-web-app-settings-variable-group
