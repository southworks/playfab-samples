using Newtonsoft.Json;

namespace FantasySoccer.Schema.Models.CosmosDB
{
    public class Match : Models.GeneralModel
    {
        public string LocalFutbolTeamID { get; set; }

        public string VisitorFutbolTeamID { get; set; }
        
        public int Round { get; set; }
        
        public string TournamentId { get; set; }
        
        public int LocalGoals { get; set; }
        
        public int VisitorGoals { get; set; }

        [JsonIgnore]
        public string LocalFutbolTeamName { get; set; }

        [JsonIgnore]
        public string VisitorFutbolTeamName { get; set; }
    }
}
