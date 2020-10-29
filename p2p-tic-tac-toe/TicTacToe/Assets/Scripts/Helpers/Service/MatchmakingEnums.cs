namespace TicTacToe.Models.Helpers
{
    public class MatchmakingTicketStatusEnum
    {
        public const string Canceled = "Canceled";
        public const string Matched = "Matched";
    }

    public enum QueueTypes
    {
        QuickMatch = 0,
        Party = 1
    }
}