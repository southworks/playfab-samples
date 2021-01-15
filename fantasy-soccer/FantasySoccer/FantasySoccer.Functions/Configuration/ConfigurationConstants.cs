using System;
using FantasySoccer.Core.Configuration;
using FantasySoccer.Models.Configuration;

namespace FantasySoccer.Functions.Configuration
{
    public static class ConfigurationConstants
    {
        public static readonly CosmosDBConfig CosmosDbConfig = new CosmosDBConfig
        {
            DatabaseName = Environment.GetEnvironmentVariable("CosmosDBConfig.DatabaseName", EnvironmentVariableTarget.Process),
            EndpointUri = Environment.GetEnvironmentVariable("CosmosDBConfig.EndpointUri", EnvironmentVariableTarget.Process),
            PrimaryKey = Environment.GetEnvironmentVariable("CosmosDBConfig.PrimaryKey", EnvironmentVariableTarget.Process)
        };

        public static readonly PlayFabConfiguration PlayFabConfiguration = new PlayFabConfiguration
        {
            // ID of the segment that contains all players
            AllUserSegmentId = Environment.GetEnvironmentVariable("PlayFabConfiguration.AllUserSegmentId", EnvironmentVariableTarget.Process),
            CatalogName = Environment.GetEnvironmentVariable("PlayFabConfiguration.CatalogName", EnvironmentVariableTarget.Process),
            // OpenId connection id
            ConnectionId = Environment.GetEnvironmentVariable("PlayFabConfiguration.ConnectionId", EnvironmentVariableTarget.Process),
            Currency = Environment.GetEnvironmentVariable("PlayFabConfiguration.Currency", EnvironmentVariableTarget.Process),
            DeveloperSecretKey = Environment.GetEnvironmentVariable("PlayFabConfiguration.DeveloperSecretKey", EnvironmentVariableTarget.Process),
            StoreName = Environment.GetEnvironmentVariable("PlayFabConfiguration.StoreName", EnvironmentVariableTarget.Process),
            TitleId = Environment.GetEnvironmentVariable("PlayFabConfiguration.TitleId", EnvironmentVariableTarget.Process)
        };
    }
}
