namespace FantasySoccer.Schema.Models.CosmosDB
{
    public class MatchFutbolPlayerPerformance : Models.GeneralModel
    {
        public string FutbolPlayerID { get; set; }

        public string MatchID { get; set; }
        
        public int Goals { get; set; }
        
        public int Faults { get; set; }
        
        public int YellowCards { get; set; }
        
        public int RedCards { get; set; }
        
        public int Saves { get; set; }
        
        public int OwnGoals { get; set; }
        
        public int PlayedMinutes { get; set; }
        
        public int Score { get; set; }
        
        public int Round { get; set; }
        
        public string TournamentId { get; set; }
    }
}
