# Provisioning script

This script is for creating the environment for the project. It includes the following Azure Resources:

- Resource group: to contain all the created resources
- App Service Plan: to use for running the Web App and Function Apps. It's created with operating system Linux.
- Web App: to use for the FantasySoccer  MVC project. The runtime used is DOTNETCORE 3.1.
- Storage account: storage for the Function Apps
- Function App: to implement FantasySoccerSimulator and FantasySoccerAZF projects. Both functions use dotnet as a runtime and the function version 2.
- CosmosDB Account: to support the database for the project
- SQL Database: to manage the data model
- Containers: to store data

The script also creates an Open ID connection on a Playfab title using the URL of the Azure AD B2C OpenID Connect metadata document and the PlayFab ID configured as a parameter.

## How to use

1. Open Powershell console on the script folder
2. Log in with the Service Principal and Azure Subscription to be used

    ```Powershell
    $pscredential = Get-Credential -UserName <ServicePrincipalAppId>
    # Write your secret when a window asks for a password
    Connect-AzAccount -ServicePrincipal -Credential $pscredential -Tenant <TenantId>
    ```

3. Run the following command

    ```Powershell
    .\provisioning-script.ps1 <Environment> <ProjectName> <Region> <ClientId> <ClientSecret> <PlayFabTitleId> <PlayFabSecretKey>
    ```

    Example:

    ```Powershell
    .\provisioning-script.ps1 dev fantasy-soccer eastus ...
    ```
