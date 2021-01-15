using FantasySoccer.Schema.Models;

namespace FantasySoccerDataSeeder.Models
{
    public static class BogusExtensionMethods
    {
        public static int GoalsAssistsStatsRandomizer(this Bogus.Randomizer randomizer, Position playerPosition, int minValue, int maxValue)
        {
            return playerPosition switch
            {
                Position.Goalkeeper => 0,
                _ => randomizer.Int(minValue, maxValue)
            };
        }
    }
}
