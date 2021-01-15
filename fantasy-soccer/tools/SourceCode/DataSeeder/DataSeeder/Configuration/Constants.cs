namespace DataSeeder.Configuration
{
    public static class CosmosDBConstants
    {
        public static string DatabaseName = "";
        public const string FutbolPlayerContainer = "FutbolPlayer";
        public const string FutbolTeamContainer = "FutbolTeam";
        public const string MatchContainer = "Match";
        public const string TournamentContainerName = "Tournament";
        public const string UserTransactionContainer = "UserTransaction";
        public const string MatchPlayerPerformanceContainerId = "MatchPlayerPerformance";
        public const string PartitionKey = "/id";
    }
}
