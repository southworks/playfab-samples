# Fantasy Soccer - Web Application Architecture

![Web Application architecture][webapp-architecture]

The Fantasy Soccer Web Application has the following components:

1. [FantasySoccer Service][service-fantasy-soccer-section]. This service acts as a proxy, and decides whether to retrieve/store data from Cosmos DB or from a PlayFab Title.
1. [Authentication Service][authentication-service]. Manages the Authentication logic for players either logged with PlayFab accounts, or an Azure AD account.
1. [Score Calculation Service][score-calculation-section]. Allows the calculation of a tournament round.
1. [Simulation Service][simulation-service]. Has the logic to simulate the performance of the players, and the results of the matches in a round.

## Fantasy Soccer Service

The [Fantasy Soccer Service][service-fantasy-soccer] service acts as a proxy, and decides whether to retrieve and store data from Cosmos DB or from a PlayFab Title.

The entities stored in Cosmos DB are:

- Team
- Match
- Tournament
- Match Player Performance

The entities stored in the PlayFab title are:

- User Team
  - It's on the user's inventory.
  - Each inventory item has a custom data attribute called `IsStarter` to know if the player is part of the starter team or not.
- Soccer Player
  - Each soccer player is an item of a catalog.
  - In the custom data of each item it is stored all the data of the player, like its position, it birth date, etc.

PlayFab service is integrated with Cache that keeps in memory all the Soccer Players' data.

To do so, there's a PlayFab Title data called `StoreVersion` that gets updated when the store is updated. When there's a version mismatch, the service will retrieve the latest version from the PlayFab Store, and updates it in the Cache.

## Score Calculation service

The [Score Calculation Service][score-calculation-af] is the responsible of calculating the score of an specific Tournament Round.

![Score Calculation Service][score-calculation]

### 1. Schedule task triggers a function for the 'All Players' segment

- The Task is configured to trigger an specific Azure Function on the 'All Players' segment.
- Game administrator updates the arguments of the task with the desired TournamentId and Round.
- Admin manually triggers the task.

### 2. PlayFab triggers the Azure Function for each player on the segment

- This will add the player info in the context of the request.

### 3. The Azure Function calculates the score

- The function retrieves the User Team from the user's inventory.
- Then it retrieves the performance of the players in the user's team.
- With that data, it calculates the score of the team.

### 4. The User Score gets updated

- The round score is stored in a statistic with the following key: `r-{tournamentId}-{round}`.
- The round score is also accumulated on the `TournamentScore` statistic.

<!-- Images -->
[webapp-architecture]: ./documentation-assets/webapp-architecture.png
[score-calculation]: ./documentation-assets/score-calculation.png

<!-- Code references -->
[score-calculation-section]: #score-calculation-service
[score-calculation-af]: ./FantasySoccer/FantasySoccer.Functions/UserTeamRoundScoreCalculation.cs
[simulation-service]: ./FantasySoccer/FantasySoccer.Core/Services/Simulation/SimulationService.cs
[authentication-service]: ./FantasySoccer/FantasySoccer/Services/AuthenticationService.cs
[service-fantasy-soccer]: ./FantasySoccer/FantasySoccer.Core/Services/FantasySoccerService.cs
[service-fantasy-soccer-section]: #fantasy-soccer-service
