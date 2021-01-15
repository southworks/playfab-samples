using System.Collections.Generic;
using System.Linq;
using Bogus;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.PlayFab;
using FantasySoccerDataSeeder.Models.Interfaces;

namespace FantasySoccerDataSeeder.Services.Fakers
{
    public class FutbolPlayerFakerService
    {
        private readonly int _futbolPlayerAmount;
        private Faker<FutbolPlayer> _futbolPlayersFaker;
        private Faker<SoccerPlayerGeneralStats> _soccerPlayerGeneralStatsFaker;

        public FutbolPlayerFakerService(ITournamentConfig tournamentConfig)
        {
            _futbolPlayerAmount = tournamentConfig.FutbolTeamsAmount * (tournamentConfig.TeamStartersAmount + tournamentConfig.TeamSubsAmount);
            SetFakers();
        }

        private void SetFakers()
        {
            var futbolPlayersIDs = 0;
            var futbolTeamsIDs = 0;
            var generalStatsIDs = 0;

            _futbolPlayersFaker = new Faker<FutbolPlayer>()
                .RuleFor(fp => fp.ID, f => (++futbolPlayersIDs).ToString())
                .RuleFor(fp => fp.Name, f => f.Name.FirstName(Bogus.DataSets.Name.Gender.Male))
                .RuleFor(fp => fp.LastName, f => f.Name.LastName(Bogus.DataSets.Name.Gender.Male))
                .RuleFor(fp => fp.Price, f => f.Random.UInt(0, 9999))
                .RuleFor(fp => fp.Birthdate, f => f.Date.Past(15))
                .RuleFor(fp => fp.FutbolTeamID, f => futbolTeamsIDs.ToString())
                .RuleFor(fp => fp.Position, f => f.PickRandom<Position>())
                .RuleFor(fp => fp.GeneralStats, f => GenerateSoccerPlayerGeneralStats(1));

            _soccerPlayerGeneralStatsFaker = new Faker<SoccerPlayerGeneralStats>()
                .RuleFor(gs => gs.ID, f => (++generalStatsIDs).ToString())
                .RuleFor(gs => gs.FutbolPlayerID, f => (futbolPlayersIDs).ToString())
                .RuleFor(gs => gs.Goals, f => f.Random.Int(0, 10))
                .RuleFor(gs => gs.Assists, f => f.Random.Int(5, 20))
                .RuleFor(gs => gs.Wins, f => f.Random.Int(5, 15))
                .RuleFor(gs => gs.Draws, f => f.Random.Int(5, 15))
                .RuleFor(gs => gs.Losses, f => f.Random.Int(5, 15));
        }

        public List<FutbolPlayer> GenerateFutbolPlayers()
        {
            var futbolPlayers = _futbolPlayersFaker.Generate(_futbolPlayerAmount);
            return futbolPlayers;
        }

        public SoccerPlayerGeneralStats GenerateSoccerPlayerGeneralStats(int quantity)
        {
            var soccerPlayerGeneralStats = _soccerPlayerGeneralStatsFaker.Generate(quantity);
            return soccerPlayerGeneralStats.FirstOrDefault();
        }
    }

}
