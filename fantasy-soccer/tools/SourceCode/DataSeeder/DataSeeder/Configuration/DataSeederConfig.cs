using FantasySoccer.Core.Configuration;
using FantasySoccerDataSeeder.Models;

namespace DataSeeder.Configuration
{
    public class DataSeederConfig
    {
        public CosmosDBConfig ConfigCosmosDB { get; set; }
        public PlayFabConfiguration ConfigPlayFab { get; set; }
        public TournamentConfig TournamentConfig { get; set; }
        public UserDataConfig UserDataConfig { get; set; }
        public string PlayFabId { get; set; }
        public string DatabaseName { get; set; }
    }
}
