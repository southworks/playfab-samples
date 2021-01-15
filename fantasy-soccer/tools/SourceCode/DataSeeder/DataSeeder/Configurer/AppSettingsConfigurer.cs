using System;
using System.IO;
using DataSeeder.Configuration;
using FantasySoccer.Core.Configuration;
using FantasySoccerDataSeeder.Models;
using Microsoft.Extensions.Configuration;

namespace DataSeeder.Configurer
{
    public class AppSettingsConfigurer: IDataSeederConfigurer
    {
        private readonly IConfigurationRoot configuration;

        public AppSettingsConfigurer()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            configuration = builder.Build();
        }

        private CosmosDBConfig GetCosmosDBConfig()
        {
            return new CosmosDBConfig()
            {
                EndpointUri = configuration.GetSection("CosmosDBConfig").GetSection("EndpointUri").Value,
                PrimaryKey = configuration.GetSection("CosmosDBConfig").GetSection("PrimaryKey").Value,
            };
        }

        private PlayFabConfiguration GetPlayFabConfiguration()
        {
            return new PlayFabConfiguration()
            {
                CatalogName = configuration.GetSection("PlayFabConfiguration").GetSection("CatalogName").Value,
                Currency = configuration.GetSection("PlayFabConfiguration").GetSection("Currency").Value,
                DeveloperSecretKey = configuration.GetSection("PlayFabConfiguration").GetSection("DeveloperSecretKey").Value,
                StoreName = configuration.GetSection("PlayFabConfiguration").GetSection("StoreName").Value,
                TitleId = configuration.GetSection("PlayFabConfiguration").GetSection("TitleId").Value
            };
        }

        private TournamentConfig GetTournamentConfig()
        {
            return new TournamentConfig()
            {
                FutbolTeamsAmount = Convert.ToInt32(configuration.GetSection("TournamentConfig").GetSection("FutbolTeamsAmount").Value),
                IsHomeAway = Convert.ToBoolean(configuration.GetSection("TournamentConfig").GetSection("IsHomeAway").Value),
                TeamStartersAmount = Convert.ToInt32(configuration.GetSection("TournamentConfig").GetSection("TeamStartersAmount").Value),
                TeamSubsAmount = Convert.ToInt32(configuration.GetSection("TournamentConfig").GetSection("TeamSubsAmount").Value),
                TournamentsAmount = Convert.ToInt32(configuration.GetSection("TournamentConfig").GetSection("TournamentsAmount").Value)
            };
        }

        private UserDataConfig GetUserDataConfig()
        {
            return new UserDataConfig()
            {
                UserTeamsAmount = Convert.ToInt32(configuration.GetSection("UserDataConfig").GetSection("UserTeamsAmount").Value)
            };
        }

        public DataSeederConfig Configure()
        {
            return new DataSeederConfig()
            {
                ConfigCosmosDB = GetCosmosDBConfig(),
                ConfigPlayFab = GetPlayFabConfiguration(),
                DatabaseName = configuration.GetSection("DatabaseName").Value,
                PlayFabId = configuration.GetSection("PlayFabId").Value,
                TournamentConfig = GetTournamentConfig(),
                UserDataConfig = GetUserDataConfig()
            };
        }
    }
}
