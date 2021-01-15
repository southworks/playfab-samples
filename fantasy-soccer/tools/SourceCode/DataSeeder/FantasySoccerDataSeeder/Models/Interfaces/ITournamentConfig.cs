namespace FantasySoccerDataSeeder.Models.Interfaces
{
    public interface ITournamentConfig
    {
        int FutbolTeamsAmount { get; set; }
        int TournamentsAmount { get; set; }
        int TeamStartersAmount { get; set; }
        int TeamSubsAmount { get; set; }
        bool IsHomeAway { get; set; }
    }
}
