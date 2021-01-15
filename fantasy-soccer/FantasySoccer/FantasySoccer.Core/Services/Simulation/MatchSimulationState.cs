using FantasySoccer.Schema.Models.CosmosDB;

namespace FantasySoccer.Core.Services
{
    internal class MatchSimulationState
    {
        public int FutbolPlayersProcessed { get; set; }
        public int ForwardGoals { get; set; }
        public int ForwardsProccessed { get; set; }
        public int OtherPositionGoals { get; set; }
        public int OwnGoals { get; set; }
        public int Changes { get; set; }
        public int[] MinutesPlayedBySubs { get; set; }
        public int RedCards { get; set; }
    }

    internal class MatchSimulationProcessState
    {
        public Match Match { get; set; }
        public int ForwardsNumber { get; set; }
        public bool Local { get; set; }
        public bool UndefeatedFence { get; set; }
        public bool LastFutbolPlayerToProcess => CurrentState.FutbolPlayersProcessed == FinalState.FutbolPlayersProcessed - 1;
        public bool LastForwardToProcess => CurrentState.ForwardsProccessed == FinalState.ForwardsProccessed - 1;
        public MatchSimulationState FinalState { get; set; } = new MatchSimulationState();
        public MatchSimulationState CurrentState { get; set; } = new MatchSimulationState();
    }
}
