using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Core.Services
{
    public interface IFantasySoccerService
    {
        Task<Tournament> GetTournament(string id);

        Task<Tournament> GetCurrentTournament();

        Task<List<Match>> GetMatches(string tournamentId, int? round);

        Task<int> GetNumberOfRoundsForATournament(string tournamentId);

        Task<PaginatedItem<FutbolPlayer>> GetFutbolPlayersStoreAsync(int? size = null, int? skip = null, Func<FutbolPlayer, bool> filter = null);
        
        Task<List<FutbolTeam>> GetFutbolTeamsAsync();
        
        Task<UserTeam> GetUserTeamAsync();
        
        Task<UserPlayerStatistics> GetUserTeamStatisticAsync();

        Task<List<UserTransaction>> GetUserTransactionsAsync();

        Task UpdateUserTeamAsync(UserTeam userTeam);

        Task UpdateTournament(Tournament tournament);

        Task AddTournamentAsync(Tournament tournament);

        Task OverwriteFutbolTeamsAsync(List<FutbolTeam> futbolTeams);

        Task OverwriteFutbolPlayersAsync(List<FutbolPlayer> futbolPlayers);

        Task OverwriteMatchesAsync(List<Match> matches);

        Task AddMatchesAsync(List<Match> matches);
        
        Task UpdateMatchesAsync(List<Match> matches);

        Task CleanFutbolTeamsAsync();
        
        Task<string> SellFutbolPlayerAsync(string itemInstanceId);
        
        Task<string> BuyFutbolPlayerAsync(string itemId, int price);

        Task CalculateUserRoundScore(string currentPlayerId, string tournamentId, int round);

        Task<List<MatchFutbolPlayerPerformance>> AddMatchFutbolPlayersPerformancesAsync(List<MatchFutbolPlayerPerformance> performances);
    }
}
