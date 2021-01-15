# Simulation


## Index

- [Summary][summary]
- [Implementation][implementation]

## Summary

The Simulation process generates the the player performances for a tournament round.
This simulation mocks data such as the number of goals, faults, played minutes, or red cards for each player, among other performance attributes.

## Implementation

The simulation process starts from the Simulation view and the request to the [AdminController's SimulateRound][ctrl-simulate-round] method, which uses the [SimulationService][simulation-service].

---

<p align="center">
  <img src="./documentation-assets/simulation-form.png" />
</p>


---

The simulation feature is accessible through the [SimulationService][simulation-service], which exposes the method [SimulateTournamentRound][simulate-tournament-round]. It returns a list of [MatchSimulationResult][match-simulation-result] with the simulated data for each match, including the soccer players' performance data.

To generate this list the Simulation Service [retrieves the list of matches from Cosmos DB][simulation-service-retrieves-matches] using the tournament ID, and the number of round received. Next, it gets the list of soccer players from PlayFab and simulates [every match][simulate-every-match] using the match and players grouped by the local and visitor team.

Once the data at match level is generated, the [SimulateMatchFutbolPlayerPerformance][simulate-match-futbol-player-performance] method is triggered to generate the soccer player performance is called. To do so, it sends the soccer player data, and a [MatchSimulationProcessState][match-simulation-process-state], that is the current state of the simulation, used to hold the coherence with the Match and between the all the soccer players simulations.

The responsibility of [SimulationService][simulation-service] finishes with the returning of the list of [MatchSimulationResult][match-simulation-result], then the AdminController saves into Cosmos DB the simulated data and returns a response that holds a [SimulationResponse][simulation-response]. 

<!-- Index -->

[summary]: #Summary
[implementation]: #Implementation
<!-- Code references -->

[ctrl-simulate-round]: ./FantasySoccer/FantasySoccer/Controllers/AdminController.cs#L65
[simulation-service]: ./FantasySoccer/FantasySoccer.Core/Services/Simulation/SimulationService.cs
[simulate-tournament-round]: ./FantasySoccer/FantasySoccer.Core/Services/Simulation/SimulationService.cs#L20
[match-simulation-result]: ./FantasySoccer/FantasySoccer.Core/Services/Simulation/MatchSimulationResult.cs
[simulation-service-retrieves-matches]: ./FantasySoccer/FantasySoccer.Core/Services/Simulation/SimulationService.cs#L22
[simulate-every-match]: ./FantasySoccer/FantasySoccer.Core/Services/Simulation/SimulationService.cs#L33
[simulate-match-futbol-player-performance]: ./FantasySoccer/FantasySoccer.Core/Services/Simulation/MatchSimulation.cs#L42
[match-simulation-process-state]: ./FantasySoccer/FantasySoccer.Core/Services/Simulation/MatchSimulationState.cs#L17
[match-simulation-result]: ./FantasySoccer/FantasySoccer.Core/Services/Simulation/MatchSimulationResult.cs
[simulation-response]: ./FantasySoccer/FantasySoccer/Models/Responses/SimulationResponse.cs

<!-- PlayFab references -->

[playfab-stores]: https://docs.microsoft.com/gaming/playfab/features/commerce/stores
