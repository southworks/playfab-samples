using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;
using FantasySoccerDataSeeder.Models;
using FantasySoccerDataSeeder.Models.Interfaces;

namespace FantasySoccerDataSeeder.Services.Fakers
{
    public class TournamentFakerService
    {
        private readonly int _totalFutbolTeams;
        private readonly int _totalTournaments;
        private readonly int _startersAmount;
        private readonly int _subsAmount;
        private readonly bool _isHomeAway;
        private readonly List<FutbolPlayer> _players;
        private readonly Func<int, bool, int> _matchesAmount = (int futbolTeamsAmount, bool isHomeAway) => futbolTeamsAmount*(futbolTeamsAmount-1)/2*(isHomeAway ? 2 : 1);

        private Faker<FutbolTeam> _futbolTeamsFaker;
        private Faker<Tournament> _tournamentsFaker;
        private Faker<Match> _matchesFaker;

        public List<Match> Matches { get; set; }
        public Tournament Tournament { get; set; }

        public TournamentFakerService(ITournamentConfig tournamentConfig, List<FutbolPlayer> players)
        {
            _totalFutbolTeams=tournamentConfig.FutbolTeamsAmount;
            _totalTournaments=tournamentConfig.TournamentsAmount;
            _startersAmount=tournamentConfig.TeamStartersAmount;
            _subsAmount=tournamentConfig.TeamSubsAmount;
            _isHomeAway=tournamentConfig.IsHomeAway;
            _players=players;

            SetFakers();
        }

        public void GenerateTournaments()
        {
            var teams = GenerateFutbolTeams();

            _tournamentsFaker.RuleFor(fp => fp.FutbolTeams, f => teams);
            var tournaments = _tournamentsFaker.Generate(_totalTournaments);

            tournaments.ForEach(t => t.CurrentRound=1);

            Matches = GenerateMatches(tournaments.First());
            Tournament = tournaments.First();
        }

        public List<FutbolTeam> GenerateFutbolTeams()
        {
            var teams = _futbolTeamsFaker.Generate(_totalFutbolTeams);

            var totalPlayers = _players.Count;
            var iPlayers = 0;
            var playersForTeam = _startersAmount + _subsAmount;

            foreach (var team in teams)
            {
                team.FutbolPlayersId =_players.Skip(iPlayers).Take(playersForTeam).Select(p => int.Parse(p.ID)).ToArray();
                iPlayers+=playersForTeam;
            }

            return teams;
        }


        private List<Match> GenerateMatches(Tournament tournament)
        {
            var teams = tournament.FutbolTeams;

            var matches = _matchesFaker.Generate(_matchesAmount(tournament.FutbolTeams.Count, _isHomeAway));
            

            var matchesIDs=CalculateRounds(teams);

            // populate the matches Id's
            for (var i = 0; i<matches.Count; i++)
            {
                matches[i].LocalFutbolTeamID=matchesIDs[i].HomeTeamId;
                matches[i].VisitorFutbolTeamID=matchesIDs[i].AwayTeamId;
                matches[i].Round=matchesIDs[i].Round;
            }

            return matches;
        }
        private List<RoundInfo> CalculateRounds(List<FutbolTeam> teams)
        {
            var matchesIDs = new List<RoundInfo>();
            var totalRounds = teams.Count-1;
            var halfTeamsAmount = teams.Count/2;
            var tempTeams = teams.ToList();

            for (var round = 0; round<totalRounds; round++)
            {
                for (var i = 0; i<halfTeamsAmount; i++)
                {
                    matchesIDs.Add(new RoundInfo
                    {
                        HomeTeamId=tempTeams[i].ID,
                        AwayTeamId=tempTeams[teams.Count-1-i].ID,
                        Round=round+1
                    });
                    if (_isHomeAway)
                    {
                        matchesIDs.Add(new RoundInfo
                        {
                            HomeTeamId=tempTeams[teams.Count-1-i].ID,
                            AwayTeamId=tempTeams[i].ID,
                            Round=round+1+(totalRounds/2)
                        });
                    }
                }
                SwapTeams(tempTeams);
            }
            return matchesIDs;

            void SwapTeams(List<FutbolTeam> teams)
            {
                var item = teams[^1];
                teams.RemoveAt(teams.Count-1);
                teams.Insert(1, item);
            }
        }
        private void SetFakers()
        {
            var futbolTeamsIDs = 0;
            var tournamentIDs = 0;

            // Futbol Team Faker
            _futbolTeamsFaker=new Faker<FutbolTeam>()
                .RuleFor(ft => ft.ID, f => (++futbolTeamsIDs).ToString())
                .RuleFor(ft => ft.Name, f => f.Lorem.Word())
                .RuleFor(ft => ft.FutbolPlayersId, f => new int[0]);

            // Tournament Faker
            _tournamentsFaker=new Faker<Tournament>()
                .RuleFor(fp => fp.ID, f => (++tournamentIDs).ToString())
                .RuleFor(fp => fp.Name, f => f.Lorem.Word())
                .RuleFor(fp => fp.StartDate, f => DateTime.Now)
                .RuleFor(fp => fp.EndDate, f => DateTime.Now.AddMonths(2));

            // Match Faker
            _matchesFaker=new Faker<Match>()
                .RuleFor(m => m.ID, f => (Guid.NewGuid()).ToString())
                .RuleFor(m => m.TournamentId, f => "1")
                .RuleFor(m => m.LocalGoals, f => f.Random.Int(0, 5))
                .RuleFor(m => m.VisitorGoals, f => f.Random.Int(0, 5));
        }
    }
}
