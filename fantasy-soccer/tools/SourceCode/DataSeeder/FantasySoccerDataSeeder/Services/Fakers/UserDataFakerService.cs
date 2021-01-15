using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.CosmosDB;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccerDataSeeder.Services.Fakers
{
   public class UserDataFakerService
    {
        private readonly int _userteamsAmount;
        private Faker<UserTeam> _userTeamFaker;
        private Faker<UserTransaction> _userTransactionFaker;
        private readonly List<FutbolPlayer> _players;

        public UserDataFakerService(IUserDataConfig userDataConfig, List<FutbolPlayer> futbolPlayers)
        {
            _userteamsAmount = userDataConfig.UserTeamsAmount;
            _players = futbolPlayers;
            SetFakers(_players);
        }

        private void SetFakers(List<FutbolPlayer> futbolPlayers)
        {
            var userTeamIDs = 0;
            var userPlayFabIDs = 0;

            _userTeamFaker=new Faker<UserTeam>().RuleFor(ut => ut.ID, f => (++userTeamIDs).ToString())
                .RuleFor(ut => ut.UserPlayFabID, f => (++userPlayFabIDs).ToString())
                .RuleFor(ut => ut.MatchdayScores, f => new Dictionary<int, int> { })
                .RuleFor(ut => ut.Players, f => f.PickRandom(futbolPlayers, 14).ToList())
                .RuleFor(ut => ut.TournamentScore, f => f.Random.Int(10, 500));

            _userTransactionFaker=new Faker<UserTransaction>().RuleFor(ut => ut.ID, f => (Guid.NewGuid()).ToString())
                .RuleFor(ut => ut.UserPlayFabID, f => (++userPlayFabIDs).ToString())
                .RuleFor(fp => fp.InvolvedFutbolPlayerID, f => f.PickRandom(futbolPlayers).ID)
                .RuleFor(fp => fp.OperationDate, f => f.Date.Past(0))
                .RuleFor(fp => fp.OperationType, f => f.PickRandom<OperationTypes>());
        }

        public List<UserTeam> GenerateUserTeams()
        {
            var userTeams = _userTeamFaker.Generate(_userteamsAmount);
            return userTeams;
        }

        public List<UserTransaction> GenerateUserTransactions()
        {
            var userTransactions = _userTransactionFaker.Generate(_userteamsAmount);
            return userTransactions;
        }
    }

}
