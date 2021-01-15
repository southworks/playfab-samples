param (
    [parameter(Mandatory=$true)]
    [String]$Environment, 
    [parameter(Mandatory=$true)]
    [String]$ProjectName, 
    [parameter(Mandatory=$true)]
    [String]$Region,
    [parameter(Mandatory=$true)]
    [String]$ClientId,
    [parameter(Mandatory=$true)]
    [String]$ClientSecret,
    [parameter(Mandatory=$true)]
    [String]$PlayFabTitleId,
    [parameter(Mandatory=$true)]
    [String]$PlayFabSecretKey
 )

$ResourceGroupName = "rg-$ProjectName-$Environment"
$AppServicePlanName = "plan-$ProjectName-$Environment"
$WebAppName = "app-$ProjectName-$Environment"
$StorageAccountName = "st$ProjectName$Environment"
$FunctionAppSimulatorName = "func-$ProjectName-simulator-$Environment"
$FunctionAppAZFName = "func-$ProjectName-azf-$Environment"
$CosmosDBName = "cosmos-$ProjectName-$Environment"
$DBName = "db-$ProjectName-$Environment"
$AppInsightsName = "$ProjectName-app-insights"

write-host "Params:"
write-host "Environment: $Environment"
write-host "ProjectName: $ProjectName"
write-host "Region: $Region"
write-host "ClientId: $ClientId"
write-host "ClientSecret: $ClientSecret"
write-host "PlayFabTitleId: $PlayFabTitleId"
write-host "PlayFabSecretKey: $PlayFabSecretKey"

write-host "`r`nTask 1: Create Resource group"
az group create -n $ResourceGroupName -l $Region

write-host "`r`nTask 2: Create App Service plan"
az appservice plan create -n $AppServicePlanName `
    -g $ResourceGroupName `
    --sku B1 `

write-host "`r`nTask 3: Create Web App"
az webapp create -n $WebAppName `
    -g $ResourceGroupName `
    -p $AppServicePlanName `
    --runtime '"DOTNETCORE|3.1"'

write-host "`r`nTask 4: Create Storage Account"
az storage account create -n $StorageAccountName `
    -g $ResourceGroupName `
    --sku Standard_GRS


write-host "`r`nTask 4.5.1: Add App Insights Extensions"
az extension add --name application-insights --yes

write-host "`r`nTask 4.5.2: Create App Insights"
az monitor app-insights component create `
    --app $AppInsightsName `
    --location $Region `
    --resource-group $ResourceGroupName
  
write-host "`r`nTask 6: Create Function App FutbolPlaymanAZF"
az functionapp create -n $FunctionAppAZFName `
    -g $ResourceGroupName `
    -s $StorageAccountName `
    --plan $AppServicePlanName `
    --runtime dotnet `
    --functions-version 2 `
    --app-insights $AppInsightsName

write-host "`r`nUpdate Function App FutbolPlaymanAZF"
az functionapp config set --always-on false `
    -n $FunctionAppAZFName `
    -g $ResourceGroupName
    
write-host "`r`nTask 7: Create Azure Cosmos DB database account"
az cosmosdb create --n $CosmosDBName `
    -g $ResourceGroupName

write-host "`r`nTask 8: Create Database"
az cosmosdb sql database create -n $DBName `
    -a $CosmosDBName `
    -g $ResourceGroupName

write-host "`r`nTask 9: Create containers"
$Containers = @(
    [pscustomobject]@{Name="UserTeam"; PartitionKeyPath = "/id"}
    [pscustomobject]@{Name="FutbolTeam"; PartitionKeyPath = "/id"}
    [pscustomobject]@{Name="Match"; PartitionKeyPath = "/id"}
    [pscustomobject]@{Name="UserTransaction"; PartitionKeyPath = "/id"}
    [pscustomobject]@{Name="FutbolPlayer"; PartitionKeyPath = "/id"}
    [pscustomobject]@{Name="FutbolPlayerGeneralStats"; PartitionKeyPath = "/id"}
    [pscustomobject]@{Name="FutbolPlayerTournamentStats"; PartitionKeyPath = "/id"}
    [pscustomobject]@{Name="MatchPlayerPerformance"; PartitionKeyPath = "/id"}
    [pscustomobject]@{Name="Tournament"; PartitionKeyPath = "/id"}
)

Foreach ($i in $Containers)
{
    write-host "`r`nCreate container "$i.Name
    az cosmosdb sql container create -n $i.Name `
        -a $CosmosDBName `
        -g $ResourceGroupName `
        -d $DBName `
        --partition-key-path $i.PartitionKeyPath `
        --throughput 400
}
