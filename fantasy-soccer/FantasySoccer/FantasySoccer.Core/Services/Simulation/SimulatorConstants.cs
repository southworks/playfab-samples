namespace FantasySoccer.Core.Services
{
    public class SimulatorConstants
    {
        public const int MaxFaults = 10;
        public const int MaxChanges = 3;
        public const int MaxRedCards = 4;
        public const int MatchMinutes = 90;
        public const int PlayersOnTheField = 11;
        public const int MaxOwnGoals = 2;
        public const int MaxSaves = 10;
        public const int HomeGoalPoints = 5;
        public const int AwayGoalPoints = 6;
        public const int OwnGoalPoints = -2;
        public const int YellowCardPoints = -2;
        public const int RedCardPoints = -5;
        public const int SavesPoints = 2;
        public const int FaultsPoints = -1;
        public const int UndefeatedFencePointsForGoalkeeper = 3;
        public const int UndefeatedFencePointsForDefender = 2;
        public const int ProbabilityOfWinningForFavorite = 60;
        public const int ProbabilityOfWinningForLessFavorite = 40;
        public const int ProbabilityOfWinningEqual = 50;        
    }
}
