using System;
using CommandLine;
using DataSeeder.Configuration;
using FantasySoccer.Core.Configuration;
using FantasySoccerDataSeeder.Models;

namespace DataSeeder.Configurer
{
    public class CLIArgumentsConfigurer: IDataSeederConfigurer
    {
        private readonly string[] args;

        public CLIArgumentsConfigurer(string[] args)
        {
            this.args = args;
        }

        public DataSeederConfig Configure()
        {
            var dataSeederConfig = new DataSeederConfig()
            {
                ConfigCosmosDB = new CosmosDBConfig(),
                ConfigPlayFab = new PlayFabConfiguration(),
                TournamentConfig = new TournamentConfig(),
                UserDataConfig = new UserDataConfig(),
                PlayFabId = string.Empty
            };

            _ = Parser.Default.ParseArguments<CLIOptions>(args)
                .WithParsed(opt =>
                {
                    dataSeederConfig.ConfigCosmosDB.EndpointUri = opt.EndpointUri ?? string.Empty;
                    dataSeederConfig.ConfigCosmosDB.PrimaryKey = opt.PrimaryKey ?? string.Empty;

                    dataSeederConfig.ConfigPlayFab.CatalogName = opt.CatalogName ?? string.Empty;
                    dataSeederConfig.ConfigPlayFab.Currency = opt.Currency ?? string.Empty;
                    dataSeederConfig.ConfigPlayFab.DeveloperSecretKey = opt.DeveloperSecretKey ?? string.Empty;
                    dataSeederConfig.ConfigPlayFab.StoreName = opt.StoreName ?? string.Empty;
                    dataSeederConfig.ConfigPlayFab.TitleId = opt.TitleId ?? string.Empty;

                    dataSeederConfig.TournamentConfig.FutbolTeamsAmount = opt.FutbolTeamsAmount;
                    dataSeederConfig.TournamentConfig.IsHomeAway = opt.IsHomeAway;
                    dataSeederConfig.TournamentConfig.TeamStartersAmount = opt.TeamStartersAmount;
                    dataSeederConfig.TournamentConfig.TeamSubsAmount = opt.TeamSubsAmount;
                    dataSeederConfig.TournamentConfig.TournamentsAmount = opt.TournamentsAmount;

                    dataSeederConfig.UserDataConfig.UserTeamsAmount = opt.UserTeamsAmount;

                    dataSeederConfig.PlayFabId = opt.PlayFabId ?? string.Empty;

                    dataSeederConfig.DatabaseName = opt.DatabaseName ?? string.Empty;
                });

            return dataSeederConfig;
        }
    }

    public class CLIOptions
    {
        #region CosmosDBConfig section

        [Option("endpointUri", Required = false, HelpText = "Cosmos DB Endpoint URI")]
        public string EndpointUri { get; set; }

        [Option("primaryKey", Required = false, HelpText = "Cosmos DB Primary Key")]
        public string PrimaryKey { get; set; }

        #endregion

        #region PlayFabConfiguration section

        [Option("titleId", Required = false, HelpText = "PlayFab Title ID")]
        public string TitleId { get; set; }

        [Option("developerSecretKey", Required = false, HelpText = "PlayFab Developer Secret Key")]
        public string DeveloperSecretKey { get; set; }

        [Option("catalogName", Required = false, HelpText = "PlayFab Catalog name")]
        public string CatalogName { get; set; }

        [Option("storeName", Required = false, HelpText = "PlayFab Store name")]
        public string StoreName { get; set; }

        [Option("currency", Required = false, HelpText = "PlayFab Currency name")]
        public string Currency { get; set; }

        #endregion

        #region TournamentConfig section

        [Option("futbolTeamsAmount", Required = false, HelpText = "Futbol teams amount")]
        public int FutbolTeamsAmount { get; set; }

        [Option("isHomeAway", Required = false, HelpText = "Tournament is home/away or not")]
        public bool IsHomeAway { get; set; }

        [Option("teamStartersAmount", Required = false, HelpText = "Amount of starters players per team")]
        public int TeamStartersAmount { get; set; }

        [Option("teamSubsAmount", Required = false, HelpText = "Amount of sub players per team")]
        public int TeamSubsAmount { get; set; }

        [Option("tournamentsAmount", Required = false, HelpText = "Amount of tournament to fake")]
        public int TournamentsAmount { get; set; }

        #endregion

        #region UserDataConfig Section

        [Option("userTeamsAmount", Required = false, HelpText = "Amount of teams per user")]
        public int UserTeamsAmount { get; set; }

        #endregion

        [Option("playFabId", Required = false, HelpText = "PlayFabId")]
        public string PlayFabId { get; set; }

        [Option("databaseName", Required = false, HelpText = "Cosmos DB Name")]
        public string DatabaseName { get; set; }
    }
}
