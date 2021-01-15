using FantasySoccerDataSeeder.Models.Interfaces;

namespace FantasySoccerDataSeeder.Models
{
    public class TournamentConfig: ITournamentConfig
    {
        public int FutbolTeamsAmount { get; set; }
        public int TournamentsAmount { get; set; }
        public int TeamStartersAmount { get; set; }
        public int TeamSubsAmount { get; set; }
        public bool IsHomeAway { get; set; }
    }
}
