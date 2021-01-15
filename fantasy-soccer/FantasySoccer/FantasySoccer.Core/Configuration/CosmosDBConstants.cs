namespace FantasySoccer.Core.Configuration
{
    public static class CosmosDBConstants
    {
        public const string DatabaseId = "fantasysoccer";
        public const string TournamentContainerId = "Tournament";
        public const string MatchContainerId = "Match";
        public const string MatchPlayerPerformanceContainerId = "MatchPlayerPerformance";
        public const string FutbolPlayerId = "FutbolPlayer";
        public const string FutbolTeamId = "FutbolTeam";
        public const string UserTransactionId = "UserTransaction";
        public const string PartitionKey = "/id";
    }
}
