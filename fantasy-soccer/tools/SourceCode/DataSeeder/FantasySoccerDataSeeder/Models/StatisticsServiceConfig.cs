using FantasySoccerDataSeeder.Models.Interfaces;

namespace FantasySoccerDataSeeder.Models
{
    public class StatisticsServiceConfig : IStatisticsServiceConfig
    {
        public bool tournamentIsHomeAway { get; set; }
    }
}
