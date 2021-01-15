using System;
using System.Collections.Generic;
using System.Linq;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Core.Services
{
    internal class MatchSimulation
    {
        private static readonly Random random = new Random();

        private static bool GetRandomChoice()
        {
            return random.Next(0, 2) == 1;
        }

        private static int GetIntBeetween(int min, int max, bool includingMax = true)
        {
            return random.Next(min, max + (includingMax ? 1 : 0));
        }

        private static MatchSimulationProcessState InitializeState(Match match, int goals, int ownGoals, bool local, List<FutbolPlayer> futbolPlayers)
        {
            var state = new MatchSimulationProcessState();

            state.Match = match;
            state.Local = local;
            state.FinalState.OwnGoals = ownGoals;
            state.FinalState.FutbolPlayersProcessed = futbolPlayers.Count;
            state.ForwardsNumber = futbolPlayers.FindAll(fp => fp.Position == Position.Forward).Count;
            state.FinalState.ForwardGoals = (goals * random.Next(6, 10)) / 10;
            state.FinalState.ForwardsProccessed = state.ForwardsNumber;
            state.FinalState.OtherPositionGoals = goals - state.FinalState.ForwardGoals;
            state.FinalState.Changes = random.Next(0, Math.Min(SimulatorConstants.MaxChanges, futbolPlayers.Count - SimulatorConstants.PlayersOnTheField));
            state.FinalState.MinutesPlayedBySubs = new int[state.FinalState.Changes];
            state.FinalState.RedCards = random.Next(0, SimulatorConstants.MaxRedCards);

            return state;
        }

        private static MatchFutbolPlayerPerformance SimulateMatchFutbolPlayerPerformance(MatchSimulationProcessState state, FutbolPlayer futbolPlayer)
        {
            var playerPerformance = new MatchFutbolPlayerPerformance
            {
                ID = Guid.NewGuid().ToString(),
                MatchID = state.Match.ID,
                FutbolPlayerID = futbolPlayer.ID,
                Round = state.Match.Round,
                TournamentId = state.Match.TournamentId
            };

            playerPerformance.Goals = AssignGoals(state, futbolPlayer);
            if (futbolPlayer.Position == Position.Forward)
            {
                state.CurrentState.ForwardGoals += playerPerformance.Goals;
            }
            else
            {
                state.CurrentState.OtherPositionGoals += playerPerformance.Goals;
            }

            if (futbolPlayer.Position == Position.Goalkeeper)
            {
                playerPerformance.Saves = GetIntBeetween(0, SimulatorConstants.MaxSaves);
            }

            playerPerformance.YellowCards = GetIntBeetween(0, state.CurrentState.RedCards == state.FinalState.RedCards ? 1 : 2);
            if (playerPerformance.YellowCards == 2)
            {
                playerPerformance.RedCards = 1;
            }
            else
            {
                playerPerformance.RedCards = state.CurrentState.RedCards == state.FinalState.RedCards ? 0 : GetIntBeetween(0, 1);
            }
            state.CurrentState.RedCards += playerPerformance.RedCards;

            playerPerformance.Faults = GetIntBeetween(0, SimulatorConstants.MaxFaults);

            if (playerPerformance.RedCards == 1 || HasToMakeAChange(state))
            {
                playerPerformance.PlayedMinutes = GetIntBeetween(0, SimulatorConstants.MatchMinutes - 1);

                if (playerPerformance.RedCards == 0)
                {
                    state.CurrentState.MinutesPlayedBySubs[^state.CurrentState.Changes] = SimulatorConstants.MatchMinutes - playerPerformance.PlayedMinutes;
                    state.CurrentState.Changes++;
                }
            }

            playerPerformance.OwnGoals = AssignOwnGoals(state, futbolPlayer.Position);
            state.CurrentState.OwnGoals += playerPerformance.OwnGoals;

            playerPerformance.Score = GetScoreForFutbolPlayerPerformance(state, futbolPlayer.Position, playerPerformance);

            if (futbolPlayer.Position == Position.Forward)
            {
                state.CurrentState.ForwardsProccessed++;
            }

            state.CurrentState.FutbolPlayersProcessed++;

            return playerPerformance;
        }

        private static int AssignOwnGoals(MatchSimulationProcessState state, Position position)
        {
            if (state.FinalState.OwnGoals == 0 || state.CurrentState.OwnGoals == state.FinalState.OwnGoals)
            {
                return 0;
            }

            if (state.LastFutbolPlayerToProcess)
            {
                return state.FinalState.OwnGoals - state.CurrentState.OwnGoals;
            }

            if (position != Position.Forward && GetRandomChoice())
            {
                return GetIntBeetween(0, state.FinalState.OwnGoals - state.CurrentState.OwnGoals);
            }

            return 0;
        }

        private static int AssignGoals(MatchSimulationProcessState state, FutbolPlayer futbolPlayer)
        {
            switch (futbolPlayer.Position)
            {
                case Position.Forward:
                    var goalsToAssignToForwards = state.FinalState.ForwardGoals - state.CurrentState.ForwardGoals;
                    var minGoalsForPlayer = random.Next(0, (goalsToAssignToForwards * futbolPlayer.ProbabilityOfGoal / 100) + 1);
                    return state.LastForwardToProcess ? goalsToAssignToForwards : GetIntBeetween(minGoalsForPlayer, goalsToAssignToForwards);
                case Position.Midfielder:
                case Position.Defender:
                    var goalsToAssignToOtherPosition = state.FinalState.OtherPositionGoals - state.CurrentState.OtherPositionGoals;
                    var minGoalsForOtherPosition = random.Next(0, (goalsToAssignToOtherPosition * futbolPlayer.ProbabilityOfGoal / 100) + 1);
                    return GetIntBeetween(minGoalsForOtherPosition, goalsToAssignToOtherPosition);
                case Position.Goalkeeper:
                default:
                    return 0;
            }
        }

        private static bool HasToMakeAChange(MatchSimulationProcessState state)
        {
            return (state.CurrentState.Changes < state.FinalState.Changes && GetRandomChoice())
                || (state.FinalState.FutbolPlayersProcessed - state.CurrentState.FutbolPlayersProcessed == state.FinalState.Changes);
        }

        public static List<MatchFutbolPlayerPerformance> SimulateMatch(Match match, List<FutbolPlayer> localFutbolPlayers, List<FutbolPlayer> visitorFutbolPlayers)
        {
            var localOwnGoals = random.Next(0, Math.Min(match.VisitorGoals, SimulatorConstants.MaxOwnGoals));
            var visitorOwnGoals = random.Next(0, Math.Min(match.VisitorGoals, SimulatorConstants.MaxOwnGoals));

            var localState = InitializeState(match, match.LocalGoals - visitorOwnGoals, localOwnGoals, true, localFutbolPlayers);
            var visitorState = InitializeState(match, match.VisitorGoals - localOwnGoals, visitorOwnGoals, false, visitorFutbolPlayers);

            var performances = new List<MatchFutbolPlayerPerformance>();

            SetProbabilitiesOfGoal(localFutbolPlayers);
            SetProbabilitiesOfGoal(visitorFutbolPlayers);

            localFutbolPlayers.ForEach(localPlayer => performances.Add(SimulateMatchFutbolPlayerPerformance(localState, localPlayer)));
            visitorFutbolPlayers.ForEach(visitorPlayer => performances.Add(SimulateMatchFutbolPlayerPerformance(visitorState, visitorPlayer)));

            return performances;
        }

        private static void SetProbabilitiesOfGoal(List<FutbolPlayer> futbolPlayers)
        {
            var totalForwardGoals = futbolPlayers
                                                .Where(p => p.Position == Position.Forward)
                                                .Sum(p => p.GeneralStats.Goals);
            var totalOthersPositionGoals = futbolPlayers
                                    .Where(p => p.Position == Position.Defender || p.Position == Position.Midfielder)
                                    .Sum(p => p.GeneralStats.Goals);
            futbolPlayers.ForEach(player =>
            {
                switch (player.Position)
                {
                    case Position.Forward:
                        player.ProbabilityOfGoal = totalForwardGoals > 0 ? player.GeneralStats.Goals * 100 / totalForwardGoals : 0;
                        break;
                    case Position.Midfielder:
                    case Position.Defender:
                        player.ProbabilityOfGoal = totalOthersPositionGoals > 0 ? player.GeneralStats.Goals * 100 / totalOthersPositionGoals : 0;
                        break;
                    default:
                        player.ProbabilityOfGoal = 0;
                        break;
                }                
            });
        }

        private static int GetScoreForFutbolPlayerPerformance(MatchSimulationProcessState state, Position position, MatchFutbolPlayerPerformance futbolPlayerPerformance)
        {
            var score = futbolPlayerPerformance.Goals * (state.Local ? SimulatorConstants.HomeGoalPoints : SimulatorConstants.AwayGoalPoints);
            score += futbolPlayerPerformance.OwnGoals * SimulatorConstants.OwnGoalPoints;
            score += futbolPlayerPerformance.YellowCards * SimulatorConstants.YellowCardPoints;
            score += futbolPlayerPerformance.RedCards * SimulatorConstants.RedCardPoints;
            score += futbolPlayerPerformance.Saves * SimulatorConstants.SavesPoints;
            score += futbolPlayerPerformance.Faults * SimulatorConstants.FaultsPoints;

            if (state.UndefeatedFence)
            {
                switch (position)
                {
                    case Position.Defender:
                        score += SimulatorConstants.UndefeatedFencePointsForDefender;
                        break;
                    case Position.Goalkeeper:
                        score += SimulatorConstants.UndefeatedFencePointsForGoalkeeper;
                        break;
                }
            }

            return score;
        }
    }
}
